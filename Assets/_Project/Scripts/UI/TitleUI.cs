using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuneGate
{
    public readonly struct TitleMenuLayoutRects
    {
        public TitleMenuLayoutRects(Rect safeArea, Rect brandArea, Rect menuPanel, Rect menuContent, Rect modalPanel)
        {
            SafeArea = safeArea;
            BrandArea = brandArea;
            MenuPanel = menuPanel;
            MenuContent = menuContent;
            ModalPanel = modalPanel;
        }

        public Rect SafeArea { get; }
        public Rect BrandArea { get; }
        public Rect MenuPanel { get; }
        public Rect MenuContent { get; }
        public Rect ModalPanel { get; }
    }

    public sealed class TitleUI : MonoBehaviour
    {
        private const int ChapterStageCount = 10;

        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private string stageSelectSceneName = "StageSelectScene";

        private string feedbackMessage = string.Empty;
        private bool confirmReset;
        private bool confirmNewGame;
        private bool showSettings;
        private bool hasExistingProgress;
        private bool sceneTransitionRequested;

        private void OnEnable()
        {
            SaveData saveData = SaveManager.LoadOrCreate();
            hasExistingProgress = HasMeaningfulProgress(saveData);
            sceneTransitionRequested = false;
            confirmReset = false;
            confirmNewGame = false;
            showSettings = false;
            feedbackMessage = string.Empty;
        }

        private void OnGUI()
        {
            if (!drawRuntimeGui)
            {
                return;
            }

            UIResponsiveLayout.ApplyReadableDefaults();
            DrawTitleBackground();

            TitleMenuLayoutRects layout = CalculateLayoutForSize(Screen.width, Screen.height, showSettings);
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUIStyle buttonStyle = RuntimePixelGuiUtility.CreateSolidButtonStyle(GUI.skin.button);
            GUIStyle primaryButtonStyle = CreatePrimaryButtonStyle(buttonStyle);
            GUIStyle titleStyle = CreateLabelStyle(TextAnchor.MiddleCenter, ResolveFontSize(52, 32, 64), FontStyle.Bold);
            GUIStyle subtitleStyle = CreateLabelStyle(TextAnchor.UpperCenter, ResolveFontSize(24, 16, 30), FontStyle.Normal);
            GUIStyle sectionStyle = CreateLabelStyle(TextAnchor.MiddleLeft, ResolveFontSize(25, 18, 30), FontStyle.Bold);
            GUIStyle bodyStyle = CreateLabelStyle(TextAnchor.MiddleLeft, ResolveFontSize(19, 14, 24), FontStyle.Normal);
            GUIStyle centeredBodyStyle = CreateLabelStyle(TextAnchor.MiddleCenter, ResolveFontSize(18, 14, 22), FontStyle.Normal);

            DrawBrand(layout.BrandArea, titleStyle, subtitleStyle);
            GUI.Box(layout.MenuPanel, GUIContent.none, panelStyle);

            bool confirmationVisible = confirmNewGame || confirmReset;
            using (new GuiEnabledScope(!sceneTransitionRequested && !confirmationVisible))
            {
                if (showSettings)
                {
                    DrawSettings(layout.MenuContent, buttonStyle, sectionStyle, bodyStyle);
                }
                else
                {
                    DrawMainMenu(layout.MenuContent, buttonStyle, primaryButtonStyle, sectionStyle, bodyStyle, centeredBodyStyle);
                }
            }

            if (confirmationVisible)
            {
                DrawConfirmation(layout.ModalPanel, panelStyle, buttonStyle, primaryButtonStyle, sectionStyle, centeredBodyStyle);
            }
        }

        public static TitleMenuLayoutRects CalculateLayoutForSize(float width, float height, bool settingsOpen)
        {
            Rect safeArea = GameFrameLayout.SafeRectForSize(width, height);
            bool portrait = height >= width;
            float gap = Mathf.Clamp(Mathf.Min(width, height) * 0.025f, 12f, 32f);
            Rect menuPanel;
            Rect brandArea;

            if (portrait)
            {
                float panelWidth = Mathf.Min(safeArea.width, 920f);
                float panelRatio = settingsOpen ? 0.48f : 0.34f;
                float minimumHeight = settingsOpen ? 560f : 400f;
                float maximumHeight = settingsOpen ? 880f : 620f;
                float panelHeight = Mathf.Clamp(safeArea.height * panelRatio, minimumHeight, maximumHeight);
                panelHeight = Mathf.Min(panelHeight, safeArea.height * 0.68f);
                float bottomGap = Mathf.Clamp(safeArea.height * 0.035f, 20f, 72f);
                menuPanel = new Rect(
                    safeArea.x + (safeArea.width - panelWidth) * 0.5f,
                    safeArea.yMax - panelHeight - bottomGap,
                    panelWidth,
                    panelHeight);

                float brandY = safeArea.y + Mathf.Clamp(safeArea.height * 0.055f, 24f, 120f);
                float brandHeight = Mathf.Clamp(menuPanel.y - gap - brandY, 150f, 340f);
                brandArea = new Rect(safeArea.x, brandY, safeArea.width, brandHeight);
            }
            else
            {
                float panelWidth = Mathf.Min(safeArea.width * 0.52f, 780f);
                float panelHeight = Mathf.Min(safeArea.height * 0.78f, settingsOpen ? 660f : 560f);
                menuPanel = new Rect(
                    safeArea.xMax - panelWidth - gap,
                    safeArea.y + (safeArea.height - panelHeight) * 0.5f,
                    panelWidth,
                    panelHeight);
                brandArea = new Rect(
                    safeArea.x + gap,
                    safeArea.y + safeArea.height * 0.14f,
                    Mathf.Max(1f, menuPanel.x - safeArea.x - gap * 3f),
                    safeArea.height * 0.48f);
            }

            float inset = Mathf.Clamp(Mathf.Min(menuPanel.width, menuPanel.height) * 0.045f, 16f, 32f);
            Rect menuContent = new Rect(
                menuPanel.x + inset,
                menuPanel.y + inset,
                Mathf.Max(1f, menuPanel.width - inset * 2f),
                Mathf.Max(1f, menuPanel.height - inset * 2f));

            float modalWidth = Mathf.Min(760f, safeArea.width * 0.88f);
            float modalHeight = Mathf.Min(430f, safeArea.height * 0.58f);
            Rect modalPanel = new Rect(
                safeArea.x + (safeArea.width - modalWidth) * 0.5f,
                safeArea.y + (safeArea.height - modalHeight) * 0.5f,
                modalWidth,
                modalHeight);

            return new TitleMenuLayoutRects(safeArea, brandArea, menuPanel, menuContent, modalPanel);
        }

        public static bool HasMeaningfulProgress(SaveData saveData)
        {
            if (saveData == null)
            {
                return false;
            }

            return saveData.totalGold > 0 ||
                   (saveData.clearedStageIds != null && saveData.clearedStageIds.Count > 0) ||
                   !string.IsNullOrWhiteSpace(saveData.lastSelectedStageId) ||
                   saveData.hasSeenIntro ||
                   saveData.hasSeenTutorial;
        }

        public static string BuildProgressSummary(SaveData saveData)
        {
            if (saveData == null)
            {
                return "\uc0c8 \ubb38\uc9c0\uae30 \uae30\ub85d";
            }

            int clearedCount = saveData.clearedStageIds != null
                ? Mathf.Clamp(saveData.clearedStageIds.Count, 0, ChapterStageCount)
                : 0;
            string difficulty = GameTextMapper.Difficulty(saveData.selectedDifficultyId);
            return $"\ubcf4\uc720 \uace8\ub4dc {Mathf.Max(0, saveData.totalGold):N0} \u00b7 \ud074\ub9ac\uc5b4 {clearedCount}/{ChapterStageCount} \u00b7 \ub09c\uc774\ub3c4 {difficulty}";
        }

        public static string PrimaryActionLabel(bool existingProgress)
        {
            return existingProgress ? "\uc804\uc120\uc73c\ub85c \ub3cc\uc544\uac00\uae30" : "\uc0c8 \uc804\uc120 \uc2dc\uc791";
        }

        private void DrawMainMenu(Rect contentRect, GUIStyle buttonStyle, GUIStyle primaryButtonStyle, GUIStyle sectionStyle, GUIStyle bodyStyle, GUIStyle centeredBodyStyle)
        {
            GUILayout.BeginArea(contentRect);
            GUILayout.Label("\ubb38\uc9c0\uae30 \uae30\ub85d", sectionStyle, GUILayout.Height(UIResponsiveLayout.TouchHeight(32f)));
            GUILayout.Space(UIResponsiveLayout.SmallGap);
            GUILayout.Label(
                hasExistingProgress ? BuildProgressSummary(SaveManager.Current) : "\uc0c8\ub85c\uc6b4 \ubd09\ubb38 \uc804\uc120\uc774 \ubb38\uc9c0\uae30\ub97c \uae30\ub2e4\ub9bd\ub2c8\ub2e4.",
                bodyStyle,
                GUILayout.Height(UIResponsiveLayout.TouchHeight(34f)));
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(PrimaryActionLabel(hasExistingProgress), primaryButtonStyle, GUILayout.Height(UIResponsiveLayout.TouchHeight(58f))))
            {
                if (hasExistingProgress)
                {
                    LoadStageSelect();
                }
                else
                {
                    StartNewGame();
                }
            }

            GUILayout.Space(UIResponsiveLayout.Gap);
            GUILayout.BeginHorizontal();
            if (hasExistingProgress && GUILayout.Button("\uc0c8 \uc804\uc120", buttonStyle, GUILayout.Height(UIResponsiveLayout.TouchHeight(44f))))
            {
                HandleNewGamePressed();
            }

            if (GUILayout.Button("\uc124\uc815", buttonStyle, GUILayout.Height(UIResponsiveLayout.TouchHeight(44f))))
            {
                showSettings = true;
                feedbackMessage = string.Empty;
            }

            GUILayout.EndHorizontal();

            if (!string.IsNullOrWhiteSpace(feedbackMessage))
            {
                GUILayout.Space(UIResponsiveLayout.SmallGap);
                GUILayout.Label(feedbackMessage, centeredBodyStyle, GUILayout.Height(UIResponsiveLayout.TouchHeight(30f)));
            }

            GUILayout.Space(UIResponsiveLayout.SmallGap);
            GUILayout.Label("\ub85c\uceec \uc800\uc7a5 \u00b7 \uc624\ud504\ub77c\uc778 \ud50c\ub808\uc774", centeredBodyStyle, GUILayout.Height(UIResponsiveLayout.TouchHeight(28f)));
            GUILayout.EndArea();
        }

        private void DrawSettings(Rect contentRect, GUIStyle buttonStyle, GUIStyle sectionStyle, GUIStyle bodyStyle)
        {
            GUILayout.BeginArea(contentRect);
            GUILayout.BeginHorizontal();
            GUILayout.Label("\uc124\uc815", sectionStyle, GUILayout.Height(UIResponsiveLayout.TouchHeight(30f)));
            GUILayout.FlexibleSpace();
            RuntimePixelGuiUtility.DrawIcon(RuntimePixelAssetLoader.UiIconSettings, UIResponsiveLayout.TouchHeight(24f));
            GUILayout.EndHorizontal();

            string bgmLabel = AudioManager.BgmEnabled ? "BGM \ucf1c\uc9d0" : "BGM \uaebc\uc9d0";
            if (GUILayout.Button(bgmLabel, buttonStyle, GUILayout.Height(UIResponsiveLayout.TouchHeight(40f))))
            {
                bool enableBgm = !AudioManager.BgmEnabled;
                AudioManager.SetBgmEnabled(enableBgm);
                feedbackMessage = enableBgm ? "\ubc30\uacbd \uc74c\uc545\uc744 \ucf30\uc2b5\ub2c8\ub2e4." : "\ubc30\uacbd \uc74c\uc545\uc744 \uaecf\uc2b5\ub2c8\ub2e4.";
            }

            string bgmVolumeLabel = $"BGM \uc74c\ub7c9 {Mathf.RoundToInt(AudioManager.BgmVolume * 100f)}%";
            if (GUILayout.Button(bgmVolumeLabel, buttonStyle, GUILayout.Height(UIResponsiveLayout.TouchHeight(40f))))
            {
                AudioManager.SetBgmVolume(AudioManager.NextVolumeStep(AudioManager.BgmVolume));
                feedbackMessage = $"BGM \uc74c\ub7c9\uc744 {Mathf.RoundToInt(AudioManager.BgmVolume * 100f)}%\ub85c \uc124\uc815\ud588\uc2b5\ub2c8\ub2e4.";
            }

            string sfxLabel = AudioManager.SfxEnabled ? "SFX \ucf1c\uc9d0" : "SFX \uaebc\uc9d0";
            if (GUILayout.Button(sfxLabel, buttonStyle, GUILayout.Height(UIResponsiveLayout.TouchHeight(40f))))
            {
                bool enableSfx = !AudioManager.SfxEnabled;
                AudioManager.SetSfxEnabled(enableSfx);
                if (enableSfx)
                {
                    AudioManager.Play(SfxKey.ButtonClick);
                }

                feedbackMessage = enableSfx ? "\uc804\ud22c \ud6a8\uacfc\uc74c\uc744 \ucf30\uc2b5\ub2c8\ub2e4." : "\uc804\ud22c \ud6a8\uacfc\uc74c\uc744 \uaecf\uc2b5\ub2c8\ub2e4.";
            }

            string sfxVolumeLabel = $"SFX \uc74c\ub7c9 {Mathf.RoundToInt(AudioManager.SfxVolume * 100f)}%";
            if (GUILayout.Button(sfxVolumeLabel, buttonStyle, GUILayout.Height(UIResponsiveLayout.TouchHeight(40f))))
            {
                AudioManager.SetSfxVolume(AudioManager.NextVolumeStep(AudioManager.SfxVolume));
                AudioManager.Play(SfxKey.ButtonClick);
                feedbackMessage = $"SFX \uc74c\ub7c9\uc744 {Mathf.RoundToInt(AudioManager.SfxVolume * 100f)}%\ub85c \uc124\uc815\ud588\uc2b5\ub2c8\ub2e4.";
            }

            if (GUILayout.Button("\ub2e4\uc74c \uc804\ud22c\uc5d0\uc11c \ud29c\ud1a0\ub9ac\uc5bc \ub2e4\uc2dc \ubcf4\uae30", buttonStyle, GUILayout.Height(UIResponsiveLayout.TouchHeight(40f))))
            {
                SaveManager.ResetTutorialSeen();
                feedbackMessage = "\ub2e4\uc74c \uc804\ud22c\uc5d0\uc11c \ud29c\ud1a0\ub9ac\uc5bc\uc744 \ub2e4\uc2dc \ud45c\uc2dc\ud569\ub2c8\ub2e4.";
            }

            if (hasExistingProgress && GUILayout.Button("\uc800\uc7a5 \uae30\ub85d \ucd08\uae30\ud654", buttonStyle, GUILayout.Height(UIResponsiveLayout.TouchHeight(40f))))
            {
                confirmReset = true;
                confirmNewGame = false;
                feedbackMessage = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(feedbackMessage))
            {
                GUILayout.Space(UIResponsiveLayout.SmallGap);
                GUILayout.Label(feedbackMessage, bodyStyle, GUILayout.Height(UIResponsiveLayout.TouchHeight(34f)));
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("\ub3cc\uc544\uac00\uae30", buttonStyle, GUILayout.Height(UIResponsiveLayout.TouchHeight(42f))))
            {
                showSettings = false;
                feedbackMessage = string.Empty;
            }

            GUILayout.EndArea();
        }

        private void DrawConfirmation(Rect modalRect, GUIStyle panelStyle, GUIStyle buttonStyle, GUIStyle primaryButtonStyle, GUIStyle sectionStyle, GUIStyle bodyStyle)
        {
            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.72f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
            GUI.color = previousColor;
            GUI.Box(modalRect, GUIContent.none, panelStyle);

            float inset = Mathf.Clamp(Mathf.Min(modalRect.width, modalRect.height) * 0.075f, 18f, 34f);
            Rect contentRect = new Rect(modalRect.x + inset, modalRect.y + inset, modalRect.width - inset * 2f, modalRect.height - inset * 2f);
            GUILayout.BeginArea(contentRect);
            string title = confirmNewGame ? "\uc0c8 \uc804\uc120\uc744 \uc2dc\uc791\ud560\uae4c\uc694?" : "\uc800\uc7a5 \uae30\ub85d\uc744 \ucd08\uae30\ud654\ud560\uae4c\uc694?";
            string message = confirmNewGame
                ? "\ud604\uc7ac \uc9c4\ud589\uacfc \uc5c5\uadf8\ub808\uc774\ub4dc\uac00 \ucd08\uae30\ud654\ub429\ub2c8\ub2e4."
                : "\ubcf5\uad6c\ud560 \uc218 \uc5c6\uc2b5\ub2c8\ub2e4. \ucd08\uae30\ud654 \ud6c4 \uc0c8 \ubb38\uc9c0\uae30 \uae30\ub85d\uc774 \uc0dd\uc131\ub429\ub2c8\ub2e4.";
            GUILayout.Label(title, sectionStyle, GUILayout.Height(UIResponsiveLayout.TouchHeight(48f)));
            GUILayout.Space(UIResponsiveLayout.Gap);
            GUILayout.Label(message, bodyStyle, GUILayout.Height(UIResponsiveLayout.TouchHeight(74f)));
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("\ucde8\uc18c", buttonStyle, GUILayout.Height(UIResponsiveLayout.TouchHeight(46f))))
            {
                confirmNewGame = false;
                confirmReset = false;
            }

            string confirmLabel = confirmNewGame ? "\uc0c8 \uc804\uc120 \uc2dc\uc791" : "\ucd08\uae30\ud654";
            if (GUILayout.Button(confirmLabel, primaryButtonStyle, GUILayout.Height(UIResponsiveLayout.TouchHeight(46f))))
            {
                if (confirmNewGame)
                {
                    StartNewGame();
                }
                else
                {
                    ResetSaveFromSettings();
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private static void DrawBrand(Rect brandRect, GUIStyle titleStyle, GUIStyle subtitleStyle)
        {
            float titleHeight = brandRect.height * 0.54f;
            Rect titleRect = new Rect(brandRect.x, brandRect.y, brandRect.width, titleHeight);
            Rect englishRect = new Rect(brandRect.x, titleRect.yMax - 4f, brandRect.width, brandRect.height * 0.22f);
            Rect taglineRect = new Rect(brandRect.x, englishRect.yMax, brandRect.width, brandRect.height * 0.22f);

            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.8f);
            GUI.Label(new Rect(titleRect.x + 3f, titleRect.y + 3f, titleRect.width, titleRect.height), "\ub8ec\uac8c\uc774\ud2b8 \ub514\ud39c\uc2a4", titleStyle);
            GUI.color = new Color(0.92f, 0.82f, 0.48f, 1f);
            GUI.Label(titleRect, "\ub8ec\uac8c\uc774\ud2b8 \ub514\ud39c\uc2a4", titleStyle);
            GUI.color = new Color(0.76f, 0.88f, 0.9f, 1f);
            GUI.Label(englishRect, "RuneGate Defense", subtitleStyle);
            GUI.color = Color.white;
            GUI.Label(taglineRect, "\ubd09\ubb38 \uc804\uc120\uc744 \uc9c0\ucf1c\ub77c", subtitleStyle);
            GUI.color = previousColor;
        }

        private static GUIStyle CreatePrimaryButtonStyle(GUIStyle baseStyle)
        {
            GUIStyle style = new GUIStyle(baseStyle)
            {
                fontStyle = FontStyle.Bold,
                fontSize = ResolveFontSize(23, 17, 28),
                alignment = TextAnchor.MiddleCenter
            };
            KoreanFontManager.ApplyFont(style);
            return style;
        }

        private static GUIStyle CreateLabelStyle(TextAnchor alignment, int fontSize, FontStyle fontStyle)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = alignment,
                fontSize = fontSize,
                fontStyle = fontStyle,
                wordWrap = true,
                clipping = TextClipping.Clip
            };
            KoreanFontManager.ApplyFont(style);
            return style;
        }

        private static int ResolveFontSize(int preferred, int minimum, int maximum)
        {
            float scale = Mathf.Clamp(Mathf.Min(Screen.width, Screen.height) / 720f, 0.82f, 1.35f);
            return Mathf.Clamp(Mathf.RoundToInt(preferred * scale), minimum, maximum);
        }

        private static void DrawTitleBackground()
        {
            Texture2D background = RuntimePixelGuiUtility.LoadTexture(RuntimePixelAssetLoader.AppSplashBackground);
            if (background == null)
            {
                return;
            }

            Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
            Color previousColor = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(screenRect, background, ScaleMode.ScaleAndCrop, true);
            GUI.color = new Color(0f, 0f, 0f, 0.18f);
            GUI.DrawTexture(screenRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
            GUI.color = previousColor;
        }

        private void HandleNewGamePressed()
        {
            if (sceneTransitionRequested)
            {
                return;
            }

            if (hasExistingProgress)
            {
                confirmNewGame = true;
                confirmReset = false;
                return;
            }

            StartNewGame();
        }

        private void StartNewGame()
        {
            if (sceneTransitionRequested)
            {
                return;
            }

            SaveManager.ResetSave();
            hasExistingProgress = false;
            GameSession.ClearSelectedStage();
            GameSession.ClearLastBattleResult();
            LoadStageSelect();
        }

        private void ResetSaveFromSettings()
        {
            SaveManager.ResetSave();
            GameSession.ClearSelectedStage();
            GameSession.ClearLastBattleResult();
            confirmReset = false;
            confirmNewGame = false;
            hasExistingProgress = false;
            feedbackMessage = "\uc800\uc7a5 \uae30\ub85d\uc744 \ucd08\uae30\ud654\ud588\uc2b5\ub2c8\ub2e4.";
        }

        private void LoadStageSelect()
        {
            if (sceneTransitionRequested)
            {
                return;
            }

            sceneTransitionRequested = true;
            SaveManager.Load();
            SceneManager.LoadScene(stageSelectSceneName);
        }

        private readonly struct GuiEnabledScope : System.IDisposable
        {
            private readonly bool previousValue;

            public GuiEnabledScope(bool enabled)
            {
                previousValue = GUI.enabled;
                GUI.enabled = enabled;
            }

            public void Dispose()
            {
                GUI.enabled = previousValue;
            }
        }
    }
}
