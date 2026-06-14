using System;
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

            if (target == null || !target.IsAlive)
            {
                Debug.Log($"Skill {skillData.DisplayName} had no valid monster target.");
                return false;
            }

            int totalDamage = Mathf.Max(0, skillData.Power) * Mathf.Max(1, skillData.DamageHitCount);
            target.TakeDamage(totalDamage);
            StartCooldown();
            return true;
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
