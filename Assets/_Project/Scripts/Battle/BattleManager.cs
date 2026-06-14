using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class BattleManager : MonoBehaviour
    {
        [SerializeField] private StageData initialStageData;
        [SerializeField] private LaneManager laneManager;
        [SerializeField] private CrystalController crystalController;
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private RuneManager runeManager;
        [SerializeField] private RuneEffectApplier runeEffectApplier;
        [SerializeField] private List<HeroController> heroes = new List<HeroController>();
        [SerializeField] private bool autoStartOnStart = true;

        private StageData activeStageData;
        private BattleState currentState = BattleState.None;
        private int currentWaveIndex = -1;
        private int goldEarned;
        private bool initialized;

        public event Action<BattleState> BattleStateChanged;
        public event Action<int, int> WaveChanged;
        public event Action<int> GoldChanged;
        public event Action<IReadOnlyList<RuneData>> RuneOptionsOffered;
        public event Action<RuneData> RuneSelected;
        public event Action<BattleResult> BattleEnded;

        public BattleState CurrentState => currentState;
        public int CurrentWaveNumber => currentWaveIndex + 1;
        public int GoldEarned => goldEarned;
        public StageData ActiveStageData => activeStageData;
        public IReadOnlyList<HeroController> Heroes => heroes;

        private void Awake()
        {
            AutoAssignReferences();
            BindEvents();
        }

        private void Start()
        {
            if (autoStartOnStart && initialStageData != null)
            {
                InitializeStage(initialStageData);
                StartNextWave();
            }
        }

        private void OnDestroy()
        {
            if (crystalController != null)
            {
                crystalController.Destroyed -= HandleCrystalDestroyed;
            }

            if (waveManager != null)
            {
                waveManager.WaveCompleted -= HandleWaveCompleted;
                waveManager.MonsterKilled -= HandleMonsterKilled;
            }

            if (runeManager != null)
            {
                runeManager.RuneOptionsGenerated -= HandleRuneOptionsGenerated;
                runeManager.RuneSelected -= HandleRuneSelected;
            }
        }

        public void InitializeStage(StageData stageData)
        {
            if (stageData == null)
            {
                Debug.LogWarning("BattleManager cannot initialize because StageData is missing.");
                return;
            }

            AutoAssignReferences();
            BindEvents();

            activeStageData = stageData;
            currentWaveIndex = -1;
            goldEarned = 0;
            initialized = true;

            crystalController?.Initialize(stageData.CrystalHp);
            if (crystalController == null)
            {
                Debug.LogWarning("BattleManager is missing CrystalController.");
            }

            if (waveManager != null)
            {
                waveManager.Initialize(stageData, laneManager, crystalController);
            }
            else
            {
                Debug.LogWarning("BattleManager is missing WaveManager.");
            }

            for (int i = 0; i < heroes.Count; i++)
            {
                heroes[i]?.InitializeFromSerializedData();
            }

            SetState(BattleState.Preparing);
            WaveChanged?.Invoke(0, stageData.Waves.Count);
            GoldChanged?.Invoke(goldEarned);
        }

        public void StartNextWave()
        {
            if (!initialized || activeStageData == null)
            {
                Debug.LogWarning("BattleManager cannot start a wave before a stage is initialized.");
                return;
            }

            if (currentState == BattleState.Defeat || currentState == BattleState.Victory)
            {
                return;
            }

            currentWaveIndex++;
            if (currentWaveIndex >= activeStageData.Waves.Count)
            {
                FinishBattle(true, "All waves cleared.");
                return;
            }

            WaveData wave = activeStageData.Waves[currentWaveIndex];
            if (wave == null)
            {
                Debug.LogWarning($"BattleManager found a missing WaveData at index {currentWaveIndex}.");
                StartNextWave();
                return;
            }

            if (waveManager == null)
            {
                Debug.LogWarning("BattleManager cannot start a wave because WaveManager is missing.");
                FinishBattle(false, "Wave system missing.");
                return;
            }

            SetState(BattleState.WaveRunning);
            WaveChanged?.Invoke(currentWaveIndex + 1, activeStageData.Waves.Count);
            waveManager.StartWave(wave);
        }

        public void SelectRune(RuneData runeData)
        {
            if (runeManager == null)
            {
                Debug.LogWarning("BattleManager cannot select a rune because RuneManager is missing.");
                return;
            }

            runeManager.SelectRune(runeData);
        }

        public void RestartBattle()
        {
            StageData stageToRestart = activeStageData != null ? activeStageData : initialStageData;
            if (stageToRestart == null)
            {
                Debug.LogWarning("BattleManager cannot restart because StageData is missing.");
                return;
            }

            waveManager?.StopCurrentWave(true);
            InitializeStage(stageToRestart);
            StartNextWave();
        }

        private void AutoAssignReferences()
        {
            if (laneManager == null)
            {
                laneManager = FindFirstObjectByType<LaneManager>();
            }

            if (crystalController == null)
            {
                crystalController = FindFirstObjectByType<CrystalController>();
            }

            if (waveManager == null)
            {
                waveManager = FindFirstObjectByType<WaveManager>();
            }

            if (runeManager == null)
            {
                runeManager = FindFirstObjectByType<RuneManager>();
            }

            if (runeEffectApplier == null)
            {
                runeEffectApplier = FindFirstObjectByType<RuneEffectApplier>();
            }
        }

        private void BindEvents()
        {
            if (crystalController != null)
            {
                crystalController.Destroyed -= HandleCrystalDestroyed;
                crystalController.Destroyed += HandleCrystalDestroyed;
            }

            if (waveManager != null)
            {
                waveManager.WaveCompleted -= HandleWaveCompleted;
                waveManager.MonsterKilled -= HandleMonsterKilled;
                waveManager.WaveCompleted += HandleWaveCompleted;
                waveManager.MonsterKilled += HandleMonsterKilled;
            }

            if (runeManager != null)
            {
                runeManager.RuneOptionsGenerated -= HandleRuneOptionsGenerated;
                runeManager.RuneSelected -= HandleRuneSelected;
                runeManager.RuneOptionsGenerated += HandleRuneOptionsGenerated;
                runeManager.RuneSelected += HandleRuneSelected;
            }
        }

        private void HandleWaveCompleted(WaveData waveData)
        {
            if (currentState != BattleState.WaveRunning)
            {
                return;
            }

            bool lastWaveCleared = activeStageData == null || currentWaveIndex >= activeStageData.Waves.Count - 1;
            if (lastWaveCleared)
            {
                FinishBattle(true, "Kingdom Crystal defended.");
                return;
            }

            if (waveData != null && waveData.IsBossWave)
            {
                StartNextWave();
                return;
            }

            OfferRuneSelection();
        }

        private void OfferRuneSelection()
        {
            SetState(BattleState.RuneSelection);

            if (runeManager == null)
            {
                Debug.LogWarning("BattleManager has no RuneManager. Continuing to next wave without rune selection.");
                StartNextWave();
                return;
            }

            IReadOnlyList<RuneData> options = runeManager.GenerateRuneOptions();
            if (options.Count == 0)
            {
                Debug.LogWarning("RuneManager did not provide rune options. Continuing to next wave.");
                StartNextWave();
            }
        }

        private void HandleRuneOptionsGenerated(IReadOnlyList<RuneData> options)
        {
            RuneOptionsOffered?.Invoke(options);
        }

        private void HandleRuneSelected(RuneData runeData)
        {
            if (currentState != BattleState.RuneSelection)
            {
                Debug.LogWarning("BattleManager received a rune selection outside RuneSelection state.");
                return;
            }

            if (runeEffectApplier != null)
            {
                runeEffectApplier.ApplyRune(runeData, heroes, crystalController);
            }
            else
            {
                Debug.LogWarning("BattleManager has no RuneEffectApplier. Rune selection was accepted without applying an effect.");
            }

            RuneSelected?.Invoke(runeData);
            StartNextWave();
        }

        private void HandleMonsterKilled(MonsterController monster)
        {
            if (monster == null || monster.Data == null)
            {
                return;
            }

            goldEarned += Mathf.Max(0, monster.Data.RewardGold);
            GoldChanged?.Invoke(goldEarned);
        }

        private void HandleCrystalDestroyed()
        {
            FinishBattle(false, "Kingdom Crystal destroyed.");
        }

        private void FinishBattle(bool victory, string message)
        {
            if (currentState == BattleState.Victory || currentState == BattleState.Defeat)
            {
                return;
            }

            waveManager?.StopCurrentWave(!victory);
            SetState(victory ? BattleState.Victory : BattleState.Defeat);
            int wavesCleared = victory ? currentWaveIndex + 1 : Mathf.Max(0, currentWaveIndex);
            BattleEnded?.Invoke(new BattleResult(victory, activeStageData, wavesCleared, goldEarned, message));
        }

        private void SetState(BattleState nextState)
        {
            if (currentState == nextState)
            {
                return;
            }

            currentState = nextState;
            BattleStateChanged?.Invoke(currentState);
            Debug.Log($"Battle state changed to {currentState}.");
        }
    }
}
