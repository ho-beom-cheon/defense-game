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

            steps.Add(new TutorialStepData("크리스탈 방어", "몬스터가 왼쪽 크리스탈에 도달하면 피해를 줍니다. 크리스탈 HP가 0이 되면 패배합니다."));
            steps.Add(new TutorialStepData("라인 확인", "몬스터는 세 개의 라인을 따라 이동합니다. 편성된 영웅이 각 라인을 자동으로 방어합니다."));
            steps.Add(new TutorialStepData("자동 공격", "레온, 세리아, 카엘, 미레아, 브롬, 닉스는 별도 입력 없이 자동 공격합니다."));
            steps.Add(new TutorialStepData("스킬 사용", "스킬 버튼은 재사용 대기시간이 끝나면 사용할 수 있습니다. 위기 라인에 맞춰 누르세요."));
            steps.Add(new TutorialStepData("룬 선택", "보스가 아닌 웨이브가 끝나면 세 개의 룬 중 하나를 선택해 이번 전투를 강화합니다."));
            steps.Add(new TutorialStepData("승리와 해금", "승리하면 골드를 얻고 다음 스테이지가 해금됩니다."));
            steps.Add(new TutorialStepData("업그레이드", "골드로 영구 업그레이드를 구매한 뒤 스테이지 선택으로 돌아가세요."));
        }
    }
}
