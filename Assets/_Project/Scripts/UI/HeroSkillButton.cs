using UnityEngine;

namespace RuneGate
{
    public sealed class HeroSkillButton : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private HeroController heroController;
        [SerializeField] private bool drawRuntimeGui;
        [SerializeField] private Rect buttonRect = new Rect(16f, 190f, 190f, 44f);

        private float cooldownRemaining;
        private float cooldownDuration;
        private bool isInteractable;
        private BattleState battleState = BattleState.None;

        public float CooldownRemaining => cooldownRemaining;
        public float CooldownDuration => cooldownDuration;
        public bool IsInteractable => CanPressSkill();

        private void OnEnable()
        {
            if (battleManager == null)
            {
                battleManager = FindFirstObjectByType<BattleManager>();
            }

            if (battleManager != null)
            {
                battleManager.BattleStateChanged += HandleBattleStateChanged;
                battleState = battleManager.CurrentState;
            }

            Bind(heroController);
        }

        private void OnDisable()
        {
            if (battleManager != null)
            {
                battleManager.BattleStateChanged -= HandleBattleStateChanged;
            }

            if (heroController != null && heroController.SkillController != null)
            {
                heroController.SkillController.CooldownChanged -= HandleCooldownChanged;
            }
        }

        private void OnGUI()
        {
            if (!drawRuntimeGui || heroController == null)
            {
                return;
            }

            string heroName = heroController.Data != null ? heroController.Data.DisplayName : "Hero";
            string skillName = heroController.SkillController != null && heroController.SkillController.Data != null
                ? heroController.SkillController.Data.DisplayName
                : "Skill";
            string status = GetStatusText();
            string label = $"{heroName}: {skillName}\n{status}";

            Rect drawRect = buttonRect;
            drawRect.height = Mathf.Max(drawRect.height, 54f);

            GUI.enabled = CanPressSkill();
            if (GUI.Button(drawRect, label))
            {
                Press();
            }

            GUI.enabled = true;
        }

        public void Bind(HeroController hero)
        {
            if (heroController != null && heroController.SkillController != null)
            {
                heroController.SkillController.CooldownChanged -= HandleCooldownChanged;
            }

            heroController = hero;
            if (heroController == null || heroController.SkillController == null)
            {
                isInteractable = false;
                return;
            }

            heroController.SkillController.CooldownChanged += HandleCooldownChanged;
            HandleCooldownChanged(heroController.SkillController.CooldownRemaining, heroController.SkillController.CooldownDuration);
        }

        public void Press()
        {
            if (!CanPressSkill())
            {
                return;
            }

            if (heroController == null)
            {
                Debug.LogWarning("HeroSkillButton cannot trigger skill because no hero is bound.");
                return;
            }

            heroController.RequestManualSkill();
        }

        private void HandleCooldownChanged(float remaining, float duration)
        {
            cooldownRemaining = remaining;
            cooldownDuration = duration;
            isInteractable = remaining <= 0f;
        }

        private void HandleBattleStateChanged(BattleState state)
        {
            battleState = state;
        }

        private bool CanPressSkill()
        {
            return isInteractable && battleState == BattleState.WaveRunning;
        }

        private string GetStatusText()
        {
            if (battleState == BattleState.Victory || battleState == BattleState.Defeat)
            {
                return "Battle Ended";
            }

            if (battleState == BattleState.RuneSelection)
            {
                return "Rune Selection";
            }

            if (cooldownRemaining > 0f)
            {
                return $"{cooldownRemaining:0.0}s";
            }

            return "Ready";
        }
    }
}
