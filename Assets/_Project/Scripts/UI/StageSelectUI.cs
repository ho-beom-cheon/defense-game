using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuneGate
{
    public sealed class StageSelectUI : MonoBehaviour
    {
        [SerializeField] private List<StageData> stages = new List<StageData>();
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private Rect panelRect = new Rect(260f, 80f, 460f, 360f);
        [SerializeField] private string titleSceneName = "TitleScene";
        [SerializeField] private string battleSceneName = "BattleScene";
        [SerializeField] private string upgradeSceneName = "UpgradeScene";

        private Vector2 stageScrollPosition;

        public IReadOnlyList<StageData> Stages => stages;

        private void OnEnable()
        {
            SaveManager.LoadOrCreate();
        }

        private void OnGUI()
        {
            if (!drawRuntimeGui)
            {
                return;
            }

            GUILayout.BeginArea(panelRect, GUI.skin.box);
            GUILayout.Label("Stage Select");
            GUILayout.Label($"Gold: {SaveManager.Current.totalGold}");
            GUILayout.Label($"Formation Slots: {SaveManager.Current.formationSlots.Count}/9");
            GUILayout.Space(8f);

            if (stages.Count == 0)
            {
                GUILayout.Label("No stages assigned. Run Tools/RuneGate/Bootstrap Progression Prototype.");
            }

            stageScrollPosition = GUILayout.BeginScrollView(stageScrollPosition);
            for (int i = 0; i < stages.Count; i++)
            {
                DrawStageButton(i);
            }

            GUILayout.EndScrollView();

            GUILayout.Space(10f);
            if (GUILayout.Button("Go to Upgrade", GUILayout.Height(34f)))
            {
                SceneManager.LoadScene(upgradeSceneName);
            }

            if (GUILayout.Button("Back to Title", GUILayout.Height(30f)))
            {
                SceneManager.LoadScene(titleSceneName);
            }

            GUILayout.EndArea();
        }

        private void DrawStageButton(int index)
        {
            StageData stageData = stages[index];
            if (stageData == null)
            {
                GUILayout.Label($"Stage {index + 1}: Missing StageData");
                return;
            }

            bool unlocked = SaveManager.IsStageUnlocked(stageData.StageId);
            bool cleared = SaveManager.IsStageCleared(stageData.StageId);
            string status = cleared ? "Cleared" : unlocked ? "Unlocked" : "Locked";
            string label = $"{index + 1}. {stageData.DisplayName} ({status})";

            using (new GuiEnabledScope(unlocked))
            {
                if (GUILayout.Button(label, GUILayout.Height(42f)))
                {
                    string nextStageId = GetNextStageId(index);
                    GameSession.SelectStage(stageData, nextStageId);
                    SceneManager.LoadScene(battleSceneName);
                }
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
