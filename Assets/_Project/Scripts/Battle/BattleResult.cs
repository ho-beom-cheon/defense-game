namespace RuneGate
{
    public readonly struct BattleResult
    {
        public BattleResult(bool isVictory, StageData stageData, int wavesCleared, string message)
        {
            IsVictory = isVictory;
            StageData = stageData;
            WavesCleared = wavesCleared;
            Message = message;
        }

        public bool IsVictory { get; }
        public StageData StageData { get; }
        public int WavesCleared { get; }
        public string Message { get; }
    }
}
