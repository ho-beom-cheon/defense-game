using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RuneGate.Editor
{
    public static class RuneGateProgressionSmokeTest
    {
        private const string RuntimeContentCatalogPath = "Assets/_Project/Resources/RuntimeContentCatalog.asset";
        private const int ExpectedStageCount = 10;
        private const int ExpectedRuneCount = 20;
        private const int ExpectedHeroCount = 6;
        private const int ExpectedSkillCount = 6;

        private static readonly Vector2Int[] RequiredLayoutSizes =
        {
            new Vector2Int(1080, 1920),
            new Vector2Int(720, 1280),
            new Vector2Int(1440, 2560),
            new Vector2Int(1600, 900),
            new Vector2Int(2048, 1152)
        };

        private static readonly string[] RequiredRuntimeRuneEffects =
        {
            "hero_attack_percent",
            "hero_attack_speed_percent",
            "crystal_heal_flat",
            "skill_cooldown_percent",
            "hero_max_hp_percent",
            "monster_slow_percent",
            "boss_damage_percent",
            "mage_area_percent",
            "tank_defense_percent",
            "turret_attack_percent",
            "lightning_chain_percent",
            "splash_damage_percent",
            "crystal_shield_flat",
            "purification_percent",
            "crush_damage_percent",
            "ranged_chain_damage_percent"
        };

        [MenuItem("Tools/RuneGate/Run Progression Smoke Test")]
        public static void RunFromMenu()
        {
            bool passed = RunSmokeTest(out List<string> errors, out List<string> warnings);
            LogResult(passed, errors, warnings);
        }

        public static void RunFromCommandLine()
        {
            bool passed = RunSmokeTest(out List<string> errors, out List<string> warnings);
            LogResult(passed, errors, warnings);
            EditorApplication.Exit(passed ? 0 : 1);
        }

        public static bool RunSmokeTest(out List<string> errors, out List<string> warnings)
        {
            errors = new List<string>();
            warnings = new List<string>();

            RuntimeContentCatalog catalog = AssetDatabase.LoadAssetAtPath<RuntimeContentCatalog>(RuntimeContentCatalogPath);
            if (catalog == null)
            {
                errors.Add($"Missing RuntimeContentCatalog: {RuntimeContentCatalogPath}");
                return false;
            }

            ValidateStages(catalog, errors, warnings);
            ValidateRunes(catalog, errors, warnings);
            ValidateRuneRuntimeRules(errors);
            ValidateHeroSkills(catalog, errors);
            ValidateHeroSkillRuntimeRules(errors);
            ValidateBossPatternRules(errors);
            ValidateFormation(catalog, errors, warnings);
            ValidateDefaultSave(catalog, errors);
            ValidateStageSessionResolution(catalog, errors);
            ValidateBattleResultProgression(catalog, errors);
            ValidateFullStageUnlockProgression(catalog, errors);
            ValidateStagePlayabilityEstimates(catalog, errors, warnings);
            ValidateUpgradePurchaseAfterStageOne(catalog, errors);
            ValidateResponsiveLayouts(errors);
            ValidateAudioRules(errors);
            ValidateDifficultyRules(errors);
            return errors.Count == 0;
        }

        private static void ValidateStages(RuntimeContentCatalog catalog, List<string> errors, List<string> warnings)
        {
            if (catalog.Stages == null || catalog.Stages.Count < ExpectedStageCount)
            {
                errors.Add($"Expected {ExpectedStageCount} stages in RuntimeContentCatalog. Found {CountNonNull(catalog.Stages)}.");
                return;
            }

            bool stageTenHasBossWave = false;
            for (int i = 0; i < ExpectedStageCount; i++)
            {
                StageData stage = catalog.Stages[i];
                if (stage == null)
                {
                    errors.Add($"Stage slot {i + 1} is null.");
                    continue;
                }

                int stageNumber = PrototypeAssetLoader.GetStageNumber(stage);
                if (stageNumber != i + 1)
                {
                    errors.Add($"Stage order mismatch. Slot {i + 1} contains {stage.name} ({stage.StageId}) parsed as {stageNumber}.");
                }

                if (stage.Waves == null || stage.Waves.Count == 0)
                {
                    errors.Add($"{stage.name} has no waves.");
                    continue;
                }

                for (int waveIndex = 0; waveIndex < stage.Waves.Count; waveIndex++)
                {
                    WaveData wave = stage.Waves[waveIndex];
                    ValidateWave(stage, wave, waveIndex, errors);
                    if (stageNumber == 10 && wave != null && wave.IsBossWave)
                    {
                        stageTenHasBossWave = true;
                    }
                }

                if (stageNumber == 10 && stage.BossMonster == null)
                {
                    errors.Add("Stage 10 must reference a boss monster.");
                }
                else if (stageNumber > 0 && stageNumber < 10 && stage.BossMonster != null)
                {
                    errors.Add($"Stage {stageNumber} must not reference the Chapter 1 boss before Stage 10.");
                }
            }

            if (!stageTenHasBossWave)
            {
                errors.Add("Stage 10 must include a boss wave.");
            }

            if (catalog.Stages.Count > ExpectedStageCount)
            {
                warnings.Add($"RuntimeContentCatalog has extra stages beyond Stage {ExpectedStageCount}: {catalog.Stages.Count} total.");
            }
        }

        private static void ValidateStagePlayabilityEstimates(RuntimeContentCatalog catalog, List<string> errors, List<string> warnings)
        {
            float defaultFormationDps = EstimateDefaultFormationDps(catalog);
            if (defaultFormationDps <= 0f)
            {
                errors.Add("Default formation estimated DPS must be greater than zero.");
                return;
            }

            for (int i = 0; i < ExpectedStageCount && catalog.Stages != null && i < catalog.Stages.Count; i++)
            {
                StageData stage = catalog.Stages[i];
                if (stage == null)
                {
                    continue;
                }

                StageCombatEstimate estimate = BuildStageCombatEstimate(stage);
                int stageNumber = PrototypeAssetLoader.GetStageNumber(stage);
                string label = $"{stage.name} ({stage.StageId})";
                if (estimate.MonsterCount <= 0)
                {
                    errors.Add($"{label} has no estimated monsters.");
                }

                if (estimate.TotalMonsterHp <= 0)
                {
                    errors.Add($"{label} has no estimated monster HP.");
                }

                if (stageNumber == 10 && !estimate.HasBossSpawn)
                {
                    errors.Add("Stage 10 must include an actual boss monster spawn in its waves.");
                }

                if (stageNumber < 10 && estimate.HasBossSpawn)
                {
                    warnings.Add($"{label} includes a boss spawn before Stage 10.");
                }

                if (stageNumber >= 4 && estimate.UsedLaneCount < 3)
                {
                    warnings.Add($"{label} uses only {estimate.UsedLaneCount}/3 lanes; later stages should pressure all lanes.");
                }

                float estimatedClearSeconds = estimate.TotalMonsterHp / Mathf.Max(1f, defaultFormationDps);
                if (stageNumber <= 3 && estimatedClearSeconds > 160f)
                {
                    warnings.Add($"{label} estimated clear time is high for an early stage: {estimatedClearSeconds:0.#}s.");
                }

                if (stageNumber >= 7 && estimatedClearSeconds < 35f)
                {
                    warnings.Add($"{label} estimated combat load may be too low for late game: {estimatedClearSeconds:0.#}s.");
                }

                int minimumExpectedReward = MinimumExpectedStageReward(stageNumber);
                if (minimumExpectedReward > 0 && estimate.TotalRewardGold < minimumExpectedReward)
                {
                    warnings.Add($"{label} estimated kill reward {estimate.TotalRewardGold} is below target {minimumExpectedReward}; result minimums may hide this.");
                }
            }
        }

        private static void ValidateWave(StageData stage, WaveData wave, int waveIndex, List<string> errors)
        {
            string waveLabel = $"{stage.name} wave {waveIndex + 1}";
            if (wave == null)
            {
                errors.Add($"{waveLabel} is null.");
                return;
            }

            if (wave.Spawns == null || wave.Spawns.Count == 0)
            {
                errors.Add($"{waveLabel} has no spawns.");
                return;
            }

            for (int spawnIndex = 0; spawnIndex < wave.Spawns.Count; spawnIndex++)
            {
                WaveSpawnData spawn = wave.Spawns[spawnIndex];
                if (spawn == null)
                {
                    errors.Add($"{waveLabel} spawn {spawnIndex + 1} is null.");
                    continue;
                }

                if (spawn.MonsterData == null)
                {
                    errors.Add($"{waveLabel} spawn {spawnIndex + 1} has no monster.");
                }

                if (spawn.LaneIndex < 0 || spawn.LaneIndex > 2)
                {
                    errors.Add($"{waveLabel} spawn {spawnIndex + 1} has invalid lane {spawn.LaneIndex}.");
                }

                if (spawn.Count <= 0)
                {
                    errors.Add($"{waveLabel} spawn {spawnIndex + 1} has invalid count {spawn.Count}.");
                }
            }
        }

        private static void ValidateRunes(RuntimeContentCatalog catalog, List<string> errors, List<string> warnings)
        {
            int runeCount = CountNonNull(catalog.Runes);
            if (runeCount < ExpectedRuneCount)
            {
                errors.Add($"Expected {ExpectedRuneCount} runes. Found {runeCount}.");
            }

            HashSet<string> effectKeys = new HashSet<string>();
            HashSet<string> runeIds = new HashSet<string>();
            for (int i = 0; catalog.Runes != null && i < catalog.Runes.Count; i++)
            {
                RuneData rune = catalog.Runes[i];
                if (rune == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(rune.RuneId) || !runeIds.Add(rune.RuneId))
                {
                    errors.Add($"Rune at index {i} has an empty or duplicate RuneId: {rune.RuneId}");
                }

                if (string.IsNullOrWhiteSpace(rune.DisplayName) || rune.DisplayName.Contains("??"))
                {
                    errors.Add($"{rune.name} has an invalid display name.");
                }

                if (string.IsNullOrWhiteSpace(rune.Description) ||
                    rune.Description.Contains("??") ||
                    rune.Description.IndexOf("hook", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    rune.Description.IndexOf("placeholder", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    errors.Add($"{rune.name} has a developer-facing or invalid description.");
                }

                if (!string.IsNullOrWhiteSpace(rune.EffectKey))
                {
                    effectKeys.Add(rune.EffectKey);
                    if (rune.EffectKey.IndexOf("placeholder", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        errors.Add($"{rune.name} still uses placeholder effect key {rune.EffectKey}.");
                    }

                    if (!RuneEffectApplier.IsImplementedEffectKey(rune.EffectKey))
                    {
                        errors.Add($"{rune.name} uses unsupported effect key {rune.EffectKey}.");
                    }
                }
            }

            for (int i = 0; i < RequiredRuntimeRuneEffects.Length; i++)
            {
                if (!effectKeys.Contains(RequiredRuntimeRuneEffects[i]))
                {
                    errors.Add($"Missing runtime rune effect key: {RequiredRuntimeRuneEffects[i]}");
                }
            }

        }

        private static void ValidateRuneRuntimeRules(List<string> errors)
        {
            GameObject testObject = new GameObject("RuneRuntimeRulesSmoke");
            try
            {
                HeroRuneCombatModifiers modifiers = testObject.AddComponent<HeroRuneCombatModifiers>();
                modifiers.AddLightningDamagePercent(0.35f);
                modifiers.AddSplashDamagePercent(0.3f);
                modifiers.AddChainDamagePercent(0.45f);
                modifiers.AddCrushDamagePercent(0.25f);
                if (!Mathf.Approximately(modifiers.LightningDamagePercent, 0.35f) ||
                    !Mathf.Approximately(modifiers.SplashDamagePercent, 0.3f) ||
                    !Mathf.Approximately(modifiers.ChainDamagePercent, 0.45f) ||
                    !Mathf.Approximately(modifiers.CrushDamagePercent, 0.25f))
                {
                    errors.Add("Hero rune combat modifiers did not retain configured values.");
                }

                int crushedDamage = HeroRuneCombatModifiers.CalculateCrushDamage(100, 0.25f, true);
                int normalDamage = HeroRuneCombatModifiers.CalculateCrushDamage(100, 0.25f, false);
                if (crushedDamage != 125 || normalDamage != 100)
                {
                    errors.Add($"Crush rune damage rule failed. target={crushedDamage}, normal={normalDamage}.");
                }

                float stackedSlow = RuneEffectApplier.CombineSlowPercent(0.2f, 0.2f);
                if (!Mathf.Approximately(stackedSlow, 0.36f))
                {
                    errors.Add($"Monster slow stacking rule failed. Expected 0.36, found {stackedSlow:0.###}.");
                }

                CrystalController crystal = testObject.AddComponent<CrystalController>();
                crystal.Initialize(100);
                crystal.AddShield(35);
                crystal.TakeDamage(20);
                if (crystal.CurrentHp != 100 || crystal.ShieldHp != 15)
                {
                    errors.Add($"Crystal shield did not absorb damage first. HP={crystal.CurrentHp}, shield={crystal.ShieldHp}.");
                }

                crystal.TakeDamage(25);
                if (crystal.CurrentHp != 90 || crystal.ShieldHp != 0)
                {
                    errors.Add($"Crystal shield overflow damage failed. HP={crystal.CurrentHp}, shield={crystal.ShieldHp}.");
                }
            }
            finally
            {
                Object.DestroyImmediate(testObject);
            }
        }

        private static void ValidateHeroSkills(RuntimeContentCatalog catalog, List<string> errors)
        {
            if (catalog.HeroRoster == null || catalog.HeroRoster.Heroes == null)
            {
                errors.Add("RuntimeContentCatalog has no hero roster for skill validation.");
                return;
            }

            HashSet<string> skillIds = new HashSet<string>();
            HashSet<string> effectKeys = new HashSet<string>();
            int skillCount = 0;
            for (int i = 0; i < catalog.HeroRoster.Heroes.Count; i++)
            {
                HeroData hero = catalog.HeroRoster.Heroes[i];
                if (hero == null)
                {
                    continue;
                }

                SkillData skill = hero.SkillData;
                if (skill == null)
                {
                    errors.Add($"{hero.name} has no SkillData.");
                    continue;
                }

                skillCount++;
                if (string.IsNullOrWhiteSpace(skill.SkillId) || !skillIds.Add(skill.SkillId))
                {
                    errors.Add($"{hero.name} has an empty or duplicate skill id: {skill.SkillId}");
                }

                if (!SkillController.IsHeroSkillEffectKey(skill.EffectKey))
                {
                    errors.Add($"{skill.name} uses unsupported hero skill effect key: {skill.EffectKey}");
                }

                if (!effectKeys.Add(skill.EffectKey))
                {
                    errors.Add($"Hero skills must use unique effect keys. Duplicate: {skill.EffectKey}");
                }

                if (string.IsNullOrWhiteSpace(skill.DisplayName) || string.IsNullOrWhiteSpace(skill.Description))
                {
                    errors.Add($"{skill.name} is missing a user-facing name or description.");
                }
            }

            if (skillCount != ExpectedSkillCount || skillIds.Count != ExpectedSkillCount || effectKeys.Count != ExpectedSkillCount)
            {
                errors.Add($"Expected {ExpectedSkillCount} unique hero skills. Found skills={skillCount}, ids={skillIds.Count}, effects={effectKeys.Count}.");
            }
        }

        private static void ValidateHeroSkillRuntimeRules(List<string> errors)
        {
            int bossDamage = SkillController.CalculateShadowStrikeDamage(100, 100, 100, true);
            int executeDamage = SkillController.CalculateShadowStrikeDamage(100, 35, 100, false);
            int bossExecuteDamage = SkillController.CalculateShadowStrikeDamage(100, 35, 100, true);
            if (bossDamage != 130 || executeDamage != 135 || bossExecuteDamage != 165)
            {
                errors.Add($"Shadow Strike damage rules failed. boss={bossDamage}, execute={executeDamage}, bossExecute={bossExecuteDamage}.");
            }

            int heroHeal = SkillController.CalculateHeroHeal(45, 1.2f);
            int crystalHeal = SkillController.CalculateCrystalHeal(heroHeal);
            if (heroHeal != 54 || crystalHeal != 30)
            {
                errors.Add($"Holy Heal split rules failed. hero={heroHeal}, crystal={crystalHeal}.");
            }
        }

        private static void ValidateFormation(RuntimeContentCatalog catalog, List<string> errors, List<string> warnings)
        {
            if (catalog.HeroRoster == null)
            {
                errors.Add("RuntimeContentCatalog has no HeroRoster.");
                return;
            }

            if (catalog.DefaultFormation == null)
            {
                errors.Add("RuntimeContentCatalog has no DefaultFormation.");
                return;
            }

            int heroCount = CountNonNull(catalog.HeroRoster.Heroes);
            if (heroCount < ExpectedHeroCount)
            {
                errors.Add($"Expected {ExpectedHeroCount} heroes in HeroRoster. Found {heroCount}.");
            }

            HashSet<string> heroIds = new HashSet<string>();
            HashSet<string> occupiedSlots = new HashSet<string>();
            for (int i = 0; i < catalog.DefaultFormation.Slots.Count; i++)
            {
                FormationSlot slot = catalog.DefaultFormation.Slots[i];
                if (slot == null)
                {
                    errors.Add($"DefaultFormation slot {i + 1} is null.");
                    continue;
                }

                string slotKey = $"{slot.LaneIndex}:{slot.PositionType}";
                if (!occupiedSlots.Add(slotKey))
                {
                    errors.Add($"DefaultFormation has duplicate placement {slotKey}.");
                }

                if (catalog.HeroRoster.FindHeroById(slot.HeroId) == null)
                {
                    errors.Add($"DefaultFormation references missing hero: {slot.HeroId}");
                }
                else
                {
                    heroIds.Add(slot.HeroId);
                }
            }

            if (heroIds.Count < ExpectedHeroCount)
            {
                warnings.Add($"DefaultFormation uses {heroIds.Count}/{ExpectedHeroCount} heroes.");
            }
        }

        private static float EstimateDefaultFormationDps(RuntimeContentCatalog catalog)
        {
            if (catalog == null || catalog.DefaultFormation == null || catalog.DefaultFormation.Slots == null || catalog.HeroRoster == null)
            {
                return 0f;
            }

            float totalDps = 0f;
            HashSet<string> countedHeroIds = new HashSet<string>();
            for (int i = 0; i < catalog.DefaultFormation.Slots.Count; i++)
            {
                FormationSlot slot = catalog.DefaultFormation.Slots[i];
                if (slot == null || string.IsNullOrWhiteSpace(slot.HeroId) || !countedHeroIds.Add(slot.HeroId))
                {
                    continue;
                }

                HeroData hero = catalog.HeroRoster.FindHeroById(slot.HeroId);
                if (hero != null)
                {
                    totalDps += Mathf.Max(0, hero.Attack) * Mathf.Max(0.1f, hero.AttackSpeed);
                }
            }

            return totalDps;
        }

        private static StageCombatEstimate BuildStageCombatEstimate(StageData stage)
        {
            StageCombatEstimate estimate = new StageCombatEstimate();
            if (stage == null || stage.Waves == null)
            {
                return estimate;
            }

            for (int waveIndex = 0; waveIndex < stage.Waves.Count; waveIndex++)
            {
                WaveData wave = stage.Waves[waveIndex];
                if (wave == null || wave.Spawns == null)
                {
                    continue;
                }

                for (int spawnIndex = 0; spawnIndex < wave.Spawns.Count; spawnIndex++)
                {
                    WaveSpawnData spawn = wave.Spawns[spawnIndex];
                    if (spawn == null || spawn.MonsterData == null || spawn.Count <= 0)
                    {
                        continue;
                    }

                    MonsterData monster = spawn.MonsterData;
                    int count = Mathf.Max(0, spawn.Count);
                    estimate.MonsterCount += count;
                    estimate.TotalMonsterHp += Mathf.Max(1, monster.MaxHp) * count;
                    estimate.TotalRewardGold += Mathf.Max(0, monster.RewardGold) * count;
                    estimate.TotalCrystalDamage += Mathf.Max(0, monster.DamageToCrystal) * count;
                    estimate.LaneMask |= 1 << Mathf.Clamp(spawn.LaneIndex, 0, 2);
                    estimate.HasBossSpawn |= monster.IsBoss;
                }
            }

            return estimate;
        }

        private static int MinimumExpectedStageReward(int stageNumber)
        {
            switch (stageNumber)
            {
                case 1:
                    return 80;
                case 2:
                    return 100;
                case 3:
                    return 120;
                case 10:
                    return 420;
                default:
                    return 0;
            }
        }

        private static void ValidateDefaultSave(RuntimeContentCatalog catalog, List<string> errors)
        {
            SaveData saveData = SaveManager.CreateDefaultSave();
            if (saveData == null)
            {
                errors.Add("SaveManager.CreateDefaultSave returned null.");
                return;
            }

            if (saveData.unlockedStageIds == null || !saveData.unlockedStageIds.Contains(SaveManager.DefaultUnlockedStageId))
            {
                errors.Add($"Default save must unlock {SaveManager.DefaultUnlockedStageId}.");
            }

            if (saveData.saveVersion <= 0)
            {
                errors.Add("Default save must include a positive saveVersion.");
            }

            if (saveData.formationSlots == null || saveData.formationSlots.Count == 0)
            {
                errors.Add("Default save must include formation slots.");
            }
            else
            {
                HashSet<string> occupiedSlots = new HashSet<string>();
                for (int i = 0; i < saveData.formationSlots.Count; i++)
                {
                    FormationSlot slot = saveData.formationSlots[i];
                    if (slot == null)
                    {
                        errors.Add($"Default save formation slot {i + 1} is null.");
                        continue;
                    }

                    string key = $"{slot.LaneIndex}:{slot.PositionType}";
                    if (!occupiedSlots.Add(key))
                    {
                        errors.Add($"Default save has duplicate formation placement: {key}");
                    }
                }
            }

            if (catalog != null && catalog.DefaultFormation != null && catalog.DefaultFormation.Slots != null && catalog.DefaultFormation.Slots.Count > 0)
            {
                if (!FormationSlotsMatch(saveData.formationSlots, catalog.DefaultFormation.Slots))
                {
                    errors.Add("Default save formation must match RuntimeContentCatalog DefaultFormation.");
                }
            }

            if (saveData.hasSeenTutorial)
            {
                errors.Add("Default save must not mark tutorial as seen.");
            }

            if (!DifficultyRules.IsUnlocked(saveData, DifficultyRules.Easy) ||
                !DifficultyRules.IsUnlocked(saveData, DifficultyRules.Normal) ||
                DifficultyRules.IsUnlocked(saveData, DifficultyRules.Hard) ||
                DifficultyRules.IsUnlocked(saveData, DifficultyRules.Nightmare))
            {
                errors.Add("Default save must unlock Easy/Normal and lock Hard/Nightmare.");
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

        private static void ValidateResponsiveLayouts(List<string> errors)
        {
            for (int i = 0; i < RequiredLayoutSizes.Length; i++)
            {
                Vector2Int size = RequiredLayoutSizes[i];
                StageSelectFrameRects stageSelect = GameFrameLayout.StageSelectFrameForSize(size.x, size.y);
                string label = $"{size.x}x{size.y}";

                ValidatePositiveRect($"{label} StageSelect root", stageSelect.FrameRoot, errors);
                ValidateRectInside($"{label} StageSelect header", stageSelect.HeaderArea, stageSelect.FrameRoot, errors);
                ValidateRectInside($"{label} StageSelect difficulty", stageSelect.DifficultyArea, stageSelect.FrameRoot, errors);
                ValidateRectInside($"{label} StageSelect main", stageSelect.MainArea, stageSelect.FrameRoot, errors);
                ValidateRectInside($"{label} StageSelect list", stageSelect.StageListPanel, stageSelect.FrameRoot, errors);
                ValidateRectInside($"{label} StageSelect detail", stageSelect.StageDetailPanel, stageSelect.FrameRoot, errors);
                ValidateRectInside($"{label} StageSelect footer", stageSelect.FooterArea, stageSelect.FrameRoot, errors);

                if (stageSelect.PetContractArea.height > 0f)
                {
                    ValidateRectInside($"{label} StageSelect pet contract", stageSelect.PetContractArea, stageSelect.FrameRoot, errors);
                }

                if (stageSelect.DifficultyArea.height > 0.1f)
                {
                    errors.Add($"{label} StageSelect difficulty area must remain collapsed; difficulty now lives in the header button.");
                }

                if (stageSelect.PetContractArea.height > 0.1f)
                {
                    errors.Add($"{label} StageSelect pet contract area must remain collapsed; pet contract now lives in the header button/popup.");
                }

                if (stageSelect.StageListPanel.Overlaps(stageSelect.StageDetailPanel))
                {
                    errors.Add($"{label} StageSelect list and detail panels overlap.");
                }

                if (stageSelect.MainArea.height < 120f)
                {
                    errors.Add($"{label} StageSelect main area is too short: {stageSelect.MainArea.height:0.0}.");
                }

                if (stageSelect.StageListPanel.width < 220f || stageSelect.StageDetailPanel.width < 220f)
                {
                    errors.Add($"{label} StageSelect list/detail panels are too narrow. List {stageSelect.StageListPanel.width:0.0}, Detail {stageSelect.StageDetailPanel.width:0.0}.");
                }

                ScreenFrameRects upgrade = GameFrameLayout.UpgradeFrameForSize(size.x, size.y);
                ValidatePositiveRect($"{label} Upgrade root", upgrade.FrameRoot, errors);
                ValidateRectInside($"{label} Upgrade header", upgrade.HeaderArea, upgrade.FrameRoot, errors);
                ValidateRectInside($"{label} Upgrade main", upgrade.MainArea, upgrade.FrameRoot, errors);
                ValidateRectInside($"{label} Upgrade footer", upgrade.FooterArea, upgrade.FrameRoot, errors);
                if (upgrade.MainArea.Overlaps(upgrade.FooterArea))
                {
                    errors.Add($"{label} Upgrade main area and footer overlap.");
                }

                if (size.y >= size.x && upgrade.FooterArea.height < 88f)
                {
                    errors.Add($"{label} Upgrade footer is too short for feedback and a touch button: {upgrade.FooterArea.height:0.0}.");
                }

                BattleFrameRects battle = GameFrameLayout.BattleFrameForSize(size.x, size.y);
                Rect safeRect = GameFrameLayout.SafeRectForSize(size.x, size.y);
                ValidatePositiveRect($"{label} Battle root", battle.FrameRoot, errors);
                ValidateRectInside($"{label} Battle header", battle.HeaderArea, battle.FrameRoot, errors);
                ValidateRectInside($"{label} Battle field", battle.BattleFieldFrame, battle.FrameRoot, errors);
                ValidateRectInside($"{label} Battle skill panel", battle.SkillPanelArea, battle.FrameRoot, errors);
                ValidateRectInside($"{label} Battle footer", battle.FooterArea, battle.FrameRoot, errors);
                ValidateRectInside($"{label} Battle popup layer", battle.PopupLayer, safeRect, errors);

                BattleHUD.CalculateHeaderRects(battle.HeaderArea, out Rect stageStatus, out Rect crystalStatus, out Rect battleStatus, out Rect pauseStatus);
                Rect[] headerSections = { stageStatus, crystalStatus, battleStatus, pauseStatus };
                string[] headerSectionNames = { "stage", "crystal", "battle", "pause" };
                for (int headerIndex = 0; headerIndex < headerSections.Length; headerIndex++)
                {
                    ValidateRectInside($"{label} Battle header {headerSectionNames[headerIndex]}", headerSections[headerIndex], battle.HeaderArea, errors);
                    if (headerSections[headerIndex].width < 72f || headerSections[headerIndex].height < 44f)
                    {
                        errors.Add($"{label} Battle header {headerSectionNames[headerIndex]} is too small: {FormatRect(headerSections[headerIndex])}.");
                    }

                    for (int previousIndex = 0; previousIndex < headerIndex; previousIndex++)
                    {
                        if (headerSections[headerIndex].Overlaps(headerSections[previousIndex]))
                        {
                            errors.Add($"{label} Battle header {headerSectionNames[previousIndex]} and {headerSectionNames[headerIndex]} overlap.");
                        }
                    }
                }

                Color healthyCrystal = BattleHUD.CrystalHealthColor(0.8f);
                Color warningCrystal = BattleHUD.CrystalHealthColor(0.5f);
                Color dangerCrystal = BattleHUD.CrystalHealthColor(0.2f);
                if (healthyCrystal == warningCrystal || warningCrystal == dangerCrystal || healthyCrystal == dangerCrystal)
                {
                    errors.Add($"{label} Crystal health colors must distinguish healthy, warning, and danger states.");
                }

                Rect waveAnnouncement = BattleHUD.CalculateWaveAnnouncementRect(battle.BattleFieldFrame);
                ValidateRectInside($"{label} Wave announcement", waveAnnouncement, battle.BattleFieldFrame, errors);
                if (waveAnnouncement.width < 280f || waveAnnouncement.height < 88f)
                {
                    errors.Add($"{label} Wave announcement is too small: {FormatRect(waveAnnouncement)}.");
                }

                if (waveAnnouncement.Overlaps(battle.HeaderArea) || waveAnnouncement.Overlaps(battle.SkillPanelArea))
                {
                    errors.Add($"{label} Wave announcement overlaps fixed battle HUD areas.");
                }

                if (battle.BattleFieldFrame.Overlaps(battle.SkillPanelArea))
                {
                    errors.Add($"{label} Battle field and skill panel overlap.");
                }

                if (battle.BattleFieldFrame.height < 140f)
                {
                    errors.Add($"{label} Battle field is too short: {battle.BattleFieldFrame.height:0.0}.");
                }

                if (battle.SkillPanelArea.height < 96f)
                {
                    errors.Add($"{label} Battle skill panel is too short: {battle.SkillPanelArea.height:0.0}.");
                }

                Rect skillContent = new Rect(0f, 0f, Mathf.Max(1f, battle.SkillPanelArea.width - 20f), Mathf.Max(1f, battle.SkillPanelArea.height - 44f));
                Rect[] skillCards = new Rect[ExpectedHeroCount];
                for (int cardIndex = 0; cardIndex < skillCards.Length; cardIndex++)
                {
                    skillCards[cardIndex] = FormationSkillPanelUI.CalculateCardRect(skillContent, cardIndex, skillCards.Length);
                    ValidateRectInside($"{label} Skill card {cardIndex + 1}", skillCards[cardIndex], skillContent, errors);
                    if (skillCards[cardIndex].width < 80f || skillCards[cardIndex].height < 40f)
                    {
                        errors.Add($"{label} Skill card {cardIndex + 1} is too small: {FormatRect(skillCards[cardIndex])}.");
                    }

                    for (int previousIndex = 0; previousIndex < cardIndex; previousIndex++)
                    {
                        if (skillCards[cardIndex].Overlaps(skillCards[previousIndex]))
                        {
                            errors.Add($"{label} Skill cards {previousIndex + 1} and {cardIndex + 1} overlap.");
                        }
                    }
                }

                if (size.y >= size.x && battle.SkillPanelArea.width >= 540f && FormationSkillPanelUI.ResolveColumnCount(skillContent.width, ExpectedHeroCount) != 3)
                {
                    errors.Add($"{label} portrait skill cards must use three columns when enough width is available.");
                }

                if (size.y >= size.x)
                {
                    float viewportAspect = battle.BattleFieldFrame.width / Mathf.Max(1f, battle.BattleFieldFrame.height);
                    float cameraWorldHeight = Mathf.Max(7.5f, 12.5f / Mathf.Max(0.01f, viewportAspect));
                    float presentationSpacing = LaneManager.ResolvePresentationLaneSpacing(2.15f, cameraWorldHeight, true);
                    float threeLaneSpread = presentationSpacing * 2f;
                    if (threeLaneSpread < cameraWorldHeight * 0.48f)
                    {
                        errors.Add($"{label} portrait lane spread is too narrow: {threeLaneSpread:0.00}/{cameraWorldHeight:0.00}.");
                    }
                }

                bool portraitPopup = size.y >= size.x;
                float runePreferredWidth = portraitPopup ? 760f : 620f;
                float runePreferredHeight = portraitPopup ? 760f : 520f;
                Rect runePopup = GameFrameLayout.PopupFrameForSize(size.x, size.y, runePreferredWidth, runePreferredHeight, 0.92f, 0.78f);
                Rect resultPopup = GameFrameLayout.PopupFrameForSize(size.x, size.y, 620f, 560f, 0.92f, 0.78f);
                ValidatePositiveRect($"{label} Rune popup", runePopup, errors);
                ValidatePositiveRect($"{label} Result popup", resultPopup, errors);
                ValidateRectInside($"{label} Rune popup", runePopup, safeRect, errors);
                ValidateRectInside($"{label} Result popup", resultPopup, safeRect, errors);

                if (runePopup.width < 300f || runePopup.height < 280f)
                {
                    errors.Add($"{label} Rune popup is too small: {FormatRect(runePopup)}.");
                }

                float runeTitleHeight = Mathf.Clamp(runePopup.height * 0.08f, 34f, 54f);
                float runeSubtitleHeight = Mathf.Clamp(runePopup.height * 0.06f, 28f, 42f);
                float runeContentTop = runePopup.y + 12f + runeTitleHeight + runeSubtitleHeight;
                Rect runeContent = new Rect(
                    runePopup.x + 14f,
                    runeContentTop,
                    runePopup.width - 28f,
                    Mathf.Max(1f, runePopup.yMax - runeContentTop - 14f));
                Rect[] runeCards = new Rect[3];
                for (int cardIndex = 0; cardIndex < runeCards.Length; cardIndex++)
                {
                    runeCards[cardIndex] = RuneSelectionUI.CalculateCardRect(runeContent, cardIndex, runeCards.Length);
                    ValidateRectInside($"{label} Rune card {cardIndex + 1}", runeCards[cardIndex], runeContent, errors);
                    if (runeCards[cardIndex].width < 120f || runeCards[cardIndex].height < 220f)
                    {
                        errors.Add($"{label} Rune card {cardIndex + 1} is too small: {FormatRect(runeCards[cardIndex])}.");
                    }

                    for (int previousIndex = 0; previousIndex < cardIndex; previousIndex++)
                    {
                        if (runeCards[cardIndex].Overlaps(runeCards[previousIndex]))
                        {
                            errors.Add($"{label} Rune cards {previousIndex + 1} and {cardIndex + 1} overlap.");
                        }
                    }
                }

                if (portraitPopup && runeContent.width >= 540f && RuneSelectionUI.ResolveColumnCount(runeContent.width, runeCards.Length) != 3)
                {
                    errors.Add($"{label} portrait rune choices must use three columns when enough width is available.");
                }

                if (resultPopup.width < 300f || resultPopup.height < 300f)
                {
                    errors.Add($"{label} Result popup is too small: {FormatRect(resultPopup)}.");
                }
            }

            float landscapeHeroHeight = RuntimeSpritePolicy.GetHeroTargetHeight(null, false);
            float portraitHeroHeight = RuntimeSpritePolicy.GetHeroTargetHeight(null, true);
            float landscapeMonsterHeight = RuntimeSpritePolicy.GetMonsterTargetHeight(null, false);
            float portraitMonsterHeight = RuntimeSpritePolicy.GetMonsterTargetHeight(null, true);
            if (portraitHeroHeight <= landscapeHeroHeight || portraitMonsterHeight <= landscapeMonsterHeight)
            {
                errors.Add("Portrait RuntimePixel presentation scale must improve hero and monster readability.");
            }

            Color commonRuneColor = RuneSelectionUI.RarityColor(RuneRarity.Common);
            Color rareRuneColor = RuneSelectionUI.RarityColor(RuneRarity.Rare);
            Color epicRuneColor = RuneSelectionUI.RarityColor(RuneRarity.Epic);
            if (commonRuneColor == rareRuneColor || rareRuneColor == epicRuneColor || commonRuneColor == epicRuneColor)
            {
                errors.Add("Rune card rarity colors must distinguish Common, Rare, and Epic choices.");
            }

            foreach (ElementType element in System.Enum.GetValues(typeof(ElementType)))
            {
                if (string.IsNullOrWhiteSpace(RuneSelectionUI.ElementGlyph(element)))
                {
                    errors.Add($"Rune card element glyph is missing for {element}.");
                }
            }

            string openingWaveTitle = BattleHUD.WaveAnnouncementTitle(1, 3, false);
            string finalWaveTitle = BattleHUD.WaveAnnouncementTitle(3, 3, false);
            string bossWaveTitle = BattleHUD.WaveAnnouncementTitle(3, 3, true);
            if (string.IsNullOrWhiteSpace(openingWaveTitle) || openingWaveTitle == finalWaveTitle || finalWaveTitle == bossWaveTitle)
            {
                errors.Add("Wave announcements must distinguish opening, final, and boss waves.");
            }

            string runeWaveSubtitle = BattleHUD.WaveAnnouncementSubtitle(2, 3, 8, "공격 룬");
            if (!runeWaveSubtitle.Contains("2/3") || !runeWaveSubtitle.Contains("8") || !runeWaveSubtitle.Contains("공격 룬"))
            {
                errors.Add("Wave announcement subtitle must include progress, enemy count, and applied rune name.");
            }

            Color normalWaveColor = BattleHUD.WaveAnnouncementAccent(false, false);
            Color finalWaveColor = BattleHUD.WaveAnnouncementAccent(false, true);
            Color bossWaveColor = BattleHUD.WaveAnnouncementAccent(true, true);
            if (normalWaveColor == finalWaveColor || finalWaveColor == bossWaveColor || normalWaveColor == bossWaveColor)
            {
                errors.Add("Wave announcement colors must distinguish normal, final, and boss waves.");
            }
        }

        private static void ValidateStageSessionResolution(RuntimeContentCatalog catalog, List<string> errors)
        {
            if (catalog == null || catalog.Stages == null || catalog.Stages.Count < ExpectedStageCount)
            {
                return;
            }

            StageData firstStage = catalog.Stages[0];
            StageData secondStage = catalog.Stages[1];
            StageData finalStage = catalog.Stages[ExpectedStageCount - 1];
            if (firstStage == null || secondStage == null || finalStage == null)
            {
                return;
            }

            string stageTwoId = GameSession.ResolveNextStageId(firstStage.StageId, catalog.Stages);
            if (stageTwoId != secondStage.StageId)
            {
                errors.Add($"GameSession next-stage resolution failed for Stage 1. Expected {secondStage.StageId}, found {stageTwoId}.");
            }

            string afterFinalStageId = GameSession.ResolveNextStageId(finalStage.StageId, catalog.Stages);
            if (!string.IsNullOrWhiteSpace(afterFinalStageId))
            {
                errors.Add($"GameSession should not resolve a next stage after Stage {ExpectedStageCount}. Found {afterFinalStageId}.");
            }
        }

        private static void ValidateBattleResultProgression(RuntimeContentCatalog catalog, List<string> errors)
        {
            if (catalog == null || catalog.Stages == null || catalog.Stages.Count < 2 || catalog.Stages[0] == null || catalog.Stages[1] == null)
            {
                return;
            }

            StageData firstStage = catalog.Stages[0];
            StageData secondStage = catalog.Stages[1];
            string battleRunId = "smoke_test_stage_1_victory";
            SaveData saveData = SaveManager.CreateDefaultSave();
            int initialGold = saveData.totalGold;
            bool applied = SaveManager.TryApplyBattleResultProgression(saveData, battleRunId, 110, true, firstStage.StageId, secondStage.StageId);
            if (!applied)
            {
                errors.Add("Battle result progression smoke test could not apply Stage 1 victory.");
                return;
            }

            if (!saveData.clearedStageIds.Contains(firstStage.StageId))
            {
                errors.Add("Stage 1 victory must mark Stage 1 as cleared.");
            }

            if (!saveData.unlockedStageIds.Contains(firstStage.StageId))
            {
                errors.Add("Stage 1 victory must keep Stage 1 unlocked.");
            }

            if (!saveData.unlockedStageIds.Contains(secondStage.StageId))
            {
                errors.Add("Stage 1 victory must unlock Stage 2.");
            }

            if (saveData.totalGold != initialGold + 110)
            {
                errors.Add($"Stage 1 victory must award exactly 110 smoke-test gold. Expected {initialGold + 110}, found {saveData.totalGold}.");
            }

            if (saveData.lastProcessedBattleRunId != battleRunId)
            {
                errors.Add("Stage 1 victory must record the processed battle run id.");
            }

            bool duplicateApplied = SaveManager.TryApplyBattleResultProgression(saveData, battleRunId, 110, true, firstStage.StageId, secondStage.StageId);
            if (duplicateApplied)
            {
                errors.Add("Duplicate battle run id must not apply progression twice.");
            }

            if (saveData.totalGold != initialGold + 110)
            {
                errors.Add("Duplicate battle run id must not award duplicate gold.");
            }
        }

        private static void ValidateFullStageUnlockProgression(RuntimeContentCatalog catalog, List<string> errors)
        {
            if (catalog == null || catalog.Stages == null || catalog.Stages.Count < ExpectedStageCount)
            {
                return;
            }

            SaveData saveData = SaveManager.CreateDefaultSave();
            int expectedGold = saveData.totalGold;
            for (int i = 0; i < ExpectedStageCount; i++)
            {
                StageData stage = catalog.Stages[i];
                if (stage == null)
                {
                    return;
                }

                if (!saveData.unlockedStageIds.Contains(stage.StageId))
                {
                    errors.Add($"Stage {i + 1} must be unlocked before its simulated clear. Missing {stage.StageId}.");
                    return;
                }

                string nextStageId = GameSession.ResolveNextStageId(stage.StageId, catalog.Stages);
                int goldAward = 100 + i;
                expectedGold += goldAward;
                string battleRunId = $"smoke_full_progression_stage_{i + 1}";
                bool applied = SaveManager.TryApplyBattleResultProgression(saveData, battleRunId, goldAward, true, stage.StageId, nextStageId);
                if (!applied)
                {
                    errors.Add($"Could not apply simulated Stage {i + 1} victory.");
                    return;
                }

                if (!saveData.clearedStageIds.Contains(stage.StageId))
                {
                    errors.Add($"Simulated Stage {i + 1} victory did not mark {stage.StageId} cleared.");
                }

                if (!saveData.unlockedStageIds.Contains(stage.StageId))
                {
                    errors.Add($"Simulated Stage {i + 1} victory did not keep {stage.StageId} unlocked.");
                }

                if (!string.IsNullOrWhiteSpace(nextStageId) && !saveData.unlockedStageIds.Contains(nextStageId))
                {
                    errors.Add($"Simulated Stage {i + 1} victory did not unlock next stage {nextStageId}.");
                }

                if (saveData.totalGold != expectedGold)
                {
                    errors.Add($"Simulated Stage {i + 1} victory gold mismatch. Expected {expectedGold}, found {saveData.totalGold}.");
                }
            }

            StageData finalStage = catalog.Stages[ExpectedStageCount - 1];
            string afterFinalStageId = finalStage != null ? GameSession.ResolveNextStageId(finalStage.StageId, catalog.Stages) : string.Empty;
            if (!string.IsNullOrWhiteSpace(afterFinalStageId))
            {
                errors.Add($"Full progression simulation should end at Stage {ExpectedStageCount}, but resolved next stage {afterFinalStageId}.");
            }

            for (int i = 0; i < ExpectedStageCount; i++)
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
            if (catalog == null || catalog.Upgrades == null || catalog.Upgrades.Count == 0)
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

            bool purchased = SaveManager.TryPurchaseUpgrade(saveData, firstUpgrade.UpgradeId, cost, firstUpgrade.MaxLevel);
            if (!purchased)
            {
                errors.Add($"{firstUpgrade.name} could not be purchased after Stage 1 reward.");
                return;
            }

            if (SaveManager.GetUpgradeLevel(saveData, firstUpgrade.UpgradeId) != 1)
            {
                errors.Add($"{firstUpgrade.name} purchase did not increase level to 1.");
            }

            if (saveData.totalGold != 110 - cost)
            {
                errors.Add($"{firstUpgrade.name} purchase left invalid gold. Expected {110 - cost}, found {saveData.totalGold}.");
            }

            SaveData poorSave = SaveManager.CreateDefaultSave();
            poorSave.totalGold = Mathf.Max(0, cost - 1);
            if (SaveManager.TryPurchaseUpgrade(poorSave, firstUpgrade.UpgradeId, cost, firstUpgrade.MaxLevel))
            {
                errors.Add($"{firstUpgrade.name} purchase succeeded without enough gold.");
            }
        }

        private static void ValidateBossPatternRules(List<string> errors)
        {
            int phaseOneDamage = BossAttackPatternController.BaseDamageForPhase(1);
            int phaseTwoDamage = BossAttackPatternController.BaseDamageForPhase(2);
            int phaseThreeDamage = BossAttackPatternController.BaseDamageForPhase(3);
            if (!(phaseOneDamage > 0 && phaseOneDamage < phaseTwoDamage && phaseTwoDamage < phaseThreeDamage))
            {
                errors.Add($"Boss pattern hero damage progression is invalid: {phaseOneDamage}/{phaseTwoDamage}/{phaseThreeDamage}.");
            }

            if (BossAttackPatternController.BaseCrystalDamageForPhase(1) != 0 ||
                BossAttackPatternController.BaseCrystalDamageForPhase(2) != 0 ||
                BossAttackPatternController.BaseCrystalDamageForPhase(3) <= 0)
            {
                errors.Add("Boss pattern crystal pressure must activate only in phase 3.");
            }
        }

        private static void ValidateAudioRules(List<string> errors)
        {
            float volume = 0.25f;
            float[] expectedSteps = { 0.5f, 0.75f, 1f, 0.25f };
            for (int i = 0; i < expectedSteps.Length; i++)
            {
                volume = AudioManager.NextVolumeStep(volume);
                if (!Mathf.Approximately(volume, expectedSteps[i]))
                {
                    errors.Add($"Audio volume step {i + 1} was {volume}, expected {expectedSteps[i]}.");
                    break;
                }
            }

            if (!ProceduralBgmFactory.IsAvailable)
            {
                errors.Add("Procedural BGM fallback is unavailable.");
            }
        }

        private static void ValidateDifficultyRules(List<string> errors)
        {
            int easyHp = DifficultyRules.ApplyMonsterHp(100, DifficultyRules.Easy);
            int normalHp = DifficultyRules.ApplyMonsterHp(100, DifficultyRules.Normal);
            int hardHp = DifficultyRules.ApplyMonsterHp(100, DifficultyRules.Hard);
            int nightmareHp = DifficultyRules.ApplyMonsterHp(100, DifficultyRules.Nightmare);
            if (!(easyHp < normalHp && normalHp < hardHp && hardHp < nightmareHp))
            {
                errors.Add($"DifficultyRules HP scaling must increase from easy to nightmare. easy={easyHp}, normal={normalHp}, hard={hardHp}, nightmare={nightmareHp}.");
            }

            int easyCrystal = DifficultyRules.ApplyCrystalMaxHp(100, DifficultyRules.Easy);
            int normalCrystal = DifficultyRules.ApplyCrystalMaxHp(100, DifficultyRules.Normal);
            int nightmareCrystal = DifficultyRules.ApplyCrystalMaxHp(100, DifficultyRules.Nightmare);
            if (!(easyCrystal > normalCrystal && normalCrystal > nightmareCrystal))
            {
                errors.Add($"DifficultyRules crystal HP scaling must decrease from easy to nightmare. easy={easyCrystal}, normal={normalCrystal}, nightmare={nightmareCrystal}.");
            }

            int normalReward = DifficultyRules.ApplyMonsterRewardGold(100, DifficultyRules.Normal);
            int hardReward = DifficultyRules.ApplyMonsterRewardGold(100, DifficultyRules.Hard);
            int nightmareReward = DifficultyRules.ApplyMonsterRewardGold(100, DifficultyRules.Nightmare);
            if (!(normalReward < hardReward && hardReward < nightmareReward))
            {
                errors.Add($"DifficultyRules reward scaling must increase on hard/nightmare. normal={normalReward}, hard={hardReward}, nightmare={nightmareReward}.");
            }

            if (DifficultyRules.UndeadRevives(DifficultyRules.Easy) ||
                DifficultyRules.UndeadRevives(DifficultyRules.Normal) ||
                !DifficultyRules.UndeadRevives(DifficultyRules.Hard) ||
                !DifficultyRules.UndeadRevives(DifficultyRules.Nightmare))
            {
                errors.Add("Undead revival must be disabled on Easy/Normal and enabled on Hard/Nightmare.");
            }


            SaveData progressionSave = SaveManager.CreateDefaultSave();
            progressionSave.unlockedStageIds.Add(DifficultyRules.ChapterOneFinalStageId);
            bool normalClearApplied = SaveManager.TryApplyBattleResultProgression(
                progressionSave,
                "difficulty_normal_final",
                normalReward,
                true,
                DifficultyRules.ChapterOneFinalStageId,
                string.Empty,
                DifficultyRules.Normal);
            if (!normalClearApplied || !DifficultyRules.IsCompleted(progressionSave, DifficultyRules.Normal) ||
                !DifficultyRules.IsUnlocked(progressionSave, DifficultyRules.Hard) ||
                DifficultyRules.IsUnlocked(progressionSave, DifficultyRules.Nightmare))
            {
                errors.Add("Normal Chapter 1 clear must unlock Hard without unlocking Nightmare.");
            }

            bool hardClearApplied = true;
            for (int stageNumber = 1; stageNumber <= 10; stageNumber++)
            {
                string stageId = $"stage_goblin_forest_{stageNumber:D2}";
                string nextStageId = stageNumber < 10 ? $"stage_goblin_forest_{stageNumber + 1:D2}" : string.Empty;
                if (!SaveManager.IsStageUnlocked(progressionSave, stageId, DifficultyRules.Hard))
                {
                    errors.Add($"Hard Stage {stageNumber} must unlock after the previous Hard stage clear.");
                    hardClearApplied = false;
                    break;
                }

                hardClearApplied &= SaveManager.TryApplyBattleResultProgression(
                    progressionSave,
                    $"difficulty_hard_stage_{stageNumber}",
                    hardReward,
                    true,
                    stageId,
                    nextStageId,
                    DifficultyRules.Hard);
            }

            if (!hardClearApplied || !DifficultyRules.IsCompleted(progressionSave, DifficultyRules.Hard) ||
                !DifficultyRules.IsUnlocked(progressionSave, DifficultyRules.Nightmare))
            {
                errors.Add("Hard Chapter 1 clear must unlock Nightmare.");
            }

            if (DifficultyRules.NextSelectableDifficultyId(SaveManager.CreateDefaultSave(), DifficultyRules.Normal) != DifficultyRules.Easy)
            {
                errors.Add("Locked difficulties must be skipped when cycling a default save.");
            }
        }

        private static void ValidatePositiveRect(string label, Rect rect, List<string> errors)
        {
            if (rect.width <= 0f || rect.height <= 0f)
            {
                errors.Add($"{label} has invalid size {rect.width:0.0}x{rect.height:0.0}.");
            }
        }

        private static void ValidateRectInside(string label, Rect rect, Rect bounds, List<string> errors)
        {
            const float tolerance = 0.1f;
            if (rect.width < 0f || rect.height < 0f)
            {
                errors.Add($"{label} has a negative size.");
                return;
            }

            if (rect.xMin < bounds.xMin - tolerance || rect.yMin < bounds.yMin - tolerance || rect.xMax > bounds.xMax + tolerance || rect.yMax > bounds.yMax + tolerance)
            {
                errors.Add($"{label} is outside its root. Rect {FormatRect(rect)}, Root {FormatRect(bounds)}.");
            }
        }

        private static string FormatRect(Rect rect)
        {
            return $"({rect.x:0.0},{rect.y:0.0},{rect.width:0.0},{rect.height:0.0})";
        }

        private struct StageCombatEstimate
        {
            public int MonsterCount;
            public int TotalMonsterHp;
            public int TotalRewardGold;
            public int TotalCrystalDamage;
            public int LaneMask;
            public bool HasBossSpawn;

            public int UsedLaneCount
            {
                get
                {
                    int count = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        if ((LaneMask & (1 << i)) != 0)
                        {
                            count++;
                        }
                    }

                    return count;
                }
            }
        }

        private static int CountNonNull<T>(IReadOnlyList<T> assets) where T : Object
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

        private static void LogResult(bool passed, List<string> errors, List<string> warnings)
        {
            for (int i = 0; i < warnings.Count; i++)
            {
                Debug.LogWarning($"RuneGate smoke test warning: {warnings[i]}");
            }

            if (passed)
            {
                Debug.Log("RuneGate progression smoke test passed.");
                return;
            }

            Debug.LogError("RuneGate progression smoke test failed:\n" + string.Join("\n", errors));
        }
    }
}
