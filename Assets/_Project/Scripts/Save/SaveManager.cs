using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RuneGate
{
    public static class SaveManager
    {
        public const string DefaultUnlockedStageId = "stage_goblin_forest_01";

        private const int CurrentSaveVersion = 5;
        private const string SaveFileName = "runegate_save.json";
        private const string SavePathArgument = "-runegateSavePath";
        private const string TemporarySaveExtension = ".tmp";
        private const string BackupSaveExtension = ".bak";
        private const string CorruptSaveExtension = ".corrupt";
        private const string CorruptTemporarySaveExtension = ".tmp.corrupt";

        private static SaveData currentSave;

        public static SaveData Current
        {
            get
            {
                LoadOrCreate();
                return currentSave;
            }
        }

        public static string SavePath => ResolveSavePath();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadOnStartup()
        {
            LoadOrCreate();
        }

        public static bool HasSaveFile()
        {
            return File.Exists(SavePath);
        }

        public static bool HasSave()
        {
            return HasSaveFile();
        }

        public static SaveData Load()
        {
            return LoadOrCreate();
        }

        public static SaveData LoadOrCreate()
        {
            if (currentSave != null)
            {
                return currentSave;
            }

            currentSave = TryLoadFromDisk();
            bool createdDefaultSave = currentSave == null;
            if (currentSave == null)
            {
                currentSave = CreateDefaultSave();
            }

            Sanitize(currentSave);
            if (createdDefaultSave)
            {
                Save();
            }

            return currentSave;
        }

        internal static SaveData ReloadFromDiskForDiagnostics()
        {
            currentSave = null;
            return LoadOrCreate();
        }

        public static SaveData CreateDefaultSave()
        {
            SaveData saveData = new SaveData();
            saveData.saveVersion = CurrentSaveVersion;
            AddUnique(saveData.unlockedStageIds, DefaultUnlockedStageId);
            saveData.formationSlots = CreateDefaultFormationSlots();
            return saveData;
        }

        public static void Save()
        {
            LoadOrCreate();

            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);
                string json = ToJson(currentSave);
                WriteSaveAtomically(SavePath, json);
            }
            catch (Exception exception)
            {
                Debug.LogError($"SaveManager failed to save data: {exception.Message}");
            }
        }

        public static void ResetSave()
        {
            currentSave = CreateDefaultSave();
            Save();
        }

        public static void BackupAndResetSave(string reason)
        {
            try
            {
                if (HasSaveFile())
                {
                    string backupPath = SavePath + CorruptSaveExtension;
                    File.Copy(SavePath, backupPath, true);
                    Debug.LogWarning($"SaveManager backed up damaged save to {backupPath}. {reason}");
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"SaveManager failed to backup damaged save: {exception.Message}");
            }

            ResetSave();
        }

        public static void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Current.totalGold = Mathf.Max(0, Current.totalGold + amount);
            Save();
        }

        public static bool TrySpendGold(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("SaveManager cannot spend negative gold.");
                return false;
            }

            if (Current.totalGold < amount)
            {
                return false;
            }

            Current.totalGold -= amount;
            Save();
            return true;
        }

        public static bool SpendGold(int amount)
        {
            return TrySpendGold(amount);
        }

        public static bool TryPurchaseUpgrade(string upgradeId, int cost, int maxLevel)
        {
            LoadOrCreate();
            bool purchased = TryPurchaseUpgrade(currentSave, upgradeId, cost, maxLevel);
            if (purchased)
            {
                Save();
            }

            return purchased;
        }

        public static bool TryPurchaseUpgrade(SaveData saveData, string upgradeId, int cost, int maxLevel)
        {
            if (saveData == null)
            {
                Debug.LogWarning("SaveManager cannot purchase an upgrade with null save data.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(upgradeId))
            {
                Debug.LogWarning("SaveManager cannot purchase an upgrade with an empty id.");
                return false;
            }

            if (cost < 0)
            {
                Debug.LogWarning("SaveManager cannot purchase an upgrade with a negative cost.");
                return false;
            }

            Sanitize(saveData);
            int currentLevel = GetUpgradeLevel(saveData, upgradeId);
            if (currentLevel >= Mathf.Max(0, maxLevel) || saveData.totalGold < cost)
            {
                return false;
            }

            saveData.totalGold = Mathf.Max(0, saveData.totalGold - cost);
            SetUpgradeLevelInMemory(saveData, upgradeId, currentLevel + 1);
            return true;
        }

        public static void MarkStageCleared(string stageId)
        {
            if (string.IsNullOrWhiteSpace(stageId))
            {
                Debug.LogWarning("SaveManager cannot clear a stage with an empty id.");
                return;
            }

            AddUnique(Current.clearedStageIds, stageId);
            UnlockStage(stageId);
            Save();
        }

        public static void UnlockStage(string stageId)
        {
            if (string.IsNullOrWhiteSpace(stageId))
            {
                return;
            }

            AddUnique(Current.unlockedStageIds, stageId);
            Save();
        }

        public static bool IsStageCleared(string stageId)
        {
            return !string.IsNullOrWhiteSpace(stageId) && Current.clearedStageIds.Contains(stageId);
        }

        public static bool IsStageCleared(string stageId, string difficultyId)
        {
            return IsStageCleared(Current, stageId, difficultyId);
        }

        public static bool IsStageUnlocked(string stageId)
        {
            return !string.IsNullOrWhiteSpace(stageId) && Current.unlockedStageIds.Contains(stageId);
        }

        public static bool IsStageUnlocked(string stageId, string difficultyId)
        {
            return IsStageUnlocked(Current, stageId, difficultyId);
        }

        public static void SetLastSelectedStageId(string stageId)
        {
            Current.lastSelectedStageId = stageId ?? string.Empty;
            Save();
        }

        public static bool HasProcessedBattleRun(string battleRunId)
        {
            return !string.IsNullOrWhiteSpace(battleRunId) && Current.lastProcessedBattleRunId == battleRunId;
        }

        public static void MarkBattleRunProcessed(string battleRunId)
        {
            if (string.IsNullOrWhiteSpace(battleRunId))
            {
                return;
            }

            Current.lastProcessedBattleRunId = battleRunId;
            Save();
        }

        public static bool TryApplyBattleResultProgression(string battleRunId, int goldAward, bool victory, string clearedStageId, string nextStageId)
        {
            LoadOrCreate();
            bool applied = TryApplyBattleResultProgression(currentSave, battleRunId, goldAward, victory, clearedStageId, nextStageId);
            if (applied)
            {
                Save();
            }

            return applied;
        }

        public static bool TryApplyBattleResultProgression(SaveData saveData, string battleRunId, int goldAward, bool victory, string clearedStageId, string nextStageId)
        {
            string difficultyId = saveData != null ? saveData.selectedDifficultyId : DifficultyRules.Normal;
            return TryApplyBattleResultProgression(saveData, battleRunId, goldAward, victory, clearedStageId, nextStageId, difficultyId);
        }

        public static bool TryApplyBattleResultProgression(SaveData saveData, string battleRunId, int goldAward, bool victory, string clearedStageId, string nextStageId, string difficultyId)
        {
            if (saveData == null)
            {
                Debug.LogWarning("SaveManager cannot apply battle result to a null save data.");
                return false;
            }

            Sanitize(saveData);
            if (!string.IsNullOrWhiteSpace(battleRunId) && saveData.lastProcessedBattleRunId == battleRunId)
            {
                return false;
            }

            if (goldAward > 0)
            {
                saveData.totalGold = Mathf.Max(0, saveData.totalGold + goldAward);
            }

            if (victory && !string.IsNullOrWhiteSpace(clearedStageId))
            {
                string normalizedDifficulty = DifficultyRules.Normalize(difficultyId);
                bool difficultyStageWasUnlocked = IsStageUnlocked(saveData, clearedStageId, normalizedDifficulty);
                AddUnique(saveData.clearedStageIds, clearedStageId);
                AddUnique(saveData.unlockedStageIds, clearedStageId);
                if (!string.IsNullOrWhiteSpace(nextStageId))
                {
                    AddUnique(saveData.unlockedStageIds, nextStageId);
                }

                if (difficultyStageWasUnlocked)
                {
                    AddUnique(saveData.clearedDifficultyStageKeys, BuildDifficultyStageKey(normalizedDifficulty, clearedStageId));
                }

                if (difficultyStageWasUnlocked && clearedStageId == DifficultyRules.ChapterOneFinalStageId)
                {
                    if (DifficultyRules.IsUnlocked(saveData, normalizedDifficulty))
                    {
                        AddUnique(saveData.clearedDifficultyIds, normalizedDifficulty);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(battleRunId))
            {
                saveData.lastProcessedBattleRunId = battleRunId;
            }

            return true;
        }

        public static void SetSelectedDifficultyId(string difficultyId)
        {
            string normalized = NormalizeDifficultyId(difficultyId);
            if (!DifficultyRules.IsUnlocked(Current, normalized))
            {
                Debug.LogWarning($"SaveManager ignored locked difficulty: {normalized}.");
                normalized = DifficultyRules.Normal;
            }

            Current.selectedDifficultyId = normalized;
            Save();
        }

        public static bool IsDifficultyUnlocked(string difficultyId)
        {
            return DifficultyRules.IsUnlocked(Current, difficultyId);
        }

        public static bool IsDifficultyCompleted(string difficultyId)
        {
            return DifficultyRules.IsCompleted(Current, difficultyId);
        }

        public static bool HasSeenTutorial()
        {
            return Current.hasSeenTutorial;
        }

        public static void MarkTutorialSeen()
        {
            Current.hasSeenTutorial = true;
            Current.hasSeenIntro = true;
            Save();
        }

        public static void ResetTutorialSeen()
        {
            Current.hasSeenTutorial = false;
            Current.hasSeenIntro = false;
            Save();
        }

        public static int GetMonsterShardCount(string monsterId)
        {
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                return 0;
            }

            List<SerializableMonsterShardCount> counts = Current.monsterShardCounts;
            for (int i = 0; i < counts.Count; i++)
            {
                SerializableMonsterShardCount entry = counts[i];
                if (entry != null && entry.monsterId == monsterId)
                {
                    return Mathf.Max(0, entry.count);
                }
            }

            return 0;
        }

        public static void AddMonsterShards(string monsterId, int amount)
        {
            if (string.IsNullOrWhiteSpace(monsterId) || amount <= 0)
            {
                return;
            }

            List<SerializableMonsterShardCount> counts = Current.monsterShardCounts;
            for (int i = 0; i < counts.Count; i++)
            {
                SerializableMonsterShardCount entry = counts[i];
                if (entry != null && entry.monsterId == monsterId)
                {
                    entry.count = Mathf.Max(0, entry.count + amount);
                    Save();
                    return;
                }
            }

            counts.Add(new SerializableMonsterShardCount(monsterId, amount));
            Save();
        }

        public static bool HasContractedPet(string monsterId)
        {
            return !string.IsNullOrWhiteSpace(monsterId) && Current.contractedPetIds.Contains(monsterId);
        }

        public static bool TryContractPet(string monsterId, int requiredShards)
        {
            if (string.IsNullOrWhiteSpace(monsterId) || requiredShards <= 0)
            {
                return false;
            }

            if (HasContractedPet(monsterId) || GetMonsterShardCount(monsterId) < requiredShards)
            {
                return false;
            }

            AddUnique(Current.contractedPetIds, monsterId);
            if (string.IsNullOrWhiteSpace(Current.equippedPetId))
            {
                Current.equippedPetId = monsterId;
            }

            Save();
            return true;
        }

        public static void EquipPet(string monsterId)
        {
            if (string.IsNullOrWhiteSpace(monsterId) || !HasContractedPet(monsterId))
            {
                return;
            }

            Current.equippedPetId = monsterId;
            Save();
        }

        public static void UnequipPet()
        {
            Current.equippedPetId = string.Empty;
            Save();
        }

        public static bool HasSeenPetTutorial()
        {
            return Current.hasSeenPetTutorial;
        }

        public static void MarkPetTutorialSeen()
        {
            Current.hasSeenPetTutorial = true;
            Save();
        }

        public static int GetUpgradeLevel(string upgradeId)
        {
            return GetUpgradeLevel(Current, upgradeId);
        }

        public static int GetUpgradeLevel(SaveData saveData, string upgradeId)
        {
            if (string.IsNullOrWhiteSpace(upgradeId))
            {
                return 0;
            }

            if (saveData == null)
            {
                return 0;
            }

            if (saveData.upgradeLevels == null)
            {
                saveData.upgradeLevels = new List<SerializableUpgradeLevel>();
            }

            List<SerializableUpgradeLevel> levels = saveData.upgradeLevels;
            for (int i = 0; i < levels.Count; i++)
            {
                SerializableUpgradeLevel entry = levels[i];
                if (entry != null && entry.upgradeId == upgradeId)
                {
                    return Mathf.Max(0, entry.level);
                }
            }

            return 0;
        }

        public static void SetUpgradeLevel(string upgradeId, int level)
        {
            if (string.IsNullOrWhiteSpace(upgradeId))
            {
                Debug.LogWarning("SaveManager cannot set an upgrade level with an empty id.");
                return;
            }

            List<SerializableUpgradeLevel> levels = Current.upgradeLevels;
            for (int i = 0; i < levels.Count; i++)
            {
                SerializableUpgradeLevel entry = levels[i];
                if (entry != null && entry.upgradeId == upgradeId)
                {
                    entry.level = Mathf.Max(0, level);
                    Save();
                    return;
                }
            }

            levels.Add(new SerializableUpgradeLevel(upgradeId, Mathf.Max(0, level)));
            Save();
        }

        private static void SetUpgradeLevelInMemory(string upgradeId, int level)
        {
            SetUpgradeLevelInMemory(Current, upgradeId, level);
        }

        private static void SetUpgradeLevelInMemory(SaveData saveData, string upgradeId, int level)
        {
            if (saveData == null)
            {
                return;
            }

            if (saveData.upgradeLevels == null)
            {
                saveData.upgradeLevels = new List<SerializableUpgradeLevel>();
            }

            List<SerializableUpgradeLevel> levels = saveData.upgradeLevels;
            for (int i = 0; i < levels.Count; i++)
            {
                SerializableUpgradeLevel entry = levels[i];
                if (entry != null && entry.upgradeId == upgradeId)
                {
                    entry.level = Mathf.Max(0, level);
                    return;
                }
            }

            levels.Add(new SerializableUpgradeLevel(upgradeId, Mathf.Max(0, level)));
        }

        public static void ClampUpgradeLevels(IReadOnlyList<UpgradeData> upgrades)
        {
            if (upgrades == null)
            {
                return;
            }

            Dictionary<string, int> maxLevels = new Dictionary<string, int>();
            for (int i = 0; i < upgrades.Count; i++)
            {
                UpgradeData upgrade = upgrades[i];
                if (upgrade != null && !string.IsNullOrWhiteSpace(upgrade.UpgradeId))
                {
                    maxLevels[upgrade.UpgradeId] = Mathf.Max(0, upgrade.MaxLevel);
                }
            }

            bool changed = false;
            List<SerializableUpgradeLevel> levels = Current.upgradeLevels;
            for (int i = levels.Count - 1; i >= 0; i--)
            {
                SerializableUpgradeLevel entry = levels[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.upgradeId))
                {
                    levels.RemoveAt(i);
                    changed = true;
                    continue;
                }

                if (maxLevels.TryGetValue(entry.upgradeId, out int maxLevel))
                {
                    int clampedLevel = Mathf.Clamp(entry.level, 0, maxLevel);
                    if (entry.level != clampedLevel)
                    {
                        entry.level = clampedLevel;
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                Save();
            }
        }

        public static List<FormationSlot> CreateDefaultFormationSlots()
        {
            FormationData defaultFormation = PrototypeAssetLoader.LoadDefaultFormation();
            if (defaultFormation != null)
            {
                List<FormationSlot> catalogSlots = CopyFormationSlots(defaultFormation.Slots);
                if (catalogSlots.Count > 0)
                {
                    return catalogSlots;
                }

                Debug.LogWarning("SaveManager found DefaultFormation, but it has no valid slots. Falling back to built-in formation.");
            }

            return CreateFallbackFormationSlots();
        }

        private static List<FormationSlot> CreateFallbackFormationSlots()
        {
            return new List<FormationSlot>
            {
                new FormationSlot(0, HeroPositionType.Front, "hero_knight_001"),
                new FormationSlot(0, HeroPositionType.Back, "hero_archer_001"),
                new FormationSlot(1, HeroPositionType.Middle, "hero_priest_001"),
                new FormationSlot(1, HeroPositionType.Back, "hero_mage_fire_001"),
                new FormationSlot(2, HeroPositionType.Middle, "hero_engineer_dwarf_001"),
                new FormationSlot(2, HeroPositionType.Front, "hero_assassin_001")
            };
        }

        public static List<FormationSlot> GetFormationSlots()
        {
            List<FormationSlot> copy = new List<FormationSlot>();
            List<FormationSlot> slots = Current.formationSlots;
            if (slots == null)
            {
                return copy;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                FormationSlot slot = slots[i];
                if (slot != null)
                {
                    copy.Add(new FormationSlot(slot.LaneIndex, slot.PositionType, slot.HeroId));
                }
            }

            return copy;
        }

        public static void SetFormationSlots(IReadOnlyList<FormationSlot> slots)
        {
            Current.formationSlots = CopyFormationSlots(slots);
            Save();
        }

        private static SaveData TryLoadFromDisk()
        {
            if (!HasSaveFile())
            {
                SaveData temporarySave = TryPromoteTemporarySave();
                if (temporarySave != null)
                {
                    return temporarySave;
                }

                SaveData backupSave = TryLoadBackupFromDisk();
                RestorePrimarySaveFromBackup(backupSave);
                return backupSave;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                if (!IsStructurallyValidSaveJson(json, out string validationError))
                {
                    return RecoverFromInvalidPrimarySave(validationError);
                }

                return FromJson(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"SaveManager failed to load save data. A default save will be used. {exception.Message}");
                return RecoverFromInvalidPrimarySave(exception.Message);
            }
        }

        private static SaveData RecoverFromInvalidPrimarySave(string reason)
        {
            BackupSaveFile(reason);
            try
            {
                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"SaveManager could not remove invalid primary save: {exception.Message}");
            }

            SaveData temporarySave = TryPromoteTemporarySave();
            if (temporarySave != null)
            {
                return temporarySave;
            }

            SaveData backupSave = TryLoadBackupFromDisk();
            RestorePrimarySaveFromBackup(backupSave);
            return backupSave;
        }

        private static SaveData TryPromoteTemporarySave()
        {
            string temporaryPath = SavePath + TemporarySaveExtension;
            if (!File.Exists(temporaryPath))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(temporaryPath);
                if (!IsStructurallyValidSaveJson(json, out string validationError))
                {
                    string corruptTemporaryPath = SavePath + CorruptTemporarySaveExtension;
                    File.Copy(temporaryPath, corruptTemporaryPath, true);
                    File.Delete(temporaryPath);
                    Debug.LogWarning($"SaveManager isolated an invalid temporary save: {validationError}");
                    return null;
                }

                SaveData temporarySave = FromJson(json);
                if (temporarySave == null)
                {
                    return null;
                }

                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                }

                File.Move(temporaryPath, SavePath);
                Debug.LogWarning("SaveManager promoted a valid temporary save after an interrupted write.");
                return temporarySave;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"SaveManager could not recover the temporary save: {exception.Message}");
                return null;
            }
        }

        private static void RestorePrimarySaveFromBackup(SaveData backupSave)
        {
            if (backupSave == null)
            {
                return;
            }

            try
            {
                WriteSaveAtomically(SavePath, ToJson(backupSave));
                Debug.LogWarning("SaveManager restored the primary save from its backup.");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"SaveManager could not restore the primary save file: {exception.Message}");
            }
        }

        private static bool IsStructurallyValidSaveJson(string json, out string error)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                error = "save file is empty";
                return false;
            }

            string trimmed = json.Trim();
            if (!trimmed.StartsWith("{", StringComparison.Ordinal) || !trimmed.EndsWith("}", StringComparison.Ordinal))
            {
                error = "save file is not a JSON object";
                return false;
            }

            if (!HasBalancedJsonStructure(trimmed))
            {
                error = "save file has unbalanced JSON delimiters or strings";
                return false;
            }

            if (!Regex.IsMatch(trimmed, "\"saveVersion\"\\s*:\\s*-?\\d+") ||
                !Regex.IsMatch(trimmed, "\"totalGold\"\\s*:\\s*-?\\d+") ||
                !Regex.IsMatch(trimmed, "\"unlockedStageIds\"\\s*:\\s*\\["))
            {
                error = "save file is missing required fields";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static bool HasBalancedJsonStructure(string json)
        {
            Stack<char> closingDelimiters = new Stack<char>();
            bool insideString = false;
            bool escaped = false;
            bool rootClosed = false;

            for (int i = 0; i < json.Length; i++)
            {
                char character = json[i];
                if (insideString)
                {
                    if (escaped)
                    {
                        escaped = false;
                    }
                    else if (character == '\\')
                    {
                        escaped = true;
                    }
                    else if (character == '"')
                    {
                        insideString = false;
                    }

                    continue;
                }

                if (rootClosed)
                {
                    if (!char.IsWhiteSpace(character))
                    {
                        return false;
                    }

                    continue;
                }

                if (character == '"')
                {
                    insideString = true;
                }
                else if (character == '{')
                {
                    closingDelimiters.Push('}');
                }
                else if (character == '[')
                {
                    closingDelimiters.Push(']');
                }
                else if (character == '}' || character == ']')
                {
                    if (closingDelimiters.Count == 0 || closingDelimiters.Pop() != character)
                    {
                        return false;
                    }

                    rootClosed = closingDelimiters.Count == 0;
                }
            }

            return rootClosed && !insideString && !escaped && closingDelimiters.Count == 0;
        }

        private static string ResolveSavePath()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length - 1; i++)
            {
                if (!string.Equals(arguments[i], SavePathArgument, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string overridePath = arguments[i + 1];
                if (!string.IsNullOrWhiteSpace(overridePath))
                {
                    try
                    {
                        return Path.GetFullPath(overridePath);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogWarning($"SaveManager ignored an invalid {SavePathArgument} value: {exception.Message}");
                        break;
                    }
                }
            }

            return Path.Combine(Application.persistentDataPath, SaveFileName);
        }

        private static void Sanitize(SaveData saveData)
        {
            bool migrateDifficultyProgress = saveData.saveVersion < CurrentSaveVersion;
            if (saveData.saveVersion < CurrentSaveVersion)
            {
                saveData.saveVersion = CurrentSaveVersion;
            }

            if (saveData.clearedStageIds == null)
            {
                saveData.clearedStageIds = new List<string>();
            }

            if (saveData.unlockedStageIds == null)
            {
                saveData.unlockedStageIds = new List<string>();
            }

            if (saveData.clearedDifficultyIds == null)
            {
                saveData.clearedDifficultyIds = new List<string>();
            }

            if (saveData.clearedDifficultyStageKeys == null)
            {
                saveData.clearedDifficultyStageKeys = new List<string>();
            }

            if (saveData.upgradeLevels == null)
            {
                saveData.upgradeLevels = new List<SerializableUpgradeLevel>();
            }

            if (saveData.formationSlots == null)
            {
                saveData.formationSlots = new List<FormationSlot>();
            }

            if (saveData.monsterShardCounts == null)
            {
                saveData.monsterShardCounts = new List<SerializableMonsterShardCount>();
            }

            if (saveData.contractedPetIds == null)
            {
                saveData.contractedPetIds = new List<string>();
            }

            saveData.totalGold = Mathf.Max(0, saveData.totalGold);
            saveData.lastProcessedBattleRunId = saveData.lastProcessedBattleRunId ?? string.Empty;
            saveData.selectedDifficultyId = NormalizeDifficultyId(saveData.selectedDifficultyId);
            saveData.clearedStageIds = DeduplicateStrings(saveData.clearedStageIds);
            saveData.unlockedStageIds = DeduplicateStrings(saveData.unlockedStageIds);
            saveData.clearedDifficultyIds = SanitizeDifficultyIds(saveData.clearedDifficultyIds);
            saveData.clearedDifficultyStageKeys = SanitizeDifficultyStageKeys(saveData.clearedDifficultyStageKeys);
            saveData.contractedPetIds = DeduplicateStrings(saveData.contractedPetIds);
            AddUnique(saveData.unlockedStageIds, DefaultUnlockedStageId);

            if (migrateDifficultyProgress && saveData.clearedStageIds.Contains(DifficultyRules.ChapterOneFinalStageId))
            {
                AddUnique(saveData.clearedDifficultyIds, DifficultyRules.Normal);
                if (saveData.selectedDifficultyId == DifficultyRules.Hard || saveData.selectedDifficultyId == DifficultyRules.Nightmare)
                {
                    AddUnique(saveData.clearedDifficultyIds, DifficultyRules.Hard);
                }

                if (saveData.selectedDifficultyId == DifficultyRules.Nightmare)
                {
                    AddUnique(saveData.clearedDifficultyIds, DifficultyRules.Nightmare);
                }
            }

            if (migrateDifficultyProgress)
            {
                for (int i = 0; i < saveData.clearedStageIds.Count; i++)
                {
                    AddUnique(saveData.clearedDifficultyStageKeys,
                        BuildDifficultyStageKey(DifficultyRules.Normal, saveData.clearedStageIds[i]));
                }
            }

            if (!DifficultyRules.IsUnlocked(saveData, saveData.selectedDifficultyId))
            {
                saveData.selectedDifficultyId = DifficultyRules.Normal;
            }

            HashSet<string> seenUpgradeIds = new HashSet<string>();
            for (int i = saveData.upgradeLevels.Count - 1; i >= 0; i--)
            {
                SerializableUpgradeLevel entry = saveData.upgradeLevels[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.upgradeId) || seenUpgradeIds.Contains(entry.upgradeId))
                {
                    saveData.upgradeLevels.RemoveAt(i);
                    continue;
                }

                entry.level = Mathf.Max(0, entry.level);
                seenUpgradeIds.Add(entry.upgradeId);
            }

            for (int i = saveData.formationSlots.Count - 1; i >= 0; i--)
            {
                FormationSlot slot = saveData.formationSlots[i];
                if (slot == null || string.IsNullOrWhiteSpace(slot.HeroId))
                {
                    saveData.formationSlots.RemoveAt(i);
                    continue;
                }

                int laneIndex = Mathf.Clamp(slot.LaneIndex, 0, 2);
                saveData.formationSlots[i] = new FormationSlot(laneIndex, slot.PositionType, NormalizeHeroId(slot.HeroId));
            }

            if (saveData.formationSlots.Count == 0)
            {
                saveData.formationSlots = CreateDefaultFormationSlots();
            }

            saveData.monsterShardCounts = SanitizeMonsterShardCounts(saveData.monsterShardCounts);
            if (string.IsNullOrWhiteSpace(saveData.equippedPetId) || !saveData.contractedPetIds.Contains(saveData.equippedPetId))
            {
                saveData.equippedPetId = string.Empty;
            }
        }

        private static void AddUnique(List<string> values, string value)
        {
            if (values == null || string.IsNullOrWhiteSpace(value) || values.Contains(value))
            {
                return;
            }

            values.Add(value);
        }

        private static string ToJson(SaveData saveData)
        {
            StringBuilder builder = new StringBuilder(512);
            builder.AppendLine("{");
            builder.Append("  \"saveVersion\": ").Append(CurrentSaveVersion).AppendLine(",");
            builder.Append("  \"totalGold\": ").Append(saveData.totalGold).AppendLine(",");
            AppendStringList(builder, "clearedStageIds", saveData.clearedStageIds, true);
            AppendStringList(builder, "unlockedStageIds", saveData.unlockedStageIds, true);
            AppendStringList(builder, "clearedDifficultyIds", saveData.clearedDifficultyIds, true);
            AppendStringList(builder, "clearedDifficultyStageKeys", saveData.clearedDifficultyStageKeys, true);
            AppendUpgradeLevels(builder, saveData.upgradeLevels, true);
            AppendFormationSlots(builder, saveData.formationSlots, true);
            AppendMonsterShardCounts(builder, saveData.monsterShardCounts, true);
            AppendStringList(builder, "contractedPetIds", saveData.contractedPetIds, true);
            builder.Append("  \"equippedPetId\": \"").Append(EscapeJson(saveData.equippedPetId)).AppendLine("\",");
            builder.Append("  \"lastSelectedStageId\": \"").Append(EscapeJson(saveData.lastSelectedStageId)).AppendLine("\",");
            builder.Append("  \"lastProcessedBattleRunId\": \"").Append(EscapeJson(saveData.lastProcessedBattleRunId)).AppendLine("\",");
            builder.Append("  \"selectedDifficultyId\": \"").Append(EscapeJson(NormalizeDifficultyId(saveData.selectedDifficultyId))).AppendLine("\",");
            builder.Append("  \"hasSeenIntro\": ").Append(saveData.hasSeenIntro ? "true" : "false").AppendLine(",");
            builder.Append("  \"hasSeenTutorial\": ").Append(saveData.hasSeenTutorial ? "true" : "false").AppendLine(",");
            builder.Append("  \"hasSeenPetTutorial\": ").Append(saveData.hasSeenPetTutorial ? "true" : "false").AppendLine();
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static SaveData FromJson(string json)
        {
            SaveData saveData = new SaveData
            {
                saveVersion = ExtractInt(json, "saveVersion", CurrentSaveVersion),
                totalGold = ExtractInt(json, "totalGold", 0),
                clearedStageIds = ExtractStringList(json, "clearedStageIds"),
                unlockedStageIds = ExtractStringList(json, "unlockedStageIds"),
                clearedDifficultyIds = ExtractStringList(json, "clearedDifficultyIds"),
                clearedDifficultyStageKeys = ExtractStringList(json, "clearedDifficultyStageKeys"),
                upgradeLevels = ExtractUpgradeLevels(json),
                formationSlots = ExtractFormationSlots(json),
                monsterShardCounts = ExtractMonsterShardCounts(json),
                contractedPetIds = ExtractStringList(json, "contractedPetIds"),
                equippedPetId = ExtractString(json, "equippedPetId", string.Empty),
                lastSelectedStageId = ExtractString(json, "lastSelectedStageId", string.Empty),
                lastProcessedBattleRunId = ExtractString(json, "lastProcessedBattleRunId", string.Empty),
                selectedDifficultyId = ExtractString(json, "selectedDifficultyId", "normal"),
                hasSeenIntro = ExtractBool(json, "hasSeenIntro", false),
                hasSeenTutorial = ExtractBool(json, "hasSeenTutorial", ExtractBool(json, "hasSeenIntro", false)),
                hasSeenPetTutorial = ExtractBool(json, "hasSeenPetTutorial", false)
            };

            return saveData;
        }

        private static void AppendStringList(StringBuilder builder, string key, List<string> values, bool appendComma)
        {
            builder.Append("  \"").Append(key).AppendLine("\": [");

            if (values != null)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    builder.Append("    \"").Append(EscapeJson(values[i])).Append("\"");
                    if (i < values.Count - 1)
                    {
                        builder.Append(",");
                    }

                    builder.AppendLine();
                }
            }

            builder.Append("  ]");
            if (appendComma)
            {
                builder.Append(",");
            }

            builder.AppendLine();
        }

        private static void AppendFormationSlots(StringBuilder builder, List<FormationSlot> slots, bool appendComma)
        {
            builder.AppendLine("  \"formationSlots\": [");

            if (slots != null)
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    FormationSlot slot = slots[i];
                    if (slot == null)
                    {
                        continue;
                    }

                    builder.Append("    { \"laneIndex\": ")
                        .Append(Mathf.Clamp(slot.LaneIndex, 0, 2))
                        .Append(", \"positionType\": \"")
                        .Append(slot.PositionType)
                        .Append("\", \"heroId\": \"")
                        .Append(EscapeJson(slot.HeroId))
                        .Append("\" }");

                    if (i < slots.Count - 1)
                    {
                        builder.Append(",");
                    }

                    builder.AppendLine();
                }
            }

            builder.Append("  ]");
            if (appendComma)
            {
                builder.Append(",");
            }

            builder.AppendLine();
        }

        private static void AppendUpgradeLevels(StringBuilder builder, List<SerializableUpgradeLevel> levels, bool appendComma)
        {
            builder.AppendLine("  \"upgradeLevels\": [");

            if (levels != null)
            {
                for (int i = 0; i < levels.Count; i++)
                {
                    SerializableUpgradeLevel entry = levels[i];
                    if (entry == null)
                    {
                        continue;
                    }

                    builder.Append("    { \"upgradeId\": \"")
                        .Append(EscapeJson(entry.upgradeId))
                        .Append("\", \"level\": ")
                        .Append(Mathf.Max(0, entry.level))
                        .Append(" }");

                    if (i < levels.Count - 1)
                    {
                        builder.Append(",");
                    }

                    builder.AppendLine();
                }
            }

            builder.Append("  ]");
            if (appendComma)
            {
                builder.Append(",");
            }

            builder.AppendLine();
        }

        private static void AppendMonsterShardCounts(StringBuilder builder, List<SerializableMonsterShardCount> counts, bool appendComma)
        {
            builder.AppendLine("  \"monsterShardCounts\": [");

            if (counts != null)
            {
                for (int i = 0; i < counts.Count; i++)
                {
                    SerializableMonsterShardCount entry = counts[i];
                    if (entry == null || string.IsNullOrWhiteSpace(entry.monsterId) || entry.count <= 0)
                    {
                        continue;
                    }

                    builder.Append("    { \"monsterId\": \"")
                        .Append(EscapeJson(entry.monsterId))
                        .Append("\", \"count\": ")
                        .Append(Mathf.Max(0, entry.count))
                        .Append(" }");

                    if (i < counts.Count - 1)
                    {
                        builder.Append(",");
                    }

                    builder.AppendLine();
                }
            }

            builder.Append("  ]");
            if (appendComma)
            {
                builder.Append(",");
            }

            builder.AppendLine();
        }

        private static int ExtractInt(string json, string key, int defaultValue)
        {
            Match match = Regex.Match(json, $"\"{Regex.Escape(key)}\"\\s*:\\s*(-?\\d+)");
            return match.Success && int.TryParse(match.Groups[1].Value, out int value) ? value : defaultValue;
        }

        private static bool ExtractBool(string json, string key, bool defaultValue)
        {
            Match match = Regex.Match(json, $"\"{Regex.Escape(key)}\"\\s*:\\s*(true|false)", RegexOptions.IgnoreCase);
            return match.Success && bool.TryParse(match.Groups[1].Value, out bool value) ? value : defaultValue;
        }

        private static string ExtractString(string json, string key, string defaultValue)
        {
            Match match = Regex.Match(json, $"\"{Regex.Escape(key)}\"\\s*:\\s*\"((?:\\\\.|[^\"])*)\"");
            return match.Success ? UnescapeJson(match.Groups[1].Value) : defaultValue;
        }

        private static List<string> ExtractStringList(string json, string key)
        {
            List<string> values = new List<string>();
            Match arrayMatch = Regex.Match(json, $"\"{Regex.Escape(key)}\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
            if (!arrayMatch.Success)
            {
                return values;
            }

            MatchCollection itemMatches = Regex.Matches(arrayMatch.Groups[1].Value, "\"((?:\\\\.|[^\"])*)\"");
            for (int i = 0; i < itemMatches.Count; i++)
            {
                values.Add(UnescapeJson(itemMatches[i].Groups[1].Value));
            }

            return values;
        }

        private static List<SerializableUpgradeLevel> ExtractUpgradeLevels(string json)
        {
            List<SerializableUpgradeLevel> levels = new List<SerializableUpgradeLevel>();
            Match arrayMatch = Regex.Match(json, "\"upgradeLevels\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
            if (!arrayMatch.Success)
            {
                return levels;
            }

            MatchCollection objectMatches = Regex.Matches(arrayMatch.Groups[1].Value, "\\{(.*?)\\}", RegexOptions.Singleline);
            for (int i = 0; i < objectMatches.Count; i++)
            {
                string objectJson = objectMatches[i].Groups[1].Value;
                string upgradeId = ExtractString(objectJson, "upgradeId", string.Empty);
                int level = ExtractInt(objectJson, "level", 0);
                if (!string.IsNullOrWhiteSpace(upgradeId))
                {
                    levels.Add(new SerializableUpgradeLevel(upgradeId, level));
                }
            }

            return levels;
        }

        private static List<FormationSlot> ExtractFormationSlots(string json)
        {
            List<FormationSlot> slots = new List<FormationSlot>();
            Match arrayMatch = Regex.Match(json, "\"formationSlots\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
            if (!arrayMatch.Success)
            {
                return slots;
            }

            MatchCollection objectMatches = Regex.Matches(arrayMatch.Groups[1].Value, "\\{(.*?)\\}", RegexOptions.Singleline);
            for (int i = 0; i < objectMatches.Count; i++)
            {
                string objectJson = objectMatches[i].Groups[1].Value;
                int laneIndex = ExtractInt(objectJson, "laneIndex", 0);
                string positionText = ExtractString(objectJson, "positionType", HeroPositionType.Middle.ToString());
                string heroId = ExtractString(objectJson, "heroId", string.Empty);

                if (!Enum.TryParse(positionText, out HeroPositionType positionType))
                {
                    positionType = HeroPositionType.Middle;
                }

                if (!string.IsNullOrWhiteSpace(heroId))
                {
                    slots.Add(new FormationSlot(laneIndex, positionType, NormalizeHeroId(heroId)));
                }
            }

            return slots;
        }

        private static List<SerializableMonsterShardCount> ExtractMonsterShardCounts(string json)
        {
            List<SerializableMonsterShardCount> counts = new List<SerializableMonsterShardCount>();
            Match arrayMatch = Regex.Match(json, "\"monsterShardCounts\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
            if (!arrayMatch.Success)
            {
                return counts;
            }

            MatchCollection objectMatches = Regex.Matches(arrayMatch.Groups[1].Value, "\\{(.*?)\\}", RegexOptions.Singleline);
            for (int i = 0; i < objectMatches.Count; i++)
            {
                string objectJson = objectMatches[i].Groups[1].Value;
                string monsterId = ExtractString(objectJson, "monsterId", string.Empty);
                int count = ExtractInt(objectJson, "count", 0);
                if (!string.IsNullOrWhiteSpace(monsterId) && count > 0)
                {
                    counts.Add(new SerializableMonsterShardCount(monsterId, count));
                }
            }

            return counts;
        }

        private static List<FormationSlot> CopyFormationSlots(IReadOnlyList<FormationSlot> sourceSlots)
        {
            List<FormationSlot> copy = new List<FormationSlot>();
            if (sourceSlots == null)
            {
                return copy;
            }

            for (int i = 0; i < sourceSlots.Count; i++)
            {
                FormationSlot slot = sourceSlots[i];
                if (slot != null && !string.IsNullOrWhiteSpace(slot.HeroId))
                {
                    copy.Add(new FormationSlot(Mathf.Clamp(slot.LaneIndex, 0, 2), slot.PositionType, NormalizeHeroId(slot.HeroId)));
                }
            }

            return copy;
        }

        private static string NormalizeHeroId(string heroId)
        {
            return heroId == "hero_cleric_001" ? "hero_priest_001" : heroId;
        }

        private static string NormalizeDifficultyId(string difficultyId)
        {
            string value = string.IsNullOrWhiteSpace(difficultyId) ? "normal" : difficultyId.Trim().ToLowerInvariant();
            return value == "easy" || value == "hard" || value == "nightmare" ? value : "normal";
        }

        private static List<string> DeduplicateStrings(List<string> values)
        {
            List<string> result = new List<string>();
            if (values == null)
            {
                return result;
            }

            for (int i = 0; i < values.Count; i++)
            {
                AddUnique(result, values[i]);
            }

            return result;
        }

        private static List<string> SanitizeDifficultyIds(List<string> values)
        {
            List<string> result = new List<string>();
            if (values == null)
            {
                return result;
            }

            for (int i = 0; i < values.Count; i++)
            {
                string normalized = NormalizeDifficultyId(values[i]);
                if (string.Equals(values[i], normalized, StringComparison.OrdinalIgnoreCase))
                {
                    AddUnique(result, normalized);
                }
            }

            return result;
        }

        public static bool IsStageCleared(SaveData saveData, string stageId, string difficultyId)
        {
            if (saveData == null || string.IsNullOrWhiteSpace(stageId))
            {
                return false;
            }

            string normalizedDifficulty = DifficultyRules.Normalize(difficultyId);
            if (normalizedDifficulty == DifficultyRules.Easy || normalizedDifficulty == DifficultyRules.Normal)
            {
                return saveData.clearedStageIds != null && saveData.clearedStageIds.Contains(stageId);
            }

            return saveData.clearedDifficultyStageKeys != null &&
                   saveData.clearedDifficultyStageKeys.Contains(BuildDifficultyStageKey(normalizedDifficulty, stageId));
        }

        public static bool IsStageUnlocked(SaveData saveData, string stageId, string difficultyId)
        {
            if (saveData == null || string.IsNullOrWhiteSpace(stageId) || !DifficultyRules.IsUnlocked(saveData, difficultyId))
            {
                return false;
            }

            string normalizedDifficulty = DifficultyRules.Normalize(difficultyId);
            if (normalizedDifficulty == DifficultyRules.Easy || normalizedDifficulty == DifficultyRules.Normal)
            {
                return saveData.unlockedStageIds != null && saveData.unlockedStageIds.Contains(stageId);
            }

            if (stageId == DefaultUnlockedStageId)
            {
                return true;
            }

            string previousStageId = ResolvePreviousStageId(stageId);
            return !string.IsNullOrWhiteSpace(previousStageId) &&
                   IsStageCleared(saveData, previousStageId, normalizedDifficulty);
        }

        private static string ResolvePreviousStageId(string stageId)
        {
            int separatorIndex = string.IsNullOrWhiteSpace(stageId) ? -1 : stageId.LastIndexOf('_');
            if (separatorIndex < 0 || separatorIndex >= stageId.Length - 1 ||
                !int.TryParse(stageId.Substring(separatorIndex + 1), out int stageNumber) || stageNumber <= 1)
            {
                return string.Empty;
            }

            return stageId.Substring(0, separatorIndex + 1) + (stageNumber - 1).ToString("D2");
        }

        private static string BuildDifficultyStageKey(string difficultyId, string stageId)
        {
            return $"{DifficultyRules.Normalize(difficultyId)}:{stageId}";
        }

        private static List<string> SanitizeDifficultyStageKeys(List<string> values)
        {
            List<string> result = new List<string>();
            if (values == null)
            {
                return result;
            }

            for (int i = 0; i < values.Count; i++)
            {
                string value = values[i];
                int separatorIndex = string.IsNullOrWhiteSpace(value) ? -1 : value.IndexOf(':');
                if (separatorIndex <= 0 || separatorIndex >= value.Length - 1)
                {
                    continue;
                }

                string difficultyId = value.Substring(0, separatorIndex);
                string stageId = value.Substring(separatorIndex + 1);
                string normalizedDifficulty = NormalizeDifficultyId(difficultyId);
                if (string.Equals(difficultyId, normalizedDifficulty, StringComparison.OrdinalIgnoreCase))
                {
                    AddUnique(result, BuildDifficultyStageKey(normalizedDifficulty, stageId));
                }
            }

            return result;
        }

        private static List<SerializableMonsterShardCount> SanitizeMonsterShardCounts(List<SerializableMonsterShardCount> counts)
        {
            Dictionary<string, int> merged = new Dictionary<string, int>();
            if (counts != null)
            {
                for (int i = 0; i < counts.Count; i++)
                {
                    SerializableMonsterShardCount entry = counts[i];
                    if (entry == null || string.IsNullOrWhiteSpace(entry.monsterId) || entry.count <= 0)
                    {
                        continue;
                    }

                    if (!merged.ContainsKey(entry.monsterId))
                    {
                        merged[entry.monsterId] = 0;
                    }

                    merged[entry.monsterId] = Mathf.Max(0, merged[entry.monsterId] + entry.count);
                }
            }

            List<SerializableMonsterShardCount> result = new List<SerializableMonsterShardCount>();
            foreach (KeyValuePair<string, int> pair in merged)
            {
                result.Add(new SerializableMonsterShardCount(pair.Key, pair.Value));
            }

            return result;
        }

        private static void BackupSaveFile(string reason)
        {
            try
            {
                if (!HasSaveFile())
                {
                    return;
                }

                string backupPath = SavePath + CorruptSaveExtension;
                File.Copy(SavePath, backupPath, true);
                Debug.LogWarning($"SaveManager backed up invalid save to {backupPath}. Reason: {reason}");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"SaveManager could not backup invalid save: {exception.Message}");
            }
        }

        private static SaveData TryLoadBackupFromDisk()
        {
            string backupPath = SavePath + BackupSaveExtension;
            if (!File.Exists(backupPath))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(backupPath);
                if (!IsStructurallyValidSaveJson(json, out string validationError))
                {
                    Debug.LogWarning($"SaveManager ignored an invalid backup save: {validationError}");
                    return null;
                }

                SaveData backupSave = FromJson(json);
                if (backupSave != null)
                {
                    Debug.LogWarning($"SaveManager restored save data from backup: {backupPath}");
                }

                return backupSave;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"SaveManager failed to load backup save: {exception.Message}");
                return null;
            }
        }

        private static void WriteSaveAtomically(string targetPath, string json)
        {
            string directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string tempPath = targetPath + TemporarySaveExtension;
            string backupPath = targetPath + BackupSaveExtension;
            File.WriteAllText(tempPath, json, Encoding.UTF8);

            if (File.Exists(targetPath))
            {
                string existingJson = File.ReadAllText(targetPath);
                if (IsStructurallyValidSaveJson(existingJson, out _))
                {
                    File.Copy(targetPath, backupPath, true);
                }

                File.Delete(targetPath);
            }

            File.Move(tempPath, targetPath);
        }

        private static string EscapeJson(string value)
        {
            return (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static string UnescapeJson(string value)
        {
            return (value ?? string.Empty).Replace("\\\"", "\"").Replace("\\\\", "\\");
        }
    }
}
