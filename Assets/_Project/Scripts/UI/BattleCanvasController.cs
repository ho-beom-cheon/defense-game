using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RuneGate
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
    public sealed class BattleCanvasController : MonoBehaviour
    {
        private const int DamagePoolSize = 24;

        [Header("Theme")]
        [SerializeField] private RuneGateUiTheme theme;

        [Header("Battle Controllers")]
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private CrystalController crystalController;
        [SerializeField] private BattlePauseController pauseController;
        [SerializeField] private TutorialManager tutorialManager;
        [SerializeField] private RuneSelectionUI runeSelectionController;
        [SerializeField] private StageResultUI resultController;

        [Header("Root Layout")]
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private RectTransform safeAreaRoot;
        [SerializeField] private RectTransform battleRoot;
        [SerializeField] private RectTransform hudLayer;
        [SerializeField] private RectTransform battlefieldViewport;
        [SerializeField] private RectTransform skillLayer;
        [SerializeField] private RectTransform skillGrid;
        [SerializeField] private RectTransform overlayLayer;
        [SerializeField] private RectTransform damageLayer;
        [SerializeField] private CanvasGroup hudGroup;
        [SerializeField] private CanvasGroup skillGroup;

        [Header("HUD")]
        [SerializeField] private TMP_Text stageNumberText;
        [SerializeField] private TMP_Text stageNameText;
        [SerializeField] private TMP_Text crystalHpText;
        [SerializeField] private TMP_Text crystalShieldText;
        [SerializeField] private Image crystalHpFill;
        [SerializeField] private TMP_Text waveText;
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private Button pauseButton;

        [Header("Wave Banner")]
        [SerializeField] private CanvasGroup waveBannerGroup;
        [SerializeField] private RectTransform waveBannerRect;
        [SerializeField] private TMP_Text waveBannerTitle;
        [SerializeField] private TMP_Text waveBannerSubtitle;

        [Header("Overlay")]
        [SerializeField] private CanvasGroup dimGroup;
        [SerializeField] private CanvasGroup tutorialGroup;
        [SerializeField] private CanvasGroup runeGroup;
        [SerializeField] private CanvasGroup pauseGroup;
        [SerializeField] private CanvasGroup resultGroup;

        [Header("Tutorial")]
        [SerializeField] private TMP_Text tutorialProgressText;
        [SerializeField] private TMP_Text tutorialTitleText;
        [SerializeField] private TMP_Text tutorialBodyText;
        [SerializeField] private Button tutorialPreviousButton;
        [SerializeField] private Button tutorialNextButton;
        [SerializeField] private Button tutorialSkipButton;

        [Header("Rune Selection")]
        [SerializeField] private RectTransform runeCardContainer;

        [Header("Pause")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button stageSelectButton;

        [Header("Result")]
        [SerializeField] private TMP_Text resultTitleText;
        [SerializeField] private TMP_Text resultMessageText;
        [SerializeField] private TMP_Text resultGoldText;
        [SerializeField] private TMP_Text resultStatsText;
        [SerializeField] private TMP_Text resultRewardText;
        [SerializeField] private Button resultPrimaryButton;
        [SerializeField] private TMP_Text resultPrimaryButtonText;
        [SerializeField] private Button resultUpgradeButton;
        [SerializeField] private TMP_Text resultUpgradeButtonText;
        [SerializeField] private Button resultStageSelectButton;
        [SerializeField] private TMP_Text resultStageSelectButtonText;

        private readonly List<BattleSkillCardView> skillCards = new List<BattleSkillCardView>();
        private readonly List<Button> runeCards = new List<Button>();
        private readonly List<DamageNumberEntry> damageNumbers = new List<DamageNumberEntry>();
        private readonly Dictionary<CanvasGroup, Coroutine> fadeRoutines = new Dictionary<CanvasGroup, Coroutine>();
        private BattleOverlayState overlayState = BattleOverlayState.None;
        private BattleResultViewData resultViewData;
        private Coroutine waveBannerRoutine;
        private Coroutine resultCountRoutine;
        private Vector2 lastSafeRootSize = new Vector2(-1f, -1f);
        private bool eventsBound;

        public bool IsReady { get; private set; }
        public BattleOverlayState OverlayState => overlayState;
        public RectTransform BattlefieldViewport => battlefieldViewport;
        public string CrystalShieldText => crystalShieldText != null ? crystalShieldText.text : string.Empty;
        public string BossStatusText { get; private set; } = string.Empty;

        public void AssignTheme(RuneGateUiTheme value)
        {
            theme = value;
        }

        public void Configure(BattleManager manager, CrystalController crystal, BattlePauseController pause,
            TutorialManager tutorial, RuneSelectionUI runeSelection, StageResultUI result)
        {
            UnbindEvents();
            battleManager = manager;
            crystalController = crystal;
            pauseController = pause;
            tutorialManager = tutorial;
            runeSelectionController = runeSelection;
            resultController = result;
            pauseController?.Configure(battleManager);
            runeSelectionController?.SetRuntimeGuiEnabled(false);
            resultController?.SetRuntimeGuiEnabled(false);
            ConfigureCameraViewport();
            if (isActiveAndEnabled && Application.isPlaying)
            {
                BindEvents();
                RefreshAll();
            }
        }

        private void Awake()
        {
            EnsureCanvasComponents();
            if (battleRoot == null)
            {
                RebuildView();
            }

            ApplyTheme();
            AutoAssignControllers();
            ConfigureCameraViewport();
            IsReady = theme != null && battleRoot != null && battlefieldViewport != null;
            if (!IsReady)
            {
                Debug.LogError("BattleCanvasController is missing its required theme or view hierarchy. Rebuild Battle UI assets from Tools/RuneGate/Build Battle uGUI Assets.");
            }
        }

        private void OnEnable()
        {
            AutoAssignControllers();
            BindEvents();
        }

        private void Start()
        {
            ApplyResponsiveLayout(true);
            EnsureSkillCards();
            RefreshAll();
        }

        private void OnDisable()
        {
            UnbindEvents();
        }

        private void Update()
        {
            ApplyResponsiveLayout(false);
            EnsureSkillCards();
            RefreshOverlayState();
            RefreshBossStatus();
            UpdateDamageNumbers();
        }

        public bool ShowDamageNumber(string value, Vector3 worldPosition, Color color)
        {
            if (!IsReady || damageLayer == null || string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            DamageNumberEntry entry = null;
            for (int i = 0; i < damageNumbers.Count; i++)
            {
                if (!damageNumbers[i].Active)
                {
                    entry = damageNumbers[i];
                    break;
                }
            }

            if (entry == null && damageNumbers.Count > 0)
            {
                entry = damageNumbers[0];
                for (int i = 1; i < damageNumbers.Count; i++)
                {
                    if (damageNumbers[i].StartedAt < entry.StartedAt)
                    {
                        entry = damageNumbers[i];
                    }
                }
            }

            if (entry == null)
            {
                return false;
            }

            entry.Value.text = value;
            entry.WorldPosition = worldPosition;
            entry.Color = color;
            entry.StartedAt = Time.unscaledTime;
            entry.Duration = theme != null ? theme.DamageTextDuration : 0.6f;
            entry.Active = true;
            entry.Rect.gameObject.SetActive(true);
            UpdateDamageNumber(entry);
            return true;
        }

        [ContextMenu("Rebuild Battle uGUI View")]
        public void RebuildView()
        {
            EnsureCanvasComponents();
            ClearChildren(transform);

            safeAreaRoot = CreateRect("SafeAreaRoot", transform);
            Stretch(safeAreaRoot);
            SafeAreaFitter safeArea = safeAreaRoot.gameObject.AddComponent<SafeAreaFitter>();

            battleRoot = CreateRect("BattleRoot", safeAreaRoot);
            battleRoot.anchorMin = battleRoot.anchorMax = new Vector2(0.5f, 0.5f);
            battleRoot.pivot = new Vector2(0.5f, 0.5f);

            hudLayer = CreatePanel("HudLayer", battleRoot, theme != null ? theme.PanelSprite : null, theme != null ? theme.Surface : new Color32(17, 28, 34, 255));
            hudGroup = hudLayer.gameObject.AddComponent<CanvasGroup>();
            BuildHud();

            battlefieldViewport = CreateRect("BattlefieldViewport", battleRoot);
            BuildBattlefieldFrame();

            skillLayer = CreatePanel("SkillLayer", battleRoot, theme != null ? theme.PanelSprite : null, theme != null ? theme.Surface : new Color32(17, 28, 34, 255));
            skillGroup = skillLayer.gameObject.AddComponent<CanvasGroup>();
            BuildSkillLayer();

            overlayLayer = CreateRect("OverlayLayer", battleRoot);
            Stretch(overlayLayer);
            BuildOverlayLayer();

            damageLayer = CreateRect("DamageLayer", overlayLayer);
            Stretch(damageLayer);
            damageLayer.SetAsFirstSibling();
            BuildDamagePool();

            ApplyTheme();
            lastSafeRootSize = new Vector2(-1f, -1f);
            ApplyResponsiveLayout(true);
        }

        private void BuildHud()
        {
            RectTransform stageZone = CreateRect("StageZone", hudLayer);
            SetHorizontalAnchors(stageZone, 0f, 0.24f, 16f, -8f);
            stageNumberText = CreateText("StageNumber", stageZone, "STAGE 1", 22f, TextAlignmentOptions.BottomLeft, true);
            SetAnchored(stageNumberText.rectTransform, new Vector2(0f, 0.52f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0f, -10f));
            stageNameText = CreateText("StageName", stageZone, "고블린 숲길", 28f, TextAlignmentOptions.TopLeft, true);
            SetAnchored(stageNameText.rectTransform, Vector2.zero, new Vector2(1f, 0.55f), Vector2.zero, new Vector2(0f, -2f));

            RectTransform crystalZone = CreateRect("CrystalZone", hudLayer);
            SetHorizontalAnchors(crystalZone, 0.24f, 0.60f, 8f, -8f);
            TMP_Text crystalLabel = CreateText("CrystalLabel", crystalZone, "봉문 수정체", 20f, TextAlignmentOptions.BottomLeft, true);
            SetAnchored(crystalLabel.rectTransform, new Vector2(0f, 0.56f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0f, -6f));
            crystalHpText = CreateText("CrystalHp", crystalZone, "HP 100/100", 24f, TextAlignmentOptions.BottomRight, true);
            SetAnchored(crystalHpText.rectTransform, new Vector2(0.45f, 0.56f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, -6f));
            RectTransform crystalBar = CreateBar("CrystalHpBar", crystalZone, out crystalHpFill, theme != null ? theme.RuneBlue : Color.cyan);
            SetAnchored(crystalBar, new Vector2(0f, 0.30f), new Vector2(1f, 0.52f), new Vector2(0f, 2f), new Vector2(0f, -4f));
            crystalShieldText = CreateText("CrystalShield", crystalZone, "보호막 0", 18f, TextAlignmentOptions.TopLeft, false);
            SetAnchored(crystalShieldText.rectTransform, Vector2.zero, new Vector2(1f, 0.28f), Vector2.zero, Vector2.zero);

            RectTransform statusZone = CreateRect("StatusZone", hudLayer);
            SetHorizontalAnchors(statusZone, 0.60f, 0.88f, 8f, -8f);
            waveText = CreateText("Wave", statusZone, "웨이브 1/1", 23f, TextAlignmentOptions.BottomLeft, true);
            SetAnchored(waveText.rectTransform, new Vector2(0f, 0.52f), Vector2.one, Vector2.zero, new Vector2(0f, -8f));
            goldText = CreateText("Gold", statusZone, "전리품 0", 21f, TextAlignmentOptions.TopLeft, true);
            SetAnchored(goldText.rectTransform, Vector2.zero, new Vector2(1f, 0.52f), Vector2.zero, new Vector2(0f, 0f));
            stateText = CreateText("State", statusZone, "전투 준비", 17f, TextAlignmentOptions.BottomRight, false);
            SetAnchored(stateText.rectTransform, new Vector2(0.35f, 0f), Vector2.one, Vector2.zero, new Vector2(0f, -8f));

            RectTransform pauseZone = CreateRect("PauseZone", hudLayer);
            SetHorizontalAnchors(pauseZone, 0.88f, 1f, 8f, -12f);
            pauseButton = CreateButton("PauseButton", pauseZone, "Ⅱ", 30f);
            SetAnchored(pauseButton.GetComponent<RectTransform>(), new Vector2(0f, 0.18f), new Vector2(1f, 0.82f), Vector2.zero, Vector2.zero);
            pauseButton.onClick.AddListener(HandlePauseButton);
        }

        private void BuildBattlefieldFrame()
        {
            Color frameColor = theme != null ? theme.Brass : new Color32(185, 144, 72, 255);
            CreateFrameEdge("TopBorder", battlefieldViewport, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, -4f), frameColor);
            CreateFrameEdge("BottomBorder", battlefieldViewport, Vector2.zero, new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 4f), frameColor);
            CreateFrameEdge("LeftBorder", battlefieldViewport, Vector2.zero, new Vector2(0f, 1f), Vector2.zero, new Vector2(4f, 0f), frameColor);
            CreateFrameEdge("RightBorder", battlefieldViewport, new Vector2(1f, 0f), Vector2.one, new Vector2(1f, 0f), new Vector2(-4f, 0f), frameColor);
        }

        private void BuildSkillLayer()
        {
            TMP_Text title = CreateText("SkillTrayTitle", skillLayer, "봉문 수비대 · 영웅 스킬", 25f, TextAlignmentOptions.MidlineLeft, true);
            SetAnchored(title.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(18f, -58f), new Vector2(-18f, -8f));
            skillGrid = CreateRect("SkillGrid", skillLayer);
            SetAnchored(skillGrid, Vector2.zero, Vector2.one, new Vector2(16f, 16f), new Vector2(-16f, -64f));
        }

        private void BuildOverlayLayer()
        {
            waveBannerRect = CreatePanel("WaveBanner", overlayLayer, theme != null ? theme.PanelSprite : null, theme != null ? theme.Surface : Color.black);
            waveBannerGroup = waveBannerRect.gameObject.AddComponent<CanvasGroup>();
            waveBannerGroup.alpha = 0f;
            waveBannerGroup.blocksRaycasts = false;
            waveBannerTitle = CreateText("WaveBannerTitle", waveBannerRect, "제1파", 38f, TextAlignmentOptions.Bottom, true);
            SetAnchored(waveBannerTitle.rectTransform, new Vector2(0f, 0.45f), Vector2.one, Vector2.zero, new Vector2(0f, -8f));
            waveBannerSubtitle = CreateText("WaveBannerSubtitle", waveBannerRect, "균열이 열립니다", 19f, TextAlignmentOptions.Top, false);
            SetAnchored(waveBannerSubtitle.rectTransform, Vector2.zero, new Vector2(1f, 0.48f), Vector2.zero, Vector2.zero);

            RectTransform dim = CreateImageRect("Dim", overlayLayer, new Color(0f, 0f, 0f, 0.72f));
            Stretch(dim);
            dimGroup = dim.gameObject.AddComponent<CanvasGroup>();
            dimGroup.alpha = 0f;
            dimGroup.blocksRaycasts = false;

            tutorialGroup = BuildTutorialModal();
            runeGroup = BuildRuneModal();
            pauseGroup = BuildPauseModal();
            resultGroup = BuildResultModal();
            SetImmediate(tutorialGroup, false);
            SetImmediate(runeGroup, false);
            SetImmediate(pauseGroup, false);
            SetImmediate(resultGroup, false);
        }

        private CanvasGroup BuildTutorialModal()
        {
            RectTransform panel = CreateModalPanel("Tutorial");
            CanvasGroup group = panel.gameObject.AddComponent<CanvasGroup>();
            tutorialProgressText = CreateText("TutorialProgress", panel, "전술 기록 1/7", 18f, TextAlignmentOptions.TopLeft, false);
            SetAnchored(tutorialProgressText.rectTransform, new Vector2(0f, 0.84f), new Vector2(1f, 1f), new Vector2(32f, 0f), new Vector2(-32f, -24f));
            tutorialTitleText = CreateText("TutorialTitle", panel, "수정체 방어", 36f, TextAlignmentOptions.BottomLeft, true);
            SetAnchored(tutorialTitleText.rectTransform, new Vector2(0f, 0.65f), new Vector2(1f, 0.88f), new Vector2(32f, 0f), new Vector2(-32f, 0f));
            tutorialBodyText = CreateText("TutorialBody", panel, "", 23f, TextAlignmentOptions.TopLeft, false, true);
            SetAnchored(tutorialBodyText.rectTransform, new Vector2(0f, 0.22f), new Vector2(1f, 0.66f), new Vector2(32f, 8f), new Vector2(-32f, -8f));
            tutorialPreviousButton = CreateButton("TutorialPrevious", panel, "이전", 21f);
            tutorialNextButton = CreateButton("TutorialNext", panel, "다음 기록", 21f);
            tutorialSkipButton = CreateButton("TutorialSkip", panel, "건너뛰기", 19f);
            SetAnchored(tutorialPreviousButton.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0.28f, 0.18f), new Vector2(28f, 24f), new Vector2(-6f, -4f));
            SetAnchored(tutorialNextButton.GetComponent<RectTransform>(), new Vector2(0.28f, 0f), new Vector2(0.72f, 0.18f), new Vector2(6f, 24f), new Vector2(-6f, -4f));
            SetAnchored(tutorialSkipButton.GetComponent<RectTransform>(), new Vector2(0.72f, 0f), new Vector2(1f, 0.18f), new Vector2(6f, 24f), new Vector2(-28f, -4f));
            tutorialPreviousButton.onClick.AddListener(() => tutorialManager?.Previous());
            tutorialNextButton.onClick.AddListener(() => tutorialManager?.Next());
            tutorialSkipButton.onClick.AddListener(() => tutorialManager?.Skip());
            return group;
        }

        private CanvasGroup BuildRuneModal()
        {
            RectTransform panel = CreateModalPanel("RuneSelection");
            CanvasGroup group = panel.gameObject.AddComponent<CanvasGroup>();
            TMP_Text title = CreateText("RuneTitle", panel, "전술 기록서 · 룬 선택", 36f, TextAlignmentOptions.Bottom, true);
            SetAnchored(title.rectTransform, new Vector2(0f, 0.84f), Vector2.one, new Vector2(28f, 0f), new Vector2(-28f, -18f));
            TMP_Text subtitle = CreateText("RuneSubtitle", panel, "다음 웨이브를 버틸 기록 하나를 선택하세요.", 20f, TextAlignmentOptions.Top, false);
            SetAnchored(subtitle.rectTransform, new Vector2(0f, 0.73f), new Vector2(1f, 0.86f), new Vector2(28f, 0f), new Vector2(-28f, 0f));
            runeCardContainer = CreateRect("RuneCards", panel);
            SetAnchored(runeCardContainer, Vector2.zero, new Vector2(1f, 0.73f), new Vector2(28f, 28f), new Vector2(-28f, -12f));
            return group;
        }

        private CanvasGroup BuildPauseModal()
        {
            RectTransform panel = CreateModalPanel("Pause");
            CanvasGroup group = panel.gameObject.AddComponent<CanvasGroup>();
            TMP_Text title = CreateText("PauseTitle", panel, "봉문 작전 일시정지", 36f, TextAlignmentOptions.Center, true);
            SetAnchored(title.rectTransform, new Vector2(0f, 0.76f), Vector2.one, new Vector2(24f, 0f), new Vector2(-24f, -24f));
            TMP_Text body = CreateText("PauseBody", panel, "전열을 점검한 뒤 작전을 계속하세요.", 21f, TextAlignmentOptions.Center, false);
            SetAnchored(body.rectTransform, new Vector2(0f, 0.61f), new Vector2(1f, 0.78f), new Vector2(24f, 0f), new Vector2(-24f, 0f));
            resumeButton = CreateButton("ResumeButton", panel, "작전 계속", 24f);
            restartButton = CreateButton("RestartButton", panel, "전투 재시작", 22f);
            stageSelectButton = CreateButton("PauseStageSelect", panel, "스테이지 선택", 22f);
            SetAnchored(resumeButton.GetComponent<RectTransform>(), new Vector2(0.12f, 0.40f), new Vector2(0.88f, 0.57f), Vector2.zero, Vector2.zero);
            SetAnchored(restartButton.GetComponent<RectTransform>(), new Vector2(0.12f, 0.21f), new Vector2(0.88f, 0.38f), Vector2.zero, Vector2.zero);
            SetAnchored(stageSelectButton.GetComponent<RectTransform>(), new Vector2(0.12f, 0.02f), new Vector2(0.88f, 0.19f), Vector2.zero, Vector2.zero);
            resumeButton.onClick.AddListener(() => pauseController?.Resume());
            restartButton.onClick.AddListener(() => pauseController?.RestartBattle());
            stageSelectButton.onClick.AddListener(() => pauseController?.OpenStageSelect());
            return group;
        }

        private CanvasGroup BuildResultModal()
        {
            RectTransform panel = CreateModalPanel("Result");
            CanvasGroup group = panel.gameObject.AddComponent<CanvasGroup>();
            resultTitleText = CreateText("ResultTitle", panel, "봉문 성공", 44f, TextAlignmentOptions.Bottom, true);
            SetAnchored(resultTitleText.rectTransform, new Vector2(0f, 0.84f), Vector2.one, new Vector2(28f, 0f), new Vector2(-28f, -20f));
            resultMessageText = CreateText("ResultMessage", panel, "", 22f, TextAlignmentOptions.Top, false, true);
            SetAnchored(resultMessageText.rectTransform, new Vector2(0f, 0.72f), new Vector2(1f, 0.85f), new Vector2(32f, 0f), new Vector2(-32f, 0f));
            resultGoldText = CreateText("ResultGold", panel, "획득 골드 +0", 30f, TextAlignmentOptions.Center, true);
            SetAnchored(resultGoldText.rectTransform, new Vector2(0f, 0.62f), new Vector2(1f, 0.73f), new Vector2(32f, 0f), new Vector2(-32f, 0f));
            resultStatsText = CreateText("ResultStats", panel, "", 21f, TextAlignmentOptions.TopLeft, false, true);
            SetAnchored(resultStatsText.rectTransform, new Vector2(0f, 0.40f), new Vector2(0.5f, 0.62f), new Vector2(38f, 8f), new Vector2(-12f, 0f));
            resultRewardText = CreateText("ResultReward", panel, "", 20f, TextAlignmentOptions.TopLeft, false, true);
            SetAnchored(resultRewardText.rectTransform, new Vector2(0.5f, 0.40f), new Vector2(1f, 0.62f), new Vector2(12f, 8f), new Vector2(-38f, 0f));
            resultPrimaryButton = CreateButton("ResultPrimary", panel, "다음 스테이지", 25f);
            resultPrimaryButtonText = resultPrimaryButton.GetComponentInChildren<TMP_Text>();
            SetAnchored(resultPrimaryButton.GetComponent<RectTransform>(), new Vector2(0.08f, 0.24f), new Vector2(0.92f, 0.38f), Vector2.zero, Vector2.zero);
            resultUpgradeButton = CreateButton("ResultUpgrade", panel, "봉문 정비소", 21f);
            resultUpgradeButtonText = resultUpgradeButton.GetComponentInChildren<TMP_Text>();
            resultStageSelectButton = CreateButton("ResultStageSelect", panel, "스테이지 선택", 21f);
            resultStageSelectButtonText = resultStageSelectButton.GetComponentInChildren<TMP_Text>();
            SetAnchored(resultUpgradeButton.GetComponent<RectTransform>(), new Vector2(0.08f, 0.06f), new Vector2(0.49f, 0.20f), Vector2.zero, new Vector2(-8f, 0f));
            SetAnchored(resultStageSelectButton.GetComponent<RectTransform>(), new Vector2(0.51f, 0.06f), new Vector2(0.92f, 0.20f), new Vector2(8f, 0f), Vector2.zero);
            resultPrimaryButton.onClick.AddListener(HandleResultPrimary);
            resultUpgradeButton.onClick.AddListener(() => resultController?.OpenUpgrade());
            resultStageSelectButton.onClick.AddListener(() => resultController?.OpenStageSelect());
            return group;
        }

        private void BuildDamagePool()
        {
            damageNumbers.Clear();
            for (int i = 0; i < DamagePoolSize; i++)
            {
                TMP_Text value = CreateText($"DamageNumber_{i + 1:00}", damageLayer, "0", 30f, TextAlignmentOptions.Center, true);
                RectTransform rect = value.rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(140f, 52f);
                rect.gameObject.SetActive(false);
                damageNumbers.Add(new DamageNumberEntry(rect, value));
            }
        }

        private void EnsureSkillCards()
        {
            if (battleManager == null || skillGrid == null || HeroSetMatches())
            {
                return;
            }

            for (int i = 0; i < skillCards.Count; i++)
            {
                if (skillCards[i] != null)
                {
                    DestroyUnityObject(skillCards[i].gameObject);
                }
            }

            skillCards.Clear();
            for (int i = 0; i < battleManager.Heroes.Count; i++)
            {
                HeroController hero = battleManager.Heroes[i];
                if (hero == null)
                {
                    continue;
                }

                BattleSkillCardView card = CreateSkillCard(skillGrid, i);
                card.Bind(battleManager, hero, theme);
                skillCards.Add(card);
            }

            LayoutSkillCards();
            ApplyTheme();
        }

        private BattleSkillCardView CreateSkillCard(RectTransform parent, int index)
        {
            Button button = CreateButton($"HeroSkillCard_{index + 1}", parent, string.Empty, 16f);
            TMP_Text defaultLabel = button.GetComponentInChildren<TMP_Text>();
            if (defaultLabel != null)
            {
                DestroyUnityObject(defaultLabel.gameObject);
            }

            RectTransform rect = button.GetComponent<RectTransform>();
            Image ready = CreateImageRect("ReadyGlow", rect, Color.clear).GetComponent<Image>();
            Stretch(ready.rectTransform);
            ready.raycastTarget = false;

            Image portrait = CreateImageRect("Portrait", rect, theme != null ? theme.AlternateSurface : Color.gray).GetComponent<Image>();
            SetAnchored(portrait.rectTransform, new Vector2(0f, 0.20f), new Vector2(0.28f, 0.94f), new Vector2(10f, 0f), new Vector2(-4f, 0f));

            TMP_Text heroName = CreateText("HeroName", rect, "영웅", 19f, TextAlignmentOptions.BottomLeft, true);
            SetAnchored(heroName.rectTransform, new Vector2(0.29f, 0.57f), new Vector2(1f, 0.95f), new Vector2(4f, 0f), new Vector2(-8f, 0f));
            TMP_Text skillName = CreateText("SkillName", rect, "스킬", 17f, TextAlignmentOptions.TopLeft, false);
            SetAnchored(skillName.rectTransform, new Vector2(0.29f, 0.29f), new Vector2(1f, 0.60f), new Vector2(4f, 0f), new Vector2(-8f, 0f));
            TMP_Text status = CreateText("SkillStatus", rect, "준비", 15f, TextAlignmentOptions.BottomLeft, true);
            SetAnchored(status.rectTransform, new Vector2(0.29f, 0.05f), new Vector2(0.72f, 0.32f), new Vector2(4f, 0f), Vector2.zero);
            TMP_Text cooldownText = CreateText("CooldownText", rect, "준비", 16f, TextAlignmentOptions.BottomRight, true);
            SetAnchored(cooldownText.rectTransform, new Vector2(0.70f, 0.05f), new Vector2(1f, 0.32f), Vector2.zero, new Vector2(-8f, 0f));

            RectTransform hpBar = CreateBar("HeroHp", rect, out Image hpFill, theme != null ? theme.Success : Color.green);
            SetAnchored(hpBar, new Vector2(0.04f, 0.08f), new Vector2(0.27f, 0.16f), Vector2.zero, Vector2.zero);
            RectTransform cooldownBar = CreateBar("SkillCooldown", rect, out Image cooldownFill, theme != null ? theme.RuneBlue : Color.cyan);
            SetAnchored(cooldownBar, new Vector2(0.30f, 0.07f), new Vector2(0.96f, 0.13f), Vector2.zero, Vector2.zero);

            BattleSkillCardView view = button.gameObject.AddComponent<BattleSkillCardView>();
            view.ConfigureView(button, portrait, hpFill, cooldownFill, ready, heroName, skillName, status, cooldownText);
            return view;
        }

        private void RefreshAll()
        {
            HandleStageChanged();
            RefreshCrystal(crystalController != null ? crystalController.CurrentHp : 0, crystalController != null ? crystalController.MaxHp : 0);
            HandleShieldChanged(crystalController != null ? crystalController.ShieldHp : 0);
            HandleWaveChanged(battleManager != null ? battleManager.CurrentWaveNumber : 0, TotalWaves());
            HandleGoldChanged(battleManager != null ? battleManager.GoldEarned : 0);
            HandleBattleStateChanged(battleManager != null ? battleManager.CurrentState : BattleState.None);
            RefreshTutorial();
            RefreshRuneCards(runeSelectionController != null ? runeSelectionController.DisplayedRunes : Array.Empty<RuneData>());
            if (resultController != null && resultController.IsVisible)
            {
                HandleResultViewData(resultController.CurrentViewData);
            }

            RefreshOverlayState(true);
        }

        private void BindEvents()
        {
            if (eventsBound || !Application.isPlaying)
            {
                return;
            }

            if (battleManager != null)
            {
                battleManager.BattleStateChanged += HandleBattleStateChanged;
                battleManager.WaveChanged += HandleWaveChanged;
                battleManager.GoldChanged += HandleGoldChanged;
            }

            if (crystalController != null)
            {
                crystalController.HpChanged += RefreshCrystal;
                crystalController.ShieldChanged += HandleShieldChanged;
            }

            if (pauseController != null)
            {
                pauseController.PauseChanged += HandlePauseChanged;
            }

            if (tutorialManager != null)
            {
                tutorialManager.VisibilityChanged += HandleTutorialVisibility;
                tutorialManager.StepChanged += HandleTutorialStepChanged;
            }

            if (runeSelectionController != null)
            {
                runeSelectionController.VisibilityChanged += HandleRuneVisibility;
                runeSelectionController.OptionsChanged += RefreshRuneCards;
            }

            if (resultController != null)
            {
                resultController.VisibilityChanged += HandleResultVisibility;
                resultController.ViewDataChanged += HandleResultViewData;
            }

            eventsBound = true;
        }

        private void UnbindEvents()
        {
            if (!eventsBound)
            {
                return;
            }

            if (battleManager != null)
            {
                battleManager.BattleStateChanged -= HandleBattleStateChanged;
                battleManager.WaveChanged -= HandleWaveChanged;
                battleManager.GoldChanged -= HandleGoldChanged;
            }

            if (crystalController != null)
            {
                crystalController.HpChanged -= RefreshCrystal;
                crystalController.ShieldChanged -= HandleShieldChanged;
            }

            if (pauseController != null)
            {
                pauseController.PauseChanged -= HandlePauseChanged;
            }

            if (tutorialManager != null)
            {
                tutorialManager.VisibilityChanged -= HandleTutorialVisibility;
                tutorialManager.StepChanged -= HandleTutorialStepChanged;
            }

            if (runeSelectionController != null)
            {
                runeSelectionController.VisibilityChanged -= HandleRuneVisibility;
                runeSelectionController.OptionsChanged -= RefreshRuneCards;
            }

            if (resultController != null)
            {
                resultController.VisibilityChanged -= HandleResultVisibility;
                resultController.ViewDataChanged -= HandleResultViewData;
            }

            eventsBound = false;
        }

        private void AutoAssignControllers()
        {
            battleManager ??= FindAnyObjectByType<BattleManager>();
            crystalController ??= FindAnyObjectByType<CrystalController>();
            pauseController ??= FindAnyObjectByType<BattlePauseController>();
            tutorialManager ??= FindAnyObjectByType<TutorialManager>();
            runeSelectionController ??= FindAnyObjectByType<RuneSelectionUI>();
            resultController ??= FindAnyObjectByType<StageResultUI>();
            pauseController?.Configure(battleManager);
            runeSelectionController?.SetRuntimeGuiEnabled(false);
            resultController?.SetRuntimeGuiEnabled(false);
        }

        private void ConfigureCameraViewport()
        {
            Camera camera = Camera.main;
            if (camera == null || battlefieldViewport == null)
            {
                return;
            }

            BattlefieldCameraFitter fitter = camera.GetComponent<BattlefieldCameraFitter>();
            if (fitter == null)
            {
                fitter = camera.gameObject.AddComponent<BattlefieldCameraFitter>();
            }

            fitter.ConfigureViewport(battlefieldViewport, rootCanvas);
        }

        private void HandleStageChanged()
        {
            StageData stage = battleManager != null ? battleManager.ActiveStageData : null;
            if (stageNumberText != null)
            {
                int stageNumber = stage != null ? PrototypeAssetLoader.GetStageNumber(stage) : 1;
                stageNumberText.text = $"STAGE {Mathf.Max(1, stageNumber)}";
            }

            if (stageNameText != null)
            {
                stageNameText.text = stage != null ? GameTextMapper.StageName(stage) : "봉문 전선";
            }
        }

        private void RefreshCrystal(int current, int maximum)
        {
            int safeMaximum = Mathf.Max(1, maximum);
            if (crystalHpText != null)
            {
                crystalHpText.text = $"HP {Mathf.Max(0, current)}/{safeMaximum}";
            }

            if (crystalHpFill != null)
            {
                crystalHpFill.fillAmount = Mathf.Clamp01(current / (float)safeMaximum);
            }
        }

        private void HandleShieldChanged(int shield)
        {
            if (crystalShieldText != null)
            {
                crystalShieldText.text = shield > 0 ? $"보호막 {shield}" : "보호막 없음";
            }
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
                if (!string.IsNullOrEmpty(BossStatusText))
                {
                    BossStatusText = string.Empty;
                    if (stateText != null)
                    {
                        BattleState state = battleManager != null ? battleManager.CurrentState : BattleState.None;
                        stateText.text = GameTextMapper.BattleStateName(state);
                    }
                }

                return;
            }

            BossPhaseController phaseController = activeBoss.BossPhaseController;
            int currentPhase = phaseController != null ? phaseController.CurrentPhase : 1;
            int maximumPhase = phaseController != null ? phaseController.MaxPhase : 3;
            BossStatusText = $"{activeBoss.Data.DisplayNameKorean} · 페이즈 {currentPhase}/{maximumPhase}";
            if (stateText != null)
            {
                stateText.text = BossStatusText;
            }
        }

        private void HandleWaveChanged(int current, int total)
        {
            if (waveText != null)
            {
                waveText.text = $"웨이브 {Mathf.Max(0, current)}/{Mathf.Max(1, total)}";
            }

            HandleStageChanged();
            if (current <= 0 || waveBannerGroup == null || !Application.isPlaying)
            {
                return;
            }

            bool boss = battleManager != null && battleManager.ActiveStageData != null &&
                        current <= battleManager.ActiveStageData.Waves.Count &&
                        battleManager.ActiveStageData.Waves[current - 1] != null &&
                        battleManager.ActiveStageData.Waves[current - 1].IsBossWave;
            waveBannerTitle.text = boss ? "우두머리 침입" : $"제{current}파";
            waveBannerSubtitle.text = boss ? "균열의 주인이 모습을 드러냅니다" : "봉문 전선을 유지하세요";
            if (waveBannerRoutine != null)
            {
                StopCoroutine(waveBannerRoutine);
            }

            waveBannerRoutine = StartCoroutine(WaveBannerRoutine());
        }

        private void HandleGoldChanged(int gold)
        {
            if (goldText != null)
            {
                goldText.text = $"전리품 {Mathf.Max(0, gold)}";
            }
        }

        private void HandleBattleStateChanged(BattleState state)
        {
            if (stateText != null)
            {
                stateText.text = GameTextMapper.BattleStateName(state);
            }

            HandleStageChanged();
            RefreshOverlayState(true);
        }

        private void HandlePauseButton()
        {
            if (pauseController != null && pauseController.CanPause)
            {
                pauseController.Pause();
            }
        }

        private void HandlePauseChanged(bool paused)
        {
            RefreshOverlayState(true);
        }

        private void HandleTutorialVisibility(bool visible)
        {
            RefreshTutorial();
            RefreshOverlayState(true);
        }

        private void HandleTutorialStepChanged(int index, TutorialStepData step)
        {
            RefreshTutorial();
        }

        private void RefreshTutorial()
        {
            if (tutorialManager == null)
            {
                return;
            }

            TutorialStepData step = tutorialManager.CurrentStep;
            if (tutorialProgressText != null)
            {
                tutorialProgressText.text = $"전술 기록 {Mathf.Clamp(tutorialManager.CurrentStepNumber, 1, Mathf.Max(1, tutorialManager.StepCount))}/{Mathf.Max(1, tutorialManager.StepCount)}";
            }

            if (tutorialTitleText != null)
            {
                tutorialTitleText.text = step != null ? step.Title : "봉문 전술";
            }

            if (tutorialBodyText != null)
            {
                tutorialBodyText.text = step != null ? step.Body : string.Empty;
            }

            if (tutorialPreviousButton != null)
            {
                tutorialPreviousButton.interactable = tutorialManager.CurrentStepIndex > 0;
            }

            TMP_Text nextText = tutorialNextButton != null ? tutorialNextButton.GetComponentInChildren<TMP_Text>() : null;
            if (nextText != null)
            {
                nextText.text = tutorialManager.CurrentStepNumber >= tutorialManager.StepCount ? "기록 완료" : "다음 기록";
            }
        }

        private void HandleRuneVisibility(bool visible)
        {
            RefreshOverlayState(true);
        }

        private void RefreshRuneCards(IReadOnlyList<RuneData> options)
        {
            if (runeCardContainer == null)
            {
                return;
            }

            for (int i = 0; i < runeCards.Count; i++)
            {
                if (runeCards[i] != null)
                {
                    DestroyUnityObject(runeCards[i].gameObject);
                }
            }

            runeCards.Clear();
            if (options == null)
            {
                return;
            }

            for (int i = 0; i < options.Count; i++)
            {
                RuneData rune = options[i];
                if (rune == null)
                {
                    continue;
                }

                int optionIndex = i;
                Button card = CreateButton($"RuneCard_{i + 1}", runeCardContainer, string.Empty, 18f, theme != null ? theme.RuneCardSprite : null);
                TMP_Text defaultLabel = card.GetComponentInChildren<TMP_Text>();
                if (defaultLabel != null)
                {
                    DestroyUnityObject(defaultLabel.gameObject);
                }

                RectTransform rect = card.GetComponent<RectTransform>();
                Color sealColor = RuneSelectionUI.ElementColor(rune.Element);
                sealColor.a = rune.Icon != null ? 1f : 0.28f;
                Image seal = CreateImageRect("RuneSeal", rect, sealColor).GetComponent<Image>();
                seal.sprite = rune.Icon != null ? rune.Icon : theme != null ? theme.RuneCardSprite : null;
                seal.type = rune.Icon != null ? Image.Type.Simple : Image.Type.Sliced;
                seal.preserveAspect = true;
                SetAnchored(seal.rectTransform, new Vector2(0.30f, 0.55f), new Vector2(0.70f, 0.92f), Vector2.zero, Vector2.zero);
                if (rune.Icon == null)
                {
                    TMP_Text sealGlyph = CreateText("RuneSealGlyph", rect, RuneSelectionUI.ElementGlyph(rune.Element), 44f, TextAlignmentOptions.Center, true);
                    sealGlyph.color = RuneSelectionUI.ElementColor(rune.Element);
                    SetAnchored(sealGlyph.rectTransform, new Vector2(0.30f, 0.55f), new Vector2(0.70f, 0.92f), Vector2.zero, Vector2.zero);
                }

                TMP_Text name = CreateText("RuneName", rect, rune.DisplayName, 23f, TextAlignmentOptions.Bottom, true);
                SetAnchored(name.rectTransform, new Vector2(0f, 0.35f), new Vector2(1f, 0.58f), new Vector2(12f, 0f), new Vector2(-12f, 0f));
                TMP_Text meta = CreateText("RuneMeta", rect, $"{GameTextMapper.RuneRarityName(rune.Rarity)} · {RuneSelectionUI.ElementGlyph(rune.Element)} 속성", 16f, TextAlignmentOptions.Center, true);
                meta.color = RuneSelectionUI.RarityColor(rune.Rarity);
                SetAnchored(meta.rectTransform, new Vector2(0f, 0.22f), new Vector2(1f, 0.37f), new Vector2(12f, 0f), new Vector2(-12f, 0f));
                TMP_Text description = CreateText("RuneDescription", rect, rune.Description, 17f, TextAlignmentOptions.Top, false, true);
                SetAnchored(description.rectTransform, new Vector2(0f, 0.03f), new Vector2(1f, 0.23f), new Vector2(14f, 0f), new Vector2(-14f, 0f));
                card.onClick.AddListener(() => runeSelectionController?.SelectOption(optionIndex));
                runeCards.Add(card);
            }

            LayoutRuneCards();
            ApplyTheme();
        }

        private void HandleResultVisibility(bool visible)
        {
            RefreshOverlayState(true);
        }

        private void HandleResultViewData(BattleResultViewData data)
        {
            resultViewData = data;
            if (resultTitleText != null)
            {
                resultTitleText.text = data.Title;
                resultTitleText.color = data.IsVictory && theme != null ? theme.Success : theme != null ? theme.Danger : Color.white;
            }

            if (resultMessageText != null)
            {
                resultMessageText.text = data.Message;
            }

            if (resultStatsText != null)
            {
                resultStatsText.text = $"클리어 시간  {data.Elapsed}\n수정체 HP  {data.CrystalHp}\n처치한 적  {data.Kills}\n웨이브  {data.Waves}";
            }

            if (resultRewardText != null)
            {
                resultRewardText.text = $"{data.Difficulty}\n{data.UnlockMessage}" + (string.IsNullOrWhiteSpace(data.ShardRewards) ? string.Empty : $"\n\n그림자 조각\n{data.ShardRewards}");
            }

            if (resultPrimaryButtonText != null)
            {
                resultPrimaryButtonText.text = data.PrimaryActionLabel;
            }

            if (resultUpgradeButtonText != null)
            {
                resultUpgradeButtonText.text = "봉문 정비소";
            }

            if (resultStageSelectButtonText != null)
            {
                resultStageSelectButtonText.text = "스테이지 선택";
            }

            ApplyTheme();

            if (Application.isPlaying)
            {
                if (resultCountRoutine != null)
                {
                    StopCoroutine(resultCountRoutine);
                }

                resultCountRoutine = StartCoroutine(ResultCountRoutine(data.GoldEarned));
            }
            else if (resultGoldText != null)
            {
                resultGoldText.text = $"획득 골드 +{data.GoldEarned}";
            }
        }

        private void HandleResultPrimary()
        {
            if (resultController == null)
            {
                return;
            }

            if (resultViewData.IsVictory && resultViewData.HasNextStage)
            {
                resultController.ContinueToNextStage();
            }
            else
            {
                resultController.RetryBattle();
            }
        }

        private void RefreshOverlayState(bool force = false)
        {
            BattleOverlayState next = ResolveOverlayState();
            if (!force && next == overlayState)
            {
                return;
            }

            overlayState = next;
            bool modalVisible = next != BattleOverlayState.None;
            Fade(dimGroup, modalVisible, modalVisible);
            Fade(tutorialGroup, next == BattleOverlayState.Tutorial, next == BattleOverlayState.Tutorial);
            Fade(runeGroup, next == BattleOverlayState.RuneSelection, next == BattleOverlayState.RuneSelection);
            Fade(pauseGroup, next == BattleOverlayState.Pause, next == BattleOverlayState.Pause);
            Fade(resultGroup, next == BattleOverlayState.Result, next == BattleOverlayState.Result);

            bool resultVisible = next == BattleOverlayState.Result;
            if (hudGroup != null)
            {
                hudGroup.alpha = resultVisible ? 0f : 1f;
                hudGroup.blocksRaycasts = !resultVisible;
            }

            if (skillGroup != null)
            {
                skillGroup.alpha = resultVisible ? 0f : modalVisible ? 0.38f : 1f;
                skillGroup.interactable = !modalVisible;
                skillGroup.blocksRaycasts = !modalVisible;
            }
        }

        private BattleOverlayState ResolveOverlayState()
        {
            if (resultController != null && resultController.IsVisible)
            {
                return BattleOverlayState.Result;
            }

            if (runeSelectionController != null && runeSelectionController.IsVisible)
            {
                return BattleOverlayState.RuneSelection;
            }

            if (tutorialManager != null && tutorialManager.IsVisible)
            {
                return BattleOverlayState.Tutorial;
            }

            if (pauseController != null && pauseController.IsPaused)
            {
                return BattleOverlayState.Pause;
            }

            return BattleOverlayState.None;
        }

        private void ApplyResponsiveLayout(bool force)
        {
            if (safeAreaRoot == null || battleRoot == null)
            {
                return;
            }

            Vector2 size = safeAreaRoot.rect.size;
            if (!force && (size - lastSafeRootSize).sqrMagnitude < 0.01f)
            {
                return;
            }

            lastSafeRootSize = size;
            BattleCanvasRects rects = BattleCanvasLayout.Calculate(size.x, size.y, new Rect(0f, 0f, size.x, size.y));
            battleRoot.sizeDelta = rects.Root.size;
            battleRoot.anchoredPosition = Vector2.zero;
            Rect localRoot = new Rect(0f, 0f, rects.Root.width, rects.Root.height);
            ApplyLocalRect(hudLayer, ToLocal(rects.Hud, rects.Root));
            ApplyLocalRect(battlefieldViewport, ToLocal(rects.Battlefield, rects.Root));
            ApplyLocalRect(skillLayer, ToLocal(rects.Skills, rects.Root));
            ApplyLocalRect(overlayLayer, localRoot);

            float bannerWidth = Mathf.Min(720f, localRoot.width - 80f);
            float bannerHeight = Mathf.Clamp(localRoot.height * 0.10f, 132f, 180f);
            SetCenteredSize(waveBannerRect, bannerWidth, bannerHeight, localRoot.height * 0.12f);
            LayoutModal(tutorialGroup, localRoot, 680f);
            LayoutModal(runeGroup, localRoot, 820f);
            LayoutModal(pauseGroup, localRoot, 620f);
            LayoutModal(resultGroup, localRoot, 920f);
            LayoutSkillCards();
            LayoutRuneCards();
            ConfigureCameraViewport();
        }

        private void LayoutModal(CanvasGroup group, Rect root, float preferredHeight)
        {
            if (group == null)
            {
                return;
            }

            RectTransform rect = group.GetComponent<RectTransform>();
            float width = Mathf.Min(BattleCanvasLayout.ModalMaximumWidth, root.width - BattleCanvasLayout.ModalMargin * 2f);
            float height = Mathf.Min(preferredHeight, root.height - BattleCanvasLayout.ModalMargin * 2f);
            SetCenteredSize(rect, Mathf.Max(320f, width), Mathf.Max(300f, height), 0f);
        }

        private void LayoutSkillCards()
        {
            if (skillGrid == null || skillCards.Count == 0)
            {
                return;
            }

            Rect area = skillGrid.rect;
            const float gap = 12f;
            float cardWidth = Mathf.Max(1f, (area.width - gap * 2f) / 3f);
            float cardHeight = Mathf.Max(1f, (area.height - gap) / 2f);
            for (int i = 0; i < skillCards.Count; i++)
            {
                RectTransform rect = skillCards[i].GetComponent<RectTransform>();
                int column = i % 3;
                int row = i / 3;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(cardWidth, cardHeight);
                rect.anchoredPosition = new Vector2(column * (cardWidth + gap), -row * (cardHeight + gap));
            }
        }

        private void LayoutRuneCards()
        {
            if (runeCardContainer == null || runeCards.Count == 0)
            {
                return;
            }

            Rect area = runeCardContainer.rect;
            int columns = area.width >= 720f ? Mathf.Min(3, runeCards.Count) : 1;
            int rows = Mathf.CeilToInt(runeCards.Count / (float)columns);
            const float gap = 16f;
            float width = Mathf.Max(1f, (area.width - gap * (columns - 1)) / columns);
            float height = Mathf.Max(BattleCanvasLayout.MinimumTouchHeight, (area.height - gap * (rows - 1)) / rows);
            for (int i = 0; i < runeCards.Count; i++)
            {
                RectTransform rect = runeCards[i].GetComponent<RectTransform>();
                int column = i % columns;
                int row = i / columns;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(width, height);
                rect.anchoredPosition = new Vector2(column * (width + gap), -row * (height + gap));
            }
        }

        private IEnumerator WaveBannerRoutine()
        {
            float enter = theme != null ? theme.WaveEnterDuration : 0.25f;
            float hold = theme != null ? theme.WaveHoldDuration : 0.8f;
            float exit = theme != null ? theme.WaveExitDuration : 0.25f;
            yield return AnimateWaveBanner(0f, 1f, 36f, 0f, enter);
            yield return new WaitForSecondsRealtime(hold);
            yield return AnimateWaveBanner(1f, 0f, 0f, -24f, exit);
            waveBannerRoutine = null;
        }

        private IEnumerator AnimateWaveBanner(float fromAlpha, float toAlpha, float fromY, float toY, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration)));
                waveBannerGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
                Vector2 position = waveBannerRect.anchoredPosition;
                position.y = Mathf.Lerp(fromY, toY, t);
                waveBannerRect.anchoredPosition = position;
                yield return null;
            }

            waveBannerGroup.alpha = toAlpha;
        }

        private IEnumerator ResultCountRoutine(int targetGold)
        {
            float duration = theme != null ? theme.ResultCountDuration : 0.4f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                int value = Mathf.RoundToInt(Mathf.Lerp(0f, targetGold, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration))));
                if (resultGoldText != null)
                {
                    resultGoldText.text = $"획득 골드 +{value}";
                }

                yield return null;
            }

            if (resultGoldText != null)
            {
                resultGoldText.text = $"획득 골드 +{targetGold}";
            }

            resultCountRoutine = null;
        }

        private void Fade(CanvasGroup group, bool visible, bool blocksRaycasts)
        {
            if (group == null)
            {
                return;
            }

            group.blocksRaycasts = blocksRaycasts;
            group.interactable = blocksRaycasts;
            if (!Application.isPlaying || !isActiveAndEnabled)
            {
                SetImmediate(group, visible);
                return;
            }

            if (fadeRoutines.TryGetValue(group, out Coroutine running) && running != null)
            {
                StopCoroutine(running);
            }

            fadeRoutines[group] = StartCoroutine(FadeRoutine(group, visible));
        }

        private IEnumerator FadeRoutine(CanvasGroup group, bool visible)
        {
            float duration = theme != null ? theme.ModalFadeDuration : 0.15f;
            float from = group.alpha;
            float to = visible ? 1f : 0f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            group.alpha = to;
            fadeRoutines.Remove(group);
        }

        private void UpdateDamageNumbers()
        {
            for (int i = 0; i < damageNumbers.Count; i++)
            {
                if (damageNumbers[i].Active)
                {
                    UpdateDamageNumber(damageNumbers[i]);
                }
            }
        }

        private void UpdateDamageNumber(DamageNumberEntry entry)
        {
            float progress = (Time.unscaledTime - entry.StartedAt) / Mathf.Max(0.01f, entry.Duration);
            if (progress >= 1f)
            {
                entry.Active = false;
                entry.Rect.gameObject.SetActive(false);
                return;
            }

            Camera camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            Vector3 screen = camera.WorldToScreenPoint(entry.WorldPosition);
            if (screen.z < 0f)
            {
                entry.Rect.gameObject.SetActive(false);
                entry.Active = false;
                return;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(damageLayer, screen, null, out Vector2 local))
            {
                entry.Rect.anchoredPosition = local + Vector2.up * (32f * progress);
            }

            Color color = entry.Color;
            color.a *= 1f - Mathf.SmoothStep(0.45f, 1f, progress);
            entry.Value.color = color;
        }

        private bool HeroSetMatches()
        {
            if (battleManager == null || skillCards.Count != battleManager.Heroes.Count)
            {
                return false;
            }

            for (int i = 0; i < skillCards.Count; i++)
            {
                if (skillCards[i] == null || skillCards[i].Hero != battleManager.Heroes[i])
                {
                    return false;
                }
            }

            return true;
        }

        private int TotalWaves()
        {
            return battleManager != null && battleManager.ActiveStageData != null ? battleManager.ActiveStageData.Waves.Count : 1;
        }

        private void ApplyTheme()
        {
            if (theme == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
                for (int i = 0; i < texts.Length; i++)
                {
                    TMP_Text text = texts[i];
                    text.font = (text.fontStyle & FontStyles.Bold) != 0 ? theme.Bold : theme.Regular;
                    if (text.color == Color.white)
                    {
                        text.color = theme.PrimaryText;
                    }
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
            if (rootCanvas == null)
            {
                rootCanvas = gameObject.AddComponent<Canvas>();
            }

            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rootCanvas.pixelPerfect = false;
            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(BattleCanvasLayout.ReferenceWidth, BattleCanvasLayout.ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private RectTransform CreateModalPanel(string name)
        {
            return CreatePanel(name, overlayLayer, theme != null ? theme.PanelSprite : null, theme != null ? theme.Surface : Color.black);
        }

        private RectTransform CreatePanel(string name, Transform parent, Sprite sprite, Color color)
        {
            RectTransform rect = CreateImageRect(name, parent, color);
            Image image = rect.GetComponent<Image>();
            image.sprite = sprite;
            image.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            return rect;
        }

        private Button CreateButton(string name, Transform parent, string label, float fontSize, Sprite overrideSprite = null)
        {
            RectTransform rect = CreateImageRect(name, parent, theme != null ? theme.AlternateSurface : new Color32(24, 38, 45, 255));
            Image image = rect.GetComponent<Image>();
            image.sprite = overrideSprite != null ? overrideSprite : theme != null ? theme.ButtonSprite : null;
            image.type = image.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.raycastTarget = true;
            Button button = rect.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
            colors.pressedColor = new Color(0.82f, 0.92f, 0.96f, 1f);
            colors.disabledColor = new Color(0.38f, 0.42f, 0.44f, 0.72f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            rect.gameObject.AddComponent<RuneGateButtonFeedback>();
            TMP_Text text = CreateText("Label", rect, label, fontSize, TextAlignmentOptions.Center, true);
            Stretch(text.rectTransform);
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

        private RectTransform CreateBar(string name, Transform parent, out Image fill, Color fillColor)
        {
            RectTransform background = CreateImageRect(name, parent, theme != null ? theme.Background : Color.black);
            RectTransform fillRect = CreateImageRect("Fill", background, fillColor);
            Stretch(fillRect);
            fill = fillRect.GetComponent<Image>();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 1f;
            fill.raycastTarget = false;
            return background;
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

        private static void CreateFrameEdge(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            RectTransform edge = CreateImageRect(name, parent, color);
            SetAnchored(edge, anchorMin, anchorMax, offsetMin, offsetMax);
            edge.GetComponent<Image>().raycastTarget = false;
        }

        private static void SetHorizontalAnchors(RectTransform rect, float minX, float maxX, float left, float right)
        {
            SetAnchored(rect, new Vector2(minX, 0f), new Vector2(maxX, 1f), new Vector2(left, 8f), new Vector2(right, -8f));
        }

        private static void SetAnchored(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void Stretch(RectTransform rect)
        {
            SetAnchored(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        private static void ApplyLocalRect(RectTransform rectTransform, Rect rect)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = rectTransform.anchorMax = Vector2.zero;
            rectTransform.pivot = Vector2.zero;
            rectTransform.anchoredPosition = rect.position;
            rectTransform.sizeDelta = rect.size;
        }

        private static Rect ToLocal(Rect rect, Rect root)
        {
            return new Rect(rect.x - root.x, rect.y - root.y, rect.width, rect.height);
        }

        private static void SetCenteredSize(RectTransform rect, float width, float height, float yOffset)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(0f, yOffset);
        }

        private static void SetImmediate(CanvasGroup group, bool visible)
        {
            if (group == null)
            {
                return;
            }

            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                DestroyUnityObject(parent.GetChild(i).gameObject);
            }
        }

        private static void DestroyUnityObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private sealed class DamageNumberEntry
        {
            public DamageNumberEntry(RectTransform rect, TMP_Text value)
            {
                Rect = rect;
                Value = value;
            }

            public RectTransform Rect { get; }
            public TMP_Text Value { get; }
            public Vector3 WorldPosition { get; set; }
            public Color Color { get; set; }
            public float StartedAt { get; set; }
            public float Duration { get; set; }
            public bool Active { get; set; }
        }
    }
}
