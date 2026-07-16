using System;
using System.Collections;
using UnityEngine;

namespace RuneGate
{
    public sealed class CrystalController : MonoBehaviour
    {
        [SerializeField] private int defaultMaxHp = 100;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private HitFlashController hitFlashController;
        [SerializeField] private float hitFlashDuration = 0.12f;

        private int maxHp;
        private int currentHp;
        private int shieldHp;
        private bool initialized;
        private Color originalSpriteColor = Color.white;
        private Coroutine hitFlashRoutine;

        public event Action<int, int> HpChanged;
        public event Action<int> ShieldChanged;
        public event Action<int, int, int> Damaged;
        public event Action Destroyed;

        public int MaxHp => maxHp;
        public int CurrentHp => currentHp;
        public int ShieldHp => shieldHp;
        public bool IsDestroyed => initialized && currentHp <= 0;

        public void BindVisual(SpriteRenderer renderer, HitFlashController feedbackController)
        {
            spriteRenderer = renderer;
            hitFlashController = feedbackController;
            CaptureOriginalSpriteColor();
        }

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (hitFlashController == null)
            {
                hitFlashController = GetComponentInChildren<HitFlashController>();
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
            shieldHp = 0;
            initialized = true;
            CaptureOriginalSpriteColor();
            HpChanged?.Invoke(currentHp, maxHp);
            ShieldChanged?.Invoke(shieldHp);
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

            if (shieldHp > 0)
            {
                int absorbedDamage = Mathf.Min(shieldHp, damage);
                shieldHp -= absorbedDamage;
                damage -= absorbedDamage;
                ShieldChanged?.Invoke(shieldHp);
            }

            if (damage <= 0)
            {
                PlayDamageFeedback();
                AudioManager.Play(SfxKey.CrystalHit);
                return;
            }

            currentHp = Mathf.Max(0, currentHp - damage);
            HpChanged?.Invoke(currentHp, maxHp);
            Damaged?.Invoke(damage, currentHp, maxHp);
            PlayDamageFeedback();

            AudioManager.Play(SfxKey.CrystalHit);

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

        public void AddShield(int amount)
        {
            if (amount <= 0 || currentHp <= 0)
            {
                return;
            }

            shieldHp = Mathf.Clamp(shieldHp + amount, 0, maxHp);
            ShieldChanged?.Invoke(shieldHp);
            CombatFeedbackEvents.RaiseUnitHealed(transform.position);
        }

        private void PlayDamageFeedback()
        {
            if (hitFlashController != null)
            {
                hitFlashController.Flash(new Color(1f, 0.35f, 0.35f, 1f), hitFlashDuration);
            }
            else
            {
                PlayHitFlash();
            }
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
            yield return new WaitForSecondsRealtime(hitFlashDuration);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalSpriteColor;
            }

            hitFlashRoutine = null;
        }
    }
}
