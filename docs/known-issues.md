# Known Issues

## v0.88 Game Frame / UI

- The main UI is still IMGUI-based. A final mobile release should move core screens to Canvas / RectTransform prefabs.
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

## Combat

- Hero and monster movement is lane-based interpolation, not full pathfinding or physics collision.
- Battle pause, resume, restart, stage-select exit, and Android background auto-pause passed emulator QA. Physical-device phone calls, screen lock, and multi-window transitions remain unverified.
- Portrait safe bounds now use the monster sprite leading edge for crystal contact, preventing the final monster from remaining stalled at the left battlefield boundary.
- Undead revival is enabled only on Hard and Nightmare; Easy and Normal keep the hook disabled for clear-time stability.
- Attack, hit, and death feedback is still prototype-level.
- Some skill effects are RuntimePixel placeholders, not final animation.
- Boss phase changes, summon patterns, and monster-specific behavior are still mostly hooks.
- Stage 1-10 data is playable-oriented, but long-form balance testing is still required.

## Progression / Save

- UpgradeScene now recovers its four upgrade assets from RuntimeContentCatalog when serialized scene slots are null. Scene references should still be regenerated before final prefab/Canvas conversion.
- Upgrade purchase, immediate UI refresh, process-restart persistence, and Stage 2 unlock passed Android emulator QA. Physical-device scroll inertia and rapid repeated taps remain unverified.

- The game uses local JSON save only.
- An OS process kill does not restore an in-progress battle; the next launch resumes from the latest persisted progression state.
- Automated Player coverage now verifies tutorial completion persistence, JSON disk reload, full process restart reload, Reset Save defaults, and Defeat progression guards with an isolated save path.
- Corrupt primary fallback, valid `.bak` restoration, interrupted `.tmp` promotion, and invalid `.tmp` isolation are Player-tested; storage permission failures and real device power loss still need device QA.
- Reset Save is available, but final release UX should add stronger confirmation polish.
- Hard and Nightmare affect combat numbers, but unlock rules and final reward tuning need more work.

## Android / Release

- APK/AAB builds require Unity Android Build Support and a clean build environment.
- Clean-branch Android APK/AAB builds succeeded on 2026-07-12. APK installation and the Stage 1 core flow passed on an Android 15 API 35 emulator; physical-device installation is not verified yet.
- Android 15 API 35 emulator full-chapter QA passed on 2026-07-15: Stage 1~10 Victory, 10 upgrade purchases, and the Stage 10 Grumbar spawn were verified at 1080x2400 Portrait.
- The generated APK is signed with the Android Debug certificate. A protected release keystore is still required before store submission.
- Physical-device install, display-cutout Safe Area, and long-session performance validation are not complete.
- App icon, splash, and store graphics are first-pass candidates.
- Current-content APK and AAB menu/CLI automation is implemented and build-tested; release keystore signing and Play Console upload remain manual release tasks.
- Store submission still needs final target SDK, signing, permissions, Data Safety, and privacy review.

## Monetization

- No real ad SDK is included.
- No real billing SDK is included.
- v1.0 docs may mention optional rewarded ads, remove-ads purchase, starter pack, and supporter pack as future candidates only.
- Forced ads and gacha remain prohibited.
