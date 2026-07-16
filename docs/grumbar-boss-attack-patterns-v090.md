# v0.90 그룸바르 보스 공격 패턴

## 목표

Stage 10의 그룸바르가 이동·근접 공격·지원군 소환뿐 아니라 페이즈별로 구분되는 전용 공격을 사용하게 한다. 모든 패턴은 피해 전에 목표 위치에 경고 표시를 제공하며 기존 3페이즈와 지원군 5기 흐름을 유지한다.

## 패턴 규칙

| 페이즈 | 패턴 대상 | 기본 영웅 피해 | 크리스탈 피해 | 기본 주기 |
|---|---|---:|---:|---:|
| 1 | 보스와 같은 라인의 살아 있는 영웅 전원 | 4 | 0 | 7.0초 |
| 2 | 각 라인에서 보스에 가장 가까운 최전방 영웅 1명 | 6 | 0 | 5.5초 |
| 3 | 살아 있는 영웅 전원과 크리스탈 | 8 | 4 | 4.25초 |

- 피해량은 현재 난이도의 몬스터 피해 배율을 사용한다.
- 최초 패턴은 보스 등장 2.5초 후 준비한다.
- 경고 표시 후 0.6초 뒤 피해를 적용한다.
- 페이즈가 바뀌면 다음 패턴을 빠르게 예약한다.
- 경고 중 더 높은 페이즈로 전환되면 오래된 패턴을 취소해 이전 페이즈 공격이 뒤늦게 발동하지 않게 한다.
- 보스 사망, Battle 종료, Scene 전환 시 진행 중 패턴은 안전하게 중단된다.

## 런타임 구조

- `BossAttackPatternController`: 주기, 대상 선택, 텔레그래프, 피해, 진단 카운터
- `BossPhaseController`: HP 임계값과 페이즈 전환 이벤트 제공
- `MonsterController`: 보스에만 패턴 컨트롤러를 생성하고 Crystal/Boss 참조 연결
- `CombatVisualEffectFactory`: 페이즈별 경고 색상과 충격 이펙트 생성
- `RuneGateRuntimeSmokeRunner`: 실제 패턴 횟수, 피격 영웅 수, 크리스탈 피해 기록

## Android 검증

- Unity 6000.4.11f1 Project Validator: 통과
- Unity Progression Smoke: 통과
- Android 15 / API 35 / 1080x2400 Full Chapter: 통과
- Phase 1 패턴: 같은 라인 영웅 2명 피격
- Phase 2 패턴: 세 라인의 최전방 영웅 3명 피격
- Phase 3 패턴: 영웅 6명과 크리스탈 4 피해
- 기존 페이즈 2·3, 지원군 5기, Boss HUD, Stage 10 Victory 유지
- 치명적 예외, `NullReferenceException`, `MissingReferenceException`: 없음
- APK: `RuneGateDefense-issue56-boss-patterns-final.apk`
- APK 크기: `72,031,099 bytes`
- SHA-256: `807D963ADAE14B57F617CF65808DCEDE36D13057F66273C2C3A6B6CD50A7E8FB`

## 남은 범위

- 현재 경고와 충격은 RuntimePixel/색상 폴백 기반이며 최종 보스 전용 애니메이션은 별도 제작이 필요하다.
- 패턴별 고유 사운드와 화면 흔들림 강도는 실기기에서 최종 믹싱해야 한다.
- 추가 지원군 종류나 랜덤 패턴은 현재 Chapter 1 범위에 포함하지 않는다.
