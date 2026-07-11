using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class HeroController : MonoBehaviour
    {
        [SerializeField] private HeroData heroData;
        [SerializeField] private SkillController skillController;
        [SerializeField] private CharacterVisualController visualController;
        [SerializeField] private HitFlashController hitFlashController;
        [SerializeField] private UnitMovementController movementController;
        [SerializeField] private ProjectileController projectilePrefab;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private LayerMask monsterLayer = ~0;
        [SerializeField] private bool initializeOnAwake = true;
        [SerializeField] private int laneIndex = -1;
        [SerializeField] private int heroSlotIndex = -1;
        [Header("Lane Auto Move")]
        [SerializeField] private bool enableLaneAutoMove = true;
        [SerializeField] private float laneMoveSpeed = 1.85f;
        [SerializeField] private float laneSearchRangeBonus = 3.25f;
        [SerializeField] private float meleeStopDistance = 0.88f;
        [SerializeField] private float rangedStopDistanceMultiplier = 0.72f;
        [SerializeField] private float returnStopDistance = 0.05f;
        [SerializeField] private float laneLeashLeftX = -4.85f;
        [SerializeField] private float laneLeashRightX = 4.35f;
        [SerializeField] private float laneMoveAcceleration = 8.5f;
        [SerializeField] private float laneMoveDeceleration = 15f;
        [SerializeField] private float heroPersonalSpace = 0.58f;
        [SerializeField] private float rangedSafeBackOffset = 0.42f;
        [SerializeField] private float closeEnemyRetreatDistance = 1.25f;
        [SerializeField] private float meleeAttackWindUp = 0.18f;
        [SerializeField] private float meleeAttackRecovery = 0.32f;
        [SerializeField] private float rangedAttackWindUp = 0.14f;
        [SerializeField] private float rangedAttackRecovery = 0.22f;
        [SerializeField] private float targetLockDuration = 0.75f;

        private static readonly List<HeroController> activeHeroes = new List<HeroController>();
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
        private LaneManager laneManager;
        private Vector3 anchorPosition;
        private bool anchorCaptured;
        private HeroCombatState combatState = HeroCombatState.Idle;
        private MonsterController lockedTarget;
        private float targetLockRemaining;
        private Coroutine attackRoutine;

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
        public HeroCombatState CombatState => combatState;
        public static IReadOnlyList<HeroController> ActiveHeroes => activeHeroes;

        private void OnEnable()
        {
            if (!activeHeroes.Contains(this))
            {
                activeHeroes.Add(this);
            }
        }

        private void OnDisable()
        {
            activeHeroes.Remove(this);
        }

        private void Awake()
        {
            if (skillController == null)
            {
                skillController = GetComponent<SkillController>();
            }

            AutoAssignFeedbackReferences();
            EnsureMovementController();

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
            }

            if (targetLockRemaining > 0f)
            {
                targetLockRemaining -= Time.deltaTime;
            }

            if (attackRoutine != null)
            {
                visualController?.PlayIdle();
                return;
            }

            SetCombatState(HeroCombatState.Search);
            MonsterController target = ResolveTarget(GetSearchRange(), ResolveRoleTargetingType());
            if (target == null)
            {
                ReturnToAnchor();
                return;
            }

            float distanceToTarget = Mathf.Abs(transform.position.x - target.transform.position.x);
            float stopDistance = GetStopDistance();
            if (enableLaneAutoMove && ShouldRepositionForTarget(target, distanceToTarget, stopDistance))
            {
                MoveTowardTarget(target, stopDistance);
                return;
            }

            SetCombatState(HeroCombatState.HoldRange);
            visualController?.PlayIdle();
            if (attackCooldown > 0f)
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
            CaptureAnchorIfNeeded(true);

            if (skillController == null)
            {
                skillController = GetComponent<SkillController>();
            }

            AutoAssignFeedbackReferences();
            EnsureMovementController();
            movementController.Configure(laneMoveSpeed, attackRange, heroPersonalSpace, ResolveRoleLeashRange());
            movementController.SetMotionTuning(laneMoveAcceleration, laneMoveDeceleration, returnStopDistance);
            visualController?.Initialize(data.BattleSprite, data.AnimatorController);
            RefreshVisualAnchors(true);
            skillController?.Initialize(data.SkillData);
            HpChanged?.Invoke(currentHp, maxHp);
        }

        public void RefreshVisualAnchors(bool recaptureRestPose = false)
        {
            SpriteRenderer spriteRenderer = visualController != null ? visualController.SpriteRenderer : GetComponentInChildren<SpriteRenderer>();
            RuntimeSpriteBoundsUtility.AlignVisualBottomToGround(spriteRenderer, transform, ResolveLaneY());
            if (recaptureRestPose)
            {
                visualController?.CaptureCurrentRestPose();
            }
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
                SetCombatState(HeroCombatState.Skill);
                Vector3 feedbackTarget = target != null ? target.transform.position : transform.position + Vector3.right;
                visualController?.FlipToward(feedbackTarget);
                visualController?.PlaySkillPulse();
                CombatVisualEffectFactory.SpawnSkillEffect(skillController.Data, transform.position, feedbackTarget, target != null);
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
            SetCombatState(currentHp <= 0 ? HeroCombatState.Dead : HeroCombatState.Hit);
            movementController?.SetDeadState(currentHp <= 0);
            CombatFeedbackEvents.RaiseUnitHit(transform.position);
            hitFlashController?.Flash();
            visualController?.PlayHit();
            HpChanged?.Invoke(currentHp, maxHp);
            if (currentHp <= 0)
            {
                CombatFeedbackEvents.RaiseUnitDied(transform.position);
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
            CombatFeedbackEvents.RaiseUnitHealed(transform.position);
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
            CaptureAnchorIfNeeded(true);
        }

        private void BasicAttack(MonsterController target)
        {
            if (target == null || attackRoutine != null)
            {
                return;
            }

            attackRoutine = StartCoroutine(BasicAttackRoutine(target));
        }

        private IEnumerator BasicAttackRoutine(MonsterController target)
        {
            SetCombatState(HeroCombatState.Attack);
            movementController?.SetAttackState(true);
            Vector3 spawnPosition = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
            Vector3 targetPosition = target != null ? target.transform.position : transform.position + Vector3.right;
            visualController?.FlipToward(targetPosition);
            visualController?.PlayAttackLunge(targetPosition);
            CombatFeedbackEvents.RaiseAttackStarted(transform.position);
            AudioManager.Play(SfxKey.HeroAttack);

            float windUp = ShouldUseFallbackProjectile() || projectilePrefab != null ? rangedAttackWindUp : meleeAttackWindUp;
            yield return new WaitForSeconds(Mathf.Max(0f, windUp));

            if (target == null || !target.IsAlive)
            {
                yield return new WaitForSeconds(Mathf.Max(0f, rangedAttackRecovery));
                movementController?.SetAttackState(false);
                attackRoutine = null;
                yield break;
            }

            if (projectilePrefab != null)
            {
                ProjectileController projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
                projectile.Initialize(target, CalculateDamageAgainst(EffectiveAttack, target));
                yield return new WaitForSeconds(Mathf.Max(0f, rangedAttackRecovery));
                movementController?.SetAttackState(false);
                attackRoutine = null;
                yield break;
            }

            if (ShouldUseFallbackProjectile())
            {
                ProjectileController projectile = CreateFallbackProjectile(spawnPosition);
                projectile.Initialize(target, CalculateDamageAgainst(EffectiveAttack, target));
                yield return new WaitForSeconds(Mathf.Max(0f, rangedAttackRecovery));
                movementController?.SetAttackState(false);
                attackRoutine = null;
                yield break;
            }

            visualController?.PlayImpactPause();
            CombatFeedbackEvents.RaiseAttackImpacted(target.transform.position);
            target.TakeDamage(CalculateDamageAgainst(EffectiveAttack, target));
            yield return new WaitForSeconds(Mathf.Max(0f, meleeAttackRecovery));
            movementController?.SetAttackState(false);
            attackRoutine = null;
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

        private MonsterController ResolveTarget(float range, TargetingType targetingType)
        {
            if (lockedTarget != null && lockedTarget.IsAlive && targetLockRemaining > 0f && IsTargetStillValid(lockedTarget, range))
            {
                return lockedTarget;
            }

            MonsterController target = FindTarget(range, targetingType);
            lockedTarget = target;
            targetLockRemaining = target != null ? Mathf.Max(0f, targetLockDuration) : 0f;
            return target;
        }

        private bool IsTargetStillValid(MonsterController target, float range)
        {
            if (target == null || !target.IsAlive)
            {
                return false;
            }

            if (laneIndex >= 0 && target.LaneIndex != laneIndex)
            {
                return false;
            }

            return Mathf.Abs(transform.position.x - target.transform.position.x) <= Mathf.Max(range, attackRange) + 0.5f;
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

        private float GetSearchRange()
        {
            return Mathf.Max(attackRange, attackRange + Mathf.Max(0f, laneSearchRangeBonus));
        }

        private TargetingType ResolveRoleTargetingType()
        {
            if (heroData == null)
            {
                return TargetingType.First;
            }

            switch (heroData.Role)
            {
                case HeroRole.RangedDps:
                    return TargetingType.LowestHp;
                case HeroRole.Mage:
                case HeroRole.Assassin:
                    return TargetingType.HighestHp;
                case HeroRole.Tank:
                case HeroRole.Healer:
                case HeroRole.Engineer:
                case HeroRole.Support:
                case HeroRole.MeleeDps:
                default:
                    return TargetingType.First;
            }
        }

        private float GetStopDistance()
        {
            if (attackRange <= 2.25f)
            {
                return Mathf.Max(0.35f, meleeStopDistance);
            }

            return Mathf.Max(1f, attackRange * Mathf.Clamp01(rangedStopDistanceMultiplier));
        }

        private bool ShouldRepositionForTarget(MonsterController target, float distanceToTarget, float stopDistance)
        {
            if (target == null || heroData == null)
            {
                return false;
            }

            switch (heroData.Role)
            {
                case HeroRole.Healer:
                case HeroRole.Engineer:
                case HeroRole.Support:
                    return distanceToTarget < closeEnemyRetreatDistance || Mathf.Abs(transform.position.x - anchorPosition.x) > returnStopDistance;
                case HeroRole.RangedDps:
                case HeroRole.Mage:
                    return distanceToTarget < closeEnemyRetreatDistance || distanceToTarget > stopDistance;
                case HeroRole.Assassin:
                case HeroRole.Tank:
                case HeroRole.MeleeDps:
                default:
                    return distanceToTarget > stopDistance;
            }
        }

        private void MoveTowardTarget(MonsterController target, float stopDistance)
        {
            if (target == null)
            {
                ReturnToAnchor();
                return;
            }

            Vector3 current = transform.position;
            Vector3 targetPosition = target.transform.position;
            float desiredX = ResolveRoleDesiredX(target, targetPosition.x, stopDistance);
            desiredX = ApplyHeroSeparation(desiredX);
            float laneY = ResolveLaneY();
            Vector3 destination = new Vector3(desiredX, laneY, current.z);
            MoveAlongLane(destination);
        }

        private float ResolveRoleDesiredX(MonsterController target, float targetX, float stopDistance)
        {
            float distanceToTarget = Mathf.Abs(transform.position.x - targetX);
            float forwardLimit = Mathf.Min(laneLeashRightX, anchorPosition.x + ResolveRoleLeashRange());
            float backLimit = Mathf.Max(laneLeashLeftX, anchorPosition.x - ResolveRoleBackRange());
            if (laneManager != null)
            {
                forwardLimit = Mathf.Min(forwardLimit, laneManager.GetMaxCombatX());
                backLimit = Mathf.Max(backLimit, laneManager.GetMinCombatX());
            }

            if (heroData == null)
            {
                return Mathf.Clamp(targetX - stopDistance, backLimit, forwardLimit);
            }

            switch (heroData.Role)
            {
                case HeroRole.Healer:
                case HeroRole.Engineer:
                case HeroRole.Support:
                    SetCombatState(distanceToTarget < closeEnemyRetreatDistance ? HeroCombatState.Reposition : HeroCombatState.ReturnToAnchor);
                    return Mathf.Clamp(anchorPosition.x - rangedSafeBackOffset, backLimit, forwardLimit);
                case HeroRole.RangedDps:
                case HeroRole.Mage:
                    if (distanceToTarget < closeEnemyRetreatDistance)
                    {
                        SetCombatState(HeroCombatState.Reposition);
                        return Mathf.Clamp(anchorPosition.x - rangedSafeBackOffset, backLimit, forwardLimit);
                    }

                    SetCombatState(HeroCombatState.MoveToTarget);
                    return Mathf.Clamp(targetX - stopDistance, backLimit, forwardLimit);
                case HeroRole.Assassin:
                    SetCombatState(HeroCombatState.MoveToTarget);
                    return Mathf.Clamp(targetX - stopDistance * 0.65f, backLimit, forwardLimit);
                case HeroRole.Tank:
                case HeroRole.MeleeDps:
                default:
                    SetCombatState(HeroCombatState.MoveToTarget);
                    return Mathf.Clamp(targetX - stopDistance, backLimit, forwardLimit);
            }
        }

        private float ResolveRoleLeashRange()
        {
            if (heroData == null)
            {
                return 1.2f;
            }

            switch (heroData.Role)
            {
                case HeroRole.Tank:
                    return 1.65f;
                case HeroRole.Assassin:
                    return 1.35f;
                case HeroRole.MeleeDps:
                    return 1.1f;
                case HeroRole.RangedDps:
                    return 0.72f;
                case HeroRole.Mage:
                    return 0.55f;
                case HeroRole.Engineer:
                case HeroRole.Healer:
                case HeroRole.Support:
                default:
                    return 0.28f;
            }
        }

        private float ResolveRoleBackRange()
        {
            if (heroData == null)
            {
                return 0.45f;
            }

            switch (heroData.Role)
            {
                case HeroRole.RangedDps:
                case HeroRole.Mage:
                    return 0.85f;
                case HeroRole.Healer:
                case HeroRole.Engineer:
                case HeroRole.Support:
                    return 0.65f;
                case HeroRole.Assassin:
                    return 0.55f;
                case HeroRole.Tank:
                case HeroRole.MeleeDps:
                default:
                    return 0.35f;
            }
        }

        private float ApplyHeroSeparation(float desiredX)
        {
            for (int i = 0; i < activeHeroes.Count; i++)
            {
                HeroController other = activeHeroes[i];
                if (other == null || other == this || !other.IsAlive || other.LaneIndex != laneIndex)
                {
                    continue;
                }

                float gap = Mathf.Abs(desiredX - other.transform.position.x);
                if (gap >= heroPersonalSpace)
                {
                    continue;
                }

                float direction = heroSlotIndex <= other.heroSlotIndex ? -1f : 1f;
                if (Mathf.Approximately(direction, 0f))
                {
                    direction = desiredX <= other.transform.position.x ? -1f : 1f;
                }

                desiredX += direction * (heroPersonalSpace - gap) * 0.45f;
            }

            return desiredX;
        }

        private void ReturnToAnchor()
        {
            if (!enableLaneAutoMove)
            {
                SetCombatState(HeroCombatState.Idle);
                visualController?.PlayIdle();
                return;
            }

            CaptureAnchorIfNeeded(false);
            Vector3 destination = new Vector3(anchorPosition.x, ResolveLaneY(), transform.position.z);
            if (Vector2.Distance(transform.position, destination) <= returnStopDistance)
            {
                transform.position = destination;
                SetCombatState(HeroCombatState.Idle);
                movementController?.Stop();
                visualController?.PlayIdle();
                return;
            }

            SetCombatState(HeroCombatState.ReturnToAnchor);
            MoveAlongLane(destination);
        }

        private void MoveAlongLane(Vector3 destination)
        {
            Vector3 previousPosition = transform.position;
            EnsureMovementController();
            float minX = laneManager != null ? laneManager.GetMinCombatX() : laneLeashLeftX;
            float maxX = laneManager != null ? laneManager.GetMaxCombatX() : laneLeashRightX;
            movementController.MoveToX(destination.x, destination.y, minX, maxX);
            laneManager?.ClampUnitInsideBattlefield(transform, visualController != null ? visualController.SpriteRenderer : GetComponentInChildren<SpriteRenderer>());
            RefreshVisualAnchors();
            Vector3 delta = transform.position - previousPosition;
            if (delta.sqrMagnitude > 0.000001f)
            {
                visualController?.FlipByDirection(delta);
                visualController?.PlayMove();
            }
            else
            {
                visualController?.PlayIdle();
            }
        }

        private float ResolveLaneY()
        {
            if (laneManager == null)
            {
                laneManager = FindAnyObjectByType<LaneManager>();
            }

            return laneManager != null && laneIndex >= 0 ? laneManager.GetLaneY(laneIndex) : anchorPosition.y;
        }

        private void CaptureAnchorIfNeeded(bool force)
        {
            if (anchorCaptured && !force)
            {
                return;
            }

            anchorPosition = transform.position;
            if (laneIndex >= 0)
            {
                anchorPosition.y = ResolveLaneY();
            }

            anchorCaptured = true;
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

        private void EnsureMovementController()
        {
            if (movementController == null)
            {
                movementController = GetComponent<UnitMovementController>();
            }

            if (movementController == null)
            {
                movementController = gameObject.AddComponent<UnitMovementController>();
            }
        }

        private void SetCombatState(HeroCombatState nextState)
        {
            if (combatState == HeroCombatState.Dead)
            {
                return;
            }

            combatState = nextState;
        }

        private bool ShouldUseFallbackProjectile()
        {
            return attackRange > 2.25f;
        }

        private ProjectileController CreateFallbackProjectile(Vector3 spawnPosition)
        {
            GameObject projectileObject = new GameObject("Projectile_Runtime");
            projectileObject.transform.position = spawnPosition;
            SpriteRenderer spriteRenderer = projectileObject.AddComponent<SpriteRenderer>();
            Sprite projectileSprite = RuntimePixelAssetLoader.LoadSprite(RuntimePixelAssetLoader.EffectRapidShot);
            if (projectileSprite != null)
            {
                spriteRenderer.sprite = projectileSprite;
                spriteRenderer.color = new Color(1f, 0.92f, 0.38f, 0.95f);
                spriteRenderer.sortingOrder = 12;
                projectileObject.transform.localScale = new Vector3(0.36f, 0.18f, 1f);
            }
            else
            {
                PlaceholderSprite placeholderSprite = projectileObject.AddComponent<PlaceholderSprite>();
                placeholderSprite.Configure(new Color(1f, 0.85f, 0.25f, 1f), new Vector2(0.18f, 0.08f), 8);
            }

            return projectileObject.AddComponent<ProjectileController>();
        }

        private MonsterController PickBetterTarget(MonsterController current, MonsterController candidate, TargetingType targetingType)
        {
            if (current == null)
            {
                return candidate;
            }

            if (heroData != null && heroData.Role == HeroRole.RangedDps)
            {
                bool currentFast = IsFastPriorityTarget(current);
                bool candidateFast = IsFastPriorityTarget(candidate);
                if (candidateFast != currentFast)
                {
                    return candidateFast ? candidate : current;
                }
            }

            if (heroData != null && heroData.Role == HeroRole.Assassin)
            {
                bool currentBoss = current.Data != null && current.Data.MonsterType == MonsterType.Boss;
                bool candidateBoss = candidate.Data != null && candidate.Data.MonsterType == MonsterType.Boss;
                if (candidateBoss != currentBoss)
                {
                    return candidateBoss ? candidate : current;
                }
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

        private static bool IsFastPriorityTarget(MonsterController monster)
        {
            if (monster == null || monster.Data == null)
            {
                return false;
            }

            return monster.Data.MonsterType == MonsterType.Fast || monster.Data.MonsterType == MonsterType.Flying;
        }
    }
}
