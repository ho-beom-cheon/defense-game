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
- Progression Smoke Test: passed; 20 rune records and all supported runtime effect keys validated
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
- Android emulator QA: passed at 1080x2400 Portrait on Android 15 / API 35
- Android progression QA: Upgrade purchase, process restart persistence, Stage 2 unlock/start passed
- Android full-chapter QA: Stage 1~10 Victory, 10 upgrades, and Stage 10 Grumbar spawn passed on 2026-07-15
- Android pause/lifecycle QA: manual pause, resume, restart, stage-select exit, and background auto-pause passed on 2026-07-15
- StageSelect formation editor QA: Android system flow passed on 2026-07-16; 6-hero slot swap, JSON disk reload, and BattleScene placement verified
- Grumbar boss QA: Android full-chapter E2E passed on 2026-07-16; phases 2/3, five reinforcements, boss HUD, and Stage 10 Victory verified
- Complete rune QA: Android system flow passed on 2026-07-16; seven dedicated rune mechanics, crystal shield HUD, purification healing, and future-spawn frost slow verified
- Post-rune Android full-chapter regression: Stage 1~10 Victory, 10 upgrades, Grumbar phases 2/3, and five reinforcements passed on 2026-07-16

- Complete hero skill QA: Android system flow passed on 2026-07-16; Shield Bash control, Rapid Shot 3-hit timing, Meteor area damage, Holy Heal, Temporary Turret, and Shadow Strike verified
- Post-skill Android full-chapter regression: Stage 1~10 Victory, skill-assisted combat, 10 upgrades, Grumbar phases 2/3, five reinforcements, and boss defeat passed on 2026-07-16
- Scene BGM Android QA: Menu/Battle theme creation, cross-fade transition, independent BGM/SFX settings, and Menu restoration passed on 2026-07-16
- Cross-process audio settings QA: BGM/SFX enabled state and volume restored in an independent Android reader process on 2026-07-16
- Post-BGM Android full-chapter regression: Stage 1~10 Victory and Grumbar phases 2/3 passed on 2026-07-16
- Grumbar attack-pattern QA: phase 1 hit 2 same-lane heroes, phase 2 hit 3 lane-front heroes, phase 3 hit all 6 heroes and Crystal; Stage 10 Victory passed on 2026-07-16

## Progression QA

- [x] 새 세이브에서 Stage 1만 해금됨
- [x] Stage 1 시작 가능
- [x] Stage 1 클리어 가능
- [x] Stage 2 해금 확인
- [x] Stage 1~10 목록 표시 확인
- [x] Stage 10에서 그룸바르 보스 등장 확인
- [x] 그룸바르 페이즈 2/3 및 지원군 5기 스폰 확인
- [x] 그룸바르 한국어 HP/페이즈 HUD 확인
- [x] 그룸바르 1·2·3페이즈 전용 공격, 텔레그래프, 실제 피해 확인
- [x] Victory 결과창 정상 표시
- [x] Defeat 결과창 정상 표시
- [x] Result 버튼: 다음 스테이지 / 재시도 / 업그레이드 / 스테이지 선택 정상
- [x] Upgrade 구매 정상
- [x] Android에서 Upgrade 구매 직후 Gold/레벨 UI 갱신 정상
- [x] Android 앱 재시작 후 Upgrade/Gold/Stage 2 해금 유지
- [x] Save/Load 정상
- [x] Reset Save 정상
- [x] 튜토리얼 표시/완료 저장 정상
- [x] 전투 일시정지 / 계속하기 / 재시작 / 스테이지 선택 정상
- [x] Android 백그라운드 전환 후 자동 일시정지 정상
- [x] StageSelect 편성 슬롯 교환 및 JSON 저장 정상
- [x] 저장 편성의 BattleScene 배치 반영 정상
- [x] 룬 20장 모두 구현된 runtime effectKey 사용
- [x] 번개/폭발/연쇄/분쇄 전투 보정 적용
- [x] 수호 보호막 및 정화 회복 적용
- [x] 냉기 룬이 이후 웨이브 신규 스폰에도 적용
- [x] 영웅 6인 스킬이 고유 runtime effectKey와 실제 전투 효과 사용
- [x] 그룸바르가 크리스탈 접촉 시 제거되지 않고 반복 공격
- [ ] StageSelect 편성 팝업 실기기 터치/텍스트 잘림 수동 확인

## Korean UI QA

- [x] 핵심 UI에 `??` 문자 없음
- [x] 핵심 UI에 `stage_goblin_forest` 같은 내부 id 노출 없음
- [x] HUD 상태가 한국어로 표시됨
- [x] Rune Selection에 `effectKey`가 기본 노출되지 않음
- [x] 세로 화면에서 주요 버튼 텍스트가 잘리지 않음

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
- [x] Android 에뮬레이터 설치 및 실행 확인
- [x] Android 에뮬레이터 터치 입력 및 Portrait Safe Area 확인
- [ ] 실기기 설치 확인
- [ ] 긴 세션 플레이 확인
- [x] AAB 빌드 후보 확인
- [ ] 릴리스 키스토어로 APK/AAB 서명

### Latest Build Attempt

- Date: 2026-07-12
- Command: `RuneGate.Editor.RuneGateCurrentBuildPipeline.BuildCurrentAndroidApkFromCommandLine`
- Result: Succeeded
- Clean branch: `codex/issue-34-clean-android-aab`
- Output: `C:\workspace\defense-game-issue34-artifacts\RuneGateDefense-clean.apk`
- File size: `71,887,387 bytes`
- SHA-256: `61AC8D256976197BDEC30718124D048900BA533383FEE50AC5AAD46D4114E03A`
- Package/Version: `com.hobeomcheon.runegatedefense`, `0.9.0` (`9`)
- SDK: minimum 25, target 36
- Integrity: zipalign 및 APK Signature Scheme v2 검증 통과
- Signing: Android Debug 인증서 사용 중
- ADB: 연결된 실기기 없음
- Latest APK includes the system-flow, save recovery, Result navigation, and Stage 1~10 regression changes
- AAB candidate: `71,825,497 bytes`, SHA-256 `0F0BB84567E9CA135B3825F90E6B55DF654C800CFAFC8E24DCD069EDB2782466`
- AAB bundletool validation and JAR signature verification passed; signed with Android Debug certificate
- App icon import uses uncompressed source textures and the Unity compressed-icon warning is no longer emitted
- Permanent current-content APK/AAB menu and command-line entry points verified
- Remaining: Android 실기기 설치, 실기기 터치/Safe Area, 장시간 플레이 검증

### Latest Emulator QA

- Date: 2026-07-16
- Branch: `codex/issue-36-android-emulator-qa`
- Device: `RuneGate_API35_Portrait`, Android 15 / API 35, 1080x2400, density 420
- APK: `71,892,795 bytes`
- SHA-256: `040BBDF2F3F5A7F8BAEF537CD5177860BA286DCF734E30F00AD2AD6C7C5E96B3`
- Install/Launch: passed with `adb install -r` and `UnityPlayerGameActivity`
- Flow: Title > StageSelect > Stage 1 > Tutorial > Battle > Rune Selection > Victory passed
- Result: Gold 110, Stage 2 unlock, Result actions visible
- Log: no fatal exception, null reference, or invalid AudioListener warning
- Physical device and long-session verification remain open

### Android Progression QA

- Date: 2026-07-12
- Branch: `codex/issue-38-android-progression-qa`
- Upgrade purchases: Crystal Reinforcement Level 1, Hero Training Level 1
- Persistence: Gold, upgrade levels, Stage 2 unlock survived force-stop and process restart
- Stage 2: BattleScene entered with `재문 숲 2`, Wave 1/3
- Purchase redraw regression: fixed with deferred IMGUI redraw via `GUIUtility.ExitGUI()`
- APK SHA-256: `CBD20CDB2A5E3707A0E4A30232619D1E64D7E9EB732E61FF8869871CDB1F5379`
- Detailed evidence: `docs/android-progression-qa-v088.md`

### Android Full Chapter QA

- Date: 2026-07-15
- Branch: `codex/issue-40-android-full-chapter-qa`
- Device: Android 15 / API 35 emulator, 1080x2400 Portrait
- Result: Stage 1~10 Victory, 10 upgrade purchases, Grumbar spawn verified
- Runtime errors: no fatal exception, null reference, or missing reference found
- APK SHA-256: `E85A28099C0D731CE75BB2BFE74E9E244577F9E6F5C959DA848AFCCF9E449412`
- Detailed evidence: `docs/android-full-chapter-qa-v088.md`

## Audio QA

- [x] Unity 내장 Audio 모듈 활성화
- [x] `SfxKey` 9종 절차형 폴백 생성
- [x] Android 시스템 플로우에서 9개 클립 존재 확인
- [x] TitleScene SFX 켜기/끄기 동작 확인
- [x] 앱 재실행 후 SFX 설정 유지 확인
- [x] Menu/Battle 절차형 BGM 생성 및 장면 전환 확인
- [x] BGM/SFX 음소거와 음량 설정 독립 저장 확인
- [ ] 실기기 스피커/이어폰 음량 확인
- [ ] 최종 WAV 및 작곡 BGM 적용

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
