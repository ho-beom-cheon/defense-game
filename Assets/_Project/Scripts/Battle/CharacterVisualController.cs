using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class CharacterVisualController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private string movingParameter = "IsMoving";
        [SerializeField] private string deadParameter = "IsDead";
        [SerializeField] private string attackTrigger = "Attack";
        [SerializeField] private string hitTrigger = "Hit";
        [SerializeField] private string skillTrigger = "Skill";
        [SerializeField] private string deathTrigger = "Death";
        [SerializeField] private float attackLungeDistance = 0.18f;
        [SerializeField] private float attackLungeDuration = 0.12f;
        [SerializeField] private float skillPulseScale = 1.1f;
        [SerializeField] private float skillPulseDuration = 0.18f;
        [SerializeField] private float idleBobAmplitude = 0.025f;
        [SerializeField] private float idleBobFrequency = 2.6f;
        [SerializeField] private float moveBobAmplitude = 0.055f;
        [SerializeField] private float moveBobFrequency = 7.5f;
        [SerializeField] private float hitShakeDistance = 0.055f;
        [SerializeField] private float hitShakeDuration = 0.08f;
        [SerializeField] private float impactPauseDuration = 0.045f;

        private readonly HashSet<int> animatorParameterHashes = new HashSet<int>();
        private RuntimeAnimatorController cachedController;
        private Coroutine attackLungeRoutine;
        private Coroutine skillPulseRoutine;
        private Coroutine hitShakeRoutine;
        private Coroutine deathCollapseRoutine;
        private float impactPauseUntil;
        private Vector3 restLocalPosition = Vector3.zero;
        private Vector3 restLocalScale = Vector3.one;
        private bool restPoseCaptured;
        private bool isMoving;

        public SpriteRenderer SpriteRenderer => spriteRenderer;
        public Animator Animator => animator;

        private void Update()
        {
            ApplyCodeMotion();
        }

        private void Awake()
        {
            AutoAssignReferences();
            CacheAnimatorParameters();
            CaptureRestPose(true);
        }

        public void Initialize(Sprite sprite, RuntimeAnimatorController animatorController)
        {
            AutoAssignReferences();

            if (spriteRenderer != null && sprite != null)
            {
                spriteRenderer.sprite = sprite;
            }

            if (animator != null && animatorController != null)
            {
                animator.runtimeAnimatorController = animatorController;
                CacheAnimatorParameters();
            }

            CaptureRestPose(true);
            PlayIdle();
        }

        public void CaptureCurrentRestPose()
        {
            CaptureRestPose(true);
        }

        public void PlayIdle()
        {
            SetMoving(false);
        }

        public void PlayMove()
        {
            SetMoving(true);
        }

        public void PlayAttack()
        {
            SetMoving(false);
            SetTrigger(attackTrigger);
        }

        public void PlayAttackLunge(Vector3 targetPosition)
        {
            PlayAttack();
            Transform visualTransform = GetVisualTransform();
            if (visualTransform == null || attackLungeDistance <= 0f || attackLungeDuration <= 0f)
            {
                return;
            }

            if (attackLungeRoutine != null)
            {
                StopCoroutine(attackLungeRoutine);
                visualTransform.localPosition = restLocalPosition;
            }

            Vector3 direction = targetPosition - transform.position;
            attackLungeRoutine = StartCoroutine(AttackLungeRoutine(Mathf.Sign(Mathf.Approximately(direction.x, 0f) ? 1f : direction.x)));
        }

        public void PlayHit()
        {
            SetTrigger(hitTrigger);
            PlayHitShake();
        }

        public void PlayImpactPause()
        {
            impactPauseUntil = Mathf.Max(impactPauseUntil, Time.time + Mathf.Max(0f, impactPauseDuration));
        }

        public void PlaySkill()
        {
            SetMoving(false);
            SetTrigger(skillTrigger);
        }

        public void PlaySkillPulse()
        {
            PlaySkill();
            Transform visualTransform = GetVisualTransform();
            if (visualTransform == null || skillPulseDuration <= 0f)
            {
                return;
            }

            if (skillPulseRoutine != null)
            {
                StopCoroutine(skillPulseRoutine);
                visualTransform.localScale = restLocalScale;
            }

            skillPulseRoutine = StartCoroutine(SkillPulseRoutine());
        }

        public void PlayDeath()
        {
            SetMoving(false);
            SetBool(deadParameter, true);
            SetTrigger(deathTrigger);
        }

        public void PlayDeathCollapse(float duration)
        {
            PlayDeath();
            Transform visualTransform = GetVisualTransform();
            if (visualTransform == null || duration <= 0f)
            {
                return;
            }

            if (deathCollapseRoutine != null)
            {
                StopCoroutine(deathCollapseRoutine);
            }

            deathCollapseRoutine = StartCoroutine(DeathCollapseRoutine(duration));
        }

        public void PlayHitShake()
        {
            Transform visualTransform = GetVisualTransform();
            if (visualTransform == null || hitShakeDistance <= 0f || hitShakeDuration <= 0f)
            {
                return;
            }

            if (hitShakeRoutine != null)
            {
                StopCoroutine(hitShakeRoutine);
                visualTransform.localPosition = restLocalPosition;
            }

            hitShakeRoutine = StartCoroutine(HitShakeRoutine());
        }

        public void FlipByDirection(Vector3 direction)
        {
            if (spriteRenderer == null || Mathf.Approximately(direction.x, 0f))
            {
                return;
            }

            spriteRenderer.flipX = direction.x < 0f;
        }

        public void FlipToward(Vector3 targetPosition)
        {
            FlipByDirection(targetPosition - transform.position);
        }

        private void AutoAssignReferences()
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

        private Transform GetVisualTransform()
        {
            AutoAssignReferences();
            return spriteRenderer != null ? spriteRenderer.transform : transform;
        }

        private void CaptureRestPose(bool force)
        {
            if (restPoseCaptured && !force)
            {
                return;
            }

            Transform visualTransform = GetVisualTransform();
            if (visualTransform == null)
            {
                return;
            }

            restLocalPosition = visualTransform.localPosition;
            restLocalScale = visualTransform.localScale;
            restPoseCaptured = true;
        }

        private IEnumerator AttackLungeRoutine(float directionX)
        {
            CaptureRestPose(false);
            Transform visualTransform = GetVisualTransform();
            if (visualTransform == null)
            {
                yield break;
            }

            Vector3 peakPosition = restLocalPosition + new Vector3(directionX * attackLungeDistance, 0f, 0f);
            float halfDuration = attackLungeDuration * 0.5f;
            yield return MoveVisualPosition(visualTransform, restLocalPosition, peakPosition, halfDuration);
            yield return MoveVisualPosition(visualTransform, peakPosition, restLocalPosition, halfDuration);
            attackLungeRoutine = null;
        }

        private IEnumerator SkillPulseRoutine()
        {
            CaptureRestPose(false);
            Transform visualTransform = GetVisualTransform();
            if (visualTransform == null)
            {
                yield break;
            }

            Vector3 peakScale = restLocalScale * Mathf.Max(1f, skillPulseScale);
            float halfDuration = skillPulseDuration * 0.5f;
            yield return MoveVisualScale(visualTransform, restLocalScale, peakScale, halfDuration);
            yield return MoveVisualScale(visualTransform, peakScale, restLocalScale, halfDuration);
            skillPulseRoutine = null;
        }

        private IEnumerator HitShakeRoutine()
        {
            CaptureRestPose(false);
            Transform visualTransform = GetVisualTransform();
            if (visualTransform == null)
            {
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < hitShakeDuration)
            {
                elapsed += Time.deltaTime;
                float sign = Mathf.Repeat(elapsed * 80f, 2f) < 1f ? -1f : 1f;
                visualTransform.localPosition = restLocalPosition + new Vector3(sign * hitShakeDistance, 0f, 0f);
                yield return null;
            }

            visualTransform.localPosition = restLocalPosition;
            hitShakeRoutine = null;
        }

        private IEnumerator DeathCollapseRoutine(float duration)
        {
            CaptureRestPose(false);
            Transform visualTransform = GetVisualTransform();
            if (visualTransform == null)
            {
                yield break;
            }

            Color startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
            Vector3 targetScale = restLocalScale * 0.25f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float percent = Mathf.Clamp01(elapsed / duration);
                visualTransform.localScale = Vector3.Lerp(restLocalScale, targetScale, percent);
                if (spriteRenderer != null)
                {
                    Color color = startColor;
                    color.a = Mathf.Lerp(startColor.a, 0f, percent);
                    spriteRenderer.color = color;
                }

                yield return null;
            }

            deathCollapseRoutine = null;
        }

        private static IEnumerator MoveVisualPosition(Transform visualTransform, Vector3 from, Vector3 to, float duration)
        {
            if (duration <= 0f)
            {
                visualTransform.localPosition = to;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                visualTransform.localPosition = Vector3.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            visualTransform.localPosition = to;
        }

        private static IEnumerator MoveVisualScale(Transform visualTransform, Vector3 from, Vector3 to, float duration)
        {
            if (duration <= 0f)
            {
                visualTransform.localScale = to;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                visualTransform.localScale = Vector3.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            visualTransform.localScale = to;
        }

        private void SetMoving(bool isMoving)
        {
            this.isMoving = isMoving;
            SetBool(movingParameter, isMoving);
        }

        private void ApplyCodeMotion()
        {
            Transform visualTransform = GetVisualTransform();
            if (visualTransform == null || !restPoseCaptured || attackLungeRoutine != null || hitShakeRoutine != null || deathCollapseRoutine != null || Time.time < impactPauseUntil)
            {
                return;
            }

            float amplitude = isMoving ? moveBobAmplitude : idleBobAmplitude;
            float frequency = isMoving ? moveBobFrequency : idleBobFrequency;
            if (amplitude <= 0f || frequency <= 0f)
            {
                return;
            }

            float bob = Mathf.Sin(Time.time * frequency) * amplitude;
            Vector3 nextPosition = restLocalPosition + new Vector3(0f, bob, 0f);
            visualTransform.localPosition = nextPosition;
        }

        private void SetBool(string parameterName, bool value)
        {
            if (!CanUseAnimatorParameter(parameterName))
            {
                return;
            }

            animator.SetBool(parameterName, value);
        }

        private void SetTrigger(string parameterName)
        {
            if (!CanUseAnimatorParameter(parameterName))
            {
                return;
            }

            animator.ResetTrigger(parameterName);
            animator.SetTrigger(parameterName);
        }

        private bool CanUseAnimatorParameter(string parameterName)
        {
            if (animator == null || animator.runtimeAnimatorController == null || string.IsNullOrWhiteSpace(parameterName))
            {
                return false;
            }

            CacheAnimatorParameters();
            return animatorParameterHashes.Contains(Animator.StringToHash(parameterName));
        }

        private void CacheAnimatorParameters()
        {
            if (animator == null || animator.runtimeAnimatorController == null || cachedController == animator.runtimeAnimatorController)
            {
                return;
            }

            cachedController = animator.runtimeAnimatorController;
            animatorParameterHashes.Clear();
            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                animatorParameterHashes.Add(parameters[i].nameHash);
            }
        }
    }
}
