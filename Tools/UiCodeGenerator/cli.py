"""CLI interface for UiCodeGenerator."""

from __future__ import annotations

import argparse
import json
import os
import sys
from pathlib import Path
from typing import Dict, List, Optional

from . import VERSION
from .prefab_parser import UnityPrefabParser, get_all_gameobjects
from .component_resolver import ComponentResolver
from .code_generator import generate_all, write_generated_files


def _default_config_dir() -> Path:
    return Path(__file__).parent / "config"


def _find_prefab_base(prefab_path: str) -> str:
    """Find the project root-relative prefab path by looking for 'Assets/'."""
    abs_path = os.path.abspath(prefab_path)
    pos = abs_path.find("Assets" + os.sep)
    if pos >= 0:
        return abs_path[pos:].replace("\\", "/")
    return prefab_path


def _derive_form_name(prefab_path: str) -> str:
    """Derive a form name from a prefab filename."""
    basename = os.path.splitext(os.path.basename(prefab_path))[0]
    return basename


def cmd_scan(args) -> int:
    """Scan a prefab and list all detected bindings."""
    prefab_path = args.prefab
    config_path = args.config or str(_default_config_dir() / "bindings.json")
    guids_path = args.guids or str(_default_config_dir() / "unity_guids.json")

    parser = UnityPrefabParser(guids_path=guids_path)
    resolver = ComponentResolver(config_path=config_path)

    result = parser.parse(prefab_path)
    if result.errors:
        for err in result.errors:
            print(f"ERROR: {err}", file=sys.stderr)
        return 1

    bindings = resolver.resolve_all(result)

    form_name = args.form_name or _derive_form_name(prefab_path)
    form_type = resolver.get_form_type(form_name)
    rel_path = _find_prefab_base(prefab_path)

    if args.format == "json":
        output = {
            "prefab": rel_path,
            "formName": form_name,
            "formType": form_type,
            "bindings": [
                {
                    "gameObjectName": b.game_object_name,
                    "componentType": b.component_type,
                    "fullTypeName": b.full_type_name,
                    "propertyName": b.property_name,
                    "childPath": b.child_path,
                    "usingNamespace": b.using_namespace,
                }
                for b in bindings
            ],
            "warnings": result.warnings,
            "errors": result.errors,
        }
        print(json.dumps(output, indent=2, ensure_ascii=False))
    else:
        print(f"PREFAB: {rel_path}")
        print(f"FORM:   {form_name}")
        print(f"TYPE:   {form_type}")
        print()
        print("BINDINGS:")
        if bindings:
            # Align columns
            max_prefix = max(
                max(len(b.component_type.lower()) for b in bindings), 4
            )
            for b in bindings:
                prefix_tag = f"[{b.component_type.lower()}]".ljust(max_prefix + 3)
                print(
                    f"  {prefix_tag} {b.property_name:<25} "
                    f"(path: {b.child_path})"
                )
        else:
            print("  (none)")

        if result.warnings:
            print()
            print("WARNINGS:")
            for w in result.warnings:
                print(f"  - {w}")

        print()
        print(
            f"SUMMARY: {len(bindings)} binding(s), "
            f"{len(result.errors)} error(s), "
            f"{len(result.warnings)} warning(s)"
        )

    return 0


def cmd_generate(args) -> int:
    """Generate View and Logic C# code from a prefab."""
    prefab_path = args.prefab
    config_path = args.config or str(_default_config_dir() / "bindings.json")
    guids_path = args.guids or str(_default_config_dir() / "unity_guids.json")

    parser = UnityPrefabParser(guids_path=guids_path)
    resolver = ComponentResolver(config_path=config_path)

    result = parser.parse(prefab_path)
    if result.errors:
        for err in result.errors:
            print(f"ERROR: {err}", file=sys.stderr)
        return 1

    bindings = resolver.resolve_all(result)

    form_name = args.form_name or _derive_form_name(prefab_path)
    form_type = args.form_type or resolver.get_form_type(form_name)
    rel_path = _find_prefab_base(prefab_path)
    namespace = args.namespace or resolver.settings.get("defaultNamespace", "GenBall.UI")

    view_base = resolver.settings.get("viewBaseClass", "Yueyn.UI.UIBusinessFormBase")
    logic_base = resolver.settings.get("logicBaseClass", "Yueyn.UI.BusinessFormLogic")

    # Fix child paths: remove root name prefix for transform.Find()
    if result.root:
        root_prefix = result.root.name + "/"
        for b in bindings:
            if b.child_path.startswith(root_prefix):
                b.child_path = b.child_path[len(root_prefix):]

    # Generate code
    codes = generate_all(
        form_name=form_name,
        prefab_path=rel_path,
        bindings=bindings,
        form_type=form_type,
        namespace=namespace,
        view_base_class=view_base,
        logic_base_class=logic_base,
    )

    # Determine output directory
    if args.output_dir:
        output_dir = args.output_dir
    else:
        default_base = resolver.settings.get("outputBasePath", "Assets/Scripts/GenBall/UI")
        output_dir = os.path.join(default_base, form_name)

    # Write files
    view_path, logic_path, skipped = write_generated_files(
        output_dir=output_dir,
        form_name=form_name,
        view_code=codes["view"],
        logic_code=codes["logic"],
        force=args.force,
    )

    if skipped:
        for p in skipped:
            print(f"SKIPPED (exists, use --force): {p}")

    if not args.no_view and codes["view"]:
        print(f"GENERATED: {view_path}")
    if not args.no_logic and codes["logic"]:
        print(f"GENERATED: {logic_path}")

    if result.warnings:
        print()
        for w in result.warnings:
            print(f"WARNING: {w}")

    print(f"\nDone. {len(bindings)} binding(s) generated for {form_name}.")
    return 0


def cmd_validate(args) -> int:
    """Validate a prefab against the binding config."""
    prefab_path = args.prefab
    config_path = args.config or str(_default_config_dir() / "bindings.json")
    guids_path = args.guids or str(_default_config_dir() / "unity_guids.json")

    parser = UnityPrefabParser(guids_path=guids_path)
    resolver = ComponentResolver(config_path=config_path)

    result = parser.parse(prefab_path)

    exit_code = 0
    if result.errors:
        for err in result.errors:
            print(f"ERROR: {err}")
        exit_code = 1

    bindings = resolver.resolve_all(result)

    if result.warnings:
        for w in result.warnings:
            print(f"WARNING: {w}")

    if exit_code == 0 and not result.warnings:
        print(f"OK: {len(bindings)} binding(s) found. No errors or warnings.")

    return exit_code


def main(argv: Optional[List[str]] = None) -> int:
    """Main entry point."""
    parser = argparse.ArgumentParser(
        prog="ui-codegen",
        description="Generate Unity UI View and Logic C# code from .prefab files.",
    )
    parser.add_argument(
        "--version", action="version", version=f"UiCodeGenerator v{VERSION}"
    )

    subparsers = parser.add_subparsers(dest="command", help="Available commands")

    # ── scan ──
    scan = subparsers.add_parser("scan", help="Scan a prefab and list bindings")
    scan.add_argument("--prefab", required=True, help="Path to .prefab file")
    scan.add_argument("--config", help="Path to bindings.json")
    scan.add_argument("--guids", help="Path to unity_guids.json")
    scan.add_argument("--form-name", help="Form name override (default: derived from filename)")
    scan.add_argument("--format", choices=["text", "json"], default="text",
                       help="Output format (default: text)")

    # ── generate ──
    gen = subparsers.add_parser("generate", help="Generate View and Logic code")
    gen.add_argument("--prefab", required=True, help="Path to .prefab file")
    gen.add_argument("--config", help="Path to bindings.json")
    gen.add_argument("--guids", help="Path to unity_guids.json")
    gen.add_argument("--form-name", help="Form name (default: derived from filename)")
    gen.add_argument("--form-type", choices=["Persistent", "Popup", "Transition"],
                      help="UIFormType (default: from config or Popup)")
    gen.add_argument("--namespace", help="C# namespace (default: GenBall.UI)")
    gen.add_argument("--output-dir", help="Output directory (default: derived)")
    gen.add_argument("--no-view", action="store_true", help="Skip View generation")
    gen.add_argument("--no-logic", action="store_true", help="Skip Logic generation")
    gen.add_argument("--force", action="store_true", help="Overwrite existing files")

    # ── validate ──
    val = subparsers.add_parser("validate", help="Validate a prefab against binding config")
    val.add_argument("--prefab", required=True, help="Path to .prefab file")
    val.add_argument("--config", help="Path to bindings.json")
    val.add_argument("--guids", help="Path to unity_guids.json")

    args = parser.parse_args(argv)

    if not args.command:
        parser.print_help()
        return 1

    commands = {
        "scan": cmd_scan,
        "generate": cmd_generate,
        "validate": cmd_validate,
    }

    return commands[args.command](args)


if __name__ == "__main__":
    sys.exit(main())
