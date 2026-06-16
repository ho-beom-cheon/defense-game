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

        private const int CurrentSaveVersion = 2;
        private const string SaveFileName = "runegate_save.json";

        private static SaveData currentSave;

        public static SaveData Current
        {
            get
            {
                LoadOrCreate();
                return currentSave;
            }
        }

        public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

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
            if (currentSave == null)
            {
                currentSave = CreateDefaultSave();
            }

            Sanitize(currentSave);
            return currentSave;
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
                File.WriteAllText(SavePath, json);
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
                    string backupPath = SavePath + ".bak";
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

        public static bool IsStageUnlocked(string stageId)
        {
            return !string.IsNullOrWhiteSpace(stageId) && Current.unlockedStageIds.Contains(stageId);
        }

        public static void SetLastSelectedStageId(string stageId)
        {
            Current.lastSelectedStageId = stageId ?? string.Empty;
            Save();
        }

        public static void SetSelectedDifficultyId(string difficultyId)
        {
            Current.selectedDifficultyId = NormalizeDifficultyId(difficultyId);
            Save();
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

        public static int GetUpgradeLevel(string upgradeId)
        {
            if (string.IsNullOrWhiteSpace(upgradeId))
            {
                return 0;
            }

            List<SerializableUpgradeLevel> levels = Current.upgradeLevels;
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
                return null;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    BackupSaveFile("empty save file");
                    return null;
                }

                return FromJson(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"SaveManager failed to load save data. A default save will be used. {exception.Message}");
                BackupSaveFile(exception.Message);
                return null;
            }
        }

        private static void Sanitize(SaveData saveData)
        {
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

            if (saveData.upgradeLevels == null)
            {
                saveData.upgradeLevels = new List<SerializableUpgradeLevel>();
            }

            if (saveData.formationSlots == null)
            {
                saveData.formationSlots = new List<FormationSlot>();
            }

            saveData.totalGold = Mathf.Max(0, saveData.totalGold);
            saveData.selectedDifficultyId = NormalizeDifficultyId(saveData.selectedDifficultyId);
            saveData.clearedStageIds = DeduplicateStrings(saveData.clearedStageIds);
            saveData.unlockedStageIds = DeduplicateStrings(saveData.unlockedStageIds);
            AddUnique(saveData.unlockedStageIds, DefaultUnlockedStageId);

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
            AppendUpgradeLevels(builder, saveData.upgradeLevels, true);
            AppendFormationSlots(builder, saveData.formationSlots, true);
            builder.Append("  \"lastSelectedStageId\": \"").Append(EscapeJson(saveData.lastSelectedStageId)).AppendLine("\",");
            builder.Append("  \"selectedDifficultyId\": \"").Append(EscapeJson(NormalizeDifficultyId(saveData.selectedDifficultyId))).AppendLine("\",");
            builder.Append("  \"hasSeenIntro\": ").Append(saveData.hasSeenIntro ? "true" : "false").AppendLine(",");
            builder.Append("  \"hasSeenTutorial\": ").Append(saveData.hasSeenTutorial ? "true" : "false").AppendLine();
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
                upgradeLevels = ExtractUpgradeLevels(json),
                formationSlots = ExtractFormationSlots(json),
                lastSelectedStageId = ExtractString(json, "lastSelectedStageId", string.Empty),
                selectedDifficultyId = ExtractString(json, "selectedDifficultyId", "normal"),
                hasSeenIntro = ExtractBool(json, "hasSeenIntro", false),
                hasSeenTutorial = ExtractBool(json, "hasSeenTutorial", ExtractBool(json, "hasSeenIntro", false))
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

        private static void BackupSaveFile(string reason)
        {
            try
            {
                if (!HasSaveFile())
                {
                    return;
                }

                string backupPath = SavePath + ".bak";
                File.Copy(SavePath, backupPath, true);
                Debug.LogWarning($"SaveManager backed up invalid save to {backupPath}. Reason: {reason}");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"SaveManager could not backup invalid save: {exception.Message}");
            }
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
