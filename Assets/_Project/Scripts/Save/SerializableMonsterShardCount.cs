using System;

namespace RuneGate
{
    [Serializable]
    public sealed class SerializableMonsterShardCount
    {
        public string monsterId = string.Empty;
        public int count;

        public SerializableMonsterShardCount()
        {
        }

        public SerializableMonsterShardCount(string monsterId, int count)
        {
            this.monsterId = monsterId ?? string.Empty;
            this.count = count;
        }
    }
}
