using UnityEngine;

namespace RuneGate
{
    public sealed class ProjectileController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float hitDistance = 0.12f;

        private MonsterController target;
        private int damage;
        private bool initialized;

        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            if (target == null || !target.IsAlive)
            {
                Destroy(gameObject);
                return;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                target.transform.position,
                moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.transform.position) <= hitDistance)
            {
                target.TakeDamage(damage);
                Destroy(gameObject);
            }
        }

        public void Initialize(MonsterController targetMonster, int projectileDamage)
        {
            target = targetMonster;
            damage = Mathf.Max(0, projectileDamage);
            initialized = true;
        }
    }
}
