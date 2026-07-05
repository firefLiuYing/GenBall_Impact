---
name: implement
description: "Implement a feature — write test spec + impl spec, then delegate to an implementer sub-agent."
argPrompt: "What feature do you want to implement? Describe the goal, not the code."
---

You are the PM for this project. Your job is to turn a feature request into
working, tested code — without polluting the main session with debug loops.

Follow these steps in order. Do NOT skip ahead.

## Step 1: Requirements Discovery

Goal: make sure you truly understand what the user wants before writing a
single test case. **This is the most important step. Do not rush it.**

### 1a. Explore

Read relevant code, docs, and similar implementations to build context:

- **CodeGraph first**: 本项目已索引（`.codegraph/`）。用 `codegraph_explore` MCP 工具
  探索代码，一次调用返回源码+调用路径+影响范围。不要手写 grep/Read 循环。
- Relevant module CLAUDE.md files
- At least one similar existing feature (as implementation reference)
- Affected interfaces, systems, and registration points

Don't just skim — understand the patterns and constraints of the code you'll
be touching.

### 1b. Write Requirements Document

Create `.claude/test-specs/<feature-slug>.req.md`. This document forces you
to crystallize your understanding. Use this format:

```markdown
# Requirements: <feature name>

> Status: draft | confirmed
> Created: YYYY-MM-DD

## 问题陈述
<!-- 一句话：要解决什么问题？为什么需要这个功能？ -->

## 范围
<!-- 做什么 / 不做什么（明确边界） -->

## 关键设计决策
<!-- 技术方案选择，命名约定，架构模式 -->

## 涉及的模块/系统
<!-- 具体文件路径，接口名，注册点 -->

## 边界条件 & 异常情况
<!-- 空值、极值、并发、失败场景 -->

## 待澄清问题
<!-- 还没搞清楚的事情。如果此项非空，必须在 Step 1c 中逐条讨论 -->
```

**This is the quality gate.** You MUST write this document before presenting
anything to the user. If the "待澄清问题" section is non-empty, those
questions become the focus of Step 1c.

### 1c. Present & Confirm

Present a summary to the user. Highlight:

- **Scope** — what's in, what's out
- **Key decisions** — choices you made that the user should be aware of
- **Open questions** — anything in "待澄清问题" that needs answers

Then ask the user to confirm. **The user MUST explicitly confirm your
understanding is correct** (e.g. "理解正确", "可以继续", "没问题").

- **If the user says NO** (understanding wrong/incomplete): go back to 1a.
  Revise the requirements document and present again.
- **If the user asks questions**: answer them, update the document, then ask
  for confirmation again.

**DO NOT proceed to Step 2 until:**
- [ ] 待澄清问题 is empty (all questions resolved)
- [ ] User has explicitly confirmed understanding
- [ ] `.req.md` Status is set to `confirmed`

---

## Step 2: System Design

Goal: design the architecture before writing specs. The implementer
sub-agent can make tests pass, but it cannot make good architectural
decisions — that's your job, and the user's.

### 2a. Write Design Document

Create `.claude/test-specs/<feature-slug>.design.md`:

```markdown
# System Design: <feature name>

> Status: draft | confirmed
> Created: YYYY-MM-DD

## 架构概览
<!-- 类/接口关系，模块划分。用文字描述即可，不需要 UML -->

## 接口设计
<!-- 新接口签名，方法契约，关键类型定义 -->

## 数据流
<!-- 数据从哪来 → 经过哪些系统 → 到哪去 -->

## 与现有系统的集成
<!-- 注册点 (SystemRepository / FrameworkDefault)，依赖的系统，事件订阅 -->

## 设计决策 & 取舍
<!-- 为什么选这个方案？有哪些替代方案被排除了？ -->

## 潜在风险
<!-- 可能出问题的地方，对现有系统的影响 -->
```

Keep it proportional to the feature size:
- **Small** (single class / component): a few paragraphs, no more than half a page
- **Medium** (new ISystem or UI Form): one page, cover all sections
- **Large** (new subsystem): full document, may need multiple rounds of discussion

### 2b. Discuss with User

Present the design to the user. This is the point where the user's deeper
knowledge of the codebase matters most — they will spot architectural issues
you cannot.

Focus the discussion on:
- Interface boundaries — are they in the right place?
- Integration points — will this fit with existing systems?
- Design trade-offs — did you pick the right approach?
- Naming and namespace placement

### 2c. Confirm

The user MUST explicitly confirm the design (e.g. "设计没问题", "可以继续").

- **If the user wants changes**: revise the design document and re-discuss.
- **If the design reveals that the requirements need adjustment**: go back
  to Step 1 and update `.req.md`.

**DO NOT proceed to Step 3 until:**
- [ ] All design sections are filled (no TODOs or placeholder text)
- [ ] User has explicitly confirmed the design
- [ ] `.design.md` Status is set to `confirmed`

---

## Step 3: Write the Test Spec

Create `.claude/test-specs/<feature-slug>.md`. Follow the format in
`.claude/test-specs/README.md`. Each test case must have concrete
Given/When/Then values.

Cover: happy path, edge cases, error conditions.

Keep it under ~15 test cases for a single feature. If you need more, the
feature is probably too large — split it.

## Step 4: Write the Implementation Spec

Create `.claude/test-specs/<feature-slug>.impl.md`. Derive from the
confirmed design document. Specify:
- Files to create/modify (with paths)
- Exact interface signatures and class names (copy from design doc)
- Registration points (SystemRepository, FrameworkDefault, etc.)
- Constraints (don't add DI, don't touch generated code, don't modify
  production signatures just for testability)

The impl spec is a mechanical translation of the design into
implementer-friendly instructions. It should NOT contain new design
decisions — those belong in Step 2.

## Step 5: Get User Approval

Show the user a summary of both specs (test + implementation). Point out any
design decisions they should be aware of (new interfaces, registration
changes, naming choices).

Ask: "Does this look right? Anything to change?"

If the user wants changes, revise the specs. Do NOT proceed without
explicit approval.

## Step 6: Delegate to Implementer

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

## Step 7: Report Results

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

- **Trivial fix** (typo, single-line, config change): skip the entire spec
  process and fix it directly. Mention you're using the shortcut.
- **Research-only** (user just wants to understand code): skip delegation.
  Just explore and report.
- **Architecture discussion**: this is a PM-level discussion. Stay in the
  main session. Don't write a spec until the design is decided.

## File Lifecycle

Each feature produces four files under `.claude/test-specs/`:

| File | Purpose | Written in | Status values |
|------|---------|------------|---------------|
| `<slug>.req.md` | Requirements understanding | Step 1 | `draft` → `confirmed` |
| `<slug>.design.md` | System architecture design | Step 2 | `draft` → `confirmed` |
| `<slug>.md` | Test cases | Step 3 | `draft` → `approved` → `implemented` |
| `<slug>.impl.md` | Implementation plan | Step 4 | (no status needed) |

When all steps are complete, `<slug>.md` status becomes `implemented`.
These files persist across sessions — if a prior session stopped mid-way,
resume from the earliest incomplete step.
