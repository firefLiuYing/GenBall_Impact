---
name: tests-run
description: "Execute Unity tests using a sub-agent. The sub-agent runs the bash script, waits for results, parses failures, and returns a concise analysis. Saves main-session context."
---

# Tests / Run

**Always delegate to a sub-agent.** Never run `bash run_editmode_tests.sh` directly in the main session — it wastes context on raw JSON output.

## Dispatch

Launch a **general-purpose** sub-agent to run tests and report back:

```
Agent(subagent_type="general-purpose", description="Run Unity tests", prompt="
Run EditMode tests in the Unity project at 'D:\Apps\Unity\Unity Project\GenBall_Impact\'.

Execute: bash run_editmode_tests.sh [--mode <mode>] [--assembly <name>] [--namespace <ns>] [--class <name>]

Wait for completion. Then:
1. Report passed/failed counts
2. If any failures: list each failing test name and the root error message (first line only)
3. If all pass: just say 'All N tests passed (Xs)'
4. Do NOT dump full stack traces or raw JSON
5. If timeout (no Unity): say 'Unity Editor not responding — please compile (Ctrl+R) and retry'

Prerequisite: Unity Editor must be open.
")
```

## Filters (pass as args)

- `--mode` (default `EditMode`) — `EditMode` or `PlayMode`
- `--assembly` — specific test assembly
- `--namespace` — test namespace filter
- `--class` — test class name filter

## Troubleshooting

- **Timeout**: Unity Editor not open or not finished compiling. User should Ctrl+R then retry.
- **Stale results**: Delete `Temp/.run_tests.trigger` and retry.
