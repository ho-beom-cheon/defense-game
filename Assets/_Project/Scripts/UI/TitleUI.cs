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
        private bool confirmReset;
        private bool showSettings;

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
            if (GUILayout.Button("Settings", GUILayout.Height(34f)))
            {
                showSettings = !showSettings;
                confirmReset = false;
            }

            if (showSettings)
            {
                GUILayout.Label("BGM Volume: 100% (placeholder)");
                GUILayout.Label("SFX Volume: 100% (placeholder)");
                GUILayout.Label("Vibration: On (placeholder)");
            }

            if (!confirmReset && GUILayout.Button("Reset Save", GUILayout.Height(34f)))
            {
                confirmReset = true;
                feedbackMessage = "Press Confirm Reset to delete local progress.";
            }

            if (confirmReset && GUILayout.Button("Confirm Reset", GUILayout.Height(34f)))
            {
                SaveManager.ResetSave();
                GameSession.ClearSelectedStage();
                GameSession.ClearLastBattleResult();
                confirmReset = false;
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
