# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code). See `.claude/docs/` for detailed docs.

<!-- HUMAN_MAINTAINED_START -->
## Conventions

- **Two namespaces**: `Yueyn.*` (framework), `GenBall.*` (game code)
- **Dual-track architecture**: old `IComponent`/`GameEntry` system (stable, no new features) and new `ISystem`+`SystemRepository` framework (active migration). Both run in parallel.
- **New systems**: implement `ISystem`, never `MonoBehaviour`. Frame update via `IFrameUpdate`/`ILogicUpdate`/`ILateFrameUpdate`.
- **Never edit**: `**/Generated/*.Generated.cs`, `**/*.Bind.cs`, `**/*View.Generated.cs`, `**/*Logic.Generated.cs` (auto-generated)
- **Resource loading**: `#if UNITY_EDITOR` compile macros, never `Application.isEditor` at runtime
- **UI code generation**: `UiViewBinding` component on prefab root → Inspector: Scan → Generate. Outputs `XxxView.Generated.cs` + `XxxLogic.Generated.cs` (partial classes). Hand-written partial goes in `XxxView.cs` / `XxxLogic.cs`. Set `ViewType=Part` for reusable sub-components. Naming prefixes: `Btn*`→Button, `Txt*`→Text, `Img*`→Image, `Rect*`→RectTransform (16 types, see UiBindingConfig).

## Essential Docs

| Doc | Use |
|-----|-----|
| `.claude/docs/architecture.md` | Module system, EntityCreator, singletons deep-dive |
| `.claude/docs/battle-systems.md` | Damage, Death, Character, Bullet, Weapon, Buff details |
| `.claude/docs/systems-overview.md` | Player, Enemy, UI, Map, Procedure, Events overview |
| `.claude/docs/migration-guide.md` | Old vs new system migration path |
| `.claude/docs/code-patterns.md` | Implementation recipes |
| `.claude/docs/conventions.md` | Naming, best practices, partial class patterns |
| `.claude/rules/code-modification-rules.md` | Rules for modifying code |
| `.codebuddy/` | Refactoring plan, migration checklists |
<!-- HUMAN_MAINTAINED_END -->

<!-- AI_MAINTAINED_START -->
- **Self-constraint**: When asked to update this file, never add code examples, class lists, or tables >5 lines. Expand `.claude/docs/` instead and add only a one-line index link here.
- **Current focus**: Phase 1 cleanup done (deleted TimerService/SceneService/UIService dead wrappers; kept ConfigProvider + SaveSystem). Phase 2 pending.
- **Compilation**: Auto-compilation is NOT available. Remind user to manually press Ctrl+R in Unity after code changes; user compiles, then tests run automatically.
- **Testing**: User compiles manually → auto-run tests. Test files go in `Editor/` folder (no asmdef), compiled into Assembly-CSharp-Editor.
- **UI code gen**: Attach `UiViewBinding` to prefab root → set ViewType → Scan → Generate. Outputs `{Name}View.Generated.cs` / `{Name}Logic.Generated.cs`. Python CLI at `Tools/UiCodeGenerator/` (alternative).
<!-- AI_MAINTAINED_END -->