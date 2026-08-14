# The Pilgrim's Journey — Copilot Instructions

Unity (C#) reverse tower defence bullet hell adventure game. This project is split into 6 top-level assemblies, each with several child assemblies. Copilot cannot see `.asmdef` references directly — treat the rules below as ground truth for what is and isn't allowed to reference what.

## Assembly hierarchy (top to bottom = allowed reference direction)

1. **Utils** — no dependencies. Generic, game-agnostic helpers.
2. **Core** — may reference Utils only.
3. **Gameplay** — may reference Core, Utils.
4. **UI** — may reference Gameplay, Core, Utils.
5. **Editor** — may reference UI, Gameplay, Core, Utils. Editor-only, uses `UnityEditor` APIs.
6. **Tests** — may reference all of the above.

**Hard rule:** a lower tier never references a higher tier (e.g. nothing in Core or Utils may reference anything in Gameplay, UI, or Editor). If a suggestion would require that, it's wrong — restructure instead of adding the reference.

## Child assemblies

Each top-level assembly is split into child assemblies scoped to one concern. A child assembly may reference:
- its own parent's allowed tiers (e.g. `Core.SaveSystem` may reference `Utils` and  any `Utils.*` child), and
- other children of the *same* parent **only if explicitly listed as a dependency below** — don't assume sibling children can see each other.

### Utils
- `Utils.AbstractSelector`
- `Utils.DraggableArea`

### Core
- `Core.Buffs`
- `Core.DialogueSystem`
- `Core.GameManagement`
- `Core.Managers`
- `Core.SaveSystem`
- `Core.SceneManagement`

### Gameplay
- (No child assemblies at the moment)

### UI
- `UI.Minimap`
- `UI.WindowStyles`


### Editor
- `Editor.Buff`
- `Editor.DialogueSystem`
- `Editor.Equipment`
- `Editor.Helper.Font`

### Tests
- `Tests.Attacks`

- `Core.Managers` also references `Core.GameManagement`

## When writing or suggesting a script

- State which assembly (including child) the script belongs in, and match its
  existing namespace convention (e.g. `PilgrimsJourney.Core.SaveSystem`).
- Never suggest a `using` or type reference that crosses tiers upward, or between
  unlisted siblings — flag it as a missing dependency instead of guessing.
- Editor-only code (`UnityEditor.*`) belongs only in `Editor` or its children.
