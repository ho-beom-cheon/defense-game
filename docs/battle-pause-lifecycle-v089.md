# 전투 일시정지와 앱 생명주기 v0.89

## 목적

모바일 전투에서 사용자가 안전하게 전투를 멈추고 재개하거나 현재 전투를 종료할 수 있게 한다. 앱이 백그라운드로 전환된 동안 몬스터와 웨이브가 계속 진행되지 않도록 한다.

## 동작

- Battle HUD 오른쪽 위에 `일시정지` 버튼을 표시한다.
- `Preparing`, `WaveRunning` 상태에서만 수동 일시정지가 가능하다.
- 튜토리얼, 룬 선택, Victory, Defeat 상태에서는 기존 팝업 흐름을 우선한다.
- 일시정지 팝업에서 `계속하기`, `전투 재시작`, `스테이지 선택`을 사용할 수 있다.
- Android에서 앱 포커스를 잃거나 백그라운드로 전환되면 자동으로 일시정지한다.
- 앱으로 돌아와도 자동 재개하지 않는다. 사용자가 `계속하기`를 눌러야 한다.

## Time Scale 소유권

`BattlePauseController`는 일시정지 직전 `Time.timeScale`을 저장한다. 일반 플레이의 `1`뿐 아니라 자동 테스트에서 사용하는 `6`도 그대로 복원한다. 컨트롤러가 비활성화되거나 씬이 바뀌면 자신이 설정한 일시정지를 해제해 다음 씬에 `0`이 남지 않게 한다.

## 자동 검증

Android 시스템 흐름 스모크에서 다음을 확인했다.

- 튜토리얼 완료 후 일시정지 컨트롤러 사용 가능
- 일시정지 시 `Time.timeScale == 0`
- 재개 시 이전 `Time.timeScale == 6` 복원
- 생명주기 일시정지 사유 기록
- 최종 마커: `RUNEGATE_SYSTEM_FLOWS_E2E_PASSED`

## Android 수동 QA

- 검증일: 2026-07-15
- 기기: Android 15 / API 35 에뮬레이터
- 해상도: 1080x2400 Portrait
- 수동 일시정지 팝업: 화면 안 표시 확인
- 계속하기: 전투 재개 확인
- 전투 재시작: Wave 1, Gold 0으로 재시작 확인
- 스테이지 선택: StageSelectScene 이동 확인
- 홈 화면 이동 후 복귀: 자동 일시정지 및 생명주기 안내 문구 확인
- 치명적 Unity 런타임 예외 없음

## APK 증거

- 파일: `C:\workspace\defense-game-issue42-artifacts\RuneGateDefense-issue42-pause-lifecycle.apk`
- 크기: `71,896,563 bytes`
- SHA-256: `597796FB8F2D4CAD0157F95F9EEE13CA7461D7A9A3E994AD14D8C410DB6E7F22`
- 화면: `pause-popup.png`, `lifecycle-pause.png`
- 로그: `android-pause-lifecycle-unity.log`

## 남은 한계

- 실제 Android 기기의 전화 수신, 화면 잠금, 멀티윈도우 전환은 아직 확인하지 않았다.
- 현재 팝업은 기존 IMGUI UI 체계를 사용한다.
- 프로세스가 OS에 의해 완전히 종료되면 진행 중 전투 자체는 복원하지 않고 마지막 저장 진행 상태에서 시작한다.
