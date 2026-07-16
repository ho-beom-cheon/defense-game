# RuneGate Defense Art Guide

## Current Art Identity Policy

RuneGate Defense now targets a Korea-first mobile 2D pixel fantasy identity. Use `문지기`, `봉문`, `균열`, `재문`, and `기록` as the primary world-language pillars.

Record-style concept images belong under `Assets/_Project/Art/ConceptSheets` and should be treated as `문지기 기록`, `봉문 위험 기록`, or `균열 적성 기록` references. They are not BattleScene unit sprites.

Runtime battle sprites belong under `Assets/_Project/Art/RuntimePixel` and should be built separately as small, readable pixel sprites.

## Current v0.5 Policy

v0.5 keeps placeholder sprites, primitive objects, and package-free generated prefabs only. Final illustrations, paid assets, Addressables, Firebase, and external art packages are intentionally excluded.

The purpose of the art pipeline in this version is to validate the first real integration path: data assets can point to character prefabs, prefabs can own visual controllers and animator controllers, and battle code can trigger attack, hit, death, and simple projectile feedback without mutating ScriptableObject source values.

## Folder Contract

```text
Assets/_Project/Art/
  ConceptSheets/
    Heroes/
    Enemies/
  RuntimePixel/
    Heroes/
    Monsters/
    Bosses/
    UI/
  Characters/
    Heroes/
      Knight/
        Sprites/
        Animations/
        Materials/
      Archer/
        Sprites/
        Animations/
        Materials/
      FireMage/
        Sprites/
        Animations/
        Materials/
      Cleric/
        Sprites/
        Animations/
        Materials/
      Priest/
        Sprites/
        Animations/
        Materials/
      DwarfEngineer/
        Sprites/
        Animations/
        Materials/
      Assassin/
        Sprites/
        Animations/
        Materials/
    Monsters/
      Goblin/
        Sprites/
        Animations/
        Materials/
      Wolf/
        Sprites/
        Animations/
        Materials/
      Orc/
        Sprites/
        Animations/
        Materials/
      Bat/
        Sprites/
        Animations/
        Materials/
      Slime/
        Sprites/
        Animations/
        Materials/
      Skeleton/
        Sprites/
        Animations/
        Materials/
    Bosses/
      OrcWarlord/
        Sprites/
        Animations/
        Materials/
  Effects/
    Skills/
    Hit/
    Projectiles/
    Death/
  UI/
    Icons/
      Heroes/
      Skills/
      Runes/
      Upgrades/
    Buttons/
    Panels/
    Runes/
    Bars/
  Backgrounds/
  Placeholders/
  Audio/
    SFX/
    BGM/
Assets/_Project/Audio/
  SFX/
  BGM/
```

## Naming

Recommended file names for future sample art:

```text
hero_knight_idle.png
hero_knight_attack.png
hero_knight_hit.png
hero_knight_skill.png
hero_knight_death.png

monster_goblin_walk.png
monster_goblin_hit.png
monster_goblin_death.png

icon_rune_sword.png
icon_rune_bow.png
icon_rune_healing.png
icon_skill_shield_bash.png
icon_upgrade_crystal_reinforcement.png
```

## Unity Import

- Texture Type: Sprite (2D and UI)
- Sprite Mode: Single for one-off placeholder art, Multiple for sprite sheets
- Pixels Per Unit: 100 as the baseline until a final scale pass
- Compression: Normal Quality for mobile test builds, None for visual QA if needed
- Pivot: bottom-center for characters and monsters

## Initial Image Integration

`Tools/RuneGate/Bootstrap v1.0 Release Track` and `Tools/RuneGate/Apply Initial Art Images` scan the configured art folders for PNG/JPG files, set them to single Sprite import mode, and connect the first matching prototype image to the relevant ScriptableObject.

Currently linked:

- Knight and Archer hero art
- Goblin, Orc, and Orc Warlord monster art
- Shield Bash and Rapid Shot skill icons
- Sword Rune icon

The Crystal/Gate background and concept sheet are imported as reference Sprites, but they are not used directly as battle sprites yet. Concept sheets must remain reference material only.

## Korean Identity References

Reference-only concept images currently include:

- `Assets/_Project/Art/ConceptSheets/Heroes/문지기_레온의_기록_카드.png`
- `Assets/_Project/Art/ConceptSheets/Heroes/문지기_기록_바람길을_읽는_궁수.png`
- `Assets/_Project/Art/ConceptSheets/Heroes/카엘_잿불의_방랑_마법사.png`
- `Assets/_Project/Art/ConceptSheets/Heroes/미레아_금빛_성서의_사제.png`
- `Assets/_Project/Art/ConceptSheets/Heroes/브롬_룬포지_기술자_카드.png`
- `Assets/_Project/Art/ConceptSheets/Heroes/그림자_균열의_암살자_카드.png`
- `Assets/_Project/Art/ConceptSheets/Enemies/균열_적성_기록_괴물_수집가.png`
- `Assets/_Project/Art/ConceptSheets/Enemies/문파괴자_그룸바르의_위협_기록.png`

For the detailed terminology and runtime split, see:

- `docs/korean-world-identity-guide.md`
- `docs/hero-character-bible.md`
- `docs/enemy-boss-bible.md`
- `docs/pixel-art-pipeline.md`
- `docs/art-integration-notes.md`

## Prefab Contract

The v0.5 bootstrapper creates these package-free placeholder prefabs:

- `Assets/_Project/Prefabs/Heroes/Hero_Knight.prefab`
- `Assets/_Project/Prefabs/Monsters/Monster_Goblin.prefab`
- `Assets/_Project/Prefabs/Projectiles/Projectile_Arrow.prefab`
- `Assets/_Project/Prefabs/Effects/Effect_Hit_Small.prefab`
- `Assets/_Project/Prefabs/Effects/Effect_Death_Small.prefab`

Character prefabs should keep this shape:

```text
Root
  Visual (SpriteRenderer, Animator, CharacterVisualController, HitFlashController)
  HealthBarAnchor
  SkillEffectAnchor or HitEffectAnchor
  SelectionIndicator for heroes
```

Animator controllers should support these state names and parameters:

- States: `Idle`, `Move`, `Attack`, `Hit`, `Death`, `Skill`
- Bool parameters: `IsMoving`, `IsDead`
- Trigger parameters: `Attack`, `Hit`, `Death`, `Skill`

## Runtime Hooks

`HeroData` keeps `portrait`, `conceptImage`, `battleSprite`, `animatorController`, and `prefab` fields. `portrait` and `conceptImage` are for codex/settings/detail screens. `battleSprite` is only for small RuntimePixel battle sprites.

`MonsterData` keeps `conceptImage`, `runtimeSprite`, legacy `sprite` compatibility, `animatorController`, `prefab`, and `isBoss` fields. Runtime code treats `Sprite` as the `runtimeSprite` getter; concept images must not be used by BattleScene SpriteRenderers.

`SkillData`, `RuneData`, and `UpgradeData` have icon fields so UI can show sample icons later without changing the data model.

`HeroPlacementManager` and `WaveManager` prefer RuntimePixel data-linked prefabs when present and fall back to small placeholder sprites when those fields are empty. Adding a RuntimePixel sample later should only require importing sprites, assigning controller clips, and connecting the relevant ScriptableObject runtime fields.

## Android Screen Baseline

- Primary target: 1080 x 1920 portrait
- Secondary check: 720 x 1280 portrait
- Battle view must keep the continuous battlefield, crystal, rift, heroes, monsters, HUD, skill buttons, rune cards, and result panel visible.

Final UI polish should move from IMGUI to a proper Canvas later. The v1.0 release-track prototype keeps IMGUI to avoid adding packages or destabilizing the battle loop.
