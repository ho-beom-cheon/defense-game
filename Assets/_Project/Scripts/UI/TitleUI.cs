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
        private bool confirmNewGame;
        private bool showSettings;
        private Vector2 scrollPosition;
        private bool sceneTransitionRequested;

        private void OnEnable()
        {
            SaveManager.LoadOrCreate();
            sceneTransitionRequested = false;
        }

        private void OnGUI()
        {
            if (!drawRuntimeGui)
            {
                return;
            }

            UIResponsiveLayout.ApplyReadableDefaults();
            ScreenFrameRects frame = GameFrameLayout.TitleFrame(showSettings || confirmReset || confirmNewGame);
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUI.Box(frame.FrameRoot, GUIContent.none, panelStyle);

            GUILayout.BeginArea(frame.HeaderArea);
            GUILayout.Label("RuneGate Defense");
            GUILayout.Label("\ubd09\ubb38 \uc804\uc120\uc744 \uc9c0\ud718\ud558\uc138\uc694.");
            GUILayout.EndArea();

            GUILayout.BeginArea(frame.MainArea);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(Mathf.Max(120f, frame.MainArea.height)));
            GUILayout.Space(UIResponsiveLayout.Gap);

            using (new GuiEnabledScope(!sceneTransitionRequested))
            {
                string newGameLabel = SaveManager.HasSaveFile() ? "\uc0c8 \uac8c\uc784" : "\uc0c8 \uac8c\uc784 \uc2dc\uc791";
                if (GUILayout.Button(newGameLabel, GUILayout.Height(UIResponsiveLayout.TouchHeight(42f))))
                {
                    HandleNewGamePressed();
                }

                if (SaveManager.HasSaveFile())
                {
                    if (GUILayout.Button("\uc774\uc5b4\ud558\uae30", GUILayout.Height(UIResponsiveLayout.TouchHeight(42f))))
                    {
                        LoadStageSelect();
                    }
                }
            }

            if (confirmNewGame)
            {
                GUILayout.Space(UIResponsiveLayout.SmallGap);
                GUILayout.Label("\uae30\uc874 \uc800\uc7a5\uc744 \ucd08\uae30\ud654\ud558\uace0 \uc0c8 \uc804\uc120\uc744 \uc2dc\uc791\ud569\ub2c8\ub2e4.");
                GUILayout.BeginHorizontal();
                using (new GuiEnabledScope(!sceneTransitionRequested))
                {
                    if (GUILayout.Button("\uc0c8 \uac8c\uc784 \uc2dc\uc791", GUILayout.Height(UIResponsiveLayout.TouchHeight(34f))))
                    {
                        StartNewGame();
                    }
                }

                if (GUILayout.Button("\ucde8\uc18c", GUILayout.Height(UIResponsiveLayout.TouchHeight(34f))))
                {
                    confirmNewGame = false;
                    feedbackMessage = string.Empty;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.Space(UIResponsiveLayout.SmallGap);
            if (GUILayout.Button("\uc124\uc815", GUILayout.Height(UIResponsiveLayout.TouchHeight(34f))))
            {
                showSettings = !showSettings;
                confirmReset = false;
                confirmNewGame = false;
            }

            if (showSettings)
            {
                RuntimePixelGuiUtility.DrawIcon(RuntimePixelAssetLoader.UiIconSettings, 24f);
                GUILayout.Label("BGM \ubcfc\ub968: 100% (placeholder)");
                GUILayout.Label("SFX \ubcfc\ub968: 100% (placeholder)");
                GUILayout.Label("\uc9c4\ub3d9: \ucf1c\uc9d0 (placeholder)");
                if (GUILayout.Button("\ub2e4\uc74c \uc804\ud22c\uc5d0\uc11c \ud29c\ud1a0\ub9ac\uc5bc \ub2e4\uc2dc \ubcf4\uae30", GUILayout.Height(UIResponsiveLayout.TouchHeight(34f))))
                {
                    SaveManager.ResetTutorialSeen();
                    feedbackMessage = "\ub2e4\uc74c \uc804\ud22c\uc5d0\uc11c \ud29c\ud1a0\ub9ac\uc5bc\uc744 \ub2e4\uc2dc \ud45c\uc2dc\ud569\ub2c8\ub2e4.";
                }
            }

            if (!confirmReset && GUILayout.Button("\uc800\uc7a5 \ucd08\uae30\ud654", GUILayout.Height(UIResponsiveLayout.TouchHeight(34f))))
            {
                confirmReset = true;
                confirmNewGame = false;
                feedbackMessage = "\uc800\uc7a5 \ub370\uc774\ud130\ub97c \uc9c0\uc6b0\ub824\uba74 \ucd08\uae30\ud654 \ud655\uc778\uc744 \ub204\ub974\uc138\uc694.";
            }

            if (confirmReset)
            {
                RuntimePixelGuiUtility.DrawIcon(RuntimePixelAssetLoader.UiIconResetSave, 24f);
            }

            if (confirmReset && GUILayout.Button("\ucd08\uae30\ud654 \ud655\uc778", GUILayout.Height(UIResponsiveLayout.TouchHeight(34f))))
            {
                SaveManager.ResetSave();
                GameSession.ClearSelectedStage();
                GameSession.ClearLastBattleResult();
                confirmReset = false;
                confirmNewGame = false;
                feedbackMessage = "\uc800\uc7a5 \ub370\uc774\ud130\uac00 \ucd08\uae30\ud654\ub418\uc5c8\uc2b5\ub2c8\ub2e4.";
            }

            if (!string.IsNullOrWhiteSpace(feedbackMessage))
            {
                GUILayout.Space(UIResponsiveLayout.SmallGap);
                GUILayout.Label(feedbackMessage);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            GUILayout.BeginArea(frame.FooterArea);
            GUILayout.Label("\ub85c\uceec \uc800\uc7a5 / \uc624\ud504\ub77c\uc778 \ud50c\ub808\uc774");
            GUILayout.EndArea();
        }

        private void HandleNewGamePressed()
        {
            if (sceneTransitionRequested)
            {
                return;
            }

            if (SaveManager.HasSaveFile())
            {
                confirmNewGame = true;
                confirmReset = false;
                showSettings = false;
                feedbackMessage = "\uc0c8 \uac8c\uc784\uc740 \uae30\uc874 \uc9c4\ud589\uc744 \ucd08\uae30\ud654\ud569\ub2c8\ub2e4.";
                return;
            }

            StartNewGame();
        }

        private void StartNewGame()
        {
            if (sceneTransitionRequested)
            {
                return;
            }

            SaveManager.ResetSave();
            GameSession.ClearSelectedStage();
            GameSession.ClearLastBattleResult();
            LoadStageSelect();
        }

        private void LoadStageSelect()
        {
            if (sceneTransitionRequested)
            {
                return;
            }

            sceneTransitionRequested = true;
            SaveManager.Load();
            SceneManager.LoadScene(stageSelectSceneName);
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
