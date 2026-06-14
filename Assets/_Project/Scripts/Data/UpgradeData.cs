using UnityEngine;

namespace RuneGate
{
    [CreateAssetMenu(fileName = "UpgradeData", menuName = "RuneGate/Data/Upgrade")]
    public sealed class UpgradeData : ScriptableObject
    {
        [SerializeField] private string upgradeId = "upgrade_id";
        [SerializeField] private string displayName = "Upgrade";
        [SerializeField, TextArea] private string description = "Upgrade description";
        [SerializeField] private int baseCost = 20;
        [SerializeField] private float costMultiplier = 1.5f;
        [SerializeField] private int maxLevel = 10;
        [SerializeField] private string effectKey = "effect_key";
        [SerializeField] private float valuePerLevel = 1f;

        public string UpgradeId => upgradeId;
        public string DisplayName => displayName;
        public string Description => description;
        public int BaseCost => baseCost;
        public float CostMultiplier => costMultiplier;
        public int MaxLevel => maxLevel;
        public string EffectKey => effectKey;
        public float ValuePerLevel => valuePerLevel;
    }
}
