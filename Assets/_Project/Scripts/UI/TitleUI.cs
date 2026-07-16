using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
    public sealed class TitleUI : MonoBehaviour
    {
        private const int ChapterStageCount = 10;

        [Header("Theme")]
        [SerializeField] private RuneGateUiTheme theme;
        [SerializeField] private string stageSelectSceneName = "StageSelectScene";

        [Header("Root Layout")]
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private RectTransform backgroundLayer;
        [SerializeField] private RawImage backgroundImage;
        [SerializeField] private RectTransform safeAreaRoot;
        [SerializeField] private RectTransform frameRoot;
        [SerializeField] private RectTransform brandArea;
        [SerializeField] private RectTransform menuPanel;
        [SerializeField] private RectTransform headerArea;
        [SerializeField] private RectTransform bodyArea;
        [SerializeField] private RectTransform actionArea;
        [SerializeField] private RectTransform statusFooter;
        [SerializeField] private RectTransform overlayLayer;
        [SerializeField] private CanvasGroup modalGroup;
        [SerializeField] private RectTransform modalPanel;

        [Header("Content")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text englishTitleText;
        [SerializeField] private TMP_Text taglineText;
        [SerializeField] private TMP_Text headerText;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private TMP_Text feedbackText;
        [SerializeField] private TMP_Text footerText;
        [SerializeField] private RectTransform settingsViewport;
        [SerializeField] private RectTransform settingsContent;
        [SerializeField] private GameObject mainBody;
        [SerializeField] private GameObject settingsBody;

        [Header("Actions")]
        [SerializeField] private Button primaryButton;
        [SerializeField] private TMP_Text primaryButtonText;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button bgmToggleButton;
        [SerializeField] private TMP_Text bgmToggleText;
        [SerializeField] private Button bgmVolumeButton;
        [SerializeField] private TMP_Text bgmVolumeText;
        [SerializeField] private Button sfxToggleButton;
        [SerializeField] private TMP_Text sfxToggleText;
        [SerializeField] private Button sfxVolumeButton;
        [SerializeField] private TMP_Text sfxVolumeText;
        [SerializeField] private Button tutorialButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private TMP_Text modalTitleText;
        [SerializeField] private TMP_Text modalBodyText;
        [SerializeField] private Button modalCancelButton;
        [SerializeField] private Button modalConfirmButton;
        [SerializeField] private TMP_Text modalConfirmText;

        private string feedbackMessage = string.Empty;
        private bool hasExistingProgress;
        private bool sceneTransitionRequested;
        private Vector2 lastSafeSize = new Vector2(-1f, -1f);
        private float lastScaleFactor = -1f;
        private bool listenersBound;

        public bool IsReady { get; private set; }
        public TitleViewMode ViewMode { get; private set; }
        public TitleConfirmAction ConfirmAction { get; private set; }
        public TitleViewData CurrentViewData => new TitleViewData(
            hasExistingProgress,
            hasExistingProgress ? BuildProgressSummary(SaveManager.Current) : "새로운 봉문 전선이 문지기를 기다립니다.",
            PrimaryActionLabel(hasExistingProgress),
            feedbackMessage);

        private void Awake()
        {
            EnsureCanvasComponents();
            if (frameRoot == null)
            {
                RebuildView();
            }

            ApplyTheme();
            IsReady = theme != null && frameRoot != null && menuPanel != null && modalPanel != null;
            if (!IsReady)
            {
                Debug.LogError("TitleUI is missing its theme or view hierarchy. Rebuild UI assets from Tools/RuneGate/Build uGUI Assets.");
            }
        }

        private void OnEnable()
        {
            SaveData saveData = SaveManager.LoadOrCreate();
            hasExistingProgress = HasMeaningfulProgress(saveData);
            sceneTransitionRequested = false;
            ViewMode = TitleViewMode.Main;
            ConfirmAction = TitleConfirmAction.None;
            feedbackMessage = string.Empty;
            BindListeners();
            Refresh();
        }

        private void Start()
        {
            ApplyResponsiveLayout(true);
            Refresh();
        }

        private void Update()
        {
            ApplyResponsiveLayout(false);
        }

        private void OnDisable()
        {
            UnbindListeners();
        }

        public void AssignTheme(RuneGateUiTheme value)
        {
            theme = value;
            ApplyTheme();
        }

        public void Refresh()
        {
            hasExistingProgress = HasMeaningfulProgress(SaveManager.Current);
            TitleViewData data = CurrentViewData;
            bool main = ViewMode == TitleViewMode.Main;
            if (mainBody != null)
            {
                mainBody.SetActive(main);
            }

            if (settingsBody != null)
            {
                settingsBody.SetActive(!main);
            }

            if (backButton != null)
            {
                backButton.gameObject.SetActive(!main);
            }

            if (headerText != null)
            {
                headerText.text = main ? "문지기 기록" : "설정";
            }

            if (summaryText != null)
            {
                summaryText.text = data.ProgressSummary;
                summaryText.gameObject.SetActive(main);
            }

            if (feedbackText != null)
            {
                feedbackText.text = data.FeedbackMessage;
                feedbackText.gameObject.SetActive(main && !string.IsNullOrWhiteSpace(data.FeedbackMessage));
            }

            if (primaryButtonText != null)
            {
                primaryButtonText.text = data.PrimaryActionLabel;
            }

            if (newGameButton != null)
            {
                newGameButton.gameObject.SetActive(hasExistingProgress);
            }

            if (resetButton != null)
            {
                resetButton.gameObject.SetActive(hasExistingProgress);
            }

            RefreshAudioLabels();
            RefreshModal();
            SetInteractionEnabled(!sceneTransitionRequested && ConfirmAction == TitleConfirmAction.None);
            ApplyResponsiveLayout(true);
        }

        public void ShowMain()
        {
            ViewMode = TitleViewMode.Main;
            ConfirmAction = TitleConfirmAction.None;
            feedbackMessage = string.Empty;
            Refresh();
        }

        public void ShowSettings()
        {
            ViewMode = TitleViewMode.Settings;
            ConfirmAction = TitleConfirmAction.None;
            feedbackMessage = string.Empty;
            Refresh();
        }

        public void ShowConfirmation(TitleConfirmAction action)
        {
            if (action == TitleConfirmAction.None)
            {
                ConfirmAction = TitleConfirmAction.None;
            }
            else
            {
                ConfirmAction = action;
                feedbackMessage = string.Empty;
            }

            Refresh();
        }

        public void RebuildView()
        {
            EnsureCanvasComponents();
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyObject(transform.GetChild(i).gameObject);
            }

            backgroundLayer = CreateRect("BackgroundLayer", transform);
            Stretch(backgroundLayer);
            GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            backgroundObject.transform.SetParent(backgroundLayer, false);
            backgroundImage = backgroundObject.GetComponent<RawImage>();
            Stretch(backgroundImage.rectTransform);
            backgroundImage.raycastTarget = false;
            AspectRatioFitter backgroundFitter = backgroundObject.AddComponent<AspectRatioFitter>();
            backgroundFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            backgroundFitter.aspectRatio = UiFrameTokens.ReferenceWidth / UiFrameTokens.ReferenceHeight;

            safeAreaRoot = CreateRect("ContentSafeAreaRoot", transform);
            Stretch(safeAreaRoot);
            SafeAreaFitter fitter = safeAreaRoot.gameObject.AddComponent<SafeAreaFitter>();
            frameRoot = CreateRect("FrameRoot", safeAreaRoot);
            brandArea = CreateRect("BrandArea", frameRoot);
            menuPanel = CreatePanel("MenuPanel", frameRoot);
            headerArea = CreateRect("HeaderArea", menuPanel);
            bodyArea = CreateRect("BodyArea", menuPanel);
            actionArea = CreateRect("ActionArea", menuPanel);
            statusFooter = CreateRect("StatusFooter", frameRoot);

            titleText = CreateText("KoreanTitle", brandArea, "룬게이트 디펜스", 58f, TextAlignmentOptions.Center, true);
            englishTitleText = CreateText("EnglishTitle", brandArea, "RuneGate Defense", 28f, TextAlignmentOptions.Center, true);
            taglineText = CreateText("Tagline", brandArea, "봉문 전선을 지켜라", 24f, TextAlignmentOptions.Center, false);
            headerText = CreateText("HeaderText", headerArea, "문지기 기록", 34f, TextAlignmentOptions.MidlineLeft, true);
            summaryText = CreateText("SummaryText", bodyArea, string.Empty, 25f, TextAlignmentOptions.TopLeft, false, true);
            feedbackText = CreateText("FeedbackText", bodyArea, string.Empty, 21f, TextAlignmentOptions.BottomLeft, false, true);
            footerText = CreateText("FooterText", statusFooter, "로컬 저장 · 오프라인 플레이", 20f, TextAlignmentOptions.Center, false);

            mainBody = new GameObject("MainActions", typeof(RectTransform));
            mainBody.transform.SetParent(actionArea, false);
            RectTransform mainActions = mainBody.GetComponent<RectTransform>();
            Stretch(mainActions);
            primaryButton = CreateButton("PrimaryButton", mainActions, string.Empty, 28f, true, out primaryButtonText);
            newGameButton = CreateButton("NewGameButton", mainActions, "새 전선", 22f, false, out _);
            settingsButton = CreateButton("SettingsButton", mainActions, "설정", 22f, false, out _);

            settingsBody = CreateSettingsBody(bodyArea);
            backButton = CreateButton("BackButton", actionArea, "돌아가기", 24f, false, out _);

            overlayLayer = CreateRect("OverlayLayer", transform);
            Stretch(overlayLayer);
            RectTransform dim = CreateImageRect("Dim", overlayLayer, new Color(0f, 0f, 0f, 0.72f));
            Stretch(dim);
            dim.GetComponent<Image>().raycastTarget = true;
            RectTransform overlaySafeArea = CreateRect("OverlaySafeAreaRoot", overlayLayer);
            Stretch(overlaySafeArea);
            overlaySafeArea.gameObject.AddComponent<SafeAreaFitter>();
            modalPanel = CreatePanel("ModalPanel", overlaySafeArea);
            modalGroup = modalPanel.gameObject.AddComponent<CanvasGroup>();
            modalTitleText = CreateText("ModalTitle", modalPanel, string.Empty, 32f, TextAlignmentOptions.TopLeft, true, true);
            modalBodyText = CreateText("ModalBody", modalPanel, string.Empty, 23f, TextAlignmentOptions.TopLeft, false, true);
            modalCancelButton = CreateButton("CancelButton", modalPanel, "취소", 22f, false, out _);
            modalConfirmButton = CreateButton("ConfirmButton", modalPanel, string.Empty, 22f, true, out modalConfirmText);

            fitter.ApplySafeArea();
            ApplyTheme();
            BindListeners();
            ApplyResponsiveLayout(true);
        }

        public static TitleMenuLayoutRects CalculateLayoutForSize(float width, float height, bool settingsOpen)
        {
            TitleCanvasRects rects = TitleCanvasLayout.Calculate(
                new Vector2(width, height),
                1f,
                settingsOpen ? TitleViewMode.Settings : TitleViewMode.Main);
            Rect content = Rect.MinMaxRect(
                rects.BodyArea.xMin,
                rects.ActionArea.yMin,
                rects.BodyArea.xMax,
                rects.HeaderArea.yMax);
            return new TitleMenuLayoutRects(rects.SafeArea, rects.BrandArea, rects.MenuPanel, content, rects.ModalPanel);
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
                return "새 문지기 기록";
            }

            int clearedCount = saveData.clearedStageIds != null
                ? Mathf.Clamp(saveData.clearedStageIds.Count, 0, ChapterStageCount)
                : 0;
            string difficulty = GameTextMapper.Difficulty(saveData.selectedDifficultyId);
            return $"보유 골드 {Mathf.Max(0, saveData.totalGold):N0} · 클리어 {clearedCount}/{ChapterStageCount} · 난이도 {difficulty}";
        }

        public static string PrimaryActionLabel(bool existingProgress)
        {
            return existingProgress ? "전선으로 돌아가기" : "새 전선 시작";
        }

        private GameObject CreateSettingsBody(Transform parent)
        {
            GameObject root = new GameObject("SettingsBody", typeof(RectTransform), typeof(ScrollRect));
            root.transform.SetParent(parent, false);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            Stretch(rootRect);
            settingsViewport = CreateImageRect("Viewport", rootRect, Color.clear);
            Stretch(settingsViewport);
            settingsViewport.gameObject.AddComponent<RectMask2D>();
            settingsContent = CreateRect("Content", settingsViewport);
            settingsContent.anchorMin = new Vector2(0f, 1f);
            settingsContent.anchorMax = new Vector2(1f, 1f);
            settingsContent.pivot = new Vector2(0.5f, 1f);
            VerticalLayoutGroup layout = settingsContent.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = UiFrameTokens.Space16;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            ContentSizeFitter sizeFitter = settingsContent.gameObject.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            bgmToggleButton = CreateSettingsButton("BgmToggleButton", settingsContent, out bgmToggleText);
            bgmVolumeButton = CreateSettingsButton("BgmVolumeButton", settingsContent, out bgmVolumeText);
            sfxToggleButton = CreateSettingsButton("SfxToggleButton", settingsContent, out sfxToggleText);
            sfxVolumeButton = CreateSettingsButton("SfxVolumeButton", settingsContent, out sfxVolumeText);
            tutorialButton = CreateSettingsButton("TutorialButton", settingsContent, out TMP_Text tutorialText);
            tutorialText.text = "다음 전투에서 튜토리얼 다시 보기";
            resetButton = CreateSettingsButton("ResetButton", settingsContent, out TMP_Text resetText);
            resetText.text = "저장 기록 초기화";

            ScrollRect scroll = root.GetComponent<ScrollRect>();
            scroll.viewport = settingsViewport;
            scroll.content = settingsContent;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28f;
            return root;
        }

        private Button CreateSettingsButton(string name, Transform parent, out TMP_Text label)
        {
            Button button = CreateButton(name, parent, string.Empty, 22f, false, out label);
            LayoutElement element = button.gameObject.AddComponent<LayoutElement>();
            element.minHeight = UiFrameTokens.MinimumTouchHeight;
            element.preferredHeight = UiFrameTokens.MinimumTouchHeight;
            return button;
        }

        private void BindListeners()
        {
            if (listenersBound)
            {
                return;
            }

            primaryButton?.onClick.AddListener(HandlePrimary);
            newGameButton?.onClick.AddListener(HandleNewGamePressed);
            settingsButton?.onClick.AddListener(ShowSettings);
            backButton?.onClick.AddListener(ShowMain);
            bgmToggleButton?.onClick.AddListener(ToggleBgm);
            bgmVolumeButton?.onClick.AddListener(CycleBgmVolume);
            sfxToggleButton?.onClick.AddListener(ToggleSfx);
            sfxVolumeButton?.onClick.AddListener(CycleSfxVolume);
            tutorialButton?.onClick.AddListener(ResetTutorial);
            resetButton?.onClick.AddListener(HandleResetPressed);
            modalCancelButton?.onClick.AddListener(CancelConfirmation);
            modalConfirmButton?.onClick.AddListener(ConfirmPendingAction);
            listenersBound = true;
        }

        private void UnbindListeners()
        {
            if (!listenersBound)
            {
                return;
            }

            primaryButton?.onClick.RemoveListener(HandlePrimary);
            newGameButton?.onClick.RemoveListener(HandleNewGamePressed);
            settingsButton?.onClick.RemoveListener(ShowSettings);
            backButton?.onClick.RemoveListener(ShowMain);
            bgmToggleButton?.onClick.RemoveListener(ToggleBgm);
            bgmVolumeButton?.onClick.RemoveListener(CycleBgmVolume);
            sfxToggleButton?.onClick.RemoveListener(ToggleSfx);
            sfxVolumeButton?.onClick.RemoveListener(CycleSfxVolume);
            tutorialButton?.onClick.RemoveListener(ResetTutorial);
            resetButton?.onClick.RemoveListener(HandleResetPressed);
            modalCancelButton?.onClick.RemoveListener(CancelConfirmation);
            modalConfirmButton?.onClick.RemoveListener(ConfirmPendingAction);
            listenersBound = false;
        }

        private void HandlePrimary()
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

        private void HandleNewGamePressed()
        {
            if (sceneTransitionRequested)
            {
                return;
            }

            if (hasExistingProgress)
            {
                ShowConfirmation(TitleConfirmAction.NewGame);
                return;
            }

            StartNewGame();
        }

        private void ConfirmPendingAction()
        {
            if (ConfirmAction == TitleConfirmAction.NewGame)
            {
                StartNewGame();
            }
            else if (ConfirmAction == TitleConfirmAction.ResetSave)
            {
                ResetSaveFromSettings();
            }
        }

        private void HandleResetPressed()
        {
            ShowConfirmation(TitleConfirmAction.ResetSave);
        }

        private void CancelConfirmation()
        {
            ShowConfirmation(TitleConfirmAction.None);
        }

        private void ToggleBgm()
        {
            bool enabled = !AudioManager.BgmEnabled;
            AudioManager.SetBgmEnabled(enabled);
            feedbackMessage = enabled ? "배경 음악을 켰습니다." : "배경 음악을 껐습니다.";
            Refresh();
        }

        private void CycleBgmVolume()
        {
            AudioManager.SetBgmVolume(AudioManager.NextVolumeStep(AudioManager.BgmVolume));
            feedbackMessage = $"BGM 음량을 {Mathf.RoundToInt(AudioManager.BgmVolume * 100f)}%로 설정했습니다.";
            Refresh();
        }

        private void ToggleSfx()
        {
            bool enabled = !AudioManager.SfxEnabled;
            AudioManager.SetSfxEnabled(enabled);
            if (enabled)
            {
                AudioManager.Play(SfxKey.ButtonClick);
            }

            feedbackMessage = enabled ? "전투 효과음을 켰습니다." : "전투 효과음을 껐습니다.";
            Refresh();
        }

        private void CycleSfxVolume()
        {
            AudioManager.SetSfxVolume(AudioManager.NextVolumeStep(AudioManager.SfxVolume));
            AudioManager.Play(SfxKey.ButtonClick);
            feedbackMessage = $"SFX 음량을 {Mathf.RoundToInt(AudioManager.SfxVolume * 100f)}%로 설정했습니다.";
            Refresh();
        }

        private void ResetTutorial()
        {
            SaveManager.ResetTutorialSeen();
            feedbackMessage = "다음 전투에서 튜토리얼을 다시 표시합니다.";
            Refresh();
        }

        private void RefreshAudioLabels()
        {
            if (bgmToggleText != null)
            {
                bgmToggleText.text = AudioManager.BgmEnabled ? "BGM 켜짐" : "BGM 꺼짐";
            }

            if (bgmVolumeText != null)
            {
                bgmVolumeText.text = $"BGM 음량 {Mathf.RoundToInt(AudioManager.BgmVolume * 100f)}%";
            }

            if (sfxToggleText != null)
            {
                sfxToggleText.text = AudioManager.SfxEnabled ? "SFX 켜짐" : "SFX 꺼짐";
            }

            if (sfxVolumeText != null)
            {
                sfxVolumeText.text = $"SFX 음량 {Mathf.RoundToInt(AudioManager.SfxVolume * 100f)}%";
            }
        }

        private void RefreshModal()
        {
            bool visible = ConfirmAction != TitleConfirmAction.None;
            if (overlayLayer != null)
            {
                overlayLayer.gameObject.SetActive(visible);
            }

            if (modalGroup != null)
            {
                modalGroup.alpha = visible ? 1f : 0f;
                modalGroup.interactable = visible;
                modalGroup.blocksRaycasts = visible;
            }

            if (!visible)
            {
                return;
            }

            bool newGame = ConfirmAction == TitleConfirmAction.NewGame;
            modalTitleText.text = newGame ? "새 전선을 시작할까요?" : "저장 기록을 초기화할까요?";
            modalBodyText.text = newGame
                ? "현재 진행과 업그레이드가 초기화됩니다."
                : "복구할 수 없습니다. 초기화 후 새 문지기 기록이 생성됩니다.";
            modalConfirmText.text = newGame ? "새 전선 시작" : "초기화";
        }

        private void SetInteractionEnabled(bool value)
        {
            if (primaryButton != null) primaryButton.interactable = value;
            if (newGameButton != null) newGameButton.interactable = value;
            if (settingsButton != null) settingsButton.interactable = value;
            if (backButton != null) backButton.interactable = value;
            if (bgmToggleButton != null) bgmToggleButton.interactable = value;
            if (bgmVolumeButton != null) bgmVolumeButton.interactable = value;
            if (sfxToggleButton != null) sfxToggleButton.interactable = value;
            if (sfxVolumeButton != null) sfxVolumeButton.interactable = value;
            if (tutorialButton != null) tutorialButton.interactable = value;
            if (resetButton != null) resetButton.interactable = value;
            if (settingsViewport != null)
            {
                ScrollRect scroll = settingsViewport.GetComponentInParent<ScrollRect>();
                if (scroll != null) scroll.enabled = value;
            }
        }

        private void ApplyResponsiveLayout(bool force)
        {
            if (safeAreaRoot == null || frameRoot == null || rootCanvas == null)
            {
                return;
            }

            Vector2 size = safeAreaRoot.rect.size;
            float scaleFactor = rootCanvas.scaleFactor;
            if (!force && (size - lastSafeSize).sqrMagnitude < 0.01f && Mathf.Abs(scaleFactor - lastScaleFactor) < 0.001f)
            {
                return;
            }

            lastSafeSize = size;
            lastScaleFactor = scaleFactor;
            AspectRatioFitter backgroundFitter = backgroundImage != null ? backgroundImage.GetComponent<AspectRatioFitter>() : null;
            if (backgroundFitter != null)
            {
                backgroundFitter.aspectMode = size.x / Mathf.Max(1f, size.y) >= TitleCanvasLayout.LandscapeThreshold
                    ? AspectRatioFitter.AspectMode.FitInParent
                    : AspectRatioFitter.AspectMode.EnvelopeParent;
            }

            TitleCanvasRects rects = TitleCanvasLayout.Calculate(size, scaleFactor, ViewMode);
            ApplyLocalRect(frameRoot, rects.FrameRoot, size);
            ApplyRelativeRect(brandArea, rects.BrandArea, rects.FrameRoot);
            ApplyRelativeRect(menuPanel, rects.MenuPanel, rects.FrameRoot);
            ApplyRelativeRect(headerArea, rects.HeaderArea, rects.MenuPanel);
            ApplyRelativeRect(bodyArea, rects.BodyArea, rects.MenuPanel);
            ApplyRelativeRect(actionArea, rects.ActionArea, rects.MenuPanel);
            ApplyRelativeRect(statusFooter, rects.StatusFooter, rects.FrameRoot);
            if (statusFooter != null)
            {
                statusFooter.gameObject.SetActive(rects.FooterVisible);
            }

            ApplyLocalRect(modalPanel, rects.ModalPanel, size);
            LayoutBrand();
            LayoutMainActions();
            LayoutSettingsAction();
            LayoutModal();
        }

        private void LayoutBrand()
        {
            SetAnchored(titleText.rectTransform, new Vector2(0f, 0.45f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            SetAnchored(englishTitleText.rectTransform, new Vector2(0f, 0.28f), new Vector2(1f, 0.50f), Vector2.zero, Vector2.zero);
            SetAnchored(taglineText.rectTransform, new Vector2(0f, 0.06f), new Vector2(1f, 0.28f), Vector2.zero, Vector2.zero);
            Stretch(headerText.rectTransform);
            Stretch(footerText.rectTransform);
            SetAnchored(summaryText.rectTransform, new Vector2(0f, 0.32f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            SetAnchored(feedbackText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.30f), Vector2.zero, Vector2.zero);
        }

        private void LayoutMainActions()
        {
            RectTransform primary = primaryButton.GetComponent<RectTransform>();
            RectTransform secondaryLeft = newGameButton.GetComponent<RectTransform>();
            RectTransform secondaryRight = settingsButton.GetComponent<RectTransform>();
            float gap = UiFrameTokens.Space16;
            float primaryHeight = UiFrameTokens.MinimumTouchHeight;
            float secondaryHeight = UiFrameTokens.MinimumTouchHeight;
            primary.anchorMin = new Vector2(0f, 1f);
            primary.anchorMax = new Vector2(1f, 1f);
            primary.pivot = new Vector2(0.5f, 1f);
            primary.anchoredPosition = Vector2.zero;
            primary.sizeDelta = new Vector2(0f, primaryHeight);
            SetAnchored(secondaryLeft, new Vector2(0f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(-gap * 0.5f, secondaryHeight));
            SetAnchored(secondaryRight, new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(gap * 0.5f, 0f), new Vector2(0f, secondaryHeight));
            if (!hasExistingProgress)
            {
                SetAnchored(secondaryRight, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, secondaryHeight));
            }
        }

        private void LayoutSettingsAction()
        {
            if (backButton != null)
            {
                Stretch(backButton.GetComponent<RectTransform>());
            }
        }

        private void LayoutModal()
        {
            const float inset = 48f;
            SetAnchored(modalTitleText.rectTransform, new Vector2(0f, 0.70f), new Vector2(1f, 1f), new Vector2(inset, -inset), new Vector2(-inset, -inset));
            SetAnchored(modalBodyText.rectTransform, new Vector2(0f, 0.30f), new Vector2(1f, 0.70f), new Vector2(inset, 0f), new Vector2(-inset, 0f));
            float gap = UiFrameTokens.Space16;
            float height = UiFrameTokens.MinimumTouchHeight;
            SetAnchored(modalCancelButton.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0.5f, 0f), new Vector2(inset, inset), new Vector2(-gap * 0.5f, inset + height));
            SetAnchored(modalConfirmButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(gap * 0.5f, inset), new Vector2(-inset, inset + height));
        }

        private void ApplyTheme()
        {
            if (theme == null)
            {
                return;
            }

            if (backgroundImage != null)
            {
                Sprite background = RuntimePixelAssetLoader.LoadSprite(RuntimePixelAssetLoader.AppSplashBackground);
                backgroundImage.texture = background != null ? background.texture : null;
                backgroundImage.color = backgroundImage.texture != null ? Color.white : theme.Background;
                backgroundImage.uvRect = new Rect(0f, 0f, 1f, 1f);
                AspectRatioFitter fitter = backgroundImage.GetComponent<AspectRatioFitter>();
                if (fitter != null && backgroundImage.texture != null)
                {
                    fitter.aspectRatio = backgroundImage.texture.width / (float)Mathf.Max(1, backgroundImage.texture.height);
                }
            }

            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                TMP_Text text = texts[i];
                if (Application.isPlaying)
                {
                    text.font = (text.fontStyle & FontStyles.Bold) != 0 ? theme.Bold : theme.Regular;
                }

                text.color = text == titleText ? theme.Brass : text == taglineText ? theme.Brass : theme.PrimaryText;
                if (text == titleText || text == englishTitleText || text == taglineText)
                {
                    text.outlineColor = new Color32(4, 8, 12, 230);
                    text.outlineWidth = 0.14f;
                }
            }

            Image[] images = GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];
                if (image.gameObject == menuPanel?.gameObject || image.gameObject == modalPanel?.gameObject)
                {
                    image.sprite = theme.PanelSprite;
                    image.type = Image.Type.Sliced;
                    image.color = theme.Surface;
                }
            }

            RuneGateButtonFeedback[] feedback = GetComponentsInChildren<RuneGateButtonFeedback>(true);
            for (int i = 0; i < feedback.Length; i++)
            {
                feedback[i].Configure(theme.ButtonPressDuration);
            }
        }

        private void EnsureCanvasComponents()
        {
            rootCanvas = GetComponent<Canvas>();
            if (rootCanvas == null) rootCanvas = gameObject.AddComponent<Canvas>();
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rootCanvas.pixelPerfect = false;
            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(UiFrameTokens.ReferenceWidth, UiFrameTokens.ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = UiFrameTokens.CanvasMatch;
            if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();
        }

        private RectTransform CreatePanel(string name, Transform parent)
        {
            RectTransform rect = CreateImageRect(name, parent, theme != null ? theme.Surface : new Color32(17, 28, 34, 255));
            Image image = rect.GetComponent<Image>();
            image.sprite = theme != null ? theme.PanelSprite : null;
            image.type = image.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            return rect;
        }

        private Button CreateButton(string name, Transform parent, string label, float fontSize, bool primary, out TMP_Text labelText)
        {
            RectTransform rect = CreateImageRect(name, parent, primary && theme != null ? theme.Brass : theme != null ? theme.AlternateSurface : new Color32(24, 38, 45, 255));
            Image image = rect.GetComponent<Image>();
            image.sprite = theme != null ? theme.ButtonSprite : null;
            image.type = image.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.raycastTarget = true;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
            colors.pressedColor = new Color(0.82f, 0.92f, 0.96f, 1f);
            colors.disabledColor = new Color(0.38f, 0.42f, 0.44f, 0.72f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            rect.gameObject.AddComponent<RuneGateButtonFeedback>();
            labelText = CreateText("Label", rect, label, fontSize, TextAlignmentOptions.Center, true);
            Stretch(labelText.rectTransform);
            return button;
        }

        private TMP_Text CreateText(string name, Transform parent, string value, float fontSize, TextAlignmentOptions alignment, bool bold, bool wrap = false)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            text.alignment = alignment;
            text.color = theme != null ? theme.PrimaryText : Color.white;
            text.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
            return text;
        }

        private static RectTransform CreateImageRect(string name, Transform parent, Color color)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return imageObject.GetComponent<RectTransform>();
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            return child.GetComponent<RectTransform>();
        }

        private static void Stretch(RectTransform rect)
        {
            SetAnchored(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        private static void SetAnchored(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            if (rect == null) return;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void ApplyRelativeRect(RectTransform target, Rect rect, Rect parentRect)
        {
            Rect local = new Rect(rect.x - parentRect.x, rect.y - parentRect.y, rect.width, rect.height);
            ApplyLocalRect(target, local, parentRect.size);
        }

        private static void ApplyLocalRect(RectTransform target, Rect rect, Vector2 parentSize)
        {
            if (target == null) return;
            target.anchorMin = target.anchorMax = new Vector2(0.5f, 0.5f);
            target.pivot = new Vector2(0.5f, 0.5f);
            target.sizeDelta = rect.size;
            target.anchoredPosition = rect.center - parentSize * 0.5f;
        }

        private static void DestroyObject(GameObject target)
        {
            if (target == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(target);
            else UnityEngine.Object.DestroyImmediate(target);
        }

        private void StartNewGame()
        {
            if (sceneTransitionRequested) return;
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
            ConfirmAction = TitleConfirmAction.None;
            hasExistingProgress = false;
            feedbackMessage = "저장 기록을 초기화했습니다.";
            Refresh();
        }

        private void LoadStageSelect()
        {
            if (sceneTransitionRequested) return;
            sceneTransitionRequested = true;
            SaveManager.Load();
            SceneManager.LoadScene(stageSelectSceneName);
        }
    }
}
