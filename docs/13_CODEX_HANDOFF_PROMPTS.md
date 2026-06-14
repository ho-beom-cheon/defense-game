# Codex Handoff Prompts — RuneGate Defense

이 파일은 Codex에 바로 붙여넣기 위한 프롬프트 모음이다.  
Codex는 이전 대화 맥락이 없다고 가정한다. 항상 레포의 Markdown 문서를 먼저 읽게 한다.

---

# Prompt 1 — v0.3 Progression Loop

```text
Read RUNEGATE_MASTER_PLAN.md and docs if they exist.
Then implement Task v0.3 Progression Loop according to docs/11_TASK_V03_PROGRESSION_LOOP.md.

Do not rely on prior chat context.

Current state:
The project has a playable battle prototype with 3 lanes, Crystal HP, Knight/Archer, Goblin/Orc, wave spawning, hero auto attack, manual skill buttons, rune selection, victory and defeat.

Goal:
Implement Title → Stage Select → Battle → Result → Upgrade → Save → Stage Select flow.

Must implement:
- TitleScene
- StageSelectScene
- BattleScene selected StageData loading
- Result flow
- UpgradeScene
- local JSON save system
- stage unlocks
- total gold
- permanent upgrades
- bootstrapper menu Tools/RuneGate/Bootstrap Progression Prototype
- validator menu Tools/RuneGate/Validate Project
- README update

Restrictions:
- no server
- no login
- no gacha
- no ads
- no IAP
- no analytics
- no Firebase
- no Addressables
- no external packages
- no final art integration

After implementation:
- Make the project compile.
- Run Unity validation if possible.
- List created/modified files.
- Explain manual Unity test steps.
```

---

# Prompt 2 — Fix v0.3 Compile Errors

```text
Unity compile errors occurred after implementing v0.3.

Task:
Fix all compile errors while preserving the v0.3 architecture.

Rules:
- Do not add external packages.
- Do not remove the save system.
- Do not remove stage progression.
- Do not remove upgrade progression.
- Keep namespace RuneGate.
- Keep ScriptableObject data-driven design.
- Do not rewrite the project into an unrelated simpler prototype.
- Make the minimum necessary changes.
- After fixing, run validation if possible.
- List files changed and explain the root cause.
```

---

# Prompt 3 — v0.3 Runtime Fix

```text
The project compiles, but the v0.3 progression flow does not work correctly.

Expected behavior:
TitleScene → StageSelectScene → BattleScene → Result → UpgradeScene → Save → StageSelectScene.

Task:
Inspect the scenes, managers, bootstrapper, and save system.
Fix the runtime flow.

Manual acceptance:
- Start opens StageSelectScene.
- Stage 1 is unlocked by default.
- Stage 2 and Stage 3 are locked initially.
- Clearing Stage 1 unlocks Stage 2.
- Gold persists after restarting Play Mode.
- Upgrade purchases persist.
- Reset Save clears progress.

Rules:
- No external packages.
- No server.
- No ads.
- No IAP.
- Preserve existing architecture.
```

---

# Prompt 4 — v0.4 Content Expansion

```text
Read RUNEGATE_MASTER_PLAN.md and docs if they exist.
Then implement Task v0.4 according to docs/12_TASK_V04_CONTENT_EXPANSION_AND_PLACEMENT.md.

Do not rely on prior chat context.

Current expected state:
v0.3 Progression Loop is implemented:
TitleScene, StageSelectScene, BattleScene, Result flow, UpgradeScene, local JSON save, stage unlocks, and permanent upgrades.

Goal:
Expand content and prepare long-term character/art scalability.

Must implement:
- 6 HeroData assets
- 6 MonsterData assets
- 1 boss MonsterData asset
- 6 SkillData assets
- 20 RuneData assets
- 10 StageData assets
- Stage unlock extension from 1 to 10
- 3 lanes x 3 hero placement slot foundation
- art folder structure
- ASSET_LIST_V04.md
- Bootstrap Content v0.4 menu
- Validator updates
- README update

Restrictions:
- Do not add ads
- Do not add IAP
- Do not add login
- Do not add server code
- Do not add analytics
- Do not add Firebase
- Do not add Addressables
- Do not add external packages
- Do not implement gacha
- Do not implement multiplayer
- Do not implement ranking
- Do not implement final art integration yet
- Do not break v0.3 loop

After implementation:
- Make the project compile.
- Run validation if possible.
- List created/modified files.
- Explain manual Unity test steps.
```

---

# Prompt 5 — Fix v0.4 Balance/Data Loading Issues

```text
v0.4 content assets were generated, but battle data or stage loading has issues.

Task:
Fix content loading and stage data execution.

Check:
- Stage 1 to Stage 10 assets exist.
- StageSelectScene lists Stage 1 to Stage 10.
- Locked/unlocked states work.
- BattleScene loads selected StageData.
- Waves spawn the intended MonsterData.
- Boss wave works in Stage 10.
- Rune selection does not break when unsupported rune effect keys appear.
- Missing sprites do not cause null reference exceptions.

Rules:
- Keep placeholder visuals.
- Do not add final art.
- Do not add external packages.
- Do not add server/login/ads/IAP.
- Fix the minimum necessary code.
```

---

# Prompt 6 — Documentation Sync

```text
Update project documentation to match the current implementation.

Read the code and assets first.

Update:
- README.md
- RUNEGATE_MASTER_PLAN.md if needed
- docs/02_DEVELOPMENT_ROADMAP.md
- docs/09_BACKLOG.md
- docs/10_NEXT_CODEX_PROMPTS.md

Do not invent features that are not implemented.
Clearly mark planned features as planned.
Keep MVP exclusions visible:
- no server
- no login
- no gacha
- no ads
- no IAP
- no analytics
- no external paid APIs
- no multiplayer
- no ranking
- no cloud save
```
