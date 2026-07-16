# Android Release Signing

RuneGate Defense의 Android 릴리스 서명 빌드는 키스토어 정보를 저장소나 Unity 프로젝트 설정에 저장하지 않고 환경 변수로만 전달한다.

## 지원 메뉴와 CLI

- Unity 메뉴: `Tools/RuneGate/Build Signed Android APK`
- Unity 메뉴: `Tools/RuneGate/Build Signed Android AAB`
- CLI APK: `RuneGate.Editor.RuneGateCurrentBuildPipeline.BuildSignedAndroidApkFromCommandLine`
- CLI AAB: `RuneGate.Editor.RuneGateCurrentBuildPipeline.BuildSignedAndroidAabFromCommandLine`

기존 `Build Current Android APK/AAB`는 개발 및 회귀 확인용 빌드로 그대로 유지한다.

## 필수 환경 변수

```text
RUNEGATE_ANDROID_KEYSTORE_PATH
RUNEGATE_ANDROID_KEYSTORE_PASSWORD
RUNEGATE_ANDROID_KEY_ALIAS
RUNEGATE_ANDROID_KEY_ALIAS_PASSWORD
```

키스토어 파일은 저장소 밖의 보호된 위치에 둔다. 비밀번호는 로컬 비밀 저장소 또는 CI의 masked secret으로 주입하며 PowerShell 프로필, 배치 파일, 문서, Unity 로그에 기록하지 않는다.

## PowerShell 빌드 예시

```powershell
$env:RUNEGATE_ANDROID_KEYSTORE_PATH = "C:\secure\runegate-release.jks"
$env:RUNEGATE_ANDROID_KEYSTORE_PASSWORD = "<secret>"
$env:RUNEGATE_ANDROID_KEY_ALIAS = "<alias>"
$env:RUNEGATE_ANDROID_KEY_ALIAS_PASSWORD = "<secret>"

& "C:\Program Files\Unity\Hub\Editor\6000.4.11f1\Editor\Unity.exe" `
  -batchmode `
  -quit `
  -projectPath "C:\workspace\defense-game" `
  -executeMethod RuneGate.Editor.RuneGateCurrentBuildPipeline.BuildSignedAndroidAabFromCommandLine `
  -runegateBuildPath "C:\release\RuneGateDefense.aab" `
  -logFile "C:\release\RuneGateDefense-aab.log"

Remove-Item Env:RUNEGATE_ANDROID_KEYSTORE_PATH
Remove-Item Env:RUNEGATE_ANDROID_KEYSTORE_PASSWORD
Remove-Item Env:RUNEGATE_ANDROID_KEY_ALIAS
Remove-Item Env:RUNEGATE_ANDROID_KEY_ALIAS_PASSWORD
```

## 안전 동작

- 네 환경 변수 중 하나라도 없거나 키스토어 파일이 없으면 빌드를 시작하지 않는다.
- 오류 로그에는 `missing_keystore_path` 같은 일반 오류 코드만 남기고 비밀번호, 별칭, 키스토어 경로 값은 남기지 않는다.
- 빌드 직전에만 Unity Android 서명 설정을 적용한다.
- 성공, 실패, 예외와 관계없이 원래 `PlayerSettings`와 `buildAppBundle` 값을 복원한다.
- Unity의 직렬화 설정을 정확히 캡처할 수 없으면 `settings_snapshot_unavailable`로 실패한다.
- 키스토어, APK, AAB, manifest와 빌드 로그는 Git에 추가하지 않는다.

## 산출물 Manifest

성공한 빌드는 산출물 옆에 `<artifact>.manifest.json`을 생성한다. manifest에는 다음 값만 포함한다.

- 파일명과 형식
- 파일 크기와 SHA-256
- Unity 버전
- Product Name과 Application Identifier
- 앱 버전과 Android Version Code
- UTC 빌드 시각
- 릴리스 서명 경로 사용 여부

키스토어 경로, 비밀번호, 별칭은 포함하지 않는다. 배포 전 manifest의 SHA-256을 독립적으로 다시 계산해 일치 여부를 확인한다.

```powershell
$artifact = "C:\release\RuneGateDefense.aab"
$manifest = Get-Content -Raw "$artifact.manifest.json" | ConvertFrom-Json
$actual = (Get-FileHash -Algorithm SHA256 $artifact).Hash
$actual -eq $manifest.sha256
```

## 2026-07-16 QA 결과

- 자격 증명 미설정 서명 AAB: 종료 코드 `1`, `missing_keystore_path` 확인
- 저장소 밖 일회성 QA 키스토어 서명 AAB: 성공
- QA AAB: `72,051,968 bytes`
- SHA-256: `B87F2C88C373A3B323096767C9F248547EF0A26BC0DBC118D8EC6B3B53ABA7A3`
- `jarsigner -verify`: 통과
- manifest SHA-256 독립 계산: 일치
- manifest 비밀 필드 검사: 없음
- 빌드 전후 `ProjectSettings/ProjectSettings.asset`: 변경 없음
- 기존 APK 회귀 빌드: 성공, manifest의 `releaseSigned=false`와 SHA-256 일치 확인
- 회귀 APK 설치/실행: Android 15 API 35 에뮬레이터에서 통과

위 AAB는 파이프라인 검증용 일회성 키스토어로 서명한 산출물이며 스토어 제출용이 아니다. 실제 제출 전 보호된 운영 키스토어, Play App Signing, 권한, Data Safety와 업로드 절차를 별도로 확인한다.
