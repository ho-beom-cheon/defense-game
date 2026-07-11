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

            bool applied = ApplySkillEffect(caster, target);
            if (!applied)
            {
                return false;
            }

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

        private bool ApplySkillEffect(HeroController caster, MonsterController target)
        {
            string effectKey = string.IsNullOrWhiteSpace(skillData.EffectKey) ? "damage" : skillData.EffectKey;
            switch (effectKey)
            {
                case "damage":
                case "direct_damage":
                case "multi_hit_damage":
                    return ApplyDirectDamage(caster, target);
                case "area_damage":
                    return ApplyAreaDamage(caster, target);
                case "crystal_heal_flat":
                    return ApplyCrystalHeal(caster);
                case "turret_placeholder":
                    Debug.Log("Deploy Turret is reserved as a placement-system hook. Applying prototype damage if a target exists.");
                    return target != null && target.IsAlive ? ApplyDirectDamage(caster, target) : ApplyCrystalHeal(caster);
                default:
                    Debug.Log($"Skill effect '{effectKey}' is reserved as a prototype hook. Falling back to direct damage.");
                    return ApplyDirectDamage(caster, target);
            }
        }

        private bool ApplyDirectDamage(HeroController caster, MonsterController target)
        {
            if (target == null || !target.IsAlive)
            {
                Debug.Log($"Skill {skillData.DisplayName} had no valid monster target.");
                return false;
            }

            int totalDamage = Mathf.Max(0, skillData.Power) * Mathf.Max(1, skillData.DamageHitCount);
            if (caster != null)
            {
                totalDamage = caster.CalculateDamageAgainst(totalDamage, target);
            }

            target.TakeDamage(totalDamage);
            return true;
        }

        private bool ApplyAreaDamage(HeroController caster, MonsterController target)
        {
            if (target == null || !target.IsAlive)
            {
                Debug.Log($"Skill {skillData.DisplayName} had no valid area target.");
                return false;
            }

            int baseDamage = Mathf.Max(0, skillData.Power) * Mathf.Max(1, skillData.DamageHitCount);
            float radius = Mathf.Max(0.1f, skillData.Radius);
            Collider2D[] hits = Physics2D.OverlapCircleAll(target.transform.position, radius);
            HashSet<MonsterController> damagedMonsters = new HashSet<MonsterController>();

            for (int i = 0; i < hits.Length; i++)
            {
                MonsterController monster = hits[i].GetComponentInParent<MonsterController>();
                if (monster == null || !monster.IsAlive || damagedMonsters.Contains(monster))
                {
                    continue;
                }

                int damage = caster != null ? caster.CalculateDamageAgainst(baseDamage, monster) : baseDamage;
                monster.TakeDamage(damage);
                damagedMonsters.Add(monster);
            }

            if (damagedMonsters.Count == 0)
            {
                int damage = caster != null ? caster.CalculateDamageAgainst(baseDamage, target) : baseDamage;
                target.TakeDamage(damage);
            }

            return true;
        }

        private bool ApplyCrystalHeal(HeroController caster)
        {
            CrystalController crystalController = FindAnyObjectByType<CrystalController>();
            if (crystalController == null)
            {
                Debug.LogWarning($"Skill {skillData.DisplayName} could not find a CrystalController to heal.");
                return false;
            }

            float healingMultiplier = caster != null ? caster.HealingMultiplier : 1f;
            int healAmount = Mathf.Max(1, Mathf.RoundToInt(Mathf.Max(1, skillData.Power) * healingMultiplier));
            crystalController.Heal(healAmount);
            CombatFeedbackEvents.RaiseUnitHealed(crystalController.transform.position);
            return true;
        }
    }
}
