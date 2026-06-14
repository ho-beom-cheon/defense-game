# RuneGate Defense

RuneGate Defense is a Unity 6 mobile 2D portrait defense prototype. The player defends a Kingdom Crystal from Goblins and Orcs advancing through 3 lanes from a Rune Gate. Knight and Archer heroes attack automatically, hero skills can be triggered manually, and rune cards are selected between waves.

## Required Unity Version

- Unity 6 recommended
- 2D project template recommended
- Android Build Support is not required for editor prototype play, but is needed later for mobile device builds

## How To Open

1. Clone or open this repository at `C:\workspace\defense-game`.
2. Open the folder with Unity Hub or Unity Editor.
3. Wait for scripts to compile.

## Bootstrap Playable Prototype

In Unity, run:

`Tools/RuneGate/Bootstrap Playable Prototype`

This creates or updates:

- Sample heroes: Knight, Archer
- Sample monsters: Goblin, Orc
- Sample skills: Shield Bash, Rapid Shot
- Sample runes: Sword Rune, Bow Rune, Healing Rune
- Sample stage: Goblin Forest 1
- Scene: `Assets/_Project/Scenes/BattleScene.unity`

The older menu alias `Tools/RuneGate/Bootstrap MVP` is also available.

## Project Docs

- `RUNEGATE_MASTER_PLAN.md`
- `docs/00_PROJECT_CONTEXT.md`
- `docs/01_GAME_DESIGN_DOCUMENT.md`
- `docs/02_DEVELOPMENT_ROADMAP.md`
- `docs/03_CODEX_IMPLEMENTATION_GUIDE.md`
- `docs/04_SYSTEM_ARCHITECTURE.md`
- `docs/08_TEST_AND_QA_PLAN.md`
- `docs/10_NEXT_CODEX_PROMPTS.md`

## How To Play

1. Run `Tools/RuneGate/Bootstrap Playable Prototype`.
2. Open `Assets/_Project/Scenes/BattleScene.unity`.
3. Press Play.
4. Watch Goblins and Orcs spawn from the right side and move through 3 lanes.
5. Knight and Archer attack automatically when monsters enter range.
6. Confirm monsters show HP bars and flash when hit.
7. Click skill buttons and confirm cooldown text disables them until ready.
8. After the first non-final wave clears, choose 1 of 3 rune cards.
9. Confirm the selected rune changes hero ATK/SPD or heals the crystal.
10. Clear all waves for Victory, or let the crystal reach 0 HP for Defeat.
11. Click Restart on the result panel to start the battle again.

## Implemented

- 3-lane battle flow
- Kingdom Crystal HP and defeat handling
- Wave-based monster spawning
- Goblin and Orc sample monster data
- Knight and Archer sample hero data
- Hero auto attack
- Manual hero skill trigger buttons
- Skill cooldown button state
- Monster movement toward the crystal
- Monster HP bar and hit flash feedback
- Crystal hit flash feedback
- Crystal damage when monsters arrive
- Monster death and gold reward tracking
- Victory when all waves are cleared
- Rune selection after non-final waves
- Result panel with Restart button
- 3 lanes x 3 logical hero slot foundation
- Runtime rune effects:
  - `hero_attack_percent`
  - `hero_attack_speed_percent`
  - `crystal_heal_flat`
- Package-free placeholder visuals
- Package-free Play Mode GUI
- Unity Editor bootstrapper and project validator
- Unity `.gitignore`
- Git LFS tracking rules for common large art/audio files

## MVP Exclusions

- No server
- No login
- No gacha
- No ads
- No in-app purchase
- No analytics
- No external paid APIs
- No multiplayer
- No ranking
- No cloud save
- No Addressables yet
- No Firebase yet

## Folder Structure

```text
Assets/_Project/
  Scripts/
    Core/
    Battle/
    Hero/
    Monster/
    Skill/
    Rune/
    Wave/
    Data/
    Save/
    UI/
    Editor/
  Data/
    Heroes/
    Monsters/
    Skills/
    Runes/
    Stages/
  Prefabs/
    Heroes/
    Monsters/
    UI/
  Scenes/
  Art/
    Characters/
    Monsters/
    Effects/
    UI/
  Audio/
  Resources/
```

## Coding Rules

- Use the `RuneGate` namespace.
- Keep MonoBehaviours small and focused.
- Use ScriptableObjects for balance and content data.
- Prefer serialized private fields over public mutable fields.
- Use events where UI observes battle state.
- Keep battle logic separate from UI logic.
- Add null guards and clear `Debug.LogWarning` messages.
- Avoid singletons unless there is a strong reason.
- Avoid networking, monetization, analytics, Addressables, and paid dependencies during the MVP.

## Validation

Inside Unity, run:

`Tools/RuneGate/Validate Project`

For command-line validation when `Unity.exe` is available:

```powershell
& "C:\Path\To\Unity.exe" -batchmode -quit -projectPath "C:\workspace\defense-game" -executeMethod RuneGate.Editor.RuneGateProjectValidator.ValidateFromCommandLine -logFile "C:\workspace\defense-game\Logs\unity-validation.log"
```

## Next Development Checklist

- Replace placeholder visuals with real 2D art.
- Build real UGUI or TextMeshPro HUD after UI package decisions are made.
- Add hero placement and upgrade flow.
- Add explicit skill behavior types instead of placeholder direct/heal behavior.
- Expand rune effects and stacking rules.
- Add boss wave data and boss-specific behavior.
- Add local save data for stage progress and settings.
- Add Android build configuration once Unity Android modules are installed.
