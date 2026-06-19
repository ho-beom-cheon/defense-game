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

        private Vector2 stageScrollPosition;
        private Vector2 detailScrollPosition;
        private string feedbackMessage = string.Empty;
        private int selectedStageIndex;
        private bool sceneTransitionRequested;

        public IReadOnlyList<StageData> Stages => stages;

        private void OnEnable()
        {
            SaveManager.LoadOrCreate();
            EnsureStages();
            GameSession.SelectDifficulty(SaveManager.Current.selectedDifficultyId);
            selectedStageIndex = FindFirstUnlockedStageIndex();
            sceneTransitionRequested = false;
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

            DrawStageSelectFrame(frame);
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
            float headerY = frame.HeaderArea.y + Mathf.Max(6f, (frame.HeaderArea.height - 28f) * 0.5f);
            float difficultyButtonWidth = 112f;
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
            GUI.Label(new Rect(headerX, headerY, chapterWidth, 28f), "Chapter 1. \uc7ac\ubb38 \uc232", headerLabelStyle);
            GUI.Label(new Rect(headerX + chapterWidth + 6f, headerY, goldWidth, 28f), $"\ubcf4\uc720 \uace8\ub4dc: {SaveManager.Current.totalGold}", headerLabelStyle);
            string slotText = $"\ud3b8\uc131 {SaveManager.Current.formationSlots.Count}/9";

            GUI.Label(new Rect(headerX + chapterWidth + goldWidth + 12f, headerY, slotWidth, 28f), slotText, headerLabelStyle);
            float difficultyButtonX = frame.HeaderArea.xMax - actionWidth - pad;
            if (GUI.Button(new Rect(difficultyButtonX, headerY - 2f, difficultyButtonWidth, 32f), $"\ub09c\uc774\ub3c4 {GameTextMapper.Difficulty(SaveManager.Current.selectedDifficultyId)}", buttonStyle))
            {
                CycleDifficulty();
            }

            DrawCompactStageListPanel(frame.StageListPanel, buttonStyle);
            DrawCompactStageDetailPanel(frame.StageDetailPanel, buttonStyle);

            DrawCompactFooter(frame.FooterArea, buttonStyle);
        }

        private void DrawCompactStageListPanel(Rect area, GUIStyle buttonStyle)
        {
            GUI.Label(new Rect(area.x + 10f, area.y + 8f, area.width - 20f, 24f), "\uc2a4\ud14c\uc774\uc9c0 \ubaa9\ub85d");
            Rect viewRect = new Rect(area.x + 10f, area.y + 36f, Mathf.Max(1f, area.width - 20f), Mathf.Max(1f, area.height - 46f));
            float rowHeight = 34f;
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
            DrawTextureIcon(new Rect(6f, y + 4f, 22f, 22f), iconPath);
            if (GUI.Button(new Rect(34f, y, Mathf.Max(120f, contentWidth - 42f), rowHeight - 4f), BuildStageButtonLabel(index, stageData, unlocked, cleared), buttonStyle))
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
            Rect contentRect = new Rect(area.x + 12f, area.y + 10f, Mathf.Max(1f, area.width - 24f), Mathf.Max(1f, area.height - 62f));
            Rect scrollContent = new Rect(0f, 0f, contentRect.width - 18f, 220f);
            detailScrollPosition = GUI.BeginScrollView(contentRect, detailScrollPosition, scrollContent);
            float y = 0f;
            GUI.Label(new Rect(0f, y, scrollContent.width, 24f), GameTextMapper.StageName(selectedStage));
            y += 26f;
            GUI.Label(new Rect(0f, y, scrollContent.width, 22f), $"\ub09c\uc774\ub3c4: {GetStageDifficultyLabel(selectedStageIndex)}");
            y += 24f;
            GUI.Label(new Rect(0f, y, scrollContent.width, 22f), cleared ? "\uc0c1\ud0dc: \ud074\ub9ac\uc5b4" : unlocked ? "\uc0c1\ud0dc: \ud574\uae08" : "\uc0c1\ud0dc: \uc7a0\uae40");
            y += 28f;
            GUI.Label(new Rect(0f, y, scrollContent.width, 24f), string.IsNullOrWhiteSpace(selectedStage.SubtitleKorean) ? "\uade0\uc5f4 \ubc29\uc5b4 \uc804\uc120" : selectedStage.SubtitleKorean);
            y += 28f;
            GUI.Label(new Rect(0f, y, scrollContent.width, 52f), string.IsNullOrWhiteSpace(selectedStage.DescriptionKorean) ? "\uade0\uc5f4\uc5d0\uc11c \ubab0\ub824\uc624\ub294 \uc801\uc744 \ub9c9\uc73c\uc138\uc694." : selectedStage.DescriptionKorean);
            y += 56f;
            GUI.Label(new Rect(0f, y, scrollContent.width, 22f), $"\ud06c\ub9ac\uc2a4\ud0c8 HP: {selectedStage.CrystalHp}");
            y += 24f;
            GUI.Label(new Rect(0f, y, scrollContent.width, 22f), $"\uc6e8\uc774\ube0c: {(selectedStage.Waves != null ? selectedStage.Waves.Count : 0)}");
            GUI.EndScrollView();

            bool previousEnabled = GUI.enabled;
            GUI.enabled = unlocked && !sceneTransitionRequested;
            if (GUI.Button(new Rect(area.x + 12f, area.yMax - 46f, area.width - 24f, 34f), "\uc804\ud22c \uc2dc\uc791", buttonStyle))
            {
                StartSelectedStage(selectedStage);
            }

            GUI.enabled = previousEnabled;
        }

        private void DrawCompactFooter(Rect area, GUIStyle buttonStyle)
        {
            if (!string.IsNullOrWhiteSpace(feedbackMessage))
            {
                GUI.Label(new Rect(area.x + 10f, area.y + 10f, area.width * 0.35f, 24f), feedbackMessage);
            }

            float buttonWidth = Mathf.Min(240f, (area.width - 36f) * 0.5f);
            float y = area.y + Mathf.Max(6f, (area.height - 34f) * 0.5f);
            bool previousEnabled = GUI.enabled;
            GUI.enabled = !sceneTransitionRequested;
            if (GUI.Button(new Rect(area.xMax - buttonWidth * 2f - 18f, y, buttonWidth, 34f), "\uc5c5\uadf8\ub808\uc774\ub4dc", buttonStyle))
            {
                LoadSceneOnce(upgradeSceneName);
            }

            if (GUI.Button(new Rect(area.xMax - buttonWidth - 10f, y, buttonWidth, 34f), "\ud0c0\uc774\ud2c0\ub85c", buttonStyle))
            {
                LoadSceneOnce(titleSceneName);
            }

            GUI.enabled = previousEnabled;
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
