using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RuneGate
{
    public sealed class BattleSkillCardView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image portrait;
        [SerializeField] private Image healthFill;
        [SerializeField] private Image cooldownFill;
        [SerializeField] private Image readyGlow;
        [SerializeField] private TMP_Text heroName;
        [SerializeField] private TMP_Text skillName;
        [SerializeField] private TMP_Text status;
        [SerializeField] private TMP_Text cooldownText;

        private BattleManager battleManager;
        private HeroController hero;
        private SkillController skill;
        private RuneGateUiTheme theme;
        private bool wasReady;
        private float readyPulseUntil;

        public HeroController Hero => hero;

        public void ConfigureView(Button targetButton, Image targetPortrait, Image targetHealthFill, Image targetCooldownFill,
            Image targetReadyGlow, TMP_Text targetHeroName, TMP_Text targetSkillName, TMP_Text targetStatus, TMP_Text targetCooldownText)
        {
            button = targetButton;
            portrait = targetPortrait;
            healthFill = targetHealthFill;
            cooldownFill = targetCooldownFill;
            readyGlow = targetReadyGlow;
            heroName = targetHeroName;
            skillName = targetSkillName;
            status = targetStatus;
            cooldownText = targetCooldownText;
        }

        public void Bind(BattleManager manager, HeroController targetHero, RuneGateUiTheme uiTheme)
        {
            Unbind();
            battleManager = manager;
            hero = targetHero;
            theme = uiTheme;
            skill = hero != null ? hero.SkillController : null;

            if (button != null)
            {
                button.onClick.RemoveListener(Press);
                button.onClick.AddListener(Press);
            }

            if (hero != null)
            {
                hero.HpChanged += HandleHpChanged;
            }

            if (skill != null)
            {
                skill.CooldownChanged += HandleCooldownChanged;
            }

            ApplyStaticContent();
            Refresh();
        }

        private void OnDisable()
        {
            if (!gameObject.scene.isLoaded)
            {
                return;
            }

            Unbind();
        }

        private void Update()
        {
            Refresh();
        }

        private void ApplyStaticContent()
        {
            if (heroName != null)
            {
                heroName.text = hero != null && hero.Data != null ? hero.Data.DisplayNameKorean : "영웅";
            }

            if (skillName != null)
            {
                skillName.text = skill != null && skill.Data != null ? GameTextMapper.SkillName(skill.Data) : "스킬 없음";
            }

            if (portrait != null)
            {
                portrait.sprite = hero != null && hero.Data != null ? hero.Data.BattleSprite : null;
                portrait.enabled = portrait.sprite != null;
                portrait.preserveAspect = true;
            }
        }

        private void Refresh()
        {
            bool waveRunning = battleManager != null && battleManager.CurrentState == BattleState.WaveRunning;
            bool ready = waveRunning && hero != null && hero.IsAlive && skill != null && skill.CanUseSkill;
            if (button != null)
            {
                button.interactable = ready;
            }

            float healthRatio = hero != null && hero.MaxHp > 0 ? hero.CurrentHp / (float)hero.MaxHp : 0f;
            SetFill(healthFill, healthRatio);

            float duration = skill != null ? Mathf.Max(0.01f, skill.CooldownDuration) : 1f;
            float remaining = skill != null ? Mathf.Max(0f, skill.CooldownRemaining) : 0f;
            SetFill(cooldownFill, skill != null ? 1f - remaining / duration : 0f);

            if (status != null)
            {
                status.text = GameTextMapper.SkillStatus(battleManager != null ? battleManager.CurrentState : BattleState.None, remaining, skill != null && skill.Data != null);
                status.color = ready && theme != null ? theme.Success : theme != null ? theme.MutedText : Color.gray;
            }

            if (cooldownText != null)
            {
                cooldownText.text = remaining > 0.05f ? Mathf.CeilToInt(remaining).ToString() : "준비";
                cooldownText.color = ready && theme != null ? theme.Success : theme != null ? theme.PrimaryText : Color.white;
            }

            if (ready && !wasReady)
            {
                readyPulseUntil = Time.unscaledTime + 0.8f;
            }

            wasReady = ready;
            if (readyGlow != null)
            {
                float alpha = ready ? 0.45f : 0f;
                if (ready && Time.unscaledTime < readyPulseUntil)
                {
                    alpha = Mathf.Lerp(0.24f, 0.85f, 0.5f + Mathf.Sin(Time.unscaledTime * 12f) * 0.5f);
                }

                Color color = theme != null ? theme.RuneBlue : Color.cyan;
                color.a = alpha;
                readyGlow.color = color;
            }
        }

        private void Press()
        {
            if (button != null && button.interactable)
            {
                hero?.RequestManualSkill();
            }
        }

        private void HandleHpChanged(int current, int maximum)
        {
            SetFill(healthFill, maximum > 0 ? current / (float)maximum : 0f);
        }

        private void HandleCooldownChanged(float remaining, float duration)
        {
            SetFill(cooldownFill, duration > 0f ? 1f - remaining / duration : 0f);
        }

        private void Unbind()
        {
            if (hero != null)
            {
                hero.HpChanged -= HandleHpChanged;
            }

            if (skill != null)
            {
                skill.CooldownChanged -= HandleCooldownChanged;
            }

            if (button != null)
            {
                button.onClick.RemoveListener(Press);
            }

            hero = null;
            skill = null;
        }

        private static void SetFill(Image image, float ratio)
        {
            if (image != null)
            {
                image.fillAmount = Mathf.Clamp01(ratio);
            }
        }
    }
}
