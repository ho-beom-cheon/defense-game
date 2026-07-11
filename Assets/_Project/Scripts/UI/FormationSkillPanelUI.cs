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

            UIResponsiveLayout.ApplyReadableDefaults();
            AutoAssignReferences();

            BattleFrameRects battleFrame = GameFrameLayout.BattleFrame();
            Rect drawRect = battleFrame.SkillPanelArea;
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUILayout.BeginArea(drawRect, panelStyle);
            GUI.SetNextControlName("BattleFrame_SkillPanel");
            GUILayout.Label("\uc601\uc6c5 \uc2a4\ud0ac");

            if (battleManager == null || battleManager.Heroes.Count == 0)
            {
                GUILayout.Label("\ubc30\uce58\ub41c \uc601\uc6c5\uc774 \uc5c6\uc2b5\ub2c8\ub2e4.");
                GUILayout.EndArea();
                return;
            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(Mathf.Max(100f, drawRect.height - 40f)));
            int columns = Mathf.Clamp(Mathf.FloorToInt((drawRect.width - 24f) / 190f), 1, 3);
            float buttonWidth = Mathf.Max(150f, (drawRect.width - 28f - UIResponsiveLayout.SmallGap * (columns - 1)) / columns);
            for (int startIndex = 0; startIndex < battleManager.Heroes.Count; startIndex += columns)
            {
                GUILayout.BeginHorizontal();
                for (int column = 0; column < columns; column++)
                {
                    int heroIndex = startIndex + column;
                    if (heroIndex < battleManager.Heroes.Count)
                    {
                        DrawHeroSkillButton(battleManager.Heroes[heroIndex], buttonWidth);
                    }
                    else
                    {
                        GUILayout.Space(buttonWidth);
                    }
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawHeroSkillButton(HeroController hero, float buttonWidth)
        {
            if (hero == null || hero.Data == null)
            {
                return;
            }

            SkillController skillController = hero.SkillController;
            bool hasSkill = skillController != null && skillController.Data != null;
            string skillName = hasSkill ? GameTextMapper.SkillName(skillController.Data) : "\uc2a4\ud0ac";
            string status = GameTextMapper.SkillStatus(battleManager.CurrentState, hasSkill ? skillController.CooldownRemaining : 0f, hasSkill);
            bool canUse = battleManager.CurrentState == BattleState.WaveRunning && skillController != null && skillController.CanUseSkill;

            Color previousColor = GUI.backgroundColor;
            GUI.backgroundColor = canUse ? new Color(0.75f, 1f, 0.75f, 1f) : new Color(0.65f, 0.65f, 0.65f, 1f);
            GUI.enabled = canUse;
            GUIStyle buttonStyle = RuntimePixelGuiUtility.CreateButtonStyle(GUI.skin.button, RuntimePixelAssetLoader.UiButtonSkill);

            if (GUILayout.Button($"{hero.Data.DisplayNameKorean}: {skillName}\n{status}", buttonStyle, GUILayout.Width(buttonWidth), GUILayout.Height(64f)))
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
