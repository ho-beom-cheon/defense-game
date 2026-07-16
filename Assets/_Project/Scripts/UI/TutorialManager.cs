using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class TutorialManager : MonoBehaviour
    {
        [SerializeField] private bool showOnlyOnce = true;
        [SerializeField] private bool pauseGameWhileVisible = true;
        [SerializeField] private List<TutorialStepData> steps = new List<TutorialStepData>();

        private int currentStepIndex;
        private bool visible;
        private float previousTimeScale = 1f;

        public event Action<bool> VisibilityChanged;
        public event Action<int, TutorialStepData> StepChanged;

        public bool IsVisible => visible;
        public TutorialStepData CurrentStep => visible && currentStepIndex >= 0 && currentStepIndex < steps.Count ? steps[currentStepIndex] : null;
        public int CurrentStepIndex => currentStepIndex;
        public int CurrentStepNumber => currentStepIndex + 1;
        public int StepCount => steps.Count;

        private void Awake()
        {
            EnsureDefaultSteps();
        }

        private void Start()
        {
            if (showOnlyOnce && SaveManager.HasSeenTutorial())
            {
                return;
            }

            Show();
        }

        private void OnDisable()
        {
            RestoreTimeScale();
        }

        public void Show()
        {
            if (steps.Count == 0)
            {
                return;
            }

            currentStepIndex = 0;
            visible = true;
            if (pauseGameWhileVisible)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }

            VisibilityChanged?.Invoke(true);
            StepChanged?.Invoke(currentStepIndex, CurrentStep);
        }

        public void Next()
        {
            if (!visible)
            {
                return;
            }

            currentStepIndex++;
            if (currentStepIndex >= steps.Count)
            {
                Complete();
                return;
            }

            StepChanged?.Invoke(currentStepIndex, CurrentStep);
        }

        public void Previous()
        {
            if (!visible || currentStepIndex <= 0)
            {
                return;
            }

            currentStepIndex--;
            StepChanged?.Invoke(currentStepIndex, CurrentStep);
        }

        public void Skip()
        {
            Complete();
        }

        private void Complete()
        {
            if (!visible)
            {
                return;
            }

            visible = false;
            SaveManager.MarkTutorialSeen();
            RestoreTimeScale();
            VisibilityChanged?.Invoke(false);
        }

        private void RestoreTimeScale()
        {
            if (pauseGameWhileVisible && Mathf.Approximately(Time.timeScale, 0f))
            {
                Time.timeScale = Mathf.Max(0.01f, previousTimeScale);
            }
        }

        private void EnsureDefaultSteps()
        {
            if (steps.Count > 0)
            {
                return;
            }

            steps.Add(new TutorialStepData("\ud06c\ub9ac\uc2a4\ud0c8 \ubc29\uc5b4", "\ubaac\uc2a4\ud130\uac00 \uc67c\ucabd \ud06c\ub9ac\uc2a4\ud0c8\uc5d0 \ub3c4\ub2ec\ud558\uba74 \ud53c\ud574\ub97c \uc90d\ub2c8\ub2e4. \ud06c\ub9ac\uc2a4\ud0c8 HP\uac00 0\uc774 \ub418\uba74 \ud328\ubc30\ud569\ub2c8\ub2e4."));
            steps.Add(new TutorialStepData("\ub77c\uc778 \ud655\uc778", "\ubaac\uc2a4\ud130\ub294 \uc138 \uac1c\uc758 \ub77c\uc778\uc744 \ub530\ub77c \uc774\ub3d9\ud569\ub2c8\ub2e4. \ud3b8\uc131\ub41c \uc601\uc6c5\uc774 \uac01 \ub77c\uc778\uc744 \uc790\ub3d9\uc73c\ub85c \ubc29\uc5b4\ud569\ub2c8\ub2e4."));
            steps.Add(new TutorialStepData("\uc790\ub3d9 \uacf5\uaca9", "\ub808\uc628, \uc138\ub9ac\uc544, \uce74\uc5d8, \ubbf8\ub808\uc544, \ube0c\ub86c, \ub2c9\uc2a4\ub294 \ubcc4\ub3c4 \uc785\ub825 \uc5c6\uc774 \uc790\ub3d9\uc73c\ub85c \uacf5\uaca9\ud569\ub2c8\ub2e4."));
            steps.Add(new TutorialStepData("\uc2a4\ud0ac \uc0ac\uc6a9", "\uc2a4\ud0ac \ubc84\ud2bc\uc740 \uc7ac\uc0ac\uc6a9 \ub300\uae30\uc2dc\uac04\uc774 \ub05d\ub098\uba74 \uc0ac\uc6a9\ud560 \uc218 \uc788\uc2b5\ub2c8\ub2e4. \uc704\uae30 \ub77c\uc778\uc5d0 \ub9de\ucdb0 \ub204\ub974\uc138\uc694."));
            steps.Add(new TutorialStepData("\ub8ec \uc120\ud0dd", "\ubcf4\uc2a4\uac00 \uc544\ub2cc \uc6e8\uc774\ube0c\uac00 \ub05d\ub098\uba74 \uc138 \uac1c\uc758 \ub8ec \uc911 \ud558\ub098\ub97c \uc120\ud0dd\ud574 \uc774\ubc88 \uc804\ud22c\ub97c \uac15\ud654\ud569\ub2c8\ub2e4."));
            steps.Add(new TutorialStepData("\uc2b9\ub9ac\uc640 \ud574\uae08", "\uc2b9\ub9ac\ud558\uba74 \uace8\ub4dc\ub97c \uc5bb\uace0 \ub2e4\uc74c \uc2a4\ud14c\uc774\uc9c0\uac00 \ud574\uae08\ub429\ub2c8\ub2e4."));
            steps.Add(new TutorialStepData("\uc5c5\uadf8\ub808\uc774\ub4dc", "\uace8\ub4dc\ub85c \uc601\uad6c \uc5c5\uadf8\ub808\uc774\ub4dc\ub97c \uad6c\ub9e4\ud55c \ub4a4 \uc2a4\ud14c\uc774\uc9c0 \uc120\ud0dd\uc73c\ub85c \ub3cc\uc544\uac00\uc138\uc694."));
        }
    }
}
