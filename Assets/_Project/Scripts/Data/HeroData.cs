using UnityEngine;

namespace RuneGate
{
    [CreateAssetMenu(fileName = "HeroData", menuName = "RuneGate/Data/Hero")]
    public sealed class HeroData : ScriptableObject
    {
        [SerializeField] private string heroId = "hero_id";
        [SerializeField] private string displayName = "Hero";
        [SerializeField] private HeroRole role = HeroRole.MeleeDps;
        [SerializeField] private HeroPositionType positionType = HeroPositionType.Middle;
        [SerializeField] private ElementType element = ElementType.None;
        [SerializeField] private int maxHp = 100;
        [SerializeField] private int attack = 10;
        [SerializeField] private float attackSpeed = 1f;
        [SerializeField] private float attackRange = 4f;
        [SerializeField] private SkillData skillData;
        [SerializeField] private Sprite portrait;
        [SerializeField] private RuntimeAnimatorController animatorController;

        public string HeroId => heroId;
        public string DisplayName => displayName;
        public HeroRole Role => role;
        public HeroPositionType PositionType => positionType;
        public ElementType Element => element;
        public int MaxHp => maxHp;
        public int Attack => attack;
        public float AttackSpeed => attackSpeed;
        public float AttackRange => attackRange;
        public SkillData SkillData => skillData;
        public Sprite Portrait => portrait;
        public RuntimeAnimatorController AnimatorController => animatorController;
    }
}
