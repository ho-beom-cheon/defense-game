using System;
using System.Text;
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
        [SerializeField] private Rect panelRect = new Rect(310f, 90f, 520f, 410f);
        [SerializeField] private string battleSceneName = "BattleScene";
        [SerializeField] private string upgradeSceneName = "UpgradeScene";
        [SerializeField] private string stageSelectSceneName = "StageSelectScene";

        private bool isVisible;
        private bool saveApplied;
        private BattleResult latestResult;
        private int battleGoldEarned;
        private int goldEarned;
        private string newlyUnlockedDifficultyId = string.Empty;
        private Vector2 resultScrollPosition;
        private bool sceneTransitionRequested;

        public event Action<bool> VisibilityChanged;
        public event Action<BattleResultViewData> ViewDataChanged;

        public bool IsVisible => isVisible;
        public string ResultMessage { get; private set; }
        public string NewlyUnlockedDifficultyId => newlyUnlockedDifficultyId;
        public BattleResultViewData CurrentViewData { get; private set; }

        public void SetRuntimeGuiEnabled(bool enabled)
        {
            drawRuntimeGui = enabled;
        }

        private void OnEnable()
        {
            BindBattleManager();
        }

        private void OnDisable()
        {
            UnbindBattleManager();
        }

        private void BindBattleManager()
        {
            if (battleManager == null)
            {
                battleManager = FindAnyObjectByType<BattleManager>();
            }

            if (battleManager != null)
            {
                battleManager.BattleEnded -= ShowResult;
                battleManager.BattleStateChanged -= HandleBattleStateChanged;
                battleManager.BattleEnded += ShowResult;
                battleManager.BattleStateChanged += HandleBattleStateChanged;
            }
        }

        private void UnbindBattleManager()
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

            UIResponsiveLayout.ApplyReadableDefaults();
            UIPopupGuiUtility.DrawDimOverlay();

            Rect drawRect = CenteredPanelRect();
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUI.Box(drawRect, GUIContent.none, panelStyle);
            float horizontalPadding = Application.isMobilePlatform ? 18f : 14f;
            float verticalPadding = Application.isMobilePlatform ? 16f : 12f;
            Rect contentRect = new Rect(
                drawRect.x + horizontalPadding,
                drawRect.y + verticalPadding,
                Mathf.Max(1f, drawRect.width - horizontalPadding * 2f),
                Mathf.Max(1f, drawRect.height - verticalPadding * 2f));
            GUILayout.BeginArea(contentRect);
            GUI.SetNextControlName("PopupLayer_ResultPopup");
            GUILayout.Label(latestResult.IsVictory ? "\uc2b9\ub9ac!" : "\ud328\ubc30");
            GUILayout.Space(UIResponsiveLayout.SmallGap);
            float primaryButtonHeight = UIResponsiveLayout.TouchHeight(38f);
            float secondaryButtonHeight = UIResponsiveLayout.TouchHeight(34f);
            float reservedActionHeight = primaryButtonHeight + secondaryButtonHeight + 70f;
            resultScrollPosition = GUILayout.BeginScrollView(resultScrollPosition, GUILayout.Height(Mathf.Max(170f, contentRect.height - reservedActionHeight)));
            GUILayout.Label(ResultMessage);
            GUILayout.Label($"\ud68d\ub4dd \uace8\ub4dc +{goldEarned}");
            if (battleGoldEarned != goldEarned)
            {
                GUILayout.Label($"\uc804\ud22c \uace8\ub4dc: {battleGoldEarned}");
            }

            GUILayout.Label($"\ud074\ub9ac\uc5b4 \uc2dc\uac04 {FormatElapsedTime(latestResult.ElapsedSeconds)}");
            GUILayout.Label($"\ud06c\ub9ac\uc2a4\ud0c8 HP {latestResult.CrystalHp}/{latestResult.CrystalMaxHp}");
            GUILayout.Label($"\ucc98\uce58 \uc801 {latestResult.MonstersKilled}");
            GUILayout.Label($"\ud074\ub9ac\uc5b4 \uc6e8\uc774\ube0c {latestResult.WavesCleared}/{GetTotalWaves(latestResult)}");
            GUILayout.Label($"\ub09c\uc774\ub3c4 {GameTextMapper.Difficulty(DifficultyRules.CurrentDifficultyId)} / \ubcf4\uc0c1 x{DifficultyRules.RewardMultiplier(DifficultyRules.CurrentDifficultyId):0.##}");
            if (!string.IsNullOrWhiteSpace(newlyUnlockedDifficultyId))
            {
                GUILayout.Label($"새 난이도 해금: {GameTextMapper.Difficulty(newlyUnlockedDifficultyId)}");
            }

            DrawShardRewards();
            if (latestResult.IsVictory)
            {
                GUILayout.Label($"\uc2a4\ud14c\uc774\uc9c0 \ud074\ub9ac\uc5b4: {GameTextMapper.StageName(latestResult.StageData)}");
                GUILayout.Label(ResolveNextStageMessage(latestResult));
            }
            else
            {
                GUILayout.Label("\ud06c\ub9ac\uc2a4\ud0c8\uc774 \ud30c\uad34\ub418\uc5c8\uc2b5\ub2c8\ub2e4.");
                GUILayout.Label(BuildDefeatHint(latestResult));
            }

            GUILayout.EndScrollView();
            GUILayout.Space(UIResponsiveLayout.SmallGap);
            bool previousEnabled = GUI.enabled;
            GUI.enabled = !sceneTransitionRequested;
            if (latestResult.IsVictory && HasNextStage(latestResult))
            {
                if (GUILayout.Button("\ub2e4\uc74c \uc2a4\ud14c\uc774\uc9c0", GUILayout.Height(primaryButtonHeight)))
                {
                    ContinueToNextStage();
                }
            }
            else if (GUILayout.Button("\uc7ac\uc2dc\ub3c4", GUILayout.Height(primaryButtonHeight)))
            {
                RetryBattle();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("\uc5c5\uadf8\ub808\uc774\ub4dc", GUILayout.Height(secondaryButtonHeight)))
            {
                OpenUpgrade();
            }

            if (GUILayout.Button("\uc2a4\ud14c\uc774\uc9c0 \uc120\ud0dd", GUILayout.Height(secondaryButtonHeight)))
            {
                OpenStageSelect();
            }

            GUILayout.EndHorizontal();
            GUI.enabled = previousEnabled;
            GUILayout.EndArea();
        }

        public void ContinueToNextStage()
        {
            if (!isVisible || !latestResult.IsVictory)
            {
                return;
            }

            LoadNextStage(latestResult);
        }

        public void RetryBattle()
        {
            if (!isVisible)
            {
                return;
            }

            RestartBattle();
        }

        public void OpenUpgrade()
        {
            if (!isVisible)
            {
                return;
            }

            LoadSceneOnce(upgradeSceneName);
        }

        public void OpenStageSelect()
        {
            if (!isVisible)
            {
                return;
            }

            LoadSceneOnce(stageSelectSceneName);
        }

        private void ShowResult(BattleResult result)
        {
            latestResult = result;
            isVisible = true;
            sceneTransitionRequested = false;
            battleGoldEarned = result.GoldEarned;
            goldEarned = CalculateGoldAward(result);
            newlyUnlockedDifficultyId = string.Empty;
            bool progressApplied = ApplyResultToSave(result);
            if (!progressApplied && SaveManager.HasProcessedBattleRun(result.BattleRunId))
            {
                goldEarned = 0;
                ResultMessage = "\uc774\ubbf8 \uc800\uc7a5\ub41c \uc804\ud22c \uacb0\uacfc\uc785\ub2c8\ub2e4.";
            }
            else
            {
                ResultMessage = result.IsVictory ? "\ubd09\ubb38 \uae30\ub85d \uac31\uc2e0. \ud06c\ub9ac\uc2a4\ud0c8 \ubc29\uc5b4 \uc131\uacf5!" : "\ubc29\uc5b4\uc120 \ubd95\uad34. \ub2e4\uc2dc \uc804\uc5f4\uc744 \uc815\ube44\ud558\uc138\uc694.";
            }

            CurrentViewData = BuildViewData(result);
            ViewDataChanged?.Invoke(CurrentViewData);
            VisibilityChanged?.Invoke(true);
        }

        private void HandleBattleStateChanged(BattleState state)
        {
            if (state == BattleState.Preparing || state == BattleState.WaveRunning || state == BattleState.RuneSelection)
            {
                bool wasVisible = isVisible;
                isVisible = false;
                saveApplied = false;
                sceneTransitionRequested = false;
                if (wasVisible)
                {
                    VisibilityChanged?.Invoke(false);
                }
            }
        }

        private BattleResultViewData BuildViewData(BattleResult result)
        {
            bool hasNextStage = HasNextStage(result);
            string unlock = result.IsVictory ? ResolveNextStageMessage(result) : BuildDefeatHint(result);
            if (!string.IsNullOrWhiteSpace(newlyUnlockedDifficultyId))
            {
                unlock += $"\n새 난이도 해금: {GameTextMapper.Difficulty(newlyUnlockedDifficultyId)}";
            }

            return new BattleResultViewData(
                result.IsVictory,
                result.IsVictory ? "봉문 성공" : "방어선 붕괴",
                ResultMessage,
                goldEarned,
                battleGoldEarned,
                FormatElapsedTime(result.ElapsedSeconds),
                $"{result.CrystalHp}/{result.CrystalMaxHp}",
                result.MonstersKilled.ToString(),
                $"{result.WavesCleared}/{GetTotalWaves(result)}",
                $"{GameTextMapper.Difficulty(DifficultyRules.CurrentDifficultyId)} · 보상 x{DifficultyRules.RewardMultiplier(DifficultyRules.CurrentDifficultyId):0.##}",
                BuildShardRewardsText(),
                unlock,
                result.IsVictory && hasNextStage ? "다음 스테이지" : "재시도",
                hasNextStage);
        }

        private static string BuildShardRewardsText()
        {
            IReadOnlyDictionary<string, int> drops = ShadowContractService.LastBattleDrops;
            if (drops == null || drops.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, int> pair in drops)
            {
                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                string monsterName = ShadowContractService.GetMonsterDisplayName(pair.Key);
                builder.Append(monsterName).Append(" 조각 +").Append(pair.Value);
                if (ShadowContractService.CanContract(pair.Key))
                {
                    builder.Append(" · 계약 가능");
                }
            }

            return builder.ToString();
        }

        private bool ApplyResultToSave(BattleResult result)
        {
            GameSession.SetLastBattleResult(result);
            if (saveApplied)
            {
                return false;
            }

            string clearedStageId = result.IsVictory ? ResolveStageId(result) : string.Empty;
            string nextStageId = result.IsVictory ? ResolveNextStageId(clearedStageId) : string.Empty;
            string nextLockedDifficultyId = DifficultyRules.NextLockedDifficultyId(SaveManager.Current);
            bool applied;
            if (result.IsVictory)
            {
                applied = SaveManager.TryApplyBattleResultProgression(result.BattleRunId, CalculateGoldAward(result), true, clearedStageId, nextStageId);
            }
            else
            {
                applied = SaveManager.TryApplyBattleResultProgression(result.BattleRunId, CalculateGoldAward(result), false, string.Empty, string.Empty);
            }

            if (applied && result.IsVictory && !string.IsNullOrWhiteSpace(nextLockedDifficultyId) &&
                SaveManager.IsDifficultyUnlocked(nextLockedDifficultyId))
            {
                newlyUnlockedDifficultyId = nextLockedDifficultyId;
            }

            saveApplied = true;
            return applied;
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
            return GameSession.ResolveNextStageId(currentStageId, stageSequence);
        }

        private StageData ResolveStageData(string stageId)
        {
            EnsureStageSequence();
            for (int i = 0; i < stageSequence.Count; i++)
            {
                StageData stageData = stageSequence[i];
                if (stageData != null && stageData.StageId == stageId)
                {
                    return stageData;
                }
            }

            return null;
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
            if (result.IsVictory)
            {
                safeGold = Mathf.RoundToInt(safeGold * ShadowContractService.GetGoldRewardMultiplier());
                int minimumGold = DifficultyRules.ApplyMonsterRewardGold(ResolveEarlyStageMinimumGold(result));
                safeGold = Mathf.Max(safeGold, minimumGold);
            }

            return result.IsVictory ? safeGold : Mathf.FloorToInt(safeGold * 0.5f);
        }

        private static int ResolveEarlyStageMinimumGold(BattleResult result)
        {
            string stageId = result.StageData != null ? result.StageData.StageId : string.Empty;
            if (stageId.EndsWith("01"))
            {
                return 110;
            }

            if (stageId.EndsWith("02"))
            {
                return 140;
            }

            if (stageId.EndsWith("03"))
            {
                return 170;
            }

            return 0;
        }

        private static void DrawShardRewards()
        {
            IReadOnlyDictionary<string, int> drops = ShadowContractService.LastBattleDrops;
            if (drops == null || drops.Count == 0)
            {
                return;
            }

            GUILayout.Space(6f);
            GUILayout.Label("\uadf8\ub9bc\uc790 \uc870\uac01 \ud68d\ub4dd");
            foreach (KeyValuePair<string, int> pair in drops)
            {
                string monsterName = ShadowContractService.GetMonsterDisplayName(pair.Key);
                GUILayout.Label($"{monsterName} \uc870\uac01 +{pair.Value}");
                if (ShadowContractService.CanContract(pair.Key))
                {
                    GUILayout.Label($"{monsterName} \uadf8\ub9bc\uc790 \uacc4\uc57d \uac00\ub2a5");
                }
            }
        }

        private string ResolveNextStageMessage(BattleResult result)
        {
            string nextStageId = ResolveNextStageId(ResolveStageId(result));
            if (string.IsNullOrWhiteSpace(nextStageId))
            {
                return $"{GameTextMapper.Difficulty(DifficultyRules.CurrentDifficultyId)} Chapter 1 클리어";
            }

            StageData nextStage = ResolveStageData(nextStageId);
            string nextStageName = nextStage != null ? GameTextMapper.StageName(nextStage) : GameTextMapper.StageName(nextStageId);
            return $"\ub2e4\uc74c \uc2a4\ud14c\uc774\uc9c0 \ud574\uae08: {nextStageName}";
        }

        private bool HasNextStage(BattleResult result)
        {
            return result.IsVictory && !string.IsNullOrWhiteSpace(ResolveNextStageId(ResolveStageId(result)));
        }

        private void LoadNextStage(BattleResult result)
        {
            if (sceneTransitionRequested)
            {
                return;
            }

            sceneTransitionRequested = true;
            string nextStageId = ResolveNextStageId(ResolveStageId(result));
            StageData nextStage = ResolveStageData(nextStageId);
            if (nextStage == null)
            {
                SceneManager.LoadScene(stageSelectSceneName);
                return;
            }

            GameSession.SelectStage(nextStage, ResolveNextStageId(nextStage.StageId));
            SceneManager.LoadScene(battleSceneName);
        }

        private void RestartBattle()
        {
            if (sceneTransitionRequested)
            {
                return;
            }

            sceneTransitionRequested = true;
            if (battleManager != null)
            {
                battleManager.RestartBattle();
            }
            else
            {
                SceneManager.LoadScene(battleSceneName);
            }
        }

        private void LoadSceneOnce(string sceneName)
        {
            if (sceneTransitionRequested)
            {
                return;
            }

            sceneTransitionRequested = true;
            SceneManager.LoadScene(sceneName);
        }

        private static int GetTotalWaves(BattleResult result)
        {
            return result.StageData != null && result.StageData.Waves != null ? Mathf.Max(1, result.StageData.Waves.Count) : Mathf.Max(1, result.WavesCleared);
        }

        private static string BuildDefeatHint(BattleResult result)
        {
            string stageId = result.StageData != null ? result.StageData.StageId : string.Empty;
            if (stageId.EndsWith("03") || stageId.EndsWith("04"))
            {
                return "\ud78c\ud2b8: \ube60\ub978 \uc801\uc774 \ub9ce\uc2b5\ub2c8\ub2e4. \uc138\ub9ac\uc544\uc758 \uc5f0\uc18d \uc0ac\uaca9\uacfc \ub2c9\uc2a4\uc758 \uadf8\ub9bc\uc790 \uae09\uc2b5\uc744 \ud65c\uc6a9\ud558\uc138\uc694.";
            }

            if (stageId.EndsWith("02"))
            {
                return "\ud78c\ud2b8: \uc7ac\uac11 \ub3cc\uaca9\ubcd1\uc740 \ub290\ub9ac\uc9c0\ub9cc \ubc84\ud2f0\ub294 \uc801\uc785\ub2c8\ub2e4. \ub808\uc628\uc744 \uc804\uc5f4\uc5d0 \ub450\uace0 \uc6d0\uac70\ub9ac \uc601\uc6c5\uc73c\ub85c \ubcf4\uc870\ud558\uc138\uc694.";
            }

            if (stageId.EndsWith("01"))
            {
                return "\ud78c\ud2b8: \ub808\uc628\uc774 \uc55e\uc5d0\uc11c \ub9c9\uace0 \uc138\ub9ac\uc544\uac00 \ub4a4\uc5d0\uc11c \uc9c0\uc6d0\ud558\ub294 \uae30\ubcf8 \ud750\ub984\uc744 \uc720\uc9c0\ud558\uc138\uc694.";
            }

            if (stageId.EndsWith("06") || stageId.EndsWith("08"))
            {
                return "\ud78c\ud2b8: \ubab0\ub824\uc624\ub294 \ub77c\uc778\uc740 \uce74\uc5d8, \ube0c\ub86c, \uad11\uc5ed \ub8ec\uc73c\ub85c \uc815\ub9ac\ud558\uc138\uc694.";
            }

            if (stageId.EndsWith("10"))
            {
                return "\ud78c\ud2b8: \uadf8\ub8f8\ubc14\ub974 \uc804\ud22c\ub294 \ubcf4\uc2a4 \uc0ac\ub0e5 \ub8ec\uc73c\ub85c \uc9d1\uc911 \uacf5\uaca9\ud558\uc138\uc694.";
            }

            return "\ud78c\ud2b8: \ud06c\ub9ac\uc2a4\ud0c8 \uac15\ud654\uc640 \uc601\uc6c5 \ud6c8\ub828\uc744 \uba3c\uc800 \uad6c\ub9e4\ud574\ubcf4\uc138\uc694.";
        }

        private static string FormatElapsedTime(float elapsedSeconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.RoundToInt(elapsedSeconds));
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes:0}:{seconds:00}";
        }

        private Rect CenteredPanelRect()
        {
            bool mobilePortrait = Application.isMobilePlatform && GameFrameLayout.IsPortrait;
            float preferredWidth = mobilePortrait ? 760f : 620f;
            float preferredHeight = mobilePortrait ? 800f : 560f;
            return GameFrameLayout.PopupFrame(Mathf.Max(panelRect.width, preferredWidth), Mathf.Max(panelRect.height, preferredHeight), 0.92f, 0.78f);
        }
    }
}
