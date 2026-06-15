# RuneGate Defense v1.0 Asset List

## Heroes

- `hero_knight_001` - Knight - Tank - Front - Light
- `hero_archer_001` - Archer - Ranged DPS - Back - Wind
- `hero_mage_fire_001` - Fire Mage - Mage - Back - Fire
- `hero_priest_001` - Priest - Healer - Middle - Light
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
- `rune_fire`
- `rune_shield`
- `rune_healing`
- `rune_command`
- `rune_focus`
- `rune_explosion`
- `rune_haste`
- `rune_frost`
- `rune_lightning`
- `rune_earth`
- `rune_sacrifice`
- `rune_guardian`
- `rune_mana`
- `rune_hunter`
- `rune_purification`
- `rune_shatter`
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
- Lane 1 Middle: `hero_priest_001`
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

## Initial Integrated Images

These ChatGPT-generated prototype images are imported as Unity Sprites when present. Missing images keep the placeholder fallback path.

- `Assets/_Project/Art/Characters/Heroes/Knight/용감한_기사와_검과_방패.png` -> Knight `portrait` and `battleSprite`
- `Assets/_Project/Art/Characters/Heroes/Archer/엘프_궁수의_조준_자세.png` -> Archer `portrait` and `battleSprite`
- `Assets/_Project/Art/Characters/Monsters/Goblin/공격적인_포즈의_고블린_캐릭터.png` -> Goblin `sprite`
- `Assets/_Project/Art/Characters/Monsters/Orc/결전_자세의_오크_전사.png` -> Orc `sprite`
- `Assets/_Project/Art/Characters/Bosses/OrcWarlord/강렬한_전사_오크_보스.png` -> Orc Warlord `sprite`
- `Assets/_Project/Art/UI/Icons/Skills/영웅의_방패_충격.png` -> Shield Bash `icon`
- `Assets/_Project/Art/UI/Icons/Skills/궁전의_화살_발사_아이콘.png` -> Rapid Shot `icon`
- `Assets/_Project/Art/UI/Icons/Runes/마법의_검과_룬_에뮬럼.png` -> Sword Rune `icon`
- `Assets/_Project/Art/Backgrounds/마법의_푸른_크리스탈과_포탈.png` -> background reference asset
- `Assets/_Project/Art/ConceptSheets/게임_ui_콘셉트_아트_시트.png` -> reference-only concept sheet

Use `Tools/RuneGate/Apply Initial Art Images` to relink existing data without rebuilding scenes, or `Tools/RuneGate/Bootstrap v1.0 Release Track` to rebuild content and relink images together.

## v1.0 Release Track

The release-track bootstrap should create:

- 6 hero assets
- 6 monster assets
- 1 boss asset
- 20 rune assets
- 10 stage assets
- 4 upgrade assets
- Knight/Goblin visual prototype assets

Stage 10 should include an Orc Warlord boss wave.
