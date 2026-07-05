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
- **Current focus**: 开发工具修复 Phase 1D 完成 — MCP 编译状态机重构（状态文件唯一真相源，phase: refresh_pending→compiling→done/no_changes）。Phase 1A/B 待推进，Phase 2-4 待细化。计划见 `.claude/temp/devtools-fix-plan.md`。
- **Compilation**: Use `mcp__unity__unity_compile` (NOT `compile_cli.py` — conflicts on TCP port 9876). Returns `compilation_complete`/`no_changes` with errors/warnings. State file at `Temp/UnityMcpCompileState.json`.
- **Testing**: Use `bash run_editmode_tests.sh` (file IPC, Unity's `TestsAutoRunner` picks up trigger). Test files in `Editor/` folder (no asmdef).
<!-- AI_MAINTAINED_END -->