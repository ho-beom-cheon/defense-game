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

            KoreanFontManager.ApplyToGuiSkin();
            Rect drawRect = panelRect;
            drawRect.height = Mathf.Max(drawRect.height, 290f);
            GUILayout.BeginArea(drawRect, GUI.skin.box);
            GUILayout.Label(resultTitle);
            GUILayout.Label($"획득 골드: {goldEarned}");
            if (battleGoldEarned != goldEarned)
            {
                GUILayout.Label($"전투 골드: {battleGoldEarned}");
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

            if (GUILayout.Button("재시도", GUILayout.Height(34f)))
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

            if (GUILayout.Button("업그레이드", GUILayout.Height(30f)))
            {
                SceneManager.LoadScene(upgradeSceneName);
            }

            if (GUILayout.Button("스테이지 선택", GUILayout.Height(30f)))
            {
                SceneManager.LoadScene(stageSelectSceneName);
            }

            GUILayout.EndArea();
        }

        private void ShowResult(BattleResult result)
        {
            isVisible = true;
            resultTitle = result.IsVictory ? "승리" : "패배";
            battleGoldEarned = result.GoldEarned;
            goldEarned = CalculateGoldAward(result);
            ApplyResultToSave(result);
            stageStatusMessage = result.IsVictory ? "스테이지 클리어: 예" : "스테이지 클리어: 아니오";
            nextStageMessage = ResolveNextStageMessage(result);
            hintMessage = result.IsVictory ? "어려운 스테이지 전에 골드로 업그레이드하세요." : BuildDefeatHint(result);
            resultMessage = $"클리어 웨이브: {result.WavesCleared}";
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
                return "다음 스테이지 해금: 아니오";
            }

            string clearedStageId = ResolveStageId(result);
            string nextStageId = ResolveNextStageId(clearedStageId);
            return string.IsNullOrWhiteSpace(nextStageId) ? "챕터 1 보통 난이도 클리어." : $"다음 스테이지 해금: {nextStageId}";
        }

        private static string BuildDefeatHint(BattleResult result)
        {
            if (result.StageData == null)
            {
                return "팁: 업그레이드를 구매하거나 적 유형에 맞는 룬을 고르세요.";
            }

            string stageId = result.StageData.StageId ?? string.Empty;
            if (stageId.EndsWith("03") || stageId.EndsWith("04"))
            {
                return "팁: 빠른 적은 세리아 또는 속도/냉기 룬으로 대응하세요.";
            }

            if (stageId.EndsWith("06") || stageId.EndsWith("08"))
            {
                return "팁: 몰려오는 라인은 카엘, 브롬, 화염 룬, 포탑 룬이 좋습니다.";
            }

            if (stageId.EndsWith("10"))
            {
                return "팁: 그룸바르는 닉스, 공격 룬, 보스 사냥 룬으로 상대하세요.";
            }

            return "팁: 크리스탈이 무너지면 미레아, 치유 룬, 크리스탈 업그레이드를 써보세요.";
        }
    }
}
