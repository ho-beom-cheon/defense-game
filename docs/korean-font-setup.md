# Korean Font Setup

Unity IMGUI 기본 폰트는 한글 글리프를 포함하지 않을 수 있다. 이 경우 한글 문자열이 사각형이나 `??`로 표시된다. RuneGate Defense는 한국 출시 우선 방향이므로 모든 런타임 UI는 Noto Sans KR 기반 폰트를 사용한다.

## 사용 폰트

- `Assets/_Project/Fonts/NotoSansKR-Regular.ttf`
- `Assets/_Project/Fonts/NotoSansKR-SemiBold.ttf`
- `Assets/_Project/Fonts/NotoSansKR-Bold.ttf`

폰트 참조는 `Assets/_Project/Resources/KoreanFontCatalog.asset`에 저장한다. 빌드에서도 런타임 UI가 폰트를 찾을 수 있도록 Resources catalog를 사용한다.

## IMGUI 적용 방식

현재 프로젝트 UI는 대부분 `OnGUI` 기반이다. 각 `OnGUI` 진입점에서 `KoreanFontManager.ApplyToGuiSkin()`을 호출해 `GUI.skin.font`, label, box, button, toggle, text field, text area, window style에 NotoSansKR-Regular를 적용한다.

버튼이나 강조 텍스트는 `KoreanFontManager.BoldFont` 또는 `KoreanFontManager.SemiBoldFont`를 사용할 수 있다. v0.8에서는 기본 가독성 복구를 우선해 Regular 중심으로 적용한다.

## UnityEngine.UI.Text 사용 시

새 Canvas/uGUI UI를 만들 경우 모든 `UnityEngine.UI.Text` 컴포넌트의 `font`를 `NotoSansKR-Regular.ttf`로 지정한다. `KoreanFontManager.ApplyToSceneText()`는 씬 안의 legacy Text 컴포넌트를 reflection으로 찾아 기본 한글 폰트를 적용할 수 있다.

## TextMeshPro 사용 시

현재 프로젝트 스크립트는 `TMP_Text` 또는 `TextMeshProUGUI`를 직접 사용하지 않는다. TMP UI를 추가한다면 NotoSansKR-Regular 기반 Dynamic TMP Font Asset을 생성해 기본 텍스트에 적용한다. 한글 글리프 수가 많으므로 Static Font Asset보다 Dynamic 방식을 우선한다.

## 폰트 문제와 데이터 손상 구분

- 폰트가 없으면 정상 한글 문자열도 `??` 또는 빈 사각형처럼 보일 수 있다.
- NotoSansKR 적용 후에도 `??`가 남으면 폰트가 아니라 데이터 문자열 자체가 이미 `??`로 저장된 상태일 가능성이 높다.
- 이미 `??`로 저장된 문자열은 인코딩 변환만으로 복구되지 않는다. 사람이 읽을 수 있는 실제 한글 표시 문자열로 다시 입력해야 한다.
- 새 데이터 작성 시 C# 파일, JSON, ScriptableObject YAML은 UTF-8로 저장한다.
- 표시용 한글 `displayName`, `displayNameKorean`, `descriptionKorean`과 내부용 `heroId`, `stageId`, `skillId`, `effectKey`는 분리해서 관리한다.

## 새 UI 규칙

- 한글이 들어가는 새 UI는 반드시 NotoSansKR 계열 폰트를 적용한다.
- ConceptSheets 이미지, RuntimePixel 스프라이트, 폰트 문제를 서로 혼동하지 않는다.
- IMGUI 프로토타입에서는 `KoreanFontManager.ApplyToGuiSkin()` 호출을 유지한다.
- `??`가 보이면 먼저 폰트 적용 여부를 확인하고, 그 다음 데이터 파일에 `??`가 저장되어 있는지 검색한다.
