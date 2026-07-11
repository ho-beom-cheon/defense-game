# Android Build Guide

RuneGate Defense v0.9 Release Candidate 기준 Android 빌드 가이드다.

## Target Settings

- Product Name: `RuneGate Defense`
- Package Name: `com.hobeomcheon.runegatedefense`
- Version: `0.9.0`
- Bundle Version Code: `9`
- Default Orientation: Portrait
- Build format: APK first, AAB later for store submission

## Required Unity Modules

Unity Hub에서 현재 사용하는 Unity 6 버전에 맞는 아래 모듈이 필요하다.

- Android Build Support
- Android SDK & NDK Tools
- OpenJDK

## Unity Menu Flow

1. Unity에서 `C:\workspace\defense-game` 프로젝트를 연다.
2. `Tools/RuneGate/Bootstrap v1.0 Release Track`을 실행한다.
3. `Tools/RuneGate/Validate Project`를 실행한다.
4. `Tools/RuneGate/Configure Android v0.9 RC Settings`를 실행한다.
5. `Tools/RuneGate/Build Android APK v0.9 RC`를 실행한다.

빌드 후보 출력 경로:

`Builds/Android/RuneGateDefense-v0.9.0.apk`

`Builds/`는 생성 산출물이므로 커밋하지 않는다.

## Command Line Build Candidate

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.11f1\Editor\Unity.exe" `
  -batchmode `
  -quit `
  -projectPath "C:\workspace\defense-game" `
  -executeMethod RuneGate.Editor.RuneGateBootstrapper.BuildAndroidApkV09Rc `
  -logFile "C:\workspace\defense-game\unity-v09-android-build.log"
```

Unity Editor가 같은 프로젝트를 열고 있으면 batchmode는 실패한다. 이 경우 Unity Editor를 닫고 다시 실행한다.

## AAB Build 후보

현재 자동 메뉴는 APK 우선이다. Google Play 제출용 AAB는 추후 아래 방식으로 별도 확인한다.

1. Build Settings에서 Android 선택
2. Build App Bundle 체크
3. Version / Version Code 확인
4. Keystore 서명 설정 확인
5. `Builds/Android` 아래 AAB 출력

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
- Version: `0.9.0`
- Android Package Name: `com.hobeomcheon.runegatedefense`
- Android Bundle Version Code: `9`
- Resolution and Presentation > Default Orientation: Portrait
- Icon > Android icons: 앱 아이콘 후보 연결 여부

## Latest Build Attempt

- Date: 2026-07-12
- Result: Succeeded
- Command: `RuneGate.Editor.RuneGateCurrentBuildPipeline.BuildCurrentAndroidApkFromCommandLine`
- Output: `Builds/Android/RuneGateDefense-current.apk`
- File size: `65,881,473 bytes`
- SHA-256: `D8D902CBBEA90559FC68CEA7926C457706416085F9DFDBDC18E912E5CE65E824`
- Package: `com.hobeomcheon.runegatedefense`
- Version: `0.9.0` (`versionCode 9`)
- SDK: minimum 25, target 36
- App icons: Android density resources confirmed with `aapt dump badging`
- APK integrity: `zipalign -c -v 4` passed; APK Signature Scheme v2 verification passed
- Signing: Android Debug certificate; release keystore signing is still required for store submission
- Remaining verification: Android device install, touch input, safe area, and long-session performance

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

성공 시 로그에 `RUNEGATE_ANDROID_BUILD_PASSED`, 실패 시 `RUNEGATE_ANDROID_BUILD_FAILED`가 출력되고 배치 프로세스는 각각 종료 코드 `0` 또는 `1`을 반환한다.

## Common Failure Checks

- Android Build Support가 현재 Unity 버전에 설치되어 있는지 확인
- SDK/NDK/OpenJDK 경로 확인
- Build Settings의 Scenes In Build 확인
- Package Name 중복 여부 확인
- Keystore 서명 설정 확인
- Unity Editor가 같은 프로젝트를 이미 열고 있는지 확인
