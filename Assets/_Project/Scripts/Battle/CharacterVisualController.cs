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

        private readonly HashSet<int> animatorParameterHashes = new HashSet<int>();
        private RuntimeAnimatorController cachedController;

        public SpriteRenderer SpriteRenderer => spriteRenderer;
        public Animator Animator => animator;

        private void Awake()
        {
            AutoAssignReferences();
            CacheAnimatorParameters();
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

            PlayIdle();
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

        public void PlayHit()
        {
            SetTrigger(hitTrigger);
        }

        public void PlaySkill()
        {
            SetMoving(false);
            SetTrigger(skillTrigger);
        }

        public void PlayDeath()
        {
            SetMoving(false);
            SetBool(deadParameter, true);
            SetTrigger(deathTrigger);
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

        private void SetMoving(bool isMoving)
        {
            SetBool(movingParameter, isMoving);
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
