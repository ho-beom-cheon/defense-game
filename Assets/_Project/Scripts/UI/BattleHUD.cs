using UnityEngine;

namespace RuneGate
{
    public sealed class BattleHUD : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private CrystalController crystalController;
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private Rect panelRect = new Rect(16f, 16f, 280f, 150f);

        private string crystalHpText = "Crystal HP -";
        private string waveText = "Wave -";
        private string battleStateText = BattleState.None.ToString();

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
                HandleCrystalHpChanged(crystalController.CurrentHp, crystalController.MaxHp);
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

        private void OnGUI()
        {
            if (!drawRuntimeGui)
            {
                return;
            }

            AutoAssignReferences();

            GUILayout.BeginArea(panelRect, GUI.skin.box);
            GUILayout.Label("RuneGate Defense");
            GUILayout.Label(crystalHpText);
            GUILayout.Label(waveText);
            GUILayout.Label($"State {battleStateText}");

            if (battleManager != null)
            {
                for (int i = 0; i < battleManager.Heroes.Count; i++)
                {
                    HeroController hero = battleManager.Heroes[i];
                    if (hero == null || hero.Data == null)
                    {
                        continue;
                    }

                    GUILayout.Label($"{hero.Data.DisplayName} ATK {hero.EffectiveAttack} SPD {hero.EffectiveAttackSpeed:0.00}");
                }
            }

            GUILayout.EndArea();
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
