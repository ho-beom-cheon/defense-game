# Local Save Stability v0.88

## 목적

RuneGate Defense는 서버나 클라우드 저장 없이 로컬 JSON 세이브를 사용한다. 전투 결과, 골드, 업그레이드, 스테이지 해금, 편성, 그림자 계약 진행 상태가 중복 적용되거나 손상되지 않도록 기본 안전 장치를 둔다.

## 저장 파일

- 기본 저장 파일: `runegate_save.json`
- 백업 파일: `runegate_save.json.bak`
- 임시 파일: `runegate_save.json.tmp`

저장 시에는 임시 파일에 JSON을 먼저 쓰고, 기존 저장 파일이 있으면 백업한 뒤 임시 파일을 실제 저장 파일로 이동한다.

## 로드 정책

- 기본 저장 파일이 없으면 백업 파일 로드를 시도한다.
- 기본 저장 파일이 비어 있거나 파싱에 실패하면 기존 파일을 백업하고 백업 파일 로드를 시도한다.
- 백업도 사용할 수 없으면 기본 세이브를 생성한다.
- 기본 세이브는 Stage 1을 해금하고, 튜토리얼은 아직 보지 않은 상태로 시작한다.
- 기본 편성은 `RuntimeContentCatalog.asset`의 `DefaultFormation`을 우선 사용한다.
- `DefaultFormation`이 없거나 유효한 슬롯이 없으면 코드 내 fallback 편성을 사용한다.
- 세이브에는 ScriptableObject 원본 슬롯을 직접 저장하지 않고 `FormationSlot` 복사본을 저장한다.

## TitleScene 시작 정책

- 저장 파일이 없는 첫 실행은 `새 게임 시작`으로 기본 세이브를 생성하고 StageSelectScene으로 이동한다.
- 저장 파일이 있는 상태에서 `새 게임`을 누르면 확인 UI를 먼저 보여준다.
- 새 게임 확인을 누른 경우에만 기존 진행을 초기화한다.
- `이어하기`는 기존 저장을 유지하고 StageSelectScene으로 이동한다.
- 저장 초기화 버튼도 별도 확인을 거친 뒤 실행한다.

## 진행 반영 정책

- BattleScene 시작 시 `battleRunId`를 생성한다.
- Result UI는 `SaveManager.TryApplyBattleResultProgression(...)` 한 번으로 골드, 클리어, 다음 스테이지 해금, 처리한 battleRunId를 저장한다.
- 같은 `battleRunId`가 다시 들어오면 보상을 중복 지급하지 않는다.
- 업그레이드 구매는 `SaveManager.TryPurchaseUpgrade(...)`로 골드 차감과 레벨 증가를 한 번에 저장한다.

## 수동 확인

1. Unity에서 새 세이브로 시작한다.
2. StageSelect에서 편성 슬롯이 6/9로 표시되는지 확인한다.
3. Stage 1을 클리어한다.
4. 골드와 Stage 2 해금 상태가 저장되는지 확인한다.
5. Unity Play Mode를 껐다가 다시 켠다.
6. Stage 2 해금, 골드, 업그레이드, 편성 상태가 유지되는지 확인한다.
7. 같은 Result 버튼을 빠르게 여러 번 눌러도 골드가 중복 지급되지 않는지 확인한다.

## 남은 한계

- 현재 JSON 파서는 프로젝트 전용 수동 파서다.
- 세이브 암호화나 위변조 방지는 없다.
- 모바일 OS가 저장 중 앱을 강제 종료하는 경우는 실기기 테스트가 필요하다.
