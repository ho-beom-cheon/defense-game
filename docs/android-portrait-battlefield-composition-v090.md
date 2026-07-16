# Android Portrait Battlefield Composition v0.9.0

## 작업 범위

- 이슈: `#71`
- 브랜치: `codex/issue-71-portrait-battlefield-composition`
- 기기: Android 15 / API 35 에뮬레이터
- 해상도: 1080x2400 Portrait
- 검증일: 2026-07-16

## 문제

기존 카메라는 세로 `BattleFieldFrame` 안에 전체 전투 경로 폭을 표시했지만, 배경과 세 라인은 가로형 기본 world height만 사용했다. 그 결과 카메라 영역 위아래에 큰 검은 여백이 남고 RuntimePixel 유닛과 전투 피드백이 중앙의 좁은 띠에 모였다.

## 구현

- Portrait의 유효 라인 간격은 카메라 world height의 25%를 기준으로 계산한다.
- 3개 라인의 최상단과 최하단 간격은 카메라 world height의 약 50%를 사용한다.
- serialized spawn, crystal target, hero slot의 x/z 값은 유지하고 y 값은 `GetLaneY` 기준으로 통일한다.
- 전장 배경은 카메라 world bounds 크기와 중심을 사용해 viewport를 채운다.
- Portrait RuntimePixel 표시 높이는 일반 영웅/몬스터 118%, 보스 110%로 보정한다.
- 몬스터 HP Bar도 같은 Portrait 표시 비율을 적용하고 기존 bounds 기반 머리 위 배치를 유지한다.
- Landscape에서는 기존 라인 간격과 표시 높이를 유지한다.

## 검증 결과

- Unity Android 빌드와 Project Validator: 통과
- Progression Smoke Test: 통과
- Android 일반 실행 Stage 1 전장 화면: 통과
- 배경 viewport 채움, 3개 라인 세로 분산, 유닛 가독성: 통과
- Rune Selection 팝업 표시 및 선택: 통과
- Normal Stage 1~10 전체 전투: 통과
- 업그레이드 구매: 10회 통과
- 그룸바르 Phase 1/2/3 공격 패턴: 통과
- 최종 결과: `RUNEGATE_FULL_CHAPTER_E2E_PASSED: upgrades=10, gold=1613, difficulties=normal|hard|nightmare`

## Android APK

- 파일: `RuneGateDefense-portrait-battlefield.apk`
- 크기: `72,065,241 bytes`
- SHA-256: `EBDE5CC635893ABC95A7529989000A7159B9CF2E993371BC9A4545BA071EF6BF`
- 빌드 결과: `RUNEGATE_ANDROID_BUILD_PASSED`

APK와 로그는 저장소 밖 QA 산출물 폴더에 보관하며 Git에 포함하지 않는다.

## 남은 한계

- 현재 배경은 가로형 RuntimePixel 이미지를 Portrait 카메라 bounds에 맞춰 늘린 프로토타입 구성이다. 최종 단계에서는 세로 전장 전용 배경 또는 타일형 상하 확장 자산이 필요하다.
- 실제 기기 노치, 시스템 글꼴 배율, 장시간 플레이 발열과 성능은 검증하지 않았다.
- RuntimePixel Idle 이미지는 정지 프레임이며 최종 Walk/Attack/Hit/Death sprite sheet가 필요하다.
