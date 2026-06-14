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
        private const string BattleScenePath = RootPath + "/Scenes/BattleScene.unity";

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
            RootPath + "/Scripts/Save",
            RootPath + "/Scripts/UI",
            RootPath + "/Scripts/Editor",
            RootPath + "/Data/Heroes",
            RootPath + "/Data/Monsters",
            RootPath + "/Data/Skills",
            RootPath + "/Data/Runes",
            RootPath + "/Data/Stages",
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
            StageData stage = CreateSampleStage(goblin, orc);

            CreateOrUpdateBattleScene(stage, knight, archer, new[] { swordRune, bowRune, healingRune });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("RuneGate playable prototype bootstrap complete. Open Assets/_Project/Scenes/BattleScene.unity and press Play.");
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
            WaveSpawnData wave1Lane0 = CreateSpawn("Wave 1 Goblin Lane 0", goblin, 0, 2, 0.5f, 1f);
            WaveSpawnData wave1Lane1 = CreateSpawn("Wave 1 Goblin Lane 1", goblin, 1, 3, 1f, 1f);
            WaveSpawnData wave1Lane2 = CreateSpawn("Wave 1 Goblin Lane 2", goblin, 2, 1, 2f, 1f);
            WaveData wave1 = CreateWave("Wave 1", 1, false, new[] { wave1Lane0, wave1Lane1, wave1Lane2 });

            WaveSpawnData wave2GoblinLane0 = CreateSpawn("Wave 2 Goblin Lane 0", goblin, 0, 2, 0.5f, 0.8f);
            WaveSpawnData wave2OrcLane1 = CreateSpawn("Wave 2 Orc Lane 1", orc, 1, 1, 1f, 1f);
            WaveSpawnData wave2GoblinLane2 = CreateSpawn("Wave 2 Goblin Lane 2", goblin, 2, 2, 1.5f, 0.8f);
            WaveSpawnData wave2OrcLane0 = CreateSpawn("Wave 2 Orc Lane 0", orc, 0, 1, 3f, 1f);
            WaveData wave2 = CreateWave("Wave 2", 2, false, new[] { wave2GoblinLane0, wave2OrcLane1, wave2GoblinLane2, wave2OrcLane0 });

            StageData asset = CreateOrLoadAsset<StageData>(RootPath + "/Data/Stages/Goblin Forest 1.asset");
            EditAsset(asset, serializedObject =>
            {
                SetString(serializedObject, "stageId", "stage_goblin_forest_001");
                SetString(serializedObject, "displayName", "Goblin Forest 1");
                SetInt(serializedObject, "crystalHp", 180);
                SetObjectList(serializedObject, "waves", new UnityEngine.Object[] { wave1, wave2 });
                SetObject(serializedObject, "bossMonster", orc);
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

        private static void CreateOrUpdateBattleScene(StageData stageData, HeroData knight, HeroData archer, IReadOnlyList<RuneData> runes)
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
                SetBool(serializedObject, "drawRuntimeGui", true);
                SetRect(serializedObject, "panelRect", new Rect(300f, 170f, 410f, 190f));
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
