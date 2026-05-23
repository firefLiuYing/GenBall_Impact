"""Unity .prefab YAML parser.

Parses Unity .prefab files into a GameObject tree with resolved component types.
Handles Unity's YAML 1.1 format with custom tags (!u!1, !u!224, !u!114, etc.).
"""

from __future__ import annotations

import json
import os
import re
from dataclasses import dataclass, field
from pathlib import Path
from typing import Any, Dict, List, Optional, Tuple

import yaml


# ─── YAML document regex ───────────────────────────────────────────────
# Matches "--- !u!{classID} &{fileID}" header lines
_DOC_HEADER_RE = re.compile(r"^--- !u!(\d+) &(\d+)")


@dataclass
class ComponentInfo:
    """Represents a single component on a GameObject."""
    file_id: str
    class_id: int  # Unity class ID (1=GO, 224=RectTransform, 114=MonoBehaviour, etc.)
    type_name: str = ""  # e.g. "UnityEngine.UI.Text", "UnityEngine.RectTransform"
    is_mono_behaviour: bool = False
    script_guid: str = ""  # for MonoBehaviour: the script GUID


@dataclass
class GameObjectInfo:
    """Represents a single GameObject parsed from the prefab."""
    file_id: str
    name: str
    is_active: bool = True
    components: List[ComponentInfo] = field(default_factory=list)
    children: List[GameObjectInfo] = field(default_factory=list)
    parent_file_id: str = ""
    child_path: str = ""  # relative path from root


@dataclass
class BindingEntry:
    """A single detected UI binding."""
    game_object_name: str
    component_type: str       # e.g. "Button", "Text"
    full_type_name: str       # e.g. "UnityEngine.UI.Button"
    property_name: str        # C# property name
    child_path: str           # relative path from root
    component_file_id: str    # fileID of the bound component
    using_namespace: str = "" # e.g. "UnityEngine.UI"


@dataclass
class ParseResult:
    """Result of parsing a prefab file."""
    prefab_path: str
    root: Optional[GameObjectInfo] = None
    all_objects: Dict[str, GameObjectInfo] = field(default_factory=dict)
    bindings: List[BindingEntry] = field(default_factory=list)
    warnings: List[str] = field(default_factory=list)
    errors: List[str] = field(default_factory=list)


class UnityPrefabParser:
    """Parses Unity .prefab YAML files and extracts GameObject/component structure."""

    # Built-in Unity class mapping (class ID → type name)
    BUILTIN_CLASSES: Dict[int, str] = {
        1: "GameObject",
        4: "Transform",
        224: "RectTransform",
        223: "Canvas",
        222: "CanvasRenderer",
        225: "CanvasGroup",
        114: "MonoBehaviour",
    }

    # Component types detectable from class ID alone (not MonoBehaviour)
    BUILTIN_COMPONENTS: Dict[int, str] = {
        224: "UnityEngine.RectTransform",
        223: "UnityEngine.Canvas",
        222: "UnityEngine.CanvasRenderer",
        225: "UnityEngine.CanvasGroup",
    }

    def __init__(self, guids_path: Optional[str] = None):
        """
        Args:
            guids_path: Path to unity_guids.json. If None, uses default config path.
        """
        if guids_path is None:
            guids_path = Path(__file__).parent / "config" / "unity_guids.json"

        with open(guids_path, "r", encoding="utf-8") as f:
            config = json.load(f)

        self.guid_to_type: Dict[str, str] = config.get("guid_mappings", {})
        self.builtin_tags: Dict[int, str] = {
            int(k): v for k, v in config.get("builtin_tags", {}).items()
        }

    def parse(self, prefab_path: str) -> ParseResult:
        """Parse a Unity .prefab file and return its structure.

        Args:
            prefab_path: Path to the .prefab file.

        Returns:
            ParseResult with root GameObject, all objects, and any warnings/errors.
        """
        result = ParseResult(prefab_path=os.path.abspath(prefab_path))

        if not os.path.isfile(prefab_path):
            result.errors.append(f"File not found: {prefab_path}")
            return result

        # Read raw YAML
        with open(prefab_path, "r", encoding="utf-8-sig") as f:
            content = f.read()

        # Parse documents
        documents = self._parse_documents(content)
        if not documents:
            result.errors.append("No documents found in prefab file")
            return result

        # Build fileID → (class_id, data) map
        raw_objects: Dict[str, Tuple[int, Dict]] = {}
        for doc in documents:
            class_id = doc["class_id"]
            file_id = doc["file_id"]
            raw_objects[file_id] = (class_id, doc["data"])

        # Pass 1: Create GameObjectInfo for all !u!1 documents
        go_objects: Dict[str, GameObjectInfo] = {}
        for file_id, (class_id, data) in raw_objects.items():
            if class_id == 1:
                go = GameObjectInfo(
                    file_id=file_id,
                    name=data.get("m_Name", "__unnamed__"),
                    is_active=data.get("m_IsActive", 1) == 1,
                )
                go_objects[file_id] = go

        # Pass 2: Resolve components for each GameObject
        for file_id, (class_id, data) in raw_objects.items():
            if class_id == 1:
                go = go_objects[file_id]
                component_refs = data.get("m_Component", [])
                for ref in component_refs:
                    comp_file_id = str(ref.get("component", {}).get("fileID", ""))
                    if comp_file_id and comp_file_id in raw_objects:
                        comp_info = self._resolve_component(comp_file_id, raw_objects)
                        if comp_info:
                            go.components.append(comp_info)

        # Pass 3: Build parent/child relationships
        for file_id, (class_id, data) in raw_objects.items():
            if class_id in (224, 4):  # RectTransform or Transform
                go_file_id = str(data.get("m_GameObject", {}).get("fileID", ""))
                if go_file_id in go_objects:
                    go = go_objects[go_file_id]

                    # Parent
                    father_ref = data.get("m_Father", {})
                    father_id = str(father_ref.get("fileID", "0"))
                    if father_id != "0":
                        # Find parent GameObject through parent RectTransform
                        if father_id in raw_objects:
                            _, parent_data = raw_objects[father_id]
                            parent_go_id = str(parent_data.get("m_GameObject", {}).get("fileID", ""))
                            go.parent_file_id = parent_go_id

                    # Children are resolved below

        # Pass 4: Link children (using RectTransform m_Children)
        for file_id, (class_id, data) in raw_objects.items():
            if class_id in (224, 4):
                go_file_id = str(data.get("m_GameObject", {}).get("fileID", ""))
                if go_file_id in go_objects:
                    go = go_objects[go_file_id]
                    children_refs = data.get("m_Children", [])
                    for child_ref in children_refs:
                        child_rt_id = str(child_ref.get("fileID", ""))
                        if child_rt_id and child_rt_id in raw_objects:
                            _, child_data = raw_objects[child_rt_id]
                            child_go_id = str(child_data.get("m_GameObject", {}).get("fileID", ""))
                            if child_go_id in go_objects:
                                child_go = go_objects[child_go_id]
                                if child_go not in go.children:
                                    go.children.append(child_go)

        # Find root (GameObject with no parent)
        root = None
        for go in go_objects.values():
            if not go.parent_file_id or go.parent_file_id == "0":
                root = go
                break

        if root is None and go_objects:
            root = list(go_objects.values())[0]
            result.warnings.append("Could not determine root; using first GameObject")

        # Build child paths from root
        if root:
            self._build_paths(root, "")

        result.root = root
        result.all_objects = go_objects

        # Check for nested prefabs
        for go in go_objects.values():
            # Nested prefab instances are detected in the GameObject data
            pass

        return result

    def _parse_documents(self, content: str) -> List[Dict[str, Any]]:
        """Split prefab YAML into individual documents by parsing Unity's format."""
        documents = []

        # Split on "--- !u!" document separators
        # Unity format: --- !u!CLASS_ID &FILE_ID\nTypeName:\nfields...
        parts = re.split(r"\n(?=--- !u!\d+ &\d+\n)", content)

        for part in parts:
            part = part.strip()
            if not part or part.startswith("%"):
                continue

            # Ensure the part starts with "--- !u!"
            if not part.startswith("--- !u!"):
                continue

            # Parse header line: "--- !u!CLASSID &FILEID"
            header_match = re.match(r"--- !u!(\d+) &(\d+)", part)
            if not header_match:
                continue

            class_id = int(header_match.group(1))
            file_id = header_match.group(2)

            # Everything after the header line is the body
            first_newline = part.find("\n")
            if first_newline == -1:
                continue

            body = part[first_newline + 1:]

            # Use PyYAML to parse the body
            try:
                data = yaml.safe_load(body)
                if data is None:
                    continue
                # PyYAML wraps: {'GameObject': {...}} or {'MonoBehaviour': {...}}
                # Unwrap the top-level type-name key
                if isinstance(data, dict):
                    # The key is the Unity type name (e.g., "GameObject", "RectTransform")
                    for key, value in data.items():
                        if isinstance(value, dict):
                            data = value
                            break
                if isinstance(data, dict):
                    documents.append({
                        "class_id": class_id,
                        "file_id": file_id,
                        "data": data,
                    })
            except yaml.YAMLError:
                # Skip documents that can't be parsed
                continue

        return documents

    def _resolve_component(
        self, file_id: str, raw_objects: Dict[str, Tuple[int, Dict]]
    ) -> Optional[ComponentInfo]:
        """Resolve a component from its fileID."""
        if file_id not in raw_objects:
            return None

        class_id, data = raw_objects[file_id]
        comp = ComponentInfo(file_id=file_id, class_id=class_id)

        if class_id == 114:  # MonoBehaviour
            comp.is_mono_behaviour = True
            script_ref = data.get("m_Script", {})
            guid = script_ref.get("guid", "")
            comp.script_guid = guid
            comp.type_name = self.guid_to_type.get(guid, f"Unknown({guid[:8]})")
        elif class_id in self.BUILTIN_COMPONENTS:
            comp.type_name = self.BUILTIN_COMPONENTS[class_id]
        else:
            comp.type_name = self.BUILTIN_CLASSES.get(class_id, f"UnknownClass({class_id})")

        return comp

    def _build_paths(self, go: GameObjectInfo, parent_path: str) -> None:
        """Recursively build child paths from root."""
        go.child_path = f"{parent_path}/{go.name}" if parent_path else go.name

        # Handle duplicate names at same level
        seen_names: Dict[str, int] = {}
        for child in go.children:
            self._build_paths(child, go.child_path)


def get_component_of_type(
    go: GameObjectInfo, type_name: str
) -> Optional[ComponentInfo]:
    """Find a component of the given type on a GameObject.

    Args:
        go: The GameObject to search.
        type_name: Full C# type name (e.g. "UnityEngine.UI.Button").

    Returns:
        The matching ComponentInfo or None.
    """
    for comp in go.components:
        if comp.type_name == type_name:
            return comp
    return None


def get_all_gameobjects(root: GameObjectInfo) -> List[GameObjectInfo]:
    """Flatten the GameObject tree into a list (depth-first)."""
    result = [root]
    for child in root.children:
        result.extend(get_all_gameobjects(child))
    return result


def find_gameobject_by_name(root: GameObjectInfo, name: str) -> Optional[GameObjectInfo]:
    """Find a GameObject by name (case-sensitive)."""
    if root.name == name:
        return root
    for child in root.children:
        found = find_gameobject_by_name(child, name)
        if found:
            return found
    return None
