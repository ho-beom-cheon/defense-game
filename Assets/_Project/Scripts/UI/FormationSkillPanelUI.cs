using UnityEngine;

namespace RuneGate
{
    public sealed class FormationSkillPanelUI : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private Rect panelRect = new Rect(8f, 236f, 248f, 286f);

        private Vector2 scrollPosition;

        private void OnEnable()
        {
            AutoAssignReferences();
        }

        private void OnGUI()
        {
            if (!drawRuntimeGui)
            {
                return;
            }

            KoreanFontManager.ApplyToGuiSkin();
            AutoAssignReferences();
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUILayout.BeginArea(panelRect, panelStyle);
            GUILayout.Label("영웅 스킬");

            if (battleManager == null || battleManager.Heroes.Count == 0)
            {
                GUILayout.Label("배치된 영웅이 없습니다.");
                GUILayout.EndArea();
                return;
            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            for (int i = 0; i < battleManager.Heroes.Count; i++)
            {
                DrawHeroSkillButton(battleManager.Heroes[i]);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawHeroSkillButton(HeroController hero)
        {
            if (hero == null || hero.Data == null)
            {
                return;
            }

            SkillController skillController = hero.SkillController;
            bool hasSkill = skillController != null && skillController.Data != null;
            string skillName = hasSkill ? GameTextMapper.SkillName(skillController.Data) : "스킬";
            string status = GameTextMapper.SkillStatus(battleManager.CurrentState, hasSkill ? skillController.CooldownRemaining : 0f, hasSkill);
            bool canUse = battleManager.CurrentState == BattleState.WaveRunning && skillController != null && skillController.CanUseSkill;

            Color previousColor = GUI.backgroundColor;
            GUI.backgroundColor = canUse ? new Color(0.75f, 1f, 0.75f, 1f) : new Color(0.65f, 0.65f, 0.65f, 1f);
            GUI.enabled = canUse;
            GUIStyle buttonStyle = RuntimePixelGuiUtility.CreateButtonStyle(GUI.skin.button, RuntimePixelAssetLoader.UiButtonSkill);
            if (GUILayout.Button($"{hero.Data.DisplayNameKorean}: {skillName}\n{status}", buttonStyle, GUILayout.Height(54f)))
            {
                hero.RequestManualSkill();
            }

            GUI.enabled = true;
            GUI.backgroundColor = previousColor;
        }

        private void AutoAssignReferences()
        {
            if (battleManager == null)
            {
                battleManager = FindAnyObjectByType<BattleManager>();
            }
        }
    }
}
