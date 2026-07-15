using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class BattleHUD : MonoBehaviour
    {
        private const float HeaderPadding = 8f;
        private const float HeaderGap = 8f;

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
        private int currentWave;
        private int totalWaves;
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
            GUI.SetNextControlName("BattleFrame_HeaderArea");
            DrawBattleStatusHeader(drawRect);
            DrawBossStatus(battleFrame.BattleFieldFrame);

            if (pauseController != null && pauseController.IsPaused)
            {
                DrawPausePopup();
            }
        }

        public static void CalculateHeaderRects(Rect headerRect, out Rect stageRect, out Rect crystalRect, out Rect battleRect, out Rect pauseRect)
        {
            float padding = Mathf.Min(HeaderPadding, Mathf.Max(2f, headerRect.height * 0.08f));
            float gap = Mathf.Min(HeaderGap, Mathf.Max(2f, headerRect.width * 0.012f));
            Rect content = new Rect(
                headerRect.x + padding,
                headerRect.y + padding,
                Mathf.Max(1f, headerRect.width - padding * 2f),
                Mathf.Max(1f, headerRect.height - padding * 2f));
            float pauseWidth = Mathf.Clamp(content.width * 0.12f, 72f, 142f);
            float informationWidth = Mathf.Max(3f, content.width - pauseWidth - gap * 3f);
            float stageWidth = informationWidth * 0.28f;
            float crystalWidth = informationWidth * 0.38f;
            float battleWidth = Mathf.Max(1f, informationWidth - stageWidth - crystalWidth);

            stageRect = new Rect(content.x, content.y, stageWidth, content.height);
            crystalRect = new Rect(stageRect.xMax + gap, content.y, crystalWidth, content.height);
            battleRect = new Rect(crystalRect.xMax + gap, content.y, battleWidth, content.height);
            pauseRect = new Rect(battleRect.xMax + gap, content.y, pauseWidth, content.height);
        }

        public static Color CrystalHealthColor(float healthRatio)
        {
            if (healthRatio <= 0.30f)
            {
                return new Color(0.92f, 0.22f, 0.20f, 1f);
            }

            if (healthRatio <= 0.60f)
            {
                return new Color(0.95f, 0.66f, 0.18f, 1f);
            }

            return new Color(0.22f, 0.82f, 0.46f, 1f);
        }

        private void DrawBattleStatusHeader(Rect drawRect)
        {
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUI.Box(drawRect, GUIContent.none, panelStyle);
            CalculateHeaderRects(drawRect, out Rect stageRect, out Rect crystalRect, out Rect battleRect, out Rect pauseRect);
            DrawStageCard(stageRect, panelStyle);
            DrawCrystalCard(crystalRect, panelStyle);
            DrawBattleProgressCard(battleRect, panelStyle);
            DrawPauseButton(pauseRect);
        }

        private void DrawStageCard(Rect rect, GUIStyle panelStyle)
        {
            DrawTintedBox(rect, panelStyle, new Color(0.26f, 0.48f, 0.42f, 0.92f));
            float padding = 8f;
            string stageName = battleManager != null && battleManager.ActiveStageData != null
                ? GameTextMapper.StageName(battleManager.ActiveStageData)
                : "스테이지";
            GUI.Label(new Rect(rect.x + padding, rect.y + 5f, rect.width - padding * 2f, rect.height * 0.46f), stageName,
                CreateLabelStyle(TextAnchor.MiddleLeft, true, 16f));
            GUIStyle detailStyle = CreateLabelStyle(TextAnchor.MiddleLeft, false, 11f);
            detailStyle.normal.textColor = new Color(0.74f, 0.86f, 0.83f, 1f);
            GUI.Label(new Rect(rect.x + padding, rect.y + rect.height * 0.46f, rect.width - padding * 2f, rect.height * 0.23f),
                $"난이도 {GameTextMapper.Difficulty(DifficultyRules.CurrentDifficultyId)}", detailStyle);
            GUI.Label(new Rect(rect.x + padding, rect.y + rect.height * 0.69f, rect.width - padding * 2f, rect.height * 0.22f),
                "봉문 전선", detailStyle);
        }

        private void DrawCrystalCard(Rect rect, GUIStyle panelStyle)
        {
            DrawTintedBox(rect, panelStyle, new Color(0.24f, 0.58f, 0.72f, 0.94f));
            float padding = 8f;
            float hpRatio = crystalMaxHp > 0 ? Mathf.Clamp01(crystalCurrentHp / (float)crystalMaxHp) : 0f;
            GUIStyle titleStyle = CreateLabelStyle(TextAnchor.MiddleLeft, true, 14f);
            GUIStyle valueStyle = CreateLabelStyle(TextAnchor.MiddleRight, true, 13f);
            GUI.Label(new Rect(rect.x + padding, rect.y + 4f, rect.width * 0.45f, rect.height * 0.30f), "크리스탈", titleStyle);
            GUI.Label(new Rect(rect.x + rect.width * 0.42f, rect.y + 4f, rect.width * 0.58f - padding, rect.height * 0.30f),
                $"{crystalCurrentHp}/{crystalMaxHp}", valueStyle);

            Rect hpBar = new Rect(rect.x + padding, rect.y + rect.height * 0.34f, rect.width - padding * 2f, Mathf.Clamp(rect.height * 0.15f, 10f, 18f));
            DrawProgressBar(hpBar, hpRatio, CrystalHealthColor(hpRatio));

            GUIStyle feedbackStyle = CreateLabelStyle(TextAnchor.MiddleLeft, crystalShieldHp > 0, 10f);
            if (!string.IsNullOrWhiteSpace(crystalFeedbackText))
            {
                feedbackStyle.normal.textColor = new Color(1f, 0.42f, 0.38f, 1f);
                GUI.Label(new Rect(rect.x + padding, hpBar.yMax + 3f, rect.width - padding * 2f, rect.yMax - hpBar.yMax - 6f), crystalFeedbackText, feedbackStyle);
                return;
            }

            if (crystalShieldHp > 0)
            {
                float shieldRatio = crystalMaxHp > 0 ? Mathf.Clamp01(crystalShieldHp / (float)crystalMaxHp) : 0f;
                Rect shieldBar = new Rect(rect.x + padding, hpBar.yMax + 5f, rect.width - padding * 2f, Mathf.Clamp(rect.height * 0.10f, 7f, 12f));
                DrawProgressBar(shieldBar, shieldRatio, new Color(0.26f, 0.68f, 1f, 1f));
                GUI.Label(new Rect(rect.x + padding, shieldBar.yMax + 1f, rect.width - padding * 2f, rect.yMax - shieldBar.yMax - 3f), $"보호막 +{crystalShieldHp}", feedbackStyle);
            }
            else
            {
                feedbackStyle.normal.textColor = new Color(0.62f, 0.78f, 0.82f, 1f);
                GUI.Label(new Rect(rect.x + padding, hpBar.yMax + 3f, rect.width - padding * 2f, rect.yMax - hpBar.yMax - 6f), "균열 방어 핵", feedbackStyle);
            }
        }

        private void DrawBattleProgressCard(Rect rect, GUIStyle panelStyle)
        {
            DrawTintedBox(rect, panelStyle, new Color(0.50f, 0.42f, 0.22f, 0.92f));
            float padding = 8f;
            float waveRatio = totalWaves > 0 ? Mathf.Clamp01(currentWave / (float)totalWaves) : 0f;
            GUIStyle stateStyle = CreateLabelStyle(TextAnchor.MiddleLeft, true, 13f);
            stateStyle.normal.textColor = BattleStateColor(battleManager != null ? battleManager.CurrentState : BattleState.None);
            GUI.Label(new Rect(rect.x + padding, rect.y + 4f, rect.width * 0.62f, rect.height * 0.28f), battleStateText, stateStyle);
            GUI.Label(new Rect(rect.x + rect.width * 0.55f, rect.y + 4f, rect.width * 0.45f - padding, rect.height * 0.28f),
                $"골드 {gold}", CreateLabelStyle(TextAnchor.MiddleRight, true, 11f));

            GUI.Label(new Rect(rect.x + padding, rect.y + rect.height * 0.30f, rect.width - padding * 2f, rect.height * 0.27f),
                totalWaves > 0 ? $"웨이브 {currentWave}/{totalWaves}" : waveText,
                CreateLabelStyle(TextAnchor.MiddleLeft, false, 11f));
            Rect waveBar = new Rect(rect.x + padding, rect.y + rect.height * 0.62f, rect.width - padding * 2f, Mathf.Clamp(rect.height * 0.13f, 9f, 15f));
            DrawProgressBar(waveBar, waveRatio, new Color(0.96f, 0.72f, 0.22f, 1f));
        }

        private void DrawPauseButton(Rect rect)
        {
            bool previousEnabled = GUI.enabled;
            Color previousBackground = GUI.backgroundColor;
            GUI.enabled = pauseController != null && pauseController.CanPause && !pauseController.IsPaused;
            GUI.backgroundColor = GUI.enabled ? new Color(0.62f, 0.78f, 0.88f, 1f) : new Color(0.46f, 0.48f, 0.50f, 1f);
            GUIStyle buttonStyle = RuntimePixelGuiUtility.CreateButtonStyle(GUI.skin.button, RuntimePixelAssetLoader.UiButtonSkill);
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.fontSize = Mathf.RoundToInt(13f * UIResponsiveLayout.ReadabilityScale);
            buttonStyle.wordWrap = true;
            KoreanFontManager.ApplyFont(buttonStyle);
            if (GUI.Button(rect, "일시\n정지", buttonStyle))
            {
                pauseController.Pause();
            }

            GUI.backgroundColor = previousBackground;
            GUI.enabled = previousEnabled;
        }

        private static Color BattleStateColor(BattleState state)
        {
            switch (state)
            {
                case BattleState.WaveRunning:
                    return new Color(0.46f, 1f, 0.58f, 1f);
                case BattleState.RuneSelection:
                    return new Color(0.56f, 0.78f, 1f, 1f);
                case BattleState.Victory:
                    return new Color(1f, 0.82f, 0.30f, 1f);
                case BattleState.Defeat:
                    return new Color(1f, 0.36f, 0.34f, 1f);
                default:
                    return new Color(0.82f, 0.86f, 0.88f, 1f);
            }
        }

        private static void DrawTintedBox(Rect rect, GUIStyle style, Color color)
        {
            Color previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUI.Box(rect, GUIContent.none, style);
            GUI.backgroundColor = previousBackground;
        }

        private static void DrawProgressBar(Rect rect, float ratio, Color fillColor)
        {
            Color previousColor = GUI.color;
            GUI.color = new Color(0.025f, 0.04f, 0.045f, 0.98f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
            if (ratio > 0f)
            {
                GUI.color = fillColor;
                GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(ratio), rect.height), Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
            }

            GUI.color = previousColor;
        }

        private static GUIStyle CreateLabelStyle(TextAnchor alignment, bool bold, float baseFontSize)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = alignment,
                fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
                fontSize = Mathf.RoundToInt(baseFontSize * UIResponsiveLayout.ReadabilityScale),
                clipping = TextClipping.Clip,
                wordWrap = false,
                padding = new RectOffset(0, 0, 0, 0)
            };
            KoreanFontManager.ApplyFont(style);
            return style;
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
            this.currentWave = Mathf.Max(0, currentWave);
            this.totalWaves = Mathf.Max(0, totalWaves);
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
