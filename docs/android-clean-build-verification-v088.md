# v0.88 Clean Android Build Verification

## 기준

- Branch: `codex/issue-34-clean-android-aab`
- Unity: `6000.4.11f1`
- Package: `com.hobeomcheon.runegatedefense`
- Version: `0.9.0` (`versionCode 9`)
- SDK: minimum 25, target 36
- Build source: clean worktree with no dependency on the original dirty checkout

## APK

- Path: `C:\workspace\defense-game-issue34-artifacts\RuneGateDefense-clean.apk`
- Size: `71,887,387 bytes`
- SHA-256: `61AC8D256976197BDEC30718124D048900BA533383FEE50AC5AAD46D4114E03A`
- `zipalign -c 4`: passed
- `apksigner verify`: passed
- APK Signature Scheme v2: true
- Signer: Android Debug certificate
- `aapt2 dump badging`: package, version, min SDK, target SDK, label confirmed

## AAB

- Path: `C:\workspace\defense-game-issue34-artifacts\RuneGateDefense-clean.aab`
- Size: `71,825,497 bytes`
- SHA-256: `0F0BB84567E9CA135B3825F90E6B55DF654C800CFAFC8E24DCD069EDB2782466`
- `bundletool validate`: passed
- Base manifest package: `com.hobeomcheon.runegatedefense`
- Version: `0.9.0` (`versionCode 9`)
- `jarsigner -verify`: passed
- Signer: self-signed Android Debug certificate

## 함께 확인한 런타임

- Unity `Validate Project`: passed
- Clean Windows Player build: passed
- Stage 1 E2E: passed
- Tutorial, Reset Save, Defeat Result, navigation E2E: passed
- Stage 1~10 full chapter: passed
- Upgrade purchases: 10
- Stage 10 Grumbar spawn and victory: passed

## 남은 릴리스 작업

- Android 실기기 설치와 터치/Safe Area 확인
- 장시간 플레이와 발열/메모리 확인
- 보호된 release keystore로 최종 서명
- Play Console 업로드 전 정책, Data Safety, target SDK 최종 점검
