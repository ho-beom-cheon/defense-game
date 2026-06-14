# RuneGate Defense v0.5 Art Integration Plan

## Goal

v0.5 validates the first art integration pipeline without adding final art, paid assets, external packages, Addressables, Firebase, or online systems.

The prototype keeps the existing progression and battle loop while adding stable hooks for:

- Character combat prefabs
- SpriteRenderer and Animator based visuals
- Hit flash, damage text, death delay, and projectile feedback
- Optional audio feedback through local AudioSource clips
- UI-ready icon fields on skills, runes, and upgrades

## Bootstrap Menu

Run this menu in Unity:

`Tools/RuneGate/Bootstrap v0.5 Art Prototype`

It creates or updates the normal v0.4 content, then adds the v0.5 placeholder art pipeline.

## Generated Placeholder Assets

The bootstrapper creates package-free placeholder prefabs:

- `Assets/_Project/Prefabs/Heroes/Hero_Knight.prefab`
- `Assets/_Project/Prefabs/Monsters/Monster_Goblin.prefab`
- `Assets/_Project/Prefabs/Projectiles/Projectile_Arrow.prefab`
- `Assets/_Project/Prefabs/Effects/Effect_Hit_Small.prefab`
- `Assets/_Project/Prefabs/Effects/Effect_Death_Small.prefab`

It also creates animator controllers:

- `Assets/_Project/Art/Characters/Heroes/Knight/Animations/Knight_Prototype.controller`
- `Assets/_Project/Art/Characters/Monsters/Goblin/Animations/Goblin_Prototype.controller`

## Runtime Components

- `CharacterVisualController` wraps SpriteRenderer and Animator calls for move, attack, hit, death, skill, and facing direction.
- `HitFlashController` provides a reusable color flash for characters and the crystal.
- `AutoDestroyEffect` lets small placeholder effects clean themselves up.
- `AudioManager` and `SfxKey` provide optional local audio hooks. Missing clips are safe and do not block gameplay.

## Data Rules

- `HeroData` can point to a battle sprite, animator controller, and prefab.
- `MonsterData` can point to a sprite, animator controller, prefab, and boss flag.
- `SkillData`, `RuneData`, and `UpgradeData` can carry icons.
- Runtime systems must not mutate ScriptableObject source values.

## Validation

After bootstrapping, run:

`Tools/RuneGate/Validate v0.5 Art Prototype`

The validator checks folders, scripts, docs, v0.4 content, v0.5 placeholder prefabs, animator controllers, and Knight/Goblin data links.

## Current Limitations

- Placeholder shapes are still used instead of final character art.
- Animator controllers have required states and parameters, but no final animation clips.
- Audio clips are not bundled; hooks are present and safe when empty.
- Only Knight and Goblin are wired as the first art-pipeline proof. Other heroes and monsters keep fallback placeholder generation until their prefabs are added.
