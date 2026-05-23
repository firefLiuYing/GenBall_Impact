"""Component resolver — matches GameObject names against binding prefix rules."""

from __future__ import annotations

import json
import re
from pathlib import Path
from typing import Dict, List, Optional, Tuple

from .prefab_parser import (
    BindingEntry,
    ComponentInfo,
    GameObjectInfo,
    ParseResult,
    get_all_gameobjects,
    get_component_of_type,
)


class BindingConfig:
    """Represents a single prefix → component mapping."""

    def __init__(self, mapping: dict):
        self.prefix: str = mapping["prefix"]
        self.component_type: str = mapping["componentType"]
        self.full_name: str = mapping["fullName"]
        self.using_namespace: str = mapping.get("usingNamespace", "")
        self.category: str = mapping.get("category", "")
        self.priority: int = mapping.get("priority", 0)

    def __repr__(self) -> str:
        return f"BindingConfig(prefix='{self.prefix}', type='{self.component_type}')"


class ComponentResolver:
    """Matches GameObject names to UI binding prefixes and resolves component types."""

    def __init__(self, config_path: Optional[str] = None):
        """
        Args:
            config_path: Path to bindings.json. Uses default if None.
        """
        if config_path is None:
            config_path = Path(__file__).parent / "config" / "bindings.json"

        with open(config_path, "r", encoding="utf-8") as f:
            raw = json.load(f)

        self._configs: List[BindingConfig] = []
        for mapping in raw["prefixMappings"]:
            self._configs.append(BindingConfig(mapping))

        # Sort by prefix length (longest first) for greedy matching
        self._configs.sort(key=lambda c: len(c.prefix), reverse=True)

        self.settings = raw.get("generationSettings", {})
        self.overrides = raw.get("customOverrides", {})

    @property
    def prefix_mappings(self) -> List[BindingConfig]:
        return self._configs

    def resolve_gameobject(self, go: GameObjectInfo) -> Optional[BindingConfig]:
        """Find the matching binding config for a GameObject by name.

        Uses longest-prefix-match: "RawImgBackground" matches "RawImg" before "Img".
        """
        for config in self._configs:
            if go.name.startswith(config.prefix):
                return config
        return None

    def resolve_all(
        self, parse_result: ParseResult
    ) -> List[BindingEntry]:
        """Resolve all binding entries from a parsed prefab.

        Args:
            parse_result: Result from UnityPrefabParser.parse().

        Returns:
            List of BindingEntry objects for matched GameObjects.
        """
        if parse_result.root is None:
            return []

        bindings: List[BindingEntry] = []
        all_gos = get_all_gameobjects(parse_result.root)
        matched_names: Dict[str, int] = {}  # property_name → count (for dedup)

        for go in all_gos:
            # Skip root GameObject (typically the form itself)
            if go.file_id == parse_result.root.file_id:
                continue

            config = self.resolve_gameobject(go)
            if config is None:
                continue

            # Verify the expected component exists on the GameObject
            actual_comp = get_component_of_type(go, config.full_name)
            if actual_comp is None:
                # Check if this is a builtin-tag component (RectTransform, CanvasGroup)
                # that might be matched by class_id instead of guid
                for comp in go.components:
                    if comp.type_name == config.full_name:
                        actual_comp = comp
                        break

            if actual_comp is None:
                parse_result.warnings.append(
                    f"'{go.name}' matched prefix '{config.prefix}' but has no "
                    f"{config.component_type} component (expected {config.full_name})"
                )
                continue

            # Derive property name from GameObject name
            property_name = self._derive_property_name(go.name, config)

            # Handle duplicates
            if property_name in matched_names:
                matched_names[property_name] += 1
                property_name = f"{property_name}_{matched_names[property_name]}"
            else:
                matched_names[property_name] = 1

            entry = BindingEntry(
                game_object_name=go.name,
                component_type=config.component_type,
                full_type_name=config.full_name,
                property_name=property_name,
                child_path=go.child_path,
                component_file_id=actual_comp.file_id,
                using_namespace=config.using_namespace,
            )
            bindings.append(entry)

        # Check for unmatched GameObjects that have bindable components
        for go in all_gos:
            if go.file_id == parse_result.root.file_id:
                continue
            if self.resolve_gameobject(go) is not None:
                continue

            # Check if this GameObject has UI components but no prefix
            ui_comps = []
            for comp in go.components:
                for config in self._configs:
                    if comp.type_name == config.full_name:
                        ui_comps.append(config.component_type)
                        break
            if ui_comps:
                # Only warn about non-"special" GameObjects
                # (skip auto-generated names like "Text (Legacy)", "Image", etc.)
                if not any(
                    go.name.startswith(prefix)
                    for prefix in ("Text", "Image", "Button", "RawImage", "GameObject")
                ):
                    parse_result.warnings.append(
                        f"'{go.name}' has UI components ({', '.join(ui_comps)}) "
                        f"but no binding prefix"
                    )

        parse_result.bindings = bindings
        return bindings

    def get_form_type(self, form_name: str, default: str = "Popup") -> str:
        """Get the UIFormType for a given form, checking custom overrides."""
        override = self.overrides.get(form_name, {})
        return override.get("formType", default)

    def _derive_property_name(self, game_object_name: str, config: BindingConfig) -> str:
        """Derive a C# property name from a GameObject name.

        If the name exactly matches the prefix, the prefix itself becomes the property name.
        Otherwise, takes everything after the prefix.

        Examples:
            "BtnStart" → "BtnStart"
            "RawImgPreview" → "RawImgPreview"
            "Txt" → "Txt"
        """
        # Sanitize: replace spaces/special chars with underscores
        sanitized = re.sub(r"[^a-zA-Z0-9_]", "_", game_object_name)

        # Ensure valid C# identifier (can't start with digit)
        if sanitized and sanitized[0].isdigit():
            sanitized = "_" + sanitized

        return sanitized

