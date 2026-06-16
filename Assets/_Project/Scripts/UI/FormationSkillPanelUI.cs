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

            AutoAssignReferences();
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUILayout.BeginArea(panelRect, panelStyle);
            GUILayout.Label("Hero Skills");

            if (battleManager == null || battleManager.Heroes.Count == 0)
            {
                GUILayout.Label("No heroes placed.");
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
            string skillName = skillController != null && skillController.Data != null ? skillController.Data.DisplayName : "Skill";
            string status = GetStatusText(skillController);
            bool canUse = battleManager.CurrentState == BattleState.WaveRunning && skillController != null && skillController.CanUseSkill;

            Color previousColor = GUI.backgroundColor;
            GUI.backgroundColor = canUse ? new Color(0.75f, 1f, 0.75f, 1f) : new Color(0.65f, 0.65f, 0.65f, 1f);
            GUI.enabled = canUse;
            GUIStyle buttonStyle = RuntimePixelGuiUtility.CreateButtonStyle(GUI.skin.button, RuntimePixelAssetLoader.UiButtonSkill);
            if (GUILayout.Button($"{hero.Data.DisplayNameKorean}: {skillName}\n{status}", buttonStyle, GUILayout.Height(50f)))
            {
                hero.RequestManualSkill();
            }

            GUI.enabled = true;
            GUI.backgroundColor = previousColor;
        }

        private string GetStatusText(SkillController skillController)
        {
            if (battleManager == null)
            {
                return "Battle Missing";
            }

            if (battleManager.CurrentState == BattleState.Victory || battleManager.CurrentState == BattleState.Defeat)
            {
                return "Battle Ended";
            }

            if (battleManager.CurrentState == BattleState.RuneSelection)
            {
                return "Rune Selection";
            }

            if (skillController == null || skillController.Data == null)
            {
                return "Missing Skill";
            }

            if (skillController.CooldownRemaining > 0f)
            {
                return $"{skillController.CooldownRemaining:0.0}s";
            }

            return "Ready";
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
