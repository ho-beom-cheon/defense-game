# RuneGate Defense

RuneGate Defense is a Unity 6 mobile 2D portrait defense prototype. Monsters invade through lane paths toward a Kingdom Crystal, while heroes auto-attack and can later trigger manual skills. After each wave, the player chooses one of three rune cards to modify the current run.

## Unity Target

- Unity 6
- 2D mobile portrait project
- Local-only MVP

## How To Bootstrap The MVP

1. Open this folder as a Unity 6 project.
2. Wait for scripts to compile.
3. In the Unity menu, run `Tools/RuneGate/Bootstrap MVP`.
4. Open `Assets/_Project/Scenes/BattleScene.unity`.
5. Press Play.

The bootstrapper creates sample ScriptableObject assets for Knight, Archer, Goblin, Orc, Shield Bash, Rapid Shot, Sword Rune, Bow Rune, Healing Rune, and Goblin Forest 1. It also creates a basic BattleScene with lane points, a crystal, two heroes, and runtime managers.

## MVP Scope

- 3-lane battlefield support
- Kingdom Crystal HP and defeat handling
- Stage and wave data driven by ScriptableObjects
- Monster spawning by wave data
- Monster movement toward the crystal
- Hero auto attacks
- Manual skill trigger hooks
- Rune selection and rune effect application after non-final waves
- Victory handling after the final wave
- Compile-safe UI placeholder scripts without requiring TextMeshPro or UGUI

## MVP Exclusions

- No server
- No login
- No gacha
- No ads
- No in-app purchase
- No analytics
- No external paid APIs
- No multiplayer
- No ranking
- No cloud save

## Folder Structure

```text
Assets/
  _Project/
    Scripts/
      Core/
      Battle/
      Hero/
      Monster/
      Skill/
      Rune/
      Wave/
      Data/
      Save/
      UI/
      Editor/
    Data/
      Heroes/
      Monsters/
      Skills/
      Runes/
      Stages/
    Prefabs/
      Heroes/
      Monsters/
      UI/
    Scenes/
    Art/
      Characters/
      Monsters/
      Effects/
      UI/
    Audio/
    Resources/
```

## Coding Rules

- Use the `RuneGate` namespace.
- Keep MonoBehaviours small and focused.
- Use ScriptableObjects for balance and content data.
- Prefer serialized private fields over public mutable fields.
- Use events where UI observes battle state.
- Keep battle logic separate from UI logic.
- Add null guards and clear `Debug.LogWarning` messages.
- Avoid singletons unless there is a strong reason.
- Avoid networking, monetization, analytics, Addressables, and paid dependencies during the MVP.

## Next Implementation Checklist

- Replace placeholder sprites and scene objects with real 2D art.
- Build actual UGUI or TextMeshPro HUD bindings on top of the placeholder UI scripts.
- Add hero placement and lane assignment flow.
- Add skill type data so direct damage, area damage, healing, and buffs are explicit.
- Expand rune effects beyond the current prototype hooks.
- Add monster special behavior for flying, splitter, undead, and boss types.
- Add local save data for unlocked heroes, settings, and stage progress.
- Add mobile input polish, portrait camera framing, and screen-safe UI layout.
