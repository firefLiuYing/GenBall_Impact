# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code). See `.claude/docs/` for detailed docs.

<!-- HUMAN_MAINTAINED_START -->
## Conventions

- **Two namespaces**: `Yueyn.*` (framework), `GenBall.*` (game code)
- **Architecture target**: Three-layer legacy → converge to BattleEntity (thin MB shell) + optional components (Stat, DamageReceiver, BuffContainer, Attack, CommandDispatcher, DecisionLayer) + ISystem global services. Old IComponent + CharacterState are deprecated, being phased out.
- **New systems**: implement `ISystem`, never `MonoBehaviour`. Frame update via `IFrameUpdate`/`ILogicUpdate`/`ILateFrameUpdate`.
- **Never edit**: `**/Generated/*.Generated.cs`, `**/*.Bind.cs`, `**/*View.Generated.cs`, `**/*Logic.Generated.cs` (auto-generated)
- **Resource loading**: `#if UNITY_EDITOR` compile macros, never `Application.isEditor` at runtime
- **UI**: Logic-driven rendering; FormLogic→FormView, PartLogic→PartView, BusinessLogicBase for cross-form coordination. See `Assets/Scripts/GenBall/UI/CLAUDE.md`.

## Primary Reference (read first each session)

| Doc | Use |
|-----|-----|
| `.claude/docs/execution-plan.md` | **Long-term plan** — 5 phases, task-level tracking, cross-session persistent |
| `.claude/docs/design/battle-entity-architecture.md` | **Target architecture** — BattleEntity + optional components + ISystem services |

## Module CLAUDE.md

Module-specific rules auto-loaded when working in those directories:

| Path | Module |
|------|--------|
| `Assets/Scripts/GenBall/UI/CLAUDE.md` | UI — Form/Part lifecycle, communication patterns, UiViewBinding |
| `Assets/Scripts/GenBall/BattleSystem/CLAUDE.md` | Battle — BattleEntity framework, components, decision/command/executor layers |
| `Assets/Scripts/GenBall/Interact/CLAUDE.md` | Interact — IInteractable/IInteractSystem, cone sight detection, interaction flow |
| `Assets/Scripts/GenBall/Event/CLAUDE.md` | Event — EventAdapter usage, event ID ranges, parameter types, PropertyDrawer |
| `Assets/Scripts/Yueyn/CLAUDE.md` | Framework — ISystem/Singleton patterns, SystemRepository, update lifecycle |

## Essential Docs

| Doc | Use |
|-----|-----|
| `.claude/docs/battle-systems.md` | Damage, Death, Character, Bullet, Weapon, Buff details |
| `.claude/docs/systems-overview.md` | Player, Enemy, UI, Map, Procedure, Events overview |
| `.claude/docs/migration-guide.md` | Old vs new system migration path |
| `.claude/docs/code-patterns.md` | Implementation recipes |
| `.claude/docs/conventions.md` | Naming, best practices, partial class patterns |
| `.claude/docs/design/` | Game design docs (world, 3C, weapons, enemies, levels, economy) |
| `.claude/rules/code-modification-rules.md` | Rules for modifying code |
<!-- HUMAN_MAINTAINED_END -->

<!-- AI_MAINTAINED_START -->
- **Self-constraint**: When asked to update this file, never add code examples, class lists, or tables >5 lines. Expand `.claude/docs/` instead and add only a one-line index link here.
- **Current focus**: 开发工具 Phase 4 Iteration 1 完成 — 8 个 MCP UI 工具（create/add/delete/remove prefab+GameObject+component+property+code gen）；Form/Part 双模式；TCP warmup fix；`/create-ui` Step 1-2 全自动化。计划见 `.claude/temp/devtools-fix-plan.md`。
- **Compilation**: MCP 用 `mcp__unity__unity_compile`（支持 fullRebuild）。子 Agent 用 `bash devtool.sh compile [--full]`（文件 IPC，不需要 MCP）。`compile_cli.py` 冲突于端口 9876，不要用。状态文件: `Temp/UnityMcpCompileState.json`。
- **Testing**: MCP 暂未支持 test（TODO）。子 Agent 用 `bash devtool.sh test [--class X] [--method X]`。旧脚本 `bash run_editmode_tests.sh` 仍可用（向后兼容）。
- **PM 工作流**: 当用户提出功能需求或代码改动时，用 `/implement` Skill（`.claude/skills/implement/SKILL.md`）走标准流程：需求探索→需求文档(.req.md)→用户确认→系统设计(.design.md)→用户确认→Test Spec+Impl Spec→用户确认→派 implementer 子 Agent→汇报结果。框架设计和架构决策保留在主会话。若改动极小（单行修复）可跳过。
- **New .cs files**: `SyncOrphanScripts()` 在每次编译前自动导入/清理 .meta，无需手动处理。如果用 `File.WriteAllText` 写入后立即读回触发 Unity 文件监听，可加快导入速度。
<!-- AI_MAINTAINED_END -->