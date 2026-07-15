using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class BattleHUD : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private CrystalController crystalController;
        [SerializeField] private BattlePauseController pauseController;
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private Rect panelRect = new Rect(8f, 12f, 248f, 210f);

        private string crystalHpText = "\ud06c\ub9ac\uc2a4\ud0c8 HP -";
        private string waveText = "\uc6e8\uc774\ube0c -";
        private string battleStateText = GameTextMapper.BattleStateName(BattleState.None);
        private string crystalFeedbackText = string.Empty;
        private float crystalFeedbackTimer;
        private int crystalCurrentHp;
        private int crystalMaxHp;
        private int crystalShieldHp;
        private int gold;
        private string bossStatusText = string.Empty;
        private float bossHealthPercent;

        public string CrystalHpText => crystalHpText;
        public string WaveText => waveText;
        public string BattleStateText => battleStateText;
        public string BossStatusText => bossStatusText;

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
                crystalController.ShieldChanged += HandleCrystalShieldChanged;
                crystalController.Damaged += HandleCrystalDamaged;
                HandleCrystalHpChanged(crystalController.CurrentHp, crystalController.MaxHp);
                HandleCrystalShieldChanged(crystalController.ShieldHp);
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
                crystalController.ShieldChanged -= HandleCrystalShieldChanged;
                crystalController.Damaged -= HandleCrystalDamaged;
            }
        }

        private void Update()
        {
            RefreshBossStatus();
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
            DrawOutsideBattlefieldBackdrop(battleFrame.BattleFieldFrame);
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

            GUILayout.FlexibleSpace();
            bool previousEnabled = GUI.enabled;
            GUI.enabled = pauseController != null && pauseController.CanPause && !pauseController.IsPaused;
            if (GUILayout.Button("일시정지", GUILayout.Width(Mathf.Clamp(drawRect.width * 0.13f, 96f, 150f)), GUILayout.Height(UIResponsiveLayout.TouchHeight(32f))))
            {
                pauseController.Pause();
            }

            GUI.enabled = previousEnabled;

            GUILayout.EndHorizontal();

            bool showHeroSummary = !Application.isMobilePlatform || !GameFrameLayout.IsPortrait;
            if (showHeroSummary && battleManager != null && drawRect.height >= 104f)
            {
                DrawHeroSummaryStrip(drawRect.width);
            }

            GUILayout.EndArea();
            DrawBossStatus(battleFrame.BattleFieldFrame);

            if (pauseController != null && pauseController.IsPaused)
            {
                DrawPausePopup();
            }
        }

        private static void DrawOutsideBattlefieldBackdrop(Rect battlefieldRect)
        {
            int previousDepth = GUI.depth;
            Color previousColor = GUI.color;
            GUI.depth = 1000;
            GUI.color = new Color(0.025f, 0.035f, 0.04f, 1f);

            DrawSolidRect(new Rect(0f, 0f, Screen.width, Mathf.Max(0f, battlefieldRect.y)));
            DrawSolidRect(new Rect(0f, battlefieldRect.yMax, Screen.width, Mathf.Max(0f, Screen.height - battlefieldRect.yMax)));
            DrawSolidRect(new Rect(0f, battlefieldRect.y, Mathf.Max(0f, battlefieldRect.x), battlefieldRect.height));
            DrawSolidRect(new Rect(battlefieldRect.xMax, battlefieldRect.y, Mathf.Max(0f, Screen.width - battlefieldRect.xMax), battlefieldRect.height));

            GUI.color = previousColor;
            GUI.depth = previousDepth;
        }

        private static void DrawSolidRect(Rect rect)
        {
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
        }

        private void DrawBossStatus(Rect battlefieldRect)
        {
            if (string.IsNullOrWhiteSpace(bossStatusText))
            {
                return;
            }

            float width = Mathf.Clamp(battlefieldRect.width * 0.72f, 320f, 720f);
            Rect bossRect = new Rect(
                battlefieldRect.center.x - width * 0.5f,
                battlefieldRect.y + UIResponsiveLayout.SmallGap,
                width,
                UIResponsiveLayout.TouchHeight(50f));
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUILayout.BeginArea(UIResponsiveLayout.ClampToScreen(bossRect), panelStyle);
            GUILayout.Label(bossStatusText);
            Rect barRect = GUILayoutUtility.GetRect(width - 24f, 12f, GUILayout.Height(12f));
            Color previousColor = GUI.color;
            GUI.color = new Color(0.12f, 0.05f, 0.06f, 0.96f);
            GUI.DrawTexture(barRect, Texture2D.whiteTexture);
            GUI.color = new Color(0.82f, 0.16f, 0.18f, 1f);
            GUI.DrawTexture(new Rect(barRect.x, barRect.y, barRect.width * Mathf.Clamp01(bossHealthPercent), barRect.height), Texture2D.whiteTexture);
            GUI.color = previousColor;
            GUILayout.EndArea();
        }

        public void RefreshBossStatus()
        {
            MonsterController activeBoss = null;
            IReadOnlyList<MonsterController> monsters = MonsterController.ActiveMonsters;
            for (int i = 0; i < monsters.Count; i++)
            {
                MonsterController monster = monsters[i];
                if (monster != null && monster.IsAlive && monster.IsBoss)
                {
                    activeBoss = monster;
                    break;
                }
            }

            if (activeBoss == null || activeBoss.Data == null)
            {
                bossStatusText = string.Empty;
                bossHealthPercent = 0f;
                return;
            }

            BossPhaseController phaseController = activeBoss.BossPhaseController;
            int currentPhase = phaseController != null ? phaseController.CurrentPhase : 1;
            int maxPhase = phaseController != null ? phaseController.MaxPhase : 3;
            bossHealthPercent = activeBoss.MaxHp > 0 ? Mathf.Clamp01((float)activeBoss.CurrentHp / activeBoss.MaxHp) : 0f;
            bossStatusText = $"{activeBoss.Data.DisplayNameKorean}  HP {activeBoss.CurrentHp}/{activeBoss.MaxHp}  페이즈 {currentPhase}/{maxPhase}";
        }

        private void DrawPausePopup()
        {
            UIPopupGuiUtility.DrawDimOverlay();
            Rect popupRect = GameFrameLayout.PopupFrame(620f, 520f, 0.88f, 0.54f);
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUILayout.BeginArea(popupRect, panelStyle);
            GUI.SetNextControlName("PopupLayer_BattlePause");
            GUILayout.Label("전투 일시정지");
            GUILayout.Space(UIResponsiveLayout.SmallGap);
            GUILayout.Label(pauseController.PausedByLifecycle
                ? "앱이 백그라운드로 전환되어 전투를 멈췄습니다."
                : "전투가 멈춰 있습니다.");
            GUILayout.FlexibleSpace();

            float primaryHeight = UIResponsiveLayout.TouchHeight(44f);
            float secondaryHeight = UIResponsiveLayout.TouchHeight(38f);
            if (GUILayout.Button("계속하기", GUILayout.Height(primaryHeight)))
            {
                pauseController.Resume();
            }

            GUILayout.Space(UIResponsiveLayout.SmallGap);
            if (GUILayout.Button("전투 재시작", GUILayout.Height(secondaryHeight)))
            {
                pauseController.RestartBattle();
            }

            if (GUILayout.Button("스테이지 선택", GUILayout.Height(secondaryHeight)))
            {
                pauseController.OpenStageSelect();
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

            if (pauseController == null)
            {
                pauseController = FindAnyObjectByType<BattlePauseController>();
            }

            if (pauseController == null)
            {
                pauseController = gameObject.AddComponent<BattlePauseController>();
            }

            pauseController.Configure(battleManager);
        }

        private void HandleCrystalHpChanged(int currentHp, int maxHp)
        {
            crystalCurrentHp = currentHp;
            crystalMaxHp = maxHp;
            RefreshCrystalStatusText();
        }

        private void HandleCrystalShieldChanged(int shieldHp)
        {
            crystalShieldHp = Mathf.Max(0, shieldHp);
            RefreshCrystalStatusText();
        }

        private void RefreshCrystalStatusText()
        {
            crystalHpText = crystalShieldHp > 0
                ? $"\ud06c\ub9ac\uc2a4\ud0c8 HP {crystalCurrentHp}/{crystalMaxHp}  \ubcf4\ud638\ub9c9 {crystalShieldHp}"
                : $"\ud06c\ub9ac\uc2a4\ud0c8 HP {crystalCurrentHp}/{crystalMaxHp}";
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
