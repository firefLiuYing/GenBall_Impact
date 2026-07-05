---
name: compile
description: "Trigger Unity compilation and report errors."
---

## Trigger compilation

Call `mcp__unity__unity_compile` and report results as follows.

### Result types

- **`compilation_complete`**: compilation ran. Contains `errorCount`, `warningCount`, `errors[]`, `warnings[]`.
- **`no_changes`**: no script changes detected — no domain reload occurred. Not an error.
- **`compilation_timeout`**: compilation did not finish within 120s. Check Unity Editor state.

Each error/warning has `file`, `line`, `column`, `message` fields.

### Output format

1. **Summary line**: `✅ 0 errors, N warnings` or `❌ N errors, M warnings`
2. **Errors grouped by file** (only if errors > 0):
   ```
   ❌ N errors, M warnings
   **path/to/File.cs**
   - (L{line}, C{col}): {message}
   - (L{line}, C{col}): {message}
   **path/to/OtherFile.cs**
   - ...
   ```
3. **Warnings section** (only if warnings > 0): same grouping as errors.
4. **If 0 errors and 0 warnings**: just confirm compilation passed.

### Full rebuild

If you suspect Unity is reporting only new errors (not pre-existing ones from unchanged assemblies), pass `fullRebuild: true`:

```json
{"fullRebuild": true}
```

This forces recompilation of ALL assemblies (slower, causes domain reload). Default (`false` or omitted) uses fast incremental compilation.

### Fix suggestions

When errors are found:
- Analyze each error message and suggest concrete fixes
- Propose edits using the Edit tool
- After fixing, run compile again to verify

## Notes

- Do NOT use `compile_cli.py` — it conflicts with the MCP server on TCP port 9876 (single-client).
- For zero-LLM: use `! mcp__unity__unity_compile` in the prompt.
- The MCP tool uses file-based state polling (`Temp/UnityMcpCompileState.json`) to survive Unity domain reloads.
- Incremental compilation (default) avoids unnecessary domain reloads when no scripts changed.
