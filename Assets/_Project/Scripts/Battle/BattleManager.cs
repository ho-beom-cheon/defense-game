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
        [SerializeField] private HeroPlacementManager heroPlacementManager;
        [SerializeField] private List<HeroController> heroes = new List<HeroController>();
        [SerializeField] private List<UpgradeData> permanentUpgrades = new List<UpgradeData>();
        [SerializeField] private bool rebuildHeroesFromFormation = true;
        [SerializeField] private bool autoStartOnStart = true;

        private StageData activeStageData;
        private BattleState currentState = BattleState.None;
        private int currentWaveIndex = -1;
        private int goldEarned;
        private int monstersKilled;
        private float battleStartTime;
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
        public IReadOnlyList<UpgradeData> PermanentUpgrades => permanentUpgrades;

        private void Awake()
        {
            AutoAssignReferences();
            BindEvents();
        }

        private void OnEnable()
        {
            AutoAssignReferences();
            BindEvents();
        }

        private void Start()
        {
            EnsureFallbackContent();
            StageData selectedStageData = GameSession.ResolveStageForBattle(initialStageData);
            if (autoStartOnStart && selectedStageData != null)
            {
                InitializeStage(selectedStageData);
                StartNextWave();
            }
        }

        private void OnDisable()
        {
            UnbindEvents();
        }

        private void OnDestroy()
        {
            UnbindEvents();
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
            monstersKilled = 0;
            GameSession.BeginBattleRun();
            battleStartTime = Time.time;
            initialized = true;
            ShadowContractService.BeginBattle();

            int difficultyAdjustedCrystalHp = DifficultyRules.ApplyCrystalMaxHp(stageData.CrystalHp);
            int crystalMaxHp = difficultyAdjustedCrystalHp + UpgradeManager.GetCrystalMaxHpBonus(permanentUpgrades) + ShadowContractService.GetCrystalMaxHpBonus(difficultyAdjustedCrystalHp);
            crystalController?.Initialize(crystalMaxHp);
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

            runeEffectApplier?.ResetForBattle(waveManager);

            BuildAndInitializeHeroes();

            UpgradeManager.ApplyHeroUpgradeEffects(permanentUpgrades, heroes);
            ShadowContractService.ApplyHeroPassives(heroes);

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
                FinishBattle(true, "\ubaa8\ub4e0 \uc6e8\uc774\ube0c\ub97c \ud074\ub9ac\uc5b4\ud588\uc2b5\ub2c8\ub2e4.");
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
                FinishBattle(false, "\uc6e8\uc774\ube0c \uc2dc\uc2a4\ud15c\uc774 \uc5c6\uc2b5\ub2c8\ub2e4.");
                return;
            }

            SetState(BattleState.WaveRunning);
            WaveChanged?.Invoke(currentWaveIndex + 1, activeStageData.Waves.Count);
            CombatFeedbackEvents.RaiseWaveStarted(currentWaveIndex + 1);
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
            StageData stageToRestart = activeStageData != null ? activeStageData : GameSession.ResolveStageForBattle(initialStageData);
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
                laneManager = FindAnyObjectByType<LaneManager>();
            }

            if (crystalController == null)
            {
                crystalController = FindAnyObjectByType<CrystalController>();
            }

            if (waveManager == null)
            {
                waveManager = FindAnyObjectByType<WaveManager>();
            }

            if (runeManager == null)
            {
                runeManager = FindAnyObjectByType<RuneManager>();
            }

            if (runeEffectApplier == null)
            {
                runeEffectApplier = FindAnyObjectByType<RuneEffectApplier>();
            }

            if (heroPlacementManager == null)
            {
                heroPlacementManager = FindAnyObjectByType<HeroPlacementManager>();
            }
        }

        private void EnsureFallbackContent()
        {
            if (initialStageData == null)
            {
                initialStageData = PrototypeAssetLoader.LoadDefaultStage();
            }

            if (!HasAssignedUpgrades())
            {
                permanentUpgrades = PrototypeAssetLoader.LoadUpgrades();
            }
        }

        private bool HasAssignedUpgrades()
        {
            for (int i = 0; i < permanentUpgrades.Count; i++)
            {
                if (permanentUpgrades[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void BuildAndInitializeHeroes()
        {
            if (rebuildHeroesFromFormation && heroPlacementManager != null)
            {
                IReadOnlyList<HeroController> runtimeHeroes = heroPlacementManager.BuildRuntimeFormation(laneManager);
                heroes.Clear();
                for (int i = 0; i < runtimeHeroes.Count; i++)
                {
                    if (runtimeHeroes[i] != null)
                    {
                        heroes.Add(runtimeHeroes[i]);
                    }
                }
            }

            for (int i = 0; i < heroes.Count; i++)
            {
                heroes[i]?.InitializeFromSerializedData();
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

        private void UnbindEvents()
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

        private void HandleWaveCompleted(WaveData waveData)
        {
            if (currentState != BattleState.WaveRunning)
            {
                return;
            }

            bool lastWaveCleared = activeStageData == null || currentWaveIndex >= activeStageData.Waves.Count - 1;
            if (lastWaveCleared)
            {
                FinishBattle(true, "\ud06c\ub9ac\uc2a4\ud0c8 \ubc29\uc5b4 \uc131\uacf5!");
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

            goldEarned += Mathf.Max(0, monster.RewardGold);
            monstersKilled++;
            ShadowContractService.RecordMonsterKilled(monster);
            GoldChanged?.Invoke(goldEarned);
        }

        private void HandleCrystalDestroyed()
        {
            FinishBattle(false, "\ud06c\ub9ac\uc2a4\ud0c8\uc774 \ud30c\uad34\ub418\uc5c8\uc2b5\ub2c8\ub2e4.");
        }

        private void FinishBattle(bool victory, string message)
        {
            if (currentState == BattleState.Victory || currentState == BattleState.Defeat)
            {
                return;
            }

            waveManager?.StopCurrentWave(!victory);
            SetState(victory ? BattleState.Victory : BattleState.Defeat);
            if (victory)
            {
                CombatFeedbackEvents.RaiseVictory();
            }
            else
            {
                CombatFeedbackEvents.RaiseDefeat();
            }

            AudioManager.Play(victory ? SfxKey.Victory : SfxKey.Defeat);
            int wavesCleared = victory ? currentWaveIndex + 1 : Mathf.Max(0, currentWaveIndex);
            int crystalHp = crystalController != null ? crystalController.CurrentHp : 0;
            int crystalMaxHp = crystalController != null ? crystalController.MaxHp : 0;
            Debug.Log($"Battle finished. Victory={victory}, Stage={activeStageData?.StageId}, WavesCleared={wavesCleared}, Gold={goldEarned}, RunId={GameSession.CurrentBattleRunId}.");
            BattleEnded?.Invoke(new BattleResult(victory, activeStageData, wavesCleared, goldEarned, message, Mathf.Max(0f, Time.time - battleStartTime), monstersKilled, crystalHp, crystalMaxHp, GameSession.CurrentBattleRunId));
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
