using System;
using UnityEngine;

namespace RuneGate
{
    [DisallowMultipleComponent]
    public sealed class BossPhaseController : MonoBehaviour
    {
        [SerializeField, Range(0.1f, 0.95f)] private float phaseTwoHealthPercent = 0.65f;
        [SerializeField, Range(0.05f, 0.9f)] private float phaseThreeHealthPercent = 0.30f;
        [SerializeField] private int phaseTwoReinforcementCount = 2;
        [SerializeField] private int phaseThreeReinforcementCount = 3;

        private MonsterController boss;
        private WaveManager waveManager;
        private int currentPhase = 1;
        private int totalReinforcementsRequested;

        public event Action<int> PhaseChanged;
        public event Action<int> ReinforcementsRequested;

        public int CurrentPhase => currentPhase;
        public int MaxPhase => 3;
        public int TotalReinforcementsRequested => totalReinforcementsRequested;
        public float HealthPercent => boss != null && boss.MaxHp > 0
            ? Mathf.Clamp01((float)boss.CurrentHp / boss.MaxHp)
            : 0f;

        public void Configure(MonsterController bossController, WaveManager ownerWaveManager)
        {
            UnbindBoss();
            boss = bossController;
            waveManager = ownerWaveManager;
            currentPhase = 1;
            totalReinforcementsRequested = 0;

            if (boss == null || !boss.IsBoss)
            {
                Debug.LogWarning("BossPhaseController requires an initialized boss monster.");
                return;
            }

            boss.HpChanged += HandleBossHpChanged;
            boss.ApplyBossPhaseModifiers(1f, 1f, 1f);
        }

        public int ClampIncomingDamageToPhaseGate(int currentHp, int maxHp, int incomingDamage)
        {
            if (boss == null || maxHp <= 0 || incomingDamage <= 0)
            {
                return incomingDamage;
            }

            float threshold = currentPhase == 1
                ? Mathf.Clamp(phaseTwoHealthPercent, 0.1f, 0.95f)
                : currentPhase == 2
                    ? Mathf.Clamp(phaseThreeHealthPercent, 0.05f, 0.9f)
                    : 0f;
            if (threshold <= 0f)
            {
                return incomingDamage;
            }

            int gateHp = Mathf.Max(1, Mathf.FloorToInt(maxHp * threshold));
            int projectedHp = currentHp - incomingDamage;
            if (currentHp > gateHp && projectedHp < gateHp)
            {
                return Mathf.Max(1, currentHp - gateHp);
            }

            return incomingDamage;
        }

        private void OnDestroy()
        {
            UnbindBoss();
        }

        private void HandleBossHpChanged(int currentHp, int maxHp)
        {
            if (boss == null || currentHp <= 0 || maxHp <= 0)
            {
                return;
            }

            float healthPercent = Mathf.Clamp01((float)currentHp / maxHp);
            float phaseTwoThreshold = Mathf.Clamp(phaseTwoHealthPercent, 0.1f, 0.95f);
            float phaseThreeThreshold = Mathf.Clamp(phaseThreeHealthPercent, 0.05f, phaseTwoThreshold - 0.05f);

            if (currentPhase < 2 && healthPercent <= phaseTwoThreshold)
            {
                EnterPhase(2, 1.15f, 0.84f, 1.20f, phaseTwoReinforcementCount);
            }

            if (currentPhase < 3 && healthPercent <= phaseThreeThreshold)
            {
                EnterPhase(3, 1.32f, 0.66f, 1.45f, phaseThreeReinforcementCount);
            }
        }

        private void EnterPhase(int phase, float moveSpeedMultiplier, float attackIntervalMultiplier, float attackDamageMultiplier, int reinforcementCount)
        {
            if (boss == null || phase <= currentPhase)
            {
                return;
            }

            currentPhase = phase;
            boss.ApplyBossPhaseModifiers(moveSpeedMultiplier, attackIntervalMultiplier, attackDamageMultiplier);
            boss.PlayBossPhaseFeedback(phase);

            int requested = waveManager != null
                ? waveManager.RequestBossReinforcements(boss, Mathf.Max(0, reinforcementCount))
                : 0;
            totalReinforcementsRequested += requested;
            PhaseChanged?.Invoke(currentPhase);
            ReinforcementsRequested?.Invoke(requested);
            Debug.Log($"{boss.Data.DisplayNameKorean} entered boss phase {currentPhase}. Reinforcements={requested}.");
        }

        private void UnbindBoss()
        {
            if (boss != null)
            {
                boss.HpChanged -= HandleBossHpChanged;
            }
        }
    }
}
