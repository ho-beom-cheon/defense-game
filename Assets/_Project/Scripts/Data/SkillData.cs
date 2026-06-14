using UnityEngine;

namespace RuneGate
{
    [CreateAssetMenu(fileName = "SkillData", menuName = "RuneGate/Data/Skill")]
    public sealed class SkillData : ScriptableObject
    {
        [SerializeField] private string skillId = "skill_id";
        [SerializeField] private string displayName = "Skill";
        [SerializeField, TextArea] private string description = "Skill description";
        [SerializeField] private float cooldown = 8f;
        [SerializeField] private int power = 20;
        [SerializeField] private int damageHitCount = 1;
        [SerializeField] private float range = 4f;
        [SerializeField] private string effectKey = "damage";
        [SerializeField] private float radius = 1.2f;
        [SerializeField] private TargetingType targetingType = TargetingType.First;
        [SerializeField] private ElementType element = ElementType.None;
        [SerializeField] private Sprite icon;

        public string SkillId => skillId;
        public string DisplayName => displayName;
        public string Description => description;
        public float Cooldown => cooldown;
        public int Power => power;
        public int DamageHitCount => Mathf.Max(1, damageHitCount);
        public float Range => range;
        public string EffectKey => effectKey;
        public float Radius => radius;
        public TargetingType TargetingType => targetingType;
        public ElementType Element => element;
        public Sprite Icon => icon;
    }
}
