# Android Upgrade Screen Polish v0.9.0

## Scope

Issue #91 reorganizes `UpgradeScene` as a readable Android Portrait upgrade workshop while preserving the existing four upgrade records, Gold economy, local JSON persistence, and StageSelect navigation.

## Layout

- The header shows total purchased upgrade levels and the current Gold wallet.
- 1080 and 1440 Portrait layouts use a stable 2 x 2 card grid.
- 720 Portrait and landscape fallback layouts use a single-column scroll view.
- Each card contains a RuntimePixel icon, Korean name, role tag, level progress, current/next effect, price, and purchase state.
- The footer keeps purchase feedback separate from the navigation action.
- All card and sub-control rectangles are calculated by `UpgradeScreenLayout`; the scene root remains on the existing `GameFrameLayout` Safe Area.

## Purchase Feedback

- An affordable card uses its upgrade category accent color.
- An unaffordable card shows `골드 부족` and disables purchase.
- A completed card shows `강화 완료`.
- A successful purchase immediately refreshes the level, progress, current/next effect, wallet, and cost.
- The purchase summary remains visible for four seconds and the existing `UpgradeManager` save path persists the result immediately.

## Validation

- Unity Project Validator: passed.
- Unity Progression Smoke Test: passed.
- Android System Flows E2E: passed tutorial/save/reset/defeat/result navigation and upgrade flow.
- Android Full Chapter E2E: passed Stage 1-10, 10 upgrades, Grumbar phases 1-3, five reinforcements, and boss patterns.
- Android build: passed.
- Device: Android 15 / API 35 emulator, 1080 x 2400 Portrait.
- APK: `72,121,381 bytes`.
- SHA-256: `F205A1FADCD75E05A04CF08FDEC9E94E9B41C83AA361FFFE8C6238C51C7E0723`.
- Touch flow: StageSelect > Upgrade > Crystal Reinforcement purchase > StageSelect > Upgrade passed.
- Persistence: Gold `110 -> 30` and Crystal Reinforcement `Lv.0 -> Lv.1` remained after scene re-entry.
- Runtime log: no fatal exception, null reference, or missing reference was found during the manual flow.

## Remaining QA

- Physical-device cutout, system font scaling, and repeated rapid taps are not verified.
- Landscape uses the safe one-column fallback but is not a target release orientation.
- The four upgrade icons and panel frames remain prototype RuntimePixel assets.
