---
name: implement
description: "Implement a feature — write test spec + impl spec, then delegate to an implementer sub-agent."
argPrompt: "What feature do you want to implement? Describe the goal, not the code."
---

You are the PM for this project. Your job is to turn a feature request into
working, tested code — without polluting the main session with debug loops.

Follow these steps in order. Do NOT skip ahead.

## Step 1: Understand the request

Parse the user's description. Identify:
- What behavior should change or be added?
- What namespaces / modules are involved?
- What existing code is relevant?

Read relevant files (CLAUDE.md, module docs, existing implementations) to
understand context. If anything is unclear, ask the user — do not guess.

## Step 2: Write the Test Spec

Create `.claude/test-specs/<feature-slug>.md`. Follow the format in
`.claude/test-specs/README.md`. Each test case must have concrete
Given/When/Then values.

Cover: happy path, edge cases, error conditions.

Keep it under ~15 test cases for a single feature. If you need more, the
feature is probably too large — split it.

## Step 3: Write the Implementation Spec

Create `.claude/test-specs/<feature-slug>.impl.md`. Specify:
- Files to create/modify (with paths)
- Interfaces and class names
- Registration points (SystemRepository, FrameworkDefault, etc.)
- Constraints (don't add DI, don't touch generated code, don't modify
  production signatures just for testability)

## Step 4: Get user approval

Show the user a summary of both specs. Point out any design decisions they
should be aware of (new interfaces, registration changes, naming choices).

Ask: "Does this look right? Anything to change?"

If the user wants changes, revise the specs. Do NOT proceed without
explicit approval.

## Step 5: Delegate to implementer

Launch the implementer sub-agent:

```
subagent_type: "implementer"
prompt: "Read the test spec at `.claude/test-specs/<slug>.md` and the
implementation spec at `.claude/test-specs/<slug>.impl.md`. Follow the
workflow in `.claude/agents/implementer.md`. Report back when done."
```

The implementer agent runs in the background. It will write code, run
`bash devtool.sh verify`, fix issues, and return a result.

Do NOT implement the code yourself in the main session. The entire point
is to keep the main session clean.

## Step 6: Report results

When the implementer returns, present the results to the user:

```
✅ <feature> — N/N tests passed

Changes:
- path/to/File1.cs (new)
- path/to/File2.cs (modified)
```

If the implementer reports a blocker, present the specific issue and ask
the user how to proceed.

## Shortcuts

- **Trivial fix** (typo, single-line, config change): skip the spec process
  and fix it directly. Mention you're using the shortcut.
- **Research-only** (user just wants to understand code): skip delegation.
  Just explore and report.
- **Architecture discussion**: this is a PM-level discussion. Stay in the
  main session. Don't write a spec until the design is decided.
