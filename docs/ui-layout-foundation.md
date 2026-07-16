# UI Layout Foundation

> 상태: 화면 프레임의 최종 uGUI 구조와 TitleScene 기준은
> `docs/ui-frame-and-continuous-battlefield-v2-design.md`가 우선한다.
> 이 문서의 IMGUI Rect 유지 전제는 레거시 화면 참고용이다.

## 기준

- 기준 해상도: 1080 x 1920 Portrait
- 공통 safe area: `UIResponsiveLayout.SafeRect()`
- 공통 frame root: `GameFrameLayout.FrameRoot()`
- 팝업: `GameFrameLayout.PopupFrame()`

## FrameRoot 구조

Canvas 기반 화면으로 전환할 때의 목표 구조는 아래와 같다.

```text
Canvas
└── SafeAreaRoot
    └── FrameRoot
        ├── HeaderArea
        ├── MainArea
        ├── FooterArea
        └── PopupLayer
```

현재 구현은 기존 IMGUI 화면을 유지하면서 같은 구조의 Rect를 계산한다. 나중에 Canvas prefab으로 옮길 때도 동일한 영역 이름을 유지한다.

## StageSelectFrame

```text
StageSelectFrame
├── HeaderArea
├── DifficultyArea
├── MainArea
│   ├── StageListPanel
│   └── StageDetailPanel
├── PetContractArea
└── FooterArea
```

리스트와 상세 패널은 별도 Rect를 사용하므로 서로 겹치지 않는다. 상세 정보는 고정 패널 기준이고, 내용이 길어지면 ScrollView로 보호한다.

## BattleFrame

```text
BattleFrame
├── HeaderArea
├── BattleFieldFrame
├── SkillPanelArea
├── FooterArea
└── PopupLayer
```

전투 월드 카메라는 별도 시스템이 관리하지만, HUD와 스킬 패널은 BattleFrame 영역 기준으로 배치한다.

## 신규 UI 작성 규칙

- 사용자 표시 텍스트에는 내부 id를 직접 노출하지 않는다.
- 한글 UI는 NotoSansKR 적용 구조를 유지한다.
- ScrollView는 마지막 항목이 잘리지 않도록 여백을 둔다.
- 팝업은 `PopupLayer` 중앙 배치를 기본으로 한다.
