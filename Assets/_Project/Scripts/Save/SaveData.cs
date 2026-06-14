using System;
using System.Collections.Generic;

namespace RuneGate
{
    [Serializable]
    public sealed class SaveData
    {
        public int totalGold;
        public List<string> clearedStageIds = new List<string>();
        public List<string> unlockedStageIds = new List<string>();
        public List<SerializableUpgradeLevel> upgradeLevels = new List<SerializableUpgradeLevel>();
        public string lastSelectedStageId = string.Empty;
        public bool hasSeenIntro;
    }
}
