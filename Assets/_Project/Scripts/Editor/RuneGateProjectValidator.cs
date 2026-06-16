using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RuneGate.Editor
{
    public static class RuneGateProjectValidator
    {
        private static readonly string[] RequiredFolders =
        {
            "Assets/_Project/Scripts/Core",
            "Assets/_Project/Scripts/Battle",
            "Assets/_Project/Scripts/Hero",
            "Assets/_Project/Scripts/Monster",
            "Assets/_Project/Scripts/Skill",
            "Assets/_Project/Scripts/Rune",
            "Assets/_Project/Scripts/Wave",
            "Assets/_Project/Scripts/Data",
            "Assets/_Project/Scripts/Progression",
            "Assets/_Project/Scripts/Save",
            "Assets/_Project/Scripts/UI",
            "Assets/_Project/Scripts/Editor",
            "Assets/_Project/Scripts/Audio",
            "Assets/_Project/Fonts",
            "Assets/_Project/Data/Heroes",
            "Assets/_Project/Data/Monsters",
            "Assets/_Project/Data/Skills",
            "Assets/_Project/Data/Runes",
            "Assets/_Project/Data/Stages",
            "Assets/_Project/Data/Upgrades",
            "Assets/_Project/Data/Formations",
            "Assets/_Project/Data/Rosters",
            "Assets/_Project/Prefabs/Heroes",
            "Assets/_Project/Prefabs/Monsters",
            "Assets/_Project/Prefabs/Projectiles",
            "Assets/_Project/Prefabs/Effects",
            "Assets/_Project/Prefabs/UI",
            "Assets/_Project/Scenes",
            "Assets/_Project/Art/Characters/Heroes/Knight",
            "Assets/_Project/Art/Characters/Heroes/Knight/Sprites",
            "Assets/_Project/Art/Characters/Heroes/Knight/Animations",
            "Assets/_Project/Art/Characters/Heroes/Knight/Materials",
            "Assets/_Project/Art/Characters/Heroes/Archer",
            "Assets/_Project/Art/Characters/Heroes/Archer/Sprites",
            "Assets/_Project/Art/Characters/Heroes/Archer/Animations",
            "Assets/_Project/Art/Characters/Heroes/Archer/Materials",
            "Assets/_Project/Art/Characters/Heroes/FireMage",
            "Assets/_Project/Art/Characters/Heroes/FireMage/Sprites",
            "Assets/_Project/Art/Characters/Heroes/FireMage/Animations",
            "Assets/_Project/Art/Characters/Heroes/FireMage/Materials",
            "Assets/_Project/Art/Characters/Heroes/Cleric",
            "Assets/_Project/Art/Characters/Heroes/Cleric/Sprites",
            "Assets/_Project/Art/Characters/Heroes/Cleric/Animations",
            "Assets/_Project/Art/Characters/Heroes/Cleric/Materials",
            "Assets/_Project/Art/Characters/Heroes/Priest",
            "Assets/_Project/Art/Characters/Heroes/Priest/Sprites",
            "Assets/_Project/Art/Characters/Heroes/Priest/Animations",
            "Assets/_Project/Art/Characters/Heroes/Priest/Materials",
            "Assets/_Project/Art/Characters/Heroes/DwarfEngineer",
            "Assets/_Project/Art/Characters/Heroes/DwarfEngineer/Sprites",
            "Assets/_Project/Art/Characters/Heroes/DwarfEngineer/Animations",
            "Assets/_Project/Art/Characters/Heroes/DwarfEngineer/Materials",
            "Assets/_Project/Art/Characters/Heroes/Assassin",
            "Assets/_Project/Art/Characters/Heroes/Assassin/Sprites",
            "Assets/_Project/Art/Characters/Heroes/Assassin/Animations",
            "Assets/_Project/Art/Characters/Heroes/Assassin/Materials",
            "Assets/_Project/Art/Characters/Monsters/Goblin",
            "Assets/_Project/Art/Characters/Monsters/Goblin/Sprites",
            "Assets/_Project/Art/Characters/Monsters/Goblin/Animations",
            "Assets/_Project/Art/Characters/Monsters/Goblin/Materials",
            "Assets/_Project/Art/Characters/Monsters/Wolf",
            "Assets/_Project/Art/Characters/Monsters/Wolf/Sprites",
            "Assets/_Project/Art/Characters/Monsters/Wolf/Animations",
            "Assets/_Project/Art/Characters/Monsters/Wolf/Materials",
            "Assets/_Project/Art/Characters/Monsters/Orc",
            "Assets/_Project/Art/Characters/Monsters/Orc/Sprites",
            "Assets/_Project/Art/Characters/Monsters/Orc/Animations",
            "Assets/_Project/Art/Characters/Monsters/Orc/Materials",
            "Assets/_Project/Art/Characters/Monsters/Bat",
            "Assets/_Project/Art/Characters/Monsters/Bat/Sprites",
            "Assets/_Project/Art/Characters/Monsters/Bat/Animations",
            "Assets/_Project/Art/Characters/Monsters/Bat/Materials",
            "Assets/_Project/Art/Characters/Monsters/Slime",
            "Assets/_Project/Art/Characters/Monsters/Slime/Sprites",
            "Assets/_Project/Art/Characters/Monsters/Slime/Animations",
            "Assets/_Project/Art/Characters/Monsters/Slime/Materials",
            "Assets/_Project/Art/Characters/Monsters/Skeleton",
            "Assets/_Project/Art/Characters/Monsters/Skeleton/Sprites",
            "Assets/_Project/Art/Characters/Monsters/Skeleton/Animations",
            "Assets/_Project/Art/Characters/Monsters/Skeleton/Materials",
            "Assets/_Project/Art/Characters/Bosses/OrcWarlord",
            "Assets/_Project/Art/Characters/Bosses/OrcWarlord/Sprites",
            "Assets/_Project/Art/Characters/Bosses/OrcWarlord/Animations",
            "Assets/_Project/Art/Characters/Bosses/OrcWarlord/Materials",
            "Assets/_Project/Art/ConceptSheets",
            "Assets/_Project/Art/ConceptSheets/Heroes",
            "Assets/_Project/Art/ConceptSheets/Enemies",
            "Assets/_Project/Art/RuntimePixel",
            "Assets/_Project/Art/RuntimePixel/Backgrounds",
            "Assets/_Project/Art/RuntimePixel/Effects",
            "Assets/_Project/Art/RuntimePixel/Heroes",
            "Assets/_Project/Art/RuntimePixel/Heroes/Leon",
            "Assets/_Project/Art/RuntimePixel/Heroes/Seria",
            "Assets/_Project/Art/RuntimePixel/Heroes/Kael",
            "Assets/_Project/Art/RuntimePixel/Heroes/Mirea",
            "Assets/_Project/Art/RuntimePixel/Heroes/Brom",
            "Assets/_Project/Art/RuntimePixel/Heroes/Nyx",
            "Assets/_Project/Art/RuntimePixel/Monsters",
            "Assets/_Project/Art/RuntimePixel/Monsters/GateImp",
            "Assets/_Project/Art/RuntimePixel/Monsters/OrcBrute",
            "Assets/_Project/Art/RuntimePixel/Monsters/DireWolf",
            "Assets/_Project/Art/RuntimePixel/Monsters/CaveBat",
            "Assets/_Project/Art/RuntimePixel/Monsters/CoreSlime",
            "Assets/_Project/Art/RuntimePixel/Monsters/BoneSoldier",
            "Assets/_Project/Art/RuntimePixel/Bosses",
            "Assets/_Project/Art/RuntimePixel/Bosses/Grumbar",
            "Assets/_Project/Art/RuntimePixel/UI",
            "Assets/_Project/Art/Effects/Skills",
            "Assets/_Project/Art/Effects/Hit",
            "Assets/_Project/Art/Effects/Projectiles",
            "Assets/_Project/Art/Effects/Death",
            "Assets/_Project/Art/UI/Icons",
            "Assets/_Project/Art/UI/Icons/Heroes",
            "Assets/_Project/Art/UI/Icons/Skills",
            "Assets/_Project/Art/UI/Icons/Runes",
            "Assets/_Project/Art/UI/Icons/Upgrades",
            "Assets/_Project/Art/UI/Buttons",
            "Assets/_Project/Art/UI/Panels",
            "Assets/_Project/Art/UI/Runes",
            "Assets/_Project/Art/UI/Bars",
            "Assets/_Project/Art/Backgrounds",
            "Assets/_Project/Art/Placeholders",
            "Assets/_Project/Art/Audio/SFX",
            "Assets/_Project/Art/Audio/BGM",
            "Assets/_Project/Audio",
            "Assets/_Project/Audio/SFX",
            "Assets/_Project/Audio/BGM",
            "Assets/_Project/Resources"
        };

        private static readonly string[] RequiredScripts =
        {
            "Assets/_Project/Scripts/Core/ElementType.cs",
            "Assets/_Project/Scripts/Core/HeroRole.cs",
            "Assets/_Project/Scripts/Core/HeroPositionType.cs",
            "Assets/_Project/Scripts/Core/MonsterType.cs",
            "Assets/_Project/Scripts/Core/BattleState.cs",
            "Assets/_Project/Scripts/Core/RuneRarity.cs",
            "Assets/_Project/Scripts/Core/TargetingType.cs",
            "Assets/_Project/Scripts/Core/GameSession.cs",
            "Assets/_Project/Scripts/Data/HeroData.cs",
            "Assets/_Project/Scripts/Data/MonsterData.cs",
            "Assets/_Project/Scripts/Data/SkillData.cs",
            "Assets/_Project/Scripts/Data/FormationData.cs",
            "Assets/_Project/Scripts/Data/FormationSlot.cs",
            "Assets/_Project/Scripts/Data/HeroRosterData.cs",
            "Assets/_Project/Scripts/Data/RuneData.cs",
            "Assets/_Project/Scripts/Data/StageData.cs",
            "Assets/_Project/Scripts/Data/WaveData.cs",
            "Assets/_Project/Scripts/Data/WaveSpawnData.cs",
            "Assets/_Project/Scripts/Data/UpgradeData.cs",
            "Assets/_Project/Scripts/Save/SaveData.cs",
            "Assets/_Project/Scripts/Save/SaveManager.cs",
            "Assets/_Project/Scripts/Save/SerializableUpgradeLevel.cs",
            "Assets/_Project/Scripts/Progression/UpgradeManager.cs",
            "Assets/_Project/Scripts/Battle/BattleManager.cs",
            "Assets/_Project/Scripts/Battle/LaneManager.cs",
            "Assets/_Project/Scripts/Battle/CrystalController.cs",
            "Assets/_Project/Scripts/Battle/CharacterVisualController.cs",
            "Assets/_Project/Scripts/Battle/HitFlashController.cs",
            "Assets/_Project/Scripts/Battle/AutoDestroyEffect.cs",
            "Assets/_Project/Scripts/Battle/CombatVisualEffectFactory.cs",
            "Assets/_Project/Scripts/Battle/RuntimePixelAssetLoader.cs",
            "Assets/_Project/Scripts/Battle/RuntimePixelVisualCatalog.cs",
            "Assets/_Project/Scripts/Battle/RuntimeSpriteFitter.cs",
            "Assets/_Project/Scripts/Battle/RuntimeSpritePolicy.cs",
            "Assets/_Project/Scripts/Hero/HeroController.cs",
            "Assets/_Project/Scripts/Hero/HeroPlacementManager.cs",
            "Assets/_Project/Scripts/Hero/HeroPlacementSlot.cs",
            "Assets/_Project/Scripts/Monster/MonsterController.cs",
            "Assets/_Project/Scripts/Battle/ProjectileController.cs",
            "Assets/_Project/Scripts/Wave/WaveManager.cs",
            "Assets/_Project/Scripts/Rune/RuneManager.cs",
            "Assets/_Project/Scripts/Rune/RuneEffectApplier.cs",
            "Assets/_Project/Scripts/Skill/SkillController.cs",
            "Assets/_Project/Scripts/Battle/BattleResult.cs",
            "Assets/_Project/Scripts/UI/BattleHUD.cs",
            "Assets/_Project/Scripts/UI/RuneSelectionUI.cs",
            "Assets/_Project/Scripts/UI/HeroSkillButton.cs",
            "Assets/_Project/Scripts/UI/FormationSkillPanelUI.cs",
            "Assets/_Project/Scripts/UI/StageResultUI.cs",
            "Assets/_Project/Scripts/UI/TitleUI.cs",
            "Assets/_Project/Scripts/UI/StageSelectUI.cs",
            "Assets/_Project/Scripts/UI/UpgradeSceneUI.cs",
            "Assets/_Project/Scripts/UI/TutorialStepData.cs",
            "Assets/_Project/Scripts/UI/TutorialManager.cs",
            "Assets/_Project/Scripts/UI/TutorialOverlayUI.cs",
            "Assets/_Project/Scripts/UI/RuntimePixelGuiUtility.cs",
            "Assets/_Project/Scripts/UI/KoreanFontCatalog.cs",
            "Assets/_Project/Scripts/UI/KoreanFontManager.cs",
            "Assets/_Project/Scripts/UI/SafeAreaFitter.cs",
            "Assets/_Project/Scripts/Audio/AudioManager.cs",
            "Assets/_Project/Scripts/Audio/SfxKey.cs",
            "Assets/_Project/Scripts/Editor/RuneGateBootstrapper.cs"
        };

        private static readonly string[] RequiredScenes =
        {
            "Assets/_Project/Scenes/TitleScene.unity",
            "Assets/_Project/Scenes/StageSelectScene.unity",
            "Assets/_Project/Scenes/BattleScene.unity",
            "Assets/_Project/Scenes/UpgradeScene.unity"
        };

        private static readonly string[] RequiredStageAssets =
        {
            "Assets/_Project/Data/Stages/Goblin Forest 1.asset",
            "Assets/_Project/Data/Stages/Goblin Forest 2.asset",
            "Assets/_Project/Data/Stages/Goblin Forest 3.asset",
            "Assets/_Project/Data/Stages/Goblin Forest 4.asset",
            "Assets/_Project/Data/Stages/Goblin Forest 5.asset",
            "Assets/_Project/Data/Stages/Goblin Forest 6.asset",
            "Assets/_Project/Data/Stages/Goblin Forest 7.asset",
            "Assets/_Project/Data/Stages/Goblin Forest 8.asset",
            "Assets/_Project/Data/Stages/Goblin Forest 9.asset",
            "Assets/_Project/Data/Stages/Goblin Forest 10.asset"
        };

        private static readonly string[] RequiredHeroAssets =
        {
            "Assets/_Project/Data/Heroes/Knight.asset",
            "Assets/_Project/Data/Heroes/Archer.asset",
            "Assets/_Project/Data/Heroes/Fire Mage.asset",
            "Assets/_Project/Data/Heroes/Priest.asset",
            "Assets/_Project/Data/Heroes/Dwarf Engineer.asset",
            "Assets/_Project/Data/Heroes/Shadow Assassin.asset"
        };

        private static readonly string[] RequiredMonsterAssets =
        {
            "Assets/_Project/Data/Monsters/Goblin.asset",
            "Assets/_Project/Data/Monsters/Wolf.asset",
            "Assets/_Project/Data/Monsters/Orc.asset",
            "Assets/_Project/Data/Monsters/Bat.asset",
            "Assets/_Project/Data/Monsters/Slime.asset",
            "Assets/_Project/Data/Monsters/Skeleton.asset",
            "Assets/_Project/Data/Monsters/Orc Warlord.asset"
        };

        private static readonly string[] RequiredSkillAssets =
        {
            "Assets/_Project/Data/Skills/Shield Bash.asset",
            "Assets/_Project/Data/Skills/Rapid Shot.asset",
            "Assets/_Project/Data/Skills/Meteor.asset",
            "Assets/_Project/Data/Skills/Holy Heal.asset",
            "Assets/_Project/Data/Skills/Build Turret.asset",
            "Assets/_Project/Data/Skills/Shadow Strike.asset"
        };

        private static readonly string[] RequiredRuneAssets =
        {
            "Assets/_Project/Data/Runes/Sword Rune.asset",
            "Assets/_Project/Data/Runes/Bow Rune.asset",
            "Assets/_Project/Data/Runes/Healing Rune.asset",
            "Assets/_Project/Data/Runes/Fire Rune.asset",
            "Assets/_Project/Data/Runes/Shield Rune.asset",
            "Assets/_Project/Data/Runes/Command Rune.asset",
            "Assets/_Project/Data/Runes/Focus Rune.asset",
            "Assets/_Project/Data/Runes/Explosion Rune.asset",
            "Assets/_Project/Data/Runes/Haste Rune.asset",
            "Assets/_Project/Data/Runes/Frost Rune.asset",
            "Assets/_Project/Data/Runes/Lightning Rune.asset",
            "Assets/_Project/Data/Runes/Earth Rune.asset",
            "Assets/_Project/Data/Runes/Sacrifice Rune.asset",
            "Assets/_Project/Data/Runes/Guardian Rune.asset",
            "Assets/_Project/Data/Runes/Mana Rune.asset",
            "Assets/_Project/Data/Runes/Hunter Rune.asset",
            "Assets/_Project/Data/Runes/Purification Rune.asset",
            "Assets/_Project/Data/Runes/Shatter Rune.asset",
            "Assets/_Project/Data/Runes/Chain Rune.asset",
            "Assets/_Project/Data/Runes/Turret Rune.asset"
        };

        private static readonly string[] RequiredFormationAssets =
        {
            "Assets/_Project/Data/Formations/Default Formation.asset",
            "Assets/_Project/Data/Rosters/MVP Hero Roster.asset"
        };

        private static readonly string[] RequiredUpgradeAssets =
        {
            "Assets/_Project/Data/Upgrades/Crystal Reinforcement.asset",
            "Assets/_Project/Data/Upgrades/Hero Training.asset",
            "Assets/_Project/Data/Upgrades/Battle Rhythm.asset",
            "Assets/_Project/Data/Upgrades/Skill Practice.asset"
        };

        private static readonly string[] RequiredV05Assets =
        {
            "Assets/_Project/Prefabs/Heroes/Hero_Knight.prefab",
            "Assets/_Project/Prefabs/Monsters/Monster_Goblin.prefab",
            "Assets/_Project/Prefabs/Projectiles/Projectile_Arrow.prefab",
            "Assets/_Project/Prefabs/Effects/Effect_Hit_Small.prefab",
            "Assets/_Project/Prefabs/Effects/Effect_Death_Small.prefab",
            "Assets/_Project/Art/Characters/Heroes/Knight/Animations/Knight_Prototype.controller",
            "Assets/_Project/Art/Characters/Monsters/Goblin/Animations/Goblin_Prototype.controller"
        };

        private static readonly string[] RequiredCombatVisualPolishAssets =
        {
            "Assets/_Project/Art/RuntimePixel/Backgrounds/bg_goblin_forest_lanes.png",
            "Assets/_Project/Art/RuntimePixel/Effects/fx_shield_bash.png",
            "Assets/_Project/Art/RuntimePixel/Effects/fx_rapid_shot.png",
            "Assets/_Project/Art/RuntimePixel/Effects/fx_meteor_impact.png",
            "Assets/_Project/Art/RuntimePixel/Effects/fx_holy_heal.png",
            "Assets/_Project/Art/RuntimePixel/Effects/fx_turret_shot.png",
            "Assets/_Project/Art/RuntimePixel/Effects/fx_shadow_slash.png",
            "Assets/_Project/Art/RuntimePixel/Effects/fx_hit_spark.png",
            "Assets/_Project/Art/RuntimePixel/Effects/fx_death_puff.png",
            "Assets/_Project/Art/RuntimePixel/UI/ui_panel_dark.png",
            "Assets/_Project/Art/RuntimePixel/UI/ui_button_skill.png",
            "Assets/_Project/Art/RuntimePixel/UI/ui_rune_card_base.png",
            "Assets/_Project/Art/RuntimePixel/UI/Tutorial/ui_tutorial_arrow.png",
            "Assets/_Project/Art/RuntimePixel/UI/Tutorial/ui_tap_indicator.png",
            "Assets/_Project/Art/RuntimePixel/UI/StageSelect/ui_stage_node_unlocked.png",
            "Assets/_Project/Art/RuntimePixel/UI/StageSelect/ui_stage_node_locked.png",
            "Assets/_Project/Art/RuntimePixel/UI/StageSelect/ui_stage_node_cleared.png",
            "Assets/_Project/Art/RuntimePixel/UI/Upgrade/icon_upgrade_crystal_hp.png",
            "Assets/_Project/Art/RuntimePixel/UI/Upgrade/icon_upgrade_hero_attack.png",
            "Assets/_Project/Art/RuntimePixel/UI/Upgrade/icon_upgrade_attack_speed.png",
            "Assets/_Project/Art/RuntimePixel/UI/Upgrade/icon_upgrade_skill_cooldown.png",
            "Assets/_Project/Art/RuntimePixel/UI/System/ui_icon_settings.png",
            "Assets/_Project/Art/RuntimePixel/UI/System/ui_icon_reset_save.png",
            "Assets/_Project/Art/RuntimePixel/UI/System/ui_icon_save.png",
            "Assets/_Project/Art/RuntimePixel/UI/Difficulty/badge_easy.png",
            "Assets/_Project/Art/RuntimePixel/UI/Difficulty/badge_normal.png",
            "Assets/_Project/Art/RuntimePixel/UI/Difficulty/badge_hard.png",
            "Assets/_Project/Art/RuntimePixel/UI/Difficulty/badge_nightmare.png"
        };

        private const string RequiredCombatVisualCatalogAsset = "Assets/_Project/Resources/RuntimePixelVisualCatalog.asset";
        private const string RequiredKoreanFontCatalogAsset = "Assets/_Project/Resources/KoreanFontCatalog.asset";

        private static readonly string[] RequiredKoreanFontAssets =
        {
            "Assets/_Project/Fonts/NotoSansKR-Regular.ttf",
            "Assets/_Project/Fonts/NotoSansKR-SemiBold.ttf",
            "Assets/_Project/Fonts/NotoSansKR-Bold.ttf"
        };

        private static readonly string[] RequiredDocs =
        {
            "docs/art-guide.md",
            "docs/asset-list.md",
            "docs/v05-art-integration-plan.md",
            "docs/android-build-guide.md",
            "docs/release-checklist.md",
            "docs/known-issues.md",
            "docs/store-listing-draft.md",
            "docs/privacy-checklist.md",
            "docs/release-notes-v1.0.md",
            "docs/hero-character-bible.md",
            "docs/enemy-boss-bible.md",
            "docs/korean-world-identity-guide.md",
            "docs/pixel-art-pipeline.md",
            "docs/art-integration-notes.md",
            "docs/content-balance-v07.md",
            "docs/stage-design.md",
            "docs/rune-design.md",
            "docs/ui-ux-v08.md",
            "docs/tutorial-design.md",
            "docs/save-system.md",
            "docs/difficulty-design.md",
            "docs/korean-font-setup.md",
            "CHANGELOG.md"
        };

        [MenuItem("Tools/RuneGate/Validate Project")]
        public static void ValidateFromMenu()
        {
            bool valid = ValidateProject(out List<string> errors);
            if (valid)
            {
                Debug.Log("RuneGate validation passed.");
                Debug.Log($"RuneGate save path: {SaveManager.SavePath}");
                return;
            }

            Debug.LogError("RuneGate validation failed:\n" + string.Join("\n", errors));
        }

        [MenuItem("Tools/RuneGate/Validate v0.4 Content Prototype")]
        public static void ValidateV04FromMenu()
        {
            ValidateFromMenu();
        }

        [MenuItem("Tools/RuneGate/Validate v0.5 Art Prototype")]
        public static void ValidateV05FromMenu()
        {
            ValidateFromMenu();
        }

        [MenuItem("Tools/RuneGate/Validate v1.0 Release Track")]
        public static void ValidateV10FromMenu()
        {
            ValidateFromMenu();
        }

        public static void ValidateFromCommandLine()
        {
            bool valid = ValidateProject(out List<string> errors);
            if (valid)
            {
                Debug.Log("RuneGate command-line validation passed.");
                Debug.Log($"RuneGate save path: {SaveManager.SavePath}");
                EditorApplication.Exit(0);
                return;
            }

            Debug.LogError("RuneGate command-line validation failed:\n" + string.Join("\n", errors));
            EditorApplication.Exit(1);
        }

        public static bool ValidateProject(out List<string> errors)
        {
            errors = new List<string>();

            for (int i = 0; i < RequiredFolders.Length; i++)
            {
                if (!Directory.Exists(ToProjectPath(RequiredFolders[i])))
                {
                    errors.Add($"Missing folder: {RequiredFolders[i]}");
                }
            }

            for (int i = 0; i < RequiredScripts.Length; i++)
            {
                if (!File.Exists(ToProjectPath(RequiredScripts[i])))
                {
                    errors.Add($"Missing script: {RequiredScripts[i]}");
                }
            }

            for (int i = 0; i < RequiredScenes.Length; i++)
            {
                if (!File.Exists(ToProjectPath(RequiredScenes[i])))
                {
                    errors.Add($"Missing scene: {RequiredScenes[i]}");
                }
            }

            for (int i = 0; i < RequiredStageAssets.Length; i++)
            {
                ValidateAsset<StageData>(RequiredStageAssets[i], "v0.4 stage asset", errors);
            }

            for (int i = 0; i < RequiredHeroAssets.Length; i++)
            {
                ValidateAsset<HeroData>(RequiredHeroAssets[i], "v0.4 hero asset", errors);
            }

            for (int i = 0; i < RequiredMonsterAssets.Length; i++)
            {
                ValidateAsset<MonsterData>(RequiredMonsterAssets[i], "v0.4 monster asset", errors);
            }

            for (int i = 0; i < RequiredSkillAssets.Length; i++)
            {
                ValidateAsset<SkillData>(RequiredSkillAssets[i], "v0.4 skill asset", errors);
            }

            for (int i = 0; i < RequiredRuneAssets.Length; i++)
            {
                ValidateAsset<RuneData>(RequiredRuneAssets[i], "v0.4 rune asset", errors);
            }

            for (int i = 0; i < RequiredFormationAssets.Length; i++)
            {
                if (!File.Exists(ToProjectPath(RequiredFormationAssets[i])))
                {
                    errors.Add($"Missing v0.4 formation asset: {RequiredFormationAssets[i]}");
                }
            }

            for (int i = 0; i < RequiredDocs.Length; i++)
            {
                if (!File.Exists(ToProjectPath(RequiredDocs[i])))
                {
                    errors.Add($"Missing v0.4 doc: {RequiredDocs[i]}");
                }
            }

            for (int i = 0; i < RequiredUpgradeAssets.Length; i++)
            {
                if (!File.Exists(ToProjectPath(RequiredUpgradeAssets[i])))
                {
                    errors.Add($"Missing sample upgrade asset: {RequiredUpgradeAssets[i]}");
                }
            }

            for (int i = 0; i < RequiredV05Assets.Length; i++)
            {
                if (!File.Exists(ToProjectPath(RequiredV05Assets[i])))
                {
                    errors.Add($"Missing v0.5 art prototype asset: {RequiredV05Assets[i]}");
                }
            }

            for (int i = 0; i < RequiredCombatVisualPolishAssets.Length; i++)
            {
                ValidateAsset<Sprite>(RequiredCombatVisualPolishAssets[i], "v0.6 combat visual polish sprite", errors);
            }

            ValidateAsset<RuntimePixelVisualCatalog>(RequiredCombatVisualCatalogAsset, "v0.6 combat visual catalog", errors);
            ValidateAsset<KoreanFontCatalog>(RequiredKoreanFontCatalogAsset, "v0.8 Korean font catalog", errors);

            for (int i = 0; i < RequiredKoreanFontAssets.Length; i++)
            {
                ValidateAsset<Font>(RequiredKoreanFontAssets[i], "v0.8 Korean font", errors);
            }

            ValidateV05VisualLinks(errors);
            ValidateRuntimeArtPolicy(errors);

            SaveData defaultSave = SaveManager.CreateDefaultSave();
            if (defaultSave == null || defaultSave.unlockedStageIds == null || !defaultSave.unlockedStageIds.Contains(SaveManager.DefaultUnlockedStageId))
            {
                errors.Add("SaveManager default save does not unlock the first stage.");
            }

            if (defaultSave == null || defaultSave.formationSlots == null || defaultSave.formationSlots.Count == 0)
            {
                errors.Add("SaveManager default save does not include default formation slots.");
            }

            if (defaultSave == null || defaultSave.saveVersion <= 0)
            {
                errors.Add("SaveManager default save does not include a valid saveVersion.");
            }

            if (defaultSave != null && defaultSave.hasSeenTutorial)
            {
                errors.Add("SaveManager default save should not mark tutorial as seen.");
            }

            ValidateAndroidSettings(errors);

            if (!File.Exists(ToProjectPath(".gitignore")))
            {
                errors.Add("Missing .gitignore.");
            }

            if (!File.Exists(ToProjectPath(".gitattributes")))
            {
                errors.Add("Missing .gitattributes for Git LFS tracking.");
            }

            return errors.Count == 0;
        }

        private static string ToProjectPath(string relativePath)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.Combine(projectRoot, relativePath);
        }

        private static void ValidateAsset<T>(string assetPath, string label, List<string> errors) where T : UnityEngine.Object
        {
            if (!File.Exists(ToProjectPath(assetPath)))
            {
                errors.Add($"Missing {label}: {assetPath}");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<T>(assetPath) == null)
            {
                errors.Add($"Invalid {label}: {assetPath}");
            }
        }

        private static void ValidateV05VisualLinks(List<string> errors)
        {
            HeroData knight = AssetDatabase.LoadAssetAtPath<HeroData>("Assets/_Project/Data/Heroes/Knight.asset");
            if (knight == null)
            {
                return;
            }

            if (knight.Prefab == null)
            {
                errors.Add("Knight HeroData is missing the v0.5 hero prefab link. Run Tools/RuneGate/Bootstrap v0.5 Art Prototype.");
            }

            if (knight.AnimatorController == null)
            {
                errors.Add("Knight HeroData is missing the v0.5 animator controller link. Run Tools/RuneGate/Bootstrap v0.5 Art Prototype.");
            }

            MonsterData goblin = AssetDatabase.LoadAssetAtPath<MonsterData>("Assets/_Project/Data/Monsters/Goblin.asset");
            if (goblin == null)
            {
                return;
            }

            if (goblin.Prefab == null)
            {
                errors.Add("Goblin MonsterData is missing the v0.5 monster prefab link. Run Tools/RuneGate/Bootstrap v0.5 Art Prototype.");
            }

            if (goblin.AnimatorController == null)
            {
                errors.Add("Goblin MonsterData is missing the v0.5 animator controller link. Run Tools/RuneGate/Bootstrap v0.5 Art Prototype.");
            }
        }

        private static void ValidateRuntimeArtPolicy(List<string> errors)
        {
            ValidateHeroRuntimeSprite("Assets/_Project/Art/RuntimePixel/Heroes/Leon", "Assets/_Project/Data/Heroes/Knight.asset", "Leon", errors);
            ValidateHeroRuntimeSprite("Assets/_Project/Art/RuntimePixel/Heroes/Seria", "Assets/_Project/Data/Heroes/Archer.asset", "Seria", errors);
            ValidateHeroRuntimeSprite("Assets/_Project/Art/RuntimePixel/Heroes/Kael", "Assets/_Project/Data/Heroes/Fire Mage.asset", "Kael", errors);
            ValidateHeroRuntimeSprite("Assets/_Project/Art/RuntimePixel/Heroes/Mirea", "Assets/_Project/Data/Heroes/Priest.asset", "Mirea", errors);
            ValidateHeroRuntimeSprite("Assets/_Project/Art/RuntimePixel/Heroes/Brom", "Assets/_Project/Data/Heroes/Dwarf Engineer.asset", "Brom", errors);
            ValidateHeroRuntimeSprite("Assets/_Project/Art/RuntimePixel/Heroes/Nyx", "Assets/_Project/Data/Heroes/Shadow Assassin.asset", "Nyx", errors);
            ValidateMonsterRuntimeSprite("Assets/_Project/Art/RuntimePixel/Monsters/GateImp", "Assets/_Project/Data/Monsters/Goblin.asset", "GateImp", errors);
            ValidateMonsterRuntimeSprite("Assets/_Project/Art/RuntimePixel/Monsters/OrcBrute", "Assets/_Project/Data/Monsters/Orc.asset", "OrcBrute", errors);
            ValidateMonsterRuntimeSprite("Assets/_Project/Art/RuntimePixel/Monsters/DireWolf", "Assets/_Project/Data/Monsters/Wolf.asset", "DireWolf", errors);
            ValidateMonsterRuntimeSprite("Assets/_Project/Art/RuntimePixel/Monsters/CaveBat", "Assets/_Project/Data/Monsters/Bat.asset", "CaveBat", errors);
            ValidateMonsterRuntimeSprite("Assets/_Project/Art/RuntimePixel/Monsters/CoreSlime", "Assets/_Project/Data/Monsters/Slime.asset", "CoreSlime", errors);
            ValidateMonsterRuntimeSprite("Assets/_Project/Art/RuntimePixel/Monsters/BoneSoldier", "Assets/_Project/Data/Monsters/Skeleton.asset", "BoneSoldier", errors);
            ValidateMonsterRuntimeSprite("Assets/_Project/Art/RuntimePixel/Bosses/Grumbar", "Assets/_Project/Data/Monsters/Orc Warlord.asset", "Grumbar", errors);
            ValidateSkillIconLink("Assets/_Project/Art/UI/Icons/Skills", "Assets/_Project/Data/Skills/Shield Bash.asset", "Shield Bash", errors);
            ValidateSkillIconLink("Assets/_Project/Art/UI/Icons/Skills", "Assets/_Project/Data/Skills/Rapid Shot.asset", "Rapid Shot", errors);
            ValidateRuneIconLink("Assets/_Project/Art/UI/Icons/Runes", "Assets/_Project/Data/Runes/Sword Rune.asset", "Sword Rune", errors);
        }

        private static void ValidateHeroRuntimeSprite(string runtimeFolder, string dataPath, string label, List<string> errors)
        {
            HeroData data = AssetDatabase.LoadAssetAtPath<HeroData>(dataPath);
            if (data == null)
            {
                return;
            }

            ValidateNoConceptSheetSprite(data.BattleSprite, $"{label} HeroData battleSprite", errors);
            if (!HasTextureInFolder(runtimeFolder))
            {
                return;
            }

            if (data.BattleSprite == null)
            {
                errors.Add($"{label} RuntimePixel image exists but HeroData battleSprite is missing: {dataPath}");
                return;
            }

            ValidateRuntimePixelSprite(data.BattleSprite, $"{label} HeroData battleSprite", errors);
        }

        private static void ValidateMonsterRuntimeSprite(string runtimeFolder, string dataPath, string label, List<string> errors)
        {
            MonsterData data = AssetDatabase.LoadAssetAtPath<MonsterData>(dataPath);
            if (data == null)
            {
                return;
            }

            ValidateNoConceptSheetSprite(data.Sprite, $"{label} MonsterData runtimeSprite", errors);
            if (!HasTextureInFolder(runtimeFolder))
            {
                return;
            }

            if (data.Sprite == null)
            {
                errors.Add($"{label} RuntimePixel image exists but MonsterData runtimeSprite is missing: {dataPath}");
                return;
            }

            ValidateRuntimePixelSprite(data.Sprite, $"{label} MonsterData runtimeSprite", errors);
        }

        private static void ValidateNoConceptSheetSprite(Sprite sprite, string label, List<string> errors)
        {
            string path = sprite != null ? AssetDatabase.GetAssetPath(sprite) : string.Empty;
            if (!string.IsNullOrWhiteSpace(path) && path.StartsWith("Assets/_Project/Art/ConceptSheets", System.StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"{label} must not reference ConceptSheets art: {path}");
            }
        }

        private static void ValidateRuntimePixelSprite(Sprite sprite, string label, List<string> errors)
        {
            string path = sprite != null ? AssetDatabase.GetAssetPath(sprite) : string.Empty;
            if (!string.IsNullOrWhiteSpace(path) && !path.StartsWith("Assets/_Project/Art/RuntimePixel", System.StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"{label} must reference RuntimePixel art, not {path}");
            }
        }

        private static void ValidateSkillIconLink(string artFolder, string dataPath, string label, List<string> errors)
        {
            if (!HasTextureInFolder(artFolder))
            {
                return;
            }

            SkillData data = AssetDatabase.LoadAssetAtPath<SkillData>(dataPath);
            if (data == null || data.Icon == null)
            {
                errors.Add($"{label} image exists but SkillData icon link is missing: {dataPath}");
            }
        }

        private static void ValidateRuneIconLink(string artFolder, string dataPath, string label, List<string> errors)
        {
            if (!HasTextureInFolder(artFolder))
            {
                return;
            }

            RuneData data = AssetDatabase.LoadAssetAtPath<RuneData>(dataPath);
            if (data == null || data.Icon == null)
            {
                errors.Add($"{label} image exists but RuneData icon link is missing: {dataPath}");
            }
        }

        private static bool HasTextureInFolder(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                return false;
            }

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
            return guids != null && guids.Length > 0;
        }

        private static void ValidateAndroidSettings(List<string> errors)
        {
            string packageName = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
            if (packageName != "com.hobeomcheon.runegatedefense")
            {
                errors.Add("Android package name is not com.hobeomcheon.runegatedefense. Run Tools/RuneGate/Bootstrap v1.0 Release Track.");
            }

            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.Portrait)
            {
                errors.Add("Default orientation is not Portrait. Run Tools/RuneGate/Bootstrap v1.0 Release Track.");
            }
        }
    }
}
