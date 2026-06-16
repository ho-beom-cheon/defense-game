# Known Issues

## v0.6

- Final Knight and Goblin art is not included. Placeholder visual prefabs remain the fallback.
- Android APK build depends on local Unity Android Build Support installation.

## v0.7

- Monster special abilities are represented by stat differences and placeholder hooks.
- Chain shot, turret support, cleansing, shield, and armor break rune effects are simplified placeholders.
- Stage 1-10 balance is a first playable pass and still needs measured Unity playtest timing.
- Grumbar appears in Stage 10, but boss phase change and summon patterns are hook-only.

## v0.8

- Tutorial uses a simple IMGUI overlay instead of a full touch-driven UI flow.
- Settings are placeholders for BGM, SFX, and vibration.
- Safe area support is prepared as a component, but the current prototype UI is still IMGUI based.

## v0.9

- Device install and long-session performance testing must be completed manually.
- APK build succeeded locally on 2026-06-15, but device install testing is still pending.
- AAB build has not been automated separately from APK.

## v1.0

- No real monetization SDK is included.
- No final store art, screenshots, or app icon set is included.
- Balance is first-pass and should be tuned with playtest data.

## Runtime Pixel Art

- First-pass RuntimePixel idle sprites exist for six heroes, six monsters, and Grumbar boss.
- BattleScene intentionally uses small colored placeholder fallback when a future `battleSprite` or `runtimeSprite` is empty.
- ConceptSheets images are reference/codex assets only and should not appear directly in BattleScene SpriteRenderers.
- UI is still IMGUI-based, so additional mobile safe-area polish remains for a future pass.
- Combat feedback is placeholder-based: attack lunge, hit flash, damage text, death fade, and skill effects work, but final pixel VFX and animation strips are still needed.
- Boss HP is still the per-unit runtime HP bar. A dedicated boss HP UI is planned for a later pass.
- BattleScene now uses the RuntimePixel goblin forest lane background and first-pass effect sprites, but lane strip overlays remain runtime placeholders.
- UI image application is limited by the current IMGUI prototype. Final mobile UI should move to Canvas/uGUI or UI Toolkit with safe-area handling.
