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
            "Assets/_Project/Data/Heroes",
            "Assets/_Project/Data/Monsters",
            "Assets/_Project/Data/Skills",
            "Assets/_Project/Data/Runes",
            "Assets/_Project/Data/Stages",
            "Assets/_Project/Data/Upgrades",
            "Assets/_Project/Prefabs/Heroes",
            "Assets/_Project/Prefabs/Monsters",
            "Assets/_Project/Prefabs/UI",
            "Assets/_Project/Scenes",
            "Assets/_Project/Art/Characters",
            "Assets/_Project/Art/Monsters",
            "Assets/_Project/Art/Effects",
            "Assets/_Project/Art/UI",
            "Assets/_Project/Audio",
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
            "Assets/_Project/Scripts/Hero/HeroController.cs",
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
            "Assets/_Project/Scripts/UI/StageResultUI.cs",
            "Assets/_Project/Scripts/UI/TitleUI.cs",
            "Assets/_Project/Scripts/UI/StageSelectUI.cs",
            "Assets/_Project/Scripts/UI/UpgradeSceneUI.cs",
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
            "Assets/_Project/Data/Stages/Goblin Forest 3.asset"
        };

        private static readonly string[] RequiredUpgradeAssets =
        {
            "Assets/_Project/Data/Upgrades/Crystal Reinforcement.asset",
            "Assets/_Project/Data/Upgrades/Hero Training.asset",
            "Assets/_Project/Data/Upgrades/Battle Rhythm.asset",
            "Assets/_Project/Data/Upgrades/Skill Practice.asset"
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
                if (!File.Exists(ToProjectPath(RequiredStageAssets[i])))
                {
                    errors.Add($"Missing sample stage asset: {RequiredStageAssets[i]}");
                }
            }

            for (int i = 0; i < RequiredUpgradeAssets.Length; i++)
            {
                if (!File.Exists(ToProjectPath(RequiredUpgradeAssets[i])))
                {
                    errors.Add($"Missing sample upgrade asset: {RequiredUpgradeAssets[i]}");
                }
            }

            SaveData defaultSave = SaveManager.CreateDefaultSave();
            if (defaultSave == null || defaultSave.unlockedStageIds == null || !defaultSave.unlockedStageIds.Contains(SaveManager.DefaultUnlockedStageId))
            {
                errors.Add("SaveManager default save does not unlock the first stage.");
            }

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
    }
}
