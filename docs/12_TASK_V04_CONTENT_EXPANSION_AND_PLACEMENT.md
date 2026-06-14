# Task v0.4 — Content Expansion, Placement Foundation, and First Art Pipeline Sample

## 0. 목적

`v0.3 Progression Loop`가 완료되면 게임은 아래 구조를 갖는다.

```text
Title
→ Stage Select
→ Battle
→ Result
→ Upgrade
→ Save
```

`v0.4`의 목표는 전투 콘텐츠를 늘리고, 나중에 캐릭터를 다양하게 확장할 수 있는 구조를 만드는 것이다.

핵심:

```text
영웅 6종
몬스터 6종
보스 1종
룬 카드 20장
스테이지 10개
배치 슬롯 구조
첫 아트 샘플 적용 준비
```

아직 최종 일러스트 대량 적용은 하지 않는다.  
단, Knight/Goblin 정도의 샘플 이미지를 넣을 수 있는 파이프라인을 준비한다.

## 1. 현재 상태 가정

v0.4는 v0.3 완료 후 시작한다.

v0.3 완료 상태:

- TitleScene 존재
- StageSelectScene 존재
- BattleScene 존재
- UpgradeScene 존재
- Local JSON save
- Stage unlocks
- Upgrade purchase
- Battle result flow
- Stage 1~3 존재
- Knight / Archer
- Goblin / Orc
- 3개 룬 효과

## 2. 브랜치 전략

작업 전:

```powershell
cd C:\workspace\defense-game

git switch main
git pull
git switch -c codex/content-v04
```

작업 후:

```powershell
git status
git add .
git commit -m "feat(content): expand heroes monsters runes and stages"
git push -u origin codex/content-v04
```

## 3. v0.4 목표

완료 시점의 게임은 다음을 만족해야 한다.

```text
- 6개 영웅 데이터가 존재한다.
- 6개 몬스터 데이터가 존재한다.
- 1개 보스 데이터가 존재한다.
- 20개 룬 카드 데이터가 존재한다.
- 10개 스테이지 데이터가 존재한다.
- StageSelectScene에서 Stage 1~10 표시가 가능하다.
- Stage unlock 흐름이 1→10까지 확장된다.
- BattleScene이 다양한 wave 구성을 읽고 실행한다.
- 3라인 x 3슬롯 배치 구조가 코드상 존재한다.
- 아직 드래그앤드롭은 구현하지 않는다.
- Knight/Goblin 아트 샘플을 적용할 준비가 되어 있다.
```

## 4. 영웅 콘텐츠 확장

### 4.1 영웅 6종

생성할 HeroData:

| id | 이름 | 역할 | 위치 | 속성 | 설명 |
|---|---|---|---|---|---|
| `hero_knight_001` | Kingdom Knight | Tank | Front | Light | 앞라인에서 적을 막는 기본 탱커 |
| `hero_archer_001` | Elf Archer | RangedDps | Back | Wind | 빠른 단일 원거리 공격 |
| `hero_mage_fire_001` | Fire Mage | Mage | Back | Fire | 광역 마법 피해 |
| `hero_cleric_001` | Cleric | Healer | Middle | Light | 아군 회복 |
| `hero_engineer_001` | Dwarf Engineer | Engineer | Middle | Earth | 임시 포탑/장벽 계열 |
| `hero_assassin_001` | Shadow Assassin | Assassin | Front | Dark | 강한 적 우선 폭딜 |

### 4.2 권장 기본 스탯

| 영웅 | HP | ATK | SPD | Range | Skill |
|---|---:|---:|---:|---:|---|
| Knight | 420 | 24 | 1.00 | 1.30 | Shield Bash |
| Archer | 190 | 30 | 1.35 | 4.50 | Rapid Shot |
| Fire Mage | 160 | 48 | 0.70 | 3.60 | Flame Burst |
| Cleric | 180 | 10 | 0.90 | 3.20 | Holy Heal |
| Engineer | 240 | 18 | 0.90 | 2.80 | Deploy Turret |
| Assassin | 210 | 64 | 0.80 | 1.50 | Shadow Strike |

### 4.3 영웅별 스킬

#### Knight — Shield Bash

```text
대상: 가장 가까운 적
효과: 피해 + 짧은 기절 또는 넉백
```

#### Archer — Rapid Shot

```text
대상: 가장 가까운 적 또는 HP 높은 적
효과: 빠른 다단히트
```

#### Fire Mage — Flame Burst

```text
대상: 범위
효과: 광역 피해 + 화상 placeholder
```

#### Cleric — Holy Heal

```text
대상: 체력 낮은 아군 또는 Crystal
효과: 회복
```

#### Engineer — Deploy Turret

```text
대상: 현재 라인
효과: 일정 시간 자동 공격하는 placeholder 포탑 생성
```

#### Assassin — Shadow Strike

```text
대상: 보스 우선, 없으면 HP 높은 적
효과: 강한 단일 피해
```

MVP에서는 모든 스킬을 완전 고급 구현할 필요 없다.  
다만 버튼, 쿨타임, 기본 효과는 동작해야 한다.

## 5. 몬스터 콘텐츠 확장

### 5.1 몬스터 6종 + 보스 1종

| id | 이름 | 타입 | 속성 | 특징 |
|---|---|---|---|---|
| `monster_goblin_001` | Goblin | Normal | None | 기본 적 |
| `monster_wolf_001` | Wolf | Fast | Wind | 빠른 적 |
| `monster_orc_001` | Orc | Tank | Earth | 체력 높은 적 |
| `monster_bat_001` | Bat | Flying | Dark | 빠른 침투형 |
| `monster_slime_001` | Slime | Splitter | Poison | 죽을 때 소형 슬라임 placeholder |
| `monster_skeleton_001` | Skeleton | Undead | Dark | 부활 placeholder |
| `boss_orc_warlord_001` | Orc Warlord | Boss | Earth | 고체력, 소환 placeholder |

### 5.2 권장 기본 스탯

| 몬스터 | HP | Speed | Crystal Damage | Gold |
|---|---:|---:|---:|---:|
| Goblin | 80 | 1.20 | 1 | 5 |
| Wolf | 60 | 1.80 | 1 | 6 |
| Orc | 220 | 0.75 | 2 | 12 |
| Bat | 50 | 2.00 | 1 | 8 |
| Slime | 120 | 0.85 | 1 | 10 |
| Skeleton | 100 | 1.00 | 1 | 9 |
| Orc Warlord | 1200 | 0.45 | 5 | 100 |

### 5.3 특수 능력 구현 수준

v0.4에서는 모든 특수 능력을 완성하지 않는다.  
다만 hook과 placeholder 로그를 준비한다.

| 타입 | v0.4 구현 |
|---|---|
| Fast | moveSpeed 높게 적용 |
| Tank | HP 높게 적용 |
| Flying | 별도 방어 무시 로직은 보류, 태그만 적용 |
| Splitter | 죽을 때 작은 슬라임 스폰 placeholder 가능 |
| Undead | 1회 부활 확률 placeholder 가능 |
| Boss | Victory 조건과 보스 wave에서 동작 |

가능하면 Slime split과 Skeleton revive는 단순 구현한다.  
복잡해지면 v0.5로 미룬다.

## 6. 룬 카드 20장

### 6.1 생성할 RuneData

| id | 이름 | effectKey | 값 | 설명 |
|---|---|---|---:|---|
| `rune_sword_001` | Sword Rune | hero_attack_percent | 0.20 | 모든 영웅 공격력 +20% |
| `rune_bow_001` | Bow Rune | hero_attack_speed_percent | 0.15 | 공격속도 +15% |
| `rune_heal_001` | Healing Rune | crystal_heal_flat | 30 | Crystal HP +30 |
| `rune_guard_001` | Guard Rune | crystal_shield_flat | 25 | Crystal shield placeholder |
| `rune_focus_001` | Focus Rune | single_damage_percent | 0.25 | 단일 공격 피해 증가 |
| `rune_blast_001` | Blast Rune | area_damage_percent | 0.20 | 광역 피해 증가 |
| `rune_fire_001` | Fire Rune | burn_damage_percent | 0.15 | 화상 피해 placeholder |
| `rune_ice_001` | Ice Rune | monster_slow_percent | 0.10 | 적 이동속도 감소 |
| `rune_lightning_001` | Lightning Rune | periodic_lightning_flat | 30 | 주기적 번개 placeholder |
| `rune_earth_001` | Earth Rune | tank_hp_percent | 0.25 | 탱커 HP 증가 |
| `rune_mana_001` | Mana Rune | skill_cooldown_percent | 0.10 | 스킬 쿨타임 감소 |
| `rune_hunter_001` | Hunter Rune | boss_damage_percent | 0.20 | 보스 피해 증가 |
| `rune_purify_001` | Purify Rune | undead_revive_reduce_percent | 0.50 | 언데드 부활 억제 placeholder |
| `rune_crush_001` | Crush Rune | tank_monster_damage_percent | 0.20 | 탱커형 몬스터 피해 증가 |
| `rune_chain_001` | Chain Rune | projectile_chain_count | 1 | 투사체 연쇄 placeholder |
| `rune_turret_001` | Turret Rune | engineer_summon_attack_percent | 0.30 | 기술자 소환물 공격력 증가 |
| `rune_cleric_001` | Cleric Rune | healing_percent | 0.25 | 회복량 증가 |
| `rune_assassin_001` | Assassin Rune | high_hp_target_damage_percent | 0.25 | HP 높은 대상 피해 증가 |
| `rune_risk_001` | Risk Rune | sacrifice_crystal_for_attack | 0.30 | Crystal HP 감소, 공격력 증가 |
| `rune_command_001` | Command Rune | all_hero_all_stat_percent | 0.08 | 전체 영웅 능력 소폭 증가 |

### 6.2 v0.4에서 실제 구현할 효과

필수 구현:

```text
hero_attack_percent
hero_attack_speed_percent
crystal_heal_flat
skill_cooldown_percent
monster_slow_percent
boss_damage_percent
healing_percent
```

나머지는 placeholder로 받아도 된다.

규칙:

- 미구현 effectKey를 선택해도 에러가 나면 안 된다.
- 미구현 효과는 `Debug.LogWarning` 또는 `Debug.Log`로 안내한다.
- 미구현 룬은 선택 가능하되 영향이 없을 수 있음을 문서화한다.

## 7. 스테이지 10개

### 7.1 월드 1: Goblin Forest

생성 StageData:

```text
stage_goblin_forest_01
stage_goblin_forest_02
stage_goblin_forest_03
stage_goblin_forest_04
stage_goblin_forest_05
stage_goblin_forest_06
stage_goblin_forest_07
stage_goblin_forest_08
stage_goblin_forest_09
stage_goblin_forest_10
```

### 7.2 스테이지 구조

| Stage | 테마 | Waves | 주요 적 |
|---|---|---:|---|
| 1 | Tutorial | 2 | Goblin |
| 2 | First Pressure | 3 | Goblin, Orc |
| 3 | Fast Threat | 3 | Goblin, Wolf |
| 4 | Air Warning | 3 | Bat |
| 5 | Tank Wall | 4 | Orc |
| 6 | Swarm | 4 | Goblin, Slime |
| 7 | Undead Signs | 4 | Skeleton |
| 8 | Mixed Lanes | 4 | Goblin, Wolf, Orc |
| 9 | Pre-Boss | 5 | Mixed |
| 10 | Boss Gate | 5 | Orc Warlord |

### 7.3 Stage unlock

v0.3의 1→3 해금 흐름을 1→10으로 확장한다.

```text
clear stage 1 unlocks stage 2
clear stage 2 unlocks stage 3
...
clear stage 9 unlocks stage 10
```

## 8. Hero Placement Foundation

### 8.1 목표

아직 드래그앤드롭 배치는 구현하지 않는다.  
하지만 나중에 배치를 넣을 수 있게 구조만 만든다.

### 8.2 생성 파일

```text
Assets/_Project/Scripts/Hero/HeroPlacementSlot.cs
Assets/_Project/Scripts/Hero/HeroPlacementManager.cs
Assets/_Project/Scripts/Data/HeroRosterData.cs
```

필요하면:

```text
Assets/_Project/Scripts/Hero/HeroRuntimeLoadout.cs
```

### 8.3 슬롯 구조

전투 필드는 3라인 x 3슬롯.

```text
Lane 0:
  Front
  Middle
  Back

Lane 1:
  Front
  Middle
  Back

Lane 2:
  Front
  Middle
  Back
```

데이터:

```text
int laneIndex
HeroPositionType positionType
Vector3 worldPosition
bool isOccupied
HeroController currentHero
```

### 8.4 v0.4 동작

v0.4에서는 자동 배치만 한다.

기본 배치:

```text
Knight: lane 1 / Front
Archer: lane 1 / Back
Mage: lane 0 / Back
Cleric: lane 1 / Middle
Engineer: lane 2 / Middle
Assassin: lane 2 / Front
```

단, 전투 밸런스가 너무 강해질 수 있으므로 실제 출전 영웅은 아래 정책 중 하나를 선택한다.

#### 추천 정책 A: 기본 출전 2명, 해금 구조만 준비

```text
Stage 1~3: Knight, Archer
Stage 4~6: Mage 추가
Stage 7~10: Cleric 추가
Engineer/Assassin은 데이터만 준비
```

#### 정책 B: v0.4에서 6명 모두 출전

테스트는 쉽지만 난이도 밸런스가 무너질 수 있다.

추천은 **정책 A**다.

## 9. First Art Pipeline Sample

### 9.1 목표

최종 일러스트를 대량 적용하지 않는다.  
대신 다음을 준비한다.

```text
- Art folder structure
- file naming convention
- sprite import guideline
- HeroData/MonsterData에 sprite 교체 확인
- Knight/Goblin 샘플 sprite를 나중에 꽂을 수 있는 mapping
```

### 9.2 폴더 구조

```text
Assets/_Project/Art/
  Characters/
    Heroes/
      Knight/
      Archer/
      Mage/
      Cleric/
      Engineer/
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
  Backgrounds/
```

### 9.3 파일명 규칙

```text
hero_knight_idle.png
hero_knight_attack.png
hero_knight_hit.png
hero_knight_skill.png
hero_knight_death.png

monster_goblin_walk.png
monster_goblin_hit.png
monster_goblin_death.png

icon_skill_shield_bash.png
icon_skill_rapid_shot.png
icon_rune_sword.png
icon_rune_bow.png
icon_rune_healing.png
```

### 9.4 문서 생성

생성 또는 갱신:

```text
docs/07_ART_AUDIO_PIPELINE.md
docs/ASSET_LIST_V04.md
```

`ASSET_LIST_V04.md`에는 최소 다음 목록을 작성한다.

- 6 heroes
- 6 monsters
- 1 boss
- 20 rune icons
- 6 skill icons
- battle background
- title background
- stage select background
- UI panel skin
- button skin
- hit VFX
- heal VFX
- fire VFX

### 9.5 Unity Sprite Import Settings 가이드

문서에 작성:

```text
Texture Type: Sprite (2D and UI)
Sprite Mode: Multiple for sprite sheets, Single for icon/single image
Pixels Per Unit: 100
Compression: Normal Quality for MVP, adjust later
Filter Mode: Bilinear for illustration, Point for pixel art only
Max Size: 2048 for characters, 1024 for icons unless needed
```

## 10. UI/UX 확장

### 10.1 StageSelectScene

Stage 1~10 표시.

표시 정보:

- Stage number
- Stage name
- locked/unlocked
- cleared/not cleared

### 10.2 Hero roster placeholder

v0.4에서는 전투 시작 전 영웅 선택 UI까지는 보류한다.  
단, StageSelect 또는 Battle HUD에 현재 출전 영웅 목록을 표시할 수 있으면 좋다.

### 10.3 Rune card UI

20개 룬으로 확장되므로 다음이 필요하다.

- 이름
- 설명
- rarity
- effect description
- placeholder icon

## 11. 밸런스 원칙

v0.4는 재미 완성이 아니라 콘텐츠 구조 검증이다.

우선순위:

```text
1. 모든 데이터가 정상 로드된다.
2. 스테이지 1~10이 실행된다.
3. BattleScene이 다양한 wave를 처리한다.
4. 룬 20개 중 미구현 effect가 있어도 깨지지 않는다.
5. Stage 10에서 보스가 등장한다.
```

정밀 밸런스는 v0.5 이후.

## 12. Bootstrapper 확장

### 12.1 메뉴

추가:

```text
Tools/RuneGate/Bootstrap Content v0.4
```

기존 메뉴 유지:

```text
Tools/RuneGate/Bootstrap Playable Prototype
Tools/RuneGate/Bootstrap Progression Prototype
Tools/RuneGate/Validate Project
```

### 12.2 동작

- 영웅 6종 생성
- 몬스터 6종 + 보스 생성
- 스킬 6종 생성
- 룬 20종 생성
- 스테이지 10개 생성
- Art 폴더 구조 생성
- docs/ASSET_LIST_V04.md 생성 또는 갱신
- 기존 asset은 가급적 덮어쓰지 않음
- 누락된 asset만 생성

## 13. Validator 확장

검사 항목 추가:

- HeroData 6개 존재
- MonsterData 6개 존재
- Boss MonsterData 1개 존재
- SkillData 6개 존재
- RuneData 20개 존재
- StageData 10개 존재
- Art folder structure 존재
- StageSelect가 10개 스테이지를 표시할 수 있음
- Stage 10에 boss wave 존재

## 14. README 업데이트

추가할 내용:

```text
## v0.4 Content Expansion

Implemented:
- 6 hero data assets
- 6 monster data assets
- 1 boss data asset
- 20 rune cards
- 10 Goblin Forest stages
- placement slot foundation
- art pipeline folder structure
- v0.4 asset list
```

## 15. 수동 테스트 시나리오

### 15.1 콘텐츠 생성 테스트

```text
1. Unity 실행
2. Tools/RuneGate/Bootstrap Content v0.4 실행
3. Console 에러 확인
4. Data/Heroes에 6개 asset 확인
5. Data/Monsters에 7개 asset 확인
6. Data/Runes에 20개 asset 확인
7. Data/Stages에 10개 asset 확인
```

### 15.2 스테이지 진행 테스트

```text
1. Reset Save
2. Stage 1 클리어
3. Stage 2 해금 확인
4. Stage 2 클리어
5. Stage 3 해금 확인
6. 가능하면 Stage 10까지 해금 테스트
```

빠른 테스트용 dev unlock 기능이 있으면 좋다.

```text
Tools/RuneGate/Unlock All Stages For Testing
```

이 기능은 개발용임을 README에 명시한다.

### 15.3 전투 콘텐츠 테스트

```text
1. Stage 4에서 Bat 데이터가 스폰되는지 확인
2. Stage 6에서 Slime 데이터가 스폰되는지 확인
3. Stage 7에서 Skeleton 데이터가 스폰되는지 확인
4. Stage 10에서 Orc Warlord가 등장하는지 확인
5. 보스 처치 시 Victory가 뜨는지 확인
```

### 15.4 룬 카드 테스트

```text
1. Wave clear 후 룬 3개 표시
2. 동일 선택지 중복 없음
3. Sword/Bow/Healing 룬 정상 적용
4. skill_cooldown_percent 정상 적용
5. monster_slow_percent 정상 적용
6. 미구현 룬 선택 시 에러 없이 로그만 출력
```

## 16. 완료 기준

v0.4 완료 조건:

```text
- 6 HeroData 존재
- 6 MonsterData + 1 Boss 존재
- 6 SkillData 존재
- 20 RuneData 존재
- 10 StageData 존재
- StageSelect가 1~10 표시
- Stage unlock이 1~10으로 확장
- BattleScene이 Stage 1~10 실행 가능
- Boss wave 실행 가능
- Placement slot foundation 존재
- Art folder structure 존재
- ASSET_LIST_V04.md 존재
- Unity compile error 없음
- 기존 v0.3 Progression Loop가 깨지지 않음
```

## 17. Codex 실행 프롬프트

아래를 Codex에 그대로 붙여넣는다.

```text
Read RUNEGATE_MASTER_PLAN.md and docs if they exist.
Then implement Task v0.4 according to docs/12_TASK_V04_CONTENT_EXPANSION_AND_PLACEMENT.md.

Do not rely on prior chat context.

Current expected state:
v0.3 Progression Loop is implemented:
TitleScene, StageSelectScene, BattleScene, Result flow, UpgradeScene, local JSON save, stage unlocks, and permanent upgrades.

Goal:
Expand content and prepare long-term character/art scalability.

Must implement:
- 6 HeroData assets
- 6 MonsterData assets
- 1 boss MonsterData asset
- 6 SkillData assets
- 20 RuneData assets
- 10 StageData assets
- Stage unlock extension from 1 to 10
- 3 lanes x 3 hero placement slot foundation
- art folder structure
- ASSET_LIST_V04.md
- Bootstrap Content v0.4 menu
- Validator updates
- README update

Restrictions:
- Do not add ads
- Do not add IAP
- Do not add login
- Do not add server code
- Do not add analytics
- Do not add Firebase
- Do not add Addressables
- Do not add external packages
- Do not implement gacha
- Do not implement multiplayer
- Do not implement ranking
- Do not implement final art integration yet
- Do not break v0.3 loop

After implementation:
- Make the project compile.
- Run validation if possible.
- List created/modified files.
- Explain manual Unity test steps.
```
