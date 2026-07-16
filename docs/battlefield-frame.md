# BattleField Frame

> 상태: `BattleFieldFrame`의 viewport 분리 원칙은 유지한다.
> 고정 lane Y, 같은 레인 타기팅, 세 레인 지면 구성은
> `docs/ui-frame-and-continuous-battlefield-v2-design.md`의 연속형 2D 전장 규칙으로 대체한다.

## 목적

BattleScene에서 전투 월드, HUD, 스킬 버튼, 팝업이 서로 침범하지 않도록 BattleFrame 기준을 둔다.

## 현재 구현

- `BattleHUD`는 `GameFrameLayout.BattleFrame().HeaderArea`에 표시된다.
- `FormationSkillPanelUI`는 `SkillPanelArea`에 표시된다.
- `RuneSelectionUI`와 `StageResultUI`는 `PopupLayer` 위 중앙 팝업을 사용한다.
- 카메라 viewport는 `BattleFieldFrame`을 사용하고 Portrait의 lane y는 카메라 world height에 맞춰 분산한다.
- 실제 유닛 스폰, 크리스탈 목표, 영웅 슬롯은 같은 유효 lane y를 사용한다.

## BattleFieldFrame 역할

`BattleFieldFrame`은 전투 월드가 차지해야 하는 화면 영역을 문서화한 기준이다. 현재 전투 월드 자체는 Unity world space이므로 IMGUI와 직접 결합하지 않는다.

카메라 orthographic size와 safe bounds는 전투 경로 폭을 보존한다. Portrait에서는 라인 간격을 카메라 world height의 25%로 계산하고, 배경은 카메라 bounds를 채워 세 라인이 화면 높이를 고르게 사용하도록 한다.

## 정렬 기준

- HeaderArea: 스테이지, 크리스탈 HP, 웨이브, 상태, 골드
- BattleFieldFrame: 유닛, 배경, lane, 이펙트
- SkillPanelArea: 영웅 스킬 버튼 ScrollView
- PopupLayer: Rune Selection, Result, Pet/Contract popup

## 남은 작업

- 실제 Android 기기에서 safe area와 터치 영역 확인
- 세로 전장 전용 배경 또는 타일형 상하 확장 자산 제작
- 스킬 패널을 Canvas prefab으로 이전할 때 동일 영역명 유지
