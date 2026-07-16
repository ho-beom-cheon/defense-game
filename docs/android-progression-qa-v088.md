# Android Progression QA v0.88

## 검증 범위

- 날짜: 2026-07-12
- 이슈: `#38 Android 업그레이드 저장과 Stage 2 진행 루프`
- 브랜치: `codex/issue-38-android-progression-qa`
- AVD: Android 15 / API 35 / 1080x2400 Portrait / density 420
- 패키지: `com.hobeomcheon.runegatedefense`

## 검증 APK

- 출력: `C:\workspace\defense-game-issue38-artifacts\RuneGateDefense-upgrade-fix.apk`
- 크기: `71,892,743 bytes`
- SHA-256: `CBD20CDB2A5E3707A0E4A30232619D1E64D7E9EB732E61FF8869871CDB1F5379`
- 설치: `adb install -r` 성공

APK와 캡처는 로컬 검증 산출물이므로 Git에 포함하지 않는다.

## 진행 결과

- [x] Stage 1 Result에서 UpgradeScene 진입
- [x] UpgradeScene의 4개 업그레이드와 세로 ScrollView 표시
- [x] 크리스탈 강화 구매: Gold 330 -> 250, Level 0 -> 1
- [x] 영웅 훈련 구매: Gold 250 -> 170, Level 0 -> 1
- [x] 구매 직후 현재/다음 효과와 비용 갱신
- [x] StageSelect 복귀 후 Gold와 Stage 2 해금 표시
- [x] 앱 강제 종료 후 재실행
- [x] 재실행 후 Gold, 크리스탈 강화 Level 1, Stage 2 해금 유지
- [x] Stage 2 선택 및 BattleScene 진입
- [x] Stage 2 HUD에 `재문 숲 2`, Wave 1/3, 강화가 반영된 Crystal HP 표시
- [x] 치명적 Unity/Android 런타임 오류 없음

## 발견 및 수정

구매 버튼 처리 중 같은 IMGUI 이벤트에서 레벨, 비용, Gold, 피드백 문구가 동시에 바뀌면 ScrollView의 clip 상태가 깨졌다. 구매 처리 후 `GUIUtility.ExitGUI()`로 현재 GUI 이벤트를 종료하고 다음 프레임에 전체 레이아웃을 다시 계산하도록 수정했다.

모바일 구매 버튼과 StageSelect 복귀 버튼에는 `UIResponsiveLayout.TouchHeight`를 적용했다. 피드백 문구와 복귀 버튼이 함께 표시되도록 Upgrade footer 최소 높이도 늘렸다.

`RuneGateProgressionSmokeTest`는 지원 해상도마다 Upgrade header/main/footer가 프레임 안에 있고 서로 겹치지 않는지, 세로 화면 footer가 터치 버튼과 피드백을 담을 최소 높이인지 검사한다.

## 남은 검증

- 실제 Android 휴대폰에서 구매 버튼 터치와 ScrollView 제스처
- Stage 3~10 장시간 모바일 플레이
- 저사양 ARM 기기의 저장 I/O와 프레임 유지
- release keystore 서명 및 Play 내부 테스트
