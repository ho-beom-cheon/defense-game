using System;
using UnityEngine;

namespace RuneGate
{
    public sealed class HeroController : MonoBehaviour
    {
        [SerializeField] private HeroData heroData;
        [SerializeField] private SkillController skillController;
        [SerializeField] private CharacterVisualController visualController;
        [SerializeField] private HitFlashController hitFlashController;
        [SerializeField] private ProjectileController projectilePrefab;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private LayerMask monsterLayer = ~0;
        [SerializeField] private bool initializeOnAwake = true;
        [SerializeField] private int laneIndex = -1;
        [SerializeField] private int heroSlotIndex = -1;

        private int currentHp;
        private int maxHp;
        private int baseAttack;
        private float baseAttackSpeed;
        private float attackRange;
        private float attackCooldown;
        private float attackMultiplier = 1f;
        private float attackSpeedMultiplier = 1f;
        private float hpMultiplier = 1f;
        private float bossDamageMultiplier = 1f;
        private float healingMultiplier = 1f;
        private bool initialized;

        public event Action<int, int> HpChanged;

        public HeroData Data => heroData;
        public SkillController SkillController => skillController;
        public int CurrentHp => currentHp;
        public int MaxHp => maxHp;
        public int EffectiveAttack => Mathf.Max(1, Mathf.RoundToInt(baseAttack * attackMultiplier));
        public float EffectiveAttackSpeed => Mathf.Max(0.01f, baseAttackSpeed * attackSpeedMultiplier);
        public float HealingMultiplier => healingMultiplier;
        public int LaneIndex => laneIndex;
        public int HeroSlotIndex => heroSlotIndex;
        public bool IsAlive => initialized && currentHp > 0;

        private void Awake()
        {
            if (skillController == null)
            {
                skillController = GetComponent<SkillController>();
            }

            AutoAssignFeedbackReferences();

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
            attackCooldown = 1f / EffectiveAttackSpeed;
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
            hpMultiplier = 1f;
            bossDamageMultiplier = 1f;
            healingMultiplier = 1f;
            initialized = true;

            if (skillController == null)
            {
                skillController = GetComponent<SkillController>();
            }

            AutoAssignFeedbackReferences();
            visualController?.Initialize(data.BattleSprite, data.AnimatorController);
            skillController?.Initialize(data.SkillData);
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
            if (skillController.UseSkill(this, target))
            {
                visualController?.FlipToward(target != null ? target.transform.position : transform.position + Vector3.right);
                visualController?.PlaySkill();
                AudioManager.Play(SfxKey.HeroAttack);
            }
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || !IsAlive)
            {
                return;
            }

            currentHp = Mathf.Max(0, currentHp - damage);
            hitFlashController?.Flash();
            visualController?.PlayHit();
            HpChanged?.Invoke(currentHp, maxHp);
            if (currentHp <= 0)
            {
                visualController?.PlayDeath();
            }
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

        public void ApplyHeroHpPercent(float percent)
        {
            if (heroData == null)
            {
                return;
            }

            float nextMultiplier = Mathf.Max(0.1f, hpMultiplier + percent);
            int previousMaxHp = maxHp;
            hpMultiplier = nextMultiplier;
            maxHp = Mathf.Max(1, Mathf.RoundToInt(heroData.MaxHp * hpMultiplier));
            currentHp = Mathf.Clamp(currentHp + Mathf.Max(0, maxHp - previousMaxHp), 0, maxHp);
            HpChanged?.Invoke(currentHp, maxHp);
        }

        public void ApplyBossDamagePercent(float percent)
        {
            bossDamageMultiplier = Mathf.Max(0.1f, bossDamageMultiplier + percent);
        }

        public void ApplyHealingPercent(float percent)
        {
            healingMultiplier = Mathf.Max(0.1f, healingMultiplier + percent);
        }

        public void ApplySkillCooldownPercent(float percent)
        {
            skillController?.ApplyCooldownPercent(percent);
        }

        public void SetLogicalPlacement(int assignedLaneIndex, int assignedSlotIndex)
        {
            laneIndex = assignedLaneIndex;
            heroSlotIndex = assignedSlotIndex;
        }

        private void BasicAttack(MonsterController target)
        {
            if (target == null)
            {
                return;
            }

            Vector3 spawnPosition = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
            visualController?.FlipToward(target.transform.position);
            visualController?.PlayAttack();
            AudioManager.Play(SfxKey.HeroAttack);

            if (projectilePrefab != null)
            {
                ProjectileController projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
                projectile.Initialize(target, CalculateDamageAgainst(EffectiveAttack, target));
                return;
            }

            if (ShouldUseFallbackProjectile())
            {
                ProjectileController projectile = CreateFallbackProjectile(spawnPosition);
                projectile.Initialize(target, CalculateDamageAgainst(EffectiveAttack, target));
                return;
            }

            target.TakeDamage(CalculateDamageAgainst(EffectiveAttack, target));
        }

        public int CalculateDamageAgainst(int baseDamage, MonsterController target)
        {
            float damage = Mathf.Max(0, baseDamage);
            if (target != null && target.Data != null && target.Data.MonsterType == MonsterType.Boss)
            {
                damage *= bossDamageMultiplier;
            }

            return Mathf.Max(0, Mathf.RoundToInt(damage));
        }

        private MonsterController FindTarget(float range, TargetingType targetingType)
        {
            float safeRange = Mathf.Max(0.1f, range);
            MonsterController selected = FindTargetInRange(safeRange, targetingType, true);
            if (selected == null)
            {
                selected = FindTargetInRange(safeRange, targetingType, false);
            }

            if (selected == null && targetingType == TargetingType.Boss)
            {
                return FindTarget(safeRange, TargetingType.Nearest);
            }

            return selected;
        }

        private MonsterController FindTargetInRange(float range, TargetingType targetingType, bool requireSameLane)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, monsterLayer);
            MonsterController selected = null;

            for (int i = 0; i < hits.Length; i++)
            {
                MonsterController monster = hits[i].GetComponentInParent<MonsterController>();
                if (monster == null || !monster.IsAlive)
                {
                    continue;
                }

                if (requireSameLane && laneIndex >= 0 && monster.LaneIndex != laneIndex)
                {
                    continue;
                }

                if (targetingType == TargetingType.Boss && monster.Data != null && monster.Data.MonsterType != MonsterType.Boss)
                {
                    continue;
                }

                selected = PickBetterTarget(selected, monster, targetingType);
            }

            return selected;
        }

        private void AutoAssignFeedbackReferences()
        {
            if (visualController == null)
            {
                visualController = GetComponentInChildren<CharacterVisualController>();
            }

            if (hitFlashController == null)
            {
                hitFlashController = GetComponentInChildren<HitFlashController>();
            }
        }

        private bool ShouldUseFallbackProjectile()
        {
            return attackRange > 2.25f;
        }

        private ProjectileController CreateFallbackProjectile(Vector3 spawnPosition)
        {
            GameObject projectileObject = new GameObject("Projectile_Runtime");
            projectileObject.transform.position = spawnPosition;
            projectileObject.AddComponent<SpriteRenderer>();
            PlaceholderSprite placeholderSprite = projectileObject.AddComponent<PlaceholderSprite>();
            placeholderSprite.Configure(new Color(1f, 0.85f, 0.25f, 1f), new Vector2(0.18f, 0.08f), 8);
            return projectileObject.AddComponent<ProjectileController>();
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
