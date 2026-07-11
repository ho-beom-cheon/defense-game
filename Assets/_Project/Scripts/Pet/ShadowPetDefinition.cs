namespace RuneGate
{
    public readonly struct ShadowPetDefinition
    {
        public ShadowPetDefinition(string monsterId, string displayName, ShadowPetPassiveType passiveType, float passiveValue)
        {
            MonsterId = monsterId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            PassiveType = passiveType;
            PassiveValue = passiveValue;
        }

        public string MonsterId { get; }
        public string DisplayName { get; }
        public ShadowPetPassiveType PassiveType { get; }
        public float PassiveValue { get; }
    }
}
