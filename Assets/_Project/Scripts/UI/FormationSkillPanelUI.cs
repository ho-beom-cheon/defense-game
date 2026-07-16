using UnityEngine;

namespace RuneGate
{
    public sealed class FormationSkillPanelUI : MonoBehaviour
    {
        private const float PanelPadding = 10f;
        private const float CardGap = 8f;

        [SerializeField] private BattleManager battleManager;
        [SerializeField] private bool drawRuntimeGui = true;

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
            Rect panelRect = UIResponsiveLayout.ClampToScreen(battleFrame.SkillPanelArea);
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUI.Box(panelRect, GUIContent.none, panelStyle);

            int heroCount = battleManager != null ? battleManager.Heroes.Count : 0;
            float titleHeight = Mathf.Clamp(panelRect.height * 0.14f, 22f, 34f);
            Rect titleRect = new Rect(panelRect.x + PanelPadding, panelRect.y + 6f, panelRect.width - PanelPadding * 2f, titleHeight);
            DrawPanelTitle(titleRect, heroCount);

            if (heroCount <= 0)
            {
                GUI.Label(new Rect(titleRect.x, titleRect.yMax + 4f, titleRect.width, 30f), "\ubc30\uce58\ub41c \uc601\uc6c5\uc774 \uc5c6\uc2b5\ub2c8\ub2e4.");
                return;
            }

            Rect contentRect = new Rect(
                panelRect.x + PanelPadding,
                titleRect.yMax + 4f,
                panelRect.width - PanelPadding * 2f,
                Mathf.Max(1f, panelRect.yMax - titleRect.yMax - PanelPadding - 4f));

            for (int heroIndex = 0; heroIndex < heroCount; heroIndex++)
            {
                Rect cardRect = CalculateCardRect(contentRect, heroIndex, heroCount);
                DrawHeroSkillCard(battleManager.Heroes[heroIndex], cardRect);
            }
        }

        public static int ResolveColumnCount(float availableWidth, int itemCount)
        {
            int preferredColumns = availableWidth >= 520f ? 3 : availableWidth >= 330f ? 2 : 1;
            return Mathf.Clamp(preferredColumns, 1, Mathf.Max(1, itemCount));
        }

        public static Rect CalculateCardRect(Rect contentRect, int index, int itemCount)
        {
            int safeCount = Mathf.Max(1, itemCount);
            int columns = ResolveColumnCount(contentRect.width, safeCount);
            int rows = Mathf.Max(1, Mathf.CeilToInt(safeCount / (float)columns));
            int safeIndex = Mathf.Clamp(index, 0, safeCount - 1);
            float cardWidth = Mathf.Max(1f, (contentRect.width - CardGap * (columns - 1)) / columns);
            float cardHeight = Mathf.Max(1f, (contentRect.height - CardGap * (rows - 1)) / rows);
            int column = safeIndex % columns;
            int row = safeIndex / columns;
            return new Rect(
                contentRect.x + column * (cardWidth + CardGap),
                contentRect.y + row * (cardHeight + CardGap),
                cardWidth,
                cardHeight);
        }

        private void DrawPanelTitle(Rect rect, int heroCount)
        {
            int readyCount = 0;
            if (battleManager != null && battleManager.CurrentState == BattleState.WaveRunning)
            {
                for (int i = 0; i < battleManager.Heroes.Count; i++)
                {
                    SkillController skill = battleManager.Heroes[i] != null ? battleManager.Heroes[i].SkillController : null;
                    if (skill != null && skill.CanUseSkill)
                    {
                        readyCount++;
                    }
                }
            }

            GUIStyle titleStyle = CreateLabelStyle(TextAnchor.MiddleLeft, true, 14f);
            GUI.Label(rect, $"\uc601\uc6c5 \uc2a4\ud0ac   {readyCount}/{heroCount} \uc900\ube44", titleStyle);
        }

        private void DrawHeroSkillCard(HeroController hero, Rect cardRect)
        {
            if (hero == null || hero.Data == null || cardRect.width <= 1f || cardRect.height <= 1f)
            {
                return;
            }

            SkillController skillController = hero.SkillController;
            bool hasSkill = skillController != null && skillController.Data != null;
            bool canUse = battleManager != null && battleManager.CurrentState == BattleState.WaveRunning && hasSkill && skillController.CanUseSkill;
            float cooldownRemaining = hasSkill ? skillController.CooldownRemaining : 0f;
            float cooldownDuration = hasSkill ? Mathf.Max(0.01f, skillController.CooldownDuration) : 1f;
            float cooldownRatio = Mathf.Clamp01(cooldownRemaining / cooldownDuration);

            Color previousBackground = GUI.backgroundColor;
            bool previousEnabled = GUI.enabled;
            GUI.backgroundColor = canUse
                ? new Color(0.58f, 0.92f, 0.66f, 1f)
                : cooldownRemaining > 0f
                    ? new Color(0.46f, 0.58f, 0.72f, 1f)
                    : new Color(0.48f, 0.50f, 0.50f, 1f);
            GUI.enabled = canUse;
            GUIStyle buttonStyle = RuntimePixelGuiUtility.CreateButtonStyle(GUI.skin.button, RuntimePixelAssetLoader.UiButtonSkill);
            bool pressed = GUI.Button(cardRect, GUIContent.none, buttonStyle);
            GUI.enabled = previousEnabled;
            GUI.backgroundColor = previousBackground;

            if (cooldownRatio > 0f)
            {
                Rect cooldownOverlay = new Rect(
                    cardRect.x + 3f,
                    cardRect.y + 3f + (cardRect.height - 6f) * (1f - cooldownRatio),
                    cardRect.width - 6f,
                    (cardRect.height - 6f) * cooldownRatio);
                DrawSolidRect(cooldownOverlay, new Color(0.02f, 0.04f, 0.07f, 0.62f));
            }

            bool compact = cardRect.height < 86f;
            float inset = compact ? 5f : 8f;
            float cooldownBarHeight = compact ? 4f : 7f;
            float iconSize = Mathf.Min(
                compact ? 42f : 78f,
                cardRect.width * (compact ? 0.22f : 0.28f),
                cardRect.height - inset * 2f - cooldownBarHeight);
            Rect iconRect = new Rect(cardRect.x + inset, cardRect.y + inset, iconSize, Mathf.Max(1f, iconSize));
            DrawSolidRect(iconRect, new Color(0.025f, 0.07f, 0.08f, 0.92f));
            DrawSprite(iconRect, hero.Data.BattleSprite);

            float hpRatio = hero.MaxHp > 0 ? Mathf.Clamp01(hero.CurrentHp / (float)hero.MaxHp) : 0f;
            Rect hpBarRect = new Rect(iconRect.x, iconRect.yMax - (compact ? 4f : 6f), iconRect.width, compact ? 4f : 6f);
            DrawProgressBar(hpBarRect, hpRatio, new Color(0.20f, 0.78f, 0.38f, 1f));

            string skillName = hasSkill ? GameTextMapper.SkillName(skillController.Data) : "\uc2a4\ud0ac \uc5c6\uc74c";
            string status = GameTextMapper.SkillStatus(battleManager != null ? battleManager.CurrentState : BattleState.None, cooldownRemaining, hasSkill);
            float textX = iconRect.xMax + (compact ? 5f : 8f);
            float textWidth = Mathf.Max(1f, cardRect.xMax - textX - inset);
            Rect textRect = new Rect(textX, cardRect.y + inset, textWidth, cardRect.height - inset * 2f - cooldownBarHeight);
            DrawCardText(textRect, hero, skillName, status, compact, canUse);

            if (cooldownRemaining > 0f)
            {
                GUIStyle cooldownStyle = CreateLabelStyle(TextAnchor.MiddleCenter, true, compact ? 15f : 22f);
                cooldownStyle.normal.textColor = Color.white;
                GUI.Label(iconRect, Mathf.CeilToInt(cooldownRemaining).ToString(), cooldownStyle);
            }

            Rect cooldownBarRect = new Rect(cardRect.x + inset, cardRect.yMax - cooldownBarHeight - 3f, cardRect.width - inset * 2f, cooldownBarHeight);
            float cooldownReadyRatio = hasSkill ? 1f - cooldownRatio : 0f;
            DrawProgressBar(cooldownBarRect, cooldownReadyRatio, canUse
                ? new Color(0.30f, 0.95f, 0.48f, 1f)
                : new Color(0.30f, 0.58f, 0.92f, 1f));

            DrawSolidRect(new Rect(cardRect.x + 3f, cardRect.y + 3f, cardRect.width - 6f, compact ? 3f : 5f),
                canUse ? new Color(0.42f, 1f, 0.55f, 0.95f) : new Color(0.30f, 0.40f, 0.46f, 0.82f));

            if (pressed && canUse)
            {
                hero.RequestManualSkill();
            }
        }

        private static void DrawCardText(Rect rect, HeroController hero, string skillName, string status, bool compact, bool canUse)
        {
            GUIStyle heroStyle = CreateLabelStyle(TextAnchor.MiddleLeft, true, compact ? 11f : 14f);
            GUIStyle skillStyle = CreateLabelStyle(TextAnchor.MiddleLeft, false, compact ? 10f : 13f);
            GUIStyle statusStyle = CreateLabelStyle(TextAnchor.MiddleLeft, true, compact ? 9f : 11f);
            statusStyle.normal.textColor = canUse ? new Color(0.52f, 1f, 0.62f, 1f) : new Color(0.75f, 0.84f, 0.90f, 1f);

            if (compact)
            {
                float lineHeight = rect.height * 0.5f;
                GUI.Label(new Rect(rect.x, rect.y, rect.width, lineHeight), hero.Data.DisplayNameKorean, heroStyle);
                GUI.Label(new Rect(rect.x, rect.y + lineHeight, rect.width, rect.height - lineHeight), $"{skillName} \u00b7 {status}", skillStyle);
                return;
            }

            float heroHeight = rect.height * 0.27f;
            float skillHeight = rect.height * 0.25f;
            float hpHeight = rect.height * 0.22f;
            GUI.Label(new Rect(rect.x, rect.y, rect.width, heroHeight), hero.Data.DisplayNameKorean, heroStyle);
            GUI.Label(new Rect(rect.x, rect.y + heroHeight, rect.width, skillHeight), skillName, skillStyle);
            GUI.Label(new Rect(rect.x, rect.y + heroHeight + skillHeight, rect.width, hpHeight), $"HP {hero.CurrentHp}/{hero.MaxHp}", skillStyle);
            GUI.Label(new Rect(rect.x, rect.y + heroHeight + skillHeight + hpHeight, rect.width, rect.height - heroHeight - skillHeight - hpHeight), status, statusStyle);
        }

        private static void DrawSprite(Rect rect, Sprite sprite)
        {
            if (sprite == null || sprite.texture == null)
            {
                return;
            }

            Rect textureRect = sprite.textureRect;
            Rect uv = new Rect(
                textureRect.x / sprite.texture.width,
                textureRect.y / sprite.texture.height,
                textureRect.width / sprite.texture.width,
                textureRect.height / sprite.texture.height);
            GUI.DrawTextureWithTexCoords(rect, sprite.texture, uv, true);
        }

        private static void DrawProgressBar(Rect rect, float ratio, Color fillColor)
        {
            DrawSolidRect(rect, new Color(0.035f, 0.045f, 0.05f, 0.94f));
            if (ratio <= 0f)
            {
                return;
            }

            Rect fillRect = new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(ratio), rect.height);
            DrawSolidRect(fillRect, fillColor);
        }

        private static void DrawSolidRect(Rect rect, Color color)
        {
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
            GUI.color = previousColor;
        }

        private static GUIStyle CreateLabelStyle(TextAnchor alignment, bool bold, float baseFontSize)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = alignment,
                fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
                fontSize = Mathf.RoundToInt(baseFontSize * UIResponsiveLayout.ReadabilityScale),
                clipping = TextClipping.Clip,
                wordWrap = false,
                padding = new RectOffset(0, 0, 0, 0)
            };
            KoreanFontManager.ApplyFont(style);
            return style;
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
