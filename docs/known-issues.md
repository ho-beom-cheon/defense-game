# Known Issues

## v0.9 Release Candidate

- Android APK build was attempted on 2026-06-16, but Unity batchmode stopped because the project was already open in another Unity Editor instance.
- Unity command-line validation was attempted on 2026-06-16, but Unity batchmode stopped for the same open-project lock.
- Device install and long-session performance testing must be completed manually.
- Android Player Settings are prepared for v0.9.0, but store submission settings such as target SDK and signing must be checked again before upload.
- App icon, adaptive foreground, splash background, and feature graphic are first-pass candidates, not final store art.
- Some combat effects are placeholder RuntimePixel effects.
- Current RuntimePixel unit art is still prototype-quality.
- Balance values are first-pass and should be tuned with playtest data.
- UI is still mostly IMGUI-based, so final mobile safe-area and touch polish remains.
- Tutorial arrows and tap indicators are static overlay icons, not target-following guides yet.
- TextMeshPro Font Asset is not generated yet. Current UI uses the NotoSansKR Font catalog with IMGUI.
- Hard and Nightmare difficulty rules are UI/save ready but not applied to combat yet.
- Boss phase change and summon patterns are hook-only.
- AAB build has not been automated separately from APK.
- Store policy, privacy policy URL requirement, Android permissions, target SDK, and Data Safety answers require final manual review before public release.

## Monetization

- No real ad SDK is included.
- No real billing SDK is included.
- v1.0 may document optional rewarded ads, remove-ads purchase, starter pack, or supporter pack as future candidates, but actual SDK integration is separate work.
- Forced ads and gacha remain prohibited.

## Runtime Pixel Art Policy

- BattleScene uses RuntimePixel sprites only.
- ConceptSheets images are reference/codex assets only and should not appear directly in BattleScene SpriteRenderers.
- BattleScene keeps small colored placeholder fallback when a future `battleSprite` or `runtimeSprite` is empty.

## Resolved In v0.8

- NotoSansKR font integration restored Korean UI rendering for IMGUI.
- Damaged `??` display strings were restored in hero, monster, rune, stage, skill, and upgrade ScriptableObject assets.
- Result, HUD, StageSelect, Skill Button, and Rune Selection map internal ids and enum values to Korean display text.
