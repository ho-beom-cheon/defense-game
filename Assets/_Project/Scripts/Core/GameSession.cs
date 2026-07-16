using System;
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
        public static string CurrentBattleRunId { get; private set; } = string.Empty;

        public static void SelectDifficulty(string difficultyId)
        {
            SaveManager.SetSelectedDifficultyId(difficultyId);
            SelectedDifficultyId = SaveManager.Current.selectedDifficultyId;
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

        public static StageData ResolveStageForBattle(StageData fallbackStageData)
        {
            if (SelectedStageData != null)
            {
                return SelectedStageData;
            }

            List<StageData> stages = PrototypeAssetLoader.LoadStages();
            StageData resolvedStage = FindStageById(stages, SaveManager.Current.lastSelectedStageId);
            if (resolvedStage == null)
            {
                resolvedStage = fallbackStageData;
            }

            if (resolvedStage == null)
            {
                resolvedStage = FindFirstUnlockedStage(stages);
            }

            if (resolvedStage == null && stages.Count > 0)
            {
                resolvedStage = stages[0];
            }

            if (resolvedStage != null)
            {
                SelectStage(resolvedStage, ResolveNextStageId(resolvedStage.StageId, stages));
            }

            return resolvedStage;
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
                if (stageData == null || stageData.StageId != currentStageId)
                {
                    continue;
                }

                StageData nextStageData = stages[i + 1];
                return nextStageData != null ? nextStageData.StageId : string.Empty;
            }

            return string.Empty;
        }

        public static void SetLastBattleResult(BattleResult result)
        {
            LastBattleResult = result;
            LastEarnedGold = Mathf.Max(0, result.GoldEarned);
            HasLastBattleResult = true;
        }

        public static void BeginBattleRun()
        {
            CurrentBattleRunId = Guid.NewGuid().ToString("N");
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
            CurrentBattleRunId = string.Empty;
        }

        private static StageData FindStageById(IReadOnlyList<StageData> stages, string stageId)
        {
            if (stages == null || string.IsNullOrWhiteSpace(stageId))
            {
                return null;
            }

            for (int i = 0; i < stages.Count; i++)
            {
                StageData stageData = stages[i];
                if (stageData != null && stageData.StageId == stageId)
                {
                    return stageData;
                }
            }

            return null;
        }

        private static StageData FindFirstUnlockedStage(IReadOnlyList<StageData> stages)
        {
            if (stages == null)
            {
                return null;
            }

            for (int i = 0; i < stages.Count; i++)
            {
                StageData stageData = stages[i];
                if (stageData != null && SaveManager.IsStageUnlocked(stageData.StageId))
                {
                    return stageData;
                }
            }

            return null;
        }
    }
}
