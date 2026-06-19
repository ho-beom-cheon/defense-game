# Popup Layout

## 원칙

팝업은 `PopupLayer` 위에 중앙 정렬한다. 화면 크기에 따라 `preferredWidth`, `preferredHeight`, `widthRatio`, `heightRatio`를 적용하고 safe rect 안으로 clamp한다.

## 적용 대상

- RuneSelectionPopup
- ResultPopup
- PetContractPopup

## 현재 구현

- `UIPopupGuiUtility.PopupRect()`는 `GameFrameLayout.PopupFrame()`을 호출한다.
- Rune Selection과 Result는 dim overlay를 먼저 그리고 중앙 팝업을 표시한다.
- effectKey, stageId 같은 내부 값은 일반 사용자 UI에 표시하지 않는다.

## 팝업 크기 기준

- RuneSelectionPopup: 최대 화면 폭 92%, 높이 78%
- ResultPopup: 최대 화면 폭 92%, 높이 78%
- 긴 내용은 ScrollView로 보호한다.

## 향후 Canvas 전환 기준

Canvas prefab으로 전환할 경우에도 `PopupLayer` 하위에 팝업 prefab을 생성한다. `PopupFrameController`는 RectTransform 크기와 위치를 safe area 기준으로 보정하는 용도로 사용한다.
