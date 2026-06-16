# RuneGate Defense

RuneGate Defense는 Unity 6 기반 모바일 세로형 2D 픽셀 디펜스 프로토타입이다. 한국 출시를 우선으로 하며, 문지기/봉문/균열/재문 세계관과 기록서풍 ConceptSheets, 전투용 RuntimePixel 스프라이트를 분리해서 관리한다.

## Current Release Candidate

현재 목표 버전은 v0.9 Release Candidate다.

- Product Name: `RuneGate Defense`
- Package Name: `com.hobeomcheon.runegatedefense`
- Version: `0.9.0`
- Bundle Version Code: `9`
- Orientation: Portrait
- App icon candidate: `Assets/_Project/Art/RuntimePixel/App/app_icon_1024.png`
- Adaptive icon foreground candidate: `Assets/_Project/Art/RuntimePixel/App/app_icon_adaptive_foreground.png`
- Splash candidate: `Assets/_Project/Art/RuntimePixel/App/splash_background_1080x1920.png`
- Store feature graphic candidate: `Assets/_Project/Art/Store/store_feature_graphic_1024x500.png`

## Required Unity Version

- Unity 6 recommended
- Android Build Support is required for Android APK/AAB builds
- External packages are not required for the current prototype

## How To Open

1. Open `C:\workspace\defense-game` in Unity Hub or Unity Editor.
2. Wait for scripts to compile.
3. Run `Tools/RuneGate/Validate Project`.

## How To Play

1. Run `Tools/RuneGate/Bootstrap v1.0 Release Track`.
2. Open `Assets/_Project/Scenes/TitleScene.unity`.
3. Press Play.
4. Start or Continue to Stage Select.
5. Confirm the tutorial appears on first battle entry.
6. Select Stage 1 and clear the battle.
7. Confirm Victory gives gold and unlocks Stage 2.
8. Open Upgrade, buy an upgrade if enough gold is available, then return to Stage Select.
9. Stop and restart Play Mode to confirm local save persistence.
10. Continue through Stage 10 and confirm Grumbar appears.
11. Use Reset Save on the Title scene to clear test progress.

## Unity Menus

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

## Implemented

- 3-lane battle flow
- Stage 1~10 progression
- 6 heroes, 6 monsters, and 1 boss
- 20 rune cards
- RuntimePixel hero/monster/boss sprites
- Battle background and placeholder combat effects
- Tutorial, StageSelect, Battle, Result, Upgrade, Save flow
- Local JSON save/load and reset save
- Korean font setup with NotoSansKR
- Korean display data repair
- Internal id / debug text hiding for main UI
- Android v0.9 RC Player Settings
- App icon, adaptive icon foreground, splash, and store feature graphic candidate assets

## MVP Exclusions

- No server
- No login
- No gacha
- No ads
- No in-app purchase
- No analytics SDK
- No Firebase
- No Addressables
- No multiplayer
- No ranking
- No cloud save
- No external paid APIs

## Folder Structure

```text
Assets/_Project/
  Scripts/
    Core/
    Battle/
    Hero/
    Monster/
    Skill/
    Rune/
    Wave/
    Data/
    Progression/
    Save/
    UI/
    Editor/
  Data/
    Heroes/
    Monsters/
    Skills/
    Runes/
    Stages/
    Upgrades/
    Formations/
    Rosters/
  Scenes/
  Art/
    ConceptSheets/
    RuntimePixel/
      App/
      Backgrounds/
      Bosses/
      Effects/
      Heroes/
      Monsters/
      UI/
    Store/
  Resources/
```

## Key Docs

- `docs/android-build-guide.md`
- `docs/release-checklist.md`
- `docs/known-issues.md`
- `docs/store-listing-draft.md`
- `docs/privacy-checklist.md`
- `docs/korean-font-setup.md`
- `docs/localization-polish-v08.md`
- `docs/ui-ux-v08.md`
- `docs/content-balance-v07.md`
- `docs/stage-design.md`
- `docs/rune-design.md`
- `docs/pixel-art-pipeline.md`
- `docs/art-integration-notes.md`

## Android Build

For v0.9 RC:

```text
Tools/RuneGate/Configure Android v0.9 RC Settings
Tools/RuneGate/Build Android APK v0.9 RC
```

Expected APK path:

```text
Builds/Android/RuneGateDefense-v0.9.0.apk
```

`Builds/` is generated output and should not be committed.

## Save Data

Save data is stored locally as JSON:

```text
Application.persistentDataPath/runegate_save.json
```

In the Unity Editor on Windows this is usually under:

```text
%USERPROFILE%\AppData\LocalLow\<CompanyName>\<ProductName>\runegate_save.json
```

Use the Title scene `Reset Save` button during development testing.
