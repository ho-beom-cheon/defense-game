using System;
using UnityEngine;

namespace RuneGate
{
    public sealed class CrystalController : MonoBehaviour
    {
        [SerializeField] private int defaultMaxHp = 100;

        private int maxHp;
        private int currentHp;
        private int shield;
        private bool initialized;

        public event Action<int, int> HpChanged;
        public event Action<int> ShieldChanged;
        public event Action Destroyed;

        public int MaxHp => maxHp;
        public int CurrentHp => currentHp;
        public int Shield => shield;
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
            shield = 0;
            initialized = true;
            HpChanged?.Invoke(currentHp, maxHp);
            ShieldChanged?.Invoke(shield);
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

            int remainingDamage = damage;
            if (shield > 0)
            {
                int absorbed = Mathf.Min(shield, remainingDamage);
                shield -= absorbed;
                remainingDamage -= absorbed;
                ShieldChanged?.Invoke(shield);
            }

            if (remainingDamage > 0)
            {
                currentHp = Mathf.Max(0, currentHp - remainingDamage);
                HpChanged?.Invoke(currentHp, maxHp);
            }

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

        public void AddShield(int amount)
        {
            if (amount <= 0 || currentHp <= 0)
            {
                return;
            }

            shield += amount;
            ShieldChanged?.Invoke(shield);
        }
    }
}
