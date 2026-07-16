# Android 전체 챕터 QA v0.88

## 검증 환경

- 검증일: 2026-07-15
- 브랜치: `codex/issue-40-android-full-chapter-qa`
- 기기: `RuneGate_API35_Portrait` Android 에뮬레이터
- OS / 해상도: Android 15, API 35, 1080x2400 Portrait
- 패키지: `com.hobeomcheon.runegatedefense`
- 저장 경로: 앱 내부의 격리된 `runegate-issue40-final.json`

## 실행 방법

Android 빌드 후 앱을 다음 인자로 실행했다.

```powershell
$adb = "C:\Users\cjs41\AppData\Local\Android\SdkRuneGate\platform-tools\adb.exe"

& $adb shell svc power stayon true
& $adb logcat -c
& $adb shell am start -S `
  -n com.hobeomcheon.runegatedefense/com.unity3d.player.UnityPlayerGameActivity `
  --es unity '\"-runegateSmokeFullChapter -runegateSavePath /data/user/0/com.hobeomcheon.runegatedefense/files/runegate-issue40-final.json\"'
```

## 확인 결과

- Stage 1~10 모두 Victory
- 각 스테이지 사이 강화 구매 10회 성공
- Stage 10에서 `그룸바르` 보스 스폰 확인
- 최종 로그: `RUNEGATE_FULL_CHAPTER_E2E_PASSED: upgrades=10, gold=595`
- `FATAL EXCEPTION`, `NullReferenceException`, `MissingReferenceException` 없음
- Stage 7은 수정 전 180초 타임아웃, 수정 후 약 22초에 Victory

## 수정한 교착 원인

Portrait 안전 경계에서 몬스터 루트는 스프라이트 폭만큼 화면 안쪽으로 보정된다. 기존 크리스탈 도착 판정은 루트 x 좌표만 비교했기 때문에, 화면 안에 마지막 몬스터가 남아도 도착으로 처리되지 않는 경우가 있었다.

- 크리스탈 접촉 x를 UI 안전 전투 경계 안으로 제한했다.
- 크리스탈 도착 판정은 몬스터 루트가 아니라 진행 방향의 스프라이트 왼쪽 경계를 사용한다.
- 공격 코루틴 없이 이동 컨트롤러의 공격 잠금만 남은 경우 잠금을 복구한다.
- 망각의 뼈병 부활은 설계 의도대로 Hard / Nightmare에서만 활성화한다.
- 전체 챕터 타임아웃 시 생존 몬스터 위치, HP, 상태, 공격 잠금을 기록한다.

## APK 증거

- 파일: `C:\workspace\defense-game-issue40-artifacts\RuneGateDefense-issue40-full-chapter-passed.apk`
- 크기: `71,893,583 bytes`
- SHA-256: `E85A28099C0D731CE75BB2BFE74E9E244577F9E6F5C959DA848AFCCF9E449412`
- Unity 로그: `C:\workspace\defense-game-issue40-artifacts\android-full-chapter-unity.log`

## 남은 범위

- Android 실기기 전체 챕터 플레이는 아직 확인하지 않았다.
- 10개 스테이지 자동 진행은 기능 회귀 검증이며 장시간 수동 밸런스 검증을 대신하지 않는다.
- 릴리스 키스토어 서명과 Play Console 업로드는 별도 릴리스 작업이다.
