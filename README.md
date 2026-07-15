# RuneGate Defense

## v0.88.0 Game Frame Rebuild

- 기존 UI를 단순 미세 수정한 것이 아니라 StageSelectFrame / BattleFrame / PopupLayer 구조로 재구성했습니다.
- 현재 프로젝트는 IMGUI 기반 UI가 많으므로, `GameFrameLayout`으로 공통 frame rect를 계산하고 기존 UI 스크립트가 이를 사용한다.
- Unity Editor 메뉴 `Tools/RuneGate/Validate Game Frame`으로 UI frame foundation, 필수 scene/script/doc, 위험 문자열을 점검할 수 있다.
- 관련 문서: `docs/game-frame-rebuild-v088-0.md`, `docs/ui-layout-foundation.md`, `docs/battlefield-frame.md`, `docs/popup-layout.md`, `docs/ui-frame-validation-report-v088-0.md`.

RuneGate Defense는 Unity 6 기반 모바일 Portrait 우선 2D 픽셀 판타지 디펜스 프로토타입이다. 한국 출시 우선 방향으로 문지기, 봉문, 균열, 재문 세계관 표현을 사용하며, 전투 런타임에는 `RuntimePixel` 스프라이트를 사용한다.

## 현재 개발 상태

- Stage 1~10 진행 흐름
- 영웅 6종, 몬스터 6종, 보스 1종 데이터
- 실제 전투 효과가 연결된 룬 20장
- 밀치기, 3연타, 범위 피해, 회복, 임시 포탑, 보스 처형으로 구분되는 영웅 6인 고유 스킬
- RuntimePixel 영웅/몬스터/보스 표시
- 전투 배경, 기본 공격/피격/사망 피드백
- 튜토리얼, StageSelect, Battle, Result, Upgrade, Save 흐름
- 로컬 JSON 저장
- 랜덤 몬스터 변종 MVP
- 그림자 조각, 그림자 계약, 펫 장착 MVP
- Normal Chapter 1 완료 후 Hard, Hard 완료 후 Nightmare가 열리는 난이도별 순차 진행과 보상 보정
- NotoSansKR 기반 한글 표시
- 모바일 Portrait 기준 IMGUI 레이아웃 정규화 1차 적용
- Safe Area 기반 UI 계산, PetContract 중앙 팝업, 공통 팝업 유틸 1차 적용
- Android 15 API 35 에뮬레이터에서 1080x2400 Portrait Stage 1 전체 흐름 검증
- Android 에뮬레이터에서 업그레이드 구매, 재시작 저장 유지, Stage 2 진입 검증
- Android 에뮬레이터에서 Stage 1~10 전체 승리, 강화 10회, Stage 10 그룸바르 스폰 검증
- 전투 일시정지, 재시작, 스테이지 복귀와 Android 백그라운드 자동 일시정지 지원
- StageSelect 3라인 x 3슬롯 영웅 편성 편집 및 로컬 JSON 즉시 저장
- Stage 10 그룸바르 3단계 페이즈, 지원군 소환, 전용 보스 HP HUD
- 그룸바르 크리스탈 접촉 후 반복 공격 및 실제 처치 Victory 규칙
- 그룸바르 페이즈별 라인 강타, 전선 충격파, 크리스탈 파괴 파동과 사전 경고 표시
- 번개/폭발/수호/정화/분쇄/연쇄 룬 전용 효과와 웨이브 지속 감속
- 메뉴/전투 장면별 절차형 BGM, 교차 페이드, BGM/SFX 독립 음소거와 음량 저장
- Android 에뮬레이터에서 Normal/Hard/Nightmare Stage 1~10, 총 30전투 연속 승리와 강화 20회 검증

## 실행 방법

1. Unity Hub 또는 Unity Editor에서 `C:\workspace\defense-game`을 연다.
2. 스크립트 컴파일이 끝날 때까지 기다린다.
3. `Tools/RuneGate/Validate Project`를 실행한다.
4. `Assets/_Project/Scenes/TitleScene.unity`를 연다.
5. Play를 눌러 타이틀에서 시작한다.

## 수동 테스트 흐름

1. TitleScene에서 `새 게임 시작` 또는 `이어하기`를 선택한다.
2. StageSelectScene 하단의 `편성`에서 영웅을 선택하고 3라인 x 후열/중열/전열 슬롯을 편집한다.
3. Stage 1을 선택하고 전투 시작을 누른 뒤 저장한 편성이 적용되는지 확인한다.
4. BattleScene에서 영웅이 라인 안에서 이동/공격하는지 확인한다.
5. 몬스터가 오른쪽에서 왼쪽으로 진격하고 크리스탈에 도달하면 피해를 주는지 확인한다.
6. 웨이브 후 룬 선택 UI에서 룬을 선택한다.
7. Victory 후 골드 지급, 스테이지 해금, 그림자 조각 보상을 확인한다.
8. StageSelectScene의 그림자 계약 패널에서 조각 수, 계약, 장착/해제를 확인한다.
9. UpgradeScene에서 업그레이드를 구매하고 저장되는지 확인한다.
10. Stage 10에서 그룸바르 보스가 등장하는지 확인한다.
11. Normal Stage 10 승리 후 `어려움`이 열리고, 어려움에서는 Stage 1부터 순서대로 열리는지 확인한다.
12. 어려움 Stage 10 승리 후 `악몽`이 열리고 앱 재실행 뒤에도 유지되는지 확인한다.

## UI 레이아웃 기준

- 기준 해상도: 1080 x 1920 Portrait
- 보조 확인: 720 x 1280, 1440 x 2560, Unity Game View 세로 Free Aspect
- 현재 주요 런타임 UI는 IMGUI 기반이다.
- `UIResponsiveLayout`으로 safe margin, 중앙 패널, 좌측 컬럼, 화면 클램프 Rect를 계산한다.
- StageSelectScene은 Header / Body 2컬럼 / Footer 구조를 사용한다.
- 리스트, 상세 정보, 룬 선택, 결과, 업그레이드 목록은 ScrollView로 보호한다.
- StageSelect의 난이도와 그림자 계약은 헤더 버튼으로 접근하며, 그림자 계약 상세는 중앙 팝업으로 표시한다.
- Canvas 기반 UI 전환을 위해 `SafeAreaFitter`와 `PanelClampToScreen`을 준비했다.
- 자세한 내용은 `docs/ui-layout-normalization-v085.md`를 참고한다.

## Unity 메뉴

- `Tools/RuneGate/Bootstrap Playable Prototype`
- `Tools/RuneGate/Bootstrap Progression Prototype`
- `Tools/RuneGate/Bootstrap v0.4 Content Prototype`
- `Tools/RuneGate/Bootstrap v0.5 Art Prototype`
- `Tools/RuneGate/Bootstrap v1.0 Release Track`
- `Tools/RuneGate/Apply Initial Art Images`
- `Tools/RuneGate/Validate Project`
- `Tools/RuneGate/Validate v1.0 Release Track`
- `Tools/RuneGate/Configure Android v0.9 RC Settings`
- `Tools/RuneGate/Build Android APK v0.9 RC`
- `Tools/RuneGate/Configure Android Release Settings`
- `Tools/RuneGate/Build Android APK v1.0`
- `Tools/RuneGate/Build Current Android APK`
- `Tools/RuneGate/Build Current Android AAB`

현재 콘텐츠를 덮어쓰지 않는 Android 빌드는 `Build Current Android APK` 또는 `Build Current Android AAB`를 사용한다. Player 런타임 자동 검증 방법은 `docs/runtime-e2e-smoke-test.md`를 참고한다.

## 아트 정책

- `Assets/_Project/Art/ConceptSheets`는 도감, 설정, 참고용이다.
- `Assets/_Project/Art/MotionSheets`는 향후 애니메이션 후보이며 BattleScene SpriteRenderer에 직접 연결하지 않는다.
- `Assets/_Project/Art/RuntimePixel`만 BattleScene 전투용 스프라이트로 사용한다.
- RuntimePixel이 없으면 작은 placeholder fallback을 사용한다.

## 금지 범위

- 서버
- 로그인
- 가챠
- 광고 SDK
- 인앱결제 SDK
- 분석/트래킹 SDK
- Firebase
- Addressables
- 멀티플레이
- 랭킹
- 클라우드 저장
- 외부 유료 API

## 주요 문서

- `docs/ui-layout-normalization-v085.md`
- `docs/ui-foundation-v0865.md`
- `docs/sprite-bounds-and-camera-fix-v0866.md`
- `docs/game-feel-v085.md`
- `docs/random-monster-system-v086.md`
- `docs/pet-contract-system-v086.md`
- `docs/combat-lane-ai.md`
- `docs/motion-sheet-pipeline.md`
- `docs/ui-ux-v085.md`
- `docs/ui-ux-v08.md`
- `docs/known-issues.md`
- `docs/android-build-guide.md`
- `docs/android-emulator-qa-v088.md`
- `docs/android-progression-qa-v088.md`
- `docs/android-full-chapter-qa-v088.md`
- `docs/android-battle-camera-framing-v090.md`
- `docs/android-result-upgrade-layout-v090.md`
- `docs/android-title-background-v090.md`
- `docs/battle-pause-lifecycle-v089.md`
- `docs/procedural-sfx-v089.md`
- `docs/scene-bgm-audio-settings-v090.md`
- `docs/formation-editor-v090.md`
- `docs/grumbar-boss-phases-v090.md`
- `docs/grumbar-boss-attack-patterns-v090.md`
- `docs/difficulty-progression-v090.md`
- `docs/difficulty-design.md`
- `docs/android-all-difficulty-campaign-qa-v090.md`
- `docs/complete-rune-effects-v090.md`
- `docs/release-checklist.md`
- `docs/store-listing-draft.md`
- `docs/privacy-checklist.md`
- `docs/korean-font-setup.md`
- `docs/localization-polish-v08.md`
- `docs/content-balance-v07.md`
- `docs/stage-design.md`
- `docs/rune-design.md`
- `docs/pixel-art-pipeline.md`
- `docs/art-integration-notes.md`

## 저장 데이터

저장 데이터는 로컬 JSON으로 관리한다.

```text
Application.persistentDataPath/runegate_save.json
```

Unity Editor on Windows 기준으로는 보통 아래 경로에 생성된다.

```text
%USERPROFILE%\AppData\LocalLow\<CompanyName>\<ProductName>\runegate_save.json
```

개발 테스트 중에는 TitleScene의 저장 초기화 기능을 사용할 수 있다.
## v0.88-1 전투 이동 기준

- Stage 1~3 전투 감각 개선을 위해 `UnitMovementController` 기반 x축 가속/감속 이동을 적용했다.
- 영웅은 `HeroCombatState`, 몬스터는 `MonsterCombatState`를 사용한다.
- 레온은 전열에서 더 적극적으로 막고, 세리아/카엘은 사거리 유지, 미레아/브롬은 anchor 유지, 닉스는 짧게 접근하는 성향을 갖는다.
- 몬스터는 오른쪽에서 왼쪽으로 전진하다가 같은 라인 영웅과 근접하면 멈춰 공격한다.
- 상세 기준은 `docs/organic-combat-movement-v088-1.md`와 `docs/combat-lane-ai.md`를 참고한다.

## v0.88-2 Stage 1~3 튜닝

- Stage 1은 문틈 도깨비 중심 2 waves, 승리 최소 보상 110 Gold.
- Stage 2는 문틈 도깨비 + 재갑 돌격병 3 waves, 승리 최소 보상 140 Gold.
- Stage 3은 문틈 도깨비 + 부식 늑대 3 waves, 승리 최소 보상 170 Gold.
- `Tools/RuneGate/Start Stage 1 Test`, `Start Stage 2 Test`, `Start Stage 3 Test` 메뉴로 BattleScene 테스트 준비가 가능하다.
- 상세 기준은 `docs/stage-1-3-combat-tuning-v088-2.md`와 `docs/balance-v088.md`를 참고한다.

## v0.88-2 전투 감각 보강

- 기본 공격은 WindUp / Impact / Recovery 단계로 처리한다.
- 데미지는 Impact 시점에 적용된다.
- 원거리 공격은 projectile 도착 시점에 데미지를 준다.
- 피격자는 hit flash, shake, hit spark, damage text를 표시한다.
- Result 화면은 클리어 시간, 크리스탈 HP, 처치 수를 표시한다.
- `Tools/RuneGate/Playtest Stage 1~3` 메뉴를 사용할 수 있다.
