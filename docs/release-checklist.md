# Release Checklist

RuneGate Defense v0.9 Release Candidate 기준 체크리스트다. 체크가 비어 있는 항목은 실제 Unity/기기 수동 확인이 아직 필요하다는 뜻이다.

## Project Validation

- [ ] Unity 컴파일 에러 없음
- [ ] `Tools/RuneGate/Validate Project` 실행
- [ ] `Tools/RuneGate/Validate v1.0 Release Track` 실행
- [ ] `TitleScene` 실행 가능
- [ ] `StageSelectScene` 실행 가능
- [ ] `BattleScene` 실행 가능
- [ ] `UpgradeScene` 실행 가능

### Latest Validation Attempt

- Date: 2026-06-16
- Command: `Unity.exe -batchmode -quit -projectPath C:\workspace\defense-game -executeMethod RuneGate.Editor.RuneGateProjectValidator.ValidateFromCommandLine`
- Result: Failed before validation start
- Reason: another Unity instance is running with this project open
- Local fallback: `git diff --check`, asset existence checks, PlayerSettings text checks, and Editor.log C# error scan were performed.

## Progression QA

- [ ] 새 세이브에서 Stage 1만 해금됨
- [ ] Stage 1 시작 가능
- [ ] Stage 1 클리어 가능
- [ ] Stage 2 해금 확인
- [ ] Stage 1~10 목록 표시 확인
- [ ] Stage 10에서 그룸바르 보스 등장 확인
- [ ] Victory 결과창 정상 표시
- [ ] Defeat 결과창 정상 표시
- [ ] Result 버튼: 재시도 / 업그레이드 / 스테이지 선택 정상
- [ ] Upgrade 구매 정상
- [ ] Save/Load 정상
- [ ] Reset Save 정상
- [ ] 튜토리얼 표시/완료 저장 정상

## Korean UI QA

- [ ] 핵심 UI에 `??` 문자 없음
- [ ] 핵심 UI에 `stage_goblin_forest` 같은 내부 id 노출 없음
- [ ] HUD 상태가 한국어로 표시됨
- [ ] Rune Selection에 `effectKey`가 기본 노출되지 않음
- [ ] 세로 화면에서 주요 버튼 텍스트가 잘리지 않음

## Android RC Settings

- [x] Product Name: `RuneGate Defense`
- [x] Package Name: `com.hobeomcheon.runegatedefense`
- [x] Version: `0.9.0`
- [x] Bundle Version Code: `9`
- [x] Default Orientation: Portrait
- [x] 앱 아이콘 후보 파일 포함
- [x] 스플래시 후보 파일 포함
- [x] Google Play feature graphic 후보 파일 포함

## Android Build

- [x] APK 빌드 시도
- [ ] APK 빌드 성공
- [ ] 실기기 설치 확인
- [ ] 긴 세션 플레이 확인
- [ ] AAB 빌드 후보 확인

### Latest Build Attempt

- Date: 2026-06-16
- Command: `Unity.exe -batchmode -quit -projectPath C:\workspace\defense-game -executeMethod RuneGate.Editor.RuneGateBootstrapper.BuildAndroidApkV09Rc`
- Result: Failed before build start
- Reason: another Unity instance is running with this project open
- Next action: Unity Editor를 닫은 뒤 같은 명령 또는 `Tools/RuneGate/Build Android APK v0.9 RC` 메뉴로 다시 시도한다.

## Policy / Monetization

- [x] 강제 광고 없음
- [x] 광고 SDK 없음
- [x] 결제 SDK 없음
- [x] 가챠 없음
- [x] 서버/로그인 없음
- [x] Firebase 없음
- [x] Addressables 없음
- [ ] 스토어 제출 전 Google Play 정책 최종 확인
- [ ] 스토어 제출 전 Data Safety 답변 최종 확인
