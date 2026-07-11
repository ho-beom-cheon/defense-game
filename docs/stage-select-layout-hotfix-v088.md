# Stage Select Layout Hotfix v0.88

## 목적

StageSelectScene이 모바일 세로 화면을 기준으로 동작하되, Unity Editor의 `Free Aspect` 또는 가로 화면에서도 주요 패널이 겹치지 않게 한다.

이번 수정은 기존 IMGUI 기반 화면을 완전히 갈아엎지 않고, 공통 프레임 계산과 StageSelect 전용 Rect 계산을 안정화하는 데 집중한다.

## 기준 해상도

- 기본 기준: 1080 x 1920 Portrait
- 보조 확인: 720 x 1280 Portrait
- 에디터 확인: Free Aspect Landscape

## 변경 기준

- StageSelect 본문에는 스테이지 목록과 상세 패널만 둔다.
- 난이도 선택은 별도 행을 만들지 않고 헤더 오른쪽 버튼으로 순환 선택한다.
- 그림자 계약은 별도 하단 요약 행을 만들지 않고 헤더 버튼과 중앙 팝업으로 접근한다.
- 스테이지 목록과 상세 패널은 같은 본문 영역을 나누어 사용하며 서로 겹치지 않는다.
- 스크롤이 필요한 영역은 목록/상세/팝업 내부로 제한한다.
- 스테이지 데이터는 `Resources/RuntimeContentCatalog.asset`을 우선 사용하고, 씬에 직접 연결된 목록은 fallback으로만 사용한다.
- 스테이지 표시 순서는 StageId 숫자 기준으로 정렬해 `1, 10, 2` 순서 오류를 막는다.

## 기대 결과

- StageSelectScene에서 스테이지 목록과 오른쪽 상세 패널이 겹치지 않는다.
- Free Aspect 가로 화면에서도 상세 패널이 화면 밖으로 밀리지 않는다.
- 난이도 영역과 그림자 계약 영역이 본문/푸터를 밀어내지 않는다.
- Stage 1 다음에 Stage 2가 표시되고, Stage 10이 중간에 끼지 않는다.

## 검증 방법

1. Unity에서 Play Mode를 완전히 종료한다.
2. 스크립트 컴파일과 도메인 리로드가 끝날 때까지 기다린다.
3. StageSelectScene을 다시 실행한다.
4. Game View를 `Free Aspect`와 1080 x 1920 Portrait 양쪽에서 확인한다.
5. 스테이지 목록, 상세 패널, 하단 버튼이 서로 겹치지 않는지 확인한다.
6. `Tools/RuneGate/Validate Project` 또는 `Tools/RuneGate/Validate Game Frame`을 실행한다.

## 주의

- Play Mode에 들어간 상태에서 스크립트를 수정하면, 에디터 설정에 따라 화면이 이전 코드로 보일 수 있다.
- 레이아웃이 똑같이 보이면 Play Mode를 끄고 스크립트 리로드가 끝난 뒤 다시 실행한다.
- 현재 주요 화면은 아직 IMGUI 기반이다. 최종 모바일 UI는 Canvas/RectTransform 기반 prefab으로 전환하는 것이 더 안정적이다.
