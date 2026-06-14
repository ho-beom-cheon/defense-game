namespace RuneGate
{
    public readonly struct BattleResult
    {
        public BattleResult(bool isVictory, StageData stageData, int wavesCleared, int goldEarned, string message)
        {
            IsVictory = isVictory;
            StageData = stageData;
            WavesCleared = wavesCleared;
            GoldEarned = goldEarned;
            Message = message;
        }

        public bool IsVictory { get; }
        public StageData StageData { get; }
        public int WavesCleared { get; }
        public int GoldEarned { get; }
        public string Message { get; }
    }
}
