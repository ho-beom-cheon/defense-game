using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class MonsterController : MonoBehaviour
    {
        [SerializeField] private MonsterData monsterData;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterVisualController visualController;
        [SerializeField] private HitFlashController hitFlashController;
        [SerializeField] private UnitMovementController movementController;
        [SerializeField] private AutoDestroyEffect hitEffectPrefab;
        [SerializeField] private AutoDestroyEffect deathEffectPrefab;
        [SerializeField] private DamageText damageTextPrefab;
        [SerializeField] private Transform hitEffectAnchor;
        [SerializeField] private float reachDistance = 0.08f;
        [SerializeField] private bool createRuntimeHpBar = true;
        [SerializeField] private Vector2 hpBarSize = new Vector2(0.7f, 0.08f);
        [SerializeField] private float hpBarYOffset = 0.48f;
        [SerializeField] private float hitFlashDuration = 0.08f;
        [SerializeField] private float deathDestroyDelay = 0.35f;
        [SerializeField] private float spawnPulseDuration = 0.18f;
        [SerializeField] private float bossSpawnPulseScale = 1.18f;
        [Header("Organic Movement")]
        [SerializeField] private float moveAcceleration = 6.5f;
        [SerializeField] private float moveDeceleration = 13f;
        [SerializeField] private float meleeStopDistance = 0.52f;
        [SerializeField] private float attackCooldownDuration = 1.05f;
        [SerializeField] private float monsterPersonalSpace = 0.36f;
        [SerializeField] private float attackWindUp = 0.2f;
        [SerializeField] private float attackRecovery = 0.32f;

        private static readonly List<MonsterController> activeMonsters = new List<MonsterController>();
        private int maxHp;
        private int currentHp;
        private int laneIndex;
        private Vector3 crystalTargetPosition;
        private CrystalController crystalController;
        private WaveManager ownerWaveManager;
        private LaneManager laneManager;
        private MonsterVariantType variantType = MonsterVariantType.Normal;
        private float speedMultiplier = 1f;
        private float bossMoveSpeedMultiplier = 1f;
        private float bossAttackIntervalMultiplier = 1f;
        private float bossAttackDamageMultiplier = 1f;
        private float attackCooldown;
        private bool initialized;
        private bool removedFromWave;
        private bool revivedOnce;
        private Color originalSpriteColor = Color.white;
        private Transform hpBarRoot;
        private PlaceholderSprite hpBarFill;
        private Coroutine hitFlashRoutine;
        private Coroutine deathRoutine;
        private Coroutine spawnPulseRoutine;
        private Coroutine attackRoutine;
        private MonsterCombatState combatState = MonsterCombatState.Spawn;

        public event Action<MonsterController> Died;
        public event Action<MonsterController> ReachedCrystal;
        public event Action<int, int> HpChanged;

        public MonsterData Data => monsterData;
        public int MaxHp => maxHp;
        public int CurrentHp => currentHp;
        public int LaneIndex => laneIndex;
        public bool IsBoss => monsterData != null && monsterData.IsBoss;
        public bool IsAlive => initialized && currentHp > 0 && !removedFromWave;
        public MonsterVariantType VariantType => variantType;
        public int RewardGold => ShadowContractService.GetRewardGold(monsterData, variantType);
        public SpriteRenderer VisualSpriteRenderer => spriteRenderer;
        public MonsterCombatState CombatState => combatState;
        public bool IsMovementAttackLocked => movementController != null && movementController.IsAttacking;
        public bool HasActiveAttackRoutine => attackRoutine != null;
        public BossPhaseController BossPhaseController { get; private set; }
        public static IReadOnlyList<MonsterController> ActiveMonsters => activeMonsters;

        private void OnEnable()
        {
            if (!activeMonsters.Contains(this))
            {
                activeMonsters.Add(this);
            }
        }

        private void OnDisable()
        {
            activeMonsters.Remove(this);
        }

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            AutoAssignFeedbackReferences();
            EnsureMovementController();
            CaptureOriginalSpriteColor();
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

            float speed = ShadowContractService.GetMoveSpeed(monsterData, variantType);
            Vector3 previousPosition = transform.position;
            HeroController blocker = FindBlockingHero();
            if (blocker != null)
            {
                SetCombatState(MonsterCombatState.Attack);
                movementController?.Stop();
                visualController?.FlipToward(blocker.transform.position);
                visualController?.PlayIdle();
                TryAttackHero(blocker);
            }
            else
            {
                SetCombatState(MonsterCombatState.Advance);
                MoveTowardCrystal(speed, previousPosition);
            }

            if (HasReachedCrystal())
            {
                DamageCrystalAndRemove();
            }
        }

        public void Initialize(MonsterData data, int assignedLaneIndex, Vector3 targetPosition, CrystalController targetCrystal, WaveManager waveManager)
        {
            Initialize(data, assignedLaneIndex, targetPosition, targetCrystal, waveManager, MonsterVariantType.Normal);
        }

        public void Initialize(MonsterData data, int assignedLaneIndex, Vector3 targetPosition, CrystalController targetCrystal, WaveManager waveManager, MonsterVariantType assignedVariantType)
        {
            if (data == null)
            {
                Debug.LogWarning($"{nameof(MonsterController)} on {name} cannot initialize because MonsterData is missing.");
                return;
            }

            monsterData = data;
            laneIndex = assignedLaneIndex;
            crystalTargetPosition = targetPosition;
            crystalController = targetCrystal;
            ownerWaveManager = waveManager;
            laneManager = FindAnyObjectByType<LaneManager>();
            variantType = data.IsBoss ? MonsterVariantType.Normal : assignedVariantType;
            maxHp = ShadowContractService.GetMaxHp(data, variantType);
            currentHp = maxHp;
            speedMultiplier = 1f;
            bossMoveSpeedMultiplier = 1f;
            bossAttackIntervalMultiplier = 1f;
            bossAttackDamageMultiplier = 1f;
            attackCooldown = 0f;
            removedFromWave = false;
            revivedOnce = false;
            initialized = true;
            SetCombatState(MonsterCombatState.Spawn);

            hpBarSize = RuntimeSpritePolicy.GetMonsterHpBarSize(data);
            hpBarYOffset = RuntimeSpritePolicy.GetMonsterHpBarYOffset(data);

            AutoAssignFeedbackReferences();
            EnsureMovementController();
            float targetHeight = RuntimeSpritePolicy.GetMonsterTargetHeight(data);
            movementController.Configure(ShadowContractService.GetMoveSpeed(data, variantType), meleeStopDistance, ResolvePersonalSpace(data, targetHeight), 2f);
            movementController.SetMotionTuning(moveAcceleration, moveDeceleration, reachDistance);
            ApplyRuntimeVisual(data);

            if (animator != null && data.AnimatorController != null)
            {
                animator.runtimeAnimatorController = data.AnimatorController;
            }

            visualController?.Initialize(data.Sprite, data.AnimatorController);
            visualController?.FlipByDirection(Vector3.left);
            ApplyVariantTint();
            AlignVisualToGround(true);
            CaptureOriginalSpriteColor();
            EnsureRuntimeHpBar();
            UpdateHpBar();
            PlaySpawnPulse();
            EnsureBossPhaseController();
            HpChanged?.Invoke(currentHp, maxHp);
        }

        private void ApplyRuntimeVisual(MonsterData data)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                return;
            }

            if (data.Sprite != null)
            {
                spriteRenderer.sprite = data.Sprite;
            }
            else
            {
                PlaceholderSprite placeholder = spriteRenderer.gameObject.GetComponent<PlaceholderSprite>();
                if (placeholder == null)
                {
                    placeholder = spriteRenderer.gameObject.AddComponent<PlaceholderSprite>();
                }

                float targetHeight = RuntimeSpritePolicy.GetMonsterTargetHeight(data);
                placeholder.Configure(RuntimeSpritePolicy.GetMonsterColor(data), new Vector2(targetHeight, targetHeight), spriteRenderer.sortingOrder);
            }

            RuntimeSpriteFitter fitter = spriteRenderer.gameObject.GetComponent<RuntimeSpriteFitter>();
            if (fitter == null)
            {
                fitter = spriteRenderer.gameObject.AddComponent<RuntimeSpriteFitter>();
            }

            fitter.TargetHeight = RuntimeSpritePolicy.GetMonsterTargetHeight(data);
            fitter.FitNow();
            AlignVisualToGround(false);
        }

        public void RefreshBoundsAnchors()
        {
            AlignVisualToGround(true);
            UpdateHpBarAnchor();
            UpdateHpBar();
        }

        private void AlignVisualToGround(bool recaptureRestPose)
        {
            RuntimeSpriteBoundsUtility.AlignVisualBottomToGround(spriteRenderer, transform, transform.position.y);
            if (recaptureRestPose)
            {
                visualController?.CaptureCurrentRestPose();
            }
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || !IsAlive)
            {
                return;
            }

            if (BossPhaseController != null)
            {
                damage = BossPhaseController.ClampIncomingDamageToPhaseGate(currentHp, maxHp, damage);
            }

            PlayHitFeedback(damage);
            SetCombatState(MonsterCombatState.Hit);
            CombatFeedbackEvents.RaiseUnitHit(transform.position);
            currentHp = Mathf.Max(0, currentHp - damage);
            HpChanged?.Invoke(currentHp, maxHp);
            UpdateHpBar();
            if (currentHp <= 0)
            {
                Die();
            }
        }

        public void ApplySlowPercent(float percent)
        {
            speedMultiplier = Mathf.Clamp(1f - percent, 0.1f, 1f);
        }

        public void ApplyBossPhaseModifiers(float moveSpeedMultiplier, float attackIntervalMultiplier, float attackDamageMultiplier)
        {
            if (!IsBoss)
            {
                return;
            }

            bossMoveSpeedMultiplier = Mathf.Clamp(moveSpeedMultiplier, 0.5f, 2.5f);
            bossAttackIntervalMultiplier = Mathf.Clamp(attackIntervalMultiplier, 0.35f, 2f);
            bossAttackDamageMultiplier = Mathf.Clamp(attackDamageMultiplier, 0.5f, 3f);
        }

        public void PlayBossPhaseFeedback(int phase)
        {
            if (!IsBoss || !IsAlive)
            {
                return;
            }

            PlaySpawnPulse();
            float targetHeight = RuntimeSpritePolicy.GetMonsterTargetHeight(monsterData);
            CombatVisualEffectFactory.SpawnHitSpark(GetEffectPosition(), targetHeight * Mathf.Clamp(0.9f + phase * 0.12f, 1f, 1.4f));
            CombatFeedbackEvents.RaiseAttackImpacted(transform.position);
        }

        private void Die()
        {
            if (removedFromWave)
            {
                return;
            }

            if (TryHandlePrototypeDeathHook())
            {
                return;
            }

            removedFromWave = true;
            SetCombatState(MonsterCombatState.Dead);
            movementController?.SetDeadState(true);
            StopAttackRoutine();
            Died?.Invoke(this);
            ownerWaveManager?.NotifyMonsterKilled(this);
            float targetHeight = RuntimeSpritePolicy.GetMonsterTargetHeight(monsterData);
            CombatVisualEffectFactory.SpawnDeathPuff(GetEffectPosition(), targetHeight);
            CombatFeedbackEvents.RaiseUnitDied(transform.position);
            AudioManager.Play(SfxKey.MonsterDeath);
            DisableColliders();
            if (hpBarRoot != null)
            {
                hpBarRoot.gameObject.SetActive(false);
            }

            float delay = Mathf.Max(0.08f, deathDestroyDelay);
            if (deathRoutine == null)
            {
                deathRoutine = StartCoroutine(DeathRoutine(delay));
            }
        }

        private void DamageCrystalAndRemove()
        {
            if (removedFromWave)
            {
                return;
            }

            removedFromWave = true;
            SetCombatState(MonsterCombatState.Dead);
            movementController?.SetDeadState(true);
            StopAttackRoutine();
            if (crystalController != null && monsterData != null)
            {
                crystalController.TakeDamage(ShadowContractService.GetDamageToCrystal(monsterData, variantType));
                CombatFeedbackEvents.RaiseCrystalDamaged(transform.position);
            }

            ReachedCrystal?.Invoke(this);
            ownerWaveManager?.NotifyMonsterRemoved(this);
            CombatVisualEffectFactory.SpawnHitSpark(transform.position, RuntimeSpritePolicy.GetMonsterTargetHeight(monsterData));
            Destroy(gameObject);
        }

        private void StopAttackRoutine()
        {
            if (attackRoutine == null)
            {
                return;
            }

            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        private void MoveTowardCrystal(float baseSpeed, Vector3 previousPosition)
        {
            EnsureMovementController();
            if (attackRoutine == null && movementController.IsAttacking)
            {
                movementController.SetAttackState(false);
            }

            movementController.Configure(baseSpeed * speedMultiplier * bossMoveSpeedMultiplier, meleeStopDistance, monsterPersonalSpace, 2f);
            float destinationX = ApplyMonsterSeparation(ResolveCrystalContactX());
            float laneY = ResolveLaneY();
            float minX = laneManager != null ? laneManager.GetMinCombatX() : Mathf.Min(crystalTargetPosition.x, transform.position.x) - 0.25f;
            float maxX = laneManager != null ? laneManager.GetMaxCombatX() : Mathf.Max(crystalTargetPosition.x, transform.position.x) + 0.25f;
            movementController.MoveToX(destinationX, laneY, minX, maxX);
            laneManager?.ClampUnitInsideBattlefield(transform, spriteRenderer);
            AlignVisualToGround(false);
            UpdateHpBarAnchor();

            Vector3 delta = transform.position - previousPosition;
            visualController?.FlipByDirection(delta.sqrMagnitude > 0.000001f ? delta : Vector3.left);
            if (delta.sqrMagnitude > 0.000001f)
            {
                visualController?.PlayMove();
            }
            else
            {
                visualController?.PlayIdle();
            }
        }

        private bool HasReachedCrystal()
        {
            float leadingEdgeX = spriteRenderer != null ? spriteRenderer.bounds.min.x : transform.position.x;
            return leadingEdgeX <= ResolveCrystalContactX() + Mathf.Max(0.01f, reachDistance);
        }

        private float ResolveCrystalContactX()
        {
            return laneManager != null ? laneManager.GetCrystalContactX(laneIndex) : crystalTargetPosition.x;
        }

        private void CaptureOriginalSpriteColor()
        {
            if (spriteRenderer != null)
            {
                originalSpriteColor = spriteRenderer.color;
            }
        }

        private void ApplyVariantTint()
        {
            if (spriteRenderer == null || variantType == MonsterVariantType.Normal)
            {
                return;
            }

            spriteRenderer.color = ShadowContractService.GetVariantTint(variantType);
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

            if (hitEffectAnchor == null)
            {
                hitEffectAnchor = transform;
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

        private void EnsureBossPhaseController()
        {
            if (!IsBoss)
            {
                BossPhaseController = null;
                return;
            }

            BossPhaseController = GetComponent<BossPhaseController>();
            if (BossPhaseController == null)
            {
                BossPhaseController = gameObject.AddComponent<BossPhaseController>();
            }

            BossPhaseController.Configure(this, ownerWaveManager);
        }

        private HeroController FindBlockingHero()
        {
            IReadOnlyList<HeroController> heroes = HeroController.ActiveHeroes;
            HeroController selected = null;
            float selectedDistance = float.MaxValue;
            for (int i = 0; i < heroes.Count; i++)
            {
                HeroController hero = heroes[i];
                if (hero == null || !hero.IsAlive || hero.LaneIndex != laneIndex)
                {
                    continue;
                }

                float distanceX = Mathf.Abs(transform.position.x - hero.transform.position.x);
                if (distanceX > ResolveMeleeStopDistance())
                {
                    continue;
                }

                if (distanceX < selectedDistance)
                {
                    selected = hero;
                    selectedDistance = distanceX;
                }
            }

            return selected;
        }

        private void TryAttackHero(HeroController hero)
        {
            if (hero == null || attackCooldown > 0f || attackRoutine != null)
            {
                return;
            }

            attackRoutine = StartCoroutine(AttackHeroRoutine(hero));
            attackCooldown = ResolveAttackCooldown();
        }

        private IEnumerator AttackHeroRoutine(HeroController hero)
        {
            movementController?.SetAttackState(true);
            Vector3 targetPosition = hero != null ? hero.transform.position : transform.position + Vector3.left;
            visualController?.FlipToward(targetPosition);
            visualController?.PlayAttackLunge(targetPosition);
            CombatFeedbackEvents.RaiseAttackStarted(transform.position);
            yield return new WaitForSeconds(Mathf.Max(0f, attackWindUp));

            if (hero != null && hero.IsAlive)
            {
                int damage = ResolveMonsterAttackDamage();
                visualController?.PlayImpactPause();
                CombatVisualEffectFactory.SpawnHitSpark(hero.transform.position, 1f);
                CombatFeedbackEvents.RaiseAttackImpacted(hero.transform.position);
                hero.TakeDamage(damage);
            }

            yield return new WaitForSeconds(Mathf.Max(0f, attackRecovery));
            movementController?.SetAttackState(false);
            attackRoutine = null;
        }

        private int ResolveMonsterAttackDamage()
        {
            int crystalDamage = ShadowContractService.GetDamageToCrystal(monsterData, variantType);
            if (monsterData != null && monsterData.IsBoss)
            {
                return Mathf.Max(8, Mathf.RoundToInt(crystalDamage * 2f * bossAttackDamageMultiplier));
            }

            return Mathf.Max(2, crystalDamage * 3);
        }

        private float ResolveAttackCooldown()
        {
            if (monsterData != null && monsterData.MonsterType == MonsterType.Fast)
            {
                return Mathf.Max(0.35f, attackCooldownDuration * 0.72f);
            }

            if (monsterData != null && monsterData.IsBoss)
            {
                return Mathf.Max(0.35f, attackCooldownDuration * 1.25f * bossAttackIntervalMultiplier);
            }

            return Mathf.Max(0.35f, attackCooldownDuration);
        }

        private float ResolveMeleeStopDistance()
        {
            float baseDistance = meleeStopDistance;
            if (monsterData != null && monsterData.IsBoss)
            {
                baseDistance += 0.42f;
            }
            else if (monsterData != null && monsterData.MonsterType == MonsterType.Tank)
            {
                baseDistance += 0.18f;
            }

            return Mathf.Max(0.2f, baseDistance);
        }

        private float ApplyMonsterSeparation(float desiredX)
        {
            float personalSpace = ResolvePersonalSpace(monsterData, RuntimeSpritePolicy.GetMonsterTargetHeight(monsterData));
            for (int i = 0; i < activeMonsters.Count; i++)
            {
                MonsterController other = activeMonsters[i];
                if (other == null || other == this || !other.IsAlive || other.LaneIndex != laneIndex)
                {
                    continue;
                }

                float gap = Mathf.Abs(transform.position.x - other.transform.position.x);
                if (gap >= personalSpace)
                {
                    continue;
                }

                if (transform.position.x >= other.transform.position.x)
                {
                    desiredX = Mathf.Max(desiredX, other.transform.position.x + personalSpace);
                }
                else
                {
                    desiredX = Mathf.Min(desiredX, other.transform.position.x - personalSpace * 0.5f);
                }
            }

            return desiredX;
        }

        private float ResolvePersonalSpace(MonsterData data, float targetHeight)
        {
            float resolved = Mathf.Max(monsterPersonalSpace, targetHeight * 0.32f);
            if (data != null && data.IsBoss)
            {
                return Mathf.Max(resolved, 1.05f);
            }

            if (data != null && data.MonsterType == MonsterType.Tank)
            {
                return Mathf.Max(resolved, 0.68f);
            }

            return resolved;
        }

        private float ResolveLaneY()
        {
            if (laneManager == null)
            {
                laneManager = FindAnyObjectByType<LaneManager>();
            }

            return laneManager != null ? laneManager.GetLaneY(laneIndex) : transform.position.y;
        }

        private void SetCombatState(MonsterCombatState nextState)
        {
            if (combatState == MonsterCombatState.Dead)
            {
                return;
            }

            combatState = nextState;
        }

        private void PlayHitFeedback(int damage)
        {
            visualController?.PlayHit();
            if (hitFlashController != null)
            {
                hitFlashController.Flash();
            }
            else
            {
                PlayHitFlash();
            }

            float targetHeight = RuntimeSpritePolicy.GetMonsterTargetHeight(monsterData);
            CombatVisualEffectFactory.SpawnHitSpark(GetEffectPosition(), targetHeight);
            SpawnDamageText(damage);
            AudioManager.Play(SfxKey.MonsterHit);
        }

        private Vector3 GetEffectPosition()
        {
            return hitEffectAnchor != null ? hitEffectAnchor.position : transform.position;
        }

        private void SpawnEffect(AutoDestroyEffect prefab, Vector3 position, Color fallbackColor, Vector2 fallbackSize, int sortingOrder)
        {
            if (prefab != null)
            {
                Instantiate(prefab, position, Quaternion.identity);
                return;
            }

            GameObject effectObject = new GameObject("Effect_Runtime");
            effectObject.transform.position = position;
            effectObject.AddComponent<SpriteRenderer>();
            PlaceholderSprite placeholderSprite = effectObject.AddComponent<PlaceholderSprite>();
            placeholderSprite.Configure(fallbackColor, fallbackSize, sortingOrder);
            effectObject.AddComponent<AutoDestroyEffect>();
        }

        private void SpawnDamageText(int damage)
        {
            float targetHeight = RuntimeSpritePolicy.GetMonsterTargetHeight(monsterData);
            Vector3 position = transform.position + new Vector3(0f, Mathf.Clamp(targetHeight * 0.78f, 0.72f, 2.1f), 0f);
            DamageText damageText = damageTextPrefab != null
                ? Instantiate(damageTextPrefab, position, Quaternion.identity)
                : new GameObject("DamageText_Runtime").AddComponent<DamageText>();
            damageText.Show(damage, position);
        }

        private IEnumerator DeathRoutine(float delay)
        {
            visualController?.PlayDeathCollapse(delay);
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }

        private void PlaySpawnPulse()
        {
            if (visualController == null || spawnPulseDuration <= 0f)
            {
                return;
            }

            Transform visualTransform = visualController.SpriteRenderer != null ? visualController.SpriteRenderer.transform : transform;
            if (visualTransform == null)
            {
                return;
            }

            if (spawnPulseRoutine != null)
            {
                StopCoroutine(spawnPulseRoutine);
            }

            spawnPulseRoutine = StartCoroutine(SpawnPulseRoutine(visualTransform));
        }

        private IEnumerator SpawnPulseRoutine(Transform visualTransform)
        {
            Vector3 baseScale = visualTransform.localScale;
            float peakMultiplier = IsBoss ? bossSpawnPulseScale : 1.08f;
            Vector3 peakScale = baseScale * Mathf.Max(1f, peakMultiplier);
            float elapsed = 0f;
            while (elapsed < spawnPulseDuration)
            {
                elapsed += Time.deltaTime;
                float percent = Mathf.Clamp01(elapsed / spawnPulseDuration);
                float curve = Mathf.Sin(percent * Mathf.PI);
                visualTransform.localScale = Vector3.Lerp(baseScale, peakScale, curve);
                yield return null;
            }

            visualTransform.localScale = baseScale;
            spawnPulseRoutine = null;
        }

        private void DisableColliders()
        {
            Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
        }

        private bool TryHandlePrototypeDeathHook()
        {
            if (monsterData == null)
            {
                return false;
            }

            switch (monsterData.MonsterType)
            {
                case MonsterType.Undead:
                    if (!revivedOnce && DifficultyRules.UndeadRevives(DifficultyRules.CurrentDifficultyId))
                    {
                        revivedOnce = true;
                        currentHp = Mathf.Max(1, maxHp / 2);
                        HpChanged?.Invoke(currentHp, maxHp);
                        UpdateHpBar();
                        Debug.Log($"{monsterData.DisplayName} revived once as an Undead prototype hook.");
                        return true;
                    }

                    break;
                case MonsterType.Splitter:
                    Debug.Log($"{monsterData.DisplayName} reached a Splitter death hook. Split spawn is reserved for a later prototype.");
                    break;
            }

            return false;
        }

        private void EnsureRuntimeHpBar()
        {
            if (!createRuntimeHpBar || hpBarRoot != null)
            {
                return;
            }

            GameObject root = new GameObject("HP Bar");
            root.transform.SetParent(transform);
            root.transform.localPosition = new Vector3(0f, hpBarYOffset, 0f);
            hpBarRoot = root.transform;

            GameObject background = new GameObject("HP Bar Background");
            background.transform.SetParent(hpBarRoot);
            background.transform.localPosition = Vector3.zero;
            background.AddComponent<SpriteRenderer>();
            PlaceholderSprite backgroundSprite = background.AddComponent<PlaceholderSprite>();
            backgroundSprite.Configure(new Color(0.08f, 0.08f, 0.08f, 0.9f), hpBarSize, 20);

            GameObject fill = new GameObject("HP Bar Fill");
            fill.transform.SetParent(hpBarRoot);
            fill.transform.localPosition = Vector3.zero;
            fill.AddComponent<SpriteRenderer>();
            hpBarFill = fill.AddComponent<PlaceholderSprite>();
            hpBarFill.Configure(new Color(0.35f, 0.95f, 0.35f, 1f), hpBarSize, 21);
        }

        private void UpdateHpBar()
        {
            if (hpBarFill == null)
            {
                return;
            }

            UpdateHpBarAnchor();
            float percent = maxHp > 0 ? Mathf.Clamp01((float)currentHp / maxHp) : 0f;
            Vector2 fillSize = new Vector2(Mathf.Max(0.01f, hpBarSize.x * percent), hpBarSize.y);
            hpBarFill.Configure(new Color(0.35f, 0.95f, 0.35f, 1f), fillSize, 21);
            hpBarFill.transform.localPosition = new Vector3(-hpBarSize.x * (1f - percent) * 0.5f, 0f, 0f);
        }

        private void UpdateHpBarAnchor()
        {
            if (hpBarRoot == null)
            {
                return;
            }

            float targetWorldY = transform.position.y + hpBarYOffset;
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                targetWorldY = spriteRenderer.bounds.max.y + Mathf.Clamp(RuntimeSpritePolicy.GetMonsterTargetHeight(monsterData) * 0.08f, 0.08f, 0.22f);
            }

            targetWorldY = RuntimeSpriteBoundsUtility.ClampWorldYInsideCamera(targetWorldY, 0.08f, 0.08f);
            hpBarRoot.position = new Vector3(transform.position.x, targetWorldY, transform.position.z);
        }

        private void PlayHitFlash()
        {
            if (spriteRenderer == null || hitFlashDuration <= 0f)
            {
                return;
            }

            if (hitFlashRoutine != null)
            {
                StopCoroutine(hitFlashRoutine);
            }

            hitFlashRoutine = StartCoroutine(HitFlashRoutine());
        }

        private IEnumerator HitFlashRoutine()
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(hitFlashDuration);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalSpriteColor;
            }

            hitFlashRoutine = null;
        }
    }
}
