# Test Specs

Test specs define WHAT to test before writing any code. Each spec is a
Given/When/Then specification reviewed and approved by the user.

## File naming

```
.claude/test-specs/<feature-slug>.req.md    ← Requirements doc (what & why)
.claude/test-specs/<feature-slug>.design.md ← System design (architecture & interfaces)
.claude/test-specs/<feature-slug>.md        ← Test spec (what to test)
.claude/test-specs/<feature-slug>.impl.md   ← Implementation spec (where/how to build)
```

## Lifecycle

```
.req.md:    draft → confirmed
.design.md: draft → confirmed
.md:        draft → approved → implemented
```

1. **Requirements doc** (`.req.md`) — written first. Forces PM to crystallize
   understanding before writing tests. Must be user-confirmed.
2. **System design** (`.design.md`) — architecture, interfaces, data flow,
   design trade-offs. Discussed with user before any spec is written.
3. **Test spec** (`.md`) — written after requirements and design confirmed.
   Concrete Given/When/Then cases.
4. **Impl spec** (`.impl.md`) — written alongside test spec. Mechanical
   translation of the design into file paths and implementer instructions.

Files persist across sessions. If a prior session stopped mid-way, resume
from the earliest incomplete step.

---

## Requirements Doc format (`.req.md`)

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
<!-- 还没搞清楚的事情。如果此项非空，必须在确认前逐条讨论 -->
```

### Quality gate

Before proceeding to system design, the requirements doc must be:

- [ ] 待澄清问题 is empty
- [ ] Status is `confirmed` (user explicitly agreed)
- [ ] User has said "可以继续" or equivalent

---

## System Design format (`.design.md`)

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

### Scope by feature size

- **Small** (single class / component): a few paragraphs, half a page
- **Medium** (new ISystem or UI Form): one page, cover all sections
- **Large** (new subsystem): full document, may need multiple rounds

### Quality gate

Before proceeding to test spec, the design must be:

- [ ] All sections filled (no TODOs or placeholder text)
- [ ] Status is `confirmed` (user explicitly agreed on architecture)
- [ ] User has said "设计没问题" or equivalent

---

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
- Keep under ~15 test cases (or ~40 lines) per spec file

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
