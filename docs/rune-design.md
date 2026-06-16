# Rune Design

v0.7 룬은 웨이브 사이 선택지로 등장하는 전술 기록서 조각이다. 최소 10개 이상의 룬은 실제 런타임 효과를 가지며, 나머지는 compile-safe placeholder hook으로 남긴다.

| 번호 | 룬 | effectKey | 값 | 상태 |
| ---: | --- | --- | ---: | --- |
| 1 | 공격 룬 | `hero_attack_percent` | 0.14 | 실제 적용 |
| 2 | 방어 룬 | `tank_defense_percent` | 0.18 | 실제 적용 |
| 3 | 생명 룬 | `hero_max_hp_percent` | 0.16 | 실제 적용 |
| 4 | 마법 룬 | `mage_area_percent` | 0.16 | 단순화 적용 |
| 5 | 속도 룬 | `hero_attack_speed_percent` | 0.12 | 실제 적용 |
| 6 | 치유 룬 | `crystal_heal_flat` | 45 | 실제 적용 |
| 7 | 화염 룬 | `mage_area_percent` | 0.18 | 단순화 적용 |
| 8 | 냉기 룬 | `monster_slow_percent` | 0.20 | 실제 적용 |
| 9 | 번개 룬 | `lightning_placeholder` | 1.0 | placeholder |
| 10 | 대지 룬 | `hero_max_hp_percent` | 0.12 | 실제 적용 |
| 11 | 그림자 룬 | `hero_attack_percent` | 0.20 | 실제 적용 |
| 12 | 사냥 룬 | `boss_damage_percent` | 0.18 | 실제 적용 |
| 13 | 집중 룬 | `skill_cooldown_percent` | 0.10 | 실제 적용 |
| 14 | 폭발 룬 | `blast_placeholder` | 1.0 | placeholder |
| 15 | 수호 룬 | `crystal_shield_flat` | 25 | placeholder |
| 16 | 정화 룬 | `purify_placeholder` | 1.0 | placeholder |
| 17 | 분쇄 룬 | `crush_placeholder` | 1.0 | placeholder |
| 18 | 연쇄 룬 | `ranged_chain_shot_placeholder` | 0.14 | placeholder |
| 19 | 포탑 룬 | `turret_attack_percent` | 0.18 | 실제 적용 |
| 20 | 보스 사냥 룬 | `boss_damage_percent` | 0.32 | 실제 적용 |

## 적용 정책

- 룬은 ScriptableObject 원본을 런타임 중 직접 변경하지 않는다.
- 선택된 룬 효과는 `RuneEffectApplier`를 통해 현재 전투의 런타임 컨트롤러에 적용한다.
- `monster_slow_percent`와 기존 `enemy_slow_percent`는 호환 처리한다.
- `hero_max_hp_percent`와 기존 `hero_hp_percent`는 호환 처리한다.
- `tank_defense_percent`는 v0.7에서 전열 방어 역할의 최대 HP 보너스로 단순화했다.

## 다음 단계

- 번개, 폭발, 수호, 정화, 분쇄, 연쇄 룬은 전용 전투 시스템이 준비되면 실제 효과로 확장한다.
- rarity별 등장 확률과 3선택 UI 가중치는 플레이테스트 이후 조정한다.
