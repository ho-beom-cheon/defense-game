# RuneGate Defense Art Guide

## Current v0.4 Policy

v0.4 keeps placeholder sprites and primitive objects only. Final illustrations, paid assets, Addressables, Firebase, and external art packages are intentionally excluded.

The purpose of the art pipeline in this version is to reserve stable folders and data hooks so that Knight, Goblin, and later character sprites can be connected without changing battle code.

## Folder Contract

```text
Assets/_Project/Art/
  Characters/
    Heroes/
      Knight/
      Archer/
      FireMage/
      Cleric/
      DwarfEngineer/
      Assassin/
    Monsters/
      Goblin/
      Wolf/
      Orc/
      Bat/
      Slime/
      Skeleton/
    Bosses/
      OrcWarlord/
  Effects/
    Skills/
    Hit/
  UI/
    Icons/
    Buttons/
    Panels/
    Runes/
  Backgrounds/
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
```

## Unity Import

- Texture Type: Sprite (2D and UI)
- Sprite Mode: Single for one-off placeholder art, Multiple for sprite sheets
- Pixels Per Unit: 100 as the baseline until a final scale pass
- Compression: Normal Quality for mobile test builds, None for visual QA if needed
- Pivot: bottom-center for characters and monsters

## Runtime Hooks

`HeroData` keeps `portrait` and `animatorController` fields.

`MonsterData` keeps `sprite` and `animatorController` fields.

`HeroPlacementManager` and `WaveManager` currently fall back to placeholder sprites when those fields are empty. Adding a Knight or Goblin sample later should only require assigning those fields on the ScriptableObject assets.
