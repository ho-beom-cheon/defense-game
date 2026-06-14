using System;

namespace RuneGate
{
    [Serializable]
    public sealed class SerializableUpgradeLevel
    {
        public string upgradeId = string.Empty;
        public int level;

        public SerializableUpgradeLevel()
        {
        }

        public SerializableUpgradeLevel(string upgradeId, int level)
        {
            this.upgradeId = upgradeId;
            this.level = level;
        }
    }
}
