using UnityEngine;

namespace RuneGate
{
    public sealed class StageResultUI : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private Rect panelRect = new Rect(260f, 170f, 360f, 120f);

        private bool isVisible;
        private string resultMessage;

        public bool IsVisible => isVisible;
        public string ResultMessage => resultMessage;

        private void OnEnable()
        {
            if (battleManager == null)
            {
                battleManager = FindFirstObjectByType<BattleManager>();
            }

            if (battleManager != null)
            {
                battleManager.BattleEnded += ShowResult;
            }
        }

        private void OnDisable()
        {
            if (battleManager != null)
            {
                battleManager.BattleEnded -= ShowResult;
            }
        }

        private void ShowResult(BattleResult result)
        {
            isVisible = true;
            resultMessage = result.IsVictory ? "Victory" : "Defeat";
            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                resultMessage = $"{resultMessage}: {result.Message}";
            }
        }

        private void OnGUI()
        {
            if (!drawRuntimeGui || !isVisible)
            {
                return;
            }

            GUILayout.BeginArea(panelRect, GUI.skin.box);
            GUILayout.Label(resultMessage);
            GUILayout.Label("Exit Play Mode and run the bootstrapper again to reset generated sample data.");
            GUILayout.EndArea();
        }
    }
}
