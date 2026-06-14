using System;
using System.Collections;
using UnityEngine;

namespace RuneGate
{
    public sealed class CrystalController : MonoBehaviour
    {
        [SerializeField] private int defaultMaxHp = 100;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float hitFlashDuration = 0.12f;

        private int maxHp;
        private int currentHp;
        private bool initialized;
        private Color originalSpriteColor = Color.white;
        private Coroutine hitFlashRoutine;

        public event Action<int, int> HpChanged;
        public event Action<int, int, int> Damaged;
        public event Action Destroyed;

        public int MaxHp => maxHp;
        public int CurrentHp => currentHp;
        public bool IsDestroyed => initialized && currentHp <= 0;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            CaptureOriginalSpriteColor();

            if (!initialized)
            {
                Initialize(defaultMaxHp);
            }
        }

        public void Initialize(int hp)
        {
            maxHp = Mathf.Max(1, hp);
            currentHp = maxHp;
            initialized = true;
            CaptureOriginalSpriteColor();
            HpChanged?.Invoke(currentHp, maxHp);
        }

        public void TakeDamage(int damage)
        {
            if (!initialized)
            {
                Initialize(defaultMaxHp);
            }

            if (damage <= 0 || currentHp <= 0)
            {
                return;
            }

            currentHp = Mathf.Max(0, currentHp - damage);
            HpChanged?.Invoke(currentHp, maxHp);
            Damaged?.Invoke(damage, currentHp, maxHp);
            PlayHitFlash();

            if (currentHp <= 0)
            {
                Destroyed?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || currentHp <= 0)
            {
                return;
            }

            currentHp = Mathf.Min(maxHp, currentHp + amount);
            HpChanged?.Invoke(currentHp, maxHp);
        }

        private void CaptureOriginalSpriteColor()
        {
            if (spriteRenderer != null)
            {
                originalSpriteColor = spriteRenderer.color;
            }
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
            spriteRenderer.color = new Color(1f, 0.35f, 0.35f, 1f);
            yield return new WaitForSeconds(hitFlashDuration);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalSpriteColor;
            }

            hitFlashRoutine = null;
        }
    }
}
