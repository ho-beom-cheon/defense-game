# v0.90 룬 20장 전투 효과 완성

## 목표

룬 선택이 단순 능력치 증가나 placeholder 로그로 끝나지 않고 다음 웨이브의 전투 양상을 실제로 바꾸도록 정리했다. 모든 `RuneData`는 구현된 effectKey를 사용하며 ScriptableObject 원본은 런타임 중 변경하지 않는다.

## 새 전투 효과

| 룬 | effectKey | 실제 효과 |
| --- | --- | --- |
| 번개 룬 | `lightning_chain_percent` | 세 번째 기본 공격마다 주변 적 최대 2명에게 35% 연쇄 피해 |
| 폭발 룬 | `splash_damage_percent` | 기본 공격 대상 주변의 같은 라인 적에게 30% 범위 피해 |
| 수호 룬 | `crystal_shield_flat` | 크리스탈에 35 보호막을 부여하고 HP보다 먼저 피해 흡수 |
| 정화 룬 | `purification_percent` | 모든 영웅과 크리스탈을 최대 HP의 20% 회복하고 영웅 회복 효율 보강 |
| 분쇄 룬 | `crush_damage_percent` | 탱커형 몬스터와 보스에게 주는 피해 25% 증가 |
| 연쇄 룬 | `ranged_chain_damage_percent` | 원거리 영웅 기본 공격이 같은 라인의 다른 적에게 45% 연쇄 피해 |

냉기 룬의 `monster_slow_percent`는 선택 시 활성 몬스터에만 적용하던 문제를 수정했다. `RuneEffectApplier`가 선택된 감속을 전투 동안 누적하고 `WaveManager.MonsterSpawned`에 연결해 다음 웨이브와 보스 지원군에도 적용한다.

## 런타임 구조

- `HeroRuneCombatModifiers`가 영웅별 번개, 범위, 연쇄, 분쇄 수치를 별도 런타임 상태로 보관한다.
- `HeroController`는 기본 공격 Impact 시점에만 보조 피해를 발생시킨다.
- `ProjectileController`는 원거리 투사체가 실제로 도착한 뒤 Impact 콜백을 호출한다.
- `CrystalController`는 HP와 분리된 `ShieldHp`를 관리한다.
- `BattleHUD`는 보호막이 있을 때 `크리스탈 HP ... 보호막 ...` 형식으로 표시한다.
- 기존 placeholder effectKey는 오래된 데이터 호환용으로만 코드에서 수용하며 현재 20개 데이터 에셋은 사용하지 않는다.

## 밸런스 규칙

- 보조 피해는 다른 보조 피해를 다시 발생시키지 않는다.
- 폭발 피해는 같은 라인, 반경 1.75 world units 안의 적에게만 적용한다.
- 연쇄 룬은 원거리 영웅만 발동한다.
- 번개 룬은 기본 공격 Impact 3회마다 발동하며 반경 3.5 안에서 아직 맞지 않은 적을 고른다.
- 감속 중첩은 곱연산으로 누적하고 최대 80%에서 제한한다.
- 보호막은 크리스탈 최대 HP를 초과해 누적되지 않는다.

## 실제 검증

- Unity 6 `Validate Project`: 통과
- Unity 6 `Progression Smoke Test`: 통과
- Android APK 빌드: 통과
- Android 15 / API 35 / 1080x2400 에뮬레이터 설치: 통과
- Android System Flow E2E: 새 룬 7종 적용, 정화 회복, 수호 보호막/한국어 HUD, 신규 스폰 감속 확인
- Android Full Chapter E2E: Stage 1~10 승리, 업그레이드 10회, 그룸바르 3페이즈와 지원군 5마리 확인

APK:

```text
C:\workspace\defense-game-issue50-artifacts\RuneGateDefense-issue50-complete-runes.apk
SHA-256: 0C607C50BA18AD98ACF692B94913F54CBE2288B67C20CFBEF958A9406F25F9EE
```

## 남은 범위

- 번개와 연쇄의 전용 궤적 애니메이션은 현재 hit spark 기반이다.
- 룬 조합별 장시간 밸런스와 희귀도 등장 확률은 추가 플레이테스트가 필요하다.
- 실기기에서 다수 보조 피해가 동시에 발생할 때의 프레임 타임은 아직 측정하지 않았다.
