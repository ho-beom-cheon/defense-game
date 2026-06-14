using System;
using UnityEngine;

namespace RuneGate
{
    public sealed class HeroController : MonoBehaviour
    {
        [SerializeField] private HeroData heroData;
        [SerializeField] private SkillController skillController;
        [SerializeField] private ProjectileController projectilePrefab;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private LayerMask monsterLayer = ~0;
        [SerializeField] private bool initializeOnAwake = true;

        private int currentHp;
        private int maxHp;
        private int baseAttack;
        private float baseAttackSpeed;
        private float attackRange;
        private float attackCooldown;
        private float attackMultiplier = 1f;
        private float attackSpeedMultiplier = 1f;
        private bool initialized;

        public event Action<int, int> HpChanged;

        public HeroData Data => heroData;
        public SkillController SkillController => skillController;
        public int CurrentHp => currentHp;
        public int MaxHp => maxHp;
        public int EffectiveAttack => Mathf.Max(1, Mathf.RoundToInt(baseAttack * attackMultiplier));
        public float EffectiveAttackSpeed => Mathf.Max(0.01f, baseAttackSpeed * attackSpeedMultiplier);
        public bool IsAlive => initialized && currentHp > 0;

        private void Awake()
        {
            if (skillController == null)
            {
                skillController = GetComponent<SkillController>();
            }

            if (initializeOnAwake && heroData != null)
            {
                Initialize(heroData);
            }
        }

        private void Update()
        {
            if (!IsAlive)
            {
                return;
            }

            if (attackCooldown > 0f)
            {
                attackCooldown -= Time.deltaTime;
                return;
            }

            MonsterController target = FindTarget(attackRange, TargetingType.First);
            if (target == null)
            {
                return;
            }

            BasicAttack(target);
            float attacksPerSecond = Mathf.Max(0.01f, baseAttackSpeed * attackSpeedMultiplier);
            attackCooldown = 1f / attacksPerSecond;
        }

        public void InitializeFromSerializedData()
        {
            Initialize(heroData);
        }

        public void Initialize(HeroData data)
        {
            if (data == null)
            {
                Debug.LogWarning($"{nameof(HeroController)} on {name} cannot initialize because HeroData is missing.");
                return;
            }

            heroData = data;
            maxHp = Mathf.Max(1, data.MaxHp);
            currentHp = maxHp;
            baseAttack = Mathf.Max(0, data.Attack);
            baseAttackSpeed = Mathf.Max(0.01f, data.AttackSpeed);
            attackRange = Mathf.Max(0.1f, data.AttackRange);
            attackCooldown = 0f;
            attackMultiplier = 1f;
            attackSpeedMultiplier = 1f;
            initialized = true;

            if (skillController == null)
            {
                skillController = GetComponent<SkillController>();
            }

            if (skillController != null)
            {
                skillController.Initialize(data.SkillData);
            }

            HpChanged?.Invoke(currentHp, maxHp);
        }

        public void RequestManualSkill()
        {
            if (!IsAlive)
            {
                return;
            }

            if (skillController == null)
            {
                Debug.LogWarning($"{name} cannot use a skill because SkillController is missing.");
                return;
            }

            MonsterController target = FindTarget(skillController.Range, skillController.TargetingType);
            skillController.UseSkill(this, target);
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || !IsAlive)
            {
                return;
            }

            currentHp = Mathf.Max(0, currentHp - damage);
            HpChanged?.Invoke(currentHp, maxHp);
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || !IsAlive)
            {
                return;
            }

            currentHp = Mathf.Min(maxHp, currentHp + amount);
            HpChanged?.Invoke(currentHp, maxHp);
        }

        public void ApplyAttackPercent(float percent)
        {
            attackMultiplier = Mathf.Max(0.1f, attackMultiplier + percent);
        }

        public void ApplyAttackSpeedPercent(float percent)
        {
            attackSpeedMultiplier = Mathf.Max(0.1f, attackSpeedMultiplier + percent);
        }

        public void ApplySkillCooldownPercent(float percent)
        {
            if (skillController != null)
            {
                skillController.ApplyCooldownPercent(percent);
            }
        }

        private void BasicAttack(MonsterController target)
        {
            int damage = EffectiveAttack;
            Vector3 spawnPosition = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;

            if (projectilePrefab != null)
            {
                ProjectileController projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
                projectile.Initialize(target, damage);
            }
            else
            {
                target.TakeDamage(damage);
            }
        }

        private MonsterController FindTarget(float range, TargetingType targetingType)
        {
            float safeRange = Mathf.Max(0.1f, range);
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, safeRange, monsterLayer);
            MonsterController selected = null;

            for (int i = 0; i < hits.Length; i++)
            {
                MonsterController monster = hits[i].GetComponentInParent<MonsterController>();
                if (monster == null || !monster.IsAlive)
                {
                    continue;
                }

                if (targetingType == TargetingType.Boss && monster.Data != null && monster.Data.MonsterType != MonsterType.Boss)
                {
                    continue;
                }

                selected = PickBetterTarget(selected, monster, targetingType);
            }

            if (selected == null && targetingType == TargetingType.Boss)
            {
                return FindTarget(safeRange, TargetingType.Nearest);
            }

            return selected;
        }

        private MonsterController PickBetterTarget(MonsterController current, MonsterController candidate, TargetingType targetingType)
        {
            if (current == null)
            {
                return candidate;
            }

            switch (targetingType)
            {
                case TargetingType.Nearest:
                    float currentDistance = Vector2.Distance(transform.position, current.transform.position);
                    float candidateDistance = Vector2.Distance(transform.position, candidate.transform.position);
                    return candidateDistance < currentDistance ? candidate : current;
                case TargetingType.HighestHp:
                    return candidate.CurrentHp > current.CurrentHp ? candidate : current;
                case TargetingType.LowestHp:
                    return candidate.CurrentHp < current.CurrentHp ? candidate : current;
                case TargetingType.First:
                case TargetingType.Boss:
                default:
                    return candidate.transform.position.x < current.transform.position.x ? candidate : current;
            }
        }
    }
}
