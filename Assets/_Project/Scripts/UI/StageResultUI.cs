using UnityEngine;

namespace RuneGate
{
    public sealed class StageResultUI : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private Rect panelRect = new Rect(300f, 170f, 410f, 140f);

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

        private void OnGUI()
        {
            if (!drawRuntimeGui || !isVisible)
            {
                return;
            }

            GUILayout.BeginArea(panelRect, GUI.skin.box);
            GUILayout.Label(resultMessage);
            GUILayout.Label("Stop Play Mode and run the bootstrapper again to reset generated sample data.");
            GUILayout.EndArea();
        }

        private void ShowResult(BattleResult result)
        {
            isVisible = true;
            resultMessage = result.IsVictory ? "Victory" : "Defeat";
            resultMessage = $"{resultMessage} | Waves {result.WavesCleared} | Gold {result.GoldEarned}";
            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                resultMessage = $"{resultMessage}\n{result.Message}";
            }
        }
    }
}
