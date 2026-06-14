using System;
using UnityEngine;

namespace RuneGate
{
    public sealed class MonsterController : MonoBehaviour
    {
        [SerializeField] private MonsterData monsterData;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private float reachDistance = 0.08f;

        private int currentHp;
        private int laneIndex;
        private Vector3 crystalTargetPosition;
        private CrystalController crystalController;
        private WaveManager ownerWaveManager;
        private float speedMultiplier = 1f;
        private bool initialized;
        private bool removedFromWave;

        public event Action<MonsterController> Died;
        public event Action<MonsterController> ReachedCrystal;

        public MonsterData Data => monsterData;
        public int CurrentHp => currentHp;
        public int LaneIndex => laneIndex;
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
        }

        private void Update()
        {
            if (!IsAlive)
            {
                return;
            }

            float speed = monsterData != null ? monsterData.MoveSpeed : 0f;
            transform.position = Vector3.MoveTowards(transform.position, crystalTargetPosition, speed * speedMultiplier * Time.deltaTime);

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
            currentHp = Mathf.Max(1, data.MaxHp);
            speedMultiplier = 1f;
            removedFromWave = false;
            initialized = true;

            if (spriteRenderer != null && data.Sprite != null)
            {
                spriteRenderer.sprite = data.Sprite;
            }

            if (animator != null && data.AnimatorController != null)
            {
                animator.runtimeAnimatorController = data.AnimatorController;
            }
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || !IsAlive)
            {
                return;
            }

            currentHp = Mathf.Max(0, currentHp - damage);
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

            removedFromWave = true;
            Died?.Invoke(this);
            ownerWaveManager?.NotifyMonsterKilled(this);
            Destroy(gameObject);
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
    }
}
