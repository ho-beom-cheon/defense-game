namespace RuneGate
{
    public readonly struct BattleResultViewData
    {
        public BattleResultViewData(bool isVictory, string title, string message, int goldEarned, int battleGoldEarned,
            string elapsed, string crystalHp, string kills, string waves, string difficulty, string shardRewards,
            string unlockMessage, string primaryActionLabel, bool hasNextStage)
        {
            IsVictory = isVictory;
            Title = title;
            Message = message;
            GoldEarned = goldEarned;
            BattleGoldEarned = battleGoldEarned;
            Elapsed = elapsed;
            CrystalHp = crystalHp;
            Kills = kills;
            Waves = waves;
            Difficulty = difficulty;
            ShardRewards = shardRewards;
            UnlockMessage = unlockMessage;
            PrimaryActionLabel = primaryActionLabel;
            HasNextStage = hasNextStage;
        }

        public bool IsVictory { get; }
        public string Title { get; }
        public string Message { get; }
        public int GoldEarned { get; }
        public int BattleGoldEarned { get; }
        public string Elapsed { get; }
        public string CrystalHp { get; }
        public string Kills { get; }
        public string Waves { get; }
        public string Difficulty { get; }
        public string ShardRewards { get; }
        public string UnlockMessage { get; }
        public string PrimaryActionLabel { get; }
        public bool HasNextStage { get; }
    }
}
