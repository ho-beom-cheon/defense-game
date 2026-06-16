using UnityEngine;

namespace RuneGate
{
    public sealed class BattleHUD : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private CrystalController crystalController;
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private Rect panelRect = new Rect(8f, 12f, 248f, 210f);

        private string crystalHpText = "크리스탈 HP -";
        private string waveText = "웨이브 -";
        private string battleStateText = GameTextMapper.BattleStateName(BattleState.None);
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
                HandleBattleStateChanged(battleManager.CurrentState);
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

            KoreanFontManager.ApplyToGuiSkin();
            AutoAssignReferences();
            Rect drawRect = panelRect;
            drawRect.height = Mathf.Max(drawRect.height, 220f);
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUILayout.BeginArea(drawRect, panelStyle);
            GUILayout.Label("RuneGate Defense");
            if (battleManager != null && battleManager.ActiveStageData != null)
            {
                GUILayout.Label($"스테이지 {GameTextMapper.StageName(battleManager.ActiveStageData)}");
            }

            GUILayout.Label(crystalHpText);
            GUILayout.Label(waveText);
            GUILayout.Label($"상태 {battleStateText}");
            GUILayout.Label($"골드 {gold}");
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

                    GUILayout.Label($"{hero.Data.DisplayNameKorean} 공격 {hero.EffectiveAttack} 속도 {hero.EffectiveAttackSpeed:0.00}");
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
            crystalHpText = $"크리스탈 HP {currentHp}/{maxHp}";
            Debug.Log(crystalHpText);
        }

        private void HandleWaveChanged(int currentWave, int totalWaves)
        {
            waveText = $"웨이브 {currentWave}/{totalWaves}";
        }

        private void HandleBattleStateChanged(BattleState battleState)
        {
            battleStateText = GameTextMapper.BattleStateName(battleState);
        }

        private void HandleGoldChanged(int amount)
        {
            gold = amount;
        }

        private void HandleCrystalDamaged(int damage, int currentHp, int maxHp)
        {
            crystalFeedbackText = $"크리스탈 피해 -{damage}";
            crystalFeedbackTimer = 1.2f;
        }
    }
}
