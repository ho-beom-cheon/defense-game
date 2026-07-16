# Rune Design

룬은 웨이브 사이 선택지로 등장하는 전술 기록서 조각이다. 현재 20개 룬 모두 실제 런타임 효과를 가지며 ScriptableObject 원본을 변경하지 않는 전투별 보정으로 적용된다.

| 번호 | 룬 | effectKey | 값 | 상태 |
| ---: | --- | --- | ---: | --- |
| 1 | 공격 룬 | `hero_attack_percent` | 0.14 | 실제 적용 |
| 2 | 방어 룬 | `tank_defense_percent` | 0.18 | 실제 적용 |
| 3 | 생명 룬 | `hero_max_hp_percent` | 0.16 | 실제 적용 |
| 4 | 마법 룬 | `mage_area_percent` | 0.16 | 실제 적용 |
| 5 | 속도 룬 | `hero_attack_speed_percent` | 0.12 | 실제 적용 |
| 6 | 치유 룬 | `crystal_heal_flat` | 45 | 실제 적용 |
| 7 | 화염 룬 | `mage_area_percent` | 0.18 | 실제 적용 |
| 8 | 냉기 룬 | `monster_slow_percent` | 0.20 | 실제 적용 |
| 9 | 번개 룬 | `lightning_chain_percent` | 0.35 | 3번째 공격 연쇄 번개 |
| 10 | 대지 룬 | `hero_max_hp_percent` | 0.12 | 실제 적용 |
| 11 | 그림자 룬 | `hero_attack_percent` | 0.20 | 실제 적용 |
| 12 | 사냥 룬 | `boss_damage_percent` | 0.18 | 실제 적용 |
| 13 | 집중 룬 | `skill_cooldown_percent` | 0.10 | 실제 적용 |
| 14 | 폭발 룬 | `splash_damage_percent` | 0.30 | 같은 라인 범위 피해 |
| 15 | 수호 룬 | `crystal_shield_flat` | 35 | 크리스탈 보호막 |
| 16 | 정화 룬 | `purification_percent` | 0.20 | 영웅/크리스탈 회복 |
| 17 | 분쇄 룬 | `crush_damage_percent` | 0.25 | 탱커/보스 추가 피해 |
| 18 | 연쇄 룬 | `ranged_chain_damage_percent` | 0.45 | 원거리 연쇄 피해 |
| 19 | 포탑 룬 | `turret_attack_percent` | 0.18 | 실제 적용 |
| 20 | 보스 사냥 룬 | `boss_damage_percent` | 0.32 | 실제 적용 |

## 적용 정책

- 룬은 ScriptableObject 원본을 런타임 중 직접 변경하지 않는다.
- 선택된 룬 효과는 `RuneEffectApplier`를 통해 현재 전투의 런타임 컨트롤러에 적용한다.
- `monster_slow_percent`와 기존 `enemy_slow_percent`는 호환 처리한다.
- `hero_max_hp_percent`와 기존 `hero_hp_percent`는 호환 처리한다.
- `tank_defense_percent`는 v0.7에서 전열 방어 역할의 최대 HP 보너스로 단순화했다.
- `monster_slow_percent`는 현재 몬스터뿐 아니라 이후 웨이브와 보스 지원군에도 적용된다.
- 오래된 placeholder effectKey는 이전 데이터 호환을 위해 코드에서만 수용하고 현재 RuneData에서는 사용하지 않는다.

## 다음 밸런스 단계

- rarity별 등장 확률과 3선택 UI 가중치는 플레이테스트 이후 조정한다.
- 다수 적에게 번개/폭발/연쇄가 동시에 발동할 때의 모바일 프레임 타임을 실기기에서 확인한다.
