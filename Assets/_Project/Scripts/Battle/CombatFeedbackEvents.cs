using System;
using UnityEngine;

namespace RuneGate
{
    public static class CombatFeedbackEvents
    {
        public static event Action<Vector3> AttackStarted;
        public static event Action<Vector3> AttackImpacted;
        public static event Action<Vector3> UnitHit;
        public static event Action<Vector3> UnitDied;
        public static event Action<Vector3> UnitHealed;
        public static event Action<Vector3> CrystalDamaged;
        public static event Action<int> WaveStarted;
        public static event Action Victory;
        public static event Action Defeat;

        public static void RaiseAttackStarted(Vector3 position)
        {
            AttackStarted?.Invoke(position);
        }

        public static void RaiseAttackImpacted(Vector3 position)
        {
            AttackImpacted?.Invoke(position);
        }

        public static void RaiseUnitHit(Vector3 position)
        {
            UnitHit?.Invoke(position);
        }

        public static void RaiseUnitDied(Vector3 position)
        {
            UnitDied?.Invoke(position);
        }

        public static void RaiseUnitHealed(Vector3 position)
        {
            UnitHealed?.Invoke(position);
        }

        public static void RaiseCrystalDamaged(Vector3 position)
        {
            CrystalDamaged?.Invoke(position);
        }

        public static void RaiseWaveStarted(int waveNo)
        {
            WaveStarted?.Invoke(waveNo);
        }

        public static void RaiseVictory()
        {
            Victory?.Invoke();
        }

        public static void RaiseDefeat()
        {
            Defeat?.Invoke();
        }
    }
}
