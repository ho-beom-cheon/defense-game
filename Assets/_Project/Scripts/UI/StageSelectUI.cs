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
        private Vector2 petContractScrollPosition;
        private string feedbackMessage = string.Empty;
        private string formationFeedback = string.Empty;
        private string petContractFeedback = string.Empty;
        private int selectedStageIndex;
        private bool sceneTransitionRequested;
        private bool showFormationEditor;
        private bool showPetContract;

        public IReadOnlyList<StageData> Stages => stages;
        public IReadOnlyList<HeroData> FormationHeroes => heroRoster != null ? heroRoster.Heroes : null;
        public IReadOnlyList<FormationSlot> FormationSlots => formationEditor.Slots;
        public bool IsFormationEditorVisible => showFormationEditor;
        public bool IsPetContractVisible => showPetContract;

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
            showPetContract = false;
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
            GUI.enabled = !showFormationEditor && !showPetContract;
            DrawStageSelectFrame(frame);
            GUI.enabled = previousEnabled;

            if (showFormationEditor)
            {
                DrawFormationEditorPopup();
            }
            else if (showPetContract)
            {
                DrawPetContractPopup();
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
            float contractButtonWidth = difficultyButtonWidth;
            float actionWidth = difficultyButtonWidth + contractButtonWidth + 8f;
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
            float contractButtonX = frame.HeaderArea.xMax - contractButtonWidth - pad;
            float difficultyButtonX = contractButtonX - difficultyButtonWidth - 8f;
            if (GUI.Button(new Rect(difficultyButtonX, headerY, difficultyButtonWidth, difficultyButtonHeight), $"\ub09c\uc774\ub3c4 {GameTextMapper.Difficulty(SaveManager.Current.selectedDifficultyId)}", buttonStyle))
            {
                CycleDifficulty();
            }

            if (GUI.Button(new Rect(contractButtonX, headerY, contractButtonWidth, difficultyButtonHeight), "\uadf8\ub9bc\uc790 \uacc4\uc57d", buttonStyle))
            {
                OpenPetContract();
            }

            DrawCompactStageListPanel(frame.StageListPanel, buttonStyle);
            DrawCompactStageDetailPanel(frame.StageDetailPanel, buttonStyle);

            DrawCompactFooter(frame.FooterArea, buttonStyle);
        }

        private void DrawCompactStageListPanel(Rect area, GUIStyle buttonStyle)
        {
            float titleHeight = Mathf.Max(28f, 30f * UIResponsiveLayout.ReadabilityScale);
            GUIStyle titleStyle = CreateStageMapLabelStyle(TextAnchor.MiddleLeft, true, 17f);
            GUIStyle progressStyle = CreateStageMapLabelStyle(TextAnchor.MiddleRight, false, 13f);
            GUI.Label(new Rect(area.x + 10f, area.y + 8f, area.width - 20f, titleHeight), "\uc81c1\uc7a5 \u00b7 \uc7ac\ubb38 \uc232 \uacbd\ub85c", titleStyle);
            GUI.Label(new Rect(area.x + 10f, area.y + 8f, area.width - 20f, titleHeight), $"\ud074\ub9ac\uc5b4 {CountClearedStages()}/{stages.Count}", progressStyle);
            Rect viewRect = new Rect(area.x + 10f, area.y + titleHeight + 12f, Mathf.Max(1f, area.width - 20f), Mathf.Max(1f, area.height - titleHeight - 22f));
            float contentWidth = Mathf.Max(1f, viewRect.width - 18f);
            float nodeSize = Mathf.Clamp(72f * UIResponsiveLayout.ReadabilityScale, 78f, 112f);
            float stepHeight = Mathf.Clamp((viewRect.height - 16f) / Mathf.Max(1, stages.Count), nodeSize + 16f, nodeSize + 50f);
            Rect contentRect = new Rect(0f, 0f, contentWidth, Mathf.Max(viewRect.height, stages.Count * stepHeight + 16f));
            stageScrollPosition = GUI.BeginScrollView(viewRect, stageScrollPosition, contentRect);

            DrawStageMapConnectors(contentRect.width, stepHeight, nodeSize);
            for (int i = 0; i < stages.Count; i++)
            {
                DrawStageMapNode(i, stepHeight, nodeSize, contentRect.width);
            }

            GUI.EndScrollView();
        }

        private void DrawStageMapConnectors(float contentWidth, float stepHeight, float nodeSize)
        {
            for (int i = 0; i < stages.Count - 1; i++)
            {
                Vector2 from = GetStageNodeCenter(i, contentWidth, stepHeight, nodeSize);
                Vector2 to = GetStageNodeCenter(i + 1, contentWidth, stepHeight, nodeSize);
                float middleY = (from.y + to.y) * 0.5f;
                bool reached = IsStageReached(i + 1);
                Color pathColor = reached
                    ? new Color(0.72f, 0.58f, 0.24f, 0.90f)
                    : new Color(0.24f, 0.29f, 0.30f, 0.72f);
                DrawSolidRect(new Rect(from.x - 2f, from.y, 4f, Mathf.Max(1f, middleY - from.y)), pathColor);
                DrawSolidRect(new Rect(Mathf.Min(from.x, to.x), middleY - 2f, Mathf.Abs(to.x - from.x), 4f), pathColor);
                DrawSolidRect(new Rect(to.x - 2f, middleY, 4f, Mathf.Max(1f, to.y - middleY)), pathColor);
            }
        }

        private void DrawStageMapNode(int index, float stepHeight, float nodeSize, float contentWidth)
        {
            StageData stageData = stages[index];
            float rowY = index * stepHeight + 6f;
            Rect rowRect = new Rect(4f, rowY, Mathf.Max(1f, contentWidth - 8f), Mathf.Max(1f, stepHeight - 12f));
            bool selected = index == selectedStageIndex;
            if (selected)
            {
                DrawSolidRect(rowRect, new Color(0.10f, 0.25f, 0.20f, 0.78f));
            }

            if (stageData == null)
            {
                GUI.Label(rowRect, $"{index + 1}. \uc2a4\ud14c\uc774\uc9c0 \ub370\uc774\ud130 \uc5c6\uc74c");
                return;
            }

            string selectedDifficultyId = SaveManager.Current.selectedDifficultyId;
            bool unlocked = SaveManager.IsStageUnlocked(stageData.StageId, selectedDifficultyId);
            bool cleared = SaveManager.IsStageCleared(stageData.StageId, selectedDifficultyId);
            string iconPath = cleared ? RuntimePixelAssetLoader.UiStageNodeCleared : unlocked ? RuntimePixelAssetLoader.UiStageNodeUnlocked : RuntimePixelAssetLoader.UiStageNodeLocked;
            Vector2 center = GetStageNodeCenter(index, contentWidth, stepHeight, nodeSize);
            float selectedScale = selected ? 1.08f : 1f;
            float visualSize = nodeSize * selectedScale;
            Rect iconRect = new Rect(center.x - visualSize * 0.5f, center.y - visualSize * 0.5f, visualSize, visualSize);
            DrawTextureIcon(iconRect, iconPath);

            GUIStyle numberStyle = CreateStageMapLabelStyle(TextAnchor.MiddleCenter, true, 13f);
            if (unlocked && !cleared)
            {
                float centerBadgeSize = visualSize * 0.42f;
                Rect centerBadge = new Rect(center.x - centerBadgeSize * 0.5f, center.y - centerBadgeSize * 0.5f, centerBadgeSize, centerBadgeSize);
                DrawSolidRect(centerBadge, new Color(0.07f, 0.16f, 0.13f, 0.98f));
                GUI.Label(centerBadge, (index + 1).ToString(), numberStyle);
            }
            else
            {
                Rect numberBadge = new Rect(iconRect.xMax - 34f, iconRect.yMax - 28f, 32f, 24f);
                DrawSolidRect(numberBadge, selected ? new Color(0.78f, 0.58f, 0.18f, 0.96f) : new Color(0.03f, 0.06f, 0.07f, 0.96f));
                GUI.Label(numberBadge, (index + 1).ToString("00"), numberStyle);
            }

            float textGap = 10f;
            bool nodeOnLeft = index % 2 == 0;
            Rect textRect = nodeOnLeft
                ? new Rect(iconRect.xMax + textGap, rowRect.y + 10f, Mathf.Max(80f, rowRect.xMax - iconRect.xMax - textGap - 8f), rowRect.height - 20f)
                : new Rect(rowRect.x + 8f, rowRect.y + 10f, Mathf.Max(80f, iconRect.x - rowRect.x - textGap - 8f), rowRect.height - 20f);
            TextAnchor textAnchor = nodeOnLeft ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
            GUIStyle stageNameStyle = CreateStageMapLabelStyle(textAnchor, true, 15f);
            GUIStyle statusStyle = CreateStageMapLabelStyle(textAnchor, false, 12f);
            float nameHeight = textRect.height * 0.56f;
            GUI.Label(new Rect(textRect.x, textRect.y, textRect.width, nameHeight), GameTextMapper.StageName(stageData), stageNameStyle);
            GUI.Label(new Rect(textRect.x, textRect.y + nameHeight - 2f, textRect.width, textRect.height - nameHeight + 2f),
                $"{GetStageDifficultyLabel(index)} \u00b7 {GetStageStatusLabel(unlocked, cleared)}", statusStyle);

            if (GUI.Button(rowRect, GUIContent.none, GUIStyle.none))
            {
                selectedStageIndex = index;
                detailScrollPosition = Vector2.zero;
            }
        }

        private static Vector2 GetStageNodeCenter(int index, float contentWidth, float stepHeight, float nodeSize)
        {
            float sideInset = nodeSize * 0.56f + 12f;
            float x = index % 2 == 0 ? sideInset : Mathf.Max(sideInset, contentWidth - sideInset);
            float y = index * stepHeight + stepHeight * 0.5f + 6f;
            return new Vector2(x, y);
        }

        private void DrawCompactStageDetailPanel(Rect area, GUIStyle buttonStyle)
        {
            StageData selectedStage = GetSelectedStage();
            if (selectedStage == null)
            {
                GUI.Label(new Rect(area.x + 10f, area.y + 10f, area.width - 20f, 24f), "\uc120\ud0dd\ub41c \uc2a4\ud14c\uc774\uc9c0 \uc5c6\uc74c");
                return;
            }

            string selectedDifficultyId = SaveManager.Current.selectedDifficultyId;
            bool unlocked = SaveManager.IsStageUnlocked(selectedStage.StageId, selectedDifficultyId);
            bool cleared = SaveManager.IsStageCleared(selectedStage.StageId, selectedDifficultyId);
            float actionHeight = UIResponsiveLayout.TouchHeight(42f);
            Rect contentRect = new Rect(area.x + 14f, area.y + 12f, Mathf.Max(1f, area.width - 28f), Mathf.Max(1f, area.height - actionHeight - 38f));
            float lineHeight = Mathf.Max(26f, 28f * UIResponsiveLayout.ReadabilityScale);
            float descriptionHeight = Mathf.Max(lineHeight * 2f, GUI.skin.label.CalcHeight(new GUIContent(string.IsNullOrWhiteSpace(selectedStage.DescriptionKorean) ? "\uade0\uc5f4\uc5d0\uc11c \ubab0\ub824\uc624\ub294 \uc801\uc744 \ub9c9\uc73c\uc138\uc694." : selectedStage.DescriptionKorean), contentRect.width - 18f));
            List<MonsterData> stageMonsters = CollectStageMonsters(selectedStage);
            float heroHeaderHeight = Mathf.Max(86f, 92f * UIResponsiveLayout.ReadabilityScale);
            float enemyRowHeight = Mathf.Max(42f, 46f * UIResponsiveLayout.ReadabilityScale);
            float enemyHeight = Mathf.Max(lineHeight, stageMonsters.Count * enemyRowHeight);
            float requiredHeight = heroHeaderHeight + lineHeight * 9f + descriptionHeight + enemyHeight + 52f;
            Rect scrollContent = new Rect(0f, 0f, contentRect.width - 18f, Mathf.Max(contentRect.height, requiredHeight));
            detailScrollPosition = GUI.BeginScrollView(contentRect, detailScrollPosition, scrollContent);
            float y = 0f;
            string iconPath = cleared ? RuntimePixelAssetLoader.UiStageNodeCleared : unlocked ? RuntimePixelAssetLoader.UiStageNodeUnlocked : RuntimePixelAssetLoader.UiStageNodeLocked;
            float headerIconSize = Mathf.Min(80f, heroHeaderHeight - 8f);
            DrawTextureIcon(new Rect(0f, 4f, headerIconSize, headerIconSize), iconPath);
            GUIStyle stageTitleStyle = CreateStageMapLabelStyle(TextAnchor.MiddleLeft, true, 20f);
            GUIStyle stageMetaStyle = CreateStageMapLabelStyle(TextAnchor.MiddleLeft, false, 13f);
            float headerTextX = headerIconSize + 12f;
            GUI.Label(new Rect(headerTextX, 4f, scrollContent.width - headerTextX, heroHeaderHeight * 0.54f), GameTextMapper.StageName(selectedStage), stageTitleStyle);
            GUI.Label(new Rect(headerTextX, heroHeaderHeight * 0.50f, scrollContent.width - headerTextX, heroHeaderHeight * 0.38f),
                $"{GetStageDifficultyLabel(selectedStageIndex)} \u00b7 {GetStageStatusLabel(unlocked, cleared)}", stageMetaStyle);
            y += heroHeaderHeight;

            DrawSectionDivider(scrollContent.width, y);
            y += 12f;
            GUI.Label(new Rect(0f, y, scrollContent.width, lineHeight), string.IsNullOrWhiteSpace(selectedStage.SubtitleKorean) ? "\uade0\uc5f4 \ubc29\uc5b4 \uc804\uc120" : selectedStage.SubtitleKorean, CreateStageMapLabelStyle(TextAnchor.MiddleLeft, true, 15f));
            y += lineHeight;
            GUI.Label(new Rect(0f, y, scrollContent.width, descriptionHeight), string.IsNullOrWhiteSpace(selectedStage.DescriptionKorean) ? "\uade0\uc5f4\uc5d0\uc11c \ubab0\ub824\uc624\ub294 \uc801\uc744 \ub9c9\uc73c\uc138\uc694." : selectedStage.DescriptionKorean);
            y += descriptionHeight;
            DrawSectionDivider(scrollContent.width, y);
            y += 14f;
            GUI.Label(new Rect(0f, y, scrollContent.width, lineHeight), "\uc804\ud22c \uae30\ub85d", CreateStageMapLabelStyle(TextAnchor.MiddleLeft, true, 15f));
            y += lineHeight;
            GUI.Label(new Rect(0f, y, scrollContent.width, lineHeight), $"\ud06c\ub9ac\uc2a4\ud0c8 HP {selectedStage.CrystalHp}   \u00b7   \uc6e8\uc774\ube0c {(selectedStage.Waves != null ? selectedStage.Waves.Count : 0)}");
            y += lineHeight;
            GUI.Label(new Rect(0f, y, scrollContent.width, lineHeight), $"\uc608\uc0c1 \uc804\ud22c \uace8\ub4dc {EstimateStageGold(selectedStage)}+   \u00b7   \ubcf4\uc0c1 x{DifficultyRules.RewardMultiplier(selectedDifficultyId):0.##}");
            y += lineHeight;
            GUI.Label(new Rect(0f, y, scrollContent.width, lineHeight * 2f), BuildDifficultyProgressText());
            y += lineHeight * 2f;

            GUI.Label(new Rect(0f, y, scrollContent.width, lineHeight), "\ucd9c\ud604 \uc801\uc131 \uae30\ub85d", CreateStageMapLabelStyle(TextAnchor.MiddleLeft, true, 15f));
            y += lineHeight;
            if (stageMonsters.Count == 0)
            {
                GUI.Label(new Rect(0f, y, scrollContent.width, lineHeight), "\ucd9c\ud604 \uc801 \uc815\ubcf4 \uc5c6\uc74c");
            }
            else
            {
                for (int i = 0; i < stageMonsters.Count; i++)
                {
                    DrawMonsterRecordRow(new Rect(0f, y, scrollContent.width, enemyRowHeight - 4f), stageMonsters[i]);
                    y += enemyRowHeight;
                }
            }

            GUI.EndScrollView();

            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && unlocked && !sceneTransitionRequested;
            string actionLabel = unlocked ? "\uc120\ud0dd\ud55c \uc804\uc120\uc73c\ub85c \ucd9c\uc804" : "\uc7a0\uae34 \uc804\uc120";
            if (GUI.Button(new Rect(area.x + 14f, area.yMax - actionHeight - 14f, area.width - 28f, actionHeight), actionLabel, buttonStyle))
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

        private int CountClearedStages()
        {
            int count = 0;
            string difficultyId = SaveManager.Current.selectedDifficultyId;
            for (int i = 0; i < stages.Count; i++)
            {
                StageData stageData = stages[i];
                if (stageData != null && SaveManager.IsStageCleared(stageData.StageId, difficultyId))
                {
                    count++;
                }
            }

            return count;
        }

        private bool IsStageReached(int index)
        {
            if (index < 0 || index >= stages.Count || stages[index] == null)
            {
                return false;
            }

            return SaveManager.IsStageUnlocked(stages[index].StageId, SaveManager.Current.selectedDifficultyId);
        }

        private static string GetStageStatusLabel(bool unlocked, bool cleared)
        {
            return cleared ? "\ud074\ub9ac\uc5b4" : unlocked ? "\ucd9c\uc804 \uac00\ub2a5" : "\uc7a0\uae40";
        }

        private static GUIStyle CreateStageMapLabelStyle(TextAnchor alignment, bool bold, float baseFontSize)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = alignment,
                fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
                fontSize = Mathf.RoundToInt(baseFontSize * UIResponsiveLayout.ReadabilityScale),
                wordWrap = true,
                clipping = TextClipping.Clip
            };
            KoreanFontManager.ApplyFont(style);
            return style;
        }

        private static void DrawSolidRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
            GUI.color = previousColor;
        }

        private static void DrawSectionDivider(float width, float y)
        {
            DrawSolidRect(new Rect(0f, y, width, 2f), new Color(0.33f, 0.48f, 0.50f, 0.60f));
        }

        private static List<MonsterData> CollectStageMonsters(StageData stageData)
        {
            List<MonsterData> monsters = new List<MonsterData>();
            if (stageData == null || stageData.Waves == null)
            {
                return monsters;
            }

            for (int waveIndex = 0; waveIndex < stageData.Waves.Count; waveIndex++)
            {
                WaveData waveData = stageData.Waves[waveIndex];
                if (waveData == null || waveData.Spawns == null)
                {
                    continue;
                }

                for (int spawnIndex = 0; spawnIndex < waveData.Spawns.Count; spawnIndex++)
                {
                    WaveSpawnData spawnData = waveData.Spawns[spawnIndex];
                    MonsterData monsterData = spawnData != null ? spawnData.MonsterData : null;
                    if (monsterData != null && !monsters.Contains(monsterData))
                    {
                        monsters.Add(monsterData);
                    }
                }
            }

            return monsters;
        }

        private static int EstimateStageGold(StageData stageData)
        {
            int total = 0;
            if (stageData == null || stageData.Waves == null)
            {
                return total;
            }

            for (int waveIndex = 0; waveIndex < stageData.Waves.Count; waveIndex++)
            {
                WaveData waveData = stageData.Waves[waveIndex];
                if (waveData == null || waveData.Spawns == null)
                {
                    continue;
                }

                for (int spawnIndex = 0; spawnIndex < waveData.Spawns.Count; spawnIndex++)
                {
                    WaveSpawnData spawnData = waveData.Spawns[spawnIndex];
                    if (spawnData != null && spawnData.MonsterData != null)
                    {
                        total += Mathf.Max(0, spawnData.MonsterData.RewardGold) * Mathf.Max(0, spawnData.Count);
                    }
                }
            }

            return DifficultyRules.ApplyMonsterRewardGold(total, SaveManager.Current.selectedDifficultyId);
        }

        private static void DrawMonsterRecordRow(Rect rect, MonsterData monsterData)
        {
            DrawSolidRect(rect, new Color(0.05f, 0.10f, 0.10f, 0.78f));
            float iconSize = Mathf.Min(rect.height - 6f, 46f * UIResponsiveLayout.ReadabilityScale);
            Rect iconRect = new Rect(rect.x + 6f, rect.y + (rect.height - iconSize) * 0.5f, iconSize, iconSize);
            if (monsterData != null && monsterData.RuntimeSprite != null)
            {
                GUI.DrawTexture(iconRect, monsterData.RuntimeSprite.texture, ScaleMode.ScaleToFit, true);
            }

            string monsterName = monsterData != null ? monsterData.DisplayNameKorean : "\uc801\uc131 \uae30\ub85d \uc5c6\uc74c";
            string role = monsterData != null ? monsterData.SubtitleKorean : string.Empty;
            GUIStyle nameStyle = CreateStageMapLabelStyle(TextAnchor.MiddleLeft, true, 13f);
            GUIStyle roleStyle = CreateStageMapLabelStyle(TextAnchor.MiddleLeft, false, 11f);
            float textX = iconRect.xMax + 8f;
            float textWidth = Mathf.Max(1f, rect.xMax - textX - 6f);
            GUI.Label(new Rect(textX, rect.y + 2f, textWidth, rect.height * 0.54f), monsterName, nameStyle);
            GUI.Label(new Rect(textX, rect.y + rect.height * 0.48f, textWidth, rect.height * 0.46f), role, roleStyle);
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

        private void DrawPetContractPopup()
        {
            UIPopupGuiUtility.DrawDimOverlay();
            IReadOnlyList<ShadowPetDefinition> definitions = ShadowContractService.PetDefinitions;
            Rect popupRect = GameFrameLayout.PopupFrame(960f, 1680f, 0.94f, 0.84f);
            PetContractScreenLayoutRects layout = PetContractScreenLayout.Calculate(popupRect, Screen.width, Screen.height, definitions.Count);
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPetPanelBg);
            GUIStyle cardStyle = RuntimePixelGuiUtility.CreateSolidPanelStyle(GUI.skin.box, true);
            GUIStyle equippedCardStyle = RuntimePixelGuiUtility.CreateSolidPanelStyle(GUI.skin.box);
            GUIStyle buttonStyle = RuntimePixelGuiUtility.CreateSolidButtonStyle(GUI.skin.button);
            GUIStyle titleStyle = CreateStageMapLabelStyle(TextAnchor.MiddleLeft, true, 24f);
            GUIStyle summaryStyle = CreateStageMapLabelStyle(TextAnchor.MiddleLeft, false, 13f);
            GUIStyle centeredStyle = CreateStageMapLabelStyle(TextAnchor.MiddleCenter, true, 13f);

            GUI.Box(layout.Popup, GUIContent.none, panelStyle);
            GUI.Label(layout.HeaderTitle, "\uadf8\ub9bc\uc790 \uacc4\uc57d\uc11c", titleStyle);
            GUI.Label(layout.HeaderSummary, $"\uc801\uc131 \uc870\uac01 {CountAllMonsterShards()}\uac1c  \u00b7  \uacc4\uc57d {SaveManager.Current.contractedPetIds.Count}/{definitions.Count}", summaryStyle);
            if (GUI.Button(layout.CloseButton, "\ub2eb\uae30", buttonStyle))
            {
                ClosePetContract();
                GUIUtility.ExitGUI();
            }

            DrawEquippedPetSummary(layout.EquippedSummary);
            petContractScrollPosition = GUI.BeginScrollView(layout.Viewport, petContractScrollPosition, layout.Content);
            for (int i = 0; i < definitions.Count; i++)
            {
                ShadowPetDefinition definition = definitions[i];
                bool equipped = SaveManager.Current.equippedPetId == definition.MonsterId;
                DrawPetContractCard(layout.CardRect(i), definition, equipped ? equippedCardStyle : cardStyle, buttonStyle, centeredStyle);
            }

            GUI.EndScrollView();

            GUI.Label(layout.FooterFeedback, petContractFeedback, summaryStyle);
            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && !string.IsNullOrWhiteSpace(SaveManager.Current.equippedPetId);
            if (GUI.Button(layout.FooterUnequip, "\uc7a5\ucc29 \ud574\uc81c", buttonStyle))
            {
                UnequipPet();
                GUIUtility.ExitGUI();
            }

            GUI.enabled = previousEnabled;
        }

        private void DrawEquippedPetSummary(Rect rect)
        {
            GUIStyle boxStyle = RuntimePixelGuiUtility.CreateSolidPanelStyle(GUI.skin.box, true);
            GUIStyle titleStyle = CreateStageMapLabelStyle(TextAnchor.MiddleLeft, true, 16f);
            GUIStyle summaryStyle = CreateStageMapLabelStyle(TextAnchor.MiddleLeft, false, 12f);
            GUI.Box(rect, GUIContent.none, boxStyle);

            string equippedPetId = SaveManager.Current.equippedPetId;
            ShadowPetDefinition equipped = ShadowContractService.GetDefinition(equippedPetId);
            float iconSize = Mathf.Clamp(rect.height - 16f, 48f, 84f);
            Rect iconRect = new Rect(rect.x + 10f, rect.y + (rect.height - iconSize) * 0.5f, iconSize, iconSize);
            DrawTextureIcon(iconRect, string.IsNullOrWhiteSpace(equipped.MonsterId) ? RuntimePixelAssetLoader.UiPetSlotEmpty : RuntimePixelAssetLoader.UiPetContractSeal);
            float textX = iconRect.xMax + 12f;
            if (string.IsNullOrWhiteSpace(equipped.MonsterId))
            {
                GUI.Label(new Rect(textX, rect.y + 8f, rect.xMax - textX - 10f, rect.height * 0.46f), "\uc7a5\ucc29\ud55c \uadf8\ub9bc\uc790 \uc5c6\uc74c", titleStyle);
                GUI.Label(new Rect(textX, rect.y + rect.height * 0.48f, rect.xMax - textX - 10f, rect.height * 0.40f), "\uacc4\uc57d\ud55c \uadf8\ub9bc\uc790\ub97c \uc7a5\ucc29\ud558\uba74 \uc804\ud22c \ud328\uc2dc\ube0c\uac00 \uc801\uc6a9\ub429\ub2c8\ub2e4.", summaryStyle);
                return;
            }

            GUI.Label(new Rect(textX, rect.y + 8f, rect.xMax - textX - 10f, rect.height * 0.46f), $"\uc7a5\ucc29 \uc911  \u00b7  {equipped.DisplayName}", titleStyle);
            GUI.Label(new Rect(textX, rect.y + rect.height * 0.48f, rect.xMax - textX - 10f, rect.height * 0.40f), ShadowContractService.GetPassiveDescription(equipped), summaryStyle);
        }

        private void DrawPetContractCard(Rect rect, ShadowPetDefinition definition, GUIStyle cardStyle, GUIStyle buttonStyle, GUIStyle centeredStyle)
        {
            GUI.Box(rect, GUIContent.none, cardStyle);
            PetContractCardLayoutRects card = PetContractScreenLayout.CardLayout(rect);
            bool contracted = SaveManager.HasContractedPet(definition.MonsterId);
            bool equipped = SaveManager.Current.equippedPetId == definition.MonsterId;
            DrawTextureIcon(card.Portrait, equipped ? RuntimePixelAssetLoader.UiPetSlotEquipped : RuntimePixelAssetLoader.UiPetSlotEmpty);
            MonsterData monsterData = FindMonsterForPet(definition.MonsterId);
            if (monsterData != null && monsterData.RuntimeSprite != null)
            {
                float portraitInset = Mathf.Clamp(card.Portrait.width * 0.12f, 8f, 14f);
                Rect actorPortrait = new Rect(
                    card.Portrait.x + portraitInset,
                    card.Portrait.y + portraitInset,
                    card.Portrait.width - portraitInset * 2f,
                    card.Portrait.height - portraitInset * 2f);
                GUI.DrawTexture(actorPortrait, monsterData.RuntimeSprite.texture, ScaleMode.ScaleToFit, true);
            }

            DrawTextureIcon(card.PassiveIcon, GetPassiveIconPath(definition.PassiveType));
            int shards = SaveManager.GetMonsterShardCount(definition.MonsterId);
            GUI.Label(card.Title, definition.DisplayName, CreateStageMapLabelStyle(TextAnchor.MiddleLeft, true, 17f));
            GUI.Label(card.State, equipped ? "\uc7a5\ucc29 \uc911" : contracted ? "\uacc4\uc57d \uc644\ub8cc" : "\ubd09\ubb38 \ub300\uc0c1", CreateStageMapLabelStyle(TextAnchor.MiddleLeft, true, 12f));
            GUI.Label(card.Passive, ShadowContractService.GetPassiveDescription(definition), CreateStageMapLabelStyle(TextAnchor.UpperLeft, false, 12f));

            float progress = contracted ? 1f : Mathf.Clamp01(shards / (float)ShadowContractService.RequiredShardCount);
            DrawSolidRect(card.ShardBar, new Color(0.08f, 0.10f, 0.12f, 0.95f));
            DrawSolidRect(new Rect(card.ShardBar.x, card.ShardBar.y, card.ShardBar.width * progress, card.ShardBar.height), contracted ? new Color(0.40f, 0.76f, 0.53f, 1f) : new Color(0.67f, 0.45f, 0.82f, 1f));
            float shardIconSize = Mathf.Min(card.ShardText.height, 30f);
            Rect shardIconRect = new Rect(card.ShardText.x, card.ShardText.y + (card.ShardText.height - shardIconSize) * 0.5f, shardIconSize, shardIconSize);
            DrawTextureIcon(shardIconRect, RuntimePixelAssetLoader.UiPetShadowShard);
            Rect shardLabelRect = new Rect(shardIconRect.xMax + 4f, card.ShardText.y, Mathf.Max(1f, card.ShardText.xMax - shardIconRect.xMax - 4f), card.ShardText.height);
            GUI.Label(shardLabelRect, contracted ? "\uacc4\uc57d \uc644\ub8cc" : $"\uc870\uac01 {shards}/{ShadowContractService.RequiredShardCount}", centeredStyle);

            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && (contracted || ShadowContractService.CanContract(definition.MonsterId));
            string actionLabel = equipped ? "\uc7a5\ucc29 \ud574\uc81c" : contracted ? "\uc7a5\ucc29" : ShadowContractService.CanContract(definition.MonsterId) ? "\uacc4\uc57d" : "\uc870\uac01 \ubd80\uc871";
            if (GUI.Button(card.Action, actionLabel, buttonStyle))
            {
                if (equipped)
                {
                    UnequipPet();
                }
                else if (contracted)
                {
                    EquipPet(definition.MonsterId);
                }
                else
                {
                    TryContractPet(definition.MonsterId);
                }

                GUIUtility.ExitGUI();
            }

            GUI.enabled = previousEnabled;
        }

        public void OpenPetContract()
        {
            showFormationEditor = false;
            showPetContract = true;
            petContractScrollPosition = Vector2.zero;
            petContractFeedback = "\uc801\uc131 \uc870\uac01 5\uac1c\ub85c \uadf8\ub9bc\uc790\uc640 \uacc4\uc57d\ud560 \uc218 \uc788\uc2b5\ub2c8\ub2e4.";
            if (!SaveManager.HasSeenPetTutorial())
            {
                SaveManager.MarkPetTutorialSeen();
            }

            AudioManager.Play(SfxKey.ButtonClick);
        }

        public void ClosePetContract()
        {
            showPetContract = false;
            feedbackMessage = "\uadf8\ub9bc\uc790 \uacc4\uc57d\uc11c\ub97c \ub2eb\uc558\uc2b5\ub2c8\ub2e4.";
            AudioManager.Play(SfxKey.ButtonClick);
        }

        public bool TryContractPet(string monsterId)
        {
            ShadowPetDefinition definition = ShadowContractService.GetDefinition(monsterId);
            if (string.IsNullOrWhiteSpace(definition.MonsterId) || !ShadowContractService.TryContract(monsterId))
            {
                petContractFeedback = "\uacc4\uc57d\ud560 \uc218 \uc5c6\uc2b5\ub2c8\ub2e4. \uc870\uac01 \uc218\ub97c \ud655\uc778\ud558\uc138\uc694.";
                return false;
            }

            petContractFeedback = $"{definition.DisplayName}\uacfc \uacc4\uc57d\ud588\uc2b5\ub2c8\ub2e4. \uc870\uac01 {ShadowContractService.RequiredShardCount}\uac1c\ub97c \uc18c\ubaa8\ud588\uc2b5\ub2c8\ub2e4.";
            AudioManager.Play(SfxKey.UpgradePurchase);
            return true;
        }

        public bool EquipPet(string monsterId)
        {
            if (!SaveManager.HasContractedPet(monsterId))
            {
                petContractFeedback = "\uba3c\uc800 \uadf8\ub9bc\uc790\uc640 \uacc4\uc57d\ud574\uc57c \ud569\ub2c8\ub2e4.";
                return false;
            }

            ShadowContractService.Equip(monsterId);
            ShadowPetDefinition definition = ShadowContractService.GetDefinition(monsterId);
            petContractFeedback = $"{definition.DisplayName}\uc744 \uc7a5\ucc29\ud588\uc2b5\ub2c8\ub2e4.";
            AudioManager.Play(SfxKey.ButtonClick);
            return true;
        }

        public bool UnequipPet()
        {
            if (string.IsNullOrWhiteSpace(SaveManager.Current.equippedPetId))
            {
                petContractFeedback = "\uc7a5\ucc29 \uc911\uc778 \uadf8\ub9bc\uc790\uac00 \uc5c6\uc2b5\ub2c8\ub2e4.";
                return false;
            }

            ShadowContractService.Unequip();
            petContractFeedback = "\uadf8\ub9bc\uc790 \uc7a5\ucc29\uc744 \ud574\uc81c\ud588\uc2b5\ub2c8\ub2e4.";
            AudioManager.Play(SfxKey.ButtonClick);
            return true;
        }

        private int CountAllMonsterShards()
        {
            int total = 0;
            IReadOnlyList<ShadowPetDefinition> definitions = ShadowContractService.PetDefinitions;
            for (int i = 0; i < definitions.Count; i++)
            {
                total += SaveManager.GetMonsterShardCount(definitions[i].MonsterId);
            }

            return total;
        }

        private MonsterData FindMonsterForPet(string monsterId)
        {
            for (int stageIndex = 0; stageIndex < stages.Count; stageIndex++)
            {
                List<MonsterData> monsters = CollectStageMonsters(stages[stageIndex]);
                for (int monsterIndex = 0; monsterIndex < monsters.Count; monsterIndex++)
                {
                    MonsterData monsterData = monsters[monsterIndex];
                    if (monsterData != null && monsterData.MonsterId == monsterId)
                    {
                        return monsterData;
                    }
                }
            }

            return null;
        }

        private static string GetPassiveIconPath(ShadowPetPassiveType passiveType)
        {
            switch (passiveType)
            {
                case ShadowPetPassiveType.GoldRewardPercent:
                    return RuntimePixelAssetLoader.UiPetGoldBoost;
                case ShadowPetPassiveType.CrystalMaxHpPercent:
                    return RuntimePixelAssetLoader.UiPetCrystalGuard;
                case ShadowPetPassiveType.MonsterSlowPercent:
                    return RuntimePixelAssetLoader.UiPetSlowAura;
                default:
                    return RuntimePixelAssetLoader.UiPetAttackBoost;
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

        private void CycleDifficulty()
        {
            SaveData saveData = SaveManager.Current;
            GameSession.SelectDifficulty(DifficultyRules.NextSelectableDifficultyId(saveData, saveData.selectedDifficultyId));
            selectedStageIndex = FindFirstUnlockedStageIndex();
            stageScrollPosition = Vector2.zero;
            detailScrollPosition = Vector2.zero;
        }

        private static string BuildDifficultyProgressText()
        {
            string nextLockedDifficultyId = DifficultyRules.NextLockedDifficultyId(SaveManager.Current);
            if (string.IsNullOrWhiteSpace(nextLockedDifficultyId))
            {
                return "난이도 진행: 모든 난이도 해금 완료";
            }

            return $"다음 해금: {GameTextMapper.Difficulty(nextLockedDifficultyId)} - {GameTextMapper.DifficultyUnlockRequirement(nextLockedDifficultyId)}";
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
            string selectedDifficultyId = SaveManager.Current.selectedDifficultyId;
            for (int i = 0; i < stages.Count; i++)
            {
                if (stages[i] != null && SaveManager.IsStageUnlocked(stages[i].StageId, selectedDifficultyId))
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
