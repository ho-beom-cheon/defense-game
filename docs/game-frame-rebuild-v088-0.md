# v0.88.0 Game Frame Rebuild

## 목적

v0.88 전투 폴리싱 전에 StageSelect, Battle, Rune Selection, Result 화면의 기본 프레임을 다시 잡았다.

기존 UI를 단순 미세 수정한 것이 아니라 StageSelectFrame / BattleFrame / PopupLayer 구조로 재구성했습니다.

## 적용 범위

- StageSelectScene 런타임 UI는 `StageSelectFrame` 기준으로 HeaderArea, DifficultyArea, StageListPanel, StageDetailPanel, PetContractArea, FooterArea로 분리한다.
- BattleScene 런타임 UI는 `BattleFrame` 기준으로 HeaderArea, BattleFieldFrame, SkillPanelArea, FooterArea, PopupLayer를 사용한다.
- Rune Selection과 Result는 `PopupLayer` 위 중앙 팝업으로 표시한다.
- 실제 씬 프리팹 대규모 재배치는 하지 않았다. 현재 프로젝트 UI가 IMGUI 기반이므로, 공통 Rect 계산과 검증 도구를 먼저 고정했다.

## 핵심 파일

- `Assets/_Project/Scripts/UI/Foundation/GameFrameLayout.cs`
- `Assets/_Project/Scripts/UI/Foundation/FrameRootLimiter.cs`
- `Assets/_Project/Scripts/UI/Foundation/ResponsiveLayoutSwitcher.cs`
- `Assets/_Project/Scripts/UI/Foundation/PopupFrameController.cs`
- `Assets/_Project/Editor/UIFrameValidator.cs`

## 검증 메뉴

Unity Editor에서 `Tools/RuneGate/Validate Game Frame`을 실행하면 다음을 확인하고 `docs/ui-frame-validation-report-v088-0.md`를 갱신한다.

- 필수 Scene 존재 여부
- 필수 UI foundation script 존재 여부
- FrameRoot, StageSelectFrame, BattleFrame, PopupLayer 용어 반영 여부
- Scene YAML 기준 Canvas/EventSystem 중복 가능성
- 사용자 노출 위험 문자열 `??`, 내부 id, 개발용 상태값 잔존 여부

## 주의

- 이 작업은 전직, 랜덤 몬스터, 신규 캐릭터, 신규 이미지, 신규 AI 시스템을 추가하지 않는다.
- RuntimePixel / ConceptSheets 정책은 유지한다.
- 한글 폰트 구조는 변경하지 않는다.
