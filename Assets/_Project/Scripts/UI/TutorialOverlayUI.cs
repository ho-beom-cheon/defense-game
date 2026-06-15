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

            TutorialStepData step = tutorialManager.CurrentStep;
            if (step == null)
            {
                return;
            }

            GUILayout.BeginArea(panelRect, GUI.skin.box);
            GUILayout.Label($"Tutorial {tutorialManager.CurrentStepNumber}/{tutorialManager.StepCount}");
            GUILayout.Space(6f);
            GUILayout.Label(step.Title);
            GUILayout.Space(8f);
            GUILayout.Label(step.Body);
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Skip", GUILayout.Height(34f)))
            {
                tutorialManager.Skip();
            }

            string nextLabel = tutorialManager.CurrentStepNumber >= tutorialManager.StepCount ? "Finish" : "Next";
            if (GUILayout.Button(nextLabel, GUILayout.Height(34f)))
            {
                tutorialManager.Next();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
