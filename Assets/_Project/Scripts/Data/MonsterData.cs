using UnityEngine;

namespace RuneGate
{
    [CreateAssetMenu(fileName = "MonsterData", menuName = "RuneGate/Data/Monster")]
    public sealed class MonsterData : ScriptableObject
    {
        [SerializeField] private string monsterId = "monster_id";
        [SerializeField] private string displayName = "Monster";
        [SerializeField] private MonsterType monsterType = MonsterType.Normal;
        [SerializeField] private ElementType element = ElementType.None;
        [SerializeField] private int maxHp = 40;
        [SerializeField] private float moveSpeed = 1f;
        [SerializeField] private int damageToCrystal = 5;
        [SerializeField] private int rewardGold = 1;
        [SerializeField] private Sprite sprite;
        [SerializeField] private RuntimeAnimatorController animatorController;
        [SerializeField] private GameObject prefab;
        [SerializeField] private bool isBoss;

        public string MonsterId => monsterId;
        public string DisplayName => displayName;
        public MonsterType MonsterType => monsterType;
        public ElementType Element => element;
        public int MaxHp => maxHp;
        public float MoveSpeed => moveSpeed;
        public int DamageToCrystal => damageToCrystal;
        public int RewardGold => rewardGold;
        public Sprite Sprite => sprite;
        public RuntimeAnimatorController AnimatorController => animatorController;
        public GameObject Prefab => prefab;
        public bool IsBoss => isBoss || monsterType == MonsterType.Boss;
    }
}
