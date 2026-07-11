using UnityEngine;

namespace RuneGate
{
    public sealed class TutorialOverlayUI : MonoBehaviour
    {
        [SerializeField] private TutorialManager tutorialManager;
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private Rect panelRect = new Rect(270f, 90f, 500f, 260f);

        private void OnEnable()
        {
            if (tutorialManager == null)
            {
                tutorialManager = FindAnyObjectByType<TutorialManager>();
            }
        }

        private void OnGUI()
        {
            if (!drawRuntimeGui || tutorialManager == null || !tutorialManager.IsVisible)
            {
                return;
            }

            UIResponsiveLayout.ApplyReadableDefaults();
            TutorialStepData step = tutorialManager.CurrentStep;
            if (step == null)
            {
                return;
            }

            Rect drawRect = UIResponsiveLayout.Centered(Mathf.Max(panelRect.width, 640f), Mathf.Max(panelRect.height, 340f), 0.90f, 0.36f);
            drawRect.y = Mathf.Min(drawRect.y + Screen.height * 0.20f, Screen.height - drawRect.height - UIResponsiveLayout.Margin);

            GUILayout.BeginArea(drawRect, GUI.skin.box);
            GUILayout.BeginHorizontal();
            RuntimePixelGuiUtility.DrawIcon(RuntimePixelAssetLoader.UiTutorialArrow, 30f);
            GUILayout.Label($"\ud29c\ud1a0\ub9ac\uc5bc {tutorialManager.CurrentStepNumber}/{tutorialManager.StepCount}");
            RuntimePixelGuiUtility.DrawIcon(RuntimePixelAssetLoader.UiTapIndicator, 30f);
            GUILayout.EndHorizontal();
            GUILayout.Space(UIResponsiveLayout.SmallGap);
            GUILayout.Label(step.Title);
            GUILayout.Space(UIResponsiveLayout.SmallGap);
            GUILayout.Label(step.Body);
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("\uac74\ub108\ub6f0\uae30", GUILayout.Height(34f)))
            {
                tutorialManager.Skip();
            }

            string nextLabel = tutorialManager.CurrentStepNumber >= tutorialManager.StepCount ? "\uc644\ub8cc" : "\ub2e4\uc74c";
            if (GUILayout.Button(nextLabel, GUILayout.Height(34f)))
            {
                tutorialManager.Next();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
