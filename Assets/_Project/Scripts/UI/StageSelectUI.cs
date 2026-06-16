using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuneGate
{
    public sealed class StageSelectUI : MonoBehaviour
    {
        [SerializeField] private List<StageData> stages = new List<StageData>();
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private Rect panelRect = new Rect(260f, 64f, 520f, 520f);
        [SerializeField] private string titleSceneName = "TitleScene";
        [SerializeField] private string battleSceneName = "BattleScene";
        [SerializeField] private string upgradeSceneName = "UpgradeScene";

        private Vector2 stageScrollPosition;
        private string feedbackMessage = string.Empty;

        public IReadOnlyList<StageData> Stages => stages;

        private void OnEnable()
        {
            SaveManager.LoadOrCreate();
            EnsureStages();
            GameSession.SelectDifficulty(SaveManager.Current.selectedDifficultyId);
        }

        private void OnGUI()
        {
            if (!drawRuntimeGui)
            {
                return;
            }

            KoreanFontManager.ApplyToGuiSkin();
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUILayout.BeginArea(panelRect, panelStyle);
            GUILayout.Label("스테이지 선택");
            GUILayout.Label($"골드: {SaveManager.Current.totalGold}");
            GUILayout.Label($"편성 슬롯: {SaveManager.Current.formationSlots.Count}/9");
            GUILayout.Label("월드 1: 재문 숲");
            GUILayout.Space(8f);

            DrawDifficultySelector();
            GUILayout.Space(8f);

            if (stages.Count == 0)
            {
                GUILayout.Label("스테이지 데이터가 없습니다. Tools/RuneGate/Bootstrap v1.0 Release Track을 실행하세요.");
            }

            stageScrollPosition = GUILayout.BeginScrollView(stageScrollPosition, GUILayout.Height(300f));
            for (int i = 0; i < stages.Count; i++)
            {
                DrawStageButton(i);
            }

            GUILayout.EndScrollView();

            if (!string.IsNullOrWhiteSpace(feedbackMessage))
            {
                GUILayout.Space(6f);
                GUILayout.Label(feedbackMessage);
            }

            GUILayout.Space(10f);
            if (GUILayout.Button("업그레이드", GUILayout.Height(34f)))
            {
                SceneManager.LoadScene(upgradeSceneName);
            }

            if (GUILayout.Button("타이틀로", GUILayout.Height(30f)))
            {
                SceneManager.LoadScene(titleSceneName);
            }

            GUILayout.EndArea();
        }

        private void DrawDifficultySelector()
        {
            GUILayout.Label($"난이도: {GetDifficultyDisplayName(SaveManager.Current.selectedDifficultyId)}");
            GUILayout.BeginHorizontal();
            DrawDifficultyButton("easy", "Easy", RuntimePixelAssetLoader.UiBadgeEasy, true);
            DrawDifficultyButton("normal", "Normal", RuntimePixelAssetLoader.UiBadgeNormal, true);
            DrawDifficultyButton("hard", "Hard", RuntimePixelAssetLoader.UiBadgeHard, false);
            DrawDifficultyButton("nightmare", "Nightmare", RuntimePixelAssetLoader.UiBadgeNightmare, false);
            GUILayout.EndHorizontal();
        }

        private void DrawDifficultyButton(string difficultyId, string label, string iconPath, bool enabled)
        {
            GUILayout.BeginVertical(GUILayout.Width(110f));
            RuntimePixelGuiUtility.DrawIcon(iconPath, 28f);
            bool selected = SaveManager.Current.selectedDifficultyId == difficultyId;
            using (new GuiEnabledScope(enabled))
            {
                string buttonLabel = selected ? $"[{label}]" : label;
                if (GUILayout.Button(buttonLabel, GUILayout.Height(30f)))
                {
                    GameSession.SelectDifficulty(difficultyId);
                    feedbackMessage = $"{label} 선택됨.";
                }
            }

            if (!enabled)
            {
                GUILayout.Label("잠김");
            }

            GUILayout.EndVertical();
        }

        private void DrawStageButton(int index)
        {
            EnsureStages();
            StageData stageData = stages[index];
            if (stageData == null)
            {
                GUILayout.Label($"Stage {index + 1}: Missing StageData");
                return;
            }

            bool unlocked = SaveManager.IsStageUnlocked(stageData.StageId);
            bool cleared = SaveManager.IsStageCleared(stageData.StageId);
            string status = cleared ? "클리어" : unlocked ? "해금" : "잠김";
            string difficulty = GetStageDifficultyLabel(index);
            string iconPath = cleared ? RuntimePixelAssetLoader.UiStageNodeCleared : unlocked ? RuntimePixelAssetLoader.UiStageNodeUnlocked : RuntimePixelAssetLoader.UiStageNodeLocked;
            string label = $"{index + 1}. {stageData.DisplayNameKorean} - {difficulty} ({status})";

            GUILayout.BeginHorizontal(GUI.skin.box);
            RuntimePixelGuiUtility.DrawIcon(iconPath, 34f);
            using (new GuiEnabledScope(unlocked))
            {
                if (GUILayout.Button(label, GUILayout.Height(42f)))
                {
                    string nextStageId = GetNextStageId(index);
                    GameSession.SelectStage(stageData, nextStageId);
                    SceneManager.LoadScene(battleSceneName);
                }
            }

            GUILayout.EndHorizontal();

            if (!unlocked)
            {
                GUILayout.Label("이전 스테이지를 클리어하면 해금됩니다.");
            }
        }

        private static string GetStageDifficultyLabel(int index)
        {
            if (index < 3)
            {
                return "쉬움";
            }

            if (index < 6)
            {
                return "보통";
            }

            if (index < 9)
            {
                return "어려움 예고";
            }

            return "보스";
        }

        private static string GetDifficultyDisplayName(string difficultyId)
        {
            switch (difficultyId)
            {
                case "easy":
                    return "쉬움";
                case "hard":
                    return "어려움";
                case "nightmare":
                    return "악몽";
                default:
                    return "보통";
            }
        }

        private string GetNextStageId(int index)
        {
            int nextIndex = index + 1;
            if (nextIndex < 0 || nextIndex >= stages.Count || stages[nextIndex] == null)
            {
                return string.Empty;
            }

            return stages[nextIndex].StageId;
        }

        private void EnsureStages()
        {
            if (HasAssignedStages())
            {
                return;
            }

            List<StageData> loadedStages = PrototypeAssetLoader.LoadStages();
            if (loadedStages.Count > 0)
            {
                stages = loadedStages;
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
