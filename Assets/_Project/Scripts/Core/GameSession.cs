using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public static class GameSession
    {
        public static StageData SelectedStageData { get; private set; }
        public static string SelectedStageId { get; private set; } = string.Empty;
        public static string SelectedNextStageId { get; private set; } = string.Empty;
        public static string SelectedDifficultyId { get; private set; } = "normal";
        public static BattleResult LastBattleResult { get; private set; }
        public static bool HasLastBattleResult { get; private set; }
        public static int LastEarnedGold { get; private set; }

        public static void SelectDifficulty(string difficultyId)
        {
            SelectedDifficultyId = string.IsNullOrWhiteSpace(difficultyId) ? "normal" : difficultyId;
            SaveManager.SetSelectedDifficultyId(SelectedDifficultyId);
        }

        public static void SelectStage(StageData stageData, string nextStageId)
        {
            if (stageData == null)
            {
                Debug.LogWarning("GameSession cannot select a missing StageData.");
                return;
            }

            SelectedStageData = stageData;
            SelectedStageId = stageData.StageId;
            SelectedNextStageId = nextStageId ?? string.Empty;
            SelectedDifficultyId = SaveManager.Current.selectedDifficultyId;
            SaveManager.SetLastSelectedStageId(SelectedStageId);
            ClearLastBattleResult();
        }

        public static string ResolveNextStageId(string currentStageId)
        {
            return ResolveNextStageId(currentStageId, PrototypeAssetLoader.LoadStages());
        }

        public static string ResolveNextStageId(string currentStageId, IReadOnlyList<StageData> stages)
        {
            if (string.IsNullOrWhiteSpace(currentStageId) || stages == null)
            {
                return string.Empty;
            }

            for (int i = 0; i < stages.Count - 1; i++)
            {
                StageData stageData = stages[i];
                if (stageData != null && stageData.StageId == currentStageId)
                {
                    StageData nextStageData = stages[i + 1];
                    return nextStageData != null ? nextStageData.StageId : string.Empty;
                }
            }

            return string.Empty;
        }

        public static void SetLastBattleResult(BattleResult result)
        {
            LastBattleResult = result;
            LastEarnedGold = Mathf.Max(0, result.GoldEarned);
            HasLastBattleResult = true;
        }

        public static void ClearLastBattleResult()
        {
            LastBattleResult = default;
            LastEarnedGold = 0;
            HasLastBattleResult = false;
        }

        public static void ClearSelectedStage()
        {
            SelectedStageData = null;
            SelectedStageId = string.Empty;
            SelectedNextStageId = string.Empty;
        }
    }
}
