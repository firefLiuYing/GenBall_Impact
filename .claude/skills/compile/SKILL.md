---
name: compile
description: "Trigger Unity compilation and report errors."
---

## Trigger compilation

Call `mcp__unity__unity_compile` and report results.

- Returns `compilation_complete` with `errorCount`, `warningCount`, `errors`, `warnings` arrays.
- May return `no_changes` if no script changes were detected (not an error).
- Each error/warning has `file`, `line`, `column`, `message` fields.
- If errors > 0, analyze and propose fixes.

## Notes

- The MCP tool uses file-based state polling to survive Unity domain reloads during compilation.
- Do NOT use `compile_cli.py` — it conflicts with the MCP server on TCP port 9876 (single-client).
- For zero-LLM: use `! mcp__unity__unity_compile` in the prompt.
