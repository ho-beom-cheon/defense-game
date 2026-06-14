using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class SkillController : MonoBehaviour
    {
        [SerializeField] private SkillData skillData;

        private float cooldownRemaining;
        private float cooldownMultiplier = 1f;

        public event Action<float, float> CooldownChanged;

        public SkillData Data => skillData;
        public bool CanUseSkill => skillData != null && cooldownRemaining <= 0f;
        public float CooldownRemaining => cooldownRemaining;
        public float CooldownDuration => skillData != null ? skillData.Cooldown * cooldownMultiplier : 0f;
        public float Range => skillData != null ? skillData.Range : 0f;
        public TargetingType TargetingType => skillData != null ? skillData.TargetingType : TargetingType.First;

        private void Update()
        {
            if (cooldownRemaining <= 0f)
            {
                return;
            }

            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);
            CooldownChanged?.Invoke(cooldownRemaining, CooldownDuration);
        }

        public void Initialize(SkillData data)
        {
            skillData = data;
            cooldownRemaining = 0f;
            cooldownMultiplier = 1f;
            CooldownChanged?.Invoke(cooldownRemaining, CooldownDuration);
        }

        public bool UseSkill(HeroController caster, MonsterController target)
        {
            if (skillData == null)
            {
                Debug.LogWarning($"{nameof(SkillController)} on {name} cannot use skill because SkillData is missing.");
                return false;
            }

            if (!CanUseSkill)
            {
                return false;
            }

            if (target != null && target.IsAlive)
            {
                target.TakeDamage(Mathf.Max(0, skillData.Power));
            }
            else if (caster != null)
            {
                // Healing behavior is intentionally a hook until skills have a dedicated type field.
                caster.Heal(Mathf.Max(0, skillData.Power));
                Debug.Log($"Skill {skillData.DisplayName} used heal placeholder because no valid monster target was found.");
            }
            else
            {
                Debug.LogWarning($"Skill {skillData.DisplayName} had no caster or target.");
                return false;
            }

            StartCooldown();
            return true;
        }

        public bool UseAreaDamagePlaceholder(IEnumerable<MonsterController> targets)
        {
            if (skillData == null || !CanUseSkill)
            {
                return false;
            }

            bool hitAnyTarget = false;
            foreach (MonsterController target in targets)
            {
                if (target != null && target.IsAlive)
                {
                    target.TakeDamage(Mathf.Max(0, skillData.Power));
                    hitAnyTarget = true;
                }
            }

            if (hitAnyTarget)
            {
                StartCooldown();
            }

            return hitAnyTarget;
        }

        public void ApplyCooldownPercent(float percent)
        {
            cooldownMultiplier = Mathf.Clamp(cooldownMultiplier * (1f - percent), 0.1f, 10f);
            CooldownChanged?.Invoke(cooldownRemaining, CooldownDuration);
        }

        private void StartCooldown()
        {
            cooldownRemaining = CooldownDuration;
            CooldownChanged?.Invoke(cooldownRemaining, CooldownDuration);
        }
    }
}
