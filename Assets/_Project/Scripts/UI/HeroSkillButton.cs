using UnityEngine;

namespace RuneGate
{
    public sealed class HeroSkillButton : MonoBehaviour
    {
        [SerializeField] private HeroController heroController;

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
    }
}
