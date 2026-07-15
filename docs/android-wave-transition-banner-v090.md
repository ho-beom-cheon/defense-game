# Android Wave Transition Banner v0.9.0

## 작업 범위

- 관련 이슈: `#79`
- 작업 브랜치: `codex/issue-79-wave-transition-banner`
- 웨이브가 조용히 바뀌던 BattleScene에 짧은 비입력형 전환 배너를 추가했다.
- 기존 BattleManager, WaveManager, RuneManager 이벤트와 전투 데이터는 변경하지 않았다.

## 표시 규칙

- 첫 웨이브: `전투 개시`
- 마지막 일반 웨이브: `최종 웨이브`
- 보스 웨이브: `봉문 경보 · 보스 출현`
- 보조 정보: 현재 웨이브/전체 웨이브, 출현 적 수, 직전에 선택한 룬 이름
- 일반 웨이브는 청록색, 최종 웨이브는 금색, 보스 웨이브는 붉은색으로 구분한다.

배너는 전장 안쪽에만 표시되며 입력을 받지 않는다. 튜토리얼 또는 일시정지 팝업이 열려 있으면 표시 시간도 함께 멈춰 팝업 종료 후 확인할 수 있다. 룬 선택과 Result 상태에서는 배너를 숨긴다.

## 정적 검증

`RuneGateProgressionSmokeTest`는 720x1280, 1080x1920, 1440x2560, 1600x900, 2048x1152에서 다음을 검사한다.

- 전환 배너가 BattleFieldFrame 안에 위치하는지
- 배너의 최소 폭과 높이
- 상단 HUD 및 하단 스킬 카드와 중첩되지 않는지
- 첫 웨이브, 최종 웨이브, 보스 웨이브 제목이 구분되는지
- 보조 정보에 웨이브 진행, 적 수, 룬 이름이 포함되는지
- 일반, 최종, 보스 강조색이 구분되는지

## Android 검증

- 검증일: 2026-07-16
- 기기: Android 15 / API 35 에뮬레이터
- 해상도: 1080 x 2400 Portrait
- APK: `RuneGateDefense-wave-transition-banner.apk`
- 파일 크기: `72,078,137 bytes`
- SHA-256: `BCB10A46FDD46D918B2162CFB62FD86267D9E191DE8CB0FFED4671CF354B5D5F`

확인 결과:

- Unity Project Validator 통과
- Unity Progression Smoke Test 통과
- Android APK 빌드 및 재설치 통과
- Stage 1 `전투 개시`, Wave 1/2, 적 10기 표시 확인
- 사냥 룬 선택 후 `최종 웨이브`, Wave 2/2, 적 12기, `사냥 룬 적용` 표시 확인
- Stage 10 Wave 5/5에서 `봉문 경보 · 보스 출현`, 적 13기 표시 확인
- 그룸바르 전용 HP HUD와 전환 배너가 서로 분리되어 표시됨
- Normal Stage 1~10 전체 승리와 강화 10회 통과
- Hard/Nightmare 해금 및 그룸바르 페이즈 회귀 통과

## 남은 한계

- 배너는 기존 IMGUI 구조와 패널 이미지를 사용한다.
- 최종 전용 웨이브 문양, 보스 경보 아이콘, 경보 음원은 아직 없다.
- 실제 Android 기기의 컷아웃, 시스템 글꼴 배율, 장시간 반복 표시 내구성은 추가 확인이 필요하다.
