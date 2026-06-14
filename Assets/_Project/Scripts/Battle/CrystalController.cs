using System;
using UnityEngine;

namespace RuneGate
{
    public sealed class CrystalController : MonoBehaviour
    {
        [SerializeField] private int defaultMaxHp = 100;

        private int maxHp;
        private int currentHp;
        private bool initialized;

        public event Action<int, int> HpChanged;
        public event Action Destroyed;

        public int MaxHp => maxHp;
        public int CurrentHp => currentHp;
        public bool IsDestroyed => initialized && currentHp <= 0;

        private void Awake()
        {
            if (!initialized)
            {
                Initialize(defaultMaxHp);
            }
        }

        public void Initialize(int hp)
        {
            maxHp = Mathf.Max(1, hp);
            currentHp = maxHp;
            initialized = true;
            HpChanged?.Invoke(currentHp, maxHp);
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

            currentHp = Mathf.Max(0, currentHp - damage);
            HpChanged?.Invoke(currentHp, maxHp);

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
    }
}
