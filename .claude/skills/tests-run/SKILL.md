---
name: tests-run
description: "Run Unity EditMode tests."
---

## Running tests

### Main session (MCP)

Call `mcp__unity__unity_test` if available, or use the file-IPC fallback:

```bash
bash devtool.sh test [--class <name>] [--method <name>] [--namespace <ns>] [--assembly <name>]
```

### Sub-agents (file IPC)

Use `bash devtool.sh test [--class X]`. Do NOT use MCP tools — they are
unreliable in sub-agents. Use `bash devtool.sh verify` to compile + test in
one command.

### Filtering tips

- `--class SimpleFsmTests` — filter by test fixture name (substring match)
- `--method Namespace.Class.Method` — exact test method
- `--namespace GenBall` — all tests in namespace
- No filter = all tests

### Output

Results are reported with `[PASS]`/`[FAIL]` prefix and summary line:
`[PASS] All tests passed: N passed, 0 failed, 0 skipped (Xs)`

Failed tests are listed individually with failure messages.

### Important

- Test files go in `Assets/Scripts/GenBall/**/Editor/` (not `Yueyn/`).
- New `.cs` files are auto-imported on next compilation — no manual step needed.
- Do NOT modify production code just to make it testable.
- For zero-LLM: use `! bash devtool.sh test` in the prompt.
