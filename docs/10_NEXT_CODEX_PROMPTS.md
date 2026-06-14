# 다음 Codex 프롬프트 모음

## 1. 문서 반영 프롬프트

```text
You are working on the Unity project RuneGate Defense.

Task:
Add the provided docs folder into the repository and update README.md with a short link section pointing to the docs.

Rules:
- Do not change gameplay code.
- Do not modify Unity scenes.
- Do not add external packages.
- Keep markdown files under docs/.
- After adding, list files added and suggest next implementation step.
```

## 2. v0.2 구현 프롬프트

```text
Read these documents first:
- docs/00_PROJECT_CONTEXT.md
- docs/01_GAME_DESIGN_DOCUMENT.md
- docs/02_DEVELOPMENT_ROADMAP.md
- docs/03_CODEX_IMPLEMENTATION_GUIDE.md
- docs/04_SYSTEM_ARCHITECTURE.md
- docs/05_CONTENT_AND_BALANCE_SPEC.md
- docs/06_UI_UX_SPEC.md
- docs/08_TEST_AND_QA_PLAN.md

Then implement Battle Prototype v0.2.

Current state:
BattleScene already runs with 3 lanes, Crystal HP, Wave display, Gold display, Knight/Archer stats, skill buttons, placeholder heroes, and placeholder monsters.

Goal:
Complete the core battle loop.

Scope:
1. Wave clear flow
- Detect when all spawns and alive monsters of the current wave are cleared.
- After each non-final wave, pause battle and enter RuneSelection state.
- Show 3 rune choices.
- When a rune is selected, apply its effect and start the next wave.

2. Functional rune effects
Implement these effect keys:
- hero_attack_percent
- hero_attack_speed_percent
- crystal_heal_flat

Expected sample runes:
- Sword Rune: hero_attack_percent +20%
- Bow Rune: hero_attack_speed_percent +15%
- Healing Rune: crystal_heal_flat +30

3. Victory and defeat
- If Crystal HP reaches 0, enter Defeat state.
- If the final wave is cleared, enter Victory state.
- Show a simple result panel.
- Add Restart button.
- Back to Title can be a placeholder log.

4. Monster feedback
- Add simple HP bar above monsters.
- Add hit flash feedback when monsters take damage.
- Remove monster cleanly on death.
- Add simple Crystal hit feedback when damaged.

5. Skill button improvement
- Show skill cooldown state.
- Disable skill button during cooldown.
- Re-enable when cooldown is complete.
- Knight skill should damage or stun/knockback the nearest enemy in range.
- Archer skill should rapidly damage the nearest enemy or highest HP enemy.
- Keep implementation simple and compile-safe.

6. Placement slot foundation
- Define 3 lanes x 3 logical hero slots.
- Keep current Knight and Archer placement if needed.
- Structure the code so later drag-and-drop placement can be added.
- Do not implement drag-and-drop yet.

Restrictions:
- Do not add ads.
- Do not add IAP.
- Do not add login.
- Do not add server code.
- Do not add analytics.
- Do not add Firebase.
- Do not add Addressables yet.
- Do not add external packages.
- Do not import art assets.
- Use placeholder UI and primitive visuals only.
- Keep namespace RuneGate.
- Preserve existing architecture.
- Keep ScriptableObject data-driven design.
- Avoid hardcoding balance values directly in battle logic where data objects already exist.

Validation:
- Make the project compile.
- If Unity command-line validation is available, run it.
- If not, explain how to manually test in Unity.

Manual test criteria:
1. Open BattleScene.
2. Press Play.
3. Wave 1 starts.
4. Monsters spawn and move.
5. Knight and Archer attack.
6. Monsters take damage and show HP feedback.
7. When Wave 1 clears, rune selection appears.
8. Select a rune.
9. Rune effect applies.
10. Wave 2 starts.
11. Final wave clear shows Victory.
12. Crystal HP 0 shows Defeat.
13. Restart starts the battle again.

After implementation:
- List created/modified files.
- Explain how to test the prototype.
- Report any remaining limitations.
```

## 3. 컴파일 에러 수정 프롬프트

```text
Unity compile errors occurred.

Task:
Fix all compile errors while preserving the current architecture.

Rules:
- Read docs/03_CODEX_IMPLEMENTATION_GUIDE.md first.
- Do not add external packages.
- Do not remove core gameplay systems.
- Do not replace the architecture with an unrelated simpler version.
- Keep namespace RuneGate.
- Keep ScriptableObject data design.
- Keep the playable prototype goal.
- After fixing, run Unity validation again if possible.
- If Unity validation cannot be run, explain exact manual Unity steps.

Before editing:
- Inspect Console/log output.
- Identify root causes.
- Fix the minimum necessary code.
- Report changed files.
```

## 4. BattleScene 실행 문제 수정 프롬프트

```text
The project compiles, but BattleScene does not run correctly.

Task:
Make BattleScene playable according to docs/08_TEST_AND_QA_PLAN.md.

Expected behavior:
- Press Play.
- Monsters spawn.
- Heroes attack automatically.
- Crystal HP decreases when monsters reach it.
- Rune selection appears after a wave.
- Victory and Defeat states work.
- Restart works.

Rules:
- Use placeholder visuals only.
- Do not add art polish.
- Do not add ads, IAP, server, analytics, Firebase, or external packages.
- Prefer simple reliable gameplay over complex effects.
- After changes, explain exactly how to test in Unity.
```
