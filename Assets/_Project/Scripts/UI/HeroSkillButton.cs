using UnityEngine;

namespace RuneGate
{
    public sealed class HeroSkillButton : MonoBehaviour
    {
        [SerializeField] private HeroController heroController;
        [SerializeField] private bool drawRuntimeGui;
        [SerializeField] private Rect buttonRect = new Rect(16f, 176f, 160f, 44f);

        private float cooldownRemaining;
        private float cooldownDuration;
        private bool isInteractable;

        public float CooldownRemaining => cooldownRemaining;
        public float CooldownDuration => cooldownDuration;
        public bool IsInteractable => isInteractable;

        private void OnEnable()
        {
            Bind(heroController);
        }

        private void OnDisable()
        {
            if (heroController != null && heroController.SkillController != null)
            {
                heroController.SkillController.CooldownChanged -= HandleCooldownChanged;
            }
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
            string label = isInteractable
                ? $"{heroName}: {skillName}"
                : $"{heroName}: {cooldownRemaining:0.0}s";

            GUI.enabled = isInteractable;
            if (GUI.Button(buttonRect, label))
            {
                Press();
            }

            GUI.enabled = true;
        }
    }
}
