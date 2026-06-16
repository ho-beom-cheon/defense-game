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

- Date: 2026-06-16
- Result: Failed before build start
- Reason: another Unity instance was running with this project open
- Log target: `unity-v09-android-build.log`
- Next action: Unity Editor를 닫고 `Tools/RuneGate/Build Android APK v0.9 RC` 또는 command line build를 다시 실행한다.

## Common Failure Checks

- Android Build Support가 현재 Unity 버전에 설치되어 있는지 확인
- SDK/NDK/OpenJDK 경로 확인
- Build Settings의 Scenes In Build 확인
- Package Name 중복 여부 확인
- Keystore 서명 설정 확인
- Unity Editor가 같은 프로젝트를 이미 열고 있는지 확인
