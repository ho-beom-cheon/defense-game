using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuneGate
{
    public sealed class RuneGateRuntimeSmokeRunner : MonoBehaviour
    {
        private const string SmokeArgument = "-runegateSmoke";
        private const string FullChapterSmokeArgument = "-runegateSmokeFullChapter";
        private const string SystemFlowSmokeArgument = "-runegateSmokeSystemFlows";
        private const string SaveWriteSmokeArgument = "-runegateSmokeSaveWrite";
        private const string SaveReadSmokeArgument = "-runegateSmokeSaveRead";
        private const string CorruptSaveSmokeArgument = "-runegateSmokeCorruptSave";
        private const string InterruptedSaveSmokeArgument = "-runegateSmokeInterruptedSave";
        private const string DefaultSaveFileName = "runegate_save.json";
        private const float AcceleratedTimeScale = 6f;
        private const float SceneLoadTimeoutSeconds = 15f;
        private const float BattleTimeoutSeconds = 45f;
        private const float FullChapterBattleTimeoutSeconds = 180f;

        private BattleManager battleManager;
        private WaveManager waveManager;
        private BattleResult battleResult;
        private bool battleFinished;
        private bool failed;
        private bool fullChapterMode;
        private bool systemFlowMode;
        private bool saveWriteMode;
        private bool saveReadMode;
        private bool corruptSaveMode;
        private bool interruptedSaveMode;
        private bool runeSelectionPending;
        private bool sawBossThisStage;
        private bool waitSucceeded;
        private int upgradePurchaseCount;
        private float previousTimeScale = 1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            bool smokeRequested = HasCommandLineArgument(SmokeArgument) ||
                                  HasCommandLineArgument(FullChapterSmokeArgument) ||
                                  HasCommandLineArgument(SystemFlowSmokeArgument) ||
                                  HasCommandLineArgument(SaveWriteSmokeArgument) ||
                                  HasCommandLineArgument(SaveReadSmokeArgument) ||
                                  HasCommandLineArgument(CorruptSaveSmokeArgument) ||
                                  HasCommandLineArgument(InterruptedSaveSmokeArgument);
            if (!smokeRequested || FindAnyObjectByType<RuneGateRuntimeSmokeRunner>() != null)
            {
                return;
            }

            GameObject runnerObject = new GameObject(nameof(RuneGateRuntimeSmokeRunner));
            DontDestroyOnLoad(runnerObject);
            runnerObject.AddComponent<RuneGateRuntimeSmokeRunner>();
        }

        private IEnumerator Start()
        {
            Application.runInBackground = true;
            fullChapterMode = HasCommandLineArgument(FullChapterSmokeArgument);
            systemFlowMode = HasCommandLineArgument(SystemFlowSmokeArgument);
            saveWriteMode = HasCommandLineArgument(SaveWriteSmokeArgument);
            saveReadMode = HasCommandLineArgument(SaveReadSmokeArgument);
            corruptSaveMode = HasCommandLineArgument(CorruptSaveSmokeArgument);
            interruptedSaveMode = HasCommandLineArgument(InterruptedSaveSmokeArgument);
            previousTimeScale = Time.timeScale;
            Time.timeScale = AcceleratedTimeScale;

            yield return null;
            yield return RunSmokeTest();
        }

        private void OnDestroy()
        {
            UnbindBattleEvents();
            Time.timeScale = previousTimeScale;
        }

        private IEnumerator RunSmokeTest()
        {
            if (!RequireIsolatedSavePath())
            {
                yield break;
            }

            if (interruptedSaveMode)
            {
                RunInterruptedSaveRecoveryTest();
                yield break;
            }

            if (corruptSaveMode)
            {
                RunCorruptSaveRecoveryTest();
                yield break;
            }

            if (saveReadMode)
            {
                yield return RunCrossProcessSaveReadTest();
                yield break;
            }

            CleanupSmokeSave();
            SaveManager.ResetSave();

            if (saveWriteMode)
            {
                RunCrossProcessSaveWriteTest();
                yield break;
            }

            if (systemFlowMode)
            {
                yield return RunSystemFlowSmokeTest();
                yield break;
            }

            SaveManager.MarkTutorialSeen();

            if (!Require(SceneManager.GetActiveScene().name == "TitleScene", "Player did not start in TitleScene."))
            {
                yield break;
            }

            if (!Require(FindAnyObjectByType<TitleUI>() != null, "TitleScene is missing TitleUI."))
            {
                yield break;
            }

            Debug.Log("[RuneGateE2E] TitleScene verified.");
            SceneManager.LoadScene("StageSelectScene");
            yield return WaitForCondition(() => SceneManager.GetActiveScene().name == "StageSelectScene", SceneLoadTimeoutSeconds);
            if (!Require(waitSucceeded, "StageSelectScene did not load."))
            {
                yield break;
            }

            yield return null;
            StageSelectUI stageSelect = FindAnyObjectByType<StageSelectUI>();
            if (!Require(stageSelect != null, "StageSelectScene is missing StageSelectUI."))
            {
                yield break;
            }

            if (!Require(stageSelect.Stages != null && stageSelect.Stages.Count >= 10, "StageSelectScene did not expose Stage 1 through Stage 10."))
            {
                yield break;
            }

            StageData firstStage = FindStage(stageSelect.Stages, 1);
            if (!Require(firstStage != null, "Stage 1 data is missing from StageSelectScene."))
            {
                yield break;
            }

            string nextStageId = GameSession.ResolveNextStageId(firstStage.StageId, stageSelect.Stages);
            if (!Require(!string.IsNullOrWhiteSpace(nextStageId), "Stage 2 could not be resolved from Stage 1."))
            {
                yield break;
            }

            Debug.Log("[RuneGateE2E] StageSelectScene verified.");
            if (fullChapterMode)
            {
                List<StageData> chapterStages = new List<StageData>();
                for (int i = 0; i < stageSelect.Stages.Count; i++)
                {
                    if (stageSelect.Stages[i] != null)
                    {
                        chapterStages.Add(stageSelect.Stages[i]);
                    }
                }

                PrototypeAssetLoader.SortStagesByStageId(chapterStages);
                yield return RunFullChapterSmokeTest(chapterStages);
                yield break;
            }

            GameSession.SelectDifficulty("normal");
            GameSession.SelectStage(firstStage, nextStageId);
            SaveManager.SetLastSelectedStageId(firstStage.StageId);
            SceneManager.LoadScene("BattleScene");

            yield return WaitForCondition(() => SceneManager.GetActiveScene().name == "BattleScene", SceneLoadTimeoutSeconds);
            if (!Require(waitSucceeded, "BattleScene did not load."))
            {
                yield break;
            }

            yield return WaitForCondition(() =>
            {
                battleManager = FindAnyObjectByType<BattleManager>();
                return battleManager != null && battleManager.ActiveStageData != null;
            }, SceneLoadTimeoutSeconds);
            if (!Require(waitSucceeded, "BattleManager did not initialize Stage 1."))
            {
                yield break;
            }

            if (!Require(battleManager.ActiveStageData.StageId == firstStage.StageId, "BattleScene initialized the wrong stage."))
            {
                yield break;
            }

            if (!Require(battleManager.Heroes != null && battleManager.Heroes.Count >= 6, "BattleScene did not build the six-hero formation."))
            {
                yield break;
            }

            BindBattleEvents();
            Debug.Log("[RuneGateE2E] BattleScene verified. Running Stage 1 combat.");
            yield return WaitForCondition(() => battleFinished, BattleTimeoutSeconds);
            if (!Require(waitSucceeded, "Stage 1 combat did not finish before the smoke-test timeout."))
            {
                yield break;
            }

            if (!Require(battleResult.IsVictory, "Stage 1 ended in defeat during the runtime smoke test."))
            {
                yield break;
            }

            if (!Require(battleResult.WavesCleared == firstStage.Waves.Count,
                    $"Victory reported {battleResult.WavesCleared} cleared waves, expected {firstStage.Waves.Count}."))
            {
                yield break;
            }

            yield return null;
            yield return null;
            StageResultUI resultUI = FindAnyObjectByType<StageResultUI>();
            if (!Require(resultUI != null && resultUI.IsVisible, "Victory result UI was not shown."))
            {
                yield break;
            }

            if (!Require(SaveManager.IsStageCleared(firstStage.StageId), "Stage 1 clear state was not saved."))
            {
                yield break;
            }

            if (!Require(SaveManager.IsStageUnlocked(nextStageId), "Stage 2 was not unlocked after victory."))
            {
                yield break;
            }

            if (!Require(SaveManager.Current.totalGold >= 110, "Stage 1 did not award the minimum 110 Gold."))
            {
                yield break;
            }

            resultUI.ContinueToNextStage();
            yield return WaitForCondition(() =>
            {
                BattleManager nextBattle = FindAnyObjectByType<BattleManager>();
                return SceneManager.GetActiveScene().name == "BattleScene" && nextBattle != null &&
                       nextBattle.ActiveStageData != null && nextBattle.ActiveStageData.StageId == nextStageId;
            }, SceneLoadTimeoutSeconds);
            if (!Require(waitSucceeded, "Victory result did not continue to Stage 2."))
            {
                yield break;
            }

            Debug.Log("[RuneGateE2E] Victory, Gold persistence, Stage 2 unlock, and next-stage navigation verified.");
            SceneManager.LoadScene("UpgradeScene");
            yield return WaitForCondition(() => SceneManager.GetActiveScene().name == "UpgradeScene", SceneLoadTimeoutSeconds);
            if (!Require(waitSucceeded, "UpgradeScene did not load."))
            {
                yield break;
            }

            yield return null;
            if (!Require(FindAnyObjectByType<UpgradeSceneUI>() != null, "UpgradeScene is missing UpgradeSceneUI."))
            {
                yield break;
            }

            UpgradeManager upgradeManager = FindAnyObjectByType<UpgradeManager>();
            if (!Require(upgradeManager != null && upgradeManager.AvailableUpgrades.Count >= 4, "UpgradeScene did not expose the four progression upgrades."))
            {
                yield break;
            }

            Debug.Log("RUNEGATE_E2E_PASSED");
            CleanupSmokeSave();
            Time.timeScale = previousTimeScale;
            Application.Quit(0);
        }

        private void RunInterruptedSaveRecoveryTest()
        {
            SaveData recoveredSave = SaveManager.Current;
            if (!Require(recoveredSave != null && recoveredSave.totalGold == 777,
                    "Interrupted save recovery did not promote the latest temporary Gold."))
            {
                return;
            }

            if (!Require(SaveManager.GetUpgradeLevel("upgrade_hero_attack") == 3 &&
                         recoveredSave.hasSeenTutorial &&
                         SaveManager.IsStageUnlocked("stage_goblin_forest_02"),
                    "Interrupted save recovery did not promote progression fields."))
            {
                return;
            }

            if (!Require(File.Exists(SaveManager.SavePath) && !File.Exists(SaveManager.SavePath + ".tmp"),
                    "Interrupted save recovery did not replace the missing primary file with the temporary save."))
            {
                return;
            }

            bool isolatedInvalidTemporary = File.Exists(SaveManager.SavePath + ".tmp.corrupt");
            Debug.Log(isolatedInvalidTemporary
                ? "RUNEGATE_INVALID_TEMP_BACKUP_RESTORE_PASSED"
                : "RUNEGATE_INTERRUPTED_SAVE_RECOVERY_PASSED");
            CleanupSmokeSave();
            Time.timeScale = previousTimeScale;
            Application.Quit(0);
        }

        private void RunCorruptSaveRecoveryTest()
        {
            SaveData recoveredSave = SaveManager.Current;
            bool restoredBackup = File.Exists(SaveManager.SavePath + ".bak");
            int expectedGold = restoredBackup ? 777 : 0;
            if (!Require(recoveredSave != null && recoveredSave.totalGold == expectedGold,
                    restoredBackup
                        ? "Corrupt save recovery did not restore Gold from the valid backup."
                        : "Corrupt save recovery did not create safe default Gold."))
            {
                return;
            }

            if (!Require(SaveManager.IsStageUnlocked(SaveManager.DefaultUnlockedStageId),
                    "Corrupt save recovery did not restore the default Stage 1 unlock."))
            {
                return;
            }

            if (restoredBackup && !Require(SaveManager.GetUpgradeLevel("upgrade_hero_attack") == 3 &&
                                           recoveredSave.hasSeenTutorial &&
                                           SaveManager.IsStageUnlocked("stage_goblin_forest_02"),
                    "Corrupt save recovery did not restore progression fields from the valid backup."))
            {
                return;
            }

            if (!Require(File.Exists(SaveManager.SavePath),
                    "Corrupt save recovery did not write a replacement primary save."))
            {
                return;
            }

            if (!Require(File.Exists(SaveManager.SavePath + ".corrupt"),
                    "Corrupt save recovery did not preserve the damaged source file."))
            {
                return;
            }

            Debug.Log(restoredBackup
                ? "RUNEGATE_CORRUPT_SAVE_BACKUP_RESTORE_PASSED"
                : "RUNEGATE_CORRUPT_SAVE_RECOVERY_PASSED");
            CleanupSmokeSave();
            Time.timeScale = previousTimeScale;
            Application.Quit(0);
        }

        private void RunCrossProcessSaveWriteTest()
        {
            const string upgradeId = "upgrade_hero_attack";
            const string selectedStageId = "stage_goblin_forest_03";
            SaveManager.AddGold(777);
            SaveManager.SetUpgradeLevel(upgradeId, 3);
            SaveManager.MarkTutorialSeen();
            SaveManager.SetLastSelectedStageId(selectedStageId);
            SaveManager.UnlockStage("stage_goblin_forest_02");
            SaveManager.Save();

            if (!Require(File.Exists(SaveManager.SavePath), "Cross-process save writer did not create the JSON file."))
            {
                return;
            }

            Debug.Log("RUNEGATE_SAVE_WRITE_PASSED");
            Time.timeScale = previousTimeScale;
            Application.Quit(0);
        }

        private IEnumerator RunCrossProcessSaveReadTest()
        {
            if (!Require(File.Exists(SaveManager.SavePath), "Cross-process save reader could not find the writer JSON file."))
            {
                yield break;
            }

            SaveData loadedSave = SaveManager.ReloadFromDiskForDiagnostics();
            if (!Require(loadedSave != null && loadedSave.totalGold == 777,
                    "Cross-process save reader did not restore Gold."))
            {
                yield break;
            }

            if (!Require(SaveManager.GetUpgradeLevel("upgrade_hero_attack") == 3,
                    "Cross-process save reader did not restore the upgrade level."))
            {
                yield break;
            }

            if (!Require(loadedSave.hasSeenTutorial && loadedSave.lastSelectedStageId == "stage_goblin_forest_03",
                    "Cross-process save reader did not restore tutorial or stage selection state."))
            {
                yield break;
            }

            if (!Require(SaveManager.IsStageUnlocked("stage_goblin_forest_01") &&
                         SaveManager.IsStageUnlocked("stage_goblin_forest_02"),
                    "Cross-process save reader did not restore stage unlocks."))
            {
                yield break;
            }

            Debug.Log("RUNEGATE_SAVE_READ_PASSED");
            CleanupSmokeSave();
            Time.timeScale = previousTimeScale;
            yield return null;
            Application.Quit(0);
        }

        private IEnumerator RunSystemFlowSmokeTest()
        {
            if (!Require(SceneManager.GetActiveScene().name == "TitleScene", "System flow test did not start in TitleScene."))
            {
                yield break;
            }

            if (!Require(FindAnyObjectByType<TitleUI>() != null, "TitleScene is missing TitleUI."))
            {
                yield break;
            }

            SceneManager.LoadScene("StageSelectScene");
            yield return WaitForCondition(() => SceneManager.GetActiveScene().name == "StageSelectScene", SceneLoadTimeoutSeconds);
            if (!Require(waitSucceeded, "StageSelectScene did not load for the system flow test."))
            {
                yield break;
            }

            yield return null;
            StageSelectUI stageSelect = FindAnyObjectByType<StageSelectUI>();
            if (!Require(stageSelect != null && stageSelect.Stages != null && stageSelect.Stages.Count >= 2,
                    "System flow test requires at least Stage 1 and Stage 2."))
            {
                yield break;
            }

            StageData firstStage = FindStage(stageSelect.Stages, 1);
            StageData secondStage = FindStage(stageSelect.Stages, 2);
            if (!Require(firstStage != null && secondStage != null, "Stage 1 or Stage 2 data is missing."))
            {
                yield break;
            }

            GameSession.SelectDifficulty("normal");
            GameSession.SelectStage(firstStage, secondStage.StageId);
            SceneManager.LoadScene("BattleScene");
            yield return WaitForCondition(() => SceneManager.GetActiveScene().name == "BattleScene", SceneLoadTimeoutSeconds);
            if (!Require(waitSucceeded, "BattleScene did not load for tutorial verification."))
            {
                yield break;
            }

            yield return WaitForCondition(() => FindAnyObjectByType<TutorialManager>() != null, SceneLoadTimeoutSeconds);
            TutorialManager tutorialManager = FindAnyObjectByType<TutorialManager>();
            if (!Require(waitSucceeded && tutorialManager != null && tutorialManager.IsVisible,
                    "First battle did not show the tutorial overlay."))
            {
                yield break;
            }

            if (!Require(tutorialManager.StepCount >= 7, "Tutorial did not expose the expected seven guidance steps."))
            {
                yield break;
            }

            int tutorialGuard = tutorialManager.StepCount + 1;
            while (tutorialManager.IsVisible && tutorialGuard-- > 0)
            {
                tutorialManager.Next();
                yield return null;
            }

            if (!Require(!tutorialManager.IsVisible && SaveManager.HasSeenTutorial(),
                    "Tutorial completion was not persisted."))
            {
                yield break;
            }

            if (!Require(Mathf.Approximately(Time.timeScale, AcceleratedTimeScale),
                    "Tutorial completion did not restore the previous time scale."))
            {
                yield break;
            }

            BattlePauseController pauseController = FindAnyObjectByType<BattlePauseController>();
            if (!Require(pauseController != null && pauseController.CanPause,
                    "Battle pause controller was not available after the tutorial."))
            {
                yield break;
            }

            if (!Require(pauseController.Pause() && pauseController.IsPaused && Mathf.Approximately(Time.timeScale, 0f),
                    "Battle pause did not stop scaled time."))
            {
                yield break;
            }

            yield return null;
            pauseController.Resume();
            if (!Require(!pauseController.IsPaused && Mathf.Approximately(Time.timeScale, AcceleratedTimeScale),
                    "Battle resume did not restore the previous time scale."))
            {
                yield break;
            }

            if (!Require(pauseController.PauseForLifecycle() && pauseController.PausedByLifecycle,
                    "Lifecycle pause did not record its pause reason."))
            {
                yield break;
            }

            pauseController.Resume();
            Debug.Log("[RuneGateE2E] Battle pause and lifecycle resume verified.");

            const string diagnosticUpgradeId = "upgrade_hero_attack";
            SaveManager.AddGold(321);
            SaveManager.SetUpgradeLevel(diagnosticUpgradeId, 2);
            SaveManager.SetLastSelectedStageId(firstStage.StageId);
            SaveManager.Save();

            SaveData reloadedSave = SaveManager.ReloadFromDiskForDiagnostics();
            if (!Require(reloadedSave != null && reloadedSave.totalGold == 321,
                    "Gold did not survive a real JSON disk reload."))
            {
                yield break;
            }

            if (!Require(SaveManager.GetUpgradeLevel(diagnosticUpgradeId) == 2,
                    "Upgrade level did not survive a real JSON disk reload."))
            {
                yield break;
            }

            if (!Require(reloadedSave.hasSeenTutorial && reloadedSave.lastSelectedStageId == firstStage.StageId,
                    "Tutorial or selected stage state did not survive a real JSON disk reload."))
            {
                yield break;
            }

            Debug.Log("[RuneGateSystemE2E] Tutorial and JSON disk reload verified.");

            SaveManager.ResetSave();
            SaveData resetSave = SaveManager.ReloadFromDiskForDiagnostics();
            if (!Require(resetSave != null && resetSave.totalGold == 0 && !resetSave.hasSeenTutorial,
                    "Reset Save did not restore Gold and tutorial defaults."))
            {
                yield break;
            }

            if (!Require(SaveManager.GetUpgradeLevel(diagnosticUpgradeId) == 0,
                    "Reset Save did not clear persisted upgrades."))
            {
                yield break;
            }

            if (!Require(SaveManager.IsStageUnlocked(firstStage.StageId) && !SaveManager.IsStageUnlocked(secondStage.StageId),
                    "Reset Save did not restore the default Stage 1-only unlock state."))
            {
                yield break;
            }

            Debug.Log("[RuneGateSystemE2E] Reset Save defaults verified.");

            SaveManager.MarkTutorialSeen();
            GameSession.SelectDifficulty("normal");
            GameSession.SelectStage(firstStage, secondStage.StageId);
            SceneManager.LoadScene("BattleScene");
            yield return WaitForCondition(() => SceneManager.GetActiveScene().name == "BattleScene", SceneLoadTimeoutSeconds);
            if (!Require(waitSucceeded, "BattleScene did not reload for defeat verification."))
            {
                yield break;
            }

            yield return WaitForCondition(() =>
            {
                battleManager = FindAnyObjectByType<BattleManager>();
                return battleManager != null && battleManager.ActiveStageData != null &&
                       FindAnyObjectByType<CrystalController>() != null;
            }, SceneLoadTimeoutSeconds);
            if (!Require(waitSucceeded, "BattleManager or CrystalController did not initialize for defeat verification."))
            {
                yield break;
            }

            BindBattleEvents();
            CrystalController crystalController = FindAnyObjectByType<CrystalController>();
            crystalController.TakeDamage(crystalController.CurrentHp);
            yield return WaitForCondition(() => battleFinished, SceneLoadTimeoutSeconds);
            if (!Require(waitSucceeded, "Destroying the crystal did not end the battle."))
            {
                yield break;
            }

            if (!Require(!battleResult.IsVictory && battleManager.CurrentState == BattleState.Defeat,
                    "Crystal destruction did not produce the Defeat state."))
            {
                yield break;
            }

            yield return null;
            yield return null;
            StageResultUI resultUI = FindAnyObjectByType<StageResultUI>();
            if (!Require(resultUI != null && resultUI.IsVisible, "Defeat result UI was not shown."))
            {
                yield break;
            }

            if (!Require(!SaveManager.IsStageCleared(firstStage.StageId) && !SaveManager.IsStageUnlocked(secondStage.StageId),
                    "Defeat incorrectly cleared Stage 1 or unlocked Stage 2."))
            {
                yield break;
            }

            resultUI.RetryBattle();
            yield return WaitForCondition(() =>
            {
                CrystalController restartedCrystal = FindAnyObjectByType<CrystalController>();
                return battleManager != null && battleManager.CurrentState == BattleState.WaveRunning &&
                       restartedCrystal != null && restartedCrystal.CurrentHp == restartedCrystal.MaxHp &&
                       !resultUI.IsVisible;
            }, SceneLoadTimeoutSeconds);
            if (!Require(waitSucceeded, "Defeat result Retry did not restart the current battle."))
            {
                yield break;
            }

            battleFinished = false;
            crystalController = FindAnyObjectByType<CrystalController>();
            crystalController.TakeDamage(crystalController.CurrentHp);
            yield return WaitForCondition(() => battleFinished, SceneLoadTimeoutSeconds);
            if (!Require(waitSucceeded, "Second crystal destruction did not produce a result."))
            {
                yield break;
            }

            yield return null;
            resultUI = FindAnyObjectByType<StageResultUI>();
            if (!Require(resultUI != null && resultUI.IsVisible, "Second Defeat result UI was not shown."))
            {
                yield break;
            }

            resultUI.OpenUpgrade();
            yield return WaitForCondition(() => SceneManager.GetActiveScene().name == "UpgradeScene", SceneLoadTimeoutSeconds);
            if (!Require(waitSucceeded && FindAnyObjectByType<UpgradeSceneUI>() != null,
                    "Defeat result Upgrade button did not open UpgradeScene."))
            {
                yield break;
            }

            GameSession.SelectStage(firstStage, secondStage.StageId);
            SceneManager.LoadScene("BattleScene");
            yield return WaitForCondition(() => SceneManager.GetActiveScene().name == "BattleScene", SceneLoadTimeoutSeconds);
            if (!Require(waitSucceeded, "BattleScene did not load for Stage Select result navigation verification."))
            {
                yield break;
            }

            yield return WaitForCondition(() =>
            {
                battleManager = FindAnyObjectByType<BattleManager>();
                return battleManager != null && battleManager.ActiveStageData != null &&
                       FindAnyObjectByType<CrystalController>() != null;
            }, SceneLoadTimeoutSeconds);
            if (!Require(waitSucceeded, "Battle did not initialize for Stage Select result navigation verification."))
            {
                yield break;
            }

            battleFinished = false;
            BindBattleEvents();
            crystalController = FindAnyObjectByType<CrystalController>();
            crystalController.TakeDamage(crystalController.CurrentHp);
            yield return WaitForCondition(() => battleFinished, SceneLoadTimeoutSeconds);
            if (!Require(waitSucceeded, "Third crystal destruction did not produce a result."))
            {
                yield break;
            }

            yield return null;
            resultUI = FindAnyObjectByType<StageResultUI>();
            if (!Require(resultUI != null && resultUI.IsVisible, "Third Defeat result UI was not shown."))
            {
                yield break;
            }

            resultUI.OpenStageSelect();
            yield return WaitForCondition(() => SceneManager.GetActiveScene().name == "StageSelectScene", SceneLoadTimeoutSeconds);
            if (!Require(waitSucceeded && FindAnyObjectByType<StageSelectUI>() != null,
                    "Defeat result Stage Select button did not open StageSelectScene."))
            {
                yield break;
            }

            Debug.Log("RUNEGATE_SYSTEM_FLOWS_E2E_PASSED: Retry, Upgrade, and Stage Select navigation verified.");
            CleanupSmokeSave();
            Time.timeScale = previousTimeScale;
            Application.Quit(0);
        }

        private IEnumerator RunFullChapterSmokeTest(IReadOnlyList<StageData> stages)
        {
            GameSession.SelectDifficulty("normal");
            for (int stageNumber = 1; stageNumber <= 10; stageNumber++)
            {
                StageData stage = FindStage(stages, stageNumber);
                if (!Require(stage != null, $"Stage {stageNumber} data is missing."))
                {
                    yield break;
                }

                if (!Require(SaveManager.IsStageUnlocked(stage.StageId), $"Stage {stageNumber} was not unlocked before selection."))
                {
                    yield break;
                }

                string nextStageId = GameSession.ResolveNextStageId(stage.StageId, stages);
                GameSession.SelectStage(stage, nextStageId);
                SaveManager.SetLastSelectedStageId(stage.StageId);

                battleFinished = false;
                runeSelectionPending = false;
                sawBossThisStage = false;
                SceneManager.LoadScene("BattleScene");
                yield return WaitForCondition(() => SceneManager.GetActiveScene().name == "BattleScene", SceneLoadTimeoutSeconds);
                if (!Require(waitSucceeded, $"Stage {stageNumber} BattleScene did not load."))
                {
                    yield break;
                }

                yield return WaitForCondition(() =>
                {
                    battleManager = FindAnyObjectByType<BattleManager>();
                    return battleManager != null && battleManager.ActiveStageData != null;
                }, SceneLoadTimeoutSeconds);
                if (!Require(waitSucceeded, $"Stage {stageNumber} BattleManager did not initialize."))
                {
                    yield break;
                }

                if (!Require(battleManager.ActiveStageData.StageId == stage.StageId, $"Stage {stageNumber} initialized the wrong StageData."))
                {
                    yield break;
                }

                if (!Require(battleManager.Heroes != null && battleManager.Heroes.Count >= 6, $"Stage {stageNumber} did not build the six-hero formation."))
                {
                    yield break;
                }

                waveManager = FindAnyObjectByType<WaveManager>();
                BindBattleEvents();
                Debug.Log($"[RuneGateFullE2E] Running Stage {stageNumber}: {stage.DisplayNameKorean}");
                yield return WaitForCondition(() => battleFinished || failed, FullChapterBattleTimeoutSeconds);
                if (failed)
                {
                    yield break;
                }

                if (!waitSucceeded)
                {
                    LogBattleTimeoutSnapshot(stageNumber);
                }

                if (!Require(waitSucceeded,
                        $"Stage {stageNumber} did not finish within {FullChapterBattleTimeoutSeconds:0} seconds."))
                {
                    yield break;
                }

                if (!Require(battleResult.IsVictory, $"Stage {stageNumber} ended in defeat."))
                {
                    yield break;
                }

                if (!Require(battleResult.WavesCleared == stage.Waves.Count,
                        $"Stage {stageNumber} reported {battleResult.WavesCleared}/{stage.Waves.Count} cleared waves."))
                {
                    yield break;
                }

                yield return null;
                yield return null;
                StageResultUI resultUI = FindAnyObjectByType<StageResultUI>();
                if (!Require(resultUI != null && resultUI.IsVisible, $"Stage {stageNumber} result UI was not shown."))
                {
                    yield break;
                }

                if (!Require(SaveManager.IsStageCleared(stage.StageId), $"Stage {stageNumber} clear state was not saved."))
                {
                    yield break;
                }

                if (stageNumber < 10 && !Require(!string.IsNullOrWhiteSpace(nextStageId) && SaveManager.IsStageUnlocked(nextStageId),
                        $"Stage {stageNumber + 1} was not unlocked."))
                {
                    yield break;
                }

                if (stageNumber == 10 && !Require(sawBossThisStage, "Stage 10 completed without spawning a boss monster."))
                {
                    yield break;
                }

                Debug.Log($"[RuneGateFullE2E] Stage {stageNumber} victory verified. Gold={SaveManager.Current.totalGold}");
                UnbindBattleEvents();

                SceneManager.LoadScene("UpgradeScene");
                yield return WaitForCondition(() => SceneManager.GetActiveScene().name == "UpgradeScene", SceneLoadTimeoutSeconds);
                if (!Require(waitSucceeded, $"UpgradeScene did not load after Stage {stageNumber}."))
                {
                    yield break;
                }

                yield return null;
                UpgradeManager upgradeManager = FindAnyObjectByType<UpgradeManager>();
                if (!Require(upgradeManager != null && CountValidUpgrades(upgradeManager.AvailableUpgrades) >= 4,
                        $"UpgradeScene is invalid after Stage {stageNumber}."))
                {
                    yield break;
                }

                TryPurchaseOneUpgrade(upgradeManager);
                SceneManager.LoadScene("StageSelectScene");
                yield return WaitForCondition(() => SceneManager.GetActiveScene().name == "StageSelectScene", SceneLoadTimeoutSeconds);
                if (!Require(waitSucceeded && FindAnyObjectByType<StageSelectUI>() != null,
                        $"StageSelectScene did not recover after Stage {stageNumber}."))
                {
                    yield break;
                }
            }

            if (!Require(upgradePurchaseCount > 0, "Full chapter completed without any persisted upgrade purchase."))
            {
                yield break;
            }

            Debug.Log($"RUNEGATE_FULL_CHAPTER_E2E_PASSED: upgrades={upgradePurchaseCount}, gold={SaveManager.Current.totalGold}");
            CleanupSmokeSave();
            Time.timeScale = previousTimeScale;
            Application.Quit(0);
        }

        private void LogBattleTimeoutSnapshot(int stageNumber)
        {
            IReadOnlyList<MonsterController> aliveMonsters = waveManager != null ? waveManager.AliveMonsters : null;
            int aliveCount = aliveMonsters != null ? aliveMonsters.Count : 0;
            Debug.LogError($"[RuneGateFullE2E] Stage {stageNumber} timeout snapshot: alive monsters={aliveCount}.");

            int logCount = Mathf.Min(aliveCount, 12);
            for (int i = 0; i < logCount; i++)
            {
                MonsterController monster = aliveMonsters[i];
                if (monster == null)
                {
                    continue;
                }

                string monsterName = monster.Data != null ? monster.Data.DisplayNameKorean : monster.name;
                Debug.LogError(
                    $"[RuneGateFullE2E] Alive {i + 1}/{aliveCount}: {monsterName}, lane={monster.LaneIndex}, " +
                    $"hp={monster.CurrentHp}/{monster.MaxHp}, x={monster.transform.position.x:0.00}, state={monster.CombatState}, " +
                    $"moveAttackLocked={monster.IsMovementAttackLocked}, attackRoutine={monster.HasActiveAttackRoutine}.");
            }
        }

        private void TryPurchaseOneUpgrade(UpgradeManager upgradeManager)
        {
            IReadOnlyList<UpgradeData> upgrades = upgradeManager.AvailableUpgrades;
            for (int i = 0; i < upgrades.Count; i++)
            {
                UpgradeData upgrade = upgrades[i];
                if (upgrade == null || !upgradeManager.CanPurchase(upgrade))
                {
                    continue;
                }

                int previousLevel = upgradeManager.GetLevel(upgrade);
                if (upgradeManager.TryPurchase(upgrade) && upgradeManager.GetLevel(upgrade) == previousLevel + 1)
                {
                    upgradePurchaseCount++;
                    Debug.Log($"[RuneGateFullE2E] Upgrade purchased: {upgrade.DisplayName} Lv.{previousLevel + 1}");
                }

                return;
            }
        }

        private void BindBattleEvents()
        {
            if (battleManager == null)
            {
                return;
            }

            battleManager.RuneOptionsOffered -= HandleRuneOptionsOffered;
            battleManager.BattleEnded -= HandleBattleEnded;
            battleManager.RuneOptionsOffered += HandleRuneOptionsOffered;
            battleManager.BattleEnded += HandleBattleEnded;
            if (waveManager != null)
            {
                waveManager.MonsterSpawned -= HandleMonsterSpawned;
                waveManager.MonsterSpawned += HandleMonsterSpawned;
            }
        }

        private void UnbindBattleEvents()
        {
            if (battleManager == null)
            {
                return;
            }

            battleManager.RuneOptionsOffered -= HandleRuneOptionsOffered;
            battleManager.BattleEnded -= HandleBattleEnded;
            if (waveManager != null)
            {
                waveManager.MonsterSpawned -= HandleMonsterSpawned;
            }
        }

        private void HandleRuneOptionsOffered(IReadOnlyList<RuneData> options)
        {
            if (battleManager == null || options == null || options.Count == 0)
            {
                Fail("Rune selection did not provide any choices.");
                return;
            }

            if (!runeSelectionPending)
            {
                runeSelectionPending = true;
                StartCoroutine(SelectRuneNextFrame(options[0]));
            }
        }

        private IEnumerator SelectRuneNextFrame(RuneData rune)
        {
            yield return null;
            runeSelectionPending = false;
            if (battleManager == null || battleManager.CurrentState != BattleState.RuneSelection)
            {
                Fail("Battle left RuneSelection before a rune choice could be applied.");
                yield break;
            }

            Debug.Log($"[RuneGateE2E] Selecting rune: {rune.DisplayName}");
            battleManager.SelectRune(rune);
        }

        private void HandleBattleEnded(BattleResult result)
        {
            battleResult = result;
            battleFinished = true;
        }

        private void HandleMonsterSpawned(MonsterController monster)
        {
            if (monster != null && monster.Data != null && monster.Data.IsBoss)
            {
                sawBossThisStage = true;
                Debug.Log($"[RuneGateFullE2E] Boss spawned: {monster.Data.DisplayNameKorean}");
            }
        }

        private IEnumerator WaitForCondition(Func<bool> condition, float timeoutSeconds)
        {
            waitSucceeded = false;
            float startedAt = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startedAt < timeoutSeconds)
            {
                if (condition())
                {
                    waitSucceeded = true;
                    yield break;
                }

                yield return null;
            }
        }

        private bool Require(bool condition, string failureMessage)
        {
            if (condition)
            {
                return true;
            }

            Fail(failureMessage);
            return false;
        }

        private bool RequireIsolatedSavePath()
        {
            string saveFileName = Path.GetFileName(SaveManager.SavePath);
            return Require(!string.Equals(saveFileName, DefaultSaveFileName, StringComparison.OrdinalIgnoreCase),
                "Runtime smoke test requires -runegateSavePath with an isolated test file.");
        }

        private void Fail(string message)
        {
            failed = true;
            string prefix;
            if (interruptedSaveMode)
            {
                prefix = "RUNEGATE_INTERRUPTED_SAVE_RECOVERY_FAILED";
            }
            else if (corruptSaveMode)
            {
                prefix = "RUNEGATE_CORRUPT_SAVE_RECOVERY_FAILED";
            }
            else if (saveWriteMode || saveReadMode)
            {
                prefix = "RUNEGATE_SAVE_RESTART_E2E_FAILED";
            }
            else if (systemFlowMode)
            {
                prefix = "RUNEGATE_SYSTEM_FLOWS_E2E_FAILED";
            }
            else if (fullChapterMode)
            {
                prefix = "RUNEGATE_FULL_CHAPTER_E2E_FAILED";
            }
            else
            {
                prefix = "RUNEGATE_E2E_FAILED";
            }
            Debug.LogError($"{prefix}: {message}");
            CleanupSmokeSave();
            Time.timeScale = previousTimeScale;
            Application.Quit(1);
        }

        private static void CleanupSmokeSave()
        {
            TryDeleteFile(SaveManager.SavePath);
            TryDeleteFile(SaveManager.SavePath + ".tmp");
            TryDeleteFile(SaveManager.SavePath + ".bak");
            TryDeleteFile(SaveManager.SavePath + ".corrupt");
            TryDeleteFile(SaveManager.SavePath + ".tmp.corrupt");
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"RuneGate smoke cleanup could not delete {path}: {exception.Message}");
            }
        }

        private static StageData FindStage(IReadOnlyList<StageData> stages, int stageNumber)
        {
            if (stages == null)
            {
                return null;
            }

            for (int i = 0; i < stages.Count; i++)
            {
                StageData stage = stages[i];
                if (stage != null && PrototypeAssetLoader.GetStageNumber(stage) == stageNumber)
                {
                    return stage;
                }
            }

            return null;
        }

        private static int CountValidUpgrades(IReadOnlyList<UpgradeData> upgrades)
        {
            if (upgrades == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < upgrades.Count; i++)
            {
                if (upgrades[i] != null)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool HasCommandLineArgument(string argument)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length; i++)
            {
                if (string.Equals(arguments[i], argument, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
