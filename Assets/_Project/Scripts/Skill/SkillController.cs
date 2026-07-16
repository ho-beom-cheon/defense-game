using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class SkillController : MonoBehaviour
    {
        public const string ShieldBashEffect = "shield_bash";
        public const string RapidShotEffect = "rapid_shot";
        public const string MeteorAreaEffect = "meteor_area";
        public const string HolyHealEffect = "holy_heal";
        public const string TemporaryTurretEffect = "temporary_turret";
        public const string ShadowStrikeEffect = "shadow_strike";

        [SerializeField] private SkillData skillData;

        private float cooldownRemaining;
        private float cooldownMultiplier = 1f;
        private int successfulCastCount;
        private int resolvedHitCount;

        public event Action<float, float> CooldownChanged;

        public SkillData Data => skillData;
        public bool CanUseSkill => skillData != null && cooldownRemaining <= 0f;
        public float CooldownRemaining => cooldownRemaining;
        public float CooldownDuration => skillData != null ? skillData.Cooldown * cooldownMultiplier : 0f;
        public float Range => skillData != null ? skillData.Range : 0f;
        public TargetingType TargetingType => skillData != null ? skillData.TargetingType : TargetingType.First;
        public int SuccessfulCastCount => successfulCastCount;
        public int ResolvedHitCount => resolvedHitCount;
        public string ActiveEffectKey => skillData != null ? skillData.EffectKey : string.Empty;

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
            successfulCastCount = 0;
            resolvedHitCount = 0;
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

            successfulCastCount++;
            StartCooldown();
            return true;
        }

        public void ApplyCooldownPercent(float percent)
        {
            cooldownMultiplier = Mathf.Clamp(cooldownMultiplier * (1f - percent), 0.1f, 10f);
            CooldownChanged?.Invoke(cooldownRemaining, CooldownDuration);
        }

        public static bool IsHeroSkillEffectKey(string effectKey)
        {
            switch (effectKey)
            {
                case ShieldBashEffect:
                case RapidShotEffect:
                case MeteorAreaEffect:
                case HolyHealEffect:
                case TemporaryTurretEffect:
                case ShadowStrikeEffect:
                    return true;
                default:
                    return false;
            }
        }

        public static int CalculateShadowStrikeDamage(int baseDamage, int currentHp, int maxHp, bool isBoss)
        {
            float multiplier = isBoss ? 1.3f : 1f;
            if (maxHp > 0 && currentHp <= Mathf.CeilToInt(maxHp * 0.35f))
            {
                multiplier += 0.35f;
            }

            return Mathf.Max(0, Mathf.RoundToInt(Mathf.Max(0, baseDamage) * multiplier));
        }

        public static int CalculateHeroHeal(int power, float healingMultiplier)
        {
            return Mathf.Max(1, Mathf.RoundToInt(Mathf.Max(1, power) * Mathf.Max(0.1f, healingMultiplier)));
        }

        public static int CalculateCrystalHeal(int heroHeal)
        {
            return Mathf.Max(1, Mathf.RoundToInt(Mathf.Max(1, heroHeal) * 0.55f));
        }

        private void StartCooldown()
        {
            cooldownRemaining = CooldownDuration;
            CooldownChanged?.Invoke(cooldownRemaining, CooldownDuration);
        }

        private bool ApplySkillEffect(HeroController caster, MonsterController target)
        {
            string effectKey = skillData.EffectKey;
            switch (effectKey)
            {
                case ShieldBashEffect:
                    return ApplyShieldBash(caster, target);
                case RapidShotEffect:
                    return ApplyRapidShot(caster, target);
                case MeteorAreaEffect:
                    return ApplyAreaDamage(caster, target);
                case HolyHealEffect:
                    return ApplyHolyHeal(caster);
                case TemporaryTurretEffect:
                    return DeployTemporaryTurret(caster);
                case ShadowStrikeEffect:
                    return ApplyShadowStrike(caster, target);
                default:
                    Debug.LogWarning($"Skill {skillData.DisplayName} uses unsupported effect key '{effectKey}'.");
                    return false;
            }
        }

        private bool ApplyShieldBash(HeroController caster, MonsterController target)
        {
            if (!ApplyDirectDamage(caster, target, skillData.Power))
            {
                return false;
            }

            target.ApplyKnockback(
                caster != null ? (Vector2)caster.transform.position : (Vector2)target.transform.position + Vector2.left,
                0.82f,
                0.65f);
            return true;
        }

        private bool ApplyRapidShot(HeroController caster, MonsterController target)
        {
            if (target == null || !target.IsAlive)
            {
                Debug.Log($"Skill {skillData.DisplayName} had no valid monster target.");
                return false;
            }

            StartCoroutine(RapidShotRoutine(caster, target, Mathf.Max(1, skillData.DamageHitCount)));
            return true;
        }

        private IEnumerator RapidShotRoutine(HeroController caster, MonsterController target, int hitCount)
        {
            for (int i = 0; i < hitCount; i++)
            {
                if (target == null || !target.IsAlive)
                {
                    yield break;
                }

                if (!IsTargetInSkillRange(caster, target))
                {
                    yield break;
                }

                ApplyDirectDamage(caster, target, skillData.Power);
                CombatVisualEffectFactory.SpawnRapidShotImpact(
                    caster != null ? caster.transform.position : transform.position,
                    target.transform.position);

                if (i < hitCount - 1)
                {
                    yield return new WaitForSeconds(0.12f);
                }
            }
        }

        private bool ApplyDirectDamage(HeroController caster, MonsterController target, int baseDamage)
        {
            if (target == null || !target.IsAlive)
            {
                Debug.Log($"Skill {skillData.DisplayName} had no valid monster target.");
                return false;
            }

            if (!IsTargetInSkillRange(caster, target))
            {
                Debug.Log($"Skill {skillData.DisplayName} target moved outside its 2D range.");
                return false;
            }

            int damage = caster != null ? caster.CalculateDamageAgainst(baseDamage, target) : Mathf.Max(0, baseDamage);
            target.TakeDamage(damage);
            resolvedHitCount++;
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
            Vector3 impactPosition = target.transform.position;
            IReadOnlyList<MonsterController> monsters = MonsterController.ActiveMonsters;
            List<MonsterController> targets = new List<MonsterController>();

            for (int i = 0; i < monsters.Count; i++)
            {
                MonsterController monster = monsters[i];
                if (monster == null || !monster.IsAlive || Vector2.Distance(monster.transform.position, impactPosition) > radius)
                {
                    continue;
                }

                targets.Add(monster);
            }

            if (targets.Count == 0)
            {
                targets.Add(target);
            }

            for (int i = 0; i < targets.Count; i++)
            {
                MonsterController monster = targets[i];
                int damage = caster != null ? caster.CalculateDamageAgainst(baseDamage, monster) : baseDamage;
                monster.TakeDamage(damage);
                resolvedHitCount++;
            }

            return true;
        }

        private bool ApplyHolyHeal(HeroController caster)
        {
            CrystalController crystalController = FindAnyObjectByType<CrystalController>();
            HeroController woundedHero = FindLowestHealthHero();
            if (crystalController == null && woundedHero == null)
            {
                Debug.LogWarning($"Skill {skillData.DisplayName} could not find a hero or crystal to heal.");
                return false;
            }

            float healingMultiplier = caster != null ? caster.HealingMultiplier : 1f;
            int heroHeal = CalculateHeroHeal(skillData.Power, healingMultiplier);
            int crystalHeal = CalculateCrystalHeal(heroHeal);
            woundedHero?.Heal(heroHeal);
            if (crystalController != null)
            {
                crystalController.Heal(crystalHeal);
                CombatFeedbackEvents.RaiseUnitHealed(crystalController.transform.position);
            }

            return true;
        }

        private bool DeployTemporaryTurret(HeroController caster)
        {
            if (caster == null || !caster.IsAlive)
            {
                Debug.LogWarning($"Skill {skillData.DisplayName} requires a living caster.");
                return false;
            }

            GameObject turretObject = new GameObject($"TemporaryTurret_{caster.Data?.HeroId ?? caster.name}");
            TemporaryTurretController turret = turretObject.AddComponent<TemporaryTurretController>();
            turret.Initialize(
                caster,
                caster.LaneIndex,
                Mathf.Max(1, skillData.Power),
                Mathf.Max(1f, skillData.Range),
                Mathf.Max(4f, skillData.Radius * 6f),
                0.72f);
            return true;
        }

        private bool ApplyShadowStrike(HeroController caster, MonsterController target)
        {
            if (target == null || !target.IsAlive)
            {
                Debug.Log($"Skill {skillData.DisplayName} had no valid monster target.");
                return false;
            }

            int skillDamage = CalculateShadowStrikeDamage(skillData.Power, target.CurrentHp, target.MaxHp, target.IsBoss);
            return ApplyDirectDamage(caster, target, skillDamage);
        }

        private static HeroController FindLowestHealthHero()
        {
            IReadOnlyList<HeroController> heroes = HeroController.ActiveHeroes;
            HeroController selected = null;
            float selectedRatio = float.MaxValue;
            for (int i = 0; i < heroes.Count; i++)
            {
                HeroController hero = heroes[i];
                if (hero == null || !hero.IsAlive || hero.MaxHp <= 0)
                {
                    continue;
                }

                float ratio = hero.CurrentHp / (float)hero.MaxHp;
                if (ratio < selectedRatio)
                {
                    selected = hero;
                    selectedRatio = ratio;
                }
            }

            return selected;
        }

        private bool IsTargetInSkillRange(HeroController caster, MonsterController target)
        {
            if (caster == null || target == null)
            {
                return target != null;
            }

            return CombatGeometry.IsCenterInRange(
                caster.transform.position,
                target.transform.position,
                Mathf.Max(0.1f, skillData != null ? skillData.Range : 0.1f));
        }
    }
}
