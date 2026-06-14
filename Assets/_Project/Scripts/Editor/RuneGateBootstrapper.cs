using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuneGate.Editor
{
    public static class RuneGateBootstrapper
    {
        private const string RootPath = "Assets/_Project";
        private const string TitleScenePath = RootPath + "/Scenes/TitleScene.unity";
        private const string StageSelectScenePath = RootPath + "/Scenes/StageSelectScene.unity";
        private const string BattleScenePath = RootPath + "/Scenes/BattleScene.unity";
        private const string UpgradeScenePath = RootPath + "/Scenes/UpgradeScene.unity";

        private static readonly string[] RequiredFolders =
        {
            RootPath + "/Scripts/Core",
            RootPath + "/Scripts/Battle",
            RootPath + "/Scripts/Hero",
            RootPath + "/Scripts/Monster",
            RootPath + "/Scripts/Skill",
            RootPath + "/Scripts/Rune",
            RootPath + "/Scripts/Wave",
            RootPath + "/Scripts/Data",
            RootPath + "/Scripts/Progression",
            RootPath + "/Scripts/Save",
            RootPath + "/Scripts/UI",
            RootPath + "/Scripts/Editor",
            RootPath + "/Data/Heroes",
            RootPath + "/Data/Monsters",
            RootPath + "/Data/Skills",
            RootPath + "/Data/Runes",
            RootPath + "/Data/Stages",
            RootPath + "/Data/Upgrades",
            RootPath + "/Prefabs/Heroes",
            RootPath + "/Prefabs/Monsters",
            RootPath + "/Prefabs/UI",
            RootPath + "/Scenes",
            RootPath + "/Art/Characters",
            RootPath + "/Art/Monsters",
            RootPath + "/Art/Effects",
            RootPath + "/Art/UI",
            RootPath + "/Audio",
            RootPath + "/Resources"
        };

        [MenuItem("Tools/RuneGate/Bootstrap Playable Prototype")]
        public static void BootstrapPlayablePrototype()
        {
            EnsureRequiredFolders();

            SkillData shieldBash = CreateShieldBash();
            SkillData rapidShot = CreateRapidShot();
            HeroData knight = CreateKnight(shieldBash);
            HeroData archer = CreateArcher(rapidShot);
            MonsterData goblin = CreateGoblin();
            MonsterData orc = CreateOrc();
            RuneData swordRune = CreateSwordRune();
            RuneData bowRune = CreateBowRune();
            RuneData healingRune = CreateHealingRune();
            UpgradeData[] upgrades = CreateSampleUpgrades();
            StageData stage = CreateSampleStage(goblin, orc);

            CreateOrUpdateBattleScene(stage, knight, archer, new[] { swordRune, bowRune, healingRune }, upgrades, new[] { stage });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("RuneGate playable prototype bootstrap complete. Open Assets/_Project/Scenes/BattleScene.unity and press Play.");
        }

        [MenuItem("Tools/RuneGate/Bootstrap Progression Prototype")]
        public static void BootstrapProgressionPrototype()
        {
            EnsureRequiredFolders();

            SkillData shieldBash = CreateShieldBash();
            SkillData rapidShot = CreateRapidShot();
            HeroData knight = CreateKnight(shieldBash);
            HeroData archer = CreateArcher(rapidShot);
            MonsterData goblin = CreateGoblin();
            MonsterData orc = CreateOrc();
            RuneData swordRune = CreateSwordRune();
            RuneData bowRune = CreateBowRune();
            RuneData healingRune = CreateHealingRune();
            UpgradeData[] upgrades = CreateSampleUpgrades();
            StageData[] stages = CreateSampleStages(goblin, orc);

            CreateOrUpdateTitleScene();
            CreateOrUpdateStageSelectScene(stages);
            CreateOrUpdateBattleScene(stages[0], knight, archer, new[] { swordRune, bowRune, healingRune }, upgrades, stages);
            CreateOrUpdateUpgradeScene(upgrades);
            UpdateBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("RuneGate progression prototype bootstrap complete. Open Assets/_Project/Scenes/TitleScene.unity and press Play.");
        }

        [MenuItem("Tools/RuneGate/Bootstrap MVP")]
        public static void BootstrapMvpAlias()
        {
            BootstrapPlayablePrototype();
        }

        public static void EnsureRequiredFolders()
        {
            for (int i = 0; i < RequiredFolders.Length; i++)
            {
                Directory.CreateDirectory(RequiredFolders[i]);
            }

            AssetDatabase.Refresh();
        }

        private static SkillData CreateShieldBash()
        {
            SkillData asset = CreateOrLoadAsset<SkillData>(RootPath + "/Data/Skills/Shield Bash.asset");
            EditAsset(asset, serializedObject =>
            {
                SetString(serializedObject, "skillId", "skill_shield_bash_001");
                SetString(serializedObject, "displayName", "Shield Bash");
                SetString(serializedObject, "description", "Deals direct light damage to the nearest monster in range.");
                SetFloat(serializedObject, "cooldown", 8f);
                SetInt(serializedObject, "power", 60);
                SetInt(serializedObject, "damageHitCount", 1);
                SetFloat(serializedObject, "range", 2f);
                SetEnum(serializedObject, "targetingType", TargetingType.Nearest);
                SetEnum(serializedObject, "element", ElementType.Light);
            });
            return asset;
        }

        private static SkillData CreateRapidShot()
        {
            SkillData asset = CreateOrLoadAsset<SkillData>(RootPath + "/Data/Skills/Rapid Shot.asset");
            EditAsset(asset, serializedObject =>
            {
                SetString(serializedObject, "skillId", "skill_rapid_shot_001");
                SetString(serializedObject, "displayName", "Rapid Shot");
                SetString(serializedObject, "description", "Rapidly hits the nearest monster 3 times.");
                SetFloat(serializedObject, "cooldown", 6f);
                SetInt(serializedObject, "power", 25);
                SetInt(serializedObject, "damageHitCount", 3);
                SetFloat(serializedObject, "range", 5f);
                SetEnum(serializedObject, "targetingType", TargetingType.Nearest);
                SetEnum(serializedObject, "element", ElementType.Wind);
            });
            return asset;
        }

        private static HeroData CreateKnight(SkillData shieldBash)
        {
            HeroData asset = CreateOrLoadAsset<HeroData>(RootPath + "/Data/Heroes/Knight.asset");
            EditAsset(asset, serializedObject =>
            {
                SetString(serializedObject, "heroId", "hero_knight_001");
                SetString(serializedObject, "displayName", "Knight");
                SetEnum(serializedObject, "role", HeroRole.Tank);
                SetEnum(serializedObject, "positionType", HeroPositionType.Front);
                SetEnum(serializedObject, "element", ElementType.Light);
                SetInt(serializedObject, "maxHp", 400);
                SetInt(serializedObject, "attack", 20);
                SetFloat(serializedObject, "attackSpeed", 1f);
                SetFloat(serializedObject, "attackRange", 1.5f);
                SetObject(serializedObject, "skillData", shieldBash);
                SetObject(serializedObject, "portrait", null);
                SetObject(serializedObject, "animatorController", null);
            });
            return asset;
        }

        private static HeroData CreateArcher(SkillData rapidShot)
        {
            HeroData asset = CreateOrLoadAsset<HeroData>(RootPath + "/Data/Heroes/Archer.asset");
            EditAsset(asset, serializedObject =>
            {
                SetString(serializedObject, "heroId", "hero_archer_001");
                SetString(serializedObject, "displayName", "Archer");
                SetEnum(serializedObject, "role", HeroRole.RangedDps);
                SetEnum(serializedObject, "positionType", HeroPositionType.Back);
                SetEnum(serializedObject, "element", ElementType.Wind);
                SetInt(serializedObject, "maxHp", 180);
                SetInt(serializedObject, "attack", 28);
                SetFloat(serializedObject, "attackSpeed", 1.4f);
                SetFloat(serializedObject, "attackRange", 5f);
                SetObject(serializedObject, "skillData", rapidShot);
                SetObject(serializedObject, "portrait", null);
                SetObject(serializedObject, "animatorController", null);
            });
            return asset;
        }

        private static MonsterData CreateGoblin()
        {
            MonsterData asset = CreateOrLoadAsset<MonsterData>(RootPath + "/Data/Monsters/Goblin.asset");
            EditAsset(asset, serializedObject =>
            {
                SetString(serializedObject, "monsterId", "monster_goblin_001");
                SetString(serializedObject, "displayName", "Goblin");
                SetEnum(serializedObject, "monsterType", MonsterType.Normal);
                SetEnum(serializedObject, "element", ElementType.None);
                SetInt(serializedObject, "maxHp", 60);
                SetFloat(serializedObject, "moveSpeed", 1.2f);
                SetInt(serializedObject, "damageToCrystal", 5);
                SetInt(serializedObject, "rewardGold", 2);
                SetObject(serializedObject, "sprite", null);
                SetObject(serializedObject, "animatorController", null);
            });
            return asset;
        }

        private static MonsterData CreateOrc()
        {
            MonsterData asset = CreateOrLoadAsset<MonsterData>(RootPath + "/Data/Monsters/Orc.asset");
            EditAsset(asset, serializedObject =>
            {
                SetString(serializedObject, "monsterId", "monster_orc_001");
                SetString(serializedObject, "displayName", "Orc");
                SetEnum(serializedObject, "monsterType", MonsterType.Tank);
                SetEnum(serializedObject, "element", ElementType.None);
                SetInt(serializedObject, "maxHp", 180);
                SetFloat(serializedObject, "moveSpeed", 0.7f);
                SetInt(serializedObject, "damageToCrystal", 15);
                SetInt(serializedObject, "rewardGold", 8);
                SetObject(serializedObject, "sprite", null);
                SetObject(serializedObject, "animatorController", null);
            });
            return asset;
        }

        private static RuneData CreateSwordRune()
        {
            RuneData asset = CreateOrLoadAsset<RuneData>(RootPath + "/Data/Runes/Sword Rune.asset");
            EditAsset(asset, serializedObject =>
            {
                SetString(serializedObject, "runeId", "rune_sword_001");
                SetString(serializedObject, "displayName", "Sword Rune");
                SetString(serializedObject, "description", "Increases all hero attack.");
                SetEnum(serializedObject, "rarity", RuneRarity.Common);
                SetEnum(serializedObject, "element", ElementType.Fire);
                SetString(serializedObject, "effectKey", "hero_attack_percent");
                SetFloat(serializedObject, "value", 0.2f);
                SetObject(serializedObject, "icon", null);
            });
            return asset;
        }

        private static RuneData CreateBowRune()
        {
            RuneData asset = CreateOrLoadAsset<RuneData>(RootPath + "/Data/Runes/Bow Rune.asset");
            EditAsset(asset, serializedObject =>
            {
                SetString(serializedObject, "runeId", "rune_bow_001");
                SetString(serializedObject, "displayName", "Bow Rune");
                SetString(serializedObject, "description", "Increases all hero attack speed.");
                SetEnum(serializedObject, "rarity", RuneRarity.Common);
                SetEnum(serializedObject, "element", ElementType.Wind);
                SetString(serializedObject, "effectKey", "hero_attack_speed_percent");
                SetFloat(serializedObject, "value", 0.15f);
                SetObject(serializedObject, "icon", null);
            });
            return asset;
        }

        private static RuneData CreateHealingRune()
        {
            RuneData asset = CreateOrLoadAsset<RuneData>(RootPath + "/Data/Runes/Healing Rune.asset");
            EditAsset(asset, serializedObject =>
            {
                SetString(serializedObject, "runeId", "rune_healing_001");
                SetString(serializedObject, "displayName", "Healing Rune");
                SetString(serializedObject, "description", "Restores flat HP to the Kingdom Crystal.");
                SetEnum(serializedObject, "rarity", RuneRarity.Common);
                SetEnum(serializedObject, "element", ElementType.Light);
                SetString(serializedObject, "effectKey", "crystal_heal_flat");
                SetFloat(serializedObject, "value", 30f);
                SetObject(serializedObject, "icon", null);
            });
            return asset;
        }

        private static StageData CreateSampleStage(MonsterData goblin, MonsterData orc)
        {
            return CreateSampleStages(goblin, orc)[0];
        }

        private static StageData[] CreateSampleStages(MonsterData goblin, MonsterData orc)
        {
            WaveSpawnData wave1Lane0 = CreateSpawn("Wave 1 Goblin Lane 0", goblin, 0, 3, 0.5f, 1f);
            WaveSpawnData wave1Lane1 = CreateSpawn("Wave 1 Goblin Lane 1", goblin, 1, 3, 1f, 1f);
            WaveData wave1 = CreateWave("Wave 1", 1, false, new[] { wave1Lane0, wave1Lane1 });

            WaveSpawnData wave2GoblinLane0 = CreateSpawn("Wave 2 Goblin Lane 0", goblin, 0, 3, 0.5f, 0.8f);
            WaveSpawnData wave2OrcLane1 = CreateSpawn("Wave 2 Orc Lane 1", orc, 1, 1, 1f, 1f);
            WaveSpawnData wave2GoblinLane2 = CreateSpawn("Wave 2 Goblin Lane 2", goblin, 2, 3, 1.5f, 0.8f);
            WaveData wave2 = CreateWave("Wave 2", 2, false, new[] { wave2GoblinLane0, wave2OrcLane1, wave2GoblinLane2 });
            StageData stage1 = CreateStage("Goblin Forest 1", "stage_goblin_forest_01", "Goblin Forest 1", 180, orc, new[] { wave1, wave2 });

            WaveSpawnData stage2Wave1Lane0 = CreateSpawn("Stage 2 Wave 1 Goblin Lane 0", goblin, 0, 4, 0.35f, 0.7f);
            WaveSpawnData stage2Wave1Lane1 = CreateSpawn("Stage 2 Wave 1 Goblin Lane 1", goblin, 1, 4, 0.8f, 0.7f);
            WaveData stage2Wave1 = CreateWave("Stage 2 Wave 1", 1, false, new[] { stage2Wave1Lane0, stage2Wave1Lane1 });

            WaveSpawnData stage2Wave2OrcLane0 = CreateSpawn("Stage 2 Wave 2 Orc Lane 0", orc, 0, 1, 0.5f, 1f);
            WaveSpawnData stage2Wave2GoblinLane1 = CreateSpawn("Stage 2 Wave 2 Goblin Lane 1", goblin, 1, 5, 0.7f, 0.6f);
            WaveSpawnData stage2Wave2GoblinLane2 = CreateSpawn("Stage 2 Wave 2 Goblin Lane 2", goblin, 2, 4, 1f, 0.6f);
            WaveData stage2Wave2 = CreateWave("Stage 2 Wave 2", 2, false, new[] { stage2Wave2OrcLane0, stage2Wave2GoblinLane1, stage2Wave2GoblinLane2 });

            WaveSpawnData stage2Wave3OrcLane1 = CreateSpawn("Stage 2 Wave 3 Orc Lane 1", orc, 1, 2, 0.4f, 1.1f);
            WaveSpawnData stage2Wave3GoblinLane0 = CreateSpawn("Stage 2 Wave 3 Goblin Lane 0", goblin, 0, 5, 0.7f, 0.55f);
            WaveSpawnData stage2Wave3GoblinLane2 = CreateSpawn("Stage 2 Wave 3 Goblin Lane 2", goblin, 2, 5, 0.8f, 0.55f);
            WaveData stage2Wave3 = CreateWave("Stage 2 Wave 3", 3, false, new[] { stage2Wave3OrcLane1, stage2Wave3GoblinLane0, stage2Wave3GoblinLane2 });
            StageData stage2 = CreateStage("Goblin Forest 2", "stage_goblin_forest_02", "Goblin Forest 2", 175, orc, new[] { stage2Wave1, stage2Wave2, stage2Wave3 });

            WaveSpawnData stage3Wave1GoblinLane0 = CreateSpawn("Stage 3 Wave 1 Goblin Lane 0", goblin, 0, 5, 0.5f, 0.6f);
            WaveSpawnData stage3Wave1GoblinLane2 = CreateSpawn("Stage 3 Wave 1 Goblin Lane 2", goblin, 2, 5, 0.7f, 0.6f);
            WaveData stage3Wave1 = CreateWave("Stage 3 Wave 1", 1, false, new[] { stage3Wave1GoblinLane0, stage3Wave1GoblinLane2 });

            WaveSpawnData stage3Wave2OrcLane0 = CreateSpawn("Stage 3 Wave 2 Orc Lane 0", orc, 0, 2, 0.5f, 1.2f);
            WaveSpawnData stage3Wave2GoblinLane1 = CreateSpawn("Stage 3 Wave 2 Goblin Lane 1", goblin, 1, 6, 0.7f, 0.55f);
            WaveSpawnData stage3Wave2OrcLane2 = CreateSpawn("Stage 3 Wave 2 Orc Lane 2", orc, 2, 1, 0.9f, 1.2f);
            WaveData stage3Wave2 = CreateWave("Stage 3 Wave 2", 2, false, new[] { stage3Wave2OrcLane0, stage3Wave2GoblinLane1, stage3Wave2OrcLane2 });

            WaveSpawnData stage3Wave3OrcLane0 = CreateSpawn("Stage 3 Wave 3 Orc Lane 0", orc, 0, 2, 0.4f, 1f);
            WaveSpawnData stage3Wave3OrcLane1 = CreateSpawn("Stage 3 Wave 3 Orc Lane 1", orc, 1, 2, 0.8f, 1f);
            WaveSpawnData stage3Wave3OrcLane2 = CreateSpawn("Stage 3 Wave 3 Orc Lane 2", orc, 2, 2, 1.2f, 1f);
            WaveSpawnData stage3Wave3GoblinLane1 = CreateSpawn("Stage 3 Wave 3 Goblin Lane 1", goblin, 1, 6, 0.6f, 0.5f);
            WaveData stage3Wave3 = CreateWave("Stage 3 Wave 3", 3, false, new[] { stage3Wave3OrcLane0, stage3Wave3OrcLane1, stage3Wave3OrcLane2, stage3Wave3GoblinLane1 });
            StageData stage3 = CreateStage("Goblin Forest 3", "stage_goblin_forest_03", "Goblin Forest 3", 170, orc, new[] { stage3Wave1, stage3Wave2, stage3Wave3 });

            return new[] { stage1, stage2, stage3 };
        }

        private static StageData CreateStage(string assetName, string stageId, string displayName, int crystalHp, MonsterData bossMonster, WaveData[] waves)
        {
            StageData asset = CreateOrLoadAsset<StageData>($"{RootPath}/Data/Stages/{assetName}.asset");
            EditAsset(asset, serializedObject =>
            {
                SetString(serializedObject, "stageId", stageId);
                SetString(serializedObject, "displayName", displayName);
                SetInt(serializedObject, "crystalHp", crystalHp);
                SetObjectList(serializedObject, "waves", ToObjectArray(waves));
                SetObject(serializedObject, "bossMonster", bossMonster);
            });
            return asset;
        }

        private static UpgradeData[] CreateSampleUpgrades()
        {
            UpgradeData crystal = CreateUpgrade(
                "Crystal Reinforcement",
                "crystal_reinforcement",
                "Crystal Reinforcement",
                "Crystal max HP +20 per level.",
                50,
                1.35f,
                10,
                UpgradeManager.CrystalMaxHpFlat,
                20f);

            UpgradeData attack = CreateUpgrade(
                "Hero Training",
                "hero_training",
                "Hero Training",
                "All hero attack +5% per level.",
                50,
                1.35f,
                10,
                UpgradeManager.HeroAttackPercent,
                0.05f);

            UpgradeData rhythm = CreateUpgrade(
                "Battle Rhythm",
                "battle_rhythm",
                "Battle Rhythm",
                "All hero attack speed +3% per level.",
                50,
                1.35f,
                10,
                UpgradeManager.HeroAttackSpeedPercent,
                0.03f);

            UpgradeData practice = CreateUpgrade(
                "Skill Practice",
                "skill_practice",
                "Skill Practice",
                "All hero skill cooldown -3% per level.",
                50,
                1.35f,
                10,
                UpgradeManager.SkillCooldownPercent,
                0.03f);

            return new[] { crystal, attack, rhythm, practice };
        }

        private static UpgradeData CreateUpgrade(string assetName, string upgradeId, string displayName, string description, int baseCost, float costMultiplier, int maxLevel, string effectKey, float valuePerLevel)
        {
            UpgradeData asset = CreateOrLoadAsset<UpgradeData>($"{RootPath}/Data/Upgrades/{assetName}.asset");
            EditAsset(asset, serializedObject =>
            {
                SetString(serializedObject, "upgradeId", upgradeId);
                SetString(serializedObject, "displayName", displayName);
                SetString(serializedObject, "description", description);
                SetInt(serializedObject, "baseCost", baseCost);
                SetFloat(serializedObject, "costMultiplier", costMultiplier);
                SetInt(serializedObject, "maxLevel", maxLevel);
                SetString(serializedObject, "effectKey", effectKey);
                SetFloat(serializedObject, "valuePerLevel", valuePerLevel);
            });
            return asset;
        }

        private static WaveSpawnData CreateSpawn(string assetName, MonsterData monsterData, int laneIndex, int count, float startDelay, float spawnInterval)
        {
            WaveSpawnData asset = CreateOrLoadAsset<WaveSpawnData>($"{RootPath}/Data/Stages/{assetName}.asset");
            EditAsset(asset, serializedObject =>
            {
                SetObject(serializedObject, "monsterData", monsterData);
                SetInt(serializedObject, "laneIndex", laneIndex);
                SetInt(serializedObject, "count", count);
                SetFloat(serializedObject, "startDelay", startDelay);
                SetFloat(serializedObject, "spawnInterval", spawnInterval);
            });
            return asset;
        }

        private static WaveData CreateWave(string assetName, int waveNo, bool isBossWave, WaveSpawnData[] spawns)
        {
            WaveData asset = CreateOrLoadAsset<WaveData>($"{RootPath}/Data/Stages/{assetName}.asset");
            EditAsset(asset, serializedObject =>
            {
                SetInt(serializedObject, "waveNo", waveNo);
                SetBool(serializedObject, "isBossWave", isBossWave);
                SetObjectList(serializedObject, "spawns", ToObjectArray(spawns));
            });
            return asset;
        }

        private static void CreateOrUpdateBattleScene(StageData stageData, HeroData knight, HeroData archer, IReadOnlyList<RuneData> runes, IReadOnlyList<UpgradeData> upgrades, IReadOnlyList<StageData> stages)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.backgroundColor = new Color(0.06f, 0.08f, 0.1f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            GameObject root = new GameObject("RuneGate Battle Root");
            BattleManager battleManager = root.AddComponent<BattleManager>();
            LaneManager laneManager = root.AddComponent<LaneManager>();
            WaveManager waveManager = root.AddComponent<WaveManager>();
            RuneManager runeManager = root.AddComponent<RuneManager>();
            RuneEffectApplier runeEffectApplier = root.AddComponent<RuneEffectApplier>();

            GameObject crystalObject = CreatePlaceholderObject("Kingdom Crystal", null, new Vector3(-5.5f, 0f, 0f), new Color(0.25f, 0.92f, 1f), new Vector2(0.7f, 3.2f), 3);
            CrystalController crystalController = crystalObject.AddComponent<CrystalController>();

            GameObject laneRoot = new GameObject("Lane Points");
            Transform[] spawnPoints = new Transform[3];
            Transform[] targetPoints = new Transform[3];
            Transform[] heroSlotPoints = new Transform[9];
            for (int i = 0; i < 3; i++)
            {
                float y = (i - 1) * 2.4f;
                CreatePlaceholderObject($"Lane {i} Path", laneRoot.transform, new Vector3(0f, y, 0f), new Color(0.25f, 0.27f, 0.32f), new Vector2(10.8f, 0.08f), 0);

                GameObject spawnPoint = new GameObject($"Lane {i} Monster Spawn");
                spawnPoint.transform.SetParent(laneRoot.transform);
                spawnPoint.transform.position = new Vector3(5.6f, y, 0f);
                spawnPoints[i] = spawnPoint.transform;

                GameObject targetPoint = new GameObject($"Lane {i} Crystal Target");
                targetPoint.transform.SetParent(laneRoot.transform);
                targetPoint.transform.position = new Vector3(-5.2f, y, 0f);
                targetPoints[i] = targetPoint.transform;

                for (int slot = 0; slot < 3; slot++)
                {
                    int flatIndex = i * 3 + slot;
                    float x = -2.4f - slot * 0.85f;
                    GameObject slotPoint = CreatePlaceholderObject($"Lane {i} Hero Slot {slot}", laneRoot.transform, new Vector3(x, y, 0f), new Color(0.24f, 0.42f, 0.64f, 0.35f), new Vector2(0.46f, 0.46f), 1);
                    heroSlotPoints[flatIndex] = slotPoint.transform;
                }
            }

            GameObject monsterRoot = new GameObject("Monsters");
            GameObject heroRoot = new GameObject("Heroes");
            HeroController knightController = CreateHero("Knight", knight, heroSlotPoints[3].position, heroRoot.transform, 1, 0);
            HeroController archerController = CreateHero("Archer", archer, heroSlotPoints[4].position, heroRoot.transform, 1, 1);

            GameObject uiRoot = new GameObject("Runtime Prototype UI");
            BattleHUD battleHud = uiRoot.AddComponent<BattleHUD>();
            RuneSelectionUI runeSelectionUI = uiRoot.AddComponent<RuneSelectionUI>();
            StageResultUI stageResultUI = uiRoot.AddComponent<StageResultUI>();
            HeroSkillButton knightSkillButton = uiRoot.AddComponent<HeroSkillButton>();
            HeroSkillButton archerSkillButton = uiRoot.AddComponent<HeroSkillButton>();

            EditComponent(laneManager, serializedObject =>
            {
                SetInt(serializedObject, "laneCount", 3);
                SetFloat(serializedObject, "laneSpacing", 2.4f);
                SetFloat(serializedObject, "spawnX", 5.6f);
                SetFloat(serializedObject, "crystalX", -5.2f);
                SetObjectList(serializedObject, "laneSpawnPoints", ToObjectArray(spawnPoints));
                SetObjectList(serializedObject, "crystalTargetPoints", ToObjectArray(targetPoints));
                SetInt(serializedObject, "heroSlotsPerLane", 3);
                SetFloat(serializedObject, "heroFrontSlotX", -2.4f);
                SetFloat(serializedObject, "heroSlotSpacingX", 0.85f);
                SetObjectList(serializedObject, "heroSlotPoints", ToObjectArray(heroSlotPoints));
            });

            EditComponent(waveManager, serializedObject =>
            {
                SetObject(serializedObject, "stageData", stageData);
                SetObject(serializedObject, "laneManager", laneManager);
                SetObject(serializedObject, "crystalController", crystalController);
                SetObject(serializedObject, "monsterPrefab", null);
                SetObject(serializedObject, "monsterRoot", monsterRoot.transform);
                SetBool(serializedObject, "addDefaultColliderToGeneratedMonsters", true);
            });

            EditComponent(runeManager, serializedObject =>
            {
                SetObjectList(serializedObject, "availableRunes", ToObjectArray(runes));
                SetInt(serializedObject, "optionsPerSelection", 3);
            });

            EditComponent(battleManager, serializedObject =>
            {
                SetObject(serializedObject, "initialStageData", stageData);
                SetObject(serializedObject, "laneManager", laneManager);
                SetObject(serializedObject, "crystalController", crystalController);
                SetObject(serializedObject, "waveManager", waveManager);
                SetObject(serializedObject, "runeManager", runeManager);
                SetObject(serializedObject, "runeEffectApplier", runeEffectApplier);
                SetObjectList(serializedObject, "heroes", new UnityEngine.Object[] { knightController, archerController });
                SetObjectList(serializedObject, "permanentUpgrades", ToObjectArray(upgrades));
                SetBool(serializedObject, "autoStartOnStart", true);
            });

            EditComponent(battleHud, serializedObject =>
            {
                SetObject(serializedObject, "battleManager", battleManager);
                SetObject(serializedObject, "crystalController", crystalController);
                SetBool(serializedObject, "drawRuntimeGui", true);
                SetRect(serializedObject, "panelRect", new Rect(16f, 16f, 300f, 190f));
            });

            EditComponent(runeSelectionUI, serializedObject =>
            {
                SetObject(serializedObject, "battleManager", battleManager);
                SetObject(serializedObject, "runeManager", runeManager);
                SetBool(serializedObject, "drawRuntimeGui", true);
                SetRect(serializedObject, "panelRect", new Rect(300f, 110f, 460f, 310f));
            });

            EditComponent(stageResultUI, serializedObject =>
            {
                SetObject(serializedObject, "battleManager", battleManager);
                SetObjectList(serializedObject, "stageSequence", ToObjectArray(stages));
                SetBool(serializedObject, "drawRuntimeGui", true);
                SetRect(serializedObject, "panelRect", new Rect(300f, 170f, 410f, 230f));
                SetString(serializedObject, "battleSceneName", "BattleScene");
                SetString(serializedObject, "upgradeSceneName", "UpgradeScene");
                SetString(serializedObject, "stageSelectSceneName", "StageSelectScene");
            });

            EditComponent(knightSkillButton, serializedObject =>
            {
                SetObject(serializedObject, "battleManager", battleManager);
                SetObject(serializedObject, "heroController", knightController);
                SetBool(serializedObject, "drawRuntimeGui", true);
                SetRect(serializedObject, "buttonRect", new Rect(16f, 216f, 210f, 54f));
            });

            EditComponent(archerSkillButton, serializedObject =>
            {
                SetObject(serializedObject, "battleManager", battleManager);
                SetObject(serializedObject, "heroController", archerController);
                SetBool(serializedObject, "drawRuntimeGui", true);
                SetRect(serializedObject, "buttonRect", new Rect(16f, 276f, 210f, 54f));
            });

            EditorSceneManager.SaveScene(scene, BattleScenePath);
        }

        private static void CreateOrUpdateTitleScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateDefaultCamera(new Color(0.05f, 0.07f, 0.09f));

            GameObject uiRoot = new GameObject("Title UI");
            TitleUI titleUI = uiRoot.AddComponent<TitleUI>();
            EditComponent(titleUI, serializedObject =>
            {
                SetBool(serializedObject, "drawRuntimeGui", true);
                SetRect(serializedObject, "panelRect", new Rect(320f, 120f, 360f, 260f));
                SetString(serializedObject, "stageSelectSceneName", "StageSelectScene");
            });

            EditorSceneManager.SaveScene(scene, TitleScenePath);
        }

        private static void CreateOrUpdateStageSelectScene(IReadOnlyList<StageData> stages)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateDefaultCamera(new Color(0.06f, 0.08f, 0.1f));

            GameObject uiRoot = new GameObject("Stage Select UI");
            StageSelectUI stageSelectUI = uiRoot.AddComponent<StageSelectUI>();
            EditComponent(stageSelectUI, serializedObject =>
            {
                SetObjectList(serializedObject, "stages", ToObjectArray(stages));
                SetBool(serializedObject, "drawRuntimeGui", true);
                SetRect(serializedObject, "panelRect", new Rect(260f, 80f, 460f, 360f));
                SetString(serializedObject, "titleSceneName", "TitleScene");
                SetString(serializedObject, "battleSceneName", "BattleScene");
                SetString(serializedObject, "upgradeSceneName", "UpgradeScene");
            });

            EditorSceneManager.SaveScene(scene, StageSelectScenePath);
        }

        private static void CreateOrUpdateUpgradeScene(IReadOnlyList<UpgradeData> upgrades)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateDefaultCamera(new Color(0.06f, 0.08f, 0.1f));

            GameObject root = new GameObject("Upgrade Root");
            UpgradeManager upgradeManager = root.AddComponent<UpgradeManager>();
            UpgradeSceneUI upgradeSceneUI = root.AddComponent<UpgradeSceneUI>();

            EditComponent(upgradeManager, serializedObject =>
            {
                SetObjectList(serializedObject, "availableUpgrades", ToObjectArray(upgrades));
            });

            EditComponent(upgradeSceneUI, serializedObject =>
            {
                SetObject(serializedObject, "upgradeManager", upgradeManager);
                SetBool(serializedObject, "drawRuntimeGui", true);
                SetRect(serializedObject, "panelRect", new Rect(220f, 70f, 560f, 440f));
                SetString(serializedObject, "stageSelectSceneName", "StageSelectScene");
            });

            EditorSceneManager.SaveScene(scene, UpgradeScenePath);
        }

        private static void UpdateBuildSettings()
        {
            string[] scenePaths =
            {
                TitleScenePath,
                StageSelectScenePath,
                BattleScenePath,
                UpgradeScenePath
            };

            EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[scenePaths.Length];
            for (int i = 0; i < scenePaths.Length; i++)
            {
                buildScenes[i] = new EditorBuildSettingsScene(scenePaths[i], true);
            }

            EditorBuildSettings.scenes = buildScenes;
        }

        private static Camera CreateDefaultCamera(Color backgroundColor)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.backgroundColor = backgroundColor;
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            return camera;
        }

        private static HeroController CreateHero(string displayName, HeroData heroData, Vector3 position, Transform parent, int laneIndex, int slotIndex)
        {
            GameObject heroObject = CreatePlaceholderObject($"Hero_{displayName}", parent, position, GetHeroColor(heroData), new Vector2(0.72f, 0.72f), 4);
            SkillController skillController = heroObject.AddComponent<SkillController>();
            HeroController heroController = heroObject.AddComponent<HeroController>();

            EditComponent(skillController, serializedObject =>
            {
                SetObject(serializedObject, "skillData", heroData.SkillData);
            });

            EditComponent(heroController, serializedObject =>
            {
                SetObject(serializedObject, "heroData", heroData);
                SetObject(serializedObject, "skillController", skillController);
                SetObject(serializedObject, "projectilePrefab", null);
                SetObject(serializedObject, "projectileSpawnPoint", heroObject.transform);
                SetBool(serializedObject, "initializeOnAwake", true);
                SetInt(serializedObject, "laneIndex", laneIndex);
                SetInt(serializedObject, "heroSlotIndex", slotIndex);
            });

            return heroController;
        }

        private static Color GetHeroColor(HeroData heroData)
        {
            if (heroData == null)
            {
                return Color.white;
            }

            return heroData.Role == HeroRole.Tank ? new Color(0.45f, 0.62f, 1f) : new Color(0.95f, 0.78f, 0.28f);
        }

        private static GameObject CreatePlaceholderObject(string objectName, Transform parent, Vector3 position, Color color, Vector2 size, int sortingOrder)
        {
            GameObject placeholderObject = new GameObject(objectName);
            placeholderObject.transform.SetParent(parent);
            placeholderObject.transform.position = position;
            placeholderObject.AddComponent<SpriteRenderer>();
            PlaceholderSprite placeholderSprite = placeholderObject.AddComponent<PlaceholderSprite>();
            placeholderSprite.Configure(color, size, sortingOrder);
            EditorUtility.SetDirty(placeholderSprite);
            EditorUtility.SetDirty(placeholderObject);
            return placeholderObject;
        }

        private static UnityEngine.Object[] ToObjectArray<T>(IReadOnlyList<T> values) where T : UnityEngine.Object
        {
            UnityEngine.Object[] objects = new UnityEngine.Object[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                objects[i] = values[i];
            }

            return objects;
        }

        private static T CreateOrLoadAsset<T>(string assetPath) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
        }

        private static void EditAsset(UnityEngine.Object asset, Action<SerializedObject> edit)
        {
            EditSerializedObject(asset, edit);
        }

        private static void EditComponent(Component component, Action<SerializedObject> edit)
        {
            EditSerializedObject(component, edit);
        }

        private static void EditSerializedObject(UnityEngine.Object target, Action<SerializedObject> edit)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            edit(serializedObject);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            FindProperty(serializedObject, propertyName).stringValue = value;
        }

        private static void SetInt(SerializedObject serializedObject, string propertyName, int value)
        {
            FindProperty(serializedObject, propertyName).intValue = value;
        }

        private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            FindProperty(serializedObject, propertyName).floatValue = value;
        }

        private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            FindProperty(serializedObject, propertyName).boolValue = value;
        }

        private static void SetColor(SerializedObject serializedObject, string propertyName, Color value)
        {
            FindProperty(serializedObject, propertyName).colorValue = value;
        }

        private static void SetVector2(SerializedObject serializedObject, string propertyName, Vector2 value)
        {
            FindProperty(serializedObject, propertyName).vector2Value = value;
        }

        private static void SetRect(SerializedObject serializedObject, string propertyName, Rect value)
        {
            FindProperty(serializedObject, propertyName).rectValue = value;
        }

        private static void SetEnum<TEnum>(SerializedObject serializedObject, string propertyName, TEnum value) where TEnum : Enum
        {
            FindProperty(serializedObject, propertyName).enumValueIndex = Convert.ToInt32(value);
        }

        private static void SetObject(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            FindProperty(serializedObject, propertyName).objectReferenceValue = value;
        }

        private static void SetObjectList(SerializedObject serializedObject, string propertyName, IReadOnlyList<UnityEngine.Object> values)
        {
            SerializedProperty property = FindProperty(serializedObject, propertyName);
            property.ClearArray();
            for (int i = 0; i < values.Count; i++)
            {
                property.InsertArrayElementAtIndex(i);
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static SerializedProperty FindProperty(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"Could not find serialized property '{propertyName}' on {serializedObject.targetObject.name}.");
            }

            return property;
        }
    }
}
