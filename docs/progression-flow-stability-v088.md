# Progression Flow Stability v0.88

## 목적

RuneGate Defense의 기본 진행 루프가 끊기지 않도록 유지한다.

기준 흐름:

```text
TitleScene -> StageSelectScene -> BattleScene -> Result -> UpgradeScene -> StageSelectScene
```

이 문서는 씬 전환, 저장 반영, 스테이지 해금, 보상, 업그레이드 구매가 서로 충돌하지 않도록 확인하는 기준이다.

## 적용 원칙

- 씬 전환 버튼은 `sceneTransitionRequested`로 중복 입력을 막는다.
- BattleScene은 전투 시작 시 `GameSession.BeginBattleRun()`으로 고유 `battleRunId`를 만든다.
- Result UI는 같은 `battleRunId`를 두 번 처리하지 않는다.
- StageSelect는 `RuntimeContentCatalog.asset`에서 Stage 1~10을 우선 로드한다.
- Battle, Rune, Upgrade, HeroRoster, DefaultFormation은 RuntimeContentCatalog를 우선 사용한다.
- 기본 세이브의 편성은 RuntimeContentCatalog의 `DefaultFormation`과 일치해야 한다.
- 진행 검증은 실제 저장 파일을 쓰지 않고 인메모리 `SaveData`로 수행한다.

## 현재 보강 상태

- TitleScene: 시작/계속 버튼 중복 씬 전환 방지.
- StageSelectScene: 전투 시작, 업그레이드, 타이틀 이동 중복 전환 방지.
- StageSelectScene: RuntimeContentCatalog 기준 Stage 1~10 정렬.
- BattleScene: 선택 스테이지가 없으면 마지막 선택 스테이지, 기본 스테이지, 첫 해금 스테이지 순서로 안전하게 해석.
- BattleScene: BattleRunId 생성 후 결과에 포함.
- Result UI: 골드, 클리어, 다음 스테이지 해금을 한 번만 저장하고 중복 처리 방지.
- UpgradeScene: 구매는 `SaveManager.TryPurchaseUpgrade(...)`로 처리.
- SaveManager: 기본 편성은 RuntimeContentCatalog의 `DefaultFormation`을 우선 사용하고, 실패 시 fallback 편성을 사용.

## Progression Smoke Test

Unity Editor에서 다음 메뉴를 실행한다.

```text
Tools/RuneGate/Run Progression Smoke Test
```

검증 항목:

- RuntimeContentCatalog 존재
- Stage 1~10 순서와 Wave/Spawn 데이터
- Stage 10 보스 웨이브
- RuneData 20종과 필수 runtime effectKey
- HeroRoster와 DefaultFormation 연결
- 기본 세이브의 Stage 1 해금
- 기본 세이브의 튜토리얼 미완료 상태
- 기본 세이브 formationSlots와 RuntimeContentCatalog DefaultFormation 일치
- GameSession의 Stage 1 -> Stage 2 next-stage 계산
- GameSession의 최종 스테이지 이후 종료 계산
- Stage 1 Victory 결과 적용 시 Stage 1 클리어, Stage 2 해금, 골드 지급
- 같은 BattleRunId 재처리 시 중복 골드/해금 미적용
- Stage 1 보상 110골드로 첫 업그레이드 1회 구매 가능
- 골드 부족 상태에서 업그레이드 구매 실패
- Stage 1~10 순차 클리어 시뮬레이션으로 전체 해금/클리어 체인 검증
- StageSelect 레이아웃이 주요 세로/가로 해상도에서 화면 안에 들어오는지 검증
- Stage별 총 몬스터 수, 총 HP, 예상 처치 보상, 사용 라인 수 정적 추정
- Stage 10 보스 웨이브가 실제 보스 MonsterData 스폰을 포함하는지 검증

레이아웃 검증 해상도:

- 1080x1920
- 720x1280
- 1440x2560
- 1600x900
- 2048x1152

## Validate Project

Unity Editor에서 다음 메뉴를 실행한다.

```text
Tools/RuneGate/Validate Project
```

위 Smoke Test와 별도로 프로젝트 필수 폴더, 씬, 데이터, RuntimePixel/ConceptSheets 분리 정책, 한글 폰트, 진행 루프 데이터를 함께 검사한다.

## 수동 확인 절차

1. Unity에서 Play Mode를 끄고 컴파일 완료를 기다린다.
2. `Tools/RuneGate/Run Progression Smoke Test`를 실행한다.
3. `Tools/RuneGate/Validate Project`를 실행한다.
4. TitleScene에서 시작을 누른다.
5. StageSelectScene에서 Stage 1을 선택하고 전투 시작을 누른다.
6. BattleScene에서 전투가 시작되는지 확인한다.
7. Victory 후 Result 패널에서 골드와 다음 스테이지 해금 문구를 확인한다.
8. UpgradeScene으로 이동해 업그레이드를 구매한다.
9. StageSelect로 돌아와 Stage 2 해금과 저장 유지 상태를 확인한다.
10. 같은 결과창 버튼을 빠르게 여러 번 눌러도 진행이 중복 적용되지 않는지 확인한다.

## 남은 리스크

- Unity Editor가 프로젝트를 열고 있으면 command-line batchmode Validator는 프로젝트 잠금 때문에 실패할 수 있다.
- 실제 GUI 자동 테스트는 아직 없다.
- 전투 추정치는 HeroData 공격력/공속과 MonsterData HP/보상 기준의 보조 지표이며, 실제 룬 선택/스킬/이동/타겟팅을 완전히 대체하지 않는다.
- 일부 UI는 IMGUI 기반이므로 최종 릴리즈 전 Canvas 기반 UI 전환 검토가 필요하다.
