using UnityEngine;

namespace RuneGate
{
    public sealed class BattleHUD : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private CrystalController crystalController;
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private Rect panelRect = new Rect(8f, 12f, 248f, 210f);

        private string crystalHpText = "\ud06c\ub9ac\uc2a4\ud0c8 HP -";
        private string waveText = "\uc6e8\uc774\ube0c -";
        private string battleStateText = GameTextMapper.BattleStateName(BattleState.None);
        private string crystalFeedbackText = string.Empty;
        private float crystalFeedbackTimer;
        private int gold;

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

            UIResponsiveLayout.ApplyReadableDefaults();
            AutoAssignReferences();

            BattleFrameRects battleFrame = GameFrameLayout.BattleFrame();
            Rect drawRect = UIResponsiveLayout.ClampToScreen(battleFrame.HeaderArea);
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUILayout.BeginArea(drawRect, panelStyle);
            GUI.SetNextControlName("BattleFrame_HeaderArea");
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(Mathf.Clamp(drawRect.width * 0.34f, 120f, 300f)));
            GUILayout.Label("RuneGate Defense");

            if (battleManager != null && battleManager.ActiveStageData != null)
            {
                GUILayout.Label($"\uc2a4\ud14c\uc774\uc9c0 {GameTextMapper.StageName(battleManager.ActiveStageData)}");
            }

            GUILayout.Label($"\ub09c\uc774\ub3c4 {GameTextMapper.Difficulty(DifficultyRules.CurrentDifficultyId)}");

            GUILayout.EndVertical();
            GUILayout.BeginVertical(GUILayout.Width(Mathf.Clamp(drawRect.width * 0.22f, 110f, 220f)));
            GUILayout.Label(crystalHpText);
            GUILayout.Label(waveText);
            GUILayout.EndVertical();
            GUILayout.BeginVertical(GUILayout.Width(Mathf.Clamp(drawRect.width * 0.16f, 90f, 180f)));
            GUILayout.Label($"\uc0c1\ud0dc {battleStateText}");
            GUILayout.Label($"\uace8\ub4dc {gold}");
            GUILayout.EndVertical();

            if (!string.IsNullOrWhiteSpace(crystalFeedbackText))
            {
                GUILayout.BeginVertical(GUILayout.Width(Mathf.Clamp(drawRect.width * 0.14f, 80f, 160f)));
                GUILayout.Label(crystalFeedbackText);
                GUILayout.EndVertical();
            }

            GUILayout.EndHorizontal();

            bool showHeroSummary = !Application.isMobilePlatform || !GameFrameLayout.IsPortrait;
            if (showHeroSummary && battleManager != null && drawRect.height >= 104f)
            {
                DrawHeroSummaryStrip(drawRect.width);
            }

            GUILayout.EndArea();
        }

        private void DrawHeroSummaryStrip(float headerWidth)
        {
            const int columns = 3;
            float itemWidth = Mathf.Max(120f, (headerWidth - 24f) / columns);
            int heroCount = battleManager.Heroes.Count;
            for (int startIndex = 0; startIndex < heroCount; startIndex += columns)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(20f));
                for (int column = 0; column < columns; column++)
                {
                    int heroIndex = startIndex + column;
                    if (heroIndex >= heroCount)
                    {
                        GUILayout.Space(itemWidth);
                        continue;
                    }

                    HeroController hero = battleManager.Heroes[heroIndex];
                    if (hero == null || hero.Data == null)
                    {
                        GUILayout.Space(itemWidth);
                        continue;
                    }

                    GUILayout.Label($"{hero.Data.DisplayNameKorean} \uacf5\uaca9 {hero.EffectiveAttack} \uc18d\ub3c4 {hero.EffectiveAttackSpeed:0.00}", GUILayout.Width(itemWidth));
                }

                GUILayout.EndHorizontal();
            }
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
            crystalHpText = $"\ud06c\ub9ac\uc2a4\ud0c8 HP {currentHp}/{maxHp}";
        }

        private void HandleWaveChanged(int currentWave, int totalWaves)
        {
            waveText = $"\uc6e8\uc774\ube0c {currentWave}/{totalWaves}";
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
            crystalFeedbackText = $"\ud06c\ub9ac\uc2a4\ud0c8 \ud53c\ud574 -{damage}";
            crystalFeedbackTimer = 1.2f;
        }
    }
}
