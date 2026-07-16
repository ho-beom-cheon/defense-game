# UI 프레임 v2 및 연속형 전장 설계

## 0. 문서 상태

- 관련 이슈: `#103 설계: UI 프레임 v2와 연속형 전장 구조 확정`
- 기준 브랜치: `origin/main`의 PR #102 병합 이후
- 적용 대상:
  - 화면 프레임 1차 대상: `TitleScene`
  - 전투 시스템: 공용 `BattleScene`을 사용하는 Stage 1~10
  - 연속형 바닥·배경 아트 1차 대상: Stage 1
- 이 문서는 다음 두 문제의 구현 기준을 함께 정의한다.
  - 9-slice 프레임, 제목, 버튼, 푸터가 해상도마다 깨지는 화면 UI
  - 영웅과 몬스터가 세 개의 고정 Y 좌표에서만 싸우는 전투 구조

이번 변경은 단순한 수치 조정이 아니다. UI는 IMGUI 배치 방식에서 uGUI 프레임 계약으로, 전투는 `laneIndex == laneIndex` 규칙에서 연속형 2D 공간 규칙으로 책임을 바꿔야 한다.

## 1. 결정 요약

### 1.1 UI

- `BattleCanvasController`의 uGUI 구조를 화면 프레임의 기준 구현으로 사용한다.
- `TitleUI` 클래스명과 진행·저장 흐름은 유지하되 활성 `OnGUI` 렌더링을 제거한다.
- 패널과 버튼은 `Sprite`를 직접 사용하는 `Image.Type.Sliced`로만 그린다.
- 공통 숫자 토큰은 공유하지만 화면별 계산기는 `TitleCanvasLayout`, `BattleCanvasLayout`처럼 분리한다.
- 실제 런타임과 Validator가 같은 레이아웃 계산 결과를 검사하게 한다.
- `TitleScene`에는 Canvas와 EventSystem을 정확히 하나씩 둔다.

### 1.2 전투

- 편성의 3×3 슬롯은 유지하지만 전투 중에는 고정 레인으로 사용하지 않는다.
- `FormationSlot.laneIndex`는 시작 위치의 상·중·하 구역을 정하는 호환 데이터로만 사용한다.
- `WaveSpawnData.laneIndex`는 적의 출현 높이에 편향을 주는 호환 데이터로만 사용한다.
- 이동, 공격 거리, 타기팅, 가로막기, 분리는 모두 2D 좌표와 반경으로 계산한다.
- 몬스터는 수정체 주변의 7개 접근 지점을 나누어 사용하고, 영웅은 역할별 활동 범위 안에서 위아래로 이동해 이를 가로막는다.
- NavMesh, A*, 장애물 경로 탐색은 추가하지 않는다. 장식용 전장 오브젝트에는 이동 충돌을 두지 않고 직접 조향 방식으로 해결한다.
- 기존 캐릭터 수치, 스킬 수치, 웨이브 데이터, 저장 JSON 구조는 변경하지 않는다.

## 2. 기존 문서와의 우선순위

문서 간 기준이 충돌할 때 다음 우선순위를 적용한다.

1. 이 문서
2. `docs/battle-ugui-art-direction-v1.md`
3. `docs/stage1-battlefield-art-cohesion-design-v1.md`
4. `docs/ui-layout-foundation.md`
5. `docs/battlefield-frame.md`
6. `docs/game-frame-rebuild-v088-0.md`

기존 문서에서 유지하는 내용:

- `봉문 전술 기록서` UI 색상, 폰트, 9-slice 자산 기준
- 캐릭터를 기준으로 한 Stage 1 팔레트, 조명, 명암, 접지 그림자
- `BattlefieldViewport`와 월드 카메라를 연결하는 구조
- 수정체 판정 Transform과 시각 Transform의 분리
- 전투 밸런스와 저장 데이터 비변경 원칙

이 문서가 대체하는 내용:

- IMGUI Rect를 최종 화면 프레임으로 사용하는 방식
- 세 개의 레인 스프라이트와 고정 `laneY`
- 같은 `laneIndex`끼리만 공격하거나 가로막는 규칙
- X축 거리만 사용하는 이동·공격 판정
- 수정체 목표점을 레인별 한 점으로 두는 방식

## 3. 현재 문제의 직접 원인

### 3.1 화면 프레임

| 원인 | 현재 구현 | 결과 |
| --- | --- | --- |
| 9-slice 정보 유실 | `RuntimePixelGuiUtility`가 `Sprite`를 `Texture2D`로 바꾸고 `Sprite.border`를 전달하지 않음 | Sprite의 24px border 대신 fallback GUIStyle border 또는 단순 stretch가 적용되어 황동 모서리와 청색 내부선이 찌그러짐 |
| 장식 안전영역 부족 | 24px 패널 border에 비해 콘텐츠 inset이 16~32px | 제목과 푸터가 장식선을 침범함 |
| 흐름형 자동 여백 | `GUILayout.FlexibleSpace()` 뒤에 CTA, 보조 버튼, 상태 문구 배치 | 낮은 화면에서 하단 요소가 테두리까지 밀림 |
| 상태 문구 위치 | `로컬 저장 · 오프라인 플레이`가 패널 내부 마지막 항목 | 패널 하단 프레임과 시각적으로 충돌함 |
| 이중 레이아웃 기준 | 런타임은 `TitleUI.CalculateLayoutForSize`, Validator는 `GameFrameLayout.TitleFrameForSize` 사용 | 검사 결과와 실제 화면이 다름 |
| 잘못된 성공 조건 | TitleScene의 Canvas/EventSystem 개수를 `1개 이하`로 검사 | 0개인 IMGUI 화면도 통과함 |
| safe area 미반영 | 타이틀 계산이 실제 `Screen.safeArea`가 아닌 화면 전체 기반 | 노치와 시스템 영역에서 안정성을 보장하지 못함 |

### 3.2 고정 레인 전투

현재 `LaneManager`는 좌표 제공을 넘어 다음 규칙의 중심이 되어 있다.

- 영웅·몬스터의 Y 좌표를 매 프레임 `GetLaneY()`로 고정
- `UnitMovementController.MoveToX()`가 X만 이동하고 Y를 다시 레인 값으로 덮어씀
- 영웅의 잠금 대상 유효성에서 다른 `LaneIndex`를 거부
- 몬스터가 같은 `LaneIndex`의 영웅만 가로막는 대상으로 선택
- 아군·적군 분리가 같은 레인 안의 X 간격만 검사
- 임시 포탑과 일부 보스·증원 규칙이 `LaneIndex`를 전투 판정으로 사용
- 수정체 공격 목표가 레인별 한 점이라 적이 세 줄로 수렴

이 구조에서는 배경을 넓혀도 유닛은 세 개의 선 위에서만 움직이므로 전장 전체를 사용하는 인상을 만들 수 없다.

## 4. 공통 UI 프레임 v2

### 4.1 책임

공통화하는 것은 색상, 간격, 최소 터치 크기, safe area 처리, 9-slice 계약이다. 모든 화면을 하나의 거대한 레이아웃 계산기로 합치지 않는다.

신규 공통 계약:

- `UiFrameTokens`
  - 간격, border, 콘텐츠 inset, 최소 터치 높이
- `SafeAreaFitter`
  - 실제 safe area를 Canvas 로컬 좌표로 변환
- `RuneGateUiTheme`
  - 색상, TMP 폰트, 패널·버튼·카드 스프라이트, 모션 시간
- 화면별 순수 계산기
  - `TitleCanvasLayout`
  - 기존 `BattleCanvasLayout`
- 화면별 Controller
  - `TitleUI`
  - 기존 `BattleCanvasController`

### 4.2 공통 토큰과 화면별 토큰

모든 uGUI 화면이 공유하는 primitive:

| 토큰 | 공통 기준값 |
| --- | ---: |
| 기준 해상도 | 1080×1920 |
| Canvas Scaler | Scale With Screen Size |
| Match | 0.5 |
| 간격 | 8 / 16 / 24 / 32 / 48 |
| 패널 sprite border | 24 |
| 버튼 sprite border | 12 |
| 카드 sprite border | 24 |
| 기준 최소 터치 높이 | 88 Canvas unit |

TitleScene 전용:

| 토큰 | Title 기준값 |
| --- | ---: |
| 패널 콘텐츠 inset | 좌우 48, 상하 40~48 |
| 장식선 추가 안전거리 | border 안쪽에서 최소 16 |
| 화면 가장자리 최소 여백 | 40 |
| 최대 일반 프레임 너비 | 920 |
| 최대 확인 모달 너비 | 760 |

9-slice 패널의 실제 콘텐츠 시작점은 최소 `sprite border + 16`이어야 한다. 패널 원본의 장식선이 24px border보다 안쪽에 있으면 해당 자산에 한해 inset을 48px로 올린다.

기존 BattleCanvas는 최대 root 너비 1080, 내부 여백 8~18, 카드 gap 12를 유지한다. Title 전용 `40/48/920`을 BattleCanvas Validator에 적용하지 않는다.

### 4.3 금지 규칙

- 패널 `Sprite`에서 `.texture`만 꺼내 배경으로 사용하지 않는다.
- 최종 화면에서 `GUI.Box`, `GUILayout`, `OnGUI`를 레이아웃 시스템으로 사용하지 않는다.
- Screen 픽셀 값을 Canvas 로컬 좌표에 그대로 넣지 않는다.
- 텍스트, 버튼, 상태 문구를 프레임 장식 영역에 겹쳐 배치하지 않는다.
- `FlexibleSpace`만으로 주요 CTA 위치를 결정하지 않는다.
- Canvas 또는 EventSystem이 0개여도 성공으로 처리하지 않는다.

### 4.4 uGUI 전환 완료 씬의 공통 규칙

- 대상은 이번 범위의 TitleScene과 기존 uGUI BattleScene이다.
- 대상 화면당 root Canvas 정확히 1개
- 대상 화면당 EventSystem 정확히 1개
- `CanvasScaler.referenceResolution = 1080×1920`
- `CanvasScaler.matchWidthOrHeight = 0.5`
- 배경은 safe area 밖까지 채움
- 조작 콘텐츠와 모달은 safe area 안에 배치
- 레이아웃 재계산은 safe area 크기, Canvas scale factor 또는 화면 모드가 바뀔 때만 수행
- 아직 IMGUI를 유지하는 StageSelectScene과 UpgradeScene에는 이 Canvas 개수 조건을 적용하지 않는다.

## 5. TitleScene 기준 설계

### 5.1 계층

```text
TitleCanvas
├─ BackgroundLayer
├─ ContentSafeAreaRoot
│  └─ FrameRoot
│     ├─ BrandArea
│     ├─ MenuPanel                     Image.Type.Sliced
│     │  ├─ HeaderArea
│     │  ├─ BodyArea
│     │  └─ ActionArea
│     └─ StatusFooter                  패널 바깥
└─ OverlayLayer
   ├─ Dim
   └─ OverlaySafeAreaRoot
      └─ ModalPanel                    Image.Type.Sliced
```

`StatusFooter`를 패널 밖으로 분리해 하단 장식선과 `로컬 저장 · 오프라인 플레이`가 경쟁하지 않게 한다.

### 5.2 세로 기준

| 영역 | 1080×1920 기준 |
| --- | --- |
| FrameRoot | safe area에서 edge margin을 뺀 영역 |
| BrandArea | 높이 260~340 |
| Main MenuPanel | 권장 920×640, 정상 최소 높이 560 |
| Settings MenuPanel | 권장 920×880, 정상 최소 높이 640 |
| Panel HeaderArea | 높이 72 |
| Panel BodyArea | 남은 높이, 설정 화면은 ScrollRect |
| Panel ActionArea | 메인 `max(224, touch×2+16)`, 설정 `max(104, touch)` |
| StatusFooter | 높이 40~48 |
| 확인 Modal | 최대 760×430 |
| 패널과 Footer 간격 | 16 |

메인 패널은 `Header / Body / Action`의 고정 구역으로 나눈다. 본문이 짧아도 ActionArea의 위치는 변하지 않으며, 본문이 길어지면 BodyArea만 축소하거나 스크롤한다.

결정 순서:

1. safe area 로컬 너비가 800 이상이면 edge 40, 미만이면 24를 사용한다.
2. 패널 너비는 `min(920, safeWidth - edge×2)`다.
3. `effectiveTouchHeight = max(88, 56 / canvasScaleFactor)`로 계산한다.
4. Main은 640, Settings는 880에서 시작해 available height에 맞춰 정상 최소 높이까지 줄인다.
5. BrandArea는 남은 높이에서 340부터 160까지 줄인다.
6. 그래도 부족하면 compact mode로 전환해 StatusFooter를 숨기고 BodyArea를 ScrollRect로 보호한다.
7. 패널은 safe area 하단에, BrandArea는 패널 위 남은 공간의 중앙에 배치한다.
8. MenuPanel 내부에서 48 inset을 적용한 뒤 Header 72와 ActionArea를 먼저 고정하고 BodyArea가 나머지를 사용한다.

### 5.3 가로 fallback

- 가로 전환 조건은 `safeWidth / safeHeight >= 1.15`
- `FrameRoot`는 safe area에서 edge 40을 뺀 뒤 최대 1520×820
- BrandArea 44%, 메뉴 영역 56%
- 메뉴 최대 너비 720
- 브랜드와 메뉴 사이 간격 32
- 메뉴 패널 높이는 Main 최대 640, Settings 최대 720
- `panelHeight + footerHeight + 16`을 확보하지 못하면 StatusFooter를 숨김
- 높이가 부족하면 BodyArea를 ScrollRect로 전환하고 Header와 ActionArea를 먼저 보존
- 버튼은 Canvas 기준 88 이상, screen-space 기준 56px 이상을 모두 만족

### 5.4 순수 레이아웃 결과

```csharp
public readonly struct TitleCanvasRects
{
    public Rect SafeArea { get; }
    public Rect FrameRoot { get; }
    public Rect BrandArea { get; }
    public Rect MenuPanel { get; }
    public Rect HeaderArea { get; }
    public Rect BodyArea { get; }
    public Rect ActionArea { get; }
    public Rect StatusFooter { get; }
    public Rect ModalPanel { get; }
    public bool FooterVisible { get; }
    public bool BodyScrollRequired { get; }
}
```

모든 Rect는 같은 Canvas 로컬 좌표계를 사용한다. `StatusFooter`가 숨겨진 경우 높이 0의 Rect를 반환한다.

### 5.5 상태 모델

```csharp
public enum TitleViewMode
{
    Main,
    Settings
}

public enum TitleConfirmAction
{
    None,
    NewGame,
    ResetSave
}

public readonly struct TitleViewData
{
    public bool HasExistingProgress { get; }
    public string ProgressSummary { get; }
    public string PrimaryActionLabel { get; }
    public string FeedbackMessage { get; }
}
```

`TitleUI`는 클래스명과 다음 정적 메서드를 유지한다.

- `HasMeaningfulProgress(SaveData)`
- `BuildProgressSummary(SaveData)`
- `PrimaryActionLabel(bool)`

추가 공개 인터페이스:

```csharp
public bool IsReady { get; }
public TitleViewMode ViewMode { get; }
public TitleConfirmAction ConfirmAction { get; }
public TitleViewData CurrentViewData { get; }

public void AssignTheme(RuneGateUiTheme theme);
public void Refresh();
public void ShowMain();
public void ShowSettings();
public void ShowConfirmation(TitleConfirmAction action);
public void RebuildView();
```

순수 레이아웃 계약:

```csharp
public static TitleCanvasRects Calculate(
    Vector2 safeSize,
    float canvasScaleFactor,
    TitleViewMode mode);
```

`TitleUI`에서 `OnGUI()`와 `drawRuntimeGui` 필드를 제거한다. 필요한 레거시 IMGUI는 별도 컴포넌트로 분리하되 TitleScene과 Title prefab에는 부착하지 않는다. Bootstrapper는 더 이상 존재하지 않는 `panelRect`를 직렬화하지 않는다.

## 6. 연속형 전장 공간

### 6.1 좌표 정의

전투 공간은 카메라 월드 bounds에서 UI와 오브젝트 여백을 제외한 `PlayableBounds`로 정의한다.

- 정규화 X `u`
  - `0`: 수정체 쪽
  - `1`: 균열 쪽
- 정규화 Y `v`
  - `0`: 화면 아래
  - `1`: 화면 위

```text
v=1.0 ┌────────────────────────────────────────────┐
      │       상단 활동 공간                       │
      │  영웅 ↕ 이동       적 진행 ↙              │
      │                                            │
      │ [수정체]  7개 접근 지점      [균열 출현부] │
      │                                            │
      │  영웅 ↕ 이동       적 진행 ↖              │
      │       하단 활동 공간                       │
v=0.0 └────────────────────────────────────────────┘
      u=0                                      u=1
```

전장 아트에는 명시적인 세 줄을 그리지 않는다. 편성 룬은 전투 시작 전, 튜토리얼, 편성 확인 상태에서만 보인다.

### 6.2 런타임 계층

```text
BattlefieldRuntime
├─ BattleManager
├─ BattlefieldLayoutCoordinator
│  ├─ BattlefieldSpaceController
│  ├─ BattlefieldAgentRegistry
│  └─ CrystalApproachPointProvider
├─ LaneManager                         호환 어댑터
├─ HeroPlacementManager
├─ WaveManager
└─ BattlefieldArtRoot
   └─ BattlefieldVisualController
```

책임:

| 컴포넌트 | 책임 |
| --- | --- |
| `BattleManager` | 새 공간 서비스가 모두 준비된 뒤 영웅 편성과 첫 웨이브 시작 |
| `BattlefieldLayoutCoordinator` | 카메라, 공간, Agent remap, 접근 지점, 시각 갱신 순서를 단일 체인으로 보장 |
| `BattlefieldSpaceController` | 현재 PlayableBounds, 정규화 좌표 변환, 위치 clamp, 레이아웃 변경 |
| `BattlefieldAgentRegistry` | 살아 있는 전투 유닛 등록, 타깃·이웃의 재사용 쿼리 |
| `CrystalApproachPointProvider` | 수정체 접근 지점 예약·해제, 보스 다중 슬롯 예약 |
| `LaneManager` | 기존 3×3 편성과 스폰 band를 새 공간으로 변환하는 호환 API |
| `UnitMovementController` | 2D 속도, 가속·감속, 정지 거리, clamp 적용 |
| `BattlefieldVisualController` | 배경·바닥·목표물·데칼·그림자 표현 |

공간 기반과 AI를 여러 이슈로 나누어도 하나의 코드 경로를 유지하기 위해 마이그레이션 동안에만 다음 모드를 둔다.

```csharp
public enum BattlefieldMode
{
    LegacyLanes,
    Continuous2D
}
```

- 저장 데이터에는 기록하지 않는다.
- 공간 기반 이슈에서는 기존 AI를 살리기 위한 임시 실행 모드로만 사용한다.
- AI 전환 이슈가 끝나면 BattleScene 기본값은 `Continuous2D`여야 한다.
- 최종 Validator는 출시 BattleScene에서 `LegacyLanes`가 활성화되어 있으면 실패한다.
- `LegacyLanes`를 별도의 장기 유지 AI 경로로 복제하지 않는다.

`BattlefieldSpaceConfig`는 ScriptableObject로 만들고
`Assets/_Project/Data/Battlefield/DefaultBattlefieldSpaceConfig.asset`을 BattleScene의 필수 자산으로 사용한다.

저장 항목:

- 최소 월드 폭·높이
- PlayableBounds padding
- 편성 Anchor 정규화 값
- spawn band 범위
- 역할별 leash
- 접근 지점 정규화 값
- 분리 가중치와 target lock slack

Config가 누락되면 임의 기본값으로 전투를 시작하지 않는다. Bootstrapper와 Validator가 필수 참조를 보장하고 `BattleManager`는 `Space / Registry / ApproachProvider / Visual`의 `IsReady`를 확인한 뒤 전투를 시작한다. `LaneManager.Awake()`는 더 이상 카메라 최소 월드 크기를 소유하지 않는다.

전환 후 초기화 계약:

```csharp
waveManager.Initialize(
    stageData,
    battlefieldSpace,
    approachProvider,
    agentRegistry,
    crystalController);

heroPlacementManager.BuildRuntimeFormation(
    battlefieldSpace,
    agentRegistry);
```

기존 LaneManager 인자를 받는 overload는 stacked 구현 중 compile-safe를 위한 wrapper로만 유지한다. `BattleManager.Start()`는 새 의존성이 준비되지 않았으면 자동 전투를 시작하지 않고 명확한 오류를 남긴다.

### 6.3 핵심 공개 인터페이스

```csharp
public readonly struct BattlefieldBounds
{
    public Rect PlayableRect { get; }
    public Rect HeroHomeRect { get; }
    public Rect RiftSpawnRect { get; }

    public Vector2 Clamp(Vector2 position, Vector2 halfExtents);
    public Vector2 ToWorld(Vector2 normalized);
    public Vector2 ToNormalized(Vector2 world);
}

public sealed class BattlefieldSpaceController : MonoBehaviour
{
    public bool IsReady { get; }
    public BattlefieldBounds CurrentBounds { get; }

    public event Action<BattlefieldBounds, BattlefieldBounds> LayoutChanged;

    public void Configure(
        BattlefieldCameraFitter cameraFitter,
        CrystalController crystalController,
        BattlefieldSpaceConfig config);
    public void RefreshLayout();
    public Vector2 ResolveFormationAnchor(FormationSlot slot);
    public Vector2 ResolveEnemySpawn(
        int spawnBandIndex,
        int waveNumber,
        int groupIndex,
        int spawnOrdinal,
        Vector2 halfExtents);
    public Vector2 Clamp(Vector2 position, Vector2 halfExtents);
}

public sealed class BattlefieldCameraFitter : MonoBehaviour
{
    public Bounds CurrentWorldBounds { get; }
    public event Action<Bounds> WorldBoundsChanged;
}

public sealed class BattlefieldLayoutCoordinator : MonoBehaviour
{
    public bool IsReady { get; }

    public void Configure(
        BattlefieldCameraFitter cameraFitter,
        BattlefieldSpaceController space,
        BattlefieldAgentRegistry registry,
        CrystalApproachPointProvider approachProvider,
        BattlefieldVisualController visuals);
    public void RefreshLayout();
}

public sealed class UnitMovementController : MonoBehaviour
{
    public float CurrentVelocity { get; }
    public Vector2 CurrentVelocity2D { get; }
    public Vector2 DesiredPosition { get; }

    public void Configure(
        float nextMoveSpeed,
        float nextAttackRange,
        float nextPersonalSpace,
        float nextLeashRange);
    public void SetMotionTuning(
        float nextAcceleration,
        float nextDeceleration,
        float nextStoppingDistance);
    public void SetAttackState(bool value);
    public void SetDeadState(bool value);
    public bool MoveTo(
        Vector2 target,
        BattlefieldBounds bounds,
        Vector2 halfExtents,
        Vector2 steeringOffset);
    public void Stop();
}
```

기존 `CurrentVelocity`는 `CurrentVelocity2D.magnitude`를 반환해 공개 타입을 보존한다. 마이그레이션 중에는 기존 `MoveToX()`를 유지할 수 있지만 새 Hero, Monster, Skill, Wave 런타임 코드는 호출하지 않는다. 최종 시각 clamp에는 기존 `RuntimeSpriteBoundsUtility.ClampRootInsideBounds()`를 함께 사용한다.

### 6.4 전투 Agent

모든 영웅, 몬스터, 임시 포탑은 공간 쿼리에 필요한 공통 정보를 제공한다.

```csharp
public enum BattlefieldAgentKind
{
    Hero,
    GroundMonster,
    FlyingMonster,
    Boss,
    Deployable
}

public enum BattlefieldFaction
{
    Hero,
    Monster,
    Neutral
}

public sealed class BattlefieldAgent : MonoBehaviour
{
    public int StableId { get; }
    public BattlefieldFaction Faction { get; }
    public BattlefieldAgentKind Kind { get; }
    public float Radius { get; }
    public Vector2 HalfExtents { get; }
    public Vector2 Anchor { get; }
    public Rect LeashRect { get; }
    public float ObjectiveProgress { get; }

    public void Configure(
        BattlefieldAgentKind kind,
        BattlefieldFaction faction,
        int stableId,
        float radius,
        Vector2 halfExtents,
        Vector2 anchor,
        Rect leashRect);
}

public sealed class BattlefieldAgentRegistry : MonoBehaviour
{
    public bool IsReady { get; }

    public void Register(BattlefieldAgent agent);
    public void Unregister(BattlefieldAgent agent);
    public void FillNeighbors(
        BattlefieldAgent requester,
        float radius,
        List<BattlefieldAgent> results);
    public void FillAgentsInCircle(
        Vector2 center,
        float radius,
        BattlefieldFaction faction,
        List<BattlefieldAgent> results);
    public void FillAgentsInCapsule(
        Vector2 start,
        Vector2 end,
        float radius,
        BattlefieldFaction faction,
        List<BattlefieldAgent> results);
}
```

반경은 분리와 접촉에, HalfExtents는 화면 bounds clamp에 사용한다. Agent는 HeroController 또는 MonsterController 초기화가 끝난 뒤 Registry에 등록하고, 사망·제거·`OnDisable`에서 반드시 해제한다. 초기화 전 Agent는 쿼리 결과에 포함하지 않는다. 기존 `ActiveHeroes`와 `ActiveMonsters`는 마이그레이션 동안 Registry를 읽는 호환 wrapper로 유지한다.

1차 전환의 기본 공격, 직접 피해 스킬, 타깃 잠금은 기존 수치 의미를 보존하기 위해 `Vector2` 중심점 거리를 사용한다. 캐릭터 반경을 빼서 실질 사거리를 늘리는 변경은 이번 범위에 포함하지 않는다.

공통 기하와 타깃 쿼리는 Controller 안에 중복 구현하지 않는다.

```csharp
public static class CombatGeometry
{
    public static float CenterDistance(Vector2 a, Vector2 b);
    public static bool IsCenterInRange(
        Vector2 a,
        Vector2 b,
        float range);
    public static float ContactDistance(
        Vector2 a,
        float radiusA,
        Vector2 b,
        float radiusB);
}

public sealed class BattlefieldTargetQuery
{
    public MonsterController FindMonster(
        HeroController requester,
        float range,
        TargetingType targetingType);
    public HeroController FindInterceptorAhead(
        MonsterController requester,
        Vector2 destination,
        float lookAheadDistance);
    public HeroController FindAttackableBlocker(
        MonsterController requester,
        float attackRange);
    public void FillMonstersInRadius(
        Vector2 center,
        float radius,
        List<MonsterController> results);
}
```

### 6.5 편성 데이터 매핑

저장된 `FormationSlot`은 변경하지 않는다.

`laneIndex`의 의미:

| 기존 값 | 새 의미 | 정규화 v |
| ---: | --- | ---: |
| 0 | 하단 시작 구역 | 0.22 |
| 1 | 중앙 시작 구역 | 0.50 |
| 2 | 상단 시작 구역 | 0.78 |

`positionType`의 의미:

| 값 | 새 의미 | 정규화 u |
| --- | --- | ---: |
| Back | 수정체 인접 후방 | 0.20 |
| Middle | 중간 방어선 | 0.30 |
| Front | 전방 방어선 | 0.40 |

이 매핑으로 3×3의 9개 논리 조합은 서로 다른 시작 Anchor가 된다. 실제 저장 편성은 비어 있는 조합을 포함할 수 있다. 전투가 시작되면 Anchor는 복귀 기준이지 고정 좌표가 아니다.

편성 UI의 사용자 표시 문자열은 다음처럼 바꾼다.

- `3라인 전술 슬롯` → `3구역 전술 배치`
- `라인 1/2/3` → `하단/중앙/상단`

저장 키, JSON 필드, `FormationSlot` 생성자는 유지한다.

코드 가독성을 위해 `FormationRowIndex`와 `SpawnBandIndex` alias를 추가할 수 있지만 기존 `LaneIndex` 공개 프로퍼티는 호환 기간 동안 유지한다.

### 6.6 역할별 활동 범위

역할별 수치는 `BattlefieldSpaceConfig`에 저장하며 HeroData 원본을 변경하지 않는다.

| 역할 | 전방 허용 | 후방 허용 | 수직 허용 |
| --- | ---: | ---: | ---: |
| Tank / MeleeDps | +0.28W | -0.08W | ±0.24H |
| Assassin | +0.36W | -0.10W | ±0.32H |
| RangedDps / Mage | +0.18W | -0.12W | ±0.26H |
| Healer / Support / Engineer | +0.12W | -0.12W | ±0.22H |

영웅은 대상이 없으면 Anchor로 복귀한다. 대상이 있으면 역할별 LeashRect 안에서 공격 거리를 맞추며 X와 Y를 함께 이동한다.

### 6.7 적 출현 분산

`WaveSpawnData.laneIndex`는 삭제하거나 저장 포맷을 바꾸지 않고 출현 높이의 bias로 사용한다.

| legacy band | v 범위 |
| ---: | --- |
| 0 | 0.26~0.50 |
| 1 | 0.38~0.62 |
| 2 | 0.50~0.74 |

범위가 서로 겹치므로 같은 데이터라도 몬스터가 세 줄로 정렬되지 않는다. 출현부는 균열 시각 높이 안에 유지하고, 이후 7개 수정체 접근 지점으로 퍼지면서 전장 전체 높이를 사용한다.

출현 Y는 `waveNumber`, `groupIndex`, `spawnOrdinal`로 만든 고정 순서를 사용한다. `UnityEngine.Random`에 의존하지 않아 캡처와 E2E가 재현 가능해야 한다.

권장 순서는 band 반폭에 곱하는 정규화 계수다.

```text
0, +0.72, -0.72, +0.36, -0.36, +1.00, -1.00
```

최종 Y는 해당 band 범위에 clamp한다.

WaveManager는 `SpawnRoutine(spawnData, groupIndex, waveNumber)`로 그룹과 웨이브 번호를 전달한다. 보스 증원은 별도 reinforcement ordinal을 사용하며, 스폰 위치 계산에는 몬스터의 HalfExtents를 함께 전달한다.

균열 오브젝트는 하나를 유지한다. 몬스터는 균열 시각 높이 안에서 출현한 뒤 예약한 수정체 접근 지점을 향해 부채꼴로 퍼진다.

### 6.8 수정체 접근 지점

레인별 목표점 3개를 수정체 오른쪽의 접근 지점 7개로 교체한다.

정규화 Y:

```text
0.18 / 0.29 / 0.40 / 0.50 / 0.60 / 0.71 / 0.82
```

- 일반 몬스터: 비용이 가장 낮은 한 지점을 예약
- 비용: 현재 위치와의 거리 + 슬롯 혼잡도 + legacy band와의 높이 차
- 보스: 중앙을 포함한 인접 3개 지점을 예약
- 비행형: 지점은 예약하지만 지상 유닛 분리를 무시
- 사망·제거·Crystal 도달 시 즉시 예약 해제

공개 계약:

```csharp
public readonly struct BattlefieldApproachHandle
{
    public int ReservationId { get; }
    public int Generation { get; }
    public int PrimarySlotIndex { get; }
    public int OwnerStableId { get; }
    public bool IsValid { get; }
}

public sealed class CrystalApproachPointProvider : MonoBehaviour
{
    public BattlefieldApproachHandle Reserve(
        BattlefieldAgent agent,
        int spawnBandIndex);
    public bool TryGetPosition(
        BattlefieldApproachHandle handle,
        out Vector2 position);
    public void Release(BattlefieldApproachHandle handle);
    public void RefreshLayout();
}
```

접근 지점의 X는 위아래로 갈수록 조금 오른쪽에 놓아 수정체를 감싸는 완만한 호를 만든다. 모든 적이 한 점에 쌓이지 않고 전장의 세로 공간을 계속 사용하게 하는 장치다.

접근 지점은 수정체 본체에 직접 붙은 7개의 점처럼 노출하지 않는다. 수정체에서 위아래로 펼쳐지는 약한 `봉문 장벽`의 충돌 위치로 해석한다. 평상시에는 기존 룬·보호막 자산을 낮은 알파로 재사용하고, 몬스터가 공격할 때 해당 접근 위치에만 짧은 청색 피격 펄스를 표시해 수정체 HP가 감소하는 이유를 시각적으로 연결한다.

보스의 인접 3개 슬롯은 Provider 내부의 한 reservation record에 묶는다. 오래된 handle이 재사용된 슬롯을 해제하지 않도록 ReservationId, Generation, OwnerStableId를 모두 확인한다.

예약은 다음 모든 경로에서 중복 호출에 안전하게 해제되어야 한다.

- 사망 또는 수정체 도달
- `OnDisable`
- 웨이브 강제 종료
- 전투 재시작
- 생성 또는 초기화 실패

화면비 변경 후에는 reservation ID를 유지하고 월드 위치만 다시 계산한다. 다음 웨이브 시작 시 남은 reservation 수가 0인지 검증한다.

몬스터의 `ObjectiveProgress`는 스폰점 `S`, 예약 접근점 `A`, 현재 위치 `P`를 사용한다.

```text
progress = clamp01(dot(P - S, A - S) / |A - S|²)
```

따라서 서로 다른 접근점을 사용해도 0은 출현, 1은 봉문 장벽 도달이라는 같은 의미를 갖는다. 화면비 변경 때 S, A, P를 모두 정규화 좌표로 remap한 뒤 다시 계산한다.

수정체 도달은 `distance(monsterCenter, approachPoint) <= reachDistance + agentRadius`로 판정한다. 기존 `reachDistance` 값은 유지하며 Agent 반경은 목표물 접촉에만 사용한다.

### 6.9 이동과 분리

직접 조향식 이동:

```text
desired =
    seek(target)
  + separation(neighbors) × 0.35
  + anchorBias(hero only)
```

규칙:

- 방향 벡터를 normalize한 뒤 속도를 적용해 대각선 이동이 빨라지지 않게 함
- 현재 속도는 `Vector2.MoveTowards`로 가속·감속
- gameplay 이동은 `Time.deltaTime` 사용
- UI와 균열·수정체의 장식 모션은 기존처럼 `unscaled time` 사용
- 같은 진영의 지상 Agent끼리 원형 personal space 적용
- 분리 힘은 기본 이동 속도의 35% 이하로 제한
- 반대 진영은 물리 충돌로 밀어내지 않고 가로막기·전투 상태로 해결
- 비행형은 지상 분리를 무시하고 비행형끼리만 약한 분리를 적용
- decorative collider와 NavMesh는 사용하지 않음

현재 전투 규모에서는 재사용 리스트 기반 이웃 쿼리로 충분하다. 매 프레임 `OverlapCircleAll` 배열을 새로 만들지 않는다.

### 6.10 타기팅과 가로막기

영웅 타기팅:

- `Nearest`: 2D 중심점 거리가 가장 가까운 대상
- `First`: 수정체 접근 진행도가 가장 높은 대상
- `HighestHp`, `LowestHp`, `Boss`: 기존 우선순위 유지
- 대상 잠금은 `range + lockSlack` 안에서 유지
- 다른 `LaneIndex`라는 이유로 잠금이나 공격을 해제하지 않음

몬스터 가로막기:

- `FindInterceptorAhead`는 몬스터 현재 위치 앞의 제한된 look-ahead capsule에서 영웅을 찾아 이동 목적지만 영웅 쪽으로 조정
- `FindAttackableBlocker`는 실제 2D 중심점 근접 사거리 안의 영웅만 반환
- 후보가 여러 명이면 경로와의 수직 거리, 몬스터와의 중심점 거리 순으로 선택
- 실제 공격 가능 영웅이 없으면 이동을 멈추거나 공격 루틴을 시작하지 않음
- 공격 wind-up 종료 시 대상 생존 여부와 2D 사거리를 다시 확인
- 유효할 때만 기존 공격 주기와 피해량을 적용

이 방식은 영웅이 위아래로 움직여 적의 진행 경로를 실제로 가로막게 하면서, 뒤쪽 영웅을 부자연스럽게 공격하는 문제를 막는다.

### 6.11 스킬과 보스 규칙

- 범위 스킬은 이미 사용 중인 2D 반경 의미를 유지한다.
- 기본 공격과 직접 피해 스킬은 시전 시점과 impact 시점에 대상 생존과 2D 중심점 사거리를 다시 확인한다.
- 임시 포탑은 `LaneIndex` 필터를 제거하고 포탑 위치 기준 2D 원 안에서 타깃을 찾는다.
- 넉백은 고정 X 방향이 아니라 시전자에서 대상 방향으로 적용한다.
- 폭발 룬은 같은 레인 필터 대신 기존 SplashRadius의 2D 원을 사용한다.
- 연쇄 룬은 레인 제한을 제거하는 대신 기존 `LightningRange`와 같은 최대 연쇄 거리 3.5 world unit을 적용한다. 제한 없는 전장 전체 연쇄는 허용하지 않는다.
- 기존 보스의 레인 선택은 `하단/중앙/상단 BattlefieldBand` 선택으로 해석한다.
- 보스 광역 공격은 한 줄 판정이 아니라 해당 band의 Rect 또는 capsule을 사용한다.
- 보스 증원은 보스와 같은 레인을 복제하지 않고 인접 spawn bias를 순환한다.
- 룬과 펫 효과는 lane equality를 사용하지 않고 2D 거리 또는 Agent 종류를 사용한다.

`LaneIndex` 공개 프로퍼티는 E2E와 저장 호환을 위해 유지할 수 있지만, 전투 대상 자격 판정에 사용하면 안 된다.

### 6.12 깊이 정렬

연속형 Y 이동에서는 고정 sorting order를 사용할 수 없다.

`BattlefieldDepthSorter`는 화면 아래에 있는 유닛이 앞에 보이도록 정렬한다.

```text
sortingOrder =
    unitBaseOrder
  + round((bounds.maxY - position.y) × depthResolution)
```

- 그림자는 본체보다 1 낮음
- HP bar와 상태 표시는 본체의 상대 order를 따라감
- 임시 포탑과 fallback projectile도 위치 기반 order를 사용
- 월드 이펙트와 `CombatVisualEffectFactory`는 대상의 현재 order를 기준으로 앞뒤를 선택
- 보스의 추가 장식 레이어는 본체 상대 순서를 유지
- 같은 Y에서는 spawn sequence로 안정적인 tie-break 적용
- `CharacterVisualController`의 공격 lunge는 고정 X가 아니라 현재 대상 방향의 2D 벡터를 사용

DepthSorter는 본체, 그림자, HP bar, 배치물의 상대 offset을 한 묶음으로 갱신한다. 화면 아래 유닛의 order가 위 유닛보다 높아야 하며 그림자는 항상 본체보다 정확히 1 낮아야 한다.

## 7. Stage 1 전장 아트 확장

### 7.1 유지

다음 자산과 시각 기준은 그대로 사용한다.

- `bg_stage01_sealed_forest.png`
- 수정체 제단, 균열문
- 공통 접지 그림자
- 영웅 배치 룬
- 수정체 보호막 링, 균열 맥동
- 좌측 청색, 우측 보라색, 좌측 상단 달빛

### 7.2 변경

`ground_stage01_lane.png`를 세 줄로 반복하는 표현은 종료한다.

선택 기준:

1. 기존 자산을 2D 타일해도 세로 이음새가 보이지 않으면 `GroundFieldRenderer`에서 하나의 연속 바닥으로 사용
2. 세로 반복 흔적이 보이면 기존 팔레트를 재사용한 최소 파생 자산 `ground_stage01_field.png` 1종을 추가

`ground_stage01_field.png` 요구:

- 권장 1024×1024
- X/Y 양방향 반복 가능
- 중앙 전투 영역의 대비가 캐릭터보다 낮음
- 세 개의 평행한 밝은 길을 그리지 않음
- 깨진 석판, 흙, 이끼가 큰 덩어리로 이어짐
- Point, PPU 100, Compression None 또는 검증된 Low

`BattlefieldVisualController` 변경:

- `laneRenderers[]` → `groundFieldRenderer` 1개
- `ApplyLaneLayout()` → `ApplyGroundFieldLayout()`
- `slotRuneRenderers[]`는 새 Formation Anchor 위치를 사용
- `Configure(BattlefieldArtTheme, BattlefieldSpaceController, CrystalController)` 추가
- `LaneManager` 기반 Configure는 마이그레이션 기간에만 호환용으로 유지
- `IsReady`는 lane renderer 개수가 아니라 GroundField, 배경, 수정체, 균열, Space 준비 상태를 검사
- WaveManager는 `laneManager.GetComponent<WaveManager>()`로 찾지 않고 Configure 또는 직렬화 참조로 받음

연결 변경:

- `BattlefieldArtTheme.HasRequiredAssets`는 Lane 대신 GroundField를 필수 검사
- `BattlefieldArtAssetBuilder`와 Project Validator의 필수 경로를 GroundField 기준으로 변경
- Bootstrapper는 세 개 lane ground/target Transform을 생성하지 않고 SpaceConfig와 접근점 Provider를 연결
- `RuntimeSpriteBoundsUtility`는 Agent HalfExtents와 실제 Sprite bounds 검증에 재사용

## 8. 데이터 및 밸런스 호환

변경하지 않는 데이터:

- `SaveData` JSON 필드
- `FormationSlot.laneIndex`
- `FormationSlot.positionType`
- `WaveSpawnData.laneIndex`
- HeroData, MonsterData, SkillData 수치
- 웨이브 수, 개체 수, 보상
- 스테이지 진행·저장 흐름
- 현재 save version

공간 변화로 전투 결과가 달라질 수 있으므로 다음 방식으로 회귀를 관리한다.

- 비저장 진단용 `BattlefieldRegressionProbe`를 추가
- Stage 1·5·10, 보통 난이도, 기본 편성, 업그레이드 0, 펫 없음, 정해진 룬 선택, 최대 실행 시간을 fixture로 고정
- `Random.InitState`와 Shadow variant 생성까지 고정 seed로 통제
- 전투 시간, 최종 수정체 HP 비율, 영웅 생존 수, 수정체 도달 몬스터 수, Crystal damage event 수를 기록
- 구현 전후 동일 fixture로 비교
- 총 전투 시간 편차 목표: ±15%
- 최종 수정체 HP 비율 편차 목표: ±15%
- event 수 허용치는 `max(1, ceil(baseline × 0.15))`
- 수치 편차가 크면 공격력·HP·쿨다운을 바꾸지 않고 Leash, 접근 지점, spawn bias, target lock만 조정

저장 호환은 raw JSON 문자열 순서가 아니라 `saveVersion`과 `(laneIndex, positionType, heroId)` 목록의 의미적 동일성을 검사한다.

## 9. 레이아웃 변경 처리

`BattlefieldLayoutCoordinator`만 카메라 bounds 변경을 받아 다음 순서를 실행한다.

1. `BattlefieldCameraFitter`가 viewport, orthographic size, camera position을 적용한 뒤 `RuntimeSpriteBoundsUtility.GetCameraWorldBounds(targetCamera)`로 `CurrentWorldBounds` 확정
2. Coordinator가 이전 bounds를 보관하고 `BattlefieldSpaceController.RefreshLayout()` 실행
3. Registry가 활성 Agent의 위치와 Anchor를 이전 정규화 좌표에서 새 bounds로 remap
4. `CrystalApproachPointProvider.RefreshLayout()`로 기존 reservation의 월드 위치 갱신
5. `BattlefieldVisualController.RefreshLayout()` 실행
6. Agent HalfExtents와 실제 Sprite bounds를 포함해 최종 clamp

화면비 변경으로 유닛이 한 점으로 순간 이동하거나 전장 밖에 남지 않아야 한다.

기존 `BattlefieldCameraFitter.ConfigureBattlefieldVisuals()` 직접 연결은 제거한다. 다른 컴포넌트가 `WorldBoundsChanged` 구독 순서에 기대어 레이아웃을 변경하지 않게 한다.

## 10. 구현 순서와 후속 이슈

각 단계는 별도 이슈와 `codex/issue-<번호>-...` 브랜치로 진행한다. 앞 단계가 main에 반영된 뒤 다음 단계를 시작한다.

### A. 공통 UI 프레임 및 Title uGUI

제안 이슈:

`개선: 공통 UI 프레임 v2와 TitleScene uGUI 전환`

범위:

- `UiFrameTokens`, `TitleCanvasLayout`, Title prefab
- `TitleUI`의 uGUI 전환과 OnGUI 제거
- StatusFooter 분리, Settings ScrollRect, 확인 Modal
- RuntimePixelGuiUtility의 남은 IMGUI용 border 호환 보정
- Bootstrapper, Validator, 캡처 갱신

### B. 연속형 전장 공간 기반

제안 이슈:

`리팩토링: BattleScene 연속형 전장 공간과 Agent 기반 추가`

범위:

- `BattlefieldSpaceConfig`, `BattlefieldBounds`, 임시 `BattlefieldMode`
- `BattlefieldLayoutCoordinator`, `BattlefieldSpaceController`, Agent Registry
- `BattleManager` 준비 상태 gate와 초기화 순서
- `BattlefieldCameraFitter`, `BattleCanvasController`, `LaneManager` 연결 책임 정리
- Formation/Spawn 호환 매퍼
- 7개 Crystal 접근 지점
- `UnitMovementController` 2D API와 깊이 정렬
- StageSelect 편성 라벨을 `하단/중앙/상단`으로 변경
- 편성·공간 관련 Progression Smoke Test 갱신

### C. 영웅·몬스터·스킬 전투 규칙 전환

제안 이슈:

`개선: 영웅과 몬스터의 2D 이동·타기팅·가로막기 전환`

범위:

- HeroController, MonsterController
- WaveManager, HeroPlacementManager
- SkillController 직접 피해 사거리 재검증
- TemporaryTurret, 넉백, 보스 band, 폭발·연쇄 룬, 펫 lane 의존 제거
- CharacterVisualController의 X축 전용 lunge를 2D 방향으로 전환
- 레거시 `MoveToX`, lane equality 런타임 호출 제거
- 밸런스 기준 비교

### D. 연속형 전장 아트와 QA

제안 이슈:

`개선: Stage 1 연속형 전장 바닥 구성과 시각 검증`

범위:

- `GroundFieldRenderer`
- 필요 시 `ground_stage01_field.png` 최소 파생 자산
- 배치 룬과 목표물 재배치
- `BattlefieldArtTheme`, `BattlefieldArtAssetBuilder`, `BattlefieldVisualController`
- Bootstrapper의 3개 lane ground/target 생성 제거
- 본체·그림자·HP bar·배치물·투사체·CombatVisualEffectFactory 깊이 정렬
- Project/Game Frame Validator
- 네 화면비와 Android E2E

## 11. 검증 및 완료 조건

### 11.1 UI 자동 검사

- TitleScene의 Canvas, CanvasScaler, GraphicRaycaster, EventSystem 각각 정확히 1개
- TitleCanvas prefab 인스턴스 정확히 1개
- `TitleUI`에 `OnGUI()`와 `drawRuntimeGui` 필드가 없음
- Theme의 PanelSprite와 ButtonSprite가 null이 아님
- `MenuPanel`, `ModalPanel`, 모든 `Button.targetGraphic`이 `Image.Type.Sliced`
- BackgroundLayer, Dim, 아이콘은 Sliced 검사에서 제외
- sprite border가 패널 24, 버튼 12
- Header, Body, Action과 ScrollRect Viewport가 `border + 16` 안쪽에 있음
- Header, Body, Action, Footer 상호 겹침 없음
- BrandArea와 MenuPanel이 겹치지 않고 MenuPanel과 StatusFooter 간격이 16 이상
- ModalPanel이 safe area 안에 있고 Dim이 전체 Canvas를 덮음
- 모든 조작 버튼 높이가 Canvas 기준 88, screen-space 기준 56px 이상
- PlayMode에서 `Canvas.ForceUpdateCanvases()` 후 실제 RectTransform이 `TitleCanvasLayout.Calculate()` 결과와 허용 오차 안에서 일치
- 보이는 TitleCanvas TMP에 `ForceMeshUpdate()`를 호출한 뒤 `isTextOverflowing == false`
- Settings Body가 작은 높이에서 ScrollRect로 보호됨
- 런타임과 Validator가 `TitleCanvasLayout.Calculate()` 결과를 함께 사용
- 1080×2400 상단 120·하단 80 inset, 1600×900 좌우 80 inset, 같은 크기에서 위치만 다른 safe area를 주입해 검사

### 11.2 UI 캡처

해상도:

- 720×1280
- 1080×1920
- 1080×2400
- 1600×900
- 2048×1152

상태:

- 신규 저장 메인
- 기존 저장 메인
- 설정
- 새 전선 확인
- 저장 초기화 확인

사람 검토:

- 황동 모서리와 청색 내부선의 Canvas 기준 두께가 일정하고 X/Y 비균일 확대가 없음
- 9-slice 모서리와 장식선이 border 영역 밖으로 늘어나지 않음
- 제목과 푸터가 장식선을 가로지르지 않음
- CTA와 보조 버튼이 프레임에 닿지 않음
- 큰 빈 공간이 의도된 BodyArea로 읽히며 버튼을 밀어내지 않음

### 11.3 전투 자동 검사

- 3×3의 9개 논리 Formation 조합이 모두 고유한 2D Anchor로 매핑됨
- 저장 전후 saveVersion과 `(laneIndex, positionType, heroId)` 목록이 의미적으로 동일함
- 출시 BattleScene의 `BattlefieldMode`가 `Continuous2D`임
- 전투 시작 전에 Space, Registry, ApproachProvider, Visual, LayoutCoordinator가 모두 IsReady
- 모든 Agent의 HalfExtents와 실제 Sprite bounds가 PlayableBounds 안에 있음
- HeroController, MonsterController, TemporaryTurretController, HeroRuneCombatModifiers, BossAttackPatternController의 대상 선택·가로막기에 lane equality가 사용되지 않음
- 합성된 10마리 예약 fixture에서 접근 지점 7개 중 누적 고유 4개 이상이 할당됨
- 강제 웨이브 종료, 사망, 도달, 재시작 후 접근점 reservation 수가 0
- 검증 구간의 전체 지상 유닛 중심 Y 범위가 PlayableBounds 높이의 최소 60%
- cross-band interception fixture에서 영웅이 초기 Anchor Y에서 최소 0.20H 이동해 적을 공격
- 같은 X라도 Y가 2D 사거리 밖이면 근접 공격·차단하지 않음
- 대각선 중심점 거리가 사거리 안이면 기본 공격 성공
- 직접 피해 스킬과 임시 포탑이 2D 사거리 밖 대상을 공격하지 않음
- `First`가 ObjectiveProgress가 가장 높은 몬스터를 선택
- 일반 지상 같은 진영 유닛의 `distance < 0.5 × (radiusA + radiusB)` 상태가 0.5초 이상 지속되지 않음
- FlyingMonster가 지상 분리에 묶이지 않음
- 화면 아래 유닛 order가 위 유닛보다 높고 그림자는 본체보다 정확히 1 낮음
- 화면비 변경 후 Missing Reference, NaN 위치, bounds 이탈 없음
- Hero, Monster, Skill, Wave 런타임 호출부에 고정 `MoveToX` 호출 없음
- 전투 중 활성 `Lane N Ground` 렌더러 없음
- 기존 `portrait 3-lane spread` Smoke 검사를 PlayableBounds, 9개 Anchor, 7개 접근 지점 검사로 교체
- E2E 진단 몬스터 생성이 `GetLaneY()` 대신 Space API를 사용

### 11.4 통합 검증

- `git diff --check`
- C# 컴파일
- Project Validator
- Game Frame Validator
- Progression Smoke Test
- System Flows E2E
- Full Chapter E2E
- Android 15 API 35, 1080×2400
- Stage 1·5·10 전투 수치 기준 비교

## 12. 제외 범위

- 캐릭터 원화 또는 모션시트 재제작
- Stage 2~10의 개별 연속형 배경 제작
- NavMesh, A*, 장애물 회피 맵
- 물리 기반 군중 시뮬레이션
- 외부 패키지, 유료 에셋, 포스트프로세싱 패키지
- 공격력, HP, 스킬 수치, 웨이브 수치 재밸런싱
- 저장 포맷 마이그레이션
- 타이틀 외 모든 IMGUI 화면의 일괄 전환

## 13. 최종 판단

현재 문제는 기존 설계를 전부 폐기해야 하는 상황이 아니다.

- BattleCanvas uGUI와 Stage 1 캐릭터 기준 아트는 유지한다.
- 깨지는 타이틀 프레임은 같은 uGUI 규칙으로 편입한다.
- 3×3 편성은 전략 입력으로 유지한다.
- 고정 3레인만 런타임 전투 규칙에서 제거한다.

즉, `배치할 때는 3×3로 이해하기 쉽고, 싸울 때는 전장 전체를 사용하는 구조`가 이번 v2의 기준이다.
