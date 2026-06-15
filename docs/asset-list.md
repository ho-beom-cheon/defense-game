# RuneGate Defense v1.0 Asset List

## Heroes

- `hero_knight_001` - 레온 / 균열 방패의 기사 - Tank - Front - Light
- `hero_archer_001` - 세리아 / 바람길을 읽는 궁수 - Ranged DPS - Back - Wind
- `hero_mage_fire_001` - 카엘 / 잿불의 방랑 마법사 - Mage - Back - Fire
- `hero_priest_001` - 미레아 / 금빛 성서의 사제 - Healer - Middle - Light
- `hero_engineer_dwarf_001` - 브롬 / 룬포지 기술자 - Engineer - Middle - Earth
- `hero_assassin_001` - 닉스 / 그림자 균열의 암살자 - Assassin - Front - Dark

## Monsters

- `monster_goblin_001` - 문틈 도깨비 - Normal
- `monster_wolf_001` - 부식 늑대 - Fast
- `monster_orc_001` - 봉문 파쇄자 - Tank
- `monster_bat_001` - 균열 꼬마귀 - Flying
- `monster_slime_001` - 재문 점액 - Splitter prototype hook
- `monster_skeleton_001` - 균열 잔해병 - Undead prototype hook
- `boss_orc_warlord_001` - 그룸바르 / 문파괴자 - Boss

English names remain in legacy `displayName` fields for compatibility. Korean launch-facing UI and codex work should use `displayNameKorean`, `subtitleKorean`, and `descriptionKorean`.

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

The images in `Assets/_Project/Art/ConceptSheets` are concept reference assets only. They define the `문지기 기록`, `봉문 위험 기록`, and `균열 적성 기록` direction and should not be forced into BattleScene as small unit sprites.

These ChatGPT-generated prototype images are imported as Unity Sprites when present. Large concept or character images are reference/codex assets only. Missing RuntimePixel images keep the placeholder fallback path.

- `Assets/_Project/Art/ConceptSheets/Heroes/문지기_레온의_기록_카드.png` -> Knight/Leon `portrait` and `conceptImage`
- `Assets/_Project/Art/ConceptSheets/Heroes/문지기_기록_바람길을_읽는_궁수.png` -> Archer/Seria `portrait` and `conceptImage`
- `Assets/_Project/Art/ConceptSheets/Heroes/카엘_잿불의_방랑_마법사.png` -> Fire Mage/Kael `portrait` and `conceptImage`
- `Assets/_Project/Art/ConceptSheets/Heroes/미레아_금빛_성서의_사제.png` -> Priest/Mirea `portrait` and `conceptImage`
- `Assets/_Project/Art/ConceptSheets/Heroes/브롬_룬포지_기술자_카드.png` -> Dwarf Engineer/Brom `portrait` and `conceptImage`
- `Assets/_Project/Art/ConceptSheets/Heroes/그림자_균열의_암살자_카드.png` -> Shadow Assassin/Nyx `portrait` and `conceptImage`
- `Assets/_Project/Art/ConceptSheets/Enemies/균열_적성_기록_괴물_수집가.png` -> Monster `conceptImage` reference
- `Assets/_Project/Art/ConceptSheets/Enemies/문파괴자_그룸바르의_위협_기록.png` -> Orc Warlord/Grumbar `conceptImage` reference
- `Assets/_Project/Art/RuntimePixel/Heroes/*/*_idle.png` -> Hero `battleSprite`
- `Assets/_Project/Art/RuntimePixel/Monsters/GateImp/gate_imp_idle.png` -> Goblin/GateImp `runtimeSprite`
- `Assets/_Project/Art/RuntimePixel/Monsters/OrcBrute/orc_brute_idle.png` -> Orc/OrcBrute `runtimeSprite`
- `Assets/_Project/Art/RuntimePixel/Monsters/DireWolf/dire_wolf_idle.png` -> Wolf/DireWolf `runtimeSprite`
- `Assets/_Project/Art/RuntimePixel/Monsters/CaveBat/cave_bat_idle.png` -> Bat/CaveBat `runtimeSprite`
- `Assets/_Project/Art/RuntimePixel/Monsters/CoreSlime/core_slime_idle.png` -> Slime/CoreSlime `runtimeSprite`
- `Assets/_Project/Art/RuntimePixel/Monsters/BoneSoldier/bone_soldier_idle.png` -> Skeleton/BoneSoldier `runtimeSprite`
- `Assets/_Project/Art/RuntimePixel/Bosses/Grumbar/grumbar_idle.png` -> Boss/Grumbar `runtimeSprite`
- `Assets/_Project/Art/UI/Icons/Skills/영웅의_방패_충격.png` -> Shield Bash `icon`
- `Assets/_Project/Art/UI/Icons/Skills/궁전의_화살_발사_아이콘.png` -> Rapid Shot `icon`
- `Assets/_Project/Art/UI/Icons/Runes/마법의_검과_룬_에뮬럼.png` -> Sword Rune `icon`
- `Assets/_Project/Art/Backgrounds/마법의_푸른_크리스탈과_포탈.png` -> background reference asset
- `Assets/_Project/Art/ConceptSheets/게임_ui_콘셉트_아트_시트.png` -> reference-only concept sheet

Use `Tools/RuneGate/Apply Initial Art Images` to relink existing data without rebuilding scenes, or `Tools/RuneGate/Bootstrap v1.0 Release Track` to rebuild content and relink images together.

## RuntimePixel Structure

- `Assets/_Project/Art/RuntimePixel/Heroes`
- `Assets/_Project/Art/RuntimePixel/Monsters`
- `Assets/_Project/Art/RuntimePixel/Bosses`
- `Assets/_Project/Art/RuntimePixel/UI`

These folders are reserved for future small battle sprites and UI pixel elements.

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
