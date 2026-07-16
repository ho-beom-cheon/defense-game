using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuneGate
{
    public sealed class RuneGateVisualCaptureRunner : MonoBehaviour
    {
        private const string CaptureArgument = "-runegateCaptureUi";
        private const string OutputArgument = "-runegateCapturePath";
        private const string DefaultSaveFileName = "runegate_save.json";
        private const float TimeoutSeconds = 15f;

        private string outputDirectory;
        private float previousTimeScale = 1f;
        private bool waitSucceeded;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!HasArgument(CaptureArgument) || FindAnyObjectByType<RuneGateVisualCaptureRunner>() != null)
            {
                return;
            }

            GameObject runnerObject = new GameObject(nameof(RuneGateVisualCaptureRunner));
            DontDestroyOnLoad(runnerObject);
            runnerObject.AddComponent<RuneGateVisualCaptureRunner>();
        }

        private IEnumerator Start()
        {
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 0;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 1f;

            if (!TryResolveOutputDirectory(out outputDirectory))
            {
                Fail("Missing or invalid -runegateCapturePath.");
                yield break;
            }

            if (string.Equals(Path.GetFileName(SaveManager.SavePath), DefaultSaveFileName, StringComparison.OrdinalIgnoreCase))
            {
                Fail("Visual capture requires an isolated -runegateSavePath.");
                yield break;
            }

            CleanupTestSave();
            SaveManager.ResetSave();
            if (!Require(SceneManager.GetActiveScene().name == "TitleScene", "Player did not start in TitleScene."))
            {
                yield break;
            }

            yield return Capture("01-title");
            if (!waitSucceeded) yield break;

            SceneManager.LoadScene("StageSelectScene");
            yield return WaitForCondition(() => SceneManager.GetActiveScene().name == "StageSelectScene" && FindAnyObjectByType<StageSelectUI>() != null);
            if (!Require(waitSucceeded, "StageSelectScene did not load.")) yield break;
            yield return Capture("02-stage-select");
            if (!waitSucceeded) yield break;

            StageSelectUI stageSelect = FindAnyObjectByType<StageSelectUI>();
            ShadowPetDefinition capturePet = ShadowContractService.PetDefinitions[0];
            SaveManager.AddMonsterShards(capturePet.MonsterId, ShadowContractService.RequiredShardCount);
            stageSelect.OpenPetContract();
            yield return new WaitForSecondsRealtime(0.25f);
            yield return Capture("02b-pet-contract");
            if (!waitSucceeded) yield break;
            stageSelect.ClosePetContract();

            StageData firstStage = FindStage(stageSelect != null ? stageSelect.Stages : null, 1);
            if (!Require(firstStage != null, "Stage 1 data is missing.")) yield break;

            SaveManager.MarkTutorialSeen();
            GameSession.SelectDifficulty("normal");
            GameSession.SelectStage(firstStage, GameSession.ResolveNextStageId(firstStage.StageId, stageSelect.Stages));
            SceneManager.LoadScene("BattleScene");
            yield return WaitForCondition(() =>
            {
                BattleManager battle = FindAnyObjectByType<BattleManager>();
                BattleCanvasController canvas = FindAnyObjectByType<BattleCanvasController>();
                return SceneManager.GetActiveScene().name == "BattleScene" && battle != null && battle.ActiveStageData != null && canvas != null && canvas.IsReady;
            });
            if (!Require(waitSucceeded, "BattleScene did not initialize.")) yield break;

            yield return new WaitForSecondsRealtime(2f);
            yield return Capture("03-battle");
            if (!waitSucceeded) yield break;

            BattlefieldVisualController battlefieldVisuals = FindAnyObjectByType<BattlefieldVisualController>();
            if (!Require(battlefieldVisuals != null && battlefieldVisuals.IsReady, "BattleScene is missing ready battlefield visuals.")) yield break;
            battlefieldVisuals.SetCrystalState(BattlefieldCrystalState.Shielded);
            yield return new WaitForSecondsRealtime(0.2f);
            yield return Capture("03a-battle-shielded");
            if (!waitSucceeded) yield break;
            battlefieldVisuals.SetCrystalState(BattlefieldCrystalState.Normal);

            battlefieldVisuals.SetRiftState(BattlefieldRiftState.WaveWarning);
            yield return new WaitForSecondsRealtime(0.2f);
            yield return Capture("03b-battle-wave-warning");
            if (!waitSucceeded) yield break;

            battlefieldVisuals.SetRiftState(BattlefieldRiftState.BossWarning);
            yield return new WaitForSecondsRealtime(0.2f);
            yield return Capture("03c-battle-boss-warning");
            if (!waitSucceeded) yield break;
            battlefieldVisuals.SetRiftState(BattlefieldRiftState.Idle);

            TutorialManager tutorial = FindAnyObjectByType<TutorialManager>();
            if (!Require(tutorial != null, "BattleScene is missing TutorialManager.")) yield break;
            tutorial.Show();
            yield return new WaitForSecondsRealtime(0.2f);
            yield return Capture("04a-tutorial-1");
            if (!waitSucceeded) yield break;
            for (int i = 0; i < 3; i++) tutorial.Next();
            yield return new WaitForSecondsRealtime(0.2f);
            yield return Capture("04b-tutorial-4");
            if (!waitSucceeded) yield break;
            for (int i = 0; i < 3; i++) tutorial.Next();
            yield return new WaitForSecondsRealtime(0.2f);
            yield return Capture("04c-tutorial-7");
            if (!waitSucceeded) yield break;
            tutorial.Skip();

            BattlePauseController pauseController = FindAnyObjectByType<BattlePauseController>();
            if (!Require(pauseController != null, "BattleScene is missing BattlePauseController.")) yield break;
            pauseController.Pause();
            yield return WaitForCondition(() => pauseController.IsPaused);
            if (!Require(waitSucceeded, "Pause UI did not appear.")) yield break;
            yield return new WaitForSecondsRealtime(0.2f);
            yield return Capture("04d-pause");
            if (!waitSucceeded) yield break;
            pauseController.Resume();

            RuneManager runeManager = FindAnyObjectByType<RuneManager>();
            if (!Require(runeManager != null, "BattleScene is missing RuneManager.")) yield break;
            runeManager.GenerateRuneOptions();
            yield return WaitForCondition(() =>
            {
                RuneSelectionUI runeSelection = FindAnyObjectByType<RuneSelectionUI>();
                return runeSelection != null && runeSelection.IsVisible;
            });
            if (!Require(waitSucceeded, "Rune Selection UI did not appear.")) yield break;
            yield return new WaitForSecondsRealtime(0.2f);
            yield return Capture("04e-rune-selection");
            if (!waitSucceeded) yield break;

            SceneManager.LoadScene("BattleScene");
            yield return WaitForBattleCanvas();
            if (!Require(waitSucceeded, "BattleScene did not reload for result capture.")) yield break;

            CrystalController crystal = FindAnyObjectByType<CrystalController>();
            if (!Require(crystal != null, "BattleScene is missing CrystalController.")) yield break;
            crystal.TakeDamage(crystal.CurrentHp);
            yield return WaitForCondition(() =>
            {
                StageResultUI result = FindAnyObjectByType<StageResultUI>();
                return result != null && result.IsVisible;
            });
            if (!Require(waitSucceeded, "Defeat Result UI did not appear.")) yield break;
            yield return new WaitForSecondsRealtime(0.45f);
            yield return Capture("05-defeat-result");
            if (!waitSucceeded) yield break;

            SceneManager.LoadScene("BattleScene");
            yield return WaitForBattleCanvas();
            if (!Require(waitSucceeded, "BattleScene did not reload for victory capture.")) yield break;
            BattleManager victoryBattle = FindAnyObjectByType<BattleManager>();
            MethodInfo finishBattle = typeof(BattleManager).GetMethod("FinishBattle", BindingFlags.Instance | BindingFlags.NonPublic);
            if (!Require(victoryBattle != null && finishBattle != null, "Victory capture could not access BattleManager result flow.")) yield break;
            finishBattle.Invoke(victoryBattle, new object[] { true, "봉문 기록 갱신. 크리스탈 방어 성공!" });
            yield return WaitForCondition(() =>
            {
                StageResultUI result = FindAnyObjectByType<StageResultUI>();
                return result != null && result.IsVisible;
            });
            if (!Require(waitSucceeded, "Victory Result UI did not appear.")) yield break;
            yield return new WaitForSecondsRealtime(0.45f);
            yield return Capture("05b-victory-result");
            if (!waitSucceeded) yield break;

            SceneManager.LoadScene("UpgradeScene");
            yield return WaitForCondition(() => SceneManager.GetActiveScene().name == "UpgradeScene" && FindAnyObjectByType<UpgradeSceneUI>() != null);
            if (!Require(waitSucceeded, "UpgradeScene did not load.")) yield break;
            yield return Capture("06-upgrade");
            if (!waitSucceeded) yield break;

            Debug.Log($"RUNEGATE_UI_CAPTURE_PASSED: {outputDirectory} ({Screen.width}x{Screen.height})");
            CleanupTestSave();
            Time.timeScale = previousTimeScale;
            Application.Quit(0);
        }

        private IEnumerator Capture(string label)
        {
            waitSucceeded = false;
            yield return null;
            yield return new WaitForEndOfFrame();

            string path = Path.Combine(outputDirectory, $"{label}-{Screen.width}x{Screen.height}.png");
            TryDelete(path);
            Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0, false);
            texture.Apply(false, false);
            byte[] png = RuneGatePngEncoder.EncodeRgb24(Screen.width, Screen.height, texture.GetPixels32());
            Destroy(texture);
            File.WriteAllBytes(path, png);

            if (File.Exists(path) && new FileInfo(path).Length > 0)
            {
                waitSucceeded = true;
                Debug.Log($"[RuneGateVisualQA] Captured {path}");
                yield break;
            }

            Fail($"Screenshot was not written: {path}");
        }

        private IEnumerator WaitForCondition(Func<bool> condition)
        {
            waitSucceeded = false;
            float startedAt = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startedAt < TimeoutSeconds)
            {
                if (condition())
                {
                    waitSucceeded = true;
                    yield break;
                }

                yield return null;
            }
        }

        private IEnumerator WaitForBattleCanvas()
        {
            yield return WaitForCondition(() =>
            {
                BattleManager battle = FindAnyObjectByType<BattleManager>();
                BattleCanvasController canvas = FindAnyObjectByType<BattleCanvasController>();
                return SceneManager.GetActiveScene().name == "BattleScene" && battle != null && battle.ActiveStageData != null && canvas != null && canvas.IsReady;
            });
        }

        private bool Require(bool condition, string message)
        {
            if (condition) return true;
            Fail(message);
            return false;
        }

        private void Fail(string message)
        {
            Debug.LogError($"RUNEGATE_UI_CAPTURE_FAILED: {message}");
            CleanupTestSave();
            Time.timeScale = previousTimeScale;
            Application.Quit(1);
        }

        private static StageData FindStage(IReadOnlyList<StageData> stages, int stageNumber)
        {
            if (stages == null) return null;
            for (int i = 0; i < stages.Count; i++)
            {
                StageData stage = stages[i];
                if (stage != null && PrototypeAssetLoader.GetStageNumber(stage) == stageNumber) return stage;
            }

            return null;
        }

        private static bool TryResolveOutputDirectory(out string directory)
        {
            string value = GetArgumentValue(OutputArgument);
            if (string.IsNullOrWhiteSpace(value))
            {
                directory = string.Empty;
                return false;
            }

            try
            {
                directory = Path.GetFullPath(value);
                Directory.CreateDirectory(directory);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Visual capture output path is invalid: {exception.Message}");
                directory = string.Empty;
                return false;
            }
        }

        private static bool HasArgument(string argument)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length; i++)
            {
                if (string.Equals(arguments[i], argument, StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }

        private static string GetArgumentValue(string argument)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length - 1; i++)
            {
                if (string.Equals(arguments[i], argument, StringComparison.OrdinalIgnoreCase)) return arguments[i + 1];
            }

            return string.Empty;
        }

        private static void CleanupTestSave()
        {
            TryDelete(SaveManager.SavePath);
            TryDelete(SaveManager.SavePath + ".tmp");
            TryDelete(SaveManager.SavePath + ".bak");
            TryDelete(SaveManager.SavePath + ".corrupt");
            TryDelete(SaveManager.SavePath + ".tmp.corrupt");
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Visual capture cleanup could not delete {path}: {exception.Message}");
            }
        }
    }
}
