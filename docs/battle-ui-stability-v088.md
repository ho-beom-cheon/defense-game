# Battle UI Stability v0.88

## 목적

BattleScene은 전투 유닛, 배경, HUD, 스킬 버튼, 룬 선택, 결과 패널이 동시에 보이는 화면이다. 모바일 세로 화면과 Unity Editor의 Free Aspect 화면에서 UI가 전투 영역을 과도하게 침범하지 않아야 한다.

## 적용 기준

- Battle HUD는 `GameFrameLayout.BattleFrame().HeaderArea` 안에서만 그린다.
- compact 화면에서는 HUD가 전투 영역으로 내려오지 않도록 영웅 요약 줄을 생략할 수 있다.
- Hero Skill Panel은 하단 SkillPanelArea에 고정하고 내부 ScrollView로 처리한다.
- Rune Selection은 중앙 팝업 + dim overlay로 표시한다.
- 룬 선택 버튼은 한 번 누르면 `적용 중` 상태로 잠겨 중복 선택을 막는다.
- Result Panel은 기존처럼 중앙 팝업과 중복 씬 전환 방지 상태를 유지한다.

## 수동 확인 절차

1. Unity에서 Play Mode를 끄고 스크립트 컴파일 완료를 기다린다.
2. StageSelectScene에서 Stage 1을 시작한다.
3. BattleScene에서 HUD가 상단 영역을 넘지 않는지 확인한다.
4. 스킬 버튼이 하단 영역에서 스크롤 가능하게 보이는지 확인한다.
5. 웨이브 종료 후 Rune Selection 팝업이 중앙에 뜨는지 확인한다.
6. 룬 선택 버튼을 빠르게 여러 번 눌러도 한 번만 적용되는지 확인한다.
7. Victory/Defeat Result 패널이 정상 표시되고 버튼 중복 입력으로 씬 전환이 꼬이지 않는지 확인한다.

## 남은 한계

- BattleScene UI는 아직 IMGUI 기반이다.
- 실제 모바일 터치 영역, Android safe area, 장시간 플레이 성능은 기기 테스트가 필요하다.
- 최종 릴리스 전에는 Canvas/RectTransform 기반 UI prefab 전환을 검토해야 한다.
