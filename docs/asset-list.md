# RuneGate Defense v0.5 Asset List

## Heroes

- `hero_knight_001` - Knight - Tank - Front - Light
- `hero_archer_001` - Archer - Ranged DPS - Back - Wind
- `hero_mage_fire_001` - Fire Mage - Mage - Back - Fire
- `hero_cleric_001` - Cleric - Healer - Middle - Light
- `hero_engineer_dwarf_001` - Dwarf Engineer - Engineer - Middle - Earth
- `hero_assassin_001` - Shadow Assassin - Assassin - Front - Dark

## Monsters

- `monster_goblin_001` - Goblin - Normal
- `monster_wolf_001` - Wolf - Fast
- `monster_orc_001` - Orc - Tank
- `monster_bat_001` - Bat - Flying
- `monster_slime_001` - Slime - Splitter prototype hook
- `monster_skeleton_001` - Skeleton - Undead prototype hook
- `boss_orc_warlord_001` - Orc Warlord - Boss

## Skills

- `skill_shield_bash`
- `skill_rapid_shot`
- `skill_meteor`
- `skill_holy_heal`
- `skill_build_turret`
- `skill_shadow_strike`

## Rune Cards

- `rune_sword`
- `rune_bow`
- `rune_healing`
- `rune_fire`
- `rune_guard`
- `rune_command`
- `rune_focus`
- `rune_blast`
- `rune_swiftness`
- `rune_frost`
- `rune_lightning`
- `rune_earth`
- `rune_sacrifice`
- `rune_protection`
- `rune_mana`
- `rune_hunter`
- `rune_purify`
- `rune_crush`
- `rune_chain`
- `rune_turret`

## Stages

- `stage_goblin_forest_01`
- `stage_goblin_forest_02`
- `stage_goblin_forest_03`
- `stage_goblin_forest_04`
- `stage_goblin_forest_05`
- `stage_goblin_forest_06`
- `stage_goblin_forest_07`
- `stage_goblin_forest_08`
- `stage_goblin_forest_09`
- `stage_goblin_forest_10`

## Formation

Default v0.4 formation:

- Lane 0 Front: `hero_knight_001`
- Lane 0 Back: `hero_archer_001`
- Lane 1 Middle: `hero_cleric_001`
- Lane 1 Back: `hero_mage_fire_001`
- Lane 2 Middle: `hero_engineer_dwarf_001`
- Lane 2 Front: `hero_assassin_001`

## v0.5 Art Prototype Prefabs

- `Assets/_Project/Prefabs/Heroes/Hero_Knight.prefab` - data-linked Knight placeholder combat prefab
- `Assets/_Project/Prefabs/Monsters/Monster_Goblin.prefab` - data-linked Goblin placeholder combat prefab
- `Assets/_Project/Prefabs/Projectiles/Projectile_Arrow.prefab` - package-free ranged attack placeholder
- `Assets/_Project/Prefabs/Effects/Effect_Hit_Small.prefab` - short-lived hit flash effect placeholder
- `Assets/_Project/Prefabs/Effects/Effect_Death_Small.prefab` - short-lived death effect placeholder

## v0.5 Animator Controllers

- `Assets/_Project/Art/Characters/Heroes/Knight/Animations/Knight_Prototype.controller`
- `Assets/_Project/Art/Characters/Monsters/Goblin/Animations/Goblin_Prototype.controller`

Required placeholder states:

- `Idle`
- `Move`
- `Attack`
- `Hit`
- `Death`
- `Skill`

Required parameters:

- `IsMoving` bool
- `IsDead` bool
- `Attack` trigger
- `Hit` trigger
- `Death` trigger
- `Skill` trigger

## v0.5 Audio Keys

The v0.5 runtime adds an `AudioManager` and `SfxKey` enum. Clips are optional during prototype work; missing clips are ignored safely.

- `HeroAttack`
- `MonsterHit`
- `MonsterDeath`
- `CrystalHit`
- `RuneSelect`
- `UpgradePurchase`
- `Victory`
- `Defeat`

## v0.5 Icon Hooks

These remain placeholder-ready data hooks. Real image files are not included yet.

- Shield Bash skill icon
- Sword Rune icon
- Crystal Reinforcement upgrade icon
