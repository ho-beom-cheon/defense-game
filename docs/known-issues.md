# Known Issues

## v0.88 Game Frame / UI

- The main UI is still IMGUI-based. A final mobile release should move core screens to Canvas / RectTransform prefabs.
- TitleScene now separates new/continuing campaigns, settings, and destructive confirmations in a game-style Android portrait layout. Final logo animation, button art, accessibility font scaling, and physical-device cutout QA remain open.
- The seven-step first-battle tutorial now uses a dim overlay, per-step focus frames, fixed navigation, and Android portrait-safe guidance cards. Final tutorial-specific animation, accessibility scaling, and physical-device cutout QA remain open.
- StageSelect, Battle HUD, Rune Selection, and Result passed 1080x2400 Android emulator touch/Safe Area QA. Physical devices with display cutouts still require confirmation.
- If Unity is already in Play Mode while scripts change, the Game View can keep showing the previous UI. Stop Play Mode, wait for script reload, and enter Play Mode again.
- `Tools/RuneGate/Validate Game Frame` checks static layout rectangles. Manual Game View QA is still required at 720x1280, 1080x1920, and 1440x2560 portrait.
- Latest static frame validation passed with 83 checks and no warnings or failures; rendered text clipping and touch targets still require device QA.
- Windows Player screenshot QA passed for six core screens. Android emulator QA passed at 1080x2400; exact 1080x1920/1440x2560 physical-device rendering remains unverified.
- `.meta` GUID format is now checked by `Tools/RuneGate/Validate Project`, but Unity may need an AssetDatabase refresh after metadata changes.

## Runtime Pixel Art

- Current hero and monster images are prototype RuntimePixel assets.
- Source image quality, scale, contrast, and style consistency still need final art production.
- RuntimePixel import metadata has been normalized to Sprite / Single / Point / MipMap Off / Uncompressed.
- Final combat art should provide consistent Idle / Walk / Attack / Hit / Death sprite sheets.
- ConceptSheets are reference/codex art only and must not be used directly by BattleScene SpriteRenderers.
- If a RuntimePixel sprite is missing, the game still falls back to a small placeholder.
- 몬스터 6종과 그룸바르의 경계 연결 검은 픽셀은 정리했으며 Validator가 불투명 검정 경계를 검사한다. Android 1080x2400 Stage 3/7/10 화면에서는 검은 직사각형이 재현되지 않았다.

## Combat

- Hero and monster movement is lane-based interpolation, not full pathfinding or physics collision.
- Battle pause now includes a state summary, persisted BGM/SFX controls, destructive-action confirmations, resume, restart, stage-select exit, and Android background auto-pause. Android emulator QA passed; physical-device phone calls, screen lock, display cutouts, and multi-window transitions remain unverified.
- Portrait safe bounds now use the monster sprite leading edge for crystal contact, preventing the final monster from remaining stalled at the left battlefield boundary.
- Undead revival is enabled only on Hard and Nightmare; Easy and Normal keep the hook disabled for clear-time stability.
- Attack, hit, and death feedback is still prototype-level.
- Some skill effects are RuntimePixel placeholders, not final animation.
- All six hero skills now have distinct runtime mechanics. Their timing, damage, healing, and turret duration still need long-form physical-device balance testing.
- Android Portrait 전투 스킬 HUD는 3x2 카드, RuntimePixel 영웅 초상, HP, 준비/쿨다운/비활성 상태를 표시한다. 전용 스킬 아이콘과 카드 애니메이션은 아직 최종 아트가 아니다.
- Android Portrait 상단 HUD는 Crystal HP/보호막, Wave 진행, 한국어 전투 상태를 카드와 진행 바로 표시한다. 전용 HUD 아이콘과 Canvas 전환은 아직 적용하지 않았다.
- Android Portrait 룬 선택은 3장의 전술 기록 카드, 희귀도 색상, 속성 문양, 설명과 효과 수치를 표시한다. 대부분의 룬은 전용 아이콘 대신 한글 속성 문양 폴백을 사용한다.
- Android Portrait 전투는 첫/최종/보스 웨이브 배너와 선택 룬 적용 문구를 표시한다. 전용 웨이브 문양, 보스 경보 아이콘과 경보 음원은 아직 적용하지 않았다.
- Grumbar has three HP-based phases, five Gate Imp reinforcements, phase-gated damage, a dedicated boss HUD, and three telegraphed phase-specific area attacks.
- Boss attack telegraphs and impacts still use RuntimePixel/colored fallback effects; final attack animation, unique audio, and additional reinforcement types are not implemented.
- Grumbar remains at the crystal and attacks it repeatedly instead of being removed on contact; final crystal-pressure tuning is still provisional.
- All 20 RuneData records now use implemented runtime effects. Lightning, splash, and chain feedback still reuses prototype hit sparks, and combination balance needs longer device playtests.
- Stage 1-10 data is playable-oriented, but long-form balance testing is still required.

## Progression / Save

- StageSelect의 3x3 편성 편집은 Android 자동 시스템 흐름에서 슬롯 교환, JSON 재로드, BattleScene 반영까지 통과했다.
- StageSelect에서 BattleScene으로 이어지는 Android 일반 실행/터치/캡처는 1080x2400 에뮬레이터에서 통과했다. 편성 팝업의 최종 터치 영역과 텍스트 잘림은 실기기에서 추가 확인이 필요하다.
- StageSelect의 Chapter 1은 좌우 교차형 10개 노드 지도로 표시되며 Android 1080x2400 화면 QA를 통과했다. 해금 노드 원본 이미지의 숫자 `1`은 런타임 번호 오버레이로 교정하며, 최종 아트에서는 숫자 없는 공용 노드 이미지가 필요하다.
- Stage 1~9의 잘못된 그룸바르 참조를 제거하고 Stage 10만 보스를 소유하도록 Validator와 Smoke Test를 강화했다. 현재 지도 표현은 Chapter 1에 한정된다.
- 드래그 앤 드롭, 영웅 프리셋, 편성별 추천 표시는 아직 제공하지 않는다.
- UpgradeScene now recovers its four upgrade assets from RuntimeContentCatalog when serialized scene slots are null. Scene references should still be regenerated before final prefab/Canvas conversion.
- Upgrade purchase, immediate UI refresh, process-restart persistence, and Stage 2 unlock passed Android emulator QA. Physical-device scroll inertia and rapid repeated taps remain unverified.
- Victory Result와 Upgrade 카드 폭은 1080x2400 Android 실제 터치 캡처에서 정규화됐다. Defeat 수동 캡처와 시스템 글꼴 배율 확대는 실기기에서 추가 확인이 필요하다.

- The game uses local JSON save only.
- An OS process kill does not restore an in-progress battle; the next launch resumes from the latest persisted progression state.
- Automated Player coverage now verifies tutorial completion persistence, JSON disk reload, full process restart reload, Reset Save defaults, and Defeat progression guards with an isolated save path.
- Corrupt primary fallback, valid `.bak` restoration, interrupted `.tmp` promotion, and invalid `.tmp` isolation are Player-tested; storage permission failures and real device power loss still need device QA.
- Reset Save is available, but final release UX should add stronger confirmation polish.
- Normal/Hard/Nightmare 30-stage continuous combat, sequential unlocks, rewards, upgrades, and JSON reload passed Android emulator QA. Physical-device endurance and manual-input balance still need QA.

## Android / Release

- APK/AAB builds require Unity Android Build Support and a clean build environment.
- Clean-branch Android APK/AAB builds succeeded on 2026-07-12. APK installation and the Stage 1 core flow passed on an Android 15 API 35 emulator; physical-device installation is not verified yet.
- Android 15 API 35 emulator full-chapter QA passed on 2026-07-15: Stage 1~10 Victory, 10 upgrade purchases, and the Stage 10 Grumbar spawn were verified at 1080x2400 Portrait.
- Android BattleScene now fills the portrait battlefield viewport with an explicit runtime backdrop and clears non-camera IMGUI regions, preventing StageSelect UI residue after scene transitions.
- Portrait BattleScene now spreads three lanes across about half of the camera world height and scales RuntimePixel units for readability. The current landscape background is stretched to the portrait camera bounds, so final portrait or tileable battlefield art is still required.
- Spawn, crystal target, and hero slot y positions now resolve from the same runtime lane policy. Physical-device cutout and long-session combat readability remain unverified.
- Skill card touch, cooldown state, Rune Selection disable state, and Normal Stage 1~10 regression passed at 1080x2400 on the Android emulator. Physical-device font scaling and repeated-touch endurance remain unverified.
- Battle status HUD, pause touch, Rune Selection state, crystal shield bar, and Normal Stage 1~10 regression passed at 1080x2400 on the Android emulator. Physical-device cutout and long-session state refresh remain unverified.
- Rune card layout, rarity/element presentation, selection touch, Wave 2 transition, and Normal Stage 1~10 regression passed at 1080x2400 on the Android emulator. Physical-device font scaling and repeated-touch endurance remain unverified.
- Opening/final/boss wave banners, applied-rune feedback, Stage 10 boss warning, and Normal Stage 1~10 regression passed at 1080x2400 on the Android emulator. Physical-device cutout and long-session banner timing remain unverified.
- RuntimePixel actor alpha cleanup, Stage 3 wolf, Stage 7 bone soldier, Stage 10 Grumbar rendering, and Normal Stage 1~10 regression passed at 1080x2400 on the Android emulator. Physical-device GPU/texture compression remains unverified.
- Development APK/AAB builds still use Unity's development signing path. Environment-variable release signing and artifact manifests are implemented and passed with a throwaway QA keystore, but a protected production keystore is still required before store submission.
- Physical-device install, display-cutout Safe Area, and long-session performance validation are not complete.
- App icon, splash, and store graphics are first-pass candidates.
- TitleScene now uses the first-pass splash candidate as its Android portrait background. Physical-device cutout coverage and final title art remain unverified.
- Current-content and release-signed APK/AAB menu/CLI automation is build-tested. Production credential provisioning, Play App Signing, and Play Console upload remain manual release tasks.
- Store submission still needs final target SDK, signing, permissions, Data Safety, and privacy review.

## Monetization

- No real ad SDK is included.
- No real billing SDK is included.
- v1.0 docs may mention optional rewarded ads, remove-ads purchase, starter pack, and supporter pack as future candidates only.
- Forced ads and gacha remain prohibited.

## Audio

- 전투 및 주요 UI에는 외부 에셋 없는 절차형 SFX 폴백 9종이 적용되어 있다.
- 절차형 SFX는 기능 검증용이며 최종 음질, 믹싱, 음량 균형을 대표하지 않는다.
- SFX 음소거 설정은 Android 재실행 후 유지되는 것을 확인했다.
- 메뉴/전투 장면별 절차형 BGM, 교차 페이드, BGM/SFX 독립 음량 설정이 구현되어 Android 시스템 플로우를 통과했다.
- 절차형 BGM은 기능 완성용 폴백이며 최종 작곡, 마스터링, 실기기 스피커/이어폰 믹싱은 별도 제작이 필요하다.
- 실기기 스피커와 이어폰의 체감 음량은 추가 검증이 필요하다.
