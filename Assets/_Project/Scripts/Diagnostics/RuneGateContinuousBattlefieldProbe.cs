using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuneGate
{
    public sealed class RuneGateContinuousBattlefieldProbe : MonoBehaviour
    {
        private const string ProbeArgument = "-runegateContinuousProbe";
        private const float ProbeDuration = 14f;
        private const float Timeout = 20f;

        private float minUnitY = float.MaxValue;
        private float maxUnitY = float.MinValue;
        private float maxHeroVerticalTravel;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!HasArgument(ProbeArgument) || FindAnyObjectByType<RuneGateContinuousBattlefieldProbe>() != null)
            {
                return;
            }

            GameObject probeObject = new GameObject(nameof(RuneGateContinuousBattlefieldProbe));
            DontDestroyOnLoad(probeObject);
            probeObject.AddComponent<RuneGateContinuousBattlefieldProbe>();
        }

        private IEnumerator Start()
        {
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 0;
            Time.timeScale = 1f;

            List<StageData> stages = PrototypeAssetLoader.LoadStages();
            if (!Require(stages != null && stages.Count > 0 && stages[0] != null, "Stage 1 data is missing."))
            {
                yield break;
            }

            SaveManager.ResetSave();
            SaveManager.MarkTutorialSeen();
            GameSession.SelectDifficulty("normal");
            GameSession.SelectStage(stages[0], GameSession.ResolveNextStageId(stages[0].StageId, stages));
            SceneManager.LoadScene("BattleScene");

            float waitStartedAt = Time.realtimeSinceStartup;
            BattleManager battle = null;
            BattlefieldSpaceController space = null;
            WaveManager waves = null;
            CrystalApproachPointProvider approaches = null;
            while (Time.realtimeSinceStartup - waitStartedAt < Timeout)
            {
                battle = FindAnyObjectByType<BattleManager>();
                space = FindAnyObjectByType<BattlefieldSpaceController>();
                waves = FindAnyObjectByType<WaveManager>();
                approaches = FindAnyObjectByType<CrystalApproachPointProvider>();
                if (battle != null &&
                    battle.ActiveStageData != null &&
                    battle.BattlefieldMode == BattlefieldMode.Continuous2D &&
                    space != null &&
                    space.IsReady &&
                    waves != null &&
                    approaches != null &&
                    approaches.IsReady)
                {
                    break;
                }

                yield return null;
            }

            if (!Require(
                battle != null && battle.ActiveStageData != null &&
                space != null && space.IsReady &&
                waves != null &&
                approaches != null && approaches.IsReady,
                "BattleScene continuous services did not initialize."))
            {
                yield break;
            }

            float probeStartedAt = Time.realtimeSinceStartup;
            int maxUsedApproaches = 0;
            while (Time.realtimeSinceStartup - probeStartedAt < ProbeDuration)
            {
                SampleUnits();
                maxUsedApproaches = Mathf.Max(maxUsedApproaches, waves.UsedApproachPointCount);
                if ((battle.CurrentState == BattleState.Victory || battle.CurrentState == BattleState.Defeat) &&
                    Time.realtimeSinceStartup - probeStartedAt > 3f)
                {
                    break;
                }

                yield return null;
            }

            Rect playable = space.CurrentBounds.PlayableRect;
            float usedHeightRatio = playable.height > 0.01f
                ? Mathf.Max(0f, maxUnitY - minUnitY) / playable.height
                : 0f;
            float heroTravelRatio = playable.height > 0.01f ? maxHeroVerticalTravel / playable.height : 0f;
            bool passed = true;
            passed &= Check(usedHeightRatio >= 0.6f, $"Unit Y usage was {usedHeightRatio:P1}; expected at least 60%.");
            passed &= Check(heroTravelRatio >= 0.2f, $"Hero vertical travel was {heroTravelRatio:P1}; expected at least 20%.");
            passed &= Check(maxUsedApproaches >= 4, $"Only {maxUsedApproaches} crystal approach points were used; expected at least 4.");

            waves.StopCurrentWave(true);
            yield return null;
            yield return null;
            passed &= Check(approaches.ActiveReservationCount == 0, $"Approach reservations remained after stop: {approaches.ActiveReservationCount}.");

            if (passed)
            {
                Debug.Log(
                    $"RUNEGATE_CONTINUOUS_BATTLEFIELD_PROBE_PASSED: YUsage={usedHeightRatio:P1}, " +
                    $"HeroTravel={heroTravelRatio:P1}, ApproachPoints={maxUsedApproaches}, Reservations=0.");
                Application.Quit(0);
                yield break;
            }

            Debug.LogError(
                $"RUNEGATE_CONTINUOUS_BATTLEFIELD_PROBE_SUMMARY: YUsage={usedHeightRatio:P1}, " +
                $"HeroTravel={heroTravelRatio:P1}, ApproachPoints={maxUsedApproaches}, " +
                $"Reservations={approaches.ActiveReservationCount}.");
            Application.Quit(1);
        }

        private void SampleUnits()
        {
            IReadOnlyList<HeroController> heroes = HeroController.ActiveHeroes;
            for (int i = 0; i < heroes.Count; i++)
            {
                HeroController hero = heroes[i];
                if (hero == null || !hero.IsAlive)
                {
                    continue;
                }

                float y = hero.transform.position.y;
                minUnitY = Mathf.Min(minUnitY, y);
                maxUnitY = Mathf.Max(maxUnitY, y);
                if (hero.Agent != null)
                {
                    maxHeroVerticalTravel = Mathf.Max(maxHeroVerticalTravel, Mathf.Abs(y - hero.Agent.Anchor.y));
                }
            }

            IReadOnlyList<MonsterController> monsters = MonsterController.ActiveMonsters;
            for (int i = 0; i < monsters.Count; i++)
            {
                MonsterController monster = monsters[i];
                if (monster == null || !monster.IsAlive)
                {
                    continue;
                }

                float y = monster.transform.position.y;
                minUnitY = Mathf.Min(minUnitY, y);
                maxUnitY = Mathf.Max(maxUnitY, y);
            }
        }

        private static bool Require(bool condition, string message)
        {
            if (condition)
            {
                return true;
            }

            Debug.LogError($"RUNEGATE_CONTINUOUS_BATTLEFIELD_PROBE_FAILED: {message}");
            Application.Quit(1);
            return false;
        }

        private static bool Check(bool condition, string message)
        {
            if (condition)
            {
                return true;
            }

            Debug.LogError($"RUNEGATE_CONTINUOUS_BATTLEFIELD_PROBE_FAILED: {message}");
            return false;
        }

        private static bool HasArgument(string argument)
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
