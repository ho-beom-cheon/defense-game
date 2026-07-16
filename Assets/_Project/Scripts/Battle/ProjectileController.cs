using System;
using UnityEngine;

namespace RuneGate
{
    public sealed class ProjectileController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float hitDistance = 0.12f;
        [SerializeField] private bool spawnImpactEffect = true;

        private MonsterController target;
        private int damage;
        private Action<MonsterController, int> impactCallback;
        private bool initialized;

        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            if (target == null || !target.IsAlive)
            {
                if (spawnImpactEffect)
                {
                    CombatVisualEffectFactory.SpawnHitSpark(transform.position, 0.8f);
                }

                Destroy(gameObject);
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, target.transform.position) <= hitDistance)
            {
                if (spawnImpactEffect)
                {
                    CombatVisualEffectFactory.SpawnHitSpark(target.transform.position, RuntimeSpritePolicy.GetMonsterTargetHeight(target.Data));
                }

                CombatFeedbackEvents.RaiseAttackImpacted(target.transform.position);
                target.TakeDamage(damage);
                impactCallback?.Invoke(target, damage);
                Destroy(gameObject);
            }
        }

        public void Initialize(MonsterController targetMonster, int projectileDamage)
        {
            Initialize(targetMonster, projectileDamage, null);
        }

        public void Initialize(
            MonsterController targetMonster,
            int projectileDamage,
            Action<MonsterController, int> onImpact)
        {
            target = targetMonster;
            damage = Mathf.Max(0, projectileDamage);
            impactCallback = onImpact;
            initialized = true;
        }
    }
}
