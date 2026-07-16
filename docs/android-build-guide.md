# Android Build Guide

RuneGate Defense v1.0.0 Android 공개 테스트 후보 기준 빌드 가이드다.

## Target Settings

- Product Name: `RuneGate Defense`
- Package Name: `com.hobeomcheon.runegatedefense`
- Version: `1.0.0`
- Bundle Version Code: `10`
- Default Orientation: Portrait
- Build format: APK for device QA, release-signed AAB for store submission

## Required Unity Modules

Unity Hub에서 현재 사용하는 Unity 6 버전에 맞는 아래 모듈이 필요하다.

- Android Build Support
- Android SDK & NDK Tools
- OpenJDK

## Unity Menu Flow

1. Unity에서 `C:\workspace\defense-game` 프로젝트를 연다.
2. 기존 콘텐츠를 유지하려면 Bootstrap을 다시 실행하지 않는다.
3. `Tools/RuneGate/Configure Android Release Settings`를 실행한다.
4. `Tools/RuneGate/Validate v1.0 Release Track`을 실행한다.
5. 기기 QA용은 `Tools/RuneGate/Build Current Android APK`를 실행한다.
6. 스토어 후보는 환경 변수를 설정한 뒤 `Tools/RuneGate/Build Signed Android AAB`를 실행한다.

빌드 후보 출력 경로:

`Builds/Android/RuneGateDefense-v1.0.0.apk`

`Builds/`는 생성 산출물이므로 커밋하지 않는다.

## Command Line Build Candidate

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.11f1\Editor\Unity.exe" `
  -batchmode `
  -quit `
  -projectPath "C:\workspace\defense-game" `
  -executeMethod RuneGate.Editor.RuneGateCurrentBuildPipeline.BuildCurrentAndroidApkFromCommandLine `
  -runegateBuildPath "C:\workspace\defense-game-artifacts\RuneGateDefense-v1.0.0.apk" `
  -logFile "C:\workspace\defense-game\.utmp\v1-android-build.log"
```

Unity Editor가 같은 프로젝트를 열고 있으면 batchmode는 실패한다. 이 경우 Unity Editor를 닫고 다시 실행한다.

## AAB Build

Google Play 제출 후보는 `Build Signed Android AAB`를 사용한다. 키스토어와 비밀번호는 환경 변수로만 전달하고 저장소나 로그에 기록하지 않는다. 구체적인 변수명과 비밀 관리 규칙은 `docs/android-release-signing.md`를 따른다.

## Latest AAB Verification

- Date: 2026-07-16
- Result: Succeeded with current v1.0 scenes and content
- Branch: `codex/issue-95-v1-release-package`
- File size: `72,089,252 bytes`
- SHA-256: `7936ECFD04D726BA24CA807EFC288D7C8BD01663A31032D3970E3AB5C1D06FAB`
- Package/Version: `com.hobeomcheon.runegatedefense`, `1.0.0` (`versionCode 10`)
- SDK: minimum 25, target 36
- Manifest SHA-256: independently matched
- JAR signature: verified with a throwaway QA certificate
- Store submission: protected production keystore signing and Play Console upload are still required

## App Icon / Splash / Store Assets

프로젝트에 포함된 후보 에셋:

- `Assets/_Project/Art/RuntimePixel/App/app_icon_1024.png`
- `Assets/_Project/Art/RuntimePixel/App/app_icon_adaptive_foreground.png`
- `Assets/_Project/Art/RuntimePixel/App/splash_background_1080x1920.png`
- `Assets/_Project/Art/Store/store_feature_graphic_1024x500.png`

v0.9 작업에서 `app_icon_1024.png`와 `app_icon_adaptive_foreground.png`는 Android icon 후보로 PlayerSettings에 연결했다. Unity에서 최종 확인이 필요하다.

스플래시 후보 이미지는 아직 TitleScene 런타임 배경으로 강제 적용하지 않는다. 기존 TitleScene 흐름을 유지하고, 최종 스플래시/시작 화면 적용은 UI 폴리싱 단계에서 확인한다.

## Manual Player Settings Check

Unity에서 아래 항목을 직접 확인한다.

- Edit > Project Settings > Player
- Product Name: `RuneGate Defense`
- Version: `1.0.0`
- Android Package Name: `com.hobeomcheon.runegatedefense`
- Android Bundle Version Code: `10`
- Resolution and Presentation > Default Orientation: Portrait
- Icon > Android icons: 앱 아이콘 후보 연결 여부

## Previous v0.9 Build Attempt

- Date: 2026-07-12
- Result: Succeeded
- Command: `RuneGate.Editor.RuneGateCurrentBuildPipeline.BuildCurrentAndroidApkFromCommandLine`
- Clean branch: `codex/issue-34-clean-android-aab`
- Output: `C:\workspace\defense-game-issue34-artifacts\RuneGateDefense-clean.apk`
- File size: `71,887,387 bytes`
- SHA-256: `61AC8D256976197BDEC30718124D048900BA533383FEE50AC5AAD46D4114E03A`
- Package: `com.hobeomcheon.runegatedefense`
- Version: `0.9.0` (`versionCode 9`)
- SDK: minimum 25, target 36
- App icons: Android density resources confirmed with `aapt dump badging`
- App icon source compression: disabled; compressed adaptive-icon build warning removed
- APK integrity: `zipalign -c -v 4` passed; APK Signature Scheme v2 verification passed
- Signing: Android Debug certificate; release keystore signing is still required for store submission
- Included verification scope: latest system-flow, save recovery, Result navigation, and full chapter regression changes
- Remaining verification: physical Android device install, physical-device touch/safe area, and long-session performance

## Previous v0.9 Emulator Install QA

- Date: 2026-07-12
- AVD: `RuneGate_API35_Portrait`
- Android: 15 / API 35 / Google APIs x86_64
- Resolution: 1080x2400 Portrait, density 420
- APK install: passed with `adb install -r`
- Package/version: `com.hobeomcheon.runegatedefense`, `0.9.0` (`9`)
- Core flow: Title, StageSelect, Stage 1 Battle, Rune Selection, Victory, Stage 2 unlock passed
- Runtime log: no fatal exception, null reference, or invalid AudioListener warning
- Detailed evidence: `docs/android-emulator-qa-v088.md`
- Physical Android device and release-keystore verification remain required

## Current Content Build

진행 중인 데이터와 씬을 유지한 채 APK를 만들 때는 `Tools/RuneGate/Build Current Android APK`를 사용한다. 이 메뉴는 Bootstrap을 다시 실행하지 않으며, 현재 `EditorBuildSettings`에 활성화된 씬만 빌드한다.

배치 빌드 예시:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.11f1\Editor\Unity.exe" `
  -batchmode `
  -nographics `
  -projectPath "C:\workspace\defense-game" `
  -executeMethod RuneGate.Editor.RuneGateCurrentBuildPipeline.BuildCurrentAndroidApkFromCommandLine `
  -runegateBuildPath "C:\workspace\defense-game\Builds\Android\RuneGateDefense-current.apk" `
  -logFile "C:\workspace\defense-game\.utmp\android-current-build.log"
```

현재 콘텐츠 AAB 배치 빌드 예시:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.11f1\Editor\Unity.exe" `
  -batchmode `
  -nographics `
  -projectPath "C:\workspace\defense-game" `
  -executeMethod RuneGate.Editor.RuneGateCurrentBuildPipeline.BuildCurrentAndroidAabFromCommandLine `
  -runegateBuildPath "C:\workspace\defense-game\Builds\Android\RuneGateDefense-current.aab" `
  -logFile "C:\workspace\defense-game\.utmp\android-current-aab-build.log"
```

AAB 성공 시 `RUNEGATE_ANDROID_AAB_BUILD_PASSED`, 실패 시 `RUNEGATE_ANDROID_AAB_BUILD_FAILED`가 출력된다. APK와 AAB 빌드는 실행 전 `Validate Project`를 통과해야 하며 완료 후 기존 `buildAppBundle` 설정을 복원한다.

성공 시 로그에 `RUNEGATE_ANDROID_BUILD_PASSED`, 실패 시 `RUNEGATE_ANDROID_BUILD_FAILED`가 출력되고 배치 프로세스는 각각 종료 코드 `0` 또는 `1`을 반환한다.

## Release-Signed Build

스토어 후보 APK/AAB는 `Tools/RuneGate/Build Signed Android APK` 또는 `Tools/RuneGate/Build Signed Android AAB`를 사용한다. 키스토어 경로, 비밀번호와 별칭은 프로젝트에 저장하지 않고 환경 변수로만 전달한다.

- 성공 marker: `RUNEGATE_ANDROID_SIGNED_APK_BUILD_PASSED`, `RUNEGATE_ANDROID_SIGNED_AAB_BUILD_PASSED`
- 실패 marker: `RUNEGATE_ANDROID_SIGNED_APK_BUILD_FAILED`, `RUNEGATE_ANDROID_SIGNED_AAB_BUILD_FAILED`
- 산출물 metadata: `<artifact>.manifest.json`
- manifest 검증: 파일 크기, SHA-256, 앱 식별자, 버전, 서명 빌드 여부
- 설정 복원: 빌드 전 Unity `PlayerSettings`와 `buildAppBundle` 상태를 완료 후 정확히 복원

필수 환경 변수, CLI 예시, 비밀 관리와 2026-07-16 검증 결과는 `docs/android-release-signing.md`를 따른다. QA용 일회성 키스토어 서명은 통과했지만 운영 키스토어와 Play Console 업로드는 아직 별도 승인 및 확인이 필요하다.

## Common Failure Checks

- Android Build Support가 현재 Unity 버전에 설치되어 있는지 확인
- SDK/NDK/OpenJDK 경로 확인
- Build Settings의 Scenes In Build 확인
- Package Name 중복 여부 확인
- Keystore 서명 설정 확인
- 서명 빌드 환경 변수 네 개와 키스토어 파일 접근 권한 확인
- 빌드 후 생성된 manifest의 SHA-256 독립 재계산
- Unity Editor가 같은 프로젝트를 이미 열고 있는지 확인
