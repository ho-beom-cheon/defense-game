using UnityEngine;

namespace RuneGate
{
    public sealed class BattleHUD : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private CrystalController crystalController;
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private Rect panelRect = new Rect(16f, 16f, 300f, 170f);

        private string crystalHpText = "Crystal HP -";
        private string waveText = "Wave -";
        private string battleStateText = BattleState.None.ToString();
        private string crystalFeedbackText = string.Empty;
        private float crystalFeedbackTimer;
        private int gold;
        private Vector2 heroScrollPosition;

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
                battleManager.GoldChanged += HandleGoldChanged;
            }

            if (crystalController != null)
            {
                crystalController.HpChanged += HandleCrystalHpChanged;
                crystalController.Damaged += HandleCrystalDamaged;
                HandleCrystalHpChanged(crystalController.CurrentHp, crystalController.MaxHp);
            }
        }

        private void OnDisable()
        {
            if (battleManager != null)
            {
                battleManager.BattleStateChanged -= HandleBattleStateChanged;
                battleManager.WaveChanged -= HandleWaveChanged;
                battleManager.GoldChanged -= HandleGoldChanged;
            }

            if (crystalController != null)
            {
                crystalController.HpChanged -= HandleCrystalHpChanged;
                crystalController.Damaged -= HandleCrystalDamaged;
            }
        }

        private void Update()
        {
            if (crystalFeedbackTimer <= 0f)
            {
                return;
            }

            crystalFeedbackTimer = Mathf.Max(0f, crystalFeedbackTimer - Time.deltaTime);
            if (crystalFeedbackTimer <= 0f)
            {
                crystalFeedbackText = string.Empty;
            }
        }

        private void OnGUI()
        {
            if (!drawRuntimeGui)
            {
                return;
            }

            AutoAssignReferences();
            Rect drawRect = panelRect;
            drawRect.height = Mathf.Max(drawRect.height, 230f);
            GUILayout.BeginArea(drawRect, GUI.skin.box);
            GUILayout.Label("RuneGate Defense");
            if (battleManager != null && battleManager.ActiveStageData != null)
            {
                GUILayout.Label($"Stage {battleManager.ActiveStageData.DisplayName}");
            }

            GUILayout.Label(crystalHpText);
            GUILayout.Label(waveText);
            GUILayout.Label($"State {battleStateText}");
            GUILayout.Label($"Gold {gold}");
            if (!string.IsNullOrWhiteSpace(crystalFeedbackText))
            {
                GUILayout.Label(crystalFeedbackText);
            }

            if (battleManager != null)
            {
                heroScrollPosition = GUILayout.BeginScrollView(heroScrollPosition, GUILayout.Height(96f));
                for (int i = 0; i < battleManager.Heroes.Count; i++)
                {
                    HeroController hero = battleManager.Heroes[i];
                    if (hero == null || hero.Data == null)
                    {
                        continue;
                    }

                    GUILayout.Label($"{hero.Data.DisplayName} ATK {hero.EffectiveAttack} SPD {hero.EffectiveAttackSpeed:0.00}");
                }

                GUILayout.EndScrollView();
            }

            GUILayout.EndArea();
        }

        private void AutoAssignReferences()
        {
            if (battleManager == null)
            {
                battleManager = FindAnyObjectByType<BattleManager>();
            }

            if (crystalController == null)
            {
                crystalController = FindAnyObjectByType<CrystalController>();
            }
        }

        private void HandleCrystalHpChanged(int currentHp, int maxHp)
        {
            crystalHpText = $"Crystal HP {currentHp}/{maxHp}";
            Debug.Log(crystalHpText);
        }

        private void HandleWaveChanged(int currentWave, int totalWaves)
        {
            waveText = $"Wave {currentWave}/{totalWaves}";
        }

        private void HandleBattleStateChanged(BattleState battleState)
        {
            battleStateText = battleState.ToString();
        }

        private void HandleGoldChanged(int amount)
        {
            gold = amount;
        }

        private void HandleCrystalDamaged(int damage, int currentHp, int maxHp)
        {
            crystalFeedbackText = $"Crystal Hit -{damage}";
            crystalFeedbackTimer = 1.2f;
        }
    }
}
