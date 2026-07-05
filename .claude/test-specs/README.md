# Test Specs

Test specs define WHAT to test before writing any code. Each spec is a
Given/When/Then specification reviewed and approved by the user.

## File naming

```
.claude/test-specs/<feature-slug>.md       ← Test spec (what to test)
.claude/test-specs/<feature-slug>.impl.md  ← Implementation spec (where/how to build)
```

## Test Spec format (`.md`)

```markdown
# Test Spec: <feature name>

> Status: draft | approved | implemented
> Created: YYYY-MM-DD

## TC-001: <brief description>
- **Given**: <preconditions — be specific, use concrete values>
- **When**: <action>
- **Then**: <expected outcome — concrete, verifiable>

## TC-002: <edge case>
- **Given**: ...
- **When**: ...
- **Then**: ...
```

### Rules
- Each TC has a unique ID (TC-001, TC-002, ...)
- Given/When/Then values must be **concrete and verifiable** — no "should work correctly"
- Cover: happy path, boundary conditions, error cases
- Keep under ~40 lines per spec file

## Implementation Spec format (`.impl.md`)

```markdown
# Implementation Spec: <feature name>

## Files to create/modify
- `Assets/Scripts/.../IXxx.cs` — new interface
- `Assets/Scripts/.../XxxDefault.cs` — default implementation
- `Assets/Scripts/.../Editor/XxxTests.cs` — tests

## Integration points
- Register in `SystemRepository` via `FrameworkDefault.DoInit()`
- ...

## Constraints
- Must follow ISystem pattern
- Do not add DI frameworks
- Keep production code unchanged except as specified
```
