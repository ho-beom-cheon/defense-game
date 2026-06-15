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

        public bool IsVisible => visible;
        public TutorialStepData CurrentStep => visible && currentStepIndex >= 0 && currentStepIndex < steps.Count ? steps[currentStepIndex] : null;
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
            }
        }

        public void Skip()
        {
            Complete();
        }

        private void Complete()
        {
            visible = false;
            SaveManager.MarkTutorialSeen();
            RestoreTimeScale();
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

            steps.Add(new TutorialStepData("Protect the Crystal", "Monsters that reach the left crystal damage it. If Crystal HP reaches 0, the battle is lost."));
            steps.Add(new TutorialStepData("Watch the Lanes", "Enemies advance through three lanes. Your formation covers those lanes automatically."));
            steps.Add(new TutorialStepData("Heroes Auto Attack", "Knight, Archer, Fire Mage, Priest, Dwarf Engineer, and Assassin attack without extra input."));
            steps.Add(new TutorialStepData("Use Skills", "Skill buttons become available after cooldown. Tap them to help a lane at the right moment."));
            steps.Add(new TutorialStepData("Choose Runes", "After non-boss waves, pick one of three runes to strengthen this battle run."));
            steps.Add(new TutorialStepData("Upgrade Between Battles", "Victory awards gold and unlocks the next stage. Spend gold on permanent upgrades."));
        }
    }
}
