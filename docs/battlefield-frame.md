# BattleField Frame

## 목적

BattleScene에서 전투 월드, HUD, 스킬 버튼, 팝업이 서로 침범하지 않도록 BattleFrame 기준을 둔다.

## 현재 구현

- `BattleHUD`는 `GameFrameLayout.BattleFrame().HeaderArea`에 표시된다.
- `FormationSkillPanelUI`는 `SkillPanelArea`에 표시된다.
- `RuneSelectionUI`와 `StageResultUI`는 `PopupLayer` 위 중앙 팝업을 사용한다.
- 실제 유닛 스폰, 카메라, lane safe bounds는 기존 BattleField 로직을 유지한다.

## BattleFieldFrame 역할

`BattleFieldFrame`은 전투 월드가 차지해야 하는 화면 영역을 문서화한 기준이다. 현재 전투 월드 자체는 Unity world space이므로 IMGUI와 직접 결합하지 않는다.

향후 작업에서 카메라 orthographic size, spawn x, lane y, safe bounds를 이 영역 기준으로 맞춘다.

## 정렬 기준

- HeaderArea: 스테이지, 크리스탈 HP, 웨이브, 상태, 골드
- BattleFieldFrame: 유닛, 배경, lane, 이펙트
- SkillPanelArea: 영웅 스킬 버튼 ScrollView
- PopupLayer: Rune Selection, Result, Pet/Contract popup

## 남은 작업

- 실제 Android 기기에서 safe area와 터치 영역 확인
- 카메라 viewport와 BattleFieldFrame 간 연동 강화
- 스킬 패널을 Canvas prefab으로 이전할 때 동일 영역명 유지
