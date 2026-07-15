using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
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
            "Assets/_Project/Scripts/UI/Foundation",
            "Assets/_Project/Editor",
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
            "Assets/_Project/Scripts/Core/DifficultyRules.cs",
            "Assets/_Project/Scripts/Core/RuntimeContentCatalog.cs",
            "Assets/_Project/Scripts/Hero/HeroCombatState.cs",
            "Assets/_Project/Scripts/Monster/MonsterCombatState.cs",
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
            "Assets/_Project/Scripts/Battle/CombatFeedbackEvents.cs",
            "Assets/_Project/Scripts/Battle/RuntimePixelAssetLoader.cs",
            "Assets/_Project/Scripts/Battle/RuntimePixelVisualCatalog.cs",
            "Assets/_Project/Scripts/Battle/RuntimeSpriteFitter.cs",
            "Assets/_Project/Scripts/Battle/RuntimeSpritePolicy.cs",
            "Assets/_Project/Scripts/Battle/RuntimeSpriteBoundsUtility.cs",
            "Assets/_Project/Scripts/Battle/BattlefieldCameraFitter.cs",
            "Assets/_Project/Scripts/Battle/UnitMovementController.cs",
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
            "Assets/_Project/Scripts/UI/BattlePauseController.cs",
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
            "Assets/_Project/Scripts/UI/UIResponsiveLayout.cs",
            "Assets/_Project/Scripts/UI/Foundation/GameFrameLayout.cs",
            "Assets/_Project/Scripts/UI/Foundation/FrameRootLimiter.cs",
            "Assets/_Project/Scripts/UI/Foundation/ResponsiveLayoutSwitcher.cs",
            "Assets/_Project/Scripts/UI/Foundation/PopupFrameController.cs",
            "Assets/_Project/Scripts/UI/UIPopupGuiUtility.cs",
            "Assets/_Project/Scripts/UI/PanelClampToScreen.cs",
            "Assets/_Project/Scripts/UI/KoreanFontCatalog.cs",
            "Assets/_Project/Scripts/UI/KoreanFontManager.cs",
            "Assets/_Project/Scripts/UI/SafeAreaFitter.cs",
            "Assets/_Project/Scripts/Audio/AudioManager.cs",
            "Assets/_Project/Scripts/Audio/SfxKey.cs",
            "Assets/_Project/Editor/UIFrameValidator.cs",
            "Assets/_Project/Scripts/Editor/RuneGateBootstrapper.cs",
            "Assets/_Project/Scripts/Editor/RuneGateCurrentBuildPipeline.cs",
            "Assets/_Project/Scripts/Editor/RuneGateProgressionSmokeTest.cs",
            "Assets/_Project/Scripts/Diagnostics/RuneGateRuntimeSmokeRunner.cs",
            "Assets/_Project/Scripts/Diagnostics/RuneGatePngEncoder.cs",
            "Assets/_Project/Scripts/Diagnostics/RuneGateVisualCaptureRunner.cs"
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
        private const string RequiredRuntimeContentCatalogAsset = "Assets/_Project/Resources/RuntimeContentCatalog.asset";

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
            "docs/stage-select-layout-hotfix-v088.md",
            "docs/progression-flow-stability-v088.md",
            "docs/battle-ui-stability-v088.md",
            "docs/local-save-stability-v088.md",
            "docs/runtime-e2e-smoke-test.md",
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

            ValidateMetaGuidIntegrity(errors);

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
            ValidateAsset<RuntimeContentCatalog>(RequiredRuntimeContentCatalogAsset, "runtime content catalog", errors);
            ValidateRuntimeContentCatalog(errors);
            ValidateProgressionFlowData(errors);
            ValidateSceneFlowComponents(errors);
            ValidateUserFacingTextPolicy(errors);
            ValidateGameFrameLayouts(errors);
            ValidateDifficultyRules(errors);

            for (int i = 0; i < RequiredKoreanFontAssets.Length; i++)
            {
                ValidateAsset<Font>(RequiredKoreanFontAssets[i], "v0.8 Korean font", errors);
            }

            ValidateV05VisualLinks(errors);
            ValidateRuntimeArtPolicy(errors);
            ValidateRuntimePixelImportSettings(errors);

            SaveData defaultSave = SaveManager.CreateDefaultSave();
            if (defaultSave == null || defaultSave.unlockedStageIds == null || !defaultSave.unlockedStageIds.Contains(SaveManager.DefaultUnlockedStageId))
            {
                errors.Add("SaveManager default save does not unlock the first stage.");
            }

            if (defaultSave == null || defaultSave.formationSlots == null || defaultSave.formationSlots.Count == 0)
            {
                errors.Add("SaveManager default save does not include default formation slots.");
            }
            else
            {
                RuntimeContentCatalog runtimeCatalog = AssetDatabase.LoadAssetAtPath<RuntimeContentCatalog>(RequiredRuntimeContentCatalogAsset);
                if (runtimeCatalog != null && runtimeCatalog.DefaultFormation != null && runtimeCatalog.DefaultFormation.Slots != null && runtimeCatalog.DefaultFormation.Slots.Count > 0)
                {
                    if (!FormationSlotsMatch(defaultSave.formationSlots, runtimeCatalog.DefaultFormation.Slots))
                    {
                        errors.Add("SaveManager default save formation does not match RuntimeContentCatalog DefaultFormation.");
                    }
                }
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

        private static void ValidateMetaGuidIntegrity(List<string> errors)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            foreach (string metaPath in Directory.EnumerateFiles(Application.dataPath, "*.meta", SearchOption.AllDirectories))
            {
                string guid = ReadMetaGuid(metaPath);
                if (string.IsNullOrWhiteSpace(guid))
                {
                    errors.Add($"Missing Unity meta guid: {ToProjectRelativePath(projectRoot, metaPath)}");
                    continue;
                }

                if (!IsValidUnityGuid(guid))
                {
                    errors.Add($"Invalid Unity meta guid: {ToProjectRelativePath(projectRoot, metaPath)} ({guid})");
                }
            }
        }

        private static string ReadMetaGuid(string metaPath)
        {
            string[] lines = File.ReadAllLines(metaPath);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("guid:", StringComparison.Ordinal))
                {
                    return line.Substring("guid:".Length).Trim();
                }
            }

            return string.Empty;
        }

        private static bool IsValidUnityGuid(string guid)
        {
            if (guid.Length != 32)
            {
                return false;
            }

            for (int i = 0; i < guid.Length; i++)
            {
                char c = guid[i];
                bool hex = c >= '0' && c <= '9' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F';
                if (!hex)
                {
                    return false;
                }
            }

            return true;
        }

        private static string ToProjectRelativePath(string projectRoot, string fullPath)
        {
            return Path.GetRelativePath(projectRoot, fullPath).Replace(Path.DirectorySeparatorChar, '/');
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

        private static void ValidateRuntimeContentCatalog(List<string> errors)
        {
            RuntimeContentCatalog catalog = AssetDatabase.LoadAssetAtPath<RuntimeContentCatalog>(RequiredRuntimeContentCatalogAsset);
            if (catalog == null)
            {
                return;
            }

            int stageCount = CountNonNull(catalog.Stages);
            if (stageCount < RequiredStageAssets.Length)
            {
                errors.Add($"RuntimeContentCatalog must reference {RequiredStageAssets.Length} stages. Found {stageCount}.");
            }

            int expectedStageNumber = 1;
            for (int i = 0; i < catalog.Stages.Count; i++)
            {
                StageData stage = catalog.Stages[i];
                if (stage == null)
                {
                    continue;
                }

                int stageNumber = PrototypeAssetLoader.GetStageNumber(stage);
                if (stageNumber != expectedStageNumber)
                {
                    errors.Add($"RuntimeContentCatalog stage order is invalid at index {i}. Expected stage {expectedStageNumber}, found {stageNumber}.");
                    break;
                }

                expectedStageNumber++;
            }

            int runeCount = CountNonNull(catalog.Runes);
            if (runeCount < RequiredRuneAssets.Length)
            {
                errors.Add($"RuntimeContentCatalog must reference {RequiredRuneAssets.Length} runes. Found {runeCount}.");
            }

            int upgradeCount = CountNonNull(catalog.Upgrades);
            if (upgradeCount < RequiredUpgradeAssets.Length)
            {
                errors.Add($"RuntimeContentCatalog must reference {RequiredUpgradeAssets.Length} upgrades. Found {upgradeCount}.");
            }

            if (catalog.HeroRoster == null)
            {
                errors.Add("RuntimeContentCatalog is missing HeroRoster.");
            }

            if (catalog.DefaultFormation == null)
            {
                errors.Add("RuntimeContentCatalog is missing DefaultFormation.");
            }
        }

        private static void ValidateProgressionFlowData(List<string> errors)
        {
            RuntimeContentCatalog catalog = AssetDatabase.LoadAssetAtPath<RuntimeContentCatalog>(RequiredRuntimeContentCatalogAsset);
            if (catalog == null)
            {
                return;
            }

            ValidateStageProgression(catalog, errors);
            ValidateDefaultFormation(catalog, errors);
            ValidateProgressionUpgrades(catalog, errors);
            ValidateStageSessionResolution(catalog, errors);
            ValidateBattleResultProgression(catalog, errors);
            ValidateFullStageUnlockProgression(catalog, errors);
            ValidateUpgradePurchaseAfterStageOne(catalog, errors);
        }

        private static void ValidateStageProgression(RuntimeContentCatalog catalog, List<string> errors)
        {
            if (catalog.Stages == null || catalog.Stages.Count < RequiredStageAssets.Length)
            {
                return;
            }

            HashSet<string> stageIds = new HashSet<string>();
            bool stageTenHasBoss = false;
            bool stageTenHasBossSpawn = false;
            for (int i = 0; i < catalog.Stages.Count; i++)
            {
                StageData stage = catalog.Stages[i];
                if (stage == null)
                {
                    errors.Add($"RuntimeContentCatalog has a null stage at index {i}.");
                    continue;
                }

                string stageLabel = $"{stage.name} ({stage.StageId})";
                if (string.IsNullOrWhiteSpace(stage.StageId))
                {
                    errors.Add($"{stage.name} is missing StageId.");
                }
                else if (!stageIds.Add(stage.StageId))
                {
                    errors.Add($"Duplicate StageId in RuntimeContentCatalog: {stage.StageId}");
                }

                if (string.IsNullOrWhiteSpace(stage.DisplayNameKorean) || stage.DisplayNameKorean.Contains("??"))
                {
                    errors.Add($"{stageLabel} is missing a valid Korean display name.");
                }

                if (stage.CrystalHp <= 0)
                {
                    errors.Add($"{stageLabel} must have positive CrystalHp.");
                }

                if (stage.Waves == null || stage.Waves.Count == 0)
                {
                    errors.Add($"{stageLabel} has no waves.");
                    continue;
                }

                for (int waveIndex = 0; waveIndex < stage.Waves.Count; waveIndex++)
                {
                    WaveData wave = stage.Waves[waveIndex];
                    ValidateWave(stageLabel, wave, waveIndex, errors);
                    if (PrototypeAssetLoader.GetStageNumber(stage) == 10 && wave != null && wave.IsBossWave)
                    {
                        stageTenHasBoss = true;
                    }

                    if (PrototypeAssetLoader.GetStageNumber(stage) == 10 && WaveHasBossSpawn(wave))
                    {
                        stageTenHasBossSpawn = true;
                    }
                }

                if (PrototypeAssetLoader.GetStageNumber(stage) == 10 && stage.BossMonster == null)
                {
                    errors.Add($"{stageLabel} must reference the Stage 10 boss monster.");
                }
            }

            if (!stageIds.Contains(SaveManager.DefaultUnlockedStageId))
            {
                errors.Add($"RuntimeContentCatalog does not include the default unlocked stage id: {SaveManager.DefaultUnlockedStageId}");
            }

            if (!stageTenHasBoss)
            {
                errors.Add("Stage 10 must include at least one boss wave.");
            }

            if (!stageTenHasBossSpawn)
            {
                errors.Add("Stage 10 must include at least one actual boss monster spawn.");
            }
        }

        private static void ValidateWave(string stageLabel, WaveData wave, int waveIndex, List<string> errors)
        {
            if (wave == null)
            {
                errors.Add($"{stageLabel} has a null wave at index {waveIndex}.");
                return;
            }

            if (wave.Spawns == null || wave.Spawns.Count == 0)
            {
                errors.Add($"{stageLabel} wave {waveIndex + 1} has no monster spawns.");
                return;
            }

            for (int spawnIndex = 0; spawnIndex < wave.Spawns.Count; spawnIndex++)
            {
                WaveSpawnData spawn = wave.Spawns[spawnIndex];
                if (spawn == null)
                {
                    errors.Add($"{stageLabel} wave {waveIndex + 1} has a null spawn at index {spawnIndex}.");
                    continue;
                }

                if (spawn.MonsterData == null)
                {
                    errors.Add($"{stageLabel} wave {waveIndex + 1} spawn {spawnIndex + 1} has no MonsterData.");
                }

                if (spawn.LaneIndex < 0 || spawn.LaneIndex > 2)
                {
                    errors.Add($"{stageLabel} wave {waveIndex + 1} spawn {spawnIndex + 1} has invalid lane index {spawn.LaneIndex}.");
                }

                if (spawn.Count <= 0)
                {
                    errors.Add($"{stageLabel} wave {waveIndex + 1} spawn {spawnIndex + 1} must spawn at least one monster.");
                }

                if (spawn.SpawnInterval < 0f)
                {
                    errors.Add($"{stageLabel} wave {waveIndex + 1} spawn {spawnIndex + 1} has a negative spawn interval.");
                }
            }
        }

        private static bool WaveHasBossSpawn(WaveData wave)
        {
            if (wave == null || wave.Spawns == null)
            {
                return false;
            }

            for (int i = 0; i < wave.Spawns.Count; i++)
            {
                WaveSpawnData spawn = wave.Spawns[i];
                if (spawn != null && spawn.MonsterData != null && spawn.MonsterData.IsBoss && spawn.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateDefaultFormation(RuntimeContentCatalog catalog, List<string> errors)
        {
            if (catalog.HeroRoster == null || catalog.DefaultFormation == null)
            {
                return;
            }

            if (catalog.HeroRoster.Heroes == null || CountNonNull(catalog.HeroRoster.Heroes) < RequiredHeroAssets.Length)
            {
                errors.Add($"HeroRoster must reference {RequiredHeroAssets.Length} heroes.");
                return;
            }

            if (catalog.DefaultFormation.Slots == null || catalog.DefaultFormation.Slots.Count == 0)
            {
                errors.Add("DefaultFormation has no slots.");
                return;
            }

            HashSet<string> occupiedSlots = new HashSet<string>();
            HashSet<string> heroIds = new HashSet<string>();
            for (int i = 0; i < catalog.DefaultFormation.Slots.Count; i++)
            {
                FormationSlot slot = catalog.DefaultFormation.Slots[i];
                if (slot == null)
                {
                    errors.Add($"DefaultFormation has a null slot at index {i}.");
                    continue;
                }

                if (slot.LaneIndex < 0 || slot.LaneIndex > 2)
                {
                    errors.Add($"DefaultFormation slot {i + 1} has invalid lane index {slot.LaneIndex}.");
                }

                string key = $"{slot.LaneIndex}:{slot.PositionType}";
                if (!occupiedSlots.Add(key))
                {
                    errors.Add($"DefaultFormation has duplicate slot placement: lane {slot.LaneIndex}, {slot.PositionType}.");
                }

                if (string.IsNullOrWhiteSpace(slot.HeroId))
                {
                    errors.Add($"DefaultFormation slot {i + 1} has an empty HeroId.");
                    continue;
                }

                if (!heroIds.Add(slot.HeroId))
                {
                    errors.Add($"DefaultFormation has duplicate hero id: {slot.HeroId}");
                }

                if (catalog.HeroRoster.FindHeroById(slot.HeroId) == null)
                {
                    errors.Add($"DefaultFormation references a hero missing from HeroRoster: {slot.HeroId}");
                }
            }

            if (heroIds.Count < RequiredHeroAssets.Length)
            {
                errors.Add($"DefaultFormation should include {RequiredHeroAssets.Length} unique MVP heroes. Found {heroIds.Count}.");
            }
        }

        private static bool FormationSlotsMatch(IReadOnlyList<FormationSlot> saveSlots, IReadOnlyList<FormationSlot> catalogSlots)
        {
            if (saveSlots == null || catalogSlots == null)
            {
                return false;
            }

            Dictionary<string, string> saveMap = BuildFormationSlotMap(saveSlots);
            Dictionary<string, string> catalogMap = BuildFormationSlotMap(catalogSlots);
            if (saveMap.Count != catalogMap.Count)
            {
                return false;
            }

            foreach (KeyValuePair<string, string> pair in catalogMap)
            {
                if (!saveMap.TryGetValue(pair.Key, out string saveHeroId) || saveHeroId != pair.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private static Dictionary<string, string> BuildFormationSlotMap(IReadOnlyList<FormationSlot> slots)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            if (slots == null)
            {
                return map;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                FormationSlot slot = slots[i];
                if (slot == null || string.IsNullOrWhiteSpace(slot.HeroId))
                {
                    continue;
                }

                map[$"{Mathf.Clamp(slot.LaneIndex, 0, 2)}:{slot.PositionType}"] = NormalizeHeroId(slot.HeroId);
            }

            return map;
        }

        private static string NormalizeHeroId(string heroId)
        {
            return heroId == "hero_cleric_001" ? "hero_priest_001" : heroId;
        }

        private static void ValidateProgressionUpgrades(RuntimeContentCatalog catalog, List<string> errors)
        {
            if (catalog.Upgrades == null)
            {
                return;
            }

            HashSet<string> upgradeIds = new HashSet<string>();
            for (int i = 0; i < catalog.Upgrades.Count; i++)
            {
                UpgradeData upgrade = catalog.Upgrades[i];
                if (upgrade == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(upgrade.UpgradeId))
                {
                    errors.Add($"{upgrade.name} is missing UpgradeId.");
                    continue;
                }

                if (!upgradeIds.Add(upgrade.UpgradeId))
                {
                    errors.Add($"Duplicate UpgradeId in RuntimeContentCatalog: {upgrade.UpgradeId}");
                }

                if (upgrade.MaxLevel <= 0)
                {
                    errors.Add($"{upgrade.name} must have positive MaxLevel.");
                }

                if (upgrade.BaseCost < 0)
                {
                    errors.Add($"{upgrade.name} must not have a negative BaseCost.");
                }
            }
        }

        private static void ValidateStageSessionResolution(RuntimeContentCatalog catalog, List<string> errors)
        {
            if (catalog.Stages == null || catalog.Stages.Count < RequiredStageAssets.Length)
            {
                return;
            }

            StageData firstStage = catalog.Stages[0];
            StageData secondStage = catalog.Stages[1];
            StageData finalStage = catalog.Stages[RequiredStageAssets.Length - 1];
            if (firstStage == null || secondStage == null || finalStage == null)
            {
                return;
            }

            string stageTwoId = GameSession.ResolveNextStageId(firstStage.StageId, catalog.Stages);
            if (stageTwoId != secondStage.StageId)
            {
                errors.Add($"GameSession next stage resolution is invalid for Stage 1. Expected {secondStage.StageId}, found {stageTwoId}.");
            }

            string afterFinalStageId = GameSession.ResolveNextStageId(finalStage.StageId, catalog.Stages);
            if (!string.IsNullOrWhiteSpace(afterFinalStageId))
            {
                errors.Add($"GameSession should not resolve a stage after the final configured stage. Found {afterFinalStageId}.");
            }
        }

        private static void ValidateBattleResultProgression(RuntimeContentCatalog catalog, List<string> errors)
        {
            if (catalog.Stages == null || catalog.Stages.Count < 2 || catalog.Stages[0] == null || catalog.Stages[1] == null)
            {
                return;
            }

            StageData firstStage = catalog.Stages[0];
            StageData secondStage = catalog.Stages[1];
            string battleRunId = "validator_stage_1_victory";
            SaveData saveData = SaveManager.CreateDefaultSave();
            int initialGold = saveData.totalGold;
            bool applied = SaveManager.TryApplyBattleResultProgression(saveData, battleRunId, 110, true, firstStage.StageId, secondStage.StageId);
            if (!applied)
            {
                errors.Add("SaveManager could not apply a Stage 1 victory to an in-memory save.");
                return;
            }

            if (!saveData.clearedStageIds.Contains(firstStage.StageId))
            {
                errors.Add("Stage 1 victory does not mark Stage 1 cleared in save data.");
            }

            if (!saveData.unlockedStageIds.Contains(secondStage.StageId))
            {
                errors.Add("Stage 1 victory does not unlock Stage 2 in save data.");
            }

            if (saveData.totalGold != initialGold + 110)
            {
                errors.Add($"Stage 1 victory gold is invalid. Expected {initialGold + 110}, found {saveData.totalGold}.");
            }

            bool duplicateApplied = SaveManager.TryApplyBattleResultProgression(saveData, battleRunId, 110, true, firstStage.StageId, secondStage.StageId);
            if (duplicateApplied || saveData.totalGold != initialGold + 110)
            {
                errors.Add("Duplicate BattleRunId applies battle progression more than once.");
            }
        }

        private static void ValidateFullStageUnlockProgression(RuntimeContentCatalog catalog, List<string> errors)
        {
            if (catalog.Stages == null || catalog.Stages.Count < RequiredStageAssets.Length)
            {
                return;
            }

            SaveData saveData = SaveManager.CreateDefaultSave();
            int expectedGold = saveData.totalGold;
            for (int i = 0; i < RequiredStageAssets.Length; i++)
            {
                StageData stage = catalog.Stages[i];
                if (stage == null)
                {
                    return;
                }

                if (!saveData.unlockedStageIds.Contains(stage.StageId))
                {
                    errors.Add($"Stage progression simulation cannot enter Stage {i + 1}. Missing unlock for {stage.StageId}.");
                    return;
                }

                string nextStageId = GameSession.ResolveNextStageId(stage.StageId, catalog.Stages);
                int goldAward = 100 + i;
                expectedGold += goldAward;
                string battleRunId = $"validator_full_progression_stage_{i + 1}";
                bool applied = SaveManager.TryApplyBattleResultProgression(saveData, battleRunId, goldAward, true, stage.StageId, nextStageId);
                if (!applied)
                {
                    errors.Add($"SaveManager could not apply simulated Stage {i + 1} victory.");
                    return;
                }

                if (!saveData.clearedStageIds.Contains(stage.StageId))
                {
                    errors.Add($"Simulated Stage {i + 1} victory does not clear {stage.StageId}.");
                }

                if (!saveData.unlockedStageIds.Contains(stage.StageId))
                {
                    errors.Add($"Simulated Stage {i + 1} victory does not keep {stage.StageId} unlocked.");
                }

                if (!string.IsNullOrWhiteSpace(nextStageId) && !saveData.unlockedStageIds.Contains(nextStageId))
                {
                    errors.Add($"Simulated Stage {i + 1} victory does not unlock next stage {nextStageId}.");
                }

                if (saveData.totalGold != expectedGold)
                {
                    errors.Add($"Simulated Stage {i + 1} victory gold is invalid. Expected {expectedGold}, found {saveData.totalGold}.");
                }
            }

            StageData finalStage = catalog.Stages[RequiredStageAssets.Length - 1];
            string afterFinalStageId = finalStage != null ? GameSession.ResolveNextStageId(finalStage.StageId, catalog.Stages) : string.Empty;
            if (!string.IsNullOrWhiteSpace(afterFinalStageId))
            {
                errors.Add($"Full stage progression should stop after the final configured stage. Found next stage {afterFinalStageId}.");
            }

            for (int i = 0; i < RequiredStageAssets.Length; i++)
            {
                StageData stage = catalog.Stages[i];
                if (stage == null)
                {
                    continue;
                }

                if (!saveData.clearedStageIds.Contains(stage.StageId))
                {
                    errors.Add($"Full progression simulation did not clear {stage.StageId}.");
                }

                if (!saveData.unlockedStageIds.Contains(stage.StageId))
                {
                    errors.Add($"Full progression simulation did not unlock {stage.StageId}.");
                }
            }
        }

        private static void ValidateUpgradePurchaseAfterStageOne(RuntimeContentCatalog catalog, List<string> errors)
        {
            if (catalog.Upgrades == null || catalog.Upgrades.Count == 0)
            {
                return;
            }

            UpgradeData firstUpgrade = null;
            for (int i = 0; i < catalog.Upgrades.Count; i++)
            {
                if (catalog.Upgrades[i] != null)
                {
                    firstUpgrade = catalog.Upgrades[i];
                    break;
                }
            }

            if (firstUpgrade == null)
            {
                return;
            }

            SaveData saveData = SaveManager.CreateDefaultSave();
            saveData.totalGold = 110;
            int cost = UpgradeManager.CalculateCost(firstUpgrade, 0);
            if (cost > 110)
            {
                errors.Add($"{firstUpgrade.name} first purchase cost {cost}, but Stage 1 minimum reward is 110.");
                return;
            }

            if (!SaveManager.TryPurchaseUpgrade(saveData, firstUpgrade.UpgradeId, cost, firstUpgrade.MaxLevel))
            {
                errors.Add($"{firstUpgrade.name} cannot be purchased after Stage 1 reward.");
                return;
            }

            if (SaveManager.GetUpgradeLevel(saveData, firstUpgrade.UpgradeId) != 1)
            {
                errors.Add($"{firstUpgrade.name} purchase does not increase level to 1.");
            }

            if (saveData.totalGold != 110 - cost)
            {
                errors.Add($"{firstUpgrade.name} purchase leaves invalid gold. Expected {110 - cost}, found {saveData.totalGold}.");
            }

            SaveData poorSave = SaveManager.CreateDefaultSave();
            poorSave.totalGold = Mathf.Max(0, cost - 1);
            if (SaveManager.TryPurchaseUpgrade(poorSave, firstUpgrade.UpgradeId, cost, firstUpgrade.MaxLevel))
            {
                errors.Add($"{firstUpgrade.name} can be purchased without enough gold.");
            }
        }

        private static void ValidateSceneFlowComponents(List<string> errors)
        {
            ValidateSceneScriptCount("Assets/_Project/Scenes/TitleScene.unity", "Assets/_Project/Scripts/UI/TitleUI.cs", "TitleUI", 1, errors);
            ValidateSceneScriptCount("Assets/_Project/Scenes/StageSelectScene.unity", "Assets/_Project/Scripts/UI/StageSelectUI.cs", "StageSelectUI", 1, errors);
            ValidateSceneScriptCount("Assets/_Project/Scenes/BattleScene.unity", "Assets/_Project/Scripts/Battle/BattleManager.cs", "BattleManager", 1, errors);
            ValidateSceneScriptCount("Assets/_Project/Scenes/BattleScene.unity", "Assets/_Project/Scripts/UI/StageResultUI.cs", "StageResultUI", 1, errors);
            ValidateSceneScriptCount("Assets/_Project/Scenes/UpgradeScene.unity", "Assets/_Project/Scripts/UI/UpgradeSceneUI.cs", "UpgradeSceneUI", 1, errors);
        }

        private static void ValidateUserFacingTextPolicy(List<string> errors)
        {
            string[] uiFiles =
            {
                "Assets/_Project/Scripts/UI/BattleHUD.cs",
                "Assets/_Project/Scripts/UI/FormationSkillPanelUI.cs",
                "Assets/_Project/Scripts/UI/RuneSelectionUI.cs",
                "Assets/_Project/Scripts/UI/StageResultUI.cs",
                "Assets/_Project/Scripts/UI/StageSelectUI.cs",
                "Assets/_Project/Scripts/UI/TitleUI.cs",
                "Assets/_Project/Scripts/UI/TutorialManager.cs",
                "Assets/_Project/Scripts/UI/TutorialOverlayUI.cs",
                "Assets/_Project/Scripts/UI/UpgradeSceneUI.cs"
            };

            string[] forbiddenSnippets =
            {
                "Kingdom Crystal defended",
                "Missing StageData",
                "State WaveRunning",
                "State RuneSelection",
                " ATK ",
                " SPD ",
                " Lv ",
                "\ufffd",
                "\uf9cf",
                "\u8adb",
                "\u6028\u2464"
            };

            for (int fileIndex = 0; fileIndex < uiFiles.Length; fileIndex++)
            {
                string filePath = uiFiles[fileIndex];
                string absolutePath = ToProjectPath(filePath);
                if (!File.Exists(absolutePath))
                {
                    continue;
                }

                string text = File.ReadAllText(absolutePath);
                for (int snippetIndex = 0; snippetIndex < forbiddenSnippets.Length; snippetIndex++)
                {
                    string forbidden = forbiddenSnippets[snippetIndex];
                    if (text.IndexOf(forbidden, StringComparison.Ordinal) >= 0)
                    {
                        errors.Add($"{filePath} contains user-facing debug/internal text: {forbidden}");
                    }
                }
            }
        }

        private static void ValidateGameFrameLayouts(List<string> errors)
        {
            if (!UIFrameValidator.ValidateLayoutMathForProjectValidator(out List<string> layoutFailures))
            {
                for (int i = 0; i < layoutFailures.Count; i++)
                {
                    errors.Add("UI frame validation failed: " + layoutFailures[i]);
                }
            }
        }

        private static void ValidateDifficultyRules(List<string> errors)
        {
            int normalHp = DifficultyRules.ApplyMonsterHp(100, DifficultyRules.Normal);
            int easyHp = DifficultyRules.ApplyMonsterHp(100, DifficultyRules.Easy);
            int hardHp = DifficultyRules.ApplyMonsterHp(100, DifficultyRules.Hard);
            int nightmareHp = DifficultyRules.ApplyMonsterHp(100, DifficultyRules.Nightmare);
            if (!(easyHp < normalHp && normalHp < hardHp && hardHp < nightmareHp))
            {
                errors.Add($"DifficultyRules monster HP scaling is invalid. easy={easyHp}, normal={normalHp}, hard={hardHp}, nightmare={nightmareHp}.");
            }

            int normalDamage = DifficultyRules.ApplyMonsterCrystalDamage(10, DifficultyRules.Normal);
            int easyDamage = DifficultyRules.ApplyMonsterCrystalDamage(10, DifficultyRules.Easy);
            int hardDamage = DifficultyRules.ApplyMonsterCrystalDamage(10, DifficultyRules.Hard);
            int nightmareDamage = DifficultyRules.ApplyMonsterCrystalDamage(10, DifficultyRules.Nightmare);
            if (!(easyDamage < normalDamage && normalDamage < hardDamage && hardDamage < nightmareDamage))
            {
                errors.Add($"DifficultyRules damage scaling is invalid. easy={easyDamage}, normal={normalDamage}, hard={hardDamage}, nightmare={nightmareDamage}.");
            }

            int easyCrystal = DifficultyRules.ApplyCrystalMaxHp(100, DifficultyRules.Easy);
            int normalCrystal = DifficultyRules.ApplyCrystalMaxHp(100, DifficultyRules.Normal);
            int nightmareCrystal = DifficultyRules.ApplyCrystalMaxHp(100, DifficultyRules.Nightmare);
            if (!(easyCrystal > normalCrystal && normalCrystal > nightmareCrystal))
            {
                errors.Add($"DifficultyRules crystal HP scaling is invalid. easy={easyCrystal}, normal={normalCrystal}, nightmare={nightmareCrystal}.");
            }

            int normalReward = DifficultyRules.ApplyMonsterRewardGold(100, DifficultyRules.Normal);
            int hardReward = DifficultyRules.ApplyMonsterRewardGold(100, DifficultyRules.Hard);
            int nightmareReward = DifficultyRules.ApplyMonsterRewardGold(100, DifficultyRules.Nightmare);
            if (!(normalReward < hardReward && hardReward < nightmareReward))
            {
                errors.Add($"DifficultyRules reward scaling is invalid. normal={normalReward}, hard={hardReward}, nightmare={nightmareReward}.");
            }
            if (DifficultyRules.UndeadRevives(DifficultyRules.Easy) ||
                DifficultyRules.UndeadRevives(DifficultyRules.Normal) ||
                !DifficultyRules.UndeadRevives(DifficultyRules.Hard) ||
                !DifficultyRules.UndeadRevives(DifficultyRules.Nightmare))
            {
                errors.Add("Undead revival must be disabled on Easy/Normal and enabled on Hard/Nightmare.");
            }
        }

        private static void ValidateSceneScriptCount(string scenePath, string scriptPath, string label, int expectedCount, List<string> errors)
        {
            string absoluteScenePath = ToProjectPath(scenePath);
            if (!File.Exists(absoluteScenePath))
            {
                return;
            }

            string guid = AssetDatabase.AssetPathToGUID(scriptPath);
            if (string.IsNullOrWhiteSpace(guid))
            {
                errors.Add($"Cannot find script guid for {label}: {scriptPath}");
                return;
            }

            string sceneText = File.ReadAllText(absoluteScenePath);
            int count = CountOccurrences(sceneText, guid);
            if (count != expectedCount)
            {
                errors.Add($"{scenePath} must contain exactly {expectedCount} {label} component(s). Found {count}.");
            }
        }

        private static int CountOccurrences(string text, string value)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(value))
            {
                return 0;
            }

            int count = 0;
            int index = 0;
            while (index < text.Length)
            {
                int found = text.IndexOf(value, index, StringComparison.OrdinalIgnoreCase);
                if (found < 0)
                {
                    break;
                }

                count++;
                index = found + value.Length;
            }

            return count;
        }

        private static int CountNonNull<T>(IReadOnlyList<T> assets) where T : UnityEngine.Object
        {
            if (assets == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < assets.Count; i++)
            {
                if (assets[i] != null)
                {
                    count++;
                }
            }

            return count;
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

        private static void ValidateRuntimePixelImportSettings(List<string> errors)
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_Project/Art/RuntimePixel" });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                if (importer.textureType != TextureImporterType.Sprite)
                {
                    errors.Add($"RuntimePixel texture must import as Sprite: {path}");
                }

                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    Debug.LogWarning($"RuntimePixel texture should use Sprite Mode Single: {path}");
                }

                if (importer.filterMode != FilterMode.Point)
                {
                    Debug.LogWarning($"RuntimePixel texture should use Point filter for pixel art: {path}");
                }

                if (importer.mipmapEnabled)
                {
                    Debug.LogWarning($"RuntimePixel texture should disable mip maps: {path}");
                }

                if (importer.textureCompression != TextureImporterCompression.Uncompressed && importer.textureCompression != TextureImporterCompression.CompressedLQ)
                {
                    Debug.LogWarning($"RuntimePixel texture should use None or Low compression: {path}");
                }
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
            string packageName = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
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
