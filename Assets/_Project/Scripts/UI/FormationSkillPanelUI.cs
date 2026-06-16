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
            string skillName = skillController != null ? KoreanFontManager.GetSkillDisplayName(skillController.Data) : "스킬";
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
                return "전투 없음";
            }

            if (battleManager.CurrentState == BattleState.Victory || battleManager.CurrentState == BattleState.Defeat)
            {
                return "전투 종료";
            }

            if (battleManager.CurrentState == BattleState.RuneSelection)
            {
                return "룬 선택";
            }

            if (skillController == null || skillController.Data == null)
            {
                return "스킬 없음";
            }

            if (skillController.CooldownRemaining > 0f)
            {
                return $"{skillController.CooldownRemaining:0.0}s";
            }

            return "준비 완료";
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
