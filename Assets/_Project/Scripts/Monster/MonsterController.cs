using System;
using System.Collections;
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

        private int maxHp;
        private int currentHp;
        private int laneIndex;
        private Vector3 crystalTargetPosition;
        private CrystalController crystalController;
        private WaveManager ownerWaveManager;
        private float speedMultiplier = 1f;
        private bool initialized;
        private bool removedFromWave;
        private bool revivedOnce;
        private Color originalSpriteColor = Color.white;
        private Transform hpBarRoot;
        private PlaceholderSprite hpBarFill;
        private Coroutine hitFlashRoutine;

        public event Action<MonsterController> Died;
        public event Action<MonsterController> ReachedCrystal;
        public event Action<int, int> HpChanged;

        public MonsterData Data => monsterData;
        public int MaxHp => maxHp;
        public int CurrentHp => currentHp;
        public int LaneIndex => laneIndex;
        public bool IsBoss => monsterData != null && monsterData.IsBoss;
        public bool IsAlive => initialized && currentHp > 0 && !removedFromWave;

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
            CaptureOriginalSpriteColor();
        }

        private void Update()
        {
            if (!IsAlive)
            {
                return;
            }

            float speed = monsterData != null ? monsterData.MoveSpeed : 0f;
            Vector3 previousPosition = transform.position;
            transform.position = Vector3.MoveTowards(transform.position, crystalTargetPosition, speed * speedMultiplier * Time.deltaTime);
            visualController?.FlipByDirection(transform.position - previousPosition);
            visualController?.PlayMove();

            if (Vector3.Distance(transform.position, crystalTargetPosition) <= reachDistance)
            {
                DamageCrystalAndRemove();
            }
        }

        public void Initialize(MonsterData data, int assignedLaneIndex, Vector3 targetPosition, CrystalController targetCrystal, WaveManager waveManager)
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
            maxHp = Mathf.Max(1, data.MaxHp);
            currentHp = maxHp;
            speedMultiplier = 1f;
            removedFromWave = false;
            revivedOnce = false;
            initialized = true;

            if (data.IsBoss)
            {
                hpBarSize = new Vector2(1.2f, 0.12f);
                hpBarYOffset = 1.32f;
            }
            else
            {
                hpBarYOffset = RuntimeSpritePolicy.GetMonsterTargetHeight(data) * 0.58f;
            }

            AutoAssignFeedbackReferences();
            ApplyRuntimeVisual(data);

            if (animator != null && data.AnimatorController != null)
            {
                animator.runtimeAnimatorController = data.AnimatorController;
            }

            visualController?.Initialize(data.Sprite, data.AnimatorController);
            CaptureOriginalSpriteColor();
            EnsureRuntimeHpBar();
            UpdateHpBar();
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
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || !IsAlive)
            {
                return;
            }

            PlayHitFeedback(damage);
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
            Died?.Invoke(this);
            ownerWaveManager?.NotifyMonsterKilled(this);
            visualController?.PlayDeath();
            SpawnEffect(deathEffectPrefab, GetEffectPosition(), new Color(0.7f, 0.7f, 0.7f, 0.85f), new Vector2(0.78f, 0.78f), 6);
            AudioManager.Play(SfxKey.MonsterDeath);
            DisableColliders();
            Destroy(gameObject, Mathf.Max(0.05f, deathDestroyDelay));
        }

        private void DamageCrystalAndRemove()
        {
            if (removedFromWave)
            {
                return;
            }

            removedFromWave = true;
            if (crystalController != null && monsterData != null)
            {
                crystalController.TakeDamage(monsterData.DamageToCrystal);
            }

            ReachedCrystal?.Invoke(this);
            ownerWaveManager?.NotifyMonsterRemoved(this);
            Destroy(gameObject);
        }

        private void CaptureOriginalSpriteColor()
        {
            if (spriteRenderer != null)
            {
                originalSpriteColor = spriteRenderer.color;
            }
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

            SpawnEffect(hitEffectPrefab, GetEffectPosition(), new Color(1f, 0.95f, 0.45f, 0.9f), new Vector2(0.42f, 0.42f), 7);
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
            Vector3 position = transform.position + new Vector3(0f, 0.72f, 0f);
            DamageText damageText = damageTextPrefab != null
                ? Instantiate(damageTextPrefab, position, Quaternion.identity)
                : new GameObject("DamageText_Runtime").AddComponent<DamageText>();
            damageText.Show(damage, position);
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
                    if (!revivedOnce)
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

            float percent = maxHp > 0 ? Mathf.Clamp01((float)currentHp / maxHp) : 0f;
            Vector2 fillSize = new Vector2(Mathf.Max(0.01f, hpBarSize.x * percent), hpBarSize.y);
            hpBarFill.Configure(new Color(0.35f, 0.95f, 0.35f, 1f), fillSize, 21);
            hpBarFill.transform.localPosition = new Vector3(-hpBarSize.x * (1f - percent) * 0.5f, 0f, 0f);
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
