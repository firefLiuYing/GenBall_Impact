# Testing Workflow

## Key Constraint

External `.cs` file writes (via Git Bash / MSYS2) do not trigger Windows filesystem watchers. Unity's `AssetDatabase.Refresh()` will NOT detect changes written this way.

- **Press Ctrl+R in Unity Editor** to trigger recompilation after external file edits.

## Running Tests

### Via CLI (/tests-run)
Run `bash run_editmode_tests.sh` — writes a trigger file to `Temp/`, Unity's `TestsAutoRunner` picks it up and runs tests via `TestRunnerApi`. See `/tests-run` skill for details.

### Via Unity Editor
Window > General > Test Runner > EditMode > Run All

## Test File Structure

```
SystemFolder/
├── SomeSystem.cs
└── Editor/
    └── SomeSystemTests.cs
```

## Conventions

- **Naming**: `{System}Tests.cs`, methods `{Method}_{Scenario}_{Expected}`
- **Mock**: Pure handwritten mock inner classes, no third-party libraries
- **Assertions**: NUnit native `Assert.That(...)`, Arrange/Act/Assert pattern
- **TearDown**: Clean up mock registrations in `SystemRepository` to prevent cross-test pollution
