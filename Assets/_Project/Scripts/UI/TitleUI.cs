using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuneGate
{
    public sealed class TitleUI : MonoBehaviour
    {
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private Rect panelRect = new Rect(320f, 120f, 360f, 260f);
        [SerializeField] private string stageSelectSceneName = "StageSelectScene";

        private string feedbackMessage = string.Empty;

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
            GUILayout.Label("RuneGate Defense");
            GUILayout.Space(12f);

            if (GUILayout.Button("Start", GUILayout.Height(42f)))
            {
                SaveManager.Load();
                SaveManager.Save();
                LoadStageSelect();
            }

            if (SaveManager.HasSaveFile())
            {
                if (GUILayout.Button("Continue", GUILayout.Height(42f)))
                {
                    LoadStageSelect();
                }
            }

            GUILayout.Space(8f);
            if (GUILayout.Button("Reset Save", GUILayout.Height(34f)))
            {
                SaveManager.ResetSave();
                GameSession.ClearSelectedStage();
                GameSession.ClearLastBattleResult();
                feedbackMessage = "Save reset.";
            }

            if (!string.IsNullOrWhiteSpace(feedbackMessage))
            {
                GUILayout.Space(8f);
                GUILayout.Label(feedbackMessage);
            }

            GUILayout.EndArea();
        }

        private void LoadStageSelect()
        {
            SaveManager.Load();
            SceneManager.LoadScene(stageSelectSceneName);
        }
    }
}
