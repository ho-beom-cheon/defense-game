using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace RuneGate
{
    public sealed class StageResultUI : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private List<StageData> stageSequence = new List<StageData>();
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private Rect panelRect = new Rect(300f, 170f, 410f, 220f);
        [SerializeField] private string battleSceneName = "BattleScene";
        [SerializeField] private string upgradeSceneName = "UpgradeScene";
        [SerializeField] private string stageSelectSceneName = "StageSelectScene";

        private bool isVisible;
        private bool saveApplied;
        private string resultTitle;
        private string resultMessage;
        private string stageStatusMessage;
        private int battleGoldEarned;
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
            GUILayout.Label($"Gold Awarded: {goldEarned}");
            if (battleGoldEarned != goldEarned)
            {
                GUILayout.Label($"Battle Gold: {battleGoldEarned}");
            }

            if (!string.IsNullOrWhiteSpace(stageStatusMessage))
            {
                GUILayout.Label(stageStatusMessage);
            }

            if (!string.IsNullOrWhiteSpace(resultMessage))
            {
                GUILayout.Label(resultMessage);
            }

            if (GUILayout.Button("Retry", GUILayout.Height(34f)))
            {
                if (battleManager != null)
                {
                    battleManager.RestartBattle();
                }
                else
                {
                    SceneManager.LoadScene(battleSceneName);
                }
            }

            if (GUILayout.Button("Upgrade", GUILayout.Height(30f)))
            {
                SceneManager.LoadScene(upgradeSceneName);
            }

            if (GUILayout.Button("Stage Select", GUILayout.Height(30f)))
            {
                SceneManager.LoadScene(stageSelectSceneName);
            }

            GUILayout.EndArea();
        }

        private void ShowResult(BattleResult result)
        {
            isVisible = true;
            resultTitle = result.IsVictory ? "Victory" : "Defeat";
            battleGoldEarned = result.GoldEarned;
            goldEarned = CalculateGoldAward(result);
            ApplyResultToSave(result);
            stageStatusMessage = result.IsVictory ? "Stage Cleared: Yes" : "Stage Cleared: No";
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
                saveApplied = false;
            }
        }

        private void ApplyResultToSave(BattleResult result)
        {
            GameSession.SetLastBattleResult(result);
            if (saveApplied)
            {
                return;
            }

            SaveManager.AddGold(CalculateGoldAward(result));

            if (result.IsVictory)
            {
                string clearedStageId = ResolveStageId(result);
                SaveManager.MarkStageCleared(clearedStageId);

                string nextStageId = ResolveNextStageId(clearedStageId);
                if (!string.IsNullOrWhiteSpace(nextStageId))
                {
                    SaveManager.UnlockStage(nextStageId);
                }
            }

            saveApplied = true;
        }

        private string ResolveStageId(BattleResult result)
        {
            if (!string.IsNullOrWhiteSpace(GameSession.SelectedStageId))
            {
                return GameSession.SelectedStageId;
            }

            return result.StageData != null ? result.StageData.StageId : string.Empty;
        }

        private string ResolveNextStageId(string currentStageId)
        {
            if (!string.IsNullOrWhiteSpace(GameSession.SelectedNextStageId))
            {
                return GameSession.SelectedNextStageId;
            }

            for (int i = 0; i < stageSequence.Count - 1; i++)
            {
                StageData stageData = stageSequence[i];
                if (stageData != null && stageData.StageId == currentStageId)
                {
                    StageData nextStageData = stageSequence[i + 1];
                    return nextStageData != null ? nextStageData.StageId : string.Empty;
                }
            }

            return string.Empty;
        }

        private int CalculateGoldAward(BattleResult result)
        {
            int safeGold = Mathf.Max(0, result.GoldEarned);
            return result.IsVictory ? safeGold : Mathf.FloorToInt(safeGold * 0.5f);
        }
    }
}
