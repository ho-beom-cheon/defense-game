using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuneGate
{
    public sealed class BattlePauseController : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private string stageSelectSceneName = "StageSelectScene";
        [SerializeField] private bool pauseOnMobileFocusLoss = true;

        private bool isPaused;
        private bool pausedByLifecycle;
        private bool sceneTransitionRequested;
        private float resumeTimeScale = 1f;

        public event Action<bool> PauseChanged;

        public bool IsPaused => isPaused;
        public bool PausedByLifecycle => pausedByLifecycle;
        public float ResumeTimeScale => resumeTimeScale;
        public bool CanPause
        {
            get
            {
                EnsureBattleManager();
                if (battleManager == null || IsTutorialVisible())
                {
                    return false;
                }

                return battleManager.CurrentState == BattleState.Preparing ||
                       battleManager.CurrentState == BattleState.WaveRunning;
            }
        }

        private void OnEnable()
        {
            EnsureBattleManager();
            BindBattleManager();
        }

        private void OnDisable()
        {
            UnbindBattleManager();
            RestoreOwnedTimeScale();
        }

        private void OnDestroy()
        {
            UnbindBattleManager();
            RestoreOwnedTimeScale();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && ShouldPauseForLifecycle())
            {
                PauseForLifecycle();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && ShouldPauseForLifecycle())
            {
                PauseForLifecycle();
            }
        }

        public void Configure(BattleManager manager)
        {
            if (battleManager == manager)
            {
                return;
            }

            UnbindBattleManager();
            battleManager = manager;
            BindBattleManager();
        }

        public bool Pause()
        {
            return PauseInternal(false);
        }

        public bool PauseForLifecycle()
        {
            return PauseInternal(true);
        }

        public void Resume()
        {
            if (!isPaused)
            {
                return;
            }

            Time.timeScale = Mathf.Max(0.01f, resumeTimeScale);
            isPaused = false;
            pausedByLifecycle = false;
            PauseChanged?.Invoke(false);
        }

        public void RestartBattle()
        {
            if (sceneTransitionRequested)
            {
                return;
            }

            sceneTransitionRequested = true;
            Resume();
            EnsureBattleManager();
            battleManager?.RestartBattle();
            sceneTransitionRequested = false;
        }

        public void OpenStageSelect()
        {
            if (sceneTransitionRequested || string.IsNullOrWhiteSpace(stageSelectSceneName))
            {
                return;
            }

            sceneTransitionRequested = true;
            Resume();
            SceneManager.LoadScene(stageSelectSceneName);
        }

        private bool PauseInternal(bool fromLifecycle)
        {
            if (isPaused)
            {
                pausedByLifecycle |= fromLifecycle;
                return true;
            }

            if (!CanPause || Time.timeScale <= 0f)
            {
                return false;
            }

            resumeTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            isPaused = true;
            pausedByLifecycle = fromLifecycle;
            PauseChanged?.Invoke(true);
            return true;
        }

        private bool ShouldPauseForLifecycle()
        {
            if (!pauseOnMobileFocusLoss || Application.isBatchMode)
            {
                return false;
            }

            return Application.isMobilePlatform && !isPaused && CanPause;
        }

        private void HandleBattleStateChanged(BattleState state)
        {
            if (!isPaused)
            {
                return;
            }

            if (state == BattleState.RuneSelection || state == BattleState.Victory || state == BattleState.Defeat)
            {
                Resume();
            }
        }

        private void EnsureBattleManager()
        {
            if (battleManager == null)
            {
                battleManager = FindAnyObjectByType<BattleManager>();
            }
        }

        private void BindBattleManager()
        {
            if (battleManager == null)
            {
                return;
            }

            battleManager.BattleStateChanged -= HandleBattleStateChanged;
            battleManager.BattleStateChanged += HandleBattleStateChanged;
        }

        private void UnbindBattleManager()
        {
            if (battleManager != null)
            {
                battleManager.BattleStateChanged -= HandleBattleStateChanged;
            }
        }

        private static bool IsTutorialVisible()
        {
            TutorialManager tutorialManager = FindAnyObjectByType<TutorialManager>();
            return tutorialManager != null && tutorialManager.IsVisible;
        }

        private void RestoreOwnedTimeScale()
        {
            if (!isPaused)
            {
                return;
            }

            if (Mathf.Approximately(Time.timeScale, 0f))
            {
                Time.timeScale = Mathf.Max(0.01f, resumeTimeScale);
            }

            isPaused = false;
            pausedByLifecycle = false;
        }
    }
}
