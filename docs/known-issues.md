# Known Issues

## v0.88 Game Frame / UI

- The main UI is still IMGUI-based. A final mobile release should move core screens to Canvas / RectTransform prefabs.
- StageSelect, Battle HUD, Rune Selection, Result, and Upgrade screens use shared Rect math, but real Android safe-area and touch-target QA is still required.
- If Unity is already in Play Mode while scripts change, the Game View can keep showing the previous UI. Stop Play Mode, wait for script reload, and enter Play Mode again.
- `Tools/RuneGate/Validate Game Frame` checks static layout rectangles. Manual Game View QA is still required at 720x1280, 1080x1920, and 1440x2560 portrait.
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
- Attack, hit, and death feedback is still prototype-level.
- Some skill effects are RuntimePixel placeholders, not final animation.
- Boss phase changes, summon patterns, and monster-specific behavior are still mostly hooks.
- Stage 1-10 data is playable-oriented, but long-form balance testing is still required.

## Progression / Save

- UpgradeScene now recovers its four upgrade assets from RuntimeContentCatalog when serialized scene slots are null. Scene references should still be regenerated before final prefab/Canvas conversion.

- The game uses local JSON save only.
- Corrupt-save fallback exists, but more destructive save-file QA is needed.
- Reset Save is available, but final release UX should add stronger confirmation polish.
- Hard and Nightmare affect combat numbers, but unlock rules and final reward tuning need more work.

## Android / Release

- APK/AAB builds require Unity Android Build Support and a clean build environment.
- The current-content Android APK batch build succeeded on 2026-07-12; device installation has not been verified yet.
- The generated APK is signed with the Android Debug certificate. A protected release keystore is still required before store submission.
- Device install, long-session performance, screen clipping, and touch validation are not complete.
- App icon, splash, and store graphics are first-pass candidates.
- AAB build automation is not separately finalized.
- Store submission still needs final target SDK, signing, permissions, Data Safety, and privacy review.

## Monetization

- No real ad SDK is included.
- No real billing SDK is included.
- v1.0 docs may mention optional rewarded ads, remove-ads purchase, starter pack, and supporter pack as future candidates only.
- Forced ads and gacha remain prohibited.
