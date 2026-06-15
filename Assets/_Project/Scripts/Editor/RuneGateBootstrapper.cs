using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Build.Reporting;
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
        private const string AndroidPackageName = "com.hobeomcheon.runegatedefense";

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
            RootPath + "/Data/Formations",
            RootPath + "/Data/Rosters",
            RootPath + "/Prefabs/Heroes",
            RootPath + "/Prefabs/Monsters",
            RootPath + "/Prefabs/Projectiles",
            RootPath + "/Prefabs/Effects",
            RootPath + "/Prefabs/UI",
            RootPath + "/Scenes",
            RootPath + "/Art/Characters/Heroes/Knight",
            RootPath + "/Art/Characters/Heroes/Knight/Sprites",
            RootPath + "/Art/Characters/Heroes/Knight/Animations",
            RootPath + "/Art/Characters/Heroes/Knight/Materials",
            RootPath + "/Art/Characters/Heroes/Archer",
            RootPath + "/Art/Characters/Heroes/Archer/Sprites",
            RootPath + "/Art/Characters/Heroes/Archer/Animations",
            RootPath + "/Art/Characters/Heroes/Archer/Materials",
            RootPath + "/Art/Characters/Heroes/FireMage",
            RootPath + "/Art/Characters/Heroes/FireMage/Sprites",
            RootPath + "/Art/Characters/Heroes/FireMage/Animations",
            RootPath + "/Art/Characters/Heroes/FireMage/Materials",
            RootPath + "/Art/Characters/Heroes/Cleric",
            RootPath + "/Art/Characters/Heroes/Cleric/Sprites",
            RootPath + "/Art/Characters/Heroes/Cleric/Animations",
            RootPath + "/Art/Characters/Heroes/Cleric/Materials",
            RootPath + "/Art/Characters/Heroes/Priest",
            RootPath + "/Art/Characters/Heroes/Priest/Sprites",
            RootPath + "/Art/Characters/Heroes/Priest/Animations",
            RootPath + "/Art/Characters/Heroes/Priest/Materials",
            RootPath + "/Art/Characters/Heroes/DwarfEngineer",
            RootPath + "/Art/Characters/Heroes/DwarfEngineer/Sprites",
            RootPath + "/Art/Characters/Heroes/DwarfEngineer/Animations",
            RootPath + "/Art/Characters/Heroes/DwarfEngineer/Materials",
            RootPath + "/Art/Characters/Heroes/Assassin",
            RootPath + "/Art/Characters/Heroes/Assassin/Sprites",
            RootPath + "/Art/Characters/Heroes/Assassin/Animations",
            RootPath + "/Art/Characters/Heroes/Assassin/Materials",
            RootPath + "/Art/Characters/Monsters/Goblin",
            RootPath + "/Art/Characters/Monsters/Goblin/Sprites",
            RootPath + "/Art/Characters/Monsters/Goblin/Animations",
            RootPath + "/Art/Characters/Monsters/Goblin/Materials",
            RootPath + "/Art/Characters/Monsters/Wolf",
            RootPath + "/Art/Characters/Monsters/Wolf/Sprites",
            RootPath + "/Art/Characters/Monsters/Wolf/Animations",
            RootPath + "/Art/Characters/Monsters/Wolf/Materials",
            RootPath + "/Art/Characters/Monsters/Orc",
            RootPath + "/Art/Characters/Monsters/Orc/Sprites",
            RootPath + "/Art/Characters/Monsters/Orc/Animations",
            RootPath + "/Art/Characters/Monsters/Orc/Materials",
            RootPath + "/Art/Characters/Monsters/Bat",
            RootPath + "/Art/Characters/Monsters/Bat/Sprites",
            RootPath + "/Art/Characters/Monsters/Bat/Animations",
            RootPath + "/Art/Characters/Monsters/Bat/Materials",
            RootPath + "/Art/Characters/Monsters/Slime",
            RootPath + "/Art/Characters/Monsters/Slime/Sprites",
            RootPath + "/Art/Characters/Monsters/Slime/Animations",
            RootPath + "/Art/Characters/Monsters/Slime/Materials",
            RootPath + "/Art/Characters/Monsters/Skeleton",
            RootPath + "/Art/Characters/Monsters/Skeleton/Sprites",
            RootPath + "/Art/Characters/Monsters/Skeleton/Animations",
            RootPath + "/Art/Characters/Monsters/Skeleton/Materials",
            RootPath + "/Art/Characters/Bosses/OrcWarlord",
            RootPath + "/Art/Characters/Bosses/OrcWarlord/Sprites",
            RootPath + "/Art/Characters/Bosses/OrcWarlord/Animations",
            RootPath + "/Art/Characters/Bosses/OrcWarlord/Materials",
            RootPath + "/Art/ConceptSheets",
            RootPath + "/Art/ConceptSheets/Heroes",
            RootPath + "/Art/ConceptSheets/Enemies",
            RootPath + "/Art/RuntimePixel",
            RootPath + "/Art/RuntimePixel/Heroes",
            RootPath + "/Art/RuntimePixel/Heroes/Leon",
            RootPath + "/Art/RuntimePixel/Heroes/Seria",
            RootPath + "/Art/RuntimePixel/Heroes/Kael",
            RootPath + "/Art/RuntimePixel/Heroes/Mirea",
            RootPath + "/Art/RuntimePixel/Heroes/Brom",
            RootPath + "/Art/RuntimePixel/Heroes/Nyx",
            RootPath + "/Art/RuntimePixel/Monsters",
            RootPath + "/Art/RuntimePixel/Bosses",
            RootPath + "/Art/RuntimePixel/UI",
            RootPath + "/Art/Effects/Skills",
            RootPath + "/Art/Effects/Hit",
            RootPath + "/Art/Effects/Projectiles",
            RootPath + "/Art/Effects/Death",
            RootPath + "/Art/UI/Icons",
            RootPath + "/Art/UI/Icons/Heroes",
            RootPath + "/Art/UI/Icons/Skills",
            RootPath + "/Art/UI/Icons/Runes",
            RootPath + "/Art/UI/Icons/Upgrades",
            RootPath + "/Art/UI/Buttons",
            RootPath + "/Art/UI/Panels",
            RootPath + "/Art/UI/Bars",
            RootPath + "/Art/UI/Runes",
            RootPath + "/Art/Backgrounds",
            RootPath + "/Art/Placeholders",
            RootPath + "/Art/Audio/SFX",
            RootPath + "/Art/Audio/BGM",
            RootPath + "/Audio",
            RootPath + "/Audio/SFX",
            RootPath + "/Audio/BGM",
            RootPath + "/Resources"
        };

        [MenuItem("Tools/RuneGate/Bootstrap Playable Prototype")]
        public static void BootstrapPlayablePrototype()
        {
            ContentBundle content = BootstrapContentAndScenes();
            Debug.Log("RuneGate playable prototype bootstrap complete. Open Assets/_Project/Scenes/BattleScene.unity and press Play.");
            Selection.activeObject = content.Stages[0];
        }

        [MenuItem("Tools/RuneGate/Bootstrap Progression Prototype")]
        public static void BootstrapProgressionPrototype()
        {
            ContentBundle content = BootstrapContentAndScenes();
            Debug.Log("RuneGate progression prototype bootstrap complete. Open Assets/_Project/Scenes/TitleScene.unity and press Play.");
            Selection.activeObject = content.Stages[0];
        }

        [MenuItem("Tools/RuneGate/Bootstrap v0.4 Content Prototype")]
        public static void BootstrapV04ContentPrototype()
        {
            ContentBundle content = BootstrapContentAndScenes();
            Debug.Log("RuneGate v0.4 content prototype bootstrap complete. Open Assets/_Project/Scenes/TitleScene.unity and press Play.");
            Selection.activeObject = content.DefaultFormation;
        }

        [MenuItem("Tools/RuneGate/Bootstrap v0.5 Art Prototype")]
        public static void BootstrapV05ArtPrototype()
        {
            ContentBundle content = BootstrapContentAndScenes(true);
            Debug.Log("RuneGate v0.5 art prototype bootstrap complete. Open Assets/_Project/Scenes/TitleScene.unity and press Play.");
            Selection.activeObject = content.Heroes != null && content.Heroes.Length > 0 ? content.Heroes[0] : content.DefaultFormation;
        }

        [MenuItem("Tools/RuneGate/Bootstrap v1.0 Release Track")]
        public static void BootstrapV10ReleaseTrack()
        {
            ContentBundle content = BootstrapContentAndScenes(true, "1.0.0", 10);
            Debug.Log("RuneGate v1.0 release-track bootstrap complete. Run Validate Project, open TitleScene, then test Stage 1 through Stage 10.");
            Selection.activeObject = content.Stages != null && content.Stages.Length > 0 ? content.Stages[0] : content.DefaultFormation;
        }

        [MenuItem("Tools/RuneGate/Apply Initial Art Images")]
        public static void ApplyInitialArtImagesMenu()
        {
            ContentBundle content = LoadExistingContentBundle();
            if (content == null)
            {
                content = CreateV04Content();
            }

            ApplyInitialArtImages(content);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("RuneGate initial art image links applied. Missing images keep their placeholder fallback.");
        }

        [MenuItem("Tools/RuneGate/Configure Android Release Settings")]
        public static void ConfigureAndroidReleaseSettingsMenu()
        {
            ConfigureAndroidPlayerSettings("1.0.0", 10);
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("RuneGate Android release settings configured for v1.0.0.");
        }

        [MenuItem("Tools/RuneGate/Build Android APK v1.0")]
        public static void BuildAndroidApkV10()
        {
            BootstrapContentAndScenes(true, "1.0.0", 10);
            string buildDirectory = Path.Combine("Builds", "Android");
            Directory.CreateDirectory(buildDirectory);
            string apkPath = Path.Combine(buildDirectory, "RuneGateDefense-v1.0.0.apk");

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = GetBuildScenePaths(),
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            try
            {
                BuildReport report = BuildPipeline.BuildPlayer(options);
                Debug.Log($"RuneGate Android APK build result: {report.summary.result} at {apkPath}");
            }
            catch (Exception exception)
            {
                Debug.LogError($"RuneGate Android APK build failed. Check Android Build Support installation and Player Settings. {exception.Message}");
            }
        }

        [MenuItem("Tools/RuneGate/Bootstrap Content Prototype v0.4")]
        public static void BootstrapV04ContentPrototypeAlias()
        {
            BootstrapV04ContentPrototype();
        }

        [MenuItem("Tools/RuneGate/Bootstrap Content v0.4")]
        public static void BootstrapV04ContentAlias()
        {
            BootstrapV04ContentPrototype();
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

        private static ContentBundle BootstrapContentAndScenes(bool includeArtPrototype = false, string androidVersion = "0.9.0", int androidVersionCode = 9)
        {
            EnsureRequiredFolders();

            ContentBundle content = CreateV04Content();
            ArtPrototypeBundle artPrototype = includeArtPrototype ? CreateV05ArtPrototypeAssets(content) : null;
            ApplyInitialArtImages(content);
            UpgradeData[] upgrades = CreateSampleUpgrades();

            CreateOrUpdateTitleScene();
            CreateOrUpdateStageSelectScene(content.Stages);
            CreateOrUpdateBattleScene(content.Stages[0], content.Runes, upgrades, content.Stages, content.HeroRoster, content.DefaultFormation, artPrototype);
            CreateOrUpdateUpgradeScene(upgrades);
            UpdateBuildSettings();
            ConfigureAndroidPlayerSettings(androidVersion, androidVersionCode);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return content;
        }

        private static ContentBundle CreateV04Content()
        {
            SkillData shieldBash = CreateSkill("Shield Bash", "skill_shield_bash", "Shield Bash", "Deals light damage to the nearest monster.", 8f, 60, 1, 2f, TargetingType.Nearest, ElementType.Light, "damage", 1f);
            SkillData rapidShot = CreateSkill("Rapid Shot", "skill_rapid_shot", "Rapid Shot", "Hits the nearest monster 3 times.", 6f, 25, 3, 5f, TargetingType.Nearest, ElementType.Wind, "multi_hit_damage", 1f);
            SkillData meteor = CreateSkill("Meteor", "skill_meteor", "Meteor", "Deals area fire damage around a target.", 11f, 55, 1, 4f, TargetingType.HighestHp, ElementType.Fire, "area_damage", 1.6f);
            SkillData holyHeal = CreateSkill("Holy Heal", "skill_holy_heal", "Holy Heal", "Restores HP to the Kingdom Crystal.", 9f, 35, 1, 3.2f, TargetingType.LowestHp, ElementType.Light, "crystal_heal_flat", 1f);
            SkillData buildTurret = CreateSkill("Build Turret", "skill_build_turret", "Build Turret", "Prototype turret hook with fallback damage.", 12f, 35, 1, 3.2f, TargetingType.First, ElementType.Earth, "turret_placeholder", 1f);
            SkillData shadowStrike = CreateSkill("Shadow Strike", "skill_shadow_strike", "Shadow Strike", "High dark damage that prefers boss targets.", 10f, 120, 1, 2.2f, TargetingType.Boss, ElementType.Dark, "damage", 1f);

            HeroData knight = CreateHeroData("Knight", "hero_knight_001", "Knight", "레온", "균열 방패의 기사", "전열 / 방어 / 빛. 무너진 재문 앞에서 마지막까지 버틴 문지기 기사.", "이번에는, 절대 무너지지 않는다.", HeroRole.Tank, HeroPositionType.Front, ElementType.Light, 420, 25, 1f, 1.2f, shieldBash);
            HeroData archer = CreateHeroData("Archer", "hero_archer_001", "Archer", "세리아", "바람길을 읽는 궁수", "후열 / 원거리 / 바람. 균열에서 새는 바람의 방향으로 적의 진입로를 읽는다.", "바람은 이미 답을 알고 있어.", HeroRole.RangedDps, HeroPositionType.Back, ElementType.Wind, 180, 35, 1.5f, 4f, rapidShot);
            HeroData fireMage = CreateHeroData("Fire Mage", "hero_mage_fire_001", "Fire Mage", "카엘", "잿불의 방랑 마법사", "후열 / 광역 / 화염. 재문 사고 이후 떠돌며 균열의 불씨를 태워 봉한다.", "내가 나서는 건 이번뿐이다. …살아남아라.", HeroRole.Mage, HeroPositionType.Back, ElementType.Fire, 150, 55, 0.7f, 3.4f, meteor);
            HeroData cleric = CreateHeroData("Priest", "hero_priest_001", "Priest", "미레아", "금빛 성서의 사제", "중열 / 지원 / 빛. 봉문 의식을 기록한 금빛 성서로 문지기들을 지탱한다.", "", HeroRole.Healer, HeroPositionType.Middle, ElementType.Light, 170, 10, 1f, 3.2f, holyHeal);
            HeroData dwarfEngineer = CreateHeroData("Dwarf Engineer", "hero_engineer_dwarf_001", "Dwarf Engineer", "브롬", "룬포지 기술자", "중열 / 설치 / 기계. 균열 압력을 견디는 룬 장치를 현장에서 조립한다.", "", HeroRole.Engineer, HeroPositionType.Middle, ElementType.Earth, 230, 20, 1f, 2.8f, buildTurret);
            HeroData assassin = CreateHeroData("Shadow Assassin", "hero_assassin_001", "Shadow Assassin", "닉스", "그림자 균열의 암살자", "전열 또는 중열 / 암살 / 어둠. 그림자 재문을 건너 핵심 적성을 먼저 끊는다.", "", HeroRole.Assassin, HeroPositionType.Front, ElementType.Dark, 210, 75, 0.8f, 1.5f, shadowStrike);
            HeroData[] heroes = { knight, archer, fireMage, cleric, dwarfEngineer, assassin };

            MonsterData goblin = CreateMonsterData("Goblin", "monster_goblin_001", "Goblin", "문틈 도깨비", "균열 하급 적성", "문이 덜 닫힌 틈에서 가장 먼저 새어 나오는 소형 적성체.", MonsterType.Normal, ElementType.None, 80, 1.2f, 1, 5);
            MonsterData wolf = CreateMonsterData("Wolf", "monster_wolf_001", "Wolf", "부식 늑대", "봉문 부식형 추적체", "봉문 결계를 갉아먹는 부식 기운을 두른 빠른 추적체.", MonsterType.Fast, ElementType.Wind, 60, 1.85f, 1, 6);
            MonsterData orc = CreateMonsterData("Orc", "monster_orc_001", "Orc", "봉문 파쇄자", "중형 파쇄 적성", "오크풍 체형의 잔재는 남아 있지만, 한국 출시 기준으로는 봉문을 두드리는 파쇄 적성으로 분류한다.", MonsterType.Tank, ElementType.None, 230, 0.7f, 2, 12);
            MonsterData bat = CreateMonsterData("Bat", "monster_bat_001", "Bat", "균열 꼬마귀", "비행 정찰 적성", "균열 위를 낮게 날며 문지기 진형을 흔드는 작은 날개 적성체.", MonsterType.Flying, ElementType.Wind, 55, 2f, 1, 8);
            MonsterData slime = CreateMonsterData("Slime", "monster_slime_001", "Slime", "재문 점액", "잔류 균열 응집체", "닫힌 문 주변에 남은 균열 찌꺼기가 뭉쳐 움직이는 점액형 적성.", MonsterType.Splitter, ElementType.Earth, 130, 0.8f, 1, 10);
            MonsterData skeleton = CreateMonsterData("Skeleton", "monster_skeleton_001", "Skeleton", "균열 잔해병", "기록 오염 잔재", "오래된 전장의 기록이 균열에 오염되어 걸어 나온 잔해 병사.", MonsterType.Undead, ElementType.Dark, 110, 1f, 1, 9);
            MonsterData boss = CreateMonsterData("Orc Warlord", "boss_orc_warlord_001", "Orc Warlord", "그룸바르", "문파괴자", "봉문 위험 기록 최상위 개체. 재문을 직접 찢고 진입하는 대형 파쇄 적성.", MonsterType.Boss, ElementType.None, 1400, 0.45f, 5, 120);

            RuneData[] runes = CreateV04Runes();
            StageData[] stages = CreateV04Stages(goblin, wolf, orc, bat, slime, skeleton, boss);
            HeroRosterData roster = CreateHeroRoster(heroes);
            FormationData formation = CreateDefaultFormation();

            return new ContentBundle
            {
                Skills = new[] { shieldBash, rapidShot, meteor, holyHeal, buildTurret, shadowStrike },
                Heroes = heroes,
                Monsters = new[] { goblin, wolf, orc, bat, slime, skeleton },
                Boss = boss,
                Runes = runes,
                Stages = stages,
                HeroRoster = roster,
                DefaultFormation = formation
            };
        }

        private static ArtPrototypeBundle CreateV05ArtPrototypeAssets(ContentBundle content)
        {
            AnimatorController knightAnimator = CreateCharacterAnimatorController($"{RootPath}/Art/Characters/Heroes/Knight/Animations/Knight_Prototype.controller");
            AnimatorController goblinAnimator = CreateCharacterAnimatorController($"{RootPath}/Art/Characters/Monsters/Goblin/Animations/Goblin_Prototype.controller");

            GameObject hitEffectPrefab = CreateEffectPrefab(
                $"{RootPath}/Prefabs/Effects/Effect_Hit_Small.prefab",
                "Effect_Hit_Small",
                new Color(1f, 0.92f, 0.35f, 0.9f),
                new Vector2(0.42f, 0.42f),
                10,
                0.28f);

            GameObject deathEffectPrefab = CreateEffectPrefab(
                $"{RootPath}/Prefabs/Effects/Effect_Death_Small.prefab",
                "Effect_Death_Small",
                new Color(0.68f, 0.68f, 0.68f, 0.85f),
                new Vector2(0.82f, 0.82f),
                9,
                0.42f);

            GameObject projectilePrefab = CreateProjectilePrefab($"{RootPath}/Prefabs/Projectiles/Projectile_Arrow.prefab");
            GameObject knightPrefab = CreateHeroVisualPrefab($"{RootPath}/Prefabs/Heroes/Hero_Knight.prefab", knightAnimator);
            GameObject goblinPrefab = CreateMonsterVisualPrefab(
                $"{RootPath}/Prefabs/Monsters/Monster_Goblin.prefab",
                goblinAnimator,
                hitEffectPrefab.GetComponent<AutoDestroyEffect>(),
                deathEffectPrefab.GetComponent<AutoDestroyEffect>());

            HeroData knight = FindHero(content, "hero_knight_001");
            if (knight != null)
            {
                EditAsset(knight, serializedObject =>
                {
                    SetObject(serializedObject, "animatorController", knightAnimator);
                    SetObject(serializedObject, "prefab", knightPrefab);
                });
            }

            MonsterData goblin = FindMonster(content, "monster_goblin_001");
            if (goblin != null)
            {
                EditAsset(goblin, serializedObject =>
                {
                    SetObject(serializedObject, "animatorController", goblinAnimator);
                    SetObject(serializedObject, "prefab", goblinPrefab);
                    SetBool(serializedObject, "isBoss", false);
                });
            }

            return new ArtPrototypeBundle
            {
                KnightPrefab = knightPrefab,
                GoblinPrefab = goblinPrefab,
                ProjectilePrefab = projectilePrefab,
                HitEffectPrefab = hitEffectPrefab,
                DeathEffectPrefab = deathEffectPrefab
            };
        }

        private static void ApplyInitialArtImages(ContentBundle content)
        {
            if (content == null)
            {
                Debug.LogWarning("RuneGate initial art image linking skipped because content is missing.");
                return;
            }

            Sprite leonConcept = LoadSpriteByKeyword($"{RootPath}/Art/ConceptSheets/Heroes", "레온");
            Sprite seriaConcept = LoadSpriteByKeyword($"{RootPath}/Art/ConceptSheets/Heroes", "궁수");
            Sprite kaelConcept = LoadSpriteByKeyword($"{RootPath}/Art/ConceptSheets/Heroes", "카엘");
            Sprite mireaConcept = LoadSpriteByKeyword($"{RootPath}/Art/ConceptSheets/Heroes", "미레아");
            Sprite bromConcept = LoadSpriteByKeyword($"{RootPath}/Art/ConceptSheets/Heroes", "브롬");
            Sprite nyxConcept = LoadSpriteByKeyword($"{RootPath}/Art/ConceptSheets/Heroes", "암살자");
            Sprite enemyConcept = LoadSpriteByKeyword($"{RootPath}/Art/ConceptSheets/Enemies", "괴물");
            Sprite bossConcept = LoadSpriteByKeyword($"{RootPath}/Art/ConceptSheets/Enemies", "그룸바르");

            Sprite leonRuntime = LoadSprite($"{RootPath}/Art/RuntimePixel/Heroes/Leon/leon_idle.png");
            Sprite seriaRuntime = LoadSprite($"{RootPath}/Art/RuntimePixel/Heroes/Seria/seria_idle.png");
            Sprite kaelRuntime = LoadSprite($"{RootPath}/Art/RuntimePixel/Heroes/Kael/kael_idle.png");
            Sprite mireaRuntime = LoadSprite($"{RootPath}/Art/RuntimePixel/Heroes/Mirea/mirea_idle.png");
            Sprite bromRuntime = LoadSprite($"{RootPath}/Art/RuntimePixel/Heroes/Brom/brom_idle.png");
            Sprite nyxRuntime = LoadSprite($"{RootPath}/Art/RuntimePixel/Heroes/Nyx/nyx_idle.png");
            Sprite goblinRuntime = LoadSpriteByKeywordStrict($"{RootPath}/Art/RuntimePixel/Monsters", "goblin", "gate_imp");
            Sprite orcRuntime = LoadSpriteByKeywordStrict($"{RootPath}/Art/RuntimePixel/Monsters", "orc", "brute");
            Sprite wolfRuntime = LoadSpriteByKeywordStrict($"{RootPath}/Art/RuntimePixel/Monsters", "wolf");
            Sprite batRuntime = LoadSpriteByKeywordStrict($"{RootPath}/Art/RuntimePixel/Monsters", "bat");
            Sprite slimeRuntime = LoadSpriteByKeywordStrict($"{RootPath}/Art/RuntimePixel/Monsters", "slime");
            Sprite skeletonRuntime = LoadSpriteByKeywordStrict($"{RootPath}/Art/RuntimePixel/Monsters", "bone", "skeleton");
            Sprite bossRuntime = LoadSpriteByKeywordStrict($"{RootPath}/Art/RuntimePixel/Bosses", "grumbar");
            Sprite shieldBashIcon = LoadSpriteByKeyword($"{RootPath}/Art/UI/Icons/Skills", "\uBC29\uD328");
            Sprite rapidShotIcon = LoadSpriteByKeyword($"{RootPath}/Art/UI/Icons/Skills", "\uD654\uC0B4");
            Sprite swordRuneIcon = LoadFirstSprite($"{RootPath}/Art/UI/Icons/Runes");

            ApplyHeroConceptImage(content, "hero_knight_001", leonConcept);
            ApplyHeroConceptImage(content, "hero_archer_001", seriaConcept);
            ApplyHeroConceptImage(content, "hero_mage_fire_001", kaelConcept);
            ApplyHeroConceptImage(content, "hero_priest_001", mireaConcept);
            ApplyHeroConceptImage(content, "hero_engineer_dwarf_001", bromConcept);
            ApplyHeroConceptImage(content, "hero_assassin_001", nyxConcept);

            ApplyHeroRuntimeSprite(content, "hero_knight_001", leonRuntime);
            ApplyHeroRuntimeSprite(content, "hero_archer_001", seriaRuntime);
            ApplyHeroRuntimeSprite(content, "hero_mage_fire_001", kaelRuntime);
            ApplyHeroRuntimeSprite(content, "hero_priest_001", mireaRuntime);
            ApplyHeroRuntimeSprite(content, "hero_engineer_dwarf_001", bromRuntime);
            ApplyHeroRuntimeSprite(content, "hero_assassin_001", nyxRuntime);

            ApplyMonsterConceptImage(content, "monster_goblin_001", enemyConcept);
            ApplyMonsterConceptImage(content, "monster_wolf_001", enemyConcept);
            ApplyMonsterConceptImage(content, "monster_orc_001", enemyConcept);
            ApplyMonsterConceptImage(content, "monster_bat_001", enemyConcept);
            ApplyMonsterConceptImage(content, "monster_slime_001", enemyConcept);
            ApplyMonsterConceptImage(content, "monster_skeleton_001", enemyConcept);
            ApplyMonsterConceptImage(content, "boss_orc_warlord_001", bossConcept);

            ApplyMonsterRuntimeSprite(content, "monster_goblin_001", goblinRuntime);
            ApplyMonsterRuntimeSprite(content, "monster_wolf_001", wolfRuntime);
            ApplyMonsterRuntimeSprite(content, "monster_orc_001", orcRuntime);
            ApplyMonsterRuntimeSprite(content, "monster_bat_001", batRuntime);
            ApplyMonsterRuntimeSprite(content, "monster_slime_001", slimeRuntime);
            ApplyMonsterRuntimeSprite(content, "monster_skeleton_001", skeletonRuntime);
            ApplyMonsterRuntimeSprite(content, "boss_orc_warlord_001", bossRuntime);
            ApplySkillIcon(content, "skill_shield_bash", shieldBashIcon);
            ApplySkillIcon(content, "skill_rapid_shot", rapidShotIcon);
            ApplyRuneIcon(content, "rune_sword", swordRuneIcon);

            Sprite backgroundReference = LoadFirstSprite($"{RootPath}/Art/Backgrounds");
            Sprite conceptReference = LoadFirstSprite($"{RootPath}/Art/ConceptSheets");
            if (backgroundReference == null)
            {
                Debug.LogWarning("RuneGate initial art: Crystal/Gate background image not found. BattleScene keeps solid-color placeholder background.");
            }

            if (conceptReference == null)
            {
                Debug.LogWarning("RuneGate initial art: Concept sheet image not found. ConceptSheets remains a reference-only folder.");
            }
        }

        private static ContentBundle LoadExistingContentBundle()
        {
            HeroData[] heroes =
            {
                LoadAsset<HeroData>($"{RootPath}/Data/Heroes/Knight.asset"),
                LoadAsset<HeroData>($"{RootPath}/Data/Heroes/Archer.asset"),
                LoadAsset<HeroData>($"{RootPath}/Data/Heroes/Fire Mage.asset"),
                LoadAsset<HeroData>($"{RootPath}/Data/Heroes/Priest.asset"),
                LoadAsset<HeroData>($"{RootPath}/Data/Heroes/Dwarf Engineer.asset"),
                LoadAsset<HeroData>($"{RootPath}/Data/Heroes/Shadow Assassin.asset")
            };

            MonsterData[] monsters =
            {
                LoadAsset<MonsterData>($"{RootPath}/Data/Monsters/Goblin.asset"),
                LoadAsset<MonsterData>($"{RootPath}/Data/Monsters/Wolf.asset"),
                LoadAsset<MonsterData>($"{RootPath}/Data/Monsters/Orc.asset"),
                LoadAsset<MonsterData>($"{RootPath}/Data/Monsters/Bat.asset"),
                LoadAsset<MonsterData>($"{RootPath}/Data/Monsters/Slime.asset"),
                LoadAsset<MonsterData>($"{RootPath}/Data/Monsters/Skeleton.asset")
            };

            SkillData[] skills =
            {
                LoadAsset<SkillData>($"{RootPath}/Data/Skills/Shield Bash.asset"),
                LoadAsset<SkillData>($"{RootPath}/Data/Skills/Rapid Shot.asset"),
                LoadAsset<SkillData>($"{RootPath}/Data/Skills/Meteor.asset"),
                LoadAsset<SkillData>($"{RootPath}/Data/Skills/Holy Heal.asset"),
                LoadAsset<SkillData>($"{RootPath}/Data/Skills/Build Turret.asset"),
                LoadAsset<SkillData>($"{RootPath}/Data/Skills/Shadow Strike.asset")
            };

            RuneData[] runes =
            {
                LoadAsset<RuneData>($"{RootPath}/Data/Runes/Sword Rune.asset"),
                LoadAsset<RuneData>($"{RootPath}/Data/Runes/Bow Rune.asset"),
                LoadAsset<RuneData>($"{RootPath}/Data/Runes/Healing Rune.asset"),
                LoadAsset<RuneData>($"{RootPath}/Data/Runes/Fire Rune.asset"),
                LoadAsset<RuneData>($"{RootPath}/Data/Runes/Shield Rune.asset"),
                LoadAsset<RuneData>($"{RootPath}/Data/Runes/Command Rune.asset"),
                LoadAsset<RuneData>($"{RootPath}/Data/Runes/Focus Rune.asset"),
                LoadAsset<RuneData>($"{RootPath}/Data/Runes/Explosion Rune.asset"),
                LoadAsset<RuneData>($"{RootPath}/Data/Runes/Haste Rune.asset"),
                LoadAsset<RuneData>($"{RootPath}/Data/Runes/Frost Rune.asset"),
                LoadAsset<RuneData>($"{RootPath}/Data/Runes/Lightning Rune.asset"),
                LoadAsset<RuneData>($"{RootPath}/Data/Runes/Earth Rune.asset"),
                LoadAsset<RuneData>($"{RootPath}/Data/Runes/Sacrifice Rune.asset"),
                LoadAsset<RuneData>($"{RootPath}/Data/Runes/Guardian Rune.asset"),
                LoadAsset<RuneData>($"{RootPath}/Data/Runes/Mana Rune.asset"),
                LoadAsset<RuneData>($"{RootPath}/Data/Runes/Hunter Rune.asset"),
                LoadAsset<RuneData>($"{RootPath}/Data/Runes/Purification Rune.asset"),
                LoadAsset<RuneData>($"{RootPath}/Data/Runes/Shatter Rune.asset"),
                LoadAsset<RuneData>($"{RootPath}/Data/Runes/Chain Rune.asset"),
                LoadAsset<RuneData>($"{RootPath}/Data/Runes/Turret Rune.asset")
            };

            if (!HasAny(heroes) && !HasAny(monsters) && !HasAny(skills) && !HasAny(runes))
            {
                return null;
            }

            return new ContentBundle
            {
                Skills = skills,
                Heroes = heroes,
                Monsters = monsters,
                Boss = LoadAsset<MonsterData>($"{RootPath}/Data/Monsters/Orc Warlord.asset"),
                Runes = runes
            };
        }

        private static T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        private static bool HasAny(UnityEngine.Object[] assets)
        {
            if (assets == null)
            {
                return false;
            }

            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static RuneData[] CreateV04Runes()
        {
            return new[]
            {
                CreateRune("Sword Rune", "rune_sword", "Sword Rune", "Increases all hero attack.", RuneRarity.Common, ElementType.Fire, "hero_attack_percent", 0.18f),
                CreateRune("Bow Rune", "rune_bow", "Bow Rune", "Increases all hero attack speed.", RuneRarity.Common, ElementType.Wind, "hero_attack_speed_percent", 0.12f),
                CreateRune("Healing Rune", "rune_healing", "Healing Rune", "Restores flat HP to the Kingdom Crystal.", RuneRarity.Common, ElementType.Light, "crystal_heal_flat", 35f),
                CreateRune("Fire Rune", "rune_fire", "Fire Rune", "Improves mage area damage placeholder.", RuneRarity.Common, ElementType.Fire, "mage_area_percent", 0.12f),
                CreateRune("Shield Rune", "rune_shield", "Shield Rune", "Increases tank and frontline HP.", RuneRarity.Common, ElementType.Light, "tank_hp_percent", 0.14f),
                CreateRune("Command Rune", "rune_command", "Command Rune", "Slightly improves all hero stats.", RuneRarity.Rare, ElementType.Light, "all_hero_stats_percent", 0.07f),
                CreateRune("Focus Rune", "rune_focus", "Focus Rune", "Increases hero attack.", RuneRarity.Common, ElementType.None, "hero_attack_percent", 0.1f),
                CreateRune("Explosion Rune", "rune_explosion", "Explosion Rune", "Prototype hook for blast damage.", RuneRarity.Rare, ElementType.Fire, "blast_placeholder", 1f),
                CreateRune("Haste Rune", "rune_haste", "Haste Rune", "Increases attack speed.", RuneRarity.Common, ElementType.Wind, "hero_attack_speed_percent", 0.1f),
                CreateRune("Frost Rune", "rune_frost", "Frost Rune", "Slows active monsters.", RuneRarity.Rare, ElementType.Ice, "enemy_slow_percent", 0.2f),
                CreateRune("Lightning Rune", "rune_lightning", "Lightning Rune", "Prototype hook for chain lightning.", RuneRarity.Rare, ElementType.Lightning, "lightning_placeholder", 1f),
                CreateRune("Earth Rune", "rune_earth", "Earth Rune", "Increases hero max HP.", RuneRarity.Common, ElementType.Earth, "hero_hp_percent", 0.15f),
                CreateRune("Sacrifice Rune", "rune_sacrifice", "Sacrifice Rune", "Damages the crystal to greatly increase hero attack.", RuneRarity.Rare, ElementType.Dark, "sacrifice_crystal_for_attack", 20f),
                CreateRune("Guardian Rune", "rune_guardian", "Guardian Rune", "Prototype hook for protection.", RuneRarity.Rare, ElementType.Light, "crystal_shield_flat", 25f),
                CreateRune("Mana Rune", "rune_mana", "Mana Rune", "Reduces skill cooldowns.", RuneRarity.Rare, ElementType.None, "skill_cooldown_percent", 0.1f),
                CreateRune("Hunter Rune", "rune_hunter", "Hunter Rune", "Increases damage dealt to bosses.", RuneRarity.Epic, ElementType.None, "boss_damage_percent", 0.25f),
                CreateRune("Purification Rune", "rune_purification", "Purification Rune", "Prototype hook for cleansing.", RuneRarity.Common, ElementType.Light, "purify_placeholder", 1f),
                CreateRune("Shatter Rune", "rune_shatter", "Shatter Rune", "Prototype hook for armor break.", RuneRarity.Rare, ElementType.Earth, "crush_placeholder", 1f),
                CreateRune("Chain Rune", "rune_chain", "Chain Rune", "Adds a ranged chain-shot placeholder bonus.", RuneRarity.Rare, ElementType.Lightning, "ranged_chain_shot_placeholder", 0.14f),
                CreateRune("Turret Rune", "rune_turret", "Turret Rune", "Improves turret and engineer placeholder damage.", RuneRarity.Epic, ElementType.Earth, "turret_attack_percent", 0.18f)
            };
        }

        private static StageData[] CreateV04Stages(MonsterData goblin, MonsterData wolf, MonsterData orc, MonsterData bat, MonsterData slime, MonsterData skeleton, MonsterData boss)
        {
            return new[]
            {
                CreateStageFromPlan(1, "Goblin Forest 1", "재문 숲 1", "문틈 정찰", "문틈 도깨비가 처음 새어 나오는 재문 숲 입구.", 180, boss,
                    new WavePlan(false, Spawn(goblin, 0, 3, 0.4f, 0.9f), Spawn(goblin, 1, 3, 0.8f, 0.9f)),
                    new WavePlan(false, Spawn(goblin, 0, 4, 0.3f, 0.75f), Spawn(goblin, 2, 4, 0.7f, 0.75f))),
                CreateStageFromPlan(2, "Goblin Forest 2", "재문 숲 2", "바람길 흔들림", "균열 꼬리바람을 타고 빠른 적성이 섞이기 시작한다.", 185, boss,
                    new WavePlan(false, Spawn(goblin, 0, 4, 0.3f, 0.7f), Spawn(wolf, 1, 2, 1f, 0.8f)),
                    new WavePlan(false, Spawn(goblin, 1, 5, 0.4f, 0.65f), Spawn(wolf, 2, 3, 0.8f, 0.75f)),
                    new WavePlan(false, Spawn(goblin, 0, 5, 0.4f, 0.6f), Spawn(wolf, 1, 3, 0.7f, 0.7f), Spawn(goblin, 2, 4, 0.9f, 0.6f))),
                CreateStageFromPlan(3, "Goblin Forest 3", "재문 숲 3", "파쇄 흔적", "봉문 파쇄자가 전열 압박을 시험하는 구간.", 190, boss,
                    new WavePlan(false, Spawn(goblin, 0, 5, 0.3f, 0.6f), Spawn(goblin, 2, 5, 0.7f, 0.6f)),
                    new WavePlan(false, Spawn(orc, 0, 1, 0.5f, 1f), Spawn(goblin, 1, 6, 0.7f, 0.55f), Spawn(orc, 2, 1, 1f, 1f)),
                    new WavePlan(false, Spawn(orc, 0, 2, 0.4f, 1.1f), Spawn(goblin, 1, 6, 0.8f, 0.5f), Spawn(orc, 2, 2, 1.2f, 1.1f))),
                CreateStageFromPlan(4, "Goblin Forest 4", "재문 숲 4", "낮은 비행로", "균열 꼬마귀가 라인 사이를 흔드는 구간.", 195, boss,
                    new WavePlan(false, Spawn(bat, 0, 3, 0.4f, 0.65f), Spawn(goblin, 1, 4, 0.8f, 0.65f)),
                    new WavePlan(false, Spawn(wolf, 1, 4, 0.3f, 0.65f), Spawn(bat, 2, 4, 0.7f, 0.65f)),
                    new WavePlan(false, Spawn(goblin, 0, 5, 0.4f, 0.55f), Spawn(bat, 1, 4, 0.8f, 0.6f), Spawn(wolf, 2, 3, 1.1f, 0.65f))),
                CreateStageFromPlan(5, "Goblin Forest 5", "재문 숲 5", "봉문 충격", "중형 파쇄 적성이 본격적으로 문을 두드린다.", 205, boss,
                    new WavePlan(false, Spawn(orc, 0, 2, 0.4f, 1f), Spawn(goblin, 1, 5, 0.8f, 0.55f)),
                    new WavePlan(false, Spawn(goblin, 0, 5, 0.3f, 0.55f), Spawn(orc, 1, 2, 0.7f, 1f), Spawn(goblin, 2, 5, 1f, 0.55f)),
                    new WavePlan(false, Spawn(orc, 0, 2, 0.4f, 0.9f), Spawn(orc, 2, 2, 0.8f, 0.9f)),
                    new WavePlan(false, Spawn(goblin, 0, 6, 0.4f, 0.5f), Spawn(orc, 1, 3, 0.6f, 0.9f), Spawn(wolf, 2, 4, 1f, 0.6f))),
                CreateStageFromPlan(6, "Goblin Forest 6", "재문 숲 6", "잔류 점액", "재문 주변의 잔류 균열 찌꺼기가 응집한다.", 210, boss,
                    new WavePlan(false, Spawn(slime, 0, 2, 0.4f, 1f), Spawn(goblin, 1, 5, 0.8f, 0.55f)),
                    new WavePlan(false, Spawn(slime, 1, 3, 0.5f, 0.9f), Spawn(wolf, 2, 4, 0.9f, 0.6f)),
                    new WavePlan(false, Spawn(goblin, 0, 6, 0.3f, 0.5f), Spawn(slime, 1, 3, 0.8f, 0.9f), Spawn(bat, 2, 4, 1f, 0.6f)),
                    new WavePlan(false, Spawn(slime, 0, 3, 0.5f, 0.85f), Spawn(orc, 1, 2, 0.8f, 1f), Spawn(slime, 2, 3, 1.1f, 0.85f))),
                CreateStageFromPlan(7, "Goblin Forest 7", "재문 숲 7", "오염 기록", "균열에 오염된 전장 기록이 잔해병으로 되살아난다.", 215, boss,
                    new WavePlan(false, Spawn(skeleton, 0, 3, 0.4f, 0.75f), Spawn(goblin, 1, 5, 0.8f, 0.55f)),
                    new WavePlan(false, Spawn(wolf, 0, 4, 0.3f, 0.6f), Spawn(skeleton, 2, 4, 0.8f, 0.75f)),
                    new WavePlan(false, Spawn(skeleton, 0, 4, 0.5f, 0.7f), Spawn(bat, 1, 4, 0.8f, 0.6f), Spawn(goblin, 2, 5, 1f, 0.5f)),
                    new WavePlan(false, Spawn(orc, 0, 2, 0.5f, 1f), Spawn(skeleton, 1, 5, 0.8f, 0.65f), Spawn(orc, 2, 2, 1.1f, 1f))),
                CreateStageFromPlan(8, "Goblin Forest 8", "재문 숲 8", "혼합 적성", "여러 균열 적성이 함께 밀려오는 전술 기록 구간.", 220, boss,
                    new WavePlan(false, Spawn(goblin, 0, 6, 0.3f, 0.5f), Spawn(wolf, 1, 4, 0.8f, 0.55f), Spawn(bat, 2, 4, 1f, 0.6f)),
                    new WavePlan(false, Spawn(slime, 0, 3, 0.4f, 0.8f), Spawn(skeleton, 2, 4, 0.8f, 0.7f)),
                    new WavePlan(false, Spawn(orc, 0, 2, 0.5f, 1f), Spawn(wolf, 1, 5, 0.8f, 0.55f), Spawn(slime, 2, 3, 1.1f, 0.8f)),
                    new WavePlan(false, Spawn(skeleton, 0, 5, 0.4f, 0.65f), Spawn(orc, 1, 3, 0.8f, 0.9f), Spawn(bat, 2, 5, 1f, 0.55f))),
                CreateStageFromPlan(9, "Goblin Forest 9", "재문 숲 9", "붕괴 전조", "그룸바르 출현 전 봉문 압력이 급격히 오른다.", 230, boss,
                    new WavePlan(false, Spawn(wolf, 0, 5, 0.3f, 0.55f), Spawn(goblin, 1, 7, 0.7f, 0.45f)),
                    new WavePlan(false, Spawn(slime, 0, 3, 0.4f, 0.8f), Spawn(skeleton, 1, 4, 0.7f, 0.65f), Spawn(bat, 2, 4, 1f, 0.55f)),
                    new WavePlan(false, Spawn(orc, 0, 3, 0.5f, 0.9f), Spawn(wolf, 2, 5, 0.8f, 0.55f)),
                    new WavePlan(false, Spawn(skeleton, 0, 5, 0.4f, 0.65f), Spawn(slime, 1, 4, 0.7f, 0.8f), Spawn(orc, 2, 3, 1f, 0.9f)),
                    new WavePlan(false, Spawn(goblin, 0, 8, 0.3f, 0.42f), Spawn(orc, 1, 4, 0.6f, 0.85f), Spawn(wolf, 2, 6, 0.9f, 0.5f))),
                CreateStageFromPlan(10, "Goblin Forest 10", "재문 숲 10", "문파괴자의 접근", "봉문 위험 기록 최상위 개체 그룸바르가 직접 진입한다.", 240, boss,
                    new WavePlan(false, Spawn(goblin, 0, 7, 0.3f, 0.45f), Spawn(wolf, 2, 5, 0.8f, 0.55f)),
                    new WavePlan(false, Spawn(slime, 0, 4, 0.4f, 0.75f), Spawn(skeleton, 1, 5, 0.7f, 0.6f), Spawn(bat, 2, 5, 1f, 0.55f)),
                    new WavePlan(false, Spawn(orc, 0, 3, 0.4f, 0.85f), Spawn(orc, 2, 3, 0.8f, 0.85f), Spawn(goblin, 1, 8, 1f, 0.4f)),
                    new WavePlan(false, Spawn(skeleton, 0, 5, 0.4f, 0.6f), Spawn(slime, 1, 4, 0.7f, 0.75f), Spawn(wolf, 2, 6, 1f, 0.5f)),
                    new WavePlan(true, Spawn(goblin, 0, 8, 0.4f, 0.45f), Spawn(boss, 1, 1, 1.2f, 1f), Spawn(orc, 2, 4, 1.4f, 0.8f)))
            };
        }

        private static SkillData CreateSkill(string assetName, string skillId, string displayName, string description, float cooldown, int power, int hitCount, float range, TargetingType targetingType, ElementType element, string effectKey, float radius)
        {
            SkillData asset = CreateOrLoadAsset<SkillData>($"{RootPath}/Data/Skills/{assetName}.asset");
            EditAsset(asset, serializedObject =>
            {
                SetString(serializedObject, "skillId", skillId);
                SetString(serializedObject, "displayName", displayName);
                SetString(serializedObject, "description", description);
                SetFloat(serializedObject, "cooldown", cooldown);
                SetInt(serializedObject, "power", power);
                SetInt(serializedObject, "damageHitCount", hitCount);
                SetFloat(serializedObject, "range", range);
                SetString(serializedObject, "effectKey", effectKey);
                SetFloat(serializedObject, "radius", radius);
                SetEnum(serializedObject, "targetingType", targetingType);
                SetEnum(serializedObject, "element", element);
            });
            return asset;
        }

        private static HeroData CreateHeroData(string assetName, string heroId, string displayName, string displayNameKorean, string subtitleKorean, string descriptionKorean, string quoteKorean, HeroRole role, HeroPositionType positionType, ElementType element, int maxHp, int attack, float attackSpeed, float attackRange, SkillData skillData)
        {
            HeroData asset = CreateOrLoadAsset<HeroData>($"{RootPath}/Data/Heroes/{assetName}.asset");
            EditAsset(asset, serializedObject =>
            {
                SetString(serializedObject, "heroId", heroId);
                SetString(serializedObject, "displayName", displayName);
                SetString(serializedObject, "displayNameKorean", displayNameKorean);
                SetString(serializedObject, "subtitleKorean", subtitleKorean);
                SetString(serializedObject, "descriptionKorean", descriptionKorean);
                SetString(serializedObject, "quoteKorean", quoteKorean);
                SetEnum(serializedObject, "role", role);
                SetEnum(serializedObject, "positionType", positionType);
                SetEnum(serializedObject, "element", element);
                SetInt(serializedObject, "maxHp", maxHp);
                SetInt(serializedObject, "attack", attack);
                SetFloat(serializedObject, "attackSpeed", attackSpeed);
                SetFloat(serializedObject, "attackRange", attackRange);
                SetObject(serializedObject, "skillData", skillData);
                SetObject(serializedObject, "portrait", null);
                SetObject(serializedObject, "conceptImage", null);
                SetObject(serializedObject, "battleSprite", null);
                SetObject(serializedObject, "animatorController", null);
            });
            return asset;
        }

        private static MonsterData CreateMonsterData(string assetName, string monsterId, string displayName, string displayNameKorean, string subtitleKorean, string descriptionKorean, MonsterType monsterType, ElementType element, int maxHp, float moveSpeed, int damageToCrystal, int rewardGold)
        {
            MonsterData asset = CreateOrLoadAsset<MonsterData>($"{RootPath}/Data/Monsters/{assetName}.asset");
            EditAsset(asset, serializedObject =>
            {
                SetString(serializedObject, "monsterId", monsterId);
                SetString(serializedObject, "displayName", displayName);
                SetString(serializedObject, "displayNameKorean", displayNameKorean);
                SetString(serializedObject, "subtitleKorean", subtitleKorean);
                SetString(serializedObject, "descriptionKorean", descriptionKorean);
                SetEnum(serializedObject, "monsterType", monsterType);
                SetEnum(serializedObject, "element", element);
                SetInt(serializedObject, "maxHp", maxHp);
                SetFloat(serializedObject, "moveSpeed", moveSpeed);
                SetInt(serializedObject, "damageToCrystal", damageToCrystal);
                SetInt(serializedObject, "rewardGold", rewardGold);
                SetObject(serializedObject, "conceptImage", null);
                SetObject(serializedObject, "runtimeSprite", null);
                SetObject(serializedObject, "sprite", null);
                SetObject(serializedObject, "animatorController", null);
                SetBool(serializedObject, "isBoss", monsterType == MonsterType.Boss);
            });
            return asset;
        }

        private static RuneData CreateRune(string assetName, string runeId, string displayName, string description, RuneRarity rarity, ElementType element, string effectKey, float value)
        {
            RuneData asset = CreateOrLoadAsset<RuneData>($"{RootPath}/Data/Runes/{assetName}.asset");
            EditAsset(asset, serializedObject =>
            {
                SetString(serializedObject, "runeId", runeId);
                SetString(serializedObject, "displayName", displayName);
                SetString(serializedObject, "description", description);
                SetEnum(serializedObject, "rarity", rarity);
                SetEnum(serializedObject, "element", element);
                SetString(serializedObject, "effectKey", effectKey);
                SetFloat(serializedObject, "value", value);
                SetObject(serializedObject, "icon", null);
            });
            return asset;
        }

        private static HeroRosterData CreateHeroRoster(IReadOnlyList<HeroData> heroes)
        {
            HeroRosterData asset = CreateOrLoadAsset<HeroRosterData>($"{RootPath}/Data/Rosters/MVP Hero Roster.asset");
            EditAsset(asset, serializedObject =>
            {
                SetObjectList(serializedObject, "heroes", ToObjectArray(heroes));
            });
            return asset;
        }

        private static FormationData CreateDefaultFormation()
        {
            FormationData asset = CreateOrLoadAsset<FormationData>($"{RootPath}/Data/Formations/Default Formation.asset");
            List<FormationSlot> slots = SaveManager.CreateDefaultFormationSlots();
            EditAsset(asset, serializedObject =>
            {
                SetString(serializedObject, "formationId", "formation_default_mvp");
                SetString(serializedObject, "displayName", "MVP Default Formation");
                SetFormationSlotList(serializedObject, "slots", slots);
            });
            return asset;
        }

        private static StageData CreateStageFromPlan(int stageNumber, string displayName, string displayNameKorean, string subtitleKorean, string descriptionKorean, int crystalHp, MonsterData bossMonster, params WavePlan[] wavePlans)
        {
            WaveData[] waves = new WaveData[wavePlans.Length];
            for (int waveIndex = 0; waveIndex < wavePlans.Length; waveIndex++)
            {
                WavePlan wavePlan = wavePlans[waveIndex];
                WaveSpawnData[] spawns = new WaveSpawnData[wavePlan.Spawns.Length];
                for (int spawnIndex = 0; spawnIndex < wavePlan.Spawns.Length; spawnIndex++)
                {
                    SpawnPlan spawnPlan = wavePlan.Spawns[spawnIndex];
                    string monsterName = spawnPlan.MonsterData != null ? spawnPlan.MonsterData.DisplayName : "Missing";
                    string spawnName = $"Stage {stageNumber:00} Wave {waveIndex + 1} {monsterName} Lane {spawnPlan.LaneIndex} Spawn {spawnIndex + 1}";
                    spawns[spawnIndex] = CreateSpawn(spawnName, spawnPlan.MonsterData, spawnPlan.LaneIndex, spawnPlan.Count, spawnPlan.StartDelay, spawnPlan.SpawnInterval);
                }

                string waveName = $"Stage {stageNumber:00} Wave {waveIndex + 1}";
                waves[waveIndex] = CreateWave(waveName, waveIndex + 1, wavePlan.IsBossWave, spawns);
            }

            return CreateStage($"Goblin Forest {stageNumber}", $"stage_goblin_forest_{stageNumber:00}", displayName, displayNameKorean, subtitleKorean, descriptionKorean, crystalHp, bossMonster, waves);
        }

        private static StageData CreateStage(string assetName, string stageId, string displayName, string displayNameKorean, string subtitleKorean, string descriptionKorean, int crystalHp, MonsterData bossMonster, WaveData[] waves)
        {
            StageData asset = CreateOrLoadAsset<StageData>($"{RootPath}/Data/Stages/{assetName}.asset");
            EditAsset(asset, serializedObject =>
            {
                SetString(serializedObject, "stageId", stageId);
                SetString(serializedObject, "displayName", displayName);
                SetString(serializedObject, "displayNameKorean", displayNameKorean);
                SetString(serializedObject, "subtitleKorean", subtitleKorean);
                SetString(serializedObject, "descriptionKorean", descriptionKorean);
                SetInt(serializedObject, "crystalHp", crystalHp);
                SetObjectList(serializedObject, "waves", ToObjectArray(waves));
                SetObject(serializedObject, "bossMonster", bossMonster);
            });
            return asset;
        }

        private static UpgradeData[] CreateSampleUpgrades()
        {
            UpgradeData crystal = CreateUpgrade("Crystal Reinforcement", "crystal_reinforcement", "Crystal Reinforcement", "Crystal max HP +20 per level.", 50, 1.35f, 10, UpgradeManager.CrystalMaxHpFlat, 20f);
            UpgradeData attack = CreateUpgrade("Hero Training", "hero_training", "Hero Training", "All hero attack +5% per level.", 50, 1.35f, 10, UpgradeManager.HeroAttackPercent, 0.05f);
            UpgradeData rhythm = CreateUpgrade("Battle Rhythm", "battle_rhythm", "Battle Rhythm", "All hero attack speed +3% per level.", 50, 1.35f, 10, UpgradeManager.HeroAttackSpeedPercent, 0.03f);
            UpgradeData practice = CreateUpgrade("Skill Practice", "skill_practice", "Skill Practice", "All hero skill cooldown -3% per level.", 50, 1.35f, 10, UpgradeManager.SkillCooldownPercent, 0.03f);
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

        private static AnimatorController CreateCharacterAnimatorController(string assetPath)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(assetPath);
            }

            EnsureAnimatorParameter(controller, "IsMoving", AnimatorControllerParameterType.Bool);
            EnsureAnimatorParameter(controller, "IsDead", AnimatorControllerParameterType.Bool);
            EnsureAnimatorParameter(controller, "Attack", AnimatorControllerParameterType.Trigger);
            EnsureAnimatorParameter(controller, "Hit", AnimatorControllerParameterType.Trigger);
            EnsureAnimatorParameter(controller, "Skill", AnimatorControllerParameterType.Trigger);
            EnsureAnimatorParameter(controller, "Death", AnimatorControllerParameterType.Trigger);
            EnsureAnimatorState(controller, "Idle");
            EnsureAnimatorState(controller, "Walk");
            EnsureAnimatorState(controller, "Attack");
            EnsureAnimatorState(controller, "Hit");
            EnsureAnimatorState(controller, "Death");
            EnsureAnimatorState(controller, "Skill");
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void EnsureAnimatorParameter(AnimatorController controller, string parameterName, AnimatorControllerParameterType type)
        {
            AnimatorControllerParameter[] parameters = controller.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].name == parameterName)
                {
                    return;
                }
            }

            controller.AddParameter(parameterName, type);
        }

        private static void EnsureAnimatorState(AnimatorController controller, string stateName)
        {
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i].state != null && states[i].state.name == stateName)
                {
                    return;
                }
            }

            AnimatorState state = stateMachine.AddState(stateName);
            if (stateName == "Idle")
            {
                stateMachine.defaultState = state;
            }
        }

        private static GameObject CreateHeroVisualPrefab(string prefabPath, RuntimeAnimatorController animatorController)
        {
            GameObject root = new GameObject("Hero_Knight");
            GameObject visual = CreatePrefabVisual("Visual", root.transform, new Color(0.45f, 0.62f, 1f), new Vector2(0.72f, 0.72f), 4);
            RuntimeSpriteFitter fitter = visual.AddComponent<RuntimeSpriteFitter>();
            fitter.TargetHeight = 1.25f;
            Animator animator = visual.AddComponent<Animator>();
            animator.runtimeAnimatorController = animatorController;

            root.AddComponent<CharacterVisualController>();
            root.AddComponent<HitFlashController>();
            root.AddComponent<SkillController>();
            root.AddComponent<HeroController>();
            AddAnchor(root.transform, "HealthBarAnchor", new Vector3(0f, 0.58f, 0f));
            AddAnchor(root.transform, "SkillEffectAnchor", new Vector3(0.22f, 0.2f, 0f));
            CreatePrefabVisual("SelectionIndicator", root.transform, new Color(1f, 1f, 1f, 0.22f), new Vector2(0.9f, 0.12f), 2).transform.localPosition = new Vector3(0f, -0.44f, 0f);
            return SavePrefab(root, prefabPath);
        }

        private static GameObject CreateMonsterVisualPrefab(string prefabPath, RuntimeAnimatorController animatorController, AutoDestroyEffect hitEffectPrefab, AutoDestroyEffect deathEffectPrefab)
        {
            GameObject root = new GameObject("Monster_Goblin");
            GameObject visual = CreatePrefabVisual("Visual", root.transform, new Color(0.34f, 0.78f, 0.32f), new Vector2(0.62f, 0.62f), 5);
            RuntimeSpriteFitter fitter = visual.AddComponent<RuntimeSpriteFitter>();
            fitter.TargetHeight = 0.9f;
            Animator animator = visual.AddComponent<Animator>();
            animator.runtimeAnimatorController = animatorController;

            root.AddComponent<CharacterVisualController>();
            root.AddComponent<HitFlashController>();
            MonsterController monsterController = root.AddComponent<MonsterController>();
            CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
            collider.radius = 0.32f;
            AddAnchor(root.transform, "HealthBarAnchor", new Vector3(0f, 0.52f, 0f));
            Transform hitEffectAnchor = AddAnchor(root.transform, "HitEffectAnchor", new Vector3(0f, 0.16f, 0f));

            EditComponent(monsterController, serializedObject =>
            {
                SetObject(serializedObject, "hitEffectPrefab", hitEffectPrefab);
                SetObject(serializedObject, "deathEffectPrefab", deathEffectPrefab);
                SetObject(serializedObject, "hitEffectAnchor", hitEffectAnchor);
                SetFloat(serializedObject, "deathDestroyDelay", 0.35f);
            });

            return SavePrefab(root, prefabPath);
        }

        private static GameObject CreateProjectilePrefab(string prefabPath)
        {
            GameObject root = new GameObject("Projectile_Arrow");
            root.AddComponent<SpriteRenderer>();
            PlaceholderSprite placeholderSprite = root.AddComponent<PlaceholderSprite>();
            placeholderSprite.Configure(new Color(1f, 0.85f, 0.25f, 1f), new Vector2(0.22f, 0.08f), 8);
            root.AddComponent<ProjectileController>();
            return SavePrefab(root, prefabPath);
        }

        private static GameObject CreateEffectPrefab(string prefabPath, string prefabName, Color color, Vector2 size, int sortingOrder, float lifetime)
        {
            GameObject root = new GameObject(prefabName);
            root.AddComponent<SpriteRenderer>();
            PlaceholderSprite placeholderSprite = root.AddComponent<PlaceholderSprite>();
            placeholderSprite.Configure(color, size, sortingOrder);
            AutoDestroyEffect autoDestroyEffect = root.AddComponent<AutoDestroyEffect>();
            EditComponent(autoDestroyEffect, serializedObject =>
            {
                SetFloat(serializedObject, "lifetime", lifetime);
                SetBool(serializedObject, "fadeOut", true);
                SetBool(serializedObject, "scaleOut", true);
            });
            return SavePrefab(root, prefabPath);
        }

        private static GameObject CreatePrefabVisual(string objectName, Transform parent, Color color, Vector2 size, int sortingOrder)
        {
            GameObject visualObject = new GameObject(objectName);
            visualObject.transform.SetParent(parent);
            visualObject.transform.localPosition = Vector3.zero;
            visualObject.AddComponent<SpriteRenderer>();
            PlaceholderSprite placeholderSprite = visualObject.AddComponent<PlaceholderSprite>();
            placeholderSprite.Configure(color, size, sortingOrder);
            return visualObject;
        }

        private static Transform AddAnchor(Transform parent, string anchorName, Vector3 localPosition)
        {
            GameObject anchor = new GameObject(anchorName);
            anchor.transform.SetParent(parent);
            anchor.transform.localPosition = localPosition;
            return anchor.transform;
        }

        private static Sprite LoadFirstSprite(string folderPath)
        {
            string[] spritePaths = FindTexturePaths(folderPath);
            return spritePaths.Length > 0 ? LoadSprite(spritePaths[0]) : null;
        }

        private static Sprite LoadSpriteByKeyword(string folderPath, string keyword)
        {
            string[] spritePaths = FindTexturePaths(folderPath);
            for (int i = 0; i < spritePaths.Length; i++)
            {
                if (spritePaths[i].IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return LoadSprite(spritePaths[i]);
                }
            }

            return spritePaths.Length > 0 ? LoadSprite(spritePaths[0]) : null;
        }

        private static Sprite LoadSpriteByKeywordStrict(string folderPath, params string[] keywords)
        {
            string[] spritePaths = FindTexturePaths(folderPath);
            for (int i = 0; i < spritePaths.Length; i++)
            {
                for (int keywordIndex = 0; keywordIndex < keywords.Length; keywordIndex++)
                {
                    string keyword = keywords[keywordIndex];
                    if (!string.IsNullOrWhiteSpace(keyword) && spritePaths[i].IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return LoadSprite(spritePaths[i]);
                    }
                }
            }

            return null;
        }

        private static string[] FindTexturePaths(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                return Array.Empty<string>();
            }

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
            List<string> paths = new List<string>();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (IsSupportedTexturePath(path))
                {
                    paths.Add(path);
                }
            }

            paths.Sort(StringComparer.OrdinalIgnoreCase);
            return paths.ToArray();
        }

        private static bool IsSupportedTexturePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            string extension = Path.GetExtension(path);
            return string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase);
        }

        private static Sprite LoadSprite(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return null;
            }

            EnsureSpriteImportSettings(assetPath);
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static void EnsureSpriteImportSettings(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            bool dirty = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                dirty = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                dirty = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                dirty = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                dirty = true;
            }

            if (dirty)
            {
                importer.SaveAndReimport();
            }
        }

        private static void ApplyHeroConceptImage(ContentBundle content, string heroId, Sprite sprite)
        {
            HeroData hero = FindHero(content, heroId);
            if (hero == null || sprite == null)
            {
                Debug.LogWarning($"RuneGate concept art: hero concept image not linked for {heroId}.");
                return;
            }

            EditAsset(hero, serializedObject =>
            {
                SetObject(serializedObject, "portrait", sprite);
                SetObject(serializedObject, "conceptImage", sprite);
            });
        }

        private static void ApplyHeroRuntimeSprite(ContentBundle content, string heroId, Sprite sprite)
        {
            HeroData hero = FindHero(content, heroId);
            if (hero == null)
            {
                return;
            }

            EditAsset(hero, serializedObject =>
            {
                SetObject(serializedObject, "battleSprite", sprite);
            });

            if (sprite == null)
            {
                Debug.LogWarning($"RuneGate runtime art: no RuntimePixel hero sprite for {heroId}. Battle uses placeholder fallback.");
            }
        }

        private static void ApplyMonsterConceptImage(ContentBundle content, string monsterId, Sprite sprite)
        {
            MonsterData monster = FindMonster(content, monsterId);
            if (monster == null || sprite == null)
            {
                Debug.LogWarning($"RuneGate concept art: monster concept image not linked for {monsterId}.");
                return;
            }

            EditAsset(monster, serializedObject =>
            {
                SetObject(serializedObject, "conceptImage", sprite);
            });
        }

        private static void ApplyMonsterRuntimeSprite(ContentBundle content, string monsterId, Sprite sprite)
        {
            MonsterData monster = FindMonster(content, monsterId);
            if (monster == null)
            {
                return;
            }

            EditAsset(monster, serializedObject =>
            {
                SetObject(serializedObject, "runtimeSprite", sprite);
                SetObject(serializedObject, "sprite", null);
            });

            if (sprite == null)
            {
                Debug.LogWarning($"RuneGate runtime art: no RuntimePixel monster sprite for {monsterId}. Battle uses placeholder fallback.");
            }
        }

        private static void ApplySkillIcon(ContentBundle content, string skillId, Sprite icon)
        {
            SkillData skill = FindSkill(content, skillId);
            if (skill == null || icon == null)
            {
                Debug.LogWarning($"RuneGate initial art: skill icon not linked for {skillId}. UI placeholder text remains active.");
                return;
            }

            EditAsset(skill, serializedObject =>
            {
                SetObject(serializedObject, "icon", icon);
            });
        }

        private static void ApplyRuneIcon(ContentBundle content, string runeId, Sprite icon)
        {
            RuneData rune = FindRune(content, runeId);
            if (rune == null || icon == null)
            {
                Debug.LogWarning($"RuneGate initial art: rune icon not linked for {runeId}. UI placeholder text remains active.");
                return;
            }

            EditAsset(rune, serializedObject =>
            {
                SetObject(serializedObject, "icon", icon);
            });
        }

        private static GameObject SavePrefab(GameObject root, string prefabPath)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static HeroData FindHero(ContentBundle content, string heroId)
        {
            if (content == null || content.Heroes == null)
            {
                return null;
            }

            for (int i = 0; i < content.Heroes.Length; i++)
            {
                HeroData hero = content.Heroes[i];
                if (hero != null && hero.HeroId == heroId)
                {
                    return hero;
                }
            }

            return null;
        }

        private static MonsterData FindMonster(ContentBundle content, string monsterId)
        {
            if (content == null || content.Monsters == null)
            {
                return content != null && content.Boss != null && content.Boss.MonsterId == monsterId ? content.Boss : null;
            }

            for (int i = 0; i < content.Monsters.Length; i++)
            {
                MonsterData monster = content.Monsters[i];
                if (monster != null && monster.MonsterId == monsterId)
                {
                    return monster;
                }
            }

            if (content.Boss != null && content.Boss.MonsterId == monsterId)
            {
                return content.Boss;
            }

            return null;
        }

        private static SkillData FindSkill(ContentBundle content, string skillId)
        {
            if (content == null || content.Skills == null)
            {
                return null;
            }

            for (int i = 0; i < content.Skills.Length; i++)
            {
                SkillData skill = content.Skills[i];
                if (skill != null && skill.SkillId == skillId)
                {
                    return skill;
                }
            }

            return null;
        }

        private static RuneData FindRune(ContentBundle content, string runeId)
        {
            if (content == null || content.Runes == null)
            {
                return null;
            }

            for (int i = 0; i < content.Runes.Length; i++)
            {
                RuneData rune = content.Runes[i];
                if (rune != null && rune.RuneId == runeId)
                {
                    return rune;
                }
            }

            return null;
        }

        private static void CreateOrUpdateBattleScene(StageData stageData, IReadOnlyList<RuneData> runes, IReadOnlyList<UpgradeData> upgrades, IReadOnlyList<StageData> stages, HeroRosterData heroRoster, FormationData defaultFormation, ArtPrototypeBundle artPrototype = null)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.8f;
            camera.backgroundColor = new Color(0.06f, 0.08f, 0.1f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            GameObject root = new GameObject("RuneGate Battle Root");
            BattleManager battleManager = root.AddComponent<BattleManager>();
            LaneManager laneManager = root.AddComponent<LaneManager>();
            WaveManager waveManager = root.AddComponent<WaveManager>();
            RuneManager runeManager = root.AddComponent<RuneManager>();
            RuneEffectApplier runeEffectApplier = root.AddComponent<RuneEffectApplier>();
            HeroPlacementManager heroPlacementManager = root.AddComponent<HeroPlacementManager>();
            root.AddComponent<AudioManager>();

            GameObject crystalObject = CreatePlaceholderObject("Kingdom Crystal", null, new Vector3(-5.65f, 0f, 0f), new Color(0.25f, 0.92f, 1f), new Vector2(0.7f, 3.2f), 3);
            HitFlashController crystalHitFlash = crystalObject.AddComponent<HitFlashController>();
            CrystalController crystalController = crystalObject.AddComponent<CrystalController>();
            EditComponent(crystalController, serializedObject =>
            {
                SetObject(serializedObject, "hitFlashController", crystalHitFlash);
            });

            GameObject laneRoot = new GameObject("Lane Points");
            Transform[] spawnPoints = new Transform[3];
            Transform[] targetPoints = new Transform[3];
            Transform[] heroSlotPoints = new Transform[9];
            for (int laneIndex = 0; laneIndex < 3; laneIndex++)
            {
                float y = (laneIndex - 1) * 2.4f;
                CreatePlaceholderObject($"Lane {laneIndex} Path", laneRoot.transform, new Vector3(0.45f, y, 0f), new Color(0.25f, 0.27f, 0.32f), new Vector2(9.6f, 0.08f), 0);

                GameObject spawnPoint = new GameObject($"Lane {laneIndex} Monster Spawn");
                spawnPoint.transform.SetParent(laneRoot.transform);
                spawnPoint.transform.position = new Vector3(5.6f, y, 0f);
                spawnPoints[laneIndex] = spawnPoint.transform;

                GameObject targetPoint = new GameObject($"Lane {laneIndex} Crystal Target");
                targetPoint.transform.SetParent(laneRoot.transform);
                targetPoint.transform.position = new Vector3(-5.35f, y, 0f);
                targetPoints[laneIndex] = targetPoint.transform;

                for (int slotIndex = 0; slotIndex < 3; slotIndex++)
                {
                    int flatIndex = laneIndex * 3 + slotIndex;
                    float x = -1.25f - slotIndex * 0.75f;
                    GameObject slotPoint = CreatePlaceholderObject($"Lane {laneIndex} Hero Slot {slotIndex}", laneRoot.transform, new Vector3(x, y, 0f), new Color(0.24f, 0.42f, 0.64f, 0.35f), new Vector2(0.46f, 0.46f), 1);
                    HeroPlacementSlot placementSlot = slotPoint.AddComponent<HeroPlacementSlot>();
                    EditComponent(placementSlot, serializedObject =>
                    {
                        SetInt(serializedObject, "laneIndex", laneIndex);
                        SetEnum(serializedObject, "positionType", SlotIndexToPositionType(slotIndex));
                    });
                    heroSlotPoints[flatIndex] = slotPoint.transform;
                }
            }

            GameObject monsterRoot = new GameObject("Monsters");
            GameObject heroRoot = new GameObject("Heroes");

            GameObject uiRoot = new GameObject("Runtime Prototype UI");
            BattleHUD battleHud = uiRoot.AddComponent<BattleHUD>();
            RuneSelectionUI runeSelectionUI = uiRoot.AddComponent<RuneSelectionUI>();
            StageResultUI stageResultUI = uiRoot.AddComponent<StageResultUI>();
            FormationSkillPanelUI formationSkillPanelUI = uiRoot.AddComponent<FormationSkillPanelUI>();
            TutorialManager tutorialManager = uiRoot.AddComponent<TutorialManager>();
            TutorialOverlayUI tutorialOverlayUI = uiRoot.AddComponent<TutorialOverlayUI>();

            EditComponent(laneManager, serializedObject =>
            {
                SetInt(serializedObject, "laneCount", 3);
                SetFloat(serializedObject, "laneSpacing", 2.4f);
                SetFloat(serializedObject, "spawnX", 5.6f);
                SetFloat(serializedObject, "crystalX", -5.35f);
                SetObjectList(serializedObject, "laneSpawnPoints", ToObjectArray(spawnPoints));
                SetObjectList(serializedObject, "crystalTargetPoints", ToObjectArray(targetPoints));
                SetInt(serializedObject, "heroSlotsPerLane", 3);
                SetFloat(serializedObject, "heroFrontSlotX", -1.25f);
                SetFloat(serializedObject, "heroSlotSpacingX", 0.75f);
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

            EditComponent(heroPlacementManager, serializedObject =>
            {
                SetObject(serializedObject, "heroRoster", heroRoster);
                SetObject(serializedObject, "defaultFormation", defaultFormation);
                SetObject(serializedObject, "heroRoot", heroRoot.transform);
                SetBool(serializedObject, "useSavedFormation", true);
                SetBool(serializedObject, "writeDefaultFormationToSave", true);
                SetVector2(serializedObject, "heroPlaceholderSize", new Vector2(1.05f, 1.05f));
            });

            EditComponent(battleManager, serializedObject =>
            {
                SetObject(serializedObject, "initialStageData", stageData);
                SetObject(serializedObject, "laneManager", laneManager);
                SetObject(serializedObject, "crystalController", crystalController);
                SetObject(serializedObject, "waveManager", waveManager);
                SetObject(serializedObject, "runeManager", runeManager);
                SetObject(serializedObject, "runeEffectApplier", runeEffectApplier);
                SetObject(serializedObject, "heroPlacementManager", heroPlacementManager);
                SetObjectList(serializedObject, "heroes", Array.Empty<UnityEngine.Object>());
                SetObjectList(serializedObject, "permanentUpgrades", ToObjectArray(upgrades));
                SetBool(serializedObject, "rebuildHeroesFromFormation", true);
                SetBool(serializedObject, "autoStartOnStart", true);
            });

            EditComponent(battleHud, serializedObject =>
            {
                SetObject(serializedObject, "battleManager", battleManager);
                SetObject(serializedObject, "crystalController", crystalController);
                SetBool(serializedObject, "drawRuntimeGui", true);
                SetRect(serializedObject, "panelRect", new Rect(8f, 12f, 250f, 210f));
            });

            EditComponent(runeSelectionUI, serializedObject =>
            {
                SetObject(serializedObject, "battleManager", battleManager);
                SetObject(serializedObject, "runeManager", runeManager);
                SetBool(serializedObject, "drawRuntimeGui", true);
                SetRect(serializedObject, "panelRect", new Rect(430f, 96f, 420f, 280f));
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

            EditComponent(formationSkillPanelUI, serializedObject =>
            {
                SetObject(serializedObject, "battleManager", battleManager);
                SetBool(serializedObject, "drawRuntimeGui", true);
                SetRect(serializedObject, "panelRect", new Rect(8f, 230f, 220f, 220f));
            });

            EditComponent(tutorialOverlayUI, serializedObject =>
            {
                SetObject(serializedObject, "tutorialManager", tutorialManager);
                SetBool(serializedObject, "drawRuntimeGui", true);
                SetRect(serializedObject, "panelRect", new Rect(430f, 76f, 460f, 240f));
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
                SetRect(serializedObject, "panelRect", new Rect(240f, 40f, 500f, 500f));
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
            string[] scenePaths = GetBuildScenePaths();

            EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[scenePaths.Length];
            for (int i = 0; i < scenePaths.Length; i++)
            {
                buildScenes[i] = new EditorBuildSettingsScene(scenePaths[i], true);
            }

            EditorBuildSettings.scenes = buildScenes;
        }

        private static string[] GetBuildScenePaths()
        {
            return new[]
            {
                TitleScenePath,
                StageSelectScenePath,
                BattleScenePath,
                UpgradeScenePath
            };
        }

        private static void ConfigureAndroidPlayerSettings(string version, int versionCode)
        {
            PlayerSettings.companyName = "Ho Beom Cheon";
            PlayerSettings.productName = "RuneGate Defense";
            PlayerSettings.bundleVersion = string.IsNullOrWhiteSpace(version) ? "1.0.0" : version;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
#pragma warning disable 618
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, AndroidPackageName);
#pragma warning restore 618
            PlayerSettings.Android.bundleVersionCode = Mathf.Max(1, versionCode);
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

        private static SpawnPlan Spawn(MonsterData monsterData, int laneIndex, int count, float startDelay, float spawnInterval)
        {
            return new SpawnPlan(monsterData, laneIndex, count, startDelay, spawnInterval);
        }

        private static HeroPositionType SlotIndexToPositionType(int slotIndex)
        {
            switch (slotIndex)
            {
                case 0:
                    return HeroPositionType.Front;
                case 2:
                    return HeroPositionType.Back;
                case 1:
                default:
                    return HeroPositionType.Middle;
            }
        }

        private static UnityEngine.Object[] ToObjectArray<T>(IReadOnlyList<T> values) where T : UnityEngine.Object
        {
            if (values == null)
            {
                return Array.Empty<UnityEngine.Object>();
            }

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
            serializedObject.Update();
            edit(serializedObject);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);

            if (target is Component component && component.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
            }
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
            int count = values != null ? values.Count : 0;
            property.arraySize = count;
            for (int i = 0; i < count; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static void SetFormationSlotList(SerializedObject serializedObject, string propertyName, IReadOnlyList<FormationSlot> values)
        {
            SerializedProperty property = FindProperty(serializedObject, propertyName);
            property.ClearArray();
            if (values == null)
            {
                return;
            }

            for (int i = 0; i < values.Count; i++)
            {
                FormationSlot value = values[i];
                if (value == null)
                {
                    continue;
                }

                property.InsertArrayElementAtIndex(property.arraySize);
                SerializedProperty element = property.GetArrayElementAtIndex(property.arraySize - 1);
                element.FindPropertyRelative("laneIndex").intValue = Mathf.Clamp(value.LaneIndex, 0, 2);
                element.FindPropertyRelative("positionType").enumValueIndex = Convert.ToInt32(value.PositionType);
                element.FindPropertyRelative("heroId").stringValue = value.HeroId;
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

        private sealed class ContentBundle
        {
            public SkillData[] Skills;
            public HeroData[] Heroes;
            public MonsterData[] Monsters;
            public MonsterData Boss;
            public RuneData[] Runes;
            public StageData[] Stages;
            public HeroRosterData HeroRoster;
            public FormationData DefaultFormation;
        }

        private sealed class ArtPrototypeBundle
        {
            public GameObject KnightPrefab;
            public GameObject GoblinPrefab;
            public GameObject ProjectilePrefab;
            public GameObject HitEffectPrefab;
            public GameObject DeathEffectPrefab;
        }

        private readonly struct SpawnPlan
        {
            public SpawnPlan(MonsterData monsterData, int laneIndex, int count, float startDelay, float spawnInterval)
            {
                MonsterData = monsterData;
                LaneIndex = laneIndex;
                Count = count;
                StartDelay = startDelay;
                SpawnInterval = spawnInterval;
            }

            public MonsterData MonsterData { get; }
            public int LaneIndex { get; }
            public int Count { get; }
            public float StartDelay { get; }
            public float SpawnInterval { get; }
        }

        private readonly struct WavePlan
        {
            public WavePlan(bool isBossWave, params SpawnPlan[] spawns)
            {
                IsBossWave = isBossWave;
                Spawns = spawns ?? Array.Empty<SpawnPlan>();
            }

            public bool IsBossWave { get; }
            public SpawnPlan[] Spawns { get; }
        }
    }
}
