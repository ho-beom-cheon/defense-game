using UnityEngine;

namespace RuneGate
{
    public sealed class FormationSkillPanelUI : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private Rect panelRect = new Rect(16f, 230f, 230f, 250f);

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
            GUILayout.BeginArea(panelRect, GUI.skin.box);
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

            GUI.enabled = canUse;
            if (GUILayout.Button($"{hero.Data.DisplayName}: {skillName}\n{status}", GUILayout.Height(48f)))
            {
                hero.RequestManualSkill();
            }

            GUI.enabled = true;
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
