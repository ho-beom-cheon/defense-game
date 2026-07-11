using System;
using System.Collections.Generic;

namespace RuneGate
{
    [Serializable]
    public sealed class SaveData
    {
        public int saveVersion = 1;
        public int totalGold;
        public List<string> clearedStageIds = new List<string>();
        public List<string> unlockedStageIds = new List<string>();
        public List<SerializableUpgradeLevel> upgradeLevels = new List<SerializableUpgradeLevel>();
        public List<FormationSlot> formationSlots = new List<FormationSlot>();
        public List<SerializableMonsterShardCount> monsterShardCounts = new List<SerializableMonsterShardCount>();
        public List<string> contractedPetIds = new List<string>();
        public string equippedPetId = string.Empty;
        public string lastSelectedStageId = string.Empty;
        public string lastProcessedBattleRunId = string.Empty;
        public string selectedDifficultyId = "normal";
        public bool hasSeenIntro;
        public bool hasSeenTutorial;
        public bool hasSeenPetTutorial;
    }
}
