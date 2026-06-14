using UnityEngine;

namespace RuneGate
{
    public sealed class StageResultUI : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private Rect panelRect = new Rect(300f, 170f, 410f, 140f);

        private bool isVisible;
        private string resultTitle;
        private string resultMessage;
        private int goldEarned;

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
                battleManager.BattleStateChanged += HandleBattleStateChanged;
            }
        }

        private void OnDisable()
        {
            if (battleManager != null)
            {
                battleManager.BattleEnded -= ShowResult;
                battleManager.BattleStateChanged -= HandleBattleStateChanged;
            }
        }

        private void OnGUI()
        {
            if (!drawRuntimeGui || !isVisible)
            {
                return;
            }

            Rect drawRect = panelRect;
            drawRect.height = Mathf.Max(drawRect.height, 190f);
            GUILayout.BeginArea(drawRect, GUI.skin.box);
            GUILayout.Label(resultTitle);
            GUILayout.Label($"Gold Earned: {goldEarned}");
            if (!string.IsNullOrWhiteSpace(resultMessage))
            {
                GUILayout.Label(resultMessage);
            }

            if (GUILayout.Button("Restart", GUILayout.Height(34f)))
            {
                battleManager?.RestartBattle();
            }

            if (GUILayout.Button("Back - Placeholder", GUILayout.Height(28f)))
            {
                Debug.Log("Back to Title is a placeholder in Battle Prototype v0.2.");
            }

            GUILayout.EndArea();
        }

        private void ShowResult(BattleResult result)
        {
            isVisible = true;
            resultTitle = result.IsVictory ? "Victory" : "Defeat";
            goldEarned = result.GoldEarned;
            resultMessage = $"Waves Cleared: {result.WavesCleared}";
            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                resultMessage = $"{resultMessage}\n{result.Message}";
            }
        }

        private void HandleBattleStateChanged(BattleState state)
        {
            if (state == BattleState.Preparing || state == BattleState.WaveRunning || state == BattleState.RuneSelection)
            {
                isVisible = false;
            }
        }
    }
}
