using UnityEngine;

namespace RuneGate
{
    public sealed class BattleHUD : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private CrystalController crystalController;

        private string crystalHpText;
        private string waveText;
        private string battleStateText;

        public string CrystalHpText => crystalHpText;
        public string WaveText => waveText;
        public string BattleStateText => battleStateText;

        private void OnEnable()
        {
            AutoAssignReferences();

            if (battleManager != null)
            {
                battleManager.BattleStateChanged += HandleBattleStateChanged;
                battleManager.WaveChanged += HandleWaveChanged;
            }

            if (crystalController != null)
            {
                crystalController.HpChanged += HandleCrystalHpChanged;
            }
        }

        private void OnDisable()
        {
            if (battleManager != null)
            {
                battleManager.BattleStateChanged -= HandleBattleStateChanged;
                battleManager.WaveChanged -= HandleWaveChanged;
            }

            if (crystalController != null)
            {
                crystalController.HpChanged -= HandleCrystalHpChanged;
            }
        }

        private void AutoAssignReferences()
        {
            if (battleManager == null)
            {
                battleManager = FindFirstObjectByType<BattleManager>();
            }

            if (crystalController == null)
            {
                crystalController = FindFirstObjectByType<CrystalController>();
            }
        }

        private void HandleCrystalHpChanged(int currentHp, int maxHp)
        {
            crystalHpText = $"Crystal HP {currentHp}/{maxHp}";
        }

        private void HandleWaveChanged(int currentWave, int totalWaves)
        {
            waveText = $"Wave {currentWave}/{totalWaves}";
        }

        private void HandleBattleStateChanged(BattleState battleState)
        {
            battleStateText = battleState.ToString();
        }
    }
}
