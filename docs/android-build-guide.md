# Android Build Guide

## Target

- Product Name: RuneGate Defense
- Package Name: `com.hobeomcheon.runegatedefense`
- Orientation: Portrait
- v0.9 Version: `0.9.0`
- v1.0 Version: `1.0.0`
- Test format: APK first, AAB later for store submission

## Unity Setup

1. Install Unity 6 Android Build Support from Unity Hub.
2. Include Android SDK, NDK, and OpenJDK modules.
3. Open `C:\workspace\defense-game` in Unity.
4. Run `Tools/RuneGate/Bootstrap v1.0 Release Track`.
5. Run `Tools/RuneGate/Validate Project`.

## Build Menu

Use:

`Tools/RuneGate/Build Android APK v1.0`

The build output path is:

`Builds/Android/RuneGateDefense-v1.0.0.apk`

`Builds/` is generated output and should not be committed.

## Manual Player Settings Check

- Company Name: `Ho Beom Cheon`
- Product Name: `RuneGate Defense`
- Package Name: `com.hobeomcheon.runegatedefense`
- Version: `1.0.0`
- Bundle Version Code: `10`
- Default Orientation: Portrait
- Scenes in Build:
  - `TitleScene`
  - `StageSelectScene`
  - `BattleScene`
  - `UpgradeScene`

## Current Notes

No external ad SDK, billing SDK, Firebase, Addressables, login, or server integration is required for v1.0.

If the APK build fails, first check whether Android Build Support is installed for the exact Unity Editor version.

## Latest Local Build Result

- Date: 2026-06-15
- Unity: 6000.4.11f1
- Command: `Tools/RuneGate/Build Android APK v1.0`
- Result: Succeeded
- Output: `Builds/Android/RuneGateDefense-v1.0.0.apk`
