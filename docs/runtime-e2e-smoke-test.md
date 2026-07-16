# Runtime E2E Smoke Test

## 목적

실제 Windows Player 빌드에서 `TitleScene -> StageSelectScene -> BattleScene -> Result -> UpgradeScene` 흐름을 자동 검증한다. 정적 데이터 검사만으로 찾기 어려운 씬 전환, 자동 전투, 룬 선택, 승리 보상, 스테이지 해금 문제를 확인하기 위한 개발용 테스트다.

## 실행 조건

`RuneGateRuntimeSmokeRunner`는 일반 실행에서는 생성되지 않는다. Player 실행 인자에 `-runegateSmoke`, `-runegateSmokeFullChapter`, `-runegateSmokeAllDifficulties` 같은 진단 인자가 있을 때만 활성화된다.

테스트에는 반드시 `-runegateSavePath`로 별도 JSON 경로를 지정해야 한다. 지정된 테스트 저장 파일과 `.tmp`, `.bak`만 정리하며 실제 `runegate_save.json`은 읽거나 변경하지 않는다.

## Stage 1 실행 예시

```powershell
& ".utmp/WindowsPlaytest/RuneGateDefense.exe" `
  -batchmode `
  -nographics `
  -runegateSmoke `
  -runegateSavePath ".utmp/runtime-e2e-save.json" `
  -logFile ".utmp/runtime-e2e.log"
```

성공하면 Player가 종료 코드 `0`을 반환하고 로그에 `RUNEGATE_E2E_PASSED`를 남긴다. 실패하면 종료 코드 `1`과 `RUNEGATE_E2E_FAILED` 뒤에 원인을 기록한다.

## 전체 챕터 실행 예시

```powershell
& ".utmp/WindowsPlaytest/RuneGateDefense.exe" `
  -batchmode `
  -nographics `
  -runegateSmokeFullChapter `
  -runegateSavePath ".utmp/runtime-full-chapter-save.json" `
  -logFile ".utmp/runtime-full-chapter.log"
```

이 모드는 Stage 1~10을 순서대로 실제 플레이한다. 각 스테이지에서 사용 가능한 영웅 스킬을 주기적으로 발동하고 Victory와 다음 스테이지 해금을 확인하며, UpgradeScene에서 구매 가능한 강화 하나를 구매한 뒤 다음 스테이지로 진행한다. Stage 10에서는 그룸바르 보스 스폰, 페이즈 2·3, 지원군 5기, 한국어 보스 HUD와 실제 보스 처치도 확인한다.

성공 기준은 `RUNEGATE_FULL_CHAPTER_E2E_PASSED`와 종료 코드 `0`이다.

## 전체 난이도 캠페인

`-runegateSmokeAllDifficulties`는 하나의 격리 저장으로 Normal, Hard, Nightmare Stage 1~10을 모두 실제 플레이한다. 각 난이도에서 이전 스테이지 승리 후 다음 스테이지가 열리는지 확인하고, Normal/Hard 완료 후 다음 난이도 해금과 Result 안내를 검증한다.

총 30전투에서 룬 선택, 스킬 자동 사용, Result, Upgrade, StageSelect 복귀를 반복한다. 각 난이도의 Stage 10은 그룸바르 3페이즈, 지원군, 보스 HUD, Phase 1/2/3 공격 패턴을 모두 검증한다. 마지막에는 세 난이도 완료와 업그레이드가 JSON 디스크 재로드 뒤에도 유지되는지 확인한다.

성공 기준은 `RUNEGATE_ALL_DIFFICULTIES_E2E_PASSED`와 종료 코드 `0`이다. 상세 결과는 `docs/android-all-difficulty-campaign-qa-v090.md`를 참고한다.

Android에서는 Unity 실행 인자를 문자열 하나로 전달해야 한다. PowerShell에서는 바깥 따옴표를 이스케이프한 다음과 같은 형식을 사용한다.

```powershell
$adb = "C:\Users\cjs41\AppData\Local\Android\SdkRuneGate\platform-tools\adb.exe"
& $adb shell svc power stayon true
& $adb shell am start -S `
  -n com.hobeomcheon.runegatedefense/com.unity3d.player.UnityPlayerGameActivity `
  --es unity '\"-runegateSmokeFullChapter -runegateSavePath /data/user/0/com.hobeomcheon.runegatedefense/files/runegate-full-chapter-qa.json\"'
```

전체 챕터 모드는 스테이지당 최대 180초를 기다린다. 타임아웃이 발생하면 생존 몬스터의 이름, 라인, HP, x 좌표, 상태, 이동 공격 잠금, 공격 코루틴 여부를 로그에 남긴다.

Stage 10에서는 그룸바르의 페이즈 2·3과 지원군 5기뿐 아니라 보스 전용 패턴이 실제 영웅을 맞혔는지도 검증한다. 페이즈 3 패턴은 전체 영웅과 크리스탈을 압박하며, 성공 로그에는 `[RuneGateFullE2E] Boss pattern verified`가 포함된다.

## 시스템 흐름 실행 예시

```powershell
& ".utmp/WindowsSystemFlow/RuneGateDefense.exe" `
  -batchmode `
  -nographics `
  -runegateSmokeSystemFlows `
  -runegateSavePath ".utmp/system-flow-save.json" `
  -logFile ".utmp/system-flow-player.log"
```

이 모드는 첫 BattleScene의 튜토리얼 7단계를 완료한 뒤 완료 상태를 저장한다. Gold, 업그레이드 레벨, 마지막 선택 스테이지를 저장하고 메모리 캐시를 비운 뒤 실제 JSON에서 다시 읽어 값을 비교한다. 이어서 Reset Save 기본값을 확인하고, BattleScene에서 크리스탈 HP를 0으로 만들어 Defeat와 Result UI를 검증한다. Defeat 결과에서는 재시도, 업그레이드, 스테이지 선택 이동을 실제로 실행한다.

튜토리얼 완료 직후 전투 일시정지도 검증한다. `Time.timeScale`이 `0`으로 멈추는지, 재개 후 테스트 배속으로 돌아오는지, 생명주기 일시정지 사유가 구분되는지 확인한다.

같은 실행에서 번개, 폭발, 수호, 정화, 분쇄, 연쇄, 냉기 룬을 실제 `RuneEffectApplier`에 적용한다. 영웅 보조 피해 보정, 정화 회복, 크리스탈 보호막과 한국어 HUD, 냉기 룬의 이후 스폰 감속을 확인한 뒤 기존 저장/결과 흐름을 계속 검증한다.

또한 여섯 영웅 스킬의 고유 effectKey와 실제 동작을 검증한다. 레온의 밀치기/제어, 세리아의 시간차 3연타, 카엘의 다중 대상 범위 피해, 미레아의 영웅·크리스탈 회복, 브롬의 포탑 배치/자동 발사, 닉스의 보스 대상 피해가 모두 통과해야 한다.

성공 기준은 `RUNEGATE_SYSTEM_FLOWS_E2E_PASSED`와 종료 코드 `0`이다. 반드시 격리된 `-runegateSavePath`를 사용하며 종료 시 JSON, `.tmp`, `.bak` 파일이 모두 정리되어야 한다.

시스템 플로우는 오디오도 함께 검증한다. Title/StageSelect/Upgrade의 Menu BGM, Battle의 Battle BGM, 장면 전환 교차 페이드, BGM/SFX 독립 음소거와 음량 저장이 통과하면 `[RuneGateAudioE2E] Scene BGM transitions and independent audio settings verified.`를 기록한다.

## 앱 재시작 저장 실행 예시

첫 번째 Player가 저장 파일을 작성하고 종료한다.

```powershell
& ".utmp/WindowsSystemFlow/RuneGateDefense.exe" `
  -batchmode `
  -nographics `
  -runegateSmokeSaveWrite `
  -runegateSavePath ".utmp/cross-process-save.json" `
  -logFile ".utmp/cross-process-write.log"
```

두 번째 Player가 같은 JSON을 읽어 Gold, 업그레이드, 튜토리얼, 마지막 선택 스테이지, 스테이지 해금을 확인한다.

```powershell
& ".utmp/WindowsSystemFlow/RuneGateDefense.exe" `
  -batchmode `
  -nographics `
  -runegateSmokeSaveRead `
  -runegateSavePath ".utmp/cross-process-save.json" `
  -logFile ".utmp/cross-process-read.log"
```

성공 로그는 각각 `RUNEGATE_SAVE_WRITE_PASSED`, `RUNEGATE_SAVE_READ_PASSED`이며 두 프로세스 모두 종료 코드 `0`을 반환해야 한다. 읽기 검증이 끝나면 격리 JSON, `.tmp`, `.bak`를 정리한다.

## 손상 저장 복구 실행 예시

격리 경로에 중괄호나 배열이 닫히지 않은 잘못된 JSON을 준비한 뒤 Player를 실행한다.

```powershell
& ".utmp/WindowsSystemFlow/RuneGateDefense.exe" `
  -batchmode `
  -nographics `
  -runegateSmokeCorruptSave `
  -runegateSavePath ".utmp/corrupt-e2e-save.json" `
  -logFile ".utmp/corrupt-save-player.log"
```

정상 `.bak`가 없으면 손상본을 `.corrupt`로 보존하고 Stage 1만 해금된 기본 저장을 다시 작성한다. 정상 `.bak`가 있으면 손상된 주 파일이 백업을 덮어쓰지 않으며 Gold, 업그레이드, 튜토리얼, 스테이지 해금을 복원한다.

성공 로그는 기본 복구의 `RUNEGATE_CORRUPT_SAVE_RECOVERY_PASSED` 또는 백업 복원의 `RUNEGATE_CORRUPT_SAVE_BACKUP_RESTORE_PASSED`다.

## 중단된 원자적 저장 복구

주 저장 파일이 없고 유효한 `.tmp`가 남은 상태는 저장 직후 앱이 강제 종료된 상황을 나타낸다. Player를 아래 인자로 실행하면 임시 저장의 Gold와 진행 상태를 확인한 뒤 주 JSON으로 승격한다.

```powershell
& ".utmp/WindowsSystemFlow/RuneGateDefense.exe" `
  -batchmode `
  -nographics `
  -runegateSmokeInterruptedSave `
  -runegateSavePath ".utmp/interrupted-save.json" `
  -logFile ".utmp/interrupted-save-recover.log"
```

유효한 임시 저장 승격은 `RUNEGATE_INTERRUPTED_SAVE_RECOVERY_PASSED`를 출력한다. `.tmp`가 손상되었으면 `.tmp.corrupt`로 격리하고 정상 `.bak`를 복원하며 `RUNEGATE_INVALID_TEMP_BACKUP_RESTORE_PASSED`를 출력한다.

## 최근 검증 결과

- 검증일: 2026-07-15
- Stage 1~10: 모두 Victory
- 강화 구매: 10회
- Stage 10: 그룸바르 스폰 확인
- Android 15 API 35 에뮬레이터: 1080x2400 Portrait 전체 챕터 통과
- 최근 격리 저장 최종 Gold: 595
- 테스트 저장 정리: JSON, `.tmp`, `.bak` 제거 확인
- 시스템 흐름: 튜토리얼 완료, JSON 재로딩, Reset Save, Defeat Result 통과
- 전투 일시정지: pause/resume timeScale 복원과 생명주기 일시정지 통과
- 결과 이동: 다음 스테이지, 재시도, 업그레이드, 스테이지 선택 통과
- 앱 재시작 저장: 독립 Player 작성/읽기 프로세스 모두 통과
- 손상 저장: 기본값 복구와 정상 `.bak` 진행 복원 모두 통과
- 중단 저장: 유효한 `.tmp` 승격과 손상 `.tmp` 격리 후 `.bak` 복원 통과
- 저장 복구 변경 후 전체 챕터 회귀: Stage 1~10 Victory, 강화 10회, 그룸바르 스폰 통과
- 전체 난이도 캠페인: Normal/Hard/Nightmare Stage 1~10, 총 30 Victory와 강화 20회 통과

랜덤 몬스터 변형에 따라 최종 Gold는 달라질 수 있다.

## 검증 항목

- TitleScene과 TitleUI 로드
- StageSelectScene과 Stage 1~10 데이터 로드
- Stage 1 선택 및 BattleScene 진입
- 영웅 6인 편성 생성
- 실제 WaveManager 기반 자동 전투
- 비보스 웨이브 룬 3개 제시 및 룬 선택
- Victory와 Result UI 표시
- Gold 지급과 다음 스테이지 해금 저장
- UpgradeScene과 강화 4종 로드
- Stage 10 보스 스폰
- 튜토리얼 7단계 표시 및 완료 저장
- Gold, 업그레이드, 마지막 스테이지 JSON 재로딩
- Reset Save 후 Gold/업그레이드/튜토리얼 초기화
- Reset Save 후 Stage 1만 해금
- 크리스탈 파괴에 따른 Defeat와 Result UI 표시
- Defeat 시 Stage 1 클리어 및 Stage 2 해금 방지
- Victory 결과의 다음 스테이지 이동
- Defeat 결과의 재시도, 업그레이드, 스테이지 선택 이동
- 완전 종료 후 새 Player 프로세스의 Gold/업그레이드/튜토리얼/스테이지 상태 복원
- 손상 JSON 분리 보관과 정상 백업 보존
- 손상 주 파일에서 기본 저장 또는 정상 백업 복원
- 주 파일이 사라진 원자적 저장 중단 상태에서 최신 `.tmp` 승격
- 손상된 `.tmp`가 정상 `.bak`를 덮지 않는지 확인

## 한계

- 입력 좌표와 시각 품질을 검사하는 테스트는 아니다.
- 실제 화면에서 이어하기 버튼을 직접 누르는 터치 기반 저장 QA는 별도로 필요하다.
- Android 실기기의 터치 입력, Safe Area, 장시간 성능은 수동 QA가 필요하다.
