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
            "turret_attack_percent"
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
            ValidateFormation(catalog, errors, warnings);
            ValidateDefaultSave(catalog, errors);
            ValidateStageSessionResolution(catalog, errors);
            ValidateBattleResultProgression(catalog, errors);
            ValidateFullStageUnlockProgression(catalog, errors);
            ValidateStagePlayabilityEstimates(catalog, errors, warnings);
            ValidateUpgradePurchaseAfterStageOne(catalog, errors);
            ValidateResponsiveLayouts(errors);
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

                if (!string.IsNullOrWhiteSpace(rune.EffectKey))
                {
                    effectKeys.Add(rune.EffectKey);
                }
            }

            for (int i = 0; i < RequiredRuntimeRuneEffects.Length; i++)
            {
                if (!effectKeys.Contains(RequiredRuntimeRuneEffects[i]))
                {
                    errors.Add($"Missing runtime rune effect key: {RequiredRuntimeRuneEffects[i]}");
                }
            }

            if (effectKeys.Count > RequiredRuntimeRuneEffects.Length)
            {
                warnings.Add($"Rune catalog contains {effectKeys.Count} effect keys; unsupported keys should remain compile-safe placeholders.");
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

                BattleFrameRects battle = GameFrameLayout.BattleFrameForSize(size.x, size.y);
                Rect safeRect = GameFrameLayout.SafeRectForSize(size.x, size.y);
                ValidatePositiveRect($"{label} Battle root", battle.FrameRoot, errors);
                ValidateRectInside($"{label} Battle header", battle.HeaderArea, battle.FrameRoot, errors);
                ValidateRectInside($"{label} Battle field", battle.BattleFieldFrame, battle.FrameRoot, errors);
                ValidateRectInside($"{label} Battle skill panel", battle.SkillPanelArea, battle.FrameRoot, errors);
                ValidateRectInside($"{label} Battle footer", battle.FooterArea, battle.FrameRoot, errors);
                ValidateRectInside($"{label} Battle popup layer", battle.PopupLayer, safeRect, errors);

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

                Rect runePopup = GameFrameLayout.PopupFrameForSize(size.x, size.y, 620f, 520f, 0.92f, 0.78f);
                Rect resultPopup = GameFrameLayout.PopupFrameForSize(size.x, size.y, 620f, 560f, 0.92f, 0.78f);
                ValidatePositiveRect($"{label} Rune popup", runePopup, errors);
                ValidatePositiveRect($"{label} Result popup", resultPopup, errors);
                ValidateRectInside($"{label} Rune popup", runePopup, safeRect, errors);
                ValidateRectInside($"{label} Result popup", resultPopup, safeRect, errors);

                if (runePopup.width < 300f || runePopup.height < 280f)
                {
                    errors.Add($"{label} Rune popup is too small: {FormatRect(runePopup)}.");
                }

                if (resultPopup.width < 300f || resultPopup.height < 300f)
                {
                    errors.Add($"{label} Result popup is too small: {FormatRect(resultPopup)}.");
                }
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
