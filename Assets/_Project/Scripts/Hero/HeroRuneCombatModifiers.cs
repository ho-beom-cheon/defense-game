using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    [DisallowMultipleComponent]
    public sealed class HeroRuneCombatModifiers : MonoBehaviour
    {
        private const int LightningProcInterval = 3;
        private const float SplashRadius = 1.75f;
        private const float LightningRange = 3.5f;

        private readonly HashSet<MonsterController> affectedTargets = new HashSet<MonsterController>();
        private float lightningDamagePercent;
        private float splashDamagePercent;
        private float chainDamagePercent;
        private float crushDamagePercent;
        private int attackImpactCount;

        public float LightningDamagePercent => lightningDamagePercent;
        public float SplashDamagePercent => splashDamagePercent;
        public float ChainDamagePercent => chainDamagePercent;
        public float CrushDamagePercent => crushDamagePercent;
        public int AttackImpactCount => attackImpactCount;

        public void ResetModifiers()
        {
            lightningDamagePercent = 0f;
            splashDamagePercent = 0f;
            chainDamagePercent = 0f;
            crushDamagePercent = 0f;
            attackImpactCount = 0;
            affectedTargets.Clear();
        }

        public void AddLightningDamagePercent(float percent)
        {
            lightningDamagePercent = AddPercent(lightningDamagePercent, percent);
        }

        public void AddSplashDamagePercent(float percent)
        {
            splashDamagePercent = AddPercent(splashDamagePercent, percent);
        }

        public void AddChainDamagePercent(float percent)
        {
            chainDamagePercent = AddPercent(chainDamagePercent, percent);
        }

        public void AddCrushDamagePercent(float percent)
        {
            crushDamagePercent = AddPercent(crushDamagePercent, percent);
        }

        public int ModifyPrimaryDamage(int baseDamage, MonsterController target)
        {
            bool crushTarget = target != null && target.Data != null &&
                (target.IsBoss || target.Data.MonsterType == MonsterType.Tank);
            return CalculateCrushDamage(baseDamage, crushDamagePercent, crushTarget);
        }

        public static int CalculateCrushDamage(int baseDamage, float percent, bool crushTarget)
        {
            float damage = Mathf.Max(0, baseDamage);
            if (crushTarget)
            {
                damage *= 1f + Mathf.Max(0f, percent);
            }

            return Mathf.Max(0, Mathf.RoundToInt(damage));
        }

        public void HandleBasicAttackImpact(HeroController owner, MonsterController primaryTarget, int primaryDamage)
        {
            if (owner == null || primaryTarget == null || primaryDamage <= 0)
            {
                return;
            }

            attackImpactCount++;
            affectedTargets.Clear();
            affectedTargets.Add(primaryTarget);

            ApplySplashDamage(primaryTarget, primaryDamage);
            if (owner.IsRangedCombatant)
            {
                MonsterController chainTarget = FindNearestTarget(primaryTarget.transform.position, LightningRange);
                ApplySecondaryDamage(chainTarget, primaryDamage, chainDamagePercent);
            }

            if (lightningDamagePercent > 0f && attackImpactCount % LightningProcInterval == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    MonsterController lightningTarget = FindNearestTarget(primaryTarget.transform.position, LightningRange);
                    if (!ApplySecondaryDamage(lightningTarget, primaryDamage, lightningDamagePercent))
                    {
                        break;
                    }
                }
            }
        }

        private void ApplySplashDamage(MonsterController primaryTarget, int primaryDamage)
        {
            if (splashDamagePercent <= 0f)
            {
                return;
            }

            Vector3 center = primaryTarget.transform.position;
            IReadOnlyList<MonsterController> monsters = MonsterController.ActiveMonsters;
            for (int i = 0; i < monsters.Count; i++)
            {
                MonsterController candidate = monsters[i];
                if (!CanHit(candidate))
                {
                    continue;
                }

                if (Vector2.Distance(center, candidate.transform.position) <= SplashRadius)
                {
                    ApplySecondaryDamage(candidate, primaryDamage, splashDamagePercent);
                }
            }
        }

        private MonsterController FindNearestTarget(Vector3 origin, float maxDistance)
        {
            MonsterController selected = null;
            float bestDistanceSquared = maxDistance < float.MaxValue ? maxDistance * maxDistance : float.MaxValue;
            IReadOnlyList<MonsterController> monsters = MonsterController.ActiveMonsters;
            for (int i = 0; i < monsters.Count; i++)
            {
                MonsterController candidate = monsters[i];
                if (!CanHit(candidate))
                {
                    continue;
                }

                float distanceSquared = (candidate.transform.position - origin).sqrMagnitude;
                if (distanceSquared < bestDistanceSquared)
                {
                    selected = candidate;
                    bestDistanceSquared = distanceSquared;
                }
            }

            return selected;
        }

        private bool ApplySecondaryDamage(MonsterController target, int primaryDamage, float percent)
        {
            if (!CanHit(target) || percent <= 0f)
            {
                return false;
            }

            int damage = Mathf.Max(1, Mathf.RoundToInt(primaryDamage * percent));
            affectedTargets.Add(target);
            CombatVisualEffectFactory.SpawnHitSpark(
                target.transform.position,
                RuntimeSpritePolicy.GetMonsterTargetHeight(target.Data) * 0.82f);
            CombatFeedbackEvents.RaiseAttackImpacted(target.transform.position);
            target.TakeDamage(damage);
            return true;
        }

        private bool CanHit(MonsterController target)
        {
            return target != null && target.IsAlive && !affectedTargets.Contains(target);
        }

        private static float AddPercent(float current, float added)
        {
            return Mathf.Clamp(current + Mathf.Max(0f, added), 0f, 2f);
        }
    }
}
