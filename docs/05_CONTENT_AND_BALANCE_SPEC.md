# 콘텐츠 및 밸런스 명세

## 1. 밸런스 기본 원칙

MVP 밸런스는 정교함보다 “동작 확인”과 “재미의 가능성 확인”이 우선이다.

기본 원칙:

- 1스테이지는 쉽게 클리어 가능해야 한다.
- 적이 너무 빨리 죽거나 너무 오래 버티면 안 된다.
- 룬 선택 전후 차이가 눈에 보여야 한다.
- 숫자는 추후 조정 가능해야 하므로 ScriptableObject로 관리한다.

## 2. MVP HeroData

### 2.1 Knight

| 필드 | 값 |
|---|---:|
| heroId | `hero_knight_001` |
| displayName | Knight |
| role | Tank |
| positionType | Front |
| element | Light |
| maxHp | 400 |
| attack | 20 |
| attackSpeed | 1.0 |
| attackRange | 1.5 |
| skill | Shield Bash |

설계 의도:

- 낮은 공격력
- 높은 생존력
- 초반 라인 안정화

### 2.2 Archer

| 필드 | 값 |
|---|---:|
| heroId | `hero_archer_001` |
| displayName | Archer |
| role | RangedDps |
| positionType | Back |
| element | Wind |
| maxHp | 180 |
| attack | 28 |
| attackSpeed | 1.4 |
| attackRange | 5.0 |
| skill | Rapid Shot |

설계 의도:

- 안정적인 원거리 딜러
- 룬 효과가 눈에 잘 보이는 공격속도 기반 캐릭터

## 3. MVP MonsterData

### 3.1 Goblin

| 필드 | 값 |
|---|---:|
| monsterId | `monster_goblin_001` |
| displayName | Goblin |
| monsterType | Normal |
| element | None |
| maxHp | 60 |
| moveSpeed | 1.2 |
| damageToCrystal | 5 |
| rewardGold | 2 |

### 3.2 Orc

| 필드 | 값 |
|---|---:|
| monsterId | `monster_orc_001` |
| displayName | Orc |
| monsterType | Tank |
| element | None |
| maxHp | 180 |
| moveSpeed | 0.7 |
| damageToCrystal | 15 |
| rewardGold | 8 |

## 4. MVP SkillData

### 4.1 Shield Bash

| 필드 | 값 |
|---|---:|
| skillId | `skill_shield_bash_001` |
| displayName | Shield Bash |
| cooldown | 8.0 |
| power | 60 |
| range | 2.0 |
| targetingType | Nearest |
| element | Light |

MVP 효과:

- 가장 가까운 적에게 즉시 피해
- 후속 버전에서 stun/knockback 추가

### 4.2 Rapid Shot

| 필드 | 값 |
|---|---:|
| skillId | `skill_rapid_shot_001` |
| displayName | Rapid Shot |
| cooldown | 6.0 |
| power | 25 |
| range | 5.0 |
| targetingType | Nearest |
| element | Wind |

MVP 효과:

- 가장 가까운 적에게 3회 피해 또는 power * 3 단일 피해
- 구현 단순화를 위해 v0.2에서는 즉시 총 피해 처리 가능

## 5. MVP RuneData

### 5.1 Sword Rune

| 필드 | 값 |
|---|---|
| runeId | `rune_sword_001` |
| displayName | Sword Rune |
| rarity | Common |
| effectKey | `hero_attack_percent` |
| value | 0.20 |

효과:

- 모든 영웅 공격력 +20%

### 5.2 Bow Rune

| 필드 | 값 |
|---|---|
| runeId | `rune_bow_001` |
| displayName | Bow Rune |
| rarity | Common |
| effectKey | `hero_attack_speed_percent` |
| value | 0.15 |

효과:

- 모든 영웅 공격속도 +15%

### 5.3 Healing Rune

| 필드 | 값 |
|---|---|
| runeId | `rune_healing_001` |
| displayName | Healing Rune |
| rarity | Common |
| effectKey | `crystal_heal_flat` |
| value | 30 |

효과:

- Crystal HP 30 회복

## 6. StageData: Goblin Forest 1

### 6.1 기본값

| 필드 | 값 |
|---|---:|
| stageId | `stage_goblin_forest_001` |
| displayName | Goblin Forest 1 |
| crystalHp | 180 |
| waves | 2 |

### 6.2 Wave 1

| Spawn | 몬스터 | 라인 | 수량 | 시작 딜레이 | 간격 |
|---|---|---:|---:|---:|---:|
| 1 | Goblin | 0 | 2 | 0.5 | 1.0 |
| 2 | Goblin | 1 | 3 | 1.0 | 1.0 |
| 3 | Goblin | 2 | 1 | 2.0 | 1.0 |

### 6.3 Wave 2

| Spawn | 몬스터 | 라인 | 수량 | 시작 딜레이 | 간격 |
|---|---|---:|---:|---:|---:|
| 1 | Goblin | 0 | 2 | 0.5 | 0.8 |
| 2 | Orc | 1 | 1 | 1.0 | 1.0 |
| 3 | Goblin | 2 | 2 | 1.5 | 0.8 |
| 4 | Orc | 0 | 1 | 3.0 | 1.0 |

## 7. 밸런스 체크 기준

### 7.1 너무 쉬운 경우

증상:

- Crystal HP가 거의 줄지 않음
- 룬을 고르지 않아도 클리어 가능
- Orc가 금방 죽음

조정:

- Goblin 수량 증가
- Orc HP 증가
- Crystal HP 감소
- 영웅 공격력 감소

### 7.2 너무 어려운 경우

증상:

- Wave 1에서 자주 패배
- 룬 선택 전에 Crystal HP가 대부분 소진
- Orc를 처리하지 못함

조정:

- Crystal HP 증가
- Goblin HP 감소
- Knight/Archer 공격력 증가
- 스폰 간격 증가

### 7.3 룬 효과가 안 느껴지는 경우

조정:

- Sword Rune: 20% → 30%
- Bow Rune: 15% → 25%
- Healing Rune: 30 → 50

MVP에서는 효과가 약간 과장되어도 된다. 플레이어가 “선택했다”는 감각이 중요하다.

## 8. 확장 룬 목록 v0.3 이후

| 룬 | effectKey | 값 | 설명 |
|---|---|---:|---|
| Mana Rune | `skill_cooldown_percent` | 0.15 | 스킬 쿨타임 감소 |
| Frost Rune | `monster_slow_percent` | 0.10 | 적 이동속도 감소 |
| Guardian Rune | `crystal_shield_flat` | 30 | 수정 보호막 |
| Hunter Rune | `boss_damage_percent` | 0.20 | 보스 피해 증가 |

## 9. 콘텐츠 추가 규칙

새 영웅 추가 시 반드시 정의한다.

- HeroData
- SkillData
- placeholder prefab 또는 sprite
- 역할
- 사거리
- 공격속도
- 스킬 쿨타임
- 기본 밸런스 의도

새 몬스터 추가 시 반드시 정의한다.

- MonsterData
- 타입
- HP
- 이동속도
- Crystal 피해
- 보상 골드
- 특수능력 여부

새 룬 추가 시 반드시 정의한다.

- RuneData
- effectKey
- value
- 적용 대상
- 중첩 가능 여부
