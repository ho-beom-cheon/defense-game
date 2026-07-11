# Release Checklist

RuneGate Defense v0.9 Release Candidate 기준 체크리스트다. 체크가 비어 있는 항목은 실제 Unity/기기 수동 확인이 아직 필요하다는 뜻이다.

## Project Validation

- [x] Unity 컴파일 에러 없음
- [x] `Tools/RuneGate/Validate Project` 실행
- [ ] `Tools/RuneGate/Validate v1.0 Release Track` 실행
- [x] `TitleScene` 실행 가능
- [x] `StageSelectScene` 실행 가능
- [x] `BattleScene` 실행 가능
- [x] `UpgradeScene` 실행 가능

### Latest Validation Attempt

- Date: 2026-07-12
- Project Validator: passed in Unity batchmode
- Progression Smoke Test: passed; one compile-safe placeholder warning for shared rune effect keys
- Windows Player Stage 1 E2E: passed
- Windows Player Stage 1~10 full chapter E2E: passed
- Full chapter result: 10 victories, 10 persisted upgrade purchases, Grumbar boss spawn verified
- Windows Player system flows E2E: passed; tutorial persistence, JSON reload, Reset Save, Defeat Result verified
- Result navigation E2E: passed; next stage, retry, upgrade, and stage select transitions verified
- Cross-process save E2E: passed; independent writer/reader Players restored Gold, upgrade, tutorial, stage selection, and unlocks
- Corrupt-save E2E: passed; invalid primary isolated as `.corrupt`, default fallback and valid `.bak` restoration verified
- Interrupted-save E2E: passed; valid `.tmp` promotion and invalid `.tmp` isolation with `.bak` fallback verified
- Post-recovery full chapter regression: passed; Stage 1~10 victories, 10 upgrades, Grumbar spawn, and zero save artifacts verified
- UI Frame Validator: PASS 83, WARNING 0, FAIL 0 across 720x1280, 1080x1920, 1440x2560 and landscape fallback sizes
- Windows Player screenshot QA: six core screens passed at 720x1280; higher requests were desktop-capped to 1080x1440 and 1440x1440, with six screens passed at each size
- Screenshot review: Korean HUD, skill grid, StageSelect detail, Defeat Result, and Upgrade labels rendered without internal ids or broken Korean text
- Progression Smoke Test: passed; one compile-safe placeholder warning for shared rune effect keys

## Progression QA

- [x] 새 세이브에서 Stage 1만 해금됨
- [x] Stage 1 시작 가능
- [x] Stage 1 클리어 가능
- [x] Stage 2 해금 확인
- [x] Stage 1~10 목록 표시 확인
- [x] Stage 10에서 그룸바르 보스 등장 확인
- [x] Victory 결과창 정상 표시
- [x] Defeat 결과창 정상 표시
- [x] Result 버튼: 다음 스테이지 / 재시도 / 업그레이드 / 스테이지 선택 정상
- [x] Upgrade 구매 정상
- [x] Save/Load 정상
- [x] Reset Save 정상
- [x] 튜토리얼 표시/완료 저장 정상

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
- [x] APK 빌드 성공
- [ ] 실기기 설치 확인
- [ ] 긴 세션 플레이 확인
- [x] AAB 빌드 후보 확인
- [ ] 릴리스 키스토어로 APK/AAB 서명

### Latest Build Attempt

- Date: 2026-07-12
- Command: `RuneGate.Editor.RuneGateCurrentBuildPipeline.BuildCurrentAndroidApkFromCommandLine`
- Result: Succeeded
- Output: `Builds/Android/RuneGateDefense-current.apk`
- SHA-256: `4AA16D2C6A555D39C42E0DB2A309E9AD42407E85AAF017E8FDFBBDD5F6340899`
- Package/Version: `com.hobeomcheon.runegatedefense`, `0.9.0` (`9`)
- SDK: minimum 25, target 36
- Integrity: zipalign 및 APK Signature Scheme v2 검증 통과
- Signing: Android Debug 인증서 사용 중
- ADB: 연결된 실기기 없음
- Latest APK includes the system-flow, save recovery, Result navigation, and Stage 1~10 regression changes
- AAB candidate: `65,831,901 bytes`, SHA-256 `7081B4CA6E44F79BB8DFB97C5D74A3E0C7E1C4A5CA22061E7E609C9425435369`
- AAB bundletool validation and JAR signature verification passed; signed with Android Debug certificate
- Permanent current-content APK/AAB menu and command-line entry points verified
- Remaining: Android 실기기 설치, 터치 입력, Safe Area, 장시간 플레이 검증

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
