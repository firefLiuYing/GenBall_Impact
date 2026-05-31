# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code). See `.claude/docs/` for detailed docs.

<!-- HUMAN_MAINTAINED_START -->
## Conventions

- **Two namespaces**: `Yueyn.*` (framework), `GenBall.*` (game code)
- **Architecture target**: Three-layer legacy → converge to BattleEntity (thin MB shell) + optional components (Stat, DamageReceiver, BuffContainer, Attack, CommandDispatcher, DecisionLayer) + ISystem global services. Old IComponent + CharacterState are deprecated, being phased out.
- **New systems**: implement `ISystem`, never `MonoBehaviour`. Frame update via `IFrameUpdate`/`ILogicUpdate`/`ILateFrameUpdate`.
- **Never edit**: `**/Generated/*.Generated.cs`, `**/*.Bind.cs`, `**/*View.Generated.cs`, `**/*Logic.Generated.cs` (auto-generated)
- **Resource loading**: `#if UNITY_EDITOR` compile macros, never `Application.isEditor` at runtime
- **UI code generation**: `UiViewBinding` component on prefab root → Inspector: Scan → Generate. Outputs `{Name}View.cs` + `{Name}Logic.cs` + `{Name}ViewData.cs`. Generated bindings inside `### GENERATED_BINDINGS ###` markers; hand-written code goes outside. Set `ViewType=Part` for reusable sub-components. Use `/create-ui` skill for the full workflow.

## Primary Reference (read first each session)

| Doc | Use |
|-----|-----|
| `.claude/docs/execution-plan.md` | **Long-term plan** — 5 phases, task-level tracking, cross-session persistent |
| `.claude/docs/design/battle-entity-architecture.md` | **Target architecture** — BattleEntity + optional components + ISystem services |

## Essential Docs

| Doc | Use |
|-----|-----|
| `.claude/docs/architecture.md` | Module system, EntityCreator, singletons deep-dive |
| `.claude/docs/battle-systems.md` | Damage, Death, Character, Bullet, Weapon, Buff details |
| `.claude/docs/systems-overview.md` | Player, Enemy, UI, Map, Procedure, Events overview |
| `.claude/docs/migration-guide.md` | Old vs new system migration path |
| `.claude/docs/code-patterns.md` | Implementation recipes |
| `.claude/docs/ui-architecture.md` | BusinessFormLogic/PartLogic/UIComponent class hierarchy and lifecycle |
| `.claude/docs/conventions.md` | Naming, best practices, partial class patterns |
| `.claude/docs/design/` | Game design docs (world, 3C, weapons, enemies, levels, economy) |
| `.claude/rules/code-modification-rules.md` | Rules for modifying code |
<!-- HUMAN_MAINTAINED_END -->

<!-- AI_MAINTAINED_START -->
- **Self-constraint**: When asked to update this file, never add code examples, class lists, or tables >5 lines. Expand `.claude/docs/` instead and add only a one-line index link here.
- **Current focus**: Phase B 实体迁移 — B-1 Player (90%), B-2 Enemy (80%), 待编译验证。见 `.claude/docs/execution-plan.md`。
- **Compilation**: Use `mcp__unity__unity_compile` to trigger compilation and get results (errors/warnings with file/line). Waits up to 120s.
- **Testing**: User compiles manually → auto-run tests. Test files go in `Editor/` folder (no asmdef), compiled into Assembly-CSharp-Editor.
- **UI code gen**: Attach `UiViewBinding` to prefab root → set ViewType → Scan → Generate. Outputs `{Name}View.cs` / `{Name}Logic.cs`. Use `/create-ui` skill for full workflow. Python CLI at `Tools/UiCodeGenerator/` (alternative).
<!-- AI_MAINTAINED_END -->