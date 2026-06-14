using UnityEngine;

namespace RuneGate
{
    [CreateAssetMenu(fileName = "RuneData", menuName = "RuneGate/Data/Rune")]
    public sealed class RuneData : ScriptableObject
    {
        [SerializeField] private string runeId = "rune_id";
        [SerializeField] private string displayName = "Rune";
        [SerializeField, TextArea] private string description = "Rune description";
        [SerializeField] private RuneRarity rarity = RuneRarity.Common;
        [SerializeField] private ElementType element = ElementType.None;
        [SerializeField] private string effectKey = "hero_attack_percent";
        [SerializeField] private float value = 0.1f;
        [SerializeField] private Sprite icon;

        public string RuneId => runeId;
        public string DisplayName => displayName;
        public string Description => description;
        public RuneRarity Rarity => rarity;
        public ElementType Element => element;
        public string EffectKey => effectKey;
        public float Value => value;
        public Sprite Icon => icon;
    }
}
