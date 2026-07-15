using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public readonly struct BattlePauseMenuLayoutRects
    {
        public BattlePauseMenuLayoutRects(
            Rect safeArea,
            Rect panel,
            Rect headerArea,
            Rect summaryArea,
            Rect audioArea,
            Rect feedbackArea,
            Rect continueButton,
            Rect restartButton,
            Rect stageSelectButton,
            Rect confirmationPanel,
            Rect confirmationTitle,
            Rect confirmationBody,
            Rect confirmationCancelButton,
            Rect confirmationConfirmButton)
        {
            SafeArea = safeArea;
            Panel = panel;
            HeaderArea = headerArea;
            SummaryArea = summaryArea;
            AudioArea = audioArea;
            FeedbackArea = feedbackArea;
            ContinueButton = continueButton;
            RestartButton = restartButton;
            StageSelectButton = stageSelectButton;
            ConfirmationPanel = confirmationPanel;
            ConfirmationTitle = confirmationTitle;
            ConfirmationBody = confirmationBody;
            ConfirmationCancelButton = confirmationCancelButton;
            ConfirmationConfirmButton = confirmationConfirmButton;
        }

        public Rect SafeArea { get; }
        public Rect Panel { get; }
        public Rect HeaderArea { get; }
        public Rect SummaryArea { get; }
        public Rect AudioArea { get; }
        public Rect FeedbackArea { get; }
        public Rect ContinueButton { get; }
        public Rect RestartButton { get; }
        public Rect StageSelectButton { get; }
        public Rect ConfirmationPanel { get; }
        public Rect ConfirmationTitle { get; }
        public Rect ConfirmationBody { get; }
        public Rect ConfirmationCancelButton { get; }
        public Rect ConfirmationConfirmButton { get; }
    }

    public sealed class BattleHUD : MonoBehaviour
    {
        private const float HeaderPadding = 8f;
        private const float HeaderGap = 8f;
        private const float WaveAnnouncementDuration = 2.2f;
        private const float WaveAnnouncementFadeDuration = 0.35f;

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
        private string waveAnnouncementTitle = string.Empty;
        private string waveAnnouncementSubtitle = string.Empty;
        private string pendingRuneName = string.Empty;
        private Color waveAnnouncementColor = Color.white;
        private float waveAnnouncementTimer;
        private PauseConfirmation pauseConfirmation;
        private string pauseFeedbackMessage = string.Empty;

        private enum PauseConfirmation
        {
            None,
            Restart,
            StageSelect
        }

        public string CrystalHpText => crystalHpText;
        public string WaveText => waveText;
        public string BattleStateText => battleStateText;
        public string BossStatusText => bossStatusText;
        public string WaveAnnouncementTitleText => waveAnnouncementTitle;
        public string WaveAnnouncementSubtitleText => waveAnnouncementSubtitle;
        public bool IsWaveAnnouncementVisible => waveAnnouncementTimer > 0f;

        private void OnEnable()
        {
            AutoAssignReferences();

            if (battleManager != null)
            {
                battleManager.BattleStateChanged += HandleBattleStateChanged;
                battleManager.WaveChanged += HandleWaveChanged;
                battleManager.GoldChanged += HandleGoldChanged;
                battleManager.RuneSelected += HandleRuneSelected;
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
                battleManager.RuneSelected -= HandleRuneSelected;
            }

            if (crystalController != null)
            {
                crystalController.HpChanged -= HandleCrystalHpChanged;
                crystalController.ShieldChanged -= HandleCrystalShieldChanged;
                crystalController.Damaged -= HandleCrystalDamaged;
            }

            pauseConfirmation = PauseConfirmation.None;
            pauseFeedbackMessage = string.Empty;
        }

        private void Update()
        {
            RefreshBossStatus();
            if (waveAnnouncementTimer > 0f && !IsWaveAnnouncementBlocked())
            {
                waveAnnouncementTimer = Mathf.Max(0f, waveAnnouncementTimer - Time.unscaledDeltaTime);
            }

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
            DrawWaveAnnouncement(battleFrame.BattleFieldFrame);

            if (pauseController != null && pauseController.IsPaused)
            {
                DrawPausePopup();
            }
            else
            {
                pauseConfirmation = PauseConfirmation.None;
                pauseFeedbackMessage = string.Empty;
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

        public static BattlePauseMenuLayoutRects CalculatePauseMenuLayoutForSize(float width, float height)
        {
            Rect safeArea = GameFrameLayout.SafeRectForSize(width, height);
            bool portrait = height >= width;
            float panelWidth = Mathf.Min(safeArea.width * 0.92f, portrait ? 760f : 940f);
            float panelHeight = Mathf.Min(safeArea.height * (portrait ? 0.70f : 0.86f), portrait ? 980f : 640f);
            Rect panel = new Rect(
                safeArea.x + (safeArea.width - panelWidth) * 0.5f,
                safeArea.y + (safeArea.height - panelHeight) * 0.5f,
                panelWidth,
                panelHeight);

            float inset = Mathf.Clamp(Mathf.Min(panel.width, panel.height) * 0.035f, 14f, 28f);
            float gap = Mathf.Clamp(Mathf.Min(panel.width, panel.height) * 0.018f, 8f, 16f);
            Rect content = new Rect(panel.x + inset, panel.y + inset, panel.width - inset * 2f, panel.height - inset * 2f);
            float headerHeight = Mathf.Clamp(content.height * 0.12f, 72f, 112f);
            float feedbackHeight = Mathf.Clamp(content.height * 0.06f, 38f, 54f);
            float primaryHeight = Mathf.Clamp(content.height * 0.075f, 52f, 68f);
            float secondaryHeight = Mathf.Clamp(content.height * 0.07f, 50f, 62f);
            float remainingHeight = Mathf.Max(1f, content.height - headerHeight - feedbackHeight - primaryHeight - secondaryHeight - gap * 5f);
            float summaryHeight = Mathf.Clamp(remainingHeight * 0.42f, 112f, 220f);
            float audioHeight = Mathf.Max(1f, remainingHeight - summaryHeight);

            float y = content.y;
            Rect headerArea = new Rect(content.x, y, content.width, headerHeight);
            y = headerArea.yMax + gap;
            Rect summaryArea = new Rect(content.x, y, content.width, summaryHeight);
            y = summaryArea.yMax + gap;
            Rect audioArea = new Rect(content.x, y, content.width, audioHeight);
            y = audioArea.yMax + gap;
            Rect feedbackArea = new Rect(content.x, y, content.width, feedbackHeight);
            y = feedbackArea.yMax + gap;
            Rect continueButton = new Rect(content.x, y, content.width, primaryHeight);
            y = continueButton.yMax + gap;

            float secondaryGap = gap;
            float secondaryWidth = Mathf.Max(1f, (content.width - secondaryGap) * 0.5f);
            Rect restartButton = new Rect(content.x, y, secondaryWidth, secondaryHeight);
            Rect stageSelectButton = new Rect(restartButton.xMax + secondaryGap, y, secondaryWidth, secondaryHeight);

            float confirmationWidth = Mathf.Min(panel.width * 0.86f, 580f);
            float confirmationHeight = Mathf.Min(panel.height * 0.48f, 380f);
            Rect confirmationPanel = new Rect(
                panel.center.x - confirmationWidth * 0.5f,
                panel.center.y - confirmationHeight * 0.5f,
                confirmationWidth,
                confirmationHeight);
            float confirmationInset = Mathf.Clamp(Mathf.Min(confirmationWidth, confirmationHeight) * 0.07f, 16f, 26f);
            float confirmationGap = Mathf.Clamp(gap, 8f, 14f);
            Rect confirmationContent = new Rect(
                confirmationPanel.x + confirmationInset,
                confirmationPanel.y + confirmationInset,
                confirmationPanel.width - confirmationInset * 2f,
                confirmationPanel.height - confirmationInset * 2f);
            float confirmationTitleHeight = Mathf.Clamp(confirmationContent.height * 0.22f, 48f, 72f);
            float confirmationButtonHeight = Mathf.Clamp(confirmationContent.height * 0.20f, 48f, 62f);
            Rect confirmationTitle = new Rect(confirmationContent.x, confirmationContent.y, confirmationContent.width, confirmationTitleHeight);
            Rect confirmationButtonRow = new Rect(
                confirmationContent.x,
                confirmationContent.yMax - confirmationButtonHeight,
                confirmationContent.width,
                confirmationButtonHeight);
            Rect confirmationBody = new Rect(
                confirmationContent.x,
                confirmationTitle.yMax + confirmationGap,
                confirmationContent.width,
                Mathf.Max(1f, confirmationButtonRow.y - confirmationTitle.yMax - confirmationGap * 2f));
            float confirmationButtonWidth = Mathf.Max(1f, (confirmationButtonRow.width - confirmationGap) * 0.5f);
            Rect confirmationCancelButton = new Rect(confirmationButtonRow.x, confirmationButtonRow.y, confirmationButtonWidth, confirmationButtonRow.height);
            Rect confirmationConfirmButton = new Rect(confirmationCancelButton.xMax + confirmationGap, confirmationButtonRow.y, confirmationButtonWidth, confirmationButtonRow.height);

            return new BattlePauseMenuLayoutRects(
                safeArea,
                panel,
                headerArea,
                summaryArea,
                audioArea,
                feedbackArea,
                continueButton,
                restartButton,
                stageSelectButton,
                confirmationPanel,
                confirmationTitle,
                confirmationBody,
                confirmationCancelButton,
                confirmationConfirmButton);
        }

        public static string PauseReasonText(bool pausedByLifecycle)
        {
            return pausedByLifecycle
                ? "\uc571\uc774 \ubc31\uadf8\ub77c\uc6b4\ub4dc\ub85c \uc804\ud658\ub418\uc5b4 \uc804\ud22c\ub97c \uc548\uc804\ud558\uac8c \uba48\ucdc4\uc2b5\ub2c8\ub2e4."
                : "\uc804\ud22c \uc0c1\ud669\uc744 \ud655\uc778\ud55c \ub4a4 \uacc4\uc18d\ud560 \uc218 \uc788\uc2b5\ub2c8\ub2e4.";
        }

        public static string PauseConfirmationTitle(bool restart)
        {
            return restart ? "\uc804\ud22c\ub97c \ub2e4\uc2dc \uc2dc\uc791\ud560\uae4c\uc694?" : "\uc2a4\ud14c\uc774\uc9c0 \uc120\ud0dd\uc73c\ub85c \ub3cc\uc544\uac08\uae4c\uc694?";
        }

        public static string PauseConfirmationMessage(bool restart)
        {
            return restart
                ? "\ud604\uc7ac \uc804\ud22c\uc5d0\uc11c \uc5bb\uc740 \uace8\ub4dc\uc640 \ub8ec \ud6a8\uacfc\uac00 \ucd08\uae30\ud654\ub418\uace0 \ucc98\uc74c\ubd80\ud130 \ub2e4\uc2dc \uc2dc\uc791\ud569\ub2c8\ub2e4."
                : "\ud604\uc7ac \uc804\ud22c \uc9c4\ud589\uc740 \uc800\uc7a5\ub418\uc9c0 \uc54a\uc2b5\ub2c8\ub2e4. \uae30\uc874 \uc2a4\ud14c\uc774\uc9c0 \ud574\uae08\uacfc \uc5c5\uadf8\ub808\uc774\ub4dc\ub294 \uc720\uc9c0\ub429\ub2c8\ub2e4.";
        }

        public static string PauseConfirmationConfirmLabel(bool restart)
        {
            return restart ? "\ub2e4\uc2dc \uc2dc\uc791" : "\uc2a4\ud14c\uc774\uc9c0 \uc120\ud0dd";
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

        public static Rect CalculateWaveAnnouncementRect(Rect battlefieldRect)
        {
            float width = Mathf.Clamp(battlefieldRect.width * 0.78f, 300f, 760f);
            float height = Mathf.Clamp(battlefieldRect.height * 0.13f, 96f, 148f);
            float centerY = battlefieldRect.y + battlefieldRect.height * 0.24f;
            Rect rect = new Rect(
                battlefieldRect.center.x - width * 0.5f,
                centerY - height * 0.5f,
                width,
                height);
            return ClampRectInside(rect, battlefieldRect);
        }

        public static string WaveAnnouncementTitle(int currentWave, int totalWaves, bool isBossWave)
        {
            if (isBossWave)
            {
                return "봉문 경보 · 보스 출현";
            }

            if (totalWaves > 0 && currentWave >= totalWaves)
            {
                return "최종 웨이브";
            }

            return currentWave <= 1 ? "전투 개시" : $"웨이브 {Mathf.Max(1, currentWave)}";
        }

        public static string WaveAnnouncementSubtitle(int currentWave, int totalWaves, int enemyCount, string appliedRuneName)
        {
            string waveProgress = totalWaves > 0
                ? $"웨이브 {Mathf.Max(1, currentWave)}/{totalWaves}"
                : $"웨이브 {Mathf.Max(1, currentWave)}";
            string enemySummary = enemyCount > 0 ? $" · 적 {enemyCount}기" : string.Empty;
            string runeSummary = string.IsNullOrWhiteSpace(appliedRuneName) ? string.Empty : $" · {appliedRuneName} 적용";
            return waveProgress + enemySummary + runeSummary;
        }

        public static Color WaveAnnouncementAccent(bool isBossWave, bool isFinalWave)
        {
            if (isBossWave)
            {
                return new Color(0.94f, 0.24f, 0.20f, 1f);
            }

            return isFinalWave
                ? new Color(1f, 0.66f, 0.18f, 1f)
                : new Color(0.28f, 0.82f, 0.68f, 1f);
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

        private void DrawWaveAnnouncement(Rect battlefieldRect)
        {
            if (waveAnnouncementTimer <= 0f || battleManager == null || battleManager.CurrentState != BattleState.WaveRunning || IsWaveAnnouncementBlocked())
            {
                return;
            }

            float elapsed = WaveAnnouncementDuration - waveAnnouncementTimer;
            float fadeIn = Mathf.Clamp01(elapsed / WaveAnnouncementFadeDuration);
            float fadeOut = Mathf.Clamp01(waveAnnouncementTimer / WaveAnnouncementFadeDuration);
            float alpha = Mathf.Min(fadeIn, fadeOut);
            float scale = 0.98f + Mathf.Sin(Mathf.Clamp01(elapsed / WaveAnnouncementDuration) * Mathf.PI) * 0.02f;
            Rect baseRect = CalculateWaveAnnouncementRect(battlefieldRect);
            Rect rect = new Rect(
                baseRect.center.x - baseRect.width * scale * 0.5f,
                baseRect.center.y - baseRect.height * scale * 0.5f,
                baseRect.width * scale,
                baseRect.height * scale);

            int previousDepth = GUI.depth;
            Color previousColor = GUI.color;
            Color previousBackground = GUI.backgroundColor;
            GUI.depth = -60;
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.backgroundColor = new Color(waveAnnouncementColor.r, waveAnnouncementColor.g, waveAnnouncementColor.b, 0.92f);
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUI.Box(rect, GUIContent.none, panelStyle);
            GUI.backgroundColor = previousBackground;

            float stripeHeight = Mathf.Clamp(rect.height * 0.08f, 6f, 10f);
            Color accent = new Color(waveAnnouncementColor.r, waveAnnouncementColor.g, waveAnnouncementColor.b, alpha);
            DrawColoredRect(new Rect(rect.x + 8f, rect.y + 7f, rect.width - 16f, stripeHeight), accent);
            float titleHeight = rect.height * 0.52f;
            GUIStyle titleStyle = CreateLabelStyle(TextAnchor.LowerCenter, true, 22f);
            titleStyle.normal.textColor = waveAnnouncementColor;
            GUI.Label(new Rect(rect.x + 14f, rect.y + stripeHeight + 4f, rect.width - 28f, titleHeight), waveAnnouncementTitle, titleStyle);
            GUIStyle subtitleStyle = CreateLabelStyle(TextAnchor.UpperCenter, false, 12f);
            subtitleStyle.normal.textColor = new Color(0.84f, 0.90f, 0.89f, 1f);
            GUI.Label(new Rect(rect.x + 14f, rect.y + stripeHeight + 4f + titleHeight, rect.width - 28f, rect.height - titleHeight - stripeHeight - 12f), waveAnnouncementSubtitle, subtitleStyle);

            GUI.color = previousColor;
            GUI.depth = previousDepth;
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
            UIPopupGuiUtility.DrawDimOverlay(0.72f);
            BattlePauseMenuLayoutRects layout = CalculatePauseMenuLayoutForSize(Screen.width, Screen.height);
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUIStyle sectionStyle = RuntimePixelGuiUtility.CreateSolidPanelStyle(GUI.skin.box, true);
            GUIStyle buttonStyle = CreatePauseButtonStyle();
            GUIStyle titleStyle = CreateLabelStyle(TextAnchor.MiddleLeft, true, 24f);
            GUIStyle subtitleStyle = CreateLabelStyle(TextAnchor.UpperLeft, false, 13f);
            subtitleStyle.wordWrap = true;
            GUIStyle feedbackStyle = CreateLabelStyle(TextAnchor.MiddleCenter, false, 12f);
            feedbackStyle.wordWrap = true;

            GUI.SetNextControlName("PopupLayer_BattlePause");
            GUI.Box(layout.Panel, GUIContent.none, panelStyle);
            DrawColoredRect(new Rect(layout.Panel.x + 8f, layout.Panel.y + 7f, layout.Panel.width - 16f, 7f), new Color(0.25f, 0.82f, 0.64f, 0.95f));

            float titleHeight = layout.HeaderArea.height * 0.46f;
            GUI.Label(new Rect(layout.HeaderArea.x, layout.HeaderArea.y, layout.HeaderArea.width, titleHeight), "\uc804\ud22c \uc77c\uc2dc\uc815\uc9c0", titleStyle);
            GUI.Label(
                new Rect(layout.HeaderArea.x, layout.HeaderArea.y + titleHeight, layout.HeaderArea.width, layout.HeaderArea.height - titleHeight),
                PauseReasonText(pauseController.PausedByLifecycle),
                subtitleStyle);

            DrawPauseSummary(layout.SummaryArea, sectionStyle);

            bool confirmationVisible = pauseConfirmation != PauseConfirmation.None;
            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && !confirmationVisible;
            DrawPauseAudioControls(layout.AudioArea, sectionStyle, buttonStyle);

            string feedback = string.IsNullOrWhiteSpace(pauseFeedbackMessage)
                ? "\uc74c\ud5a5 \uc124\uc815\uc740 \uc989\uc2dc \uc800\uc7a5\ub429\ub2c8\ub2e4."
                : pauseFeedbackMessage;
            GUI.Label(layout.FeedbackArea, feedback, feedbackStyle);

            Color previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.28f, 0.86f, 0.62f, 1f);
            if (GUI.Button(layout.ContinueButton, "\uacc4\uc18d\ud558\uae30", buttonStyle))
            {
                PlayButtonClick();
                pauseConfirmation = PauseConfirmation.None;
                pauseFeedbackMessage = string.Empty;
                pauseController.Resume();
            }

            GUI.backgroundColor = new Color(0.92f, 0.62f, 0.20f, 1f);
            if (GUI.Button(layout.RestartButton, "\uc804\ud22c \uc7ac\uc2dc\uc791", buttonStyle))
            {
                PlayButtonClick();
                pauseConfirmation = PauseConfirmation.Restart;
                pauseFeedbackMessage = string.Empty;
            }

            GUI.backgroundColor = new Color(0.34f, 0.58f, 0.82f, 1f);
            if (GUI.Button(layout.StageSelectButton, "\uc2a4\ud14c\uc774\uc9c0 \uc120\ud0dd", buttonStyle))
            {
                PlayButtonClick();
                pauseConfirmation = PauseConfirmation.StageSelect;
                pauseFeedbackMessage = string.Empty;
            }

            GUI.backgroundColor = previousBackground;
            GUI.enabled = previousEnabled;

            if (confirmationVisible)
            {
                DrawPauseConfirmation(layout, panelStyle, buttonStyle);
            }
        }

        private void DrawPauseSummary(Rect rect, GUIStyle sectionStyle)
        {
            float gap = Mathf.Clamp(Mathf.Min(rect.width, rect.height) * 0.05f, 6f, 12f);
            float cardWidth = Mathf.Max(1f, (rect.width - gap) * 0.5f);
            float cardHeight = Mathf.Max(1f, (rect.height - gap) * 0.5f);
            Rect[] cards =
            {
                new Rect(rect.x, rect.y, cardWidth, cardHeight),
                new Rect(rect.x + cardWidth + gap, rect.y, cardWidth, cardHeight),
                new Rect(rect.x, rect.y + cardHeight + gap, cardWidth, cardHeight),
                new Rect(rect.x + cardWidth + gap, rect.y + cardHeight + gap, cardWidth, cardHeight)
            };

            string stageName = battleManager != null && battleManager.ActiveStageData != null
                ? GameTextMapper.StageName(battleManager.ActiveStageData)
                : "\uc2a4\ud14c\uc774\uc9c0 \uc815\ubcf4 \uc5c6\uc74c";
            string waveValue = totalWaves > 0 ? $"{Mathf.Max(1, currentWave)}/{totalWaves}" : "-";
            string crystalValue = crystalMaxHp > 0 ? $"{crystalCurrentHp}/{crystalMaxHp}" : "-";

            DrawPauseSummaryCard(cards[0], "\ud604\uc7ac \uc804\uc120", stageName, new Color(0.28f, 0.62f, 0.52f, 1f), sectionStyle);
            DrawPauseSummaryCard(cards[1], "\uc6e8\uc774\ube0c", waveValue, new Color(0.88f, 0.66f, 0.22f, 1f), sectionStyle);
            DrawPauseSummaryCard(cards[2], "\ud06c\ub9ac\uc2a4\ud0c8", crystalValue, CrystalHealthColor(crystalMaxHp > 0 ? crystalCurrentHp / (float)crystalMaxHp : 0f), sectionStyle);
            DrawPauseSummaryCard(cards[3], "\uc804\ud22c \uace8\ub4dc", gold.ToString(), new Color(0.94f, 0.76f, 0.30f, 1f), sectionStyle);
        }

        private static void DrawPauseSummaryCard(Rect rect, string label, string value, Color accent, GUIStyle sectionStyle)
        {
            DrawTintedBox(rect, sectionStyle, new Color(accent.r, accent.g, accent.b, 0.48f));
            float padding = Mathf.Clamp(rect.height * 0.12f, 6f, 12f);
            GUIStyle labelStyle = CreateLabelStyle(TextAnchor.MiddleLeft, false, 11f);
            labelStyle.normal.textColor = new Color(0.72f, 0.82f, 0.80f, 1f);
            GUIStyle valueStyle = CreateLabelStyle(TextAnchor.MiddleRight, true, 14f);
            valueStyle.wordWrap = true;
            GUI.Label(new Rect(rect.x + padding, rect.y + padding * 0.4f, rect.width * 0.40f, rect.height - padding), label, labelStyle);
            GUI.Label(new Rect(rect.x + rect.width * 0.38f, rect.y + padding * 0.4f, rect.width * 0.62f - padding, rect.height - padding), value, valueStyle);
        }

        private void DrawPauseAudioControls(Rect rect, GUIStyle sectionStyle, GUIStyle buttonStyle)
        {
            GUI.Box(rect, GUIContent.none, sectionStyle);
            float inset = Mathf.Clamp(rect.height * 0.07f, 8f, 16f);
            float gap = Mathf.Clamp(rect.height * 0.05f, 6f, 12f);
            float headingHeight = Mathf.Clamp(rect.height * 0.17f, 26f, 38f);
            Rect heading = new Rect(rect.x + inset, rect.y + inset * 0.5f, rect.width - inset * 2f, headingHeight);
            GUIStyle headingStyle = CreateLabelStyle(TextAnchor.MiddleLeft, true, 15f);
            GUI.Label(heading, "\uc74c\ud5a5 \uc124\uc815", headingStyle);

            float rowTop = heading.yMax + gap;
            float availableHeight = Mathf.Max(1f, rect.yMax - inset - rowTop - gap);
            float rowHeight = Mathf.Max(1f, availableHeight * 0.5f);
            Rect bgmRow = new Rect(rect.x + inset, rowTop, rect.width - inset * 2f, rowHeight);
            Rect sfxRow = new Rect(rect.x + inset, bgmRow.yMax + gap, rect.width - inset * 2f, rowHeight);
            DrawAudioRow(bgmRow, true, buttonStyle, gap);
            DrawAudioRow(sfxRow, false, buttonStyle, gap);
        }

        private void DrawAudioRow(Rect row, bool bgm, GUIStyle buttonStyle, float gap)
        {
            float buttonWidth = Mathf.Max(1f, (row.width - gap) * 0.5f);
            Rect toggleRect = new Rect(row.x, row.y, buttonWidth, row.height);
            Rect volumeRect = new Rect(toggleRect.xMax + gap, row.y, buttonWidth, row.height);
            bool enabled = bgm ? AudioManager.BgmEnabled : AudioManager.SfxEnabled;
            float volume = bgm ? AudioManager.BgmVolume : AudioManager.SfxVolume;
            string channel = bgm ? "BGM" : "SFX";

            Color previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = enabled ? new Color(0.32f, 0.76f, 0.58f, 1f) : new Color(0.48f, 0.50f, 0.52f, 1f);
            if (GUI.Button(toggleRect, $"{channel}  {(enabled ? "\ucf1c\uc9d0" : "\uaebc\uc9d0")}", buttonStyle))
            {
                bool nextEnabled = !enabled;
                if (bgm)
                {
                    PlayButtonClick();
                    AudioManager.SetBgmEnabled(nextEnabled);
                    pauseFeedbackMessage = nextEnabled ? "\ubc30\uacbd \uc74c\uc545\uc744 \ucf30\uc2b5\ub2c8\ub2e4." : "\ubc30\uacbd \uc74c\uc545\uc744 \uaecf\uc2b5\ub2c8\ub2e4.";
                }
                else
                {
                    if (!nextEnabled)
                    {
                        PlayButtonClick();
                    }

                    AudioManager.SetSfxEnabled(nextEnabled);
                    if (nextEnabled)
                    {
                        PlayButtonClick();
                    }

                    pauseFeedbackMessage = nextEnabled ? "\uc804\ud22c \ud6a8\uacfc\uc74c\uc744 \ucf30\uc2b5\ub2c8\ub2e4." : "\uc804\ud22c \ud6a8\uacfc\uc74c\uc744 \uaecf\uc2b5\ub2c8\ub2e4.";
                }
            }

            GUI.backgroundColor = new Color(0.38f, 0.60f, 0.78f, 1f);
            if (GUI.Button(volumeRect, $"{channel}  {Mathf.RoundToInt(volume * 100f)}%", buttonStyle))
            {
                float nextVolume = AudioManager.NextVolumeStep(volume);
                if (bgm)
                {
                    AudioManager.SetBgmVolume(nextVolume);
                }
                else
                {
                    AudioManager.SetSfxVolume(nextVolume);
                }

                PlayButtonClick();
                pauseFeedbackMessage = $"{channel} \uc74c\ub7c9\uc744 {Mathf.RoundToInt(nextVolume * 100f)}%\ub85c \uc124\uc815\ud588\uc2b5\ub2c8\ub2e4.";
            }

            GUI.backgroundColor = previousBackground;
        }

        private void DrawPauseConfirmation(BattlePauseMenuLayoutRects layout, GUIStyle panelStyle, GUIStyle buttonStyle)
        {
            DrawColoredRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.66f));
            GUI.Box(layout.ConfirmationPanel, GUIContent.none, panelStyle);
            bool restart = pauseConfirmation == PauseConfirmation.Restart;
            GUIStyle titleStyle = CreateLabelStyle(TextAnchor.MiddleCenter, true, 21f);
            titleStyle.wordWrap = true;
            GUIStyle bodyStyle = CreateLabelStyle(TextAnchor.MiddleCenter, false, 14f);
            bodyStyle.wordWrap = true;
            GUI.Label(layout.ConfirmationTitle, PauseConfirmationTitle(restart), titleStyle);
            GUI.Label(layout.ConfirmationBody, PauseConfirmationMessage(restart), bodyStyle);

            Color previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.48f, 0.52f, 0.54f, 1f);
            if (GUI.Button(layout.ConfirmationCancelButton, "\ucde8\uc18c", buttonStyle))
            {
                PlayButtonClick();
                pauseConfirmation = PauseConfirmation.None;
            }

            GUI.backgroundColor = restart
                ? new Color(0.90f, 0.50f, 0.18f, 1f)
                : new Color(0.30f, 0.66f, 0.82f, 1f);
            if (GUI.Button(layout.ConfirmationConfirmButton, PauseConfirmationConfirmLabel(restart), buttonStyle))
            {
                PlayButtonClick();
                pauseConfirmation = PauseConfirmation.None;
                if (restart)
                {
                    pauseController.RestartBattle();
                }
                else
                {
                    pauseController.OpenStageSelect();
                }
            }

            GUI.backgroundColor = previousBackground;
        }

        private static GUIStyle CreatePauseButtonStyle()
        {
            GUIStyle style = RuntimePixelGuiUtility.CreateSolidButtonStyle(GUI.skin.button);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;
            style.fontSize = Mathf.RoundToInt(15f * UIResponsiveLayout.ReadabilityScale);
            style.padding = new RectOffset(8, 8, 4, 4);
            return style;
        }

        private static void PlayButtonClick()
        {
            AudioManager.Play(SfxKey.ButtonClick);
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
            if (currentWave <= 0)
            {
                waveAnnouncementTimer = 0f;
                return;
            }

            WaveData waveData = ResolveWaveData(currentWave);
            bool isBossWave = waveData != null && waveData.IsBossWave;
            bool isFinalWave = totalWaves > 0 && currentWave >= totalWaves;
            waveAnnouncementTitle = WaveAnnouncementTitle(currentWave, totalWaves, isBossWave);
            waveAnnouncementSubtitle = WaveAnnouncementSubtitle(currentWave, totalWaves, CountWaveEnemies(waveData), pendingRuneName);
            waveAnnouncementColor = WaveAnnouncementAccent(isBossWave, isFinalWave);
            waveAnnouncementTimer = WaveAnnouncementDuration;
            pendingRuneName = string.Empty;
        }

        private void HandleBattleStateChanged(BattleState battleState)
        {
            battleStateText = GameTextMapper.BattleStateName(battleState);
            if (battleState != BattleState.WaveRunning)
            {
                waveAnnouncementTimer = 0f;
            }
        }

        private void HandleRuneSelected(RuneData runeData)
        {
            pendingRuneName = runeData != null ? runeData.DisplayName : string.Empty;
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

        private WaveData ResolveWaveData(int waveNumber)
        {
            if (battleManager == null || battleManager.ActiveStageData == null || battleManager.ActiveStageData.Waves == null)
            {
                return null;
            }

            int index = waveNumber - 1;
            return index >= 0 && index < battleManager.ActiveStageData.Waves.Count
                ? battleManager.ActiveStageData.Waves[index]
                : null;
        }

        private static int CountWaveEnemies(WaveData waveData)
        {
            if (waveData == null || waveData.Spawns == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < waveData.Spawns.Count; i++)
            {
                WaveSpawnData spawn = waveData.Spawns[i];
                if (spawn != null)
                {
                    count += Mathf.Max(0, spawn.Count);
                }
            }

            return count;
        }

        private static Rect ClampRectInside(Rect rect, Rect bounds)
        {
            float width = Mathf.Min(rect.width, Mathf.Max(1f, bounds.width));
            float height = Mathf.Min(rect.height, Mathf.Max(1f, bounds.height));
            float x = Mathf.Clamp(rect.x, bounds.x, Mathf.Max(bounds.x, bounds.xMax - width));
            float y = Mathf.Clamp(rect.y, bounds.y, Mathf.Max(bounds.y, bounds.yMax - height));
            return new Rect(x, y, width, height);
        }

        private static void DrawColoredRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
            GUI.color = previousColor;
        }

        private bool IsWaveAnnouncementBlocked()
        {
            if (pauseController != null && pauseController.IsPaused)
            {
                return true;
            }

            TutorialManager tutorial = FindAnyObjectByType<TutorialManager>();
            return tutorial != null && tutorial.IsVisible;
        }
    }
}
