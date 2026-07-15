# v0.9 그룸바르 보스 페이즈

## 목적

Stage 10의 그룸바르를 단순히 체력이 높은 일반 몬스터가 아니라 체력 구간에 따라 속도, 공격, 지원군 압박이 바뀌는 첫 보스전으로 구성한다.

## 페이즈 규칙

| 페이즈 | 진입 체력 | 이동 속도 | 공격 간격 | 공격 피해 | 지원군 |
|---|---:|---:|---:|---:|---:|
| 1 | 100% | 1.00x | 1.00x | 1.00x | 없음 |
| 2 | 65% 이하 | 1.15x | 0.84x | 1.20x | 문틈 도깨비 2기 |
| 3 | 30% 이하 | 1.32x | 0.66x | 1.45x | 문틈 도깨비 3기 |

높은 단일 피해로 페이즈를 건너뛰지 않도록 페이즈 1과 2에서는 다음 임계 체력에서 피해를 한 번 제한한다. 다음 공격부터 새 페이즈 규칙이 적용되며 페이즈 3에서는 정상적으로 사망할 수 있다.

## 지원군 소환

- 현재 보스 웨이브에서 일반형 비보스 MonsterData를 우선 선택한다.
- Stage 10에서는 문틈 도깨비가 선택된다.
- 보스 라인을 기준으로 다음 라인부터 순환해 소환한다.
- 기본 Stage 10 결과는 라인 `2, 0`과 `2, 0, 1`에 총 5기가 등장한다.
- 지원군은 `WaveManager`의 pending spawn과 alive monster 목록에 포함된다.
- 보스가 먼저 쓰러져도 남은 지원군이 제거되기 전에는 웨이브가 완료되지 않는다.

## 런타임 구조

- `BossPhaseController`: HP 임계값, 페이즈 배율, phase gate, 지원군 요청
- `MonsterController`: 보스 이동/공격 배율 적용과 페이즈 전환 피드백
- `WaveManager`: 지원군 MonsterData 선택, 라인 순환 스폰, 웨이브 완료 보호
- `BattleHUD`: 전장 상단에 한국어 보스 이름, HP, 페이즈, 체력 바 표시
- `RuneGateRuntimeSmokeRunner`: 페이즈 2·3, 지원군 5기, 보스 HUD, Victory 검증
- `BossAttackPatternController`: 페이즈별 대상 선택, 경고 표시, 영웅/크리스탈 광역 피해

## Android 검증

- Unity: 6000.4.11f1
- 기기: Android 15 API 35 에뮬레이터
- 해상도: 1080x2400 Portrait
- Project Validator: 통과
- Progression Smoke Test: 통과
- Stage 1~10 Full Chapter E2E: 통과
- Stage 10: 그룸바르 스폰, 페이즈 2·3, 지원군 5기, Victory 확인
- Stage 10: 1·2·3페이즈 전용 공격과 실제 영웅/크리스탈 피해 확인
- System Flow E2E: 편성 저장, 재시도, Upgrade, StageSelect 회귀 통과
- APK: `C:\workspace\defense-game-issue48-artifacts\RuneGateDefense-issue48-grumbar-boss-phases.apk`
- APK SHA-256: `AE38CA00A9C3775D4DF199FBDAA0D8C5DBCB13097969BF9A9F9820A0EB074032`

## 남은 범위

- 보스 전용 공격 로직과 경고 표시는 구현됐으며 최종 애니메이션과 고유 사운드는 에셋 작업이 필요하다.
- 지원군 종류와 패턴은 현재 한 종류로 제한한다.
- 광역 공격 피해량과 주기는 실기기 장시간 밸런스 작업이 필요하다.
