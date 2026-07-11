namespace RuneGate
{
    public readonly struct BattleResult
    {
        public BattleResult(bool isVictory, StageData stageData, int wavesCleared, int goldEarned, string message, float elapsedSeconds = 0f, int monstersKilled = 0, int crystalHp = 0, int crystalMaxHp = 0, string battleRunId = "")
        {
            IsVictory = isVictory;
            StageData = stageData;
            WavesCleared = wavesCleared;
            GoldEarned = goldEarned;
            Message = message;
            ElapsedSeconds = elapsedSeconds;
            MonstersKilled = monstersKilled;
            CrystalHp = crystalHp;
            CrystalMaxHp = crystalMaxHp;
            BattleRunId = battleRunId ?? string.Empty;
        }

        public bool IsVictory { get; }
        public StageData StageData { get; }
        public int WavesCleared { get; }
        public int GoldEarned { get; }
        public string Message { get; }
        public float ElapsedSeconds { get; }
        public int MonstersKilled { get; }
        public int CrystalHp { get; }
        public int CrystalMaxHp { get; }
        public string BattleRunId { get; }
    }
}
