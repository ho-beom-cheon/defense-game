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

            KoreanFontManager.ApplyToGuiSkin();
            GUILayout.BeginArea(panelRect, GUI.skin.box);
            GUILayout.Label("RuneGate Defense");
            GUILayout.Space(12f);

            if (GUILayout.Button("시작", GUILayout.Height(42f)))
            {
                SaveManager.Load();
                SaveManager.Save();
                LoadStageSelect();
            }

            if (SaveManager.HasSaveFile())
            {
                if (GUILayout.Button("계속하기", GUILayout.Height(42f)))
                {
                    LoadStageSelect();
                }
            }

            GUILayout.Space(8f);
            if (GUILayout.Button("설정", GUILayout.Height(34f)))
            {
                showSettings = !showSettings;
                confirmReset = false;
            }

            if (showSettings)
            {
                RuntimePixelGuiUtility.DrawIcon(RuntimePixelAssetLoader.UiIconSettings, 24f);
                GUILayout.Label("BGM 볼륨: 100% (placeholder)");
                GUILayout.Label("SFX 볼륨: 100% (placeholder)");
                GUILayout.Label("진동: 켜짐 (placeholder)");
                if (GUILayout.Button("다음 전투에서 튜토리얼 다시 보기", GUILayout.Height(30f)))
                {
                    SaveManager.ResetTutorialSeen();
                    feedbackMessage = "다음 전투에서 튜토리얼이 다시 표시됩니다.";
                }
            }

            if (!confirmReset && GUILayout.Button("저장 초기화", GUILayout.Height(34f)))
            {
                confirmReset = true;
                feedbackMessage = "저장 데이터를 지우려면 초기화 확인을 누르세요.";
            }

            if (confirmReset)
            {
                RuntimePixelGuiUtility.DrawIcon(RuntimePixelAssetLoader.UiIconResetSave, 24f);
            }

            if (confirmReset && GUILayout.Button("초기화 확인", GUILayout.Height(34f)))
            {
                SaveManager.ResetSave();
                GameSession.ClearSelectedStage();
                GameSession.ClearLastBattleResult();
                confirmReset = false;
                feedbackMessage = "저장 데이터가 초기화되었습니다.";
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
