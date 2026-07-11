# Runtime E2E Smoke Test

## 목적

실제 Windows Player 빌드에서 `TitleScene -> StageSelectScene -> BattleScene -> Result -> UpgradeScene` 흐름을 자동 검증한다. 정적 데이터 검사만으로 찾기 어려운 씬 전환, 자동 전투, 룬 선택, 승리 보상, 스테이지 해금 문제를 확인하기 위한 개발용 테스트다.

## 실행 조건

`RuneGateRuntimeSmokeRunner`는 일반 실행에서는 생성되지 않는다. Player 실행 인자에 `-runegateSmoke` 또는 `-runegateSmokeFullChapter`가 있을 때만 활성화된다.

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

이 모드는 Stage 1~10을 순서대로 실제 플레이한다. 각 스테이지에서 Victory와 다음 스테이지 해금을 확인하고, UpgradeScene에서 구매 가능한 강화 하나를 구매한 뒤 다음 스테이지로 진행한다. Stage 10에서는 그룸바르 보스 스폰도 확인한다.

성공 기준은 `RUNEGATE_FULL_CHAPTER_E2E_PASSED`와 종료 코드 `0`이다.

## 최근 검증 결과

- 검증일: 2026-07-12
- Stage 1~10: 모두 Victory
- 강화 구매: 10회
- Stage 10: 그룸바르 스폰 확인
- 최근 격리 저장 최종 Gold: 462
- 테스트 저장 정리: JSON, `.tmp`, `.bak` 제거 확인

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

## 한계

- 입력 좌표와 시각 품질을 검사하는 테스트는 아니다.
- Defeat, 튜토리얼 완료, Reset Save, 앱 재시작 후 저장 재로딩은 별도 검증이 필요하다.
- Android 실기기의 터치 입력, Safe Area, 장시간 성능은 수동 QA가 필요하다.
