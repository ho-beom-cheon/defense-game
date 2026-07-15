using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuneGate
{
    public sealed class StageSelectUI : MonoBehaviour
    {
        [SerializeField] private List<StageData> stages = new List<StageData>();
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private string titleSceneName = "TitleScene";
        [SerializeField] private string battleSceneName = "BattleScene";
        [SerializeField] private string upgradeSceneName = "UpgradeScene";
        [SerializeField] private HeroRosterData heroRoster;
        [SerializeField] private FormationData defaultFormation;

        private static readonly HeroPositionType[] FormationColumnOrder =
        {
            HeroPositionType.Back,
            HeroPositionType.Middle,
            HeroPositionType.Front
        };

        private readonly FormationEditorState formationEditor = new FormationEditorState();
        private Vector2 stageScrollPosition;
        private Vector2 detailScrollPosition;
        private Vector2 formationScrollPosition;
        private string feedbackMessage = string.Empty;
        private string formationFeedback = string.Empty;
        private int selectedStageIndex;
        private bool sceneTransitionRequested;
        private bool showFormationEditor;

        public IReadOnlyList<StageData> Stages => stages;
        public IReadOnlyList<HeroData> FormationHeroes => heroRoster != null ? heroRoster.Heroes : null;
        public IReadOnlyList<FormationSlot> FormationSlots => formationEditor.Slots;
        public bool IsFormationEditorVisible => showFormationEditor;

        private void OnEnable()
        {
            SaveManager.LoadOrCreate();
            EnsureStages();
            EnsureFormationContent();
            ReloadFormationFromSave();
            GameSession.SelectDifficulty(SaveManager.Current.selectedDifficultyId);
            selectedStageIndex = FindFirstUnlockedStageIndex();
            sceneTransitionRequested = false;
            showFormationEditor = false;
        }

        private void OnGUI()
        {
            if (!drawRuntimeGui)
            {
                return;
            }

            UIResponsiveLayout.ApplyReadableDefaults();
            EnsureStageOrder();

            StageSelectFrameRects frame = GameFrameLayout.StageSelectFrame();
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateSolidPanelStyle(GUI.skin.box);
            GUI.Box(frame.FrameRoot, GUIContent.none, panelStyle);

            bool previousEnabled = GUI.enabled;
            GUI.enabled = !showFormationEditor;
            DrawStageSelectFrame(frame);
            GUI.enabled = previousEnabled;

            if (showFormationEditor)
            {
                DrawFormationEditorPopup();
            }
        }

        private void DrawStageSelectFrame(StageSelectFrameRects frame)
        {
            GUIStyle boxStyle = RuntimePixelGuiUtility.CreateSolidPanelStyle(GUI.skin.box);
            GUIStyle altBoxStyle = RuntimePixelGuiUtility.CreateSolidPanelStyle(GUI.skin.box, true);
            GUIStyle buttonStyle = RuntimePixelGuiUtility.CreateSolidButtonStyle(GUI.skin.button);
            GUIStyle headerLabelStyle = new GUIStyle(GUI.skin.label);
            KoreanFontManager.ApplyFont(headerLabelStyle);
            headerLabelStyle.alignment = TextAnchor.MiddleLeft;
            headerLabelStyle.wordWrap = false;
            headerLabelStyle.clipping = TextClipping.Clip;
            GUI.Box(frame.HeaderArea, GUIContent.none, boxStyle);

            GUI.Box(frame.StageListPanel, GUIContent.none, altBoxStyle);
            GUI.Box(frame.StageDetailPanel, GUIContent.none, altBoxStyle);

            GUI.Box(frame.FooterArea, GUIContent.none, boxStyle);

            float pad = 10f;
            float headerLineHeight = Mathf.Max(28f, 30f * UIResponsiveLayout.ReadabilityScale);
            float difficultyButtonHeight = UIResponsiveLayout.TouchHeight(32f);
            float headerY = frame.HeaderArea.y + Mathf.Max(6f, (frame.HeaderArea.height - difficultyButtonHeight) * 0.5f);
            float difficultyButtonWidth = Mathf.Clamp(112f * UIResponsiveLayout.ReadabilityScale, 112f, 176f);
            float actionWidth = difficultyButtonWidth;
            float headerContentWidth = Mathf.Max(1f, frame.HeaderArea.width - pad * 2f - actionWidth - 8f);
            float chapterWidth = Mathf.Clamp(headerContentWidth * 0.36f, 88f, 210f);
            float goldWidth = Mathf.Clamp(headerContentWidth * 0.30f, 82f, 170f);
            float slotWidth = Mathf.Max(60f, headerContentWidth - chapterWidth - goldWidth - 12f);
            if (chapterWidth + goldWidth + slotWidth + 12f > headerContentWidth)
            {
                chapterWidth = headerContentWidth * 0.36f;
                goldWidth = headerContentWidth * 0.30f;
                slotWidth = Mathf.Max(1f, headerContentWidth - chapterWidth - goldWidth - 12f);
            }

            float headerX = frame.HeaderArea.x + pad;
            GUI.Label(new Rect(headerX, headerY, chapterWidth, headerLineHeight), "Chapter 1. \uc7ac\ubb38 \uc232", headerLabelStyle);
            GUI.Label(new Rect(headerX + chapterWidth + 6f, headerY, goldWidth, headerLineHeight), $"\ubcf4\uc720 \uace8\ub4dc: {SaveManager.Current.totalGold}", headerLabelStyle);
            string slotText = $"\ud3b8\uc131 {SaveManager.Current.formationSlots.Count}/9";

            GUI.Label(new Rect(headerX + chapterWidth + goldWidth + 12f, headerY, slotWidth, headerLineHeight), slotText, headerLabelStyle);
            float difficultyButtonX = frame.HeaderArea.xMax - actionWidth - pad;
            if (GUI.Button(new Rect(difficultyButtonX, headerY, difficultyButtonWidth, difficultyButtonHeight), $"\ub09c\uc774\ub3c4 {GameTextMapper.Difficulty(SaveManager.Current.selectedDifficultyId)}", buttonStyle))
            {
                CycleDifficulty();
            }

            DrawCompactStageListPanel(frame.StageListPanel, buttonStyle);
            DrawCompactStageDetailPanel(frame.StageDetailPanel, buttonStyle);

            DrawCompactFooter(frame.FooterArea, buttonStyle);
        }

        private void DrawCompactStageListPanel(Rect area, GUIStyle buttonStyle)
        {
            float titleHeight = Mathf.Max(24f, 28f * UIResponsiveLayout.ReadabilityScale);
            GUI.Label(new Rect(area.x + 10f, area.y + 8f, area.width - 20f, titleHeight), "\uc2a4\ud14c\uc774\uc9c0 \ubaa9\ub85d");
            Rect viewRect = new Rect(area.x + 10f, area.y + titleHeight + 12f, Mathf.Max(1f, area.width - 20f), Mathf.Max(1f, area.height - titleHeight - 22f));
            float rowHeight = UIResponsiveLayout.TouchHeight(34f);
            Rect contentRect = new Rect(0f, 0f, Mathf.Max(1f, viewRect.width - 18f), Mathf.Max(viewRect.height, stages.Count * rowHeight + 6f));
            stageScrollPosition = GUI.BeginScrollView(viewRect, stageScrollPosition, contentRect);

            for (int i = 0; i < stages.Count; i++)
            {
                DrawCompactStageButton(i, rowHeight, contentRect.width, buttonStyle);
            }

            GUI.EndScrollView();
        }

        private void DrawCompactStageButton(int index, float rowHeight, float contentWidth, GUIStyle buttonStyle)
        {
            StageData stageData = stages[index];
            float y = index * rowHeight + 2f;
            if (stageData == null)
            {
                GUI.Label(new Rect(8f, y, 320f, rowHeight - 4f), $"{index + 1}. \uc2a4\ud14c\uc774\uc9c0 \ub370\uc774\ud130 \uc5c6\uc74c");
                return;
            }

            bool unlocked = SaveManager.IsStageUnlocked(stageData.StageId);
            bool cleared = SaveManager.IsStageCleared(stageData.StageId);
            string iconPath = cleared ? RuntimePixelAssetLoader.UiStageNodeCleared : unlocked ? RuntimePixelAssetLoader.UiStageNodeUnlocked : RuntimePixelAssetLoader.UiStageNodeLocked;
            float iconSize = Mathf.Min(28f, rowHeight - 8f);
            DrawTextureIcon(new Rect(6f, y + (rowHeight - iconSize) * 0.5f, iconSize, iconSize), iconPath);
            if (GUI.Button(new Rect(38f, y, Mathf.Max(120f, contentWidth - 46f), rowHeight - 4f), BuildStageButtonLabel(index, stageData, unlocked, cleared), buttonStyle))
            {
                selectedStageIndex = index;
            }
        }

        private string BuildStageButtonLabel(int index, StageData stageData, bool unlocked, bool cleared)
        {
            string status = cleared ? "\ud074\ub9ac\uc5b4" : unlocked ? "\ud574\uae08" : "\uc7a0\uae40";
            string difficulty = GetStageDifficultyLabel(index);
            return $"{index + 1}. {GameTextMapper.StageName(stageData)} - {difficulty} ({status})";
        }

        private void DrawCompactStageDetailPanel(Rect area, GUIStyle buttonStyle)
        {
            StageData selectedStage = GetSelectedStage();
            if (selectedStage == null)
            {
                GUI.Label(new Rect(area.x + 10f, area.y + 10f, area.width - 20f, 24f), "\uc120\ud0dd\ub41c \uc2a4\ud14c\uc774\uc9c0 \uc5c6\uc74c");
                return;
            }

            bool unlocked = SaveManager.IsStageUnlocked(selectedStage.StageId);
            bool cleared = SaveManager.IsStageCleared(selectedStage.StageId);
            float actionHeight = UIResponsiveLayout.TouchHeight(34f);
            Rect contentRect = new Rect(area.x + 12f, area.y + 10f, Mathf.Max(1f, area.width - 24f), Mathf.Max(1f, area.height - actionHeight - 30f));
            float lineHeight = Mathf.Max(26f, 28f * UIResponsiveLayout.ReadabilityScale);
            float descriptionHeight = Mathf.Max(lineHeight * 2f, GUI.skin.label.CalcHeight(new GUIContent(string.IsNullOrWhiteSpace(selectedStage.DescriptionKorean) ? "\uade0\uc5f4\uc5d0\uc11c \ubab0\ub824\uc624\ub294 \uc801\uc744 \ub9c9\uc73c\uc138\uc694." : selectedStage.DescriptionKorean), contentRect.width - 18f));
            float requiredHeight = lineHeight * 6f + descriptionHeight + 24f;
            Rect scrollContent = new Rect(0f, 0f, contentRect.width - 18f, Mathf.Max(contentRect.height, requiredHeight));
            detailScrollPosition = GUI.BeginScrollView(contentRect, detailScrollPosition, scrollContent);
            float y = 0f;
            GUI.Label(new Rect(0f, y, scrollContent.width, lineHeight), GameTextMapper.StageName(selectedStage));
            y += lineHeight;
            GUI.Label(new Rect(0f, y, scrollContent.width, lineHeight), $"\ub09c\uc774\ub3c4: {GetStageDifficultyLabel(selectedStageIndex)}");
            y += lineHeight;
            GUI.Label(new Rect(0f, y, scrollContent.width, lineHeight), cleared ? "\uc0c1\ud0dc: \ud074\ub9ac\uc5b4" : unlocked ? "\uc0c1\ud0dc: \ud574\uae08" : "\uc0c1\ud0dc: \uc7a0\uae40");
            y += lineHeight;
            GUI.Label(new Rect(0f, y, scrollContent.width, lineHeight), string.IsNullOrWhiteSpace(selectedStage.SubtitleKorean) ? "\uade0\uc5f4 \ubc29\uc5b4 \uc804\uc120" : selectedStage.SubtitleKorean);
            y += lineHeight;
            GUI.Label(new Rect(0f, y, scrollContent.width, descriptionHeight), string.IsNullOrWhiteSpace(selectedStage.DescriptionKorean) ? "\uade0\uc5f4\uc5d0\uc11c \ubab0\ub824\uc624\ub294 \uc801\uc744 \ub9c9\uc73c\uc138\uc694." : selectedStage.DescriptionKorean);
            y += descriptionHeight;
            GUI.Label(new Rect(0f, y, scrollContent.width, lineHeight), $"\ud06c\ub9ac\uc2a4\ud0c8 HP: {selectedStage.CrystalHp}");
            y += lineHeight;
            GUI.Label(new Rect(0f, y, scrollContent.width, lineHeight), $"\uc6e8\uc774\ube0c: {(selectedStage.Waves != null ? selectedStage.Waves.Count : 0)}");
            GUI.EndScrollView();

            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && unlocked && !sceneTransitionRequested;
            if (GUI.Button(new Rect(area.x + 12f, area.yMax - actionHeight - 12f, area.width - 24f, actionHeight), "\uc804\ud22c \uc2dc\uc791", buttonStyle))
            {
                StartSelectedStage(selectedStage);
            }

            GUI.enabled = previousEnabled;
        }

        private void DrawCompactFooter(Rect area, GUIStyle buttonStyle)
        {
            float buttonWidth = Mathf.Min(200f, (area.width - 44f) / 3f);
            float buttonHeight = UIResponsiveLayout.TouchHeight(34f);
            float y = area.y + Mathf.Max(4f, (area.height - buttonHeight) * 0.5f);
            float buttonStartX = area.xMax - buttonWidth * 3f - 26f;
            if (!string.IsNullOrWhiteSpace(feedbackMessage))
            {
                float feedbackWidth = Mathf.Max(1f, buttonStartX - area.x - 16f);
                GUI.Label(new Rect(area.x + 10f, area.y + 10f, feedbackWidth, 24f), feedbackMessage);
            }

            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && !sceneTransitionRequested;
            if (GUI.Button(new Rect(buttonStartX, y, buttonWidth, buttonHeight), "\ud3b8\uc131", buttonStyle))
            {
                OpenFormationEditor();
            }

            if (GUI.Button(new Rect(buttonStartX + buttonWidth + 8f, y, buttonWidth, buttonHeight), "\uc5c5\uadf8\ub808\uc774\ub4dc", buttonStyle))
            {
                LoadSceneOnce(upgradeSceneName);
            }

            if (GUI.Button(new Rect(buttonStartX + (buttonWidth + 8f) * 2f, y, buttonWidth, buttonHeight), "\ud0c0\uc774\ud2c0\ub85c", buttonStyle))
            {
                LoadSceneOnce(titleSceneName);
            }

            GUI.enabled = previousEnabled;
        }

        private void DrawFormationEditorPopup()
        {
            UIPopupGuiUtility.DrawDimOverlay();
            Rect popupRect = GameFrameLayout.PopupFrame(900f, 1320f, 0.94f, 0.84f);
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUILayout.BeginArea(popupRect, panelStyle);
            GUI.SetNextControlName("PopupLayer_FormationEditor");
            bool closeRequested = UIPopupGuiUtility.DrawHeader("문지기 전술 편성");
            GUILayout.Label("영웅을 선택한 뒤 3개 라인의 후열·중열·전열 슬롯을 누르세요.");
            GUILayout.Space(UIResponsiveLayout.SmallGap);

            float actionReserve = UIResponsiveLayout.TouchHeight(42f) + 34f;
            formationScrollPosition = GUILayout.BeginScrollView(
                formationScrollPosition,
                GUILayout.Height(Mathf.Max(280f, popupRect.height - actionReserve - 82f)));
            DrawFormationRoster();
            GUILayout.Space(UIResponsiveLayout.Gap);
            DrawFormationGrid();
            GUILayout.Space(UIResponsiveLayout.Gap);
            DrawSelectedHeroSummary();
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            bool previousEnabled = GUI.enabled;
            GUI.enabled = !string.IsNullOrWhiteSpace(formationEditor.SelectedHeroId)
                && formationEditor.ContainsHero(formationEditor.SelectedHeroId)
                && formationEditor.Count > 1;
            if (GUILayout.Button("선택 영웅 빼기", GUILayout.Height(UIResponsiveLayout.TouchHeight(38f))))
            {
                TryRemoveSelectedHero();
            }

            GUI.enabled = previousEnabled;
            if (GUILayout.Button("기본 편성 복구", GUILayout.Height(UIResponsiveLayout.TouchHeight(38f))))
            {
                ResetFormationToDefault();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            if (closeRequested)
            {
                CloseFormationEditor();
            }
        }

        private void DrawFormationRoster()
        {
            GUILayout.Label($"영웅 기록 {formationEditor.Count}/9");
            IReadOnlyList<HeroData> heroes = FormationHeroes;
            if (heroes == null || heroes.Count == 0)
            {
                GUILayout.Label("영웅 기록을 불러오지 못했습니다.");
                return;
            }

            for (int i = 0; i < heroes.Count; i += 2)
            {
                GUILayout.BeginHorizontal();
                DrawFormationHeroButton(heroes[i]);
                if (i + 1 < heroes.Count)
                {
                    DrawFormationHeroButton(heroes[i + 1]);
                }
                else
                {
                    GUILayout.FlexibleSpace();
                }

                GUILayout.EndHorizontal();
            }
        }

        private void DrawFormationHeroButton(HeroData heroData)
        {
            if (heroData == null)
            {
                GUILayout.Label("영웅 데이터 없음", GUILayout.ExpandWidth(true));
                return;
            }

            bool selected = formationEditor.SelectedHeroId == heroData.HeroId;
            bool placed = formationEditor.ContainsHero(heroData.HeroId);
            Color previousColor = GUI.backgroundColor;
            if (selected)
            {
                GUI.backgroundColor = new Color(0.42f, 0.78f, 0.52f, 1f);
            }

            string placement = placed ? "배치됨" : "대기";
            string label = $"{heroData.DisplayNameKorean}\n{GameTextMapper.HeroRoleName(heroData.Role)} · {placement}";
            if (GUILayout.Button(label, GUILayout.Height(UIResponsiveLayout.TouchHeight(50f)), GUILayout.ExpandWidth(true)))
            {
                SelectFormationHero(heroData.HeroId);
            }

            GUI.backgroundColor = previousColor;
        }

        private void DrawFormationGrid()
        {
            GUILayout.Label("3라인 전술 슬롯");
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Empty, GUILayout.Width(72f));
            for (int i = 0; i < FormationColumnOrder.Length; i++)
            {
                GUILayout.Label(GameTextMapper.HeroPositionName(FormationColumnOrder[i]), GUILayout.ExpandWidth(true));
            }

            GUILayout.EndHorizontal();

            for (int laneIndex = 0; laneIndex < 3; laneIndex++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"라인 {laneIndex + 1}", GUILayout.Width(72f), GUILayout.Height(UIResponsiveLayout.TouchHeight(54f)));
                for (int positionIndex = 0; positionIndex < FormationColumnOrder.Length; positionIndex++)
                {
                    HeroPositionType positionType = FormationColumnOrder[positionIndex];
                    string heroId = formationEditor.HeroIdAt(laneIndex, positionType);
                    HeroData heroData = FindFormationHero(heroId);
                    string label = heroData != null ? heroData.DisplayNameKorean : "빈 슬롯";
                    if (heroId == formationEditor.SelectedHeroId)
                    {
                        label = $"선택\n{label}";
                    }

                    if (GUILayout.Button(label, GUILayout.Height(UIResponsiveLayout.TouchHeight(54f)), GUILayout.ExpandWidth(true)))
                    {
                        TryPlaceSelectedHero(laneIndex, positionType);
                    }
                }

                GUILayout.EndHorizontal();
            }
        }

        private void DrawSelectedHeroSummary()
        {
            HeroData selectedHero = FindFormationHero(formationEditor.SelectedHeroId);
            if (selectedHero == null)
            {
                GUILayout.Label("영웅을 선택하면 역할과 권장 위치를 확인할 수 있습니다.");
            }
            else
            {
                GUILayout.Label($"선택: {selectedHero.DisplayNameKorean} — {selectedHero.SubtitleKorean}");
                GUILayout.Label($"역할: {GameTextMapper.HeroRoleName(selectedHero.Role)} / 권장 {GameTextMapper.HeroPositionName(selectedHero.PositionType)}");
            }

            if (!string.IsNullOrWhiteSpace(formationFeedback))
            {
                GUILayout.Label(formationFeedback);
            }
        }

        public void OpenFormationEditor()
        {
            EnsureFormationContent();
            ReloadFormationFromSave();
            showFormationEditor = true;
            formationFeedback = "영웅을 선택하고 배치할 슬롯을 누르세요.";
            AudioManager.Play(SfxKey.ButtonClick);
        }

        public void CloseFormationEditor()
        {
            showFormationEditor = false;
            feedbackMessage = $"편성 {formationEditor.Count}/9 저장됨";
            AudioManager.Play(SfxKey.ButtonClick);
        }

        public bool SelectFormationHero(string heroId)
        {
            HeroData heroData = FindFormationHero(heroId);
            if (heroData == null || !formationEditor.SelectHero(heroId))
            {
                formationFeedback = "선택한 영웅 기록을 찾지 못했습니다.";
                return false;
            }

            formationFeedback = $"{heroData.DisplayNameKorean}의 이동할 슬롯을 선택하세요.";
            AudioManager.Play(SfxKey.ButtonClick);
            return true;
        }

        public bool TryPlaceSelectedHero(int laneIndex, HeroPositionType positionType)
        {
            bool changed = formationEditor.TryPlaceSelected(laneIndex, positionType, out formationFeedback);
            if (!changed)
            {
                return false;
            }

            SaveFormation();
            AudioManager.Play(SfxKey.ButtonClick);
            return true;
        }

        public bool TryRemoveSelectedHero()
        {
            bool changed = formationEditor.TryRemoveSelected(out formationFeedback);
            if (!changed)
            {
                return false;
            }

            SaveFormation();
            return true;
        }

        public void ResetFormationToDefault()
        {
            EnsureFormationContent();
            IReadOnlyList<FormationSlot> defaults = defaultFormation != null && defaultFormation.Slots != null && defaultFormation.Slots.Count > 0
                ? defaultFormation.Slots
                : SaveManager.CreateDefaultFormationSlots();
            formationEditor.Load(defaults);
            SelectFirstFormationHero();
            SaveFormation();
            formationFeedback = "기본 편성을 복구하고 저장했습니다.";
            AudioManager.Play(SfxKey.ButtonClick);
        }

        public void ReloadFormationFromSave()
        {
            formationEditor.Load(SaveManager.GetFormationSlots());
            SelectFirstFormationHero();
        }

        private void SaveFormation()
        {
            if (formationEditor.Count <= 0)
            {
                formationFeedback = "전투를 위해 영웅 한 명은 편성해야 합니다.";
                return;
            }

            SaveManager.SetFormationSlots(formationEditor.CreateCopy());
            feedbackMessage = $"편성 {formationEditor.Count}/9 저장됨";
        }

        private void SelectFirstFormationHero()
        {
            IReadOnlyList<HeroData> heroes = FormationHeroes;
            if (heroes == null)
            {
                return;
            }

            for (int i = 0; i < heroes.Count; i++)
            {
                HeroData heroData = heroes[i];
                if (heroData != null)
                {
                    formationEditor.SelectHero(heroData.HeroId);
                    return;
                }
            }
        }

        private HeroData FindFormationHero(string heroId)
        {
            return heroRoster != null ? heroRoster.FindHeroById(heroId) : null;
        }

        private static void DrawTextureIcon(Rect rect, string spritePath)
        {
            Texture2D texture = RuntimePixelGuiUtility.LoadTexture(spritePath);
            if (texture != null)
            {
                GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, true);
            }
        }

        private static string GetStageDifficultyLabel(int index)
        {
            if (index < 3)
            {
                return "\uc26c\uc6c0";
            }

            if (index < 6)
            {
                return "\ubcf4\ud1b5";
            }

            if (index < 9)
            {
                return "\uc5b4\ub824\uc6c0";
            }

            return "\ubcf4\uc2a4";
        }

        private static void CycleDifficulty()
        {
            string current = SaveManager.Current.selectedDifficultyId;
            if (current == "easy")
            {
                GameSession.SelectDifficulty("normal");
                return;
            }

            if (current == "normal")
            {
                GameSession.SelectDifficulty("hard");
                return;
            }

            if (current == "hard")
            {
                GameSession.SelectDifficulty("nightmare");
                return;
            }

            GameSession.SelectDifficulty("easy");
        }

        private StageData GetSelectedStage()
        {
            if (selectedStageIndex < 0 || selectedStageIndex >= stages.Count)
            {
                return null;
            }

            return stages[selectedStageIndex];
        }

        private int FindFirstUnlockedStageIndex()
        {
            for (int i = 0; i < stages.Count; i++)
            {
                if (stages[i] != null && SaveManager.IsStageUnlocked(stages[i].StageId))
                {
                    return i;
                }
            }

            return 0;
        }

        private string GetNextStageId(int index)
        {
            if (index < 0 || index >= stages.Count || stages[index] == null)
            {
                return string.Empty;
            }

            return GameSession.ResolveNextStageId(stages[index].StageId, stages);
        }

        private void StartSelectedStage(StageData selectedStage)
        {
            if (sceneTransitionRequested || selectedStage == null)
            {
                return;
            }

            sceneTransitionRequested = true;
            string nextStageId = GetNextStageId(selectedStageIndex);
            GameSession.SelectStage(selectedStage, nextStageId);
            SceneManager.LoadScene(battleSceneName);
        }

        private void LoadSceneOnce(string sceneName)
        {
            if (sceneTransitionRequested)
            {
                return;
            }

            sceneTransitionRequested = true;
            SceneManager.LoadScene(sceneName);
        }

        private void EnsureStages()
        {
            List<StageData> loadedStages = PrototypeAssetLoader.LoadStages();
            if (loadedStages.Count > 0)
            {
                stages = loadedStages;
                EnsureStageOrder();
                return;
            }

            if (HasAssignedStages())
            {
                EnsureStageOrder();
                Debug.LogWarning("StageSelectUI is using scene-assigned stages because RuntimeContentCatalog did not provide stages.");
                return;
            }

            Debug.LogWarning("StageSelectUI could not find stage data. Run Tools/RuneGate/Sync Runtime Content Catalog.");
        }

        private void EnsureFormationContent()
        {
            if (heroRoster == null)
            {
                heroRoster = PrototypeAssetLoader.LoadHeroRoster();
            }

            if (defaultFormation == null)
            {
                defaultFormation = PrototypeAssetLoader.LoadDefaultFormation();
            }

            if (heroRoster == null)
            {
                Debug.LogWarning("StageSelectUI could not find the MVP hero roster for formation editing.");
            }
        }

        private void EnsureStageOrder()
        {
            PrototypeAssetLoader.SortStagesByStageId(stages);
            if (selectedStageIndex >= stages.Count)
            {
                selectedStageIndex = Mathf.Max(0, stages.Count - 1);
            }
        }

        private bool HasAssignedStages()
        {
            for (int i = 0; i < stages.Count; i++)
            {
                if (stages[i] != null)
                {
                    return true;
                }
            }

            return false;
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
