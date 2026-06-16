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
        private string nextStageMessage;
        private string hintMessage;
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
            drawRect.height = Mathf.Max(drawRect.height, 290f);
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

            if (!string.IsNullOrWhiteSpace(nextStageMessage))
            {
                GUILayout.Label(nextStageMessage);
            }

            if (!string.IsNullOrWhiteSpace(resultMessage))
            {
                GUILayout.Label(resultMessage);
            }

            if (!string.IsNullOrWhiteSpace(hintMessage))
            {
                GUILayout.Label(hintMessage);
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
            nextStageMessage = ResolveNextStageMessage(result);
            hintMessage = result.IsVictory ? "Spend Gold on upgrades before harder stages." : BuildDefeatHint(result);
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

            EnsureStageSequence();
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

        private void EnsureStageSequence()
        {
            for (int i = 0; i < stageSequence.Count; i++)
            {
                if (stageSequence[i] != null)
                {
                    return;
                }
            }

            List<StageData> loadedStages = PrototypeAssetLoader.LoadStages();
            if (loadedStages.Count > 0)
            {
                stageSequence = loadedStages;
            }
        }

        private int CalculateGoldAward(BattleResult result)
        {
            int safeGold = Mathf.Max(0, result.GoldEarned);
            return result.IsVictory ? safeGold : Mathf.FloorToInt(safeGold * 0.5f);
        }

        private string ResolveNextStageMessage(BattleResult result)
        {
            if (!result.IsVictory)
            {
                return "Next stage unlock: No";
            }

            string clearedStageId = ResolveStageId(result);
            string nextStageId = ResolveNextStageId(clearedStageId);
            return string.IsNullOrWhiteSpace(nextStageId) ? "Chapter 1 Normal cleared." : $"Next stage unlocked: {nextStageId}";
        }

        private static string BuildDefeatHint(BattleResult result)
        {
            if (result.StageData == null)
            {
                return "Tip: Buy upgrades or choose runes that match the enemy type.";
            }

            string stageId = result.StageData.StageId ?? string.Empty;
            if (stageId.EndsWith("03") || stageId.EndsWith("04"))
            {
                return "Tip: Fast enemies are easier with Seria or Speed/Slow runes.";
            }

            if (stageId.EndsWith("06") || stageId.EndsWith("08"))
            {
                return "Tip: Clustered lanes reward Kael, Brom, Fire, or Turret runes.";
            }

            if (stageId.EndsWith("10"))
            {
                return "Tip: Grumbar needs Nyx, Attack, and Boss Damage runes.";
            }

            return "Tip: If the crystal falls, try Priest, Healing Rune, or Crystal upgrades.";
        }
    }
}
