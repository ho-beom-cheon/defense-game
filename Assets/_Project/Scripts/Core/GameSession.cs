using UnityEngine;

namespace RuneGate
{
    public static class GameSession
    {
        public static StageData SelectedStageData { get; private set; }
        public static string SelectedStageId { get; private set; } = string.Empty;
        public static string SelectedNextStageId { get; private set; } = string.Empty;
        public static BattleResult LastBattleResult { get; private set; }
        public static bool HasLastBattleResult { get; private set; }
        public static int LastEarnedGold { get; private set; }

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
            SaveManager.SetLastSelectedStageId(SelectedStageId);
            ClearLastBattleResult();
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
