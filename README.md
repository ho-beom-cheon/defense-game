# RuneGate Defense

RuneGate Defense is a Unity 6 mobile 2D portrait defense prototype for a Korea-first 2D pixel fantasy direction. The player protects a sealed gate record from rift hostile entities advancing through 3 lanes. Gatekeeper heroes attack automatically, hero skills can be triggered manually, and rune choices are selected between waves.

The current art identity favors `문지기`, `봉문`, `균열`, `재문`, and record-document language over generic western high-fantasy naming. Concept images live as record-style references, while BattleScene runtime units keep separate small pixel-sprite placeholders.

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

## Bootstrap Progression Prototype

For the v0.3 game shell, run:

`Tools/RuneGate/Bootstrap Progression Prototype`

This creates or updates:

- Scenes: `TitleScene`, `StageSelectScene`, `BattleScene`, `UpgradeScene`
- Sample stages: Goblin Forest 1, 2, 3
- Sample upgrades: Crystal Reinforcement, Hero Training, Battle Rhythm, Skill Practice
- Build Settings scene order for Title -> Stage Select -> Battle -> Upgrade

Open `Assets/_Project/Scenes/TitleScene.unity` after bootstrapping and press Play.

## Bootstrap v0.4 Content Prototype

In Unity, run:

`Tools/RuneGate/Bootstrap v0.4 Content Prototype`

Aliases are also available:

- `Tools/RuneGate/Bootstrap Content Prototype v0.4`
- `Tools/RuneGate/Bootstrap Content v0.4`

This creates or updates:

- 6 MVP heroes, 6 MVP monsters, and 1 Orc Warlord boss
- 6 MVP hero skills
- 20 rune cards
- Goblin Forest stages 1-10
- Default 3 lane x 3 slot formation data
- Title, Stage Select, Battle, and Upgrade scenes
- v0.4 art folder structure

Open `Assets/_Project/Scenes/TitleScene.unity` after bootstrapping and press Play.

## Bootstrap v0.5 Art Prototype

In Unity, run:

`Tools/RuneGate/Bootstrap v0.5 Art Prototype`

This keeps the v0.4 content/progression loop and adds the first package-free art integration pipeline:

- Knight and Goblin placeholder combat prefabs
- Arrow projectile and small hit/death effect prefabs
- Knight and Goblin prototype AnimatorControllers
- Character visual, hit flash, damage text, death delay, and optional audio hooks
- v0.5 art/audio folder structure
- Knight/Goblin data links for prefab and animator controller usage

Open `Assets/_Project/Scenes/TitleScene.unity` after bootstrapping and press Play.

## Bootstrap v1.0 Release Track

In Unity, run:

`Tools/RuneGate/Bootstrap v1.0 Release Track`

This creates or updates the release-track prototype:

- Stage 1-10 Goblin Forest progression
- 6 MVP heroes: Knight, Archer, Fire Mage, Priest, Dwarf Engineer, Assassin
- 6 MVP monsters and Orc Warlord boss
- 20 rune cards
- Tutorial overlay and local tutorial completion save
- Initial art links for concept references, RuntimePixel battle sprites, Shield Bash, Rapid Shot, and Sword Rune when PNG files exist under `Assets/_Project/Art`
- Korea-first identity data fields for heroes, monsters, bosses, and stages:
  - `displayNameKorean`
  - `subtitleKorean`
  - `descriptionKorean`
  - `quoteKorean` for heroes
- Concept reference and runtime pixel art separation:
  - `Assets/_Project/Art/ConceptSheets`
  - `Assets/_Project/Art/RuntimePixel`
  - BattleScene uses RuntimePixel sprites only and falls back to small placeholders when RuntimePixel art is missing.
- Android package settings for `com.hobeomcheon.runegatedefense`
- Build Settings scene order for Title -> Stage Select -> Battle -> Upgrade

For Android APK builds, see [docs/android-build-guide.md](docs/android-build-guide.md).

To relink imported prototype images without rebuilding scenes, run:

`Tools/RuneGate/Apply Initial Art Images`

## Project Docs

- `RUNEGATE_MASTER_PLAN.md`
- `docs/00_PROJECT_CONTEXT.md`
- `docs/01_GAME_DESIGN_DOCUMENT.md`
- `docs/02_DEVELOPMENT_ROADMAP.md`
- `docs/03_CODEX_IMPLEMENTATION_GUIDE.md`
- `docs/04_SYSTEM_ARCHITECTURE.md`
- `docs/08_TEST_AND_QA_PLAN.md`
- `docs/10_NEXT_CODEX_PROMPTS.md`
- `RUNEGATE_NEXT_TASKS_INDEX.md`
- `docs/11_TASK_V03_PROGRESSION_LOOP.md`
- `docs/12_TASK_V04_CONTENT_EXPANSION_AND_PLACEMENT.md`
- `docs/art-guide.md`
- `docs/asset-list.md`
- `docs/v05-art-integration-plan.md`
- `docs/android-build-guide.md`
- `docs/release-checklist.md`
- `docs/known-issues.md`
- `docs/store-listing-draft.md`
- `docs/privacy-checklist.md`
- `docs/release-notes-v1.0.md`
- `docs/hero-character-bible.md`
- `docs/enemy-boss-bible.md`
- `docs/korean-world-identity-guide.md`
- `docs/pixel-art-pipeline.md`
- `docs/art-integration-notes.md`
- `docs/content-balance-v07.md`
- `docs/stage-design.md`
- `docs/rune-design.md`
- `CHANGELOG.md`

## How To Play

1. Run `Tools/RuneGate/Bootstrap v1.0 Release Track`.
2. Open `Assets/_Project/Scenes/TitleScene.unity`.
3. Press Play.
4. Start or Continue to Stage Select.
5. Confirm the tutorial appears on first battle entry.
6. Confirm Stage 1 is unlocked and later stages are locked on a fresh save.
7. Select Stage 1 and clear the battle.
8. Confirm Victory shows earned gold and unlocks Stage 2.
9. Open Upgrade, buy an upgrade if enough gold is available, then return to Stage Select.
10. Stop and restart Play Mode to confirm local save persistence.
11. Continue through Stage 10 and confirm Orc Warlord appears.
12. Use Reset Save on the Title scene to clear test progress.

`Tools/RuneGate/Bootstrap Playable Prototype` still creates the standalone BattleScene-only prototype.

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
- Result panel with Retry, Upgrade, and Stage Select buttons
- v0.3 title, stage select, result flow, upgrade screen, and local JSON save loop
- Stage progression unlocks for Goblin Forest 1-10
- Gold persistence policy:
  - Victory awards 100% of battle gold
  - Defeat awards 50% of battle gold
- Permanent upgrade effects:
  - `crystal_max_hp_flat`
  - `hero_attack_percent`
  - `hero_attack_speed_percent`
  - `skill_cooldown_percent`
- 3 lanes x 3 logical hero slot foundation
- v0.4 formation data and runtime hero placement from local save
- Runtime rune effects:
  - `hero_attack_percent`
  - `hero_attack_speed_percent`
  - `crystal_heal_flat`
  - `skill_cooldown_percent`
  - `hero_hp_percent`
  - `boss_damage_percent`
  - `monster_slow_percent`
- Package-free placeholder visuals
- v0.5 Knight and Goblin placeholder prefab art pipeline
- Character visual controller hooks for move, attack, hit, death, skill, and facing direction
- Runtime hit flash, damage text, projectile fallback, and delayed death cleanup
- Optional local SFX manager hooks for combat, rune, upgrade, victory, and defeat feedback
- v1.0 tutorial overlay and tutorial completion persistence
- v1.0 Android release-track bootstrap and APK build menu
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
    Progression/
    Save/
    UI/
    Editor/
  Data/
    Heroes/
    Monsters/
    Skills/
    Runes/
    Stages/
    Upgrades/
    Formations/
    Rosters/
  Prefabs/
    Heroes/
    Monsters/
    Projectiles/
    Effects/
    UI/
  Scenes/
  Art/
    Characters/
    ConceptSheets/
    RuntimePixel/
    Effects/
    UI/
    Backgrounds/
  Audio/
    SFX/
    BGM/
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

For the v0.5 art pipeline after bootstrapping, run:

`Tools/RuneGate/Validate v0.5 Art Prototype`

For release-track validation after bootstrapping, run:

`Tools/RuneGate/Validate v1.0 Release Track`

To configure Android release settings:

`Tools/RuneGate/Configure Android Release Settings`

To attempt an APK build:

`Tools/RuneGate/Build Android APK v1.0`

For command-line validation when `Unity.exe` is available:

```powershell
& "C:\Path\To\Unity.exe" -batchmode -quit -projectPath "C:\workspace\defense-game" -executeMethod RuneGate.Editor.RuneGateProjectValidator.ValidateFromCommandLine -logFile "C:\workspace\defense-game\Logs\unity-validation.log"
```

## Save Data

Save data is stored locally as JSON:

`Application.persistentDataPath/runegate_save.json`

In the Unity Editor on Windows this is usually under:

`%USERPROFILE%\AppData\LocalLow\<CompanyName>\<ProductName>\runegate_save.json`

Use the Title scene `Reset Save` button during development testing.

## Next Development Checklist

- Prepare hero placement without drag-and-drop polish.
- Expand to 6 MVP heroes and 6 MVP monsters.
- Add a boss wave for the Goblin Forest arc.
- Integrate one representative art sample to validate the art pipeline.
- Add audio placeholders for hit, skill, victory, and defeat feedback.
- Replace placeholder visuals with real 2D art after core loops settle.
- Build real UGUI or TextMeshPro HUD after UI package decisions are made.
- Add explicit skill behavior types instead of placeholder direct/heal behavior.
- Expand rune effects and stacking rules.
- Add Android build configuration once Unity Android modules are installed.
