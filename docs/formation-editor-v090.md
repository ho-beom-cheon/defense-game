# v0.9 영웅 편성 편집

## 범위

StageSelectScene에서 영웅 6인을 3라인 x 3슬롯에 배치하는 편성 편집 기능을 제공한다. 슬롯 열은 후열, 중열, 전열 순서로 표시하며 저장과 전투 배치는 기존 `FormationSlot`, `SaveManager`, `HeroPlacementManager` 구조를 사용한다.

이번 단계에는 드래그 앤 드롭을 넣지 않았다. 영웅을 선택한 뒤 슬롯을 누르는 모바일 터치 방식을 사용한다.

## 사용 방법

1. TitleScene에서 새 게임 또는 이어하기로 StageSelectScene에 진입한다.
2. 하단의 `편성` 버튼을 누른다.
3. 문지기 목록에서 영웅을 선택한다.
4. 라인 1~3의 후열, 중열, 전열 슬롯 중 하나를 누른다.
5. 이미 배치된 영웅을 다른 점유 슬롯으로 옮기면 두 영웅의 위치가 교환된다.
6. `선택 영웅 빼기`로 선택 영웅을 편성에서 제외할 수 있다. 최소 한 명은 유지한다.
7. `기본 편성 복구`로 초기 6인 편성을 다시 적용한다.
8. 변경은 즉시 로컬 JSON에 저장되며 다음 BattleScene 진입부터 적용된다.

## 데이터 규칙

- 같은 영웅은 한 번만 편성할 수 있다.
- 같은 라인과 위치 슬롯에는 한 명만 배치할 수 있다.
- 라인은 0~2, 위치는 `Front`, `Middle`, `Back` 내부 값을 유지한다.
- 사용자 화면에는 영웅 id와 enum 값을 직접 표시하지 않는다.
- 저장 데이터는 원본 ScriptableObject를 변경하지 않고 `FormationSlot` 복사본으로 관리한다.

## 구현 구조

- `FormationEditorState`: 선택, 배치, 이동, 교환, 제외 규칙을 담당하는 런타임 상태 모델
- `StageSelectUI`: 모바일 편성 팝업과 즉시 저장을 담당
- `GameTextMapper`: 영웅 역할 및 전열/중열/후열 표시명을 제공
- `SaveManager.SetFormationSlots`: JSON 저장 경계
- `HeroPlacementManager`: 저장된 편성을 BattleScene 영웅 배치로 변환

## 검증 결과

- Unity 6000.4.11f1 배치 모드 Project Validator 통과
- Progression Smoke Test 통과
- Android APK 빌드 통과
- Android 15 API 35 에뮬레이터 1080x2400에서 시스템 흐름 통과
- 자동 시스템 흐름에서 6인 슬롯 교환, JSON 재로드, BattleScene 위치 반영 확인

일반 실행 ADB 화면 캡처는 화면 전환 뒤 검은 프레임을 반환해 편성 팝업의 최종 시각 배치는 수동 Game View 또는 실기기에서 추가 확인해야 한다.

## 수동 확인

1. `Tools/RuneGate/Validate Project`를 실행한다.
2. TitleScene에서 StageSelectScene으로 이동한다.
3. `편성`을 눌러 6인 목록과 3x3 슬롯이 화면 안에 표시되는지 확인한다.
4. 영웅 두 명의 슬롯을 교환하고 팝업을 닫았다 다시 열어 유지되는지 확인한다.
5. 앱을 종료 후 다시 실행해 편성이 유지되는지 확인한다.
6. Stage 1을 시작해 영웅 수, 라인, 전열/중열/후열 위치가 저장값과 일치하는지 확인한다.
7. 최소 한 명만 남았을 때 마지막 영웅 제외가 차단되는지 확인한다.
