using UnityEngine;

namespace RuneGate
{
    public readonly struct TutorialOverlayLayoutRects
    {
        public TutorialOverlayLayoutRects(
            Rect safeArea,
            Rect focusArea,
            Rect card,
            Rect progressArea,
            Rect titleArea,
            Rect bodyArea,
            Rect footerArea)
        {
            SafeArea = safeArea;
            FocusArea = focusArea;
            Card = card;
            ProgressArea = progressArea;
            TitleArea = titleArea;
            BodyArea = bodyArea;
            FooterArea = footerArea;
        }

        public Rect SafeArea { get; }
        public Rect FocusArea { get; }
        public Rect Card { get; }
        public Rect ProgressArea { get; }
        public Rect TitleArea { get; }
        public Rect BodyArea { get; }
        public Rect FooterArea { get; }
    }

    public sealed class TutorialOverlayUI : MonoBehaviour
    {
        [SerializeField] private TutorialManager tutorialManager;
        [SerializeField] private bool drawRuntimeGui = true;

        private void OnEnable()
        {
            if (tutorialManager == null)
            {
                tutorialManager = FindAnyObjectByType<TutorialManager>();
            }
        }

        private void OnGUI()
        {
            if (!drawRuntimeGui || tutorialManager == null || !tutorialManager.IsVisible)
            {
                return;
            }

            TutorialStepData step = tutorialManager.CurrentStep;
            if (step == null)
            {
                return;
            }

            UIResponsiveLayout.ApplyReadableDefaults();
            TutorialOverlayLayoutRects layout = CalculateLayoutForSize(Screen.width, Screen.height, tutorialManager.CurrentStepIndex);
            DrawDimOutsideFocus(layout.SafeArea, layout.FocusArea);

            GUIStyle focusStyle = RuntimePixelGuiUtility.CreateSolidPanelStyle(GUI.skin.box, true);
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUIStyle buttonStyle = RuntimePixelGuiUtility.CreateSolidButtonStyle(GUI.skin.button);
            GUIStyle progressStyle = CreateLabelStyle(TextAnchor.MiddleLeft, ResolveFontSize(18, 14, 22), FontStyle.Bold);
            GUIStyle titleStyle = CreateLabelStyle(TextAnchor.MiddleLeft, ResolveFontSize(28, 20, 34), FontStyle.Bold);
            GUIStyle bodyStyle = CreateLabelStyle(TextAnchor.UpperLeft, ResolveFontSize(20, 15, 25), FontStyle.Normal);
            GUIStyle focusLabelStyle = new GUIStyle(focusStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = ResolveFontSize(17, 13, 20),
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(8, 8, 4, 4)
            };
            KoreanFontManager.ApplyFont(focusLabelStyle);

            DrawFocusFrame(layout.FocusArea, layout.SafeArea, FocusLabel(tutorialManager.CurrentStepIndex), focusLabelStyle);
            GUI.Box(layout.Card, GUIContent.none, panelStyle);
            DrawProgress(layout.ProgressArea, tutorialManager.CurrentStepNumber, tutorialManager.StepCount, progressStyle);
            GUI.Label(layout.TitleArea, step.Title, titleStyle);
            GUI.Label(layout.BodyArea, step.Body, bodyStyle);
            DrawFooter(layout.FooterArea, tutorialManager.CurrentStepNumber, tutorialManager.StepCount, buttonStyle);
        }

        public static TutorialOverlayLayoutRects CalculateLayoutForSize(float width, float height, int stepIndex)
        {
            Rect safeArea = GameFrameLayout.SafeRectForSize(width, height);
            BattleFrameRects battle = GameFrameLayout.BattleFrameForSize(width, height);
            bool portrait = height >= width;
            float gap = Mathf.Clamp(Mathf.Min(width, height) * 0.022f, 10f, 26f);
            Rect focusArea = ResolveFocusArea(battle, stepIndex, gap);

            float cardWidth = Mathf.Min(840f, safeArea.width * (portrait ? 0.92f : 0.52f));
            float targetHeight = safeArea.height * (portrait ? 0.30f : 0.54f);
            float cardHeight = Mathf.Clamp(targetHeight, 320f, portrait ? 540f : 460f);
            cardHeight = Mathf.Min(cardHeight, Mathf.Max(280f, battle.BattleFieldFrame.height - gap * 2f));
            float cardX = portrait
                ? safeArea.x + (safeArea.width - cardWidth) * 0.5f
                : safeArea.xMax - cardWidth - gap;
            float cardY;
            if (stepIndex == 3)
            {
                cardY = battle.BattleFieldFrame.y + gap;
            }
            else
            {
                cardY = battle.BattleFieldFrame.yMax - cardHeight - gap;
            }

            cardY = Mathf.Clamp(cardY, safeArea.y, Mathf.Max(safeArea.y, safeArea.yMax - cardHeight));
            Rect card = new Rect(cardX, cardY, cardWidth, cardHeight);

            float inset = Mathf.Clamp(Mathf.Min(card.width, card.height) * 0.05f, 16f, 28f);
            float internalGap = Mathf.Clamp(inset * 0.45f, 7f, 12f);
            float progressHeight = Mathf.Clamp(card.height * 0.13f, 42f, 64f);
            float titleHeight = Mathf.Clamp(card.height * 0.16f, 52f, 78f);
            float footerHeight = Mathf.Clamp(card.height * 0.17f, 56f, 76f);
            Rect progressArea = new Rect(card.x + inset, card.y + inset, card.width - inset * 2f, progressHeight);
            Rect titleArea = new Rect(progressArea.x, progressArea.yMax + internalGap, progressArea.width, titleHeight);
            Rect footerArea = new Rect(progressArea.x, card.yMax - inset - footerHeight, progressArea.width, footerHeight);
            Rect bodyArea = new Rect(
                progressArea.x,
                titleArea.yMax + internalGap,
                progressArea.width,
                Mathf.Max(1f, footerArea.y - titleArea.yMax - internalGap * 2f));

            return new TutorialOverlayLayoutRects(safeArea, focusArea, card, progressArea, titleArea, bodyArea, footerArea);
        }

        public static string FocusLabel(int stepIndex)
        {
            switch (stepIndex)
            {
                case 0:
                    return "\uc9c0\ucf1c\uc57c \ud560 \ud06c\ub9ac\uc2a4\ud0c8";
                case 1:
                    return "3\uac1c \uc804\ud22c \ub77c\uc778";
                case 2:
                    return "\uc790\ub3d9 \uc804\ud22c \uc601\uc6c5";
                case 3:
                    return "\uc9c1\uc811 \uc0ac\uc6a9\ud558\ub294 \uc601\uc6c5 \uc2a4\ud0ac";
                case 4:
                    return "\uc6e8\uc774\ube0c \uc885\ub8cc \ud6c4 \ub8ec \uae30\ub85d";
                case 5:
                    return "\uc2b9\ub9ac \ubcf4\uc0c1\uacfc \ub2e4\uc74c \uc804\uc120";
                case 6:
                    return "\uace8\ub4dc\ub85c \uc5f4\ub9ac\ub294 \uc601\uad6c \uac15\ud654";
                default:
                    return "\uc804\ud22c \uc548\ub0b4";
            }
        }

        private static Rect ResolveFocusArea(BattleFrameRects battle, int stepIndex, float gap)
        {
            Rect field = battle.BattleFieldFrame;
            switch (stepIndex)
            {
                case 0:
                    return InsetRect(new Rect(field.x, field.y, field.width * 0.22f, field.height), gap * 0.35f);
                case 1:
                    return InsetRect(field, gap * 0.35f);
                case 2:
                    return InsetRect(new Rect(field.x, field.y, field.width * 0.62f, field.height), gap * 0.35f);
                case 3:
                    return InsetRect(battle.SkillPanelArea, gap * 0.25f);
                case 4:
                case 5:
                    float width = field.width * 0.78f;
                    float height = field.height * 0.58f;
                    return new Rect(
                        field.x + (field.width - width) * 0.5f,
                        field.y + (field.height - height) * 0.5f,
                        width,
                        height);
                case 6:
                    BattleHUD.CalculateHeaderRects(
                        battle.HeaderArea,
                        out _,
                        out _,
                        out Rect battleStatusArea,
                        out _);
                    return InsetRect(battleStatusArea, gap * 0.15f);
                default:
                    return InsetRect(field, gap * 0.35f);
            }
        }

        private static Rect InsetRect(Rect rect, float inset)
        {
            return new Rect(
                rect.x + inset,
                rect.y + inset,
                Mathf.Max(1f, rect.width - inset * 2f),
                Mathf.Max(1f, rect.height - inset * 2f));
        }

        private static void DrawDimOutsideFocus(Rect safeArea, Rect focusArea)
        {
            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.70f);
            DrawSolidRect(new Rect(safeArea.x, safeArea.y, safeArea.width, Mathf.Max(0f, focusArea.y - safeArea.y)));
            DrawSolidRect(new Rect(safeArea.x, focusArea.yMax, safeArea.width, Mathf.Max(0f, safeArea.yMax - focusArea.yMax)));
            DrawSolidRect(new Rect(safeArea.x, focusArea.y, Mathf.Max(0f, focusArea.x - safeArea.x), focusArea.height));
            DrawSolidRect(new Rect(focusArea.xMax, focusArea.y, Mathf.Max(0f, safeArea.xMax - focusArea.xMax), focusArea.height));
            GUI.color = previousColor;
        }

        private static void DrawFocusFrame(Rect focusArea, Rect safeArea, string label, GUIStyle labelStyle)
        {
            float pulse = 0.74f + Mathf.Sin(Time.unscaledTime * 4f) * 0.18f;
            float thickness = Mathf.Clamp(Mathf.Min(focusArea.width, focusArea.height) * 0.012f, 3f, 8f);
            Color previousColor = GUI.color;
            GUI.color = new Color(0.92f, 0.76f, 0.30f, pulse);
            DrawSolidRect(new Rect(focusArea.x, focusArea.y, focusArea.width, thickness));
            DrawSolidRect(new Rect(focusArea.x, focusArea.yMax - thickness, focusArea.width, thickness));
            DrawSolidRect(new Rect(focusArea.x, focusArea.y, thickness, focusArea.height));
            DrawSolidRect(new Rect(focusArea.xMax - thickness, focusArea.y, thickness, focusArea.height));
            GUI.color = previousColor;

            float labelWidth = Mathf.Min(Mathf.Max(220f, focusArea.width * 0.72f), 460f);
            float labelHeight = Mathf.Clamp(safeArea.height * 0.026f, 32f, 48f);
            float labelY = focusArea.y - labelHeight - 6f;
            if (labelY < safeArea.y)
            {
                labelY = focusArea.y + 8f;
            }

            Rect labelRect = new Rect(
                Mathf.Clamp(focusArea.center.x - labelWidth * 0.5f, safeArea.x, Mathf.Max(safeArea.x, safeArea.xMax - labelWidth)),
                labelY,
                labelWidth,
                labelHeight);
            GUI.Box(labelRect, label, labelStyle);
        }

        private static void DrawProgress(Rect progressArea, int currentStep, int stepCount, GUIStyle progressStyle)
        {
            float iconSize = Mathf.Min(progressArea.height * 0.72f, 42f);
            Texture2D arrow = RuntimePixelGuiUtility.LoadTexture(RuntimePixelAssetLoader.UiTutorialArrow);
            Rect iconRect = new Rect(progressArea.x, progressArea.y + (progressArea.height - iconSize) * 0.5f, iconSize, iconSize);
            if (arrow != null)
            {
                GUI.DrawTexture(iconRect, arrow, ScaleMode.ScaleToFit, true);
            }

            float labelWidth = Mathf.Clamp(progressArea.width * 0.34f, 130f, 240f);
            Rect labelRect = new Rect(iconRect.xMax + 8f, progressArea.y, labelWidth, progressArea.height);
            GUI.Label(labelRect, $"\uc804\ud22c \uc548\ub0b4 {currentStep}/{stepCount}", progressStyle);

            int segmentCount = Mathf.Max(1, stepCount);
            float trackX = labelRect.xMax + 10f;
            float trackWidth = Mathf.Max(1f, progressArea.xMax - trackX);
            float segmentGap = 5f;
            float segmentWidth = Mathf.Max(4f, (trackWidth - segmentGap * (segmentCount - 1)) / segmentCount);
            float segmentHeight = Mathf.Clamp(progressArea.height * 0.16f, 5f, 9f);
            float segmentY = progressArea.center.y - segmentHeight * 0.5f;
            Color previousColor = GUI.color;
            for (int i = 0; i < segmentCount; i++)
            {
                GUI.color = i + 1 < currentStep
                    ? new Color(0.20f, 0.72f, 0.66f, 1f)
                    : i + 1 == currentStep
                        ? new Color(0.94f, 0.78f, 0.32f, 1f)
                        : new Color(0.22f, 0.30f, 0.32f, 1f);
                DrawSolidRect(new Rect(trackX + i * (segmentWidth + segmentGap), segmentY, segmentWidth, segmentHeight));
            }

            GUI.color = previousColor;
        }

        private void DrawFooter(Rect footerArea, int currentStep, int stepCount, GUIStyle buttonStyle)
        {
            const float gap = 8f;
            float buttonWidth = (footerArea.width - gap * 2f) / 3f;
            Rect previousRect = new Rect(footerArea.x, footerArea.y, buttonWidth, footerArea.height);
            Rect skipRect = new Rect(previousRect.xMax + gap, footerArea.y, buttonWidth, footerArea.height);
            Rect nextRect = new Rect(skipRect.xMax + gap, footerArea.y, buttonWidth, footerArea.height);

            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && currentStep > 1;
            if (GUI.Button(previousRect, "\uc774\uc804", buttonStyle))
            {
                tutorialManager.Previous();
            }

            GUI.enabled = previousEnabled;
            if (GUI.Button(skipRect, "\uac74\ub108\ub6f0\uae30", buttonStyle))
            {
                tutorialManager.Skip();
            }

            string nextLabel = currentStep >= stepCount ? "\uc804\ud22c \uc2dc\uc791" : "\ub2e4\uc74c";
            if (GUI.Button(nextRect, nextLabel, buttonStyle))
            {
                tutorialManager.Next();
            }

            Texture2D tapIndicator = RuntimePixelGuiUtility.LoadTexture(RuntimePixelAssetLoader.UiTapIndicator);
            if (tapIndicator != null)
            {
                float size = Mathf.Min(nextRect.height * 0.54f, 34f);
                Rect iconRect = new Rect(nextRect.xMax - size - 8f, nextRect.y + (nextRect.height - size) * 0.5f, size, size);
                GUI.DrawTexture(iconRect, tapIndicator, ScaleMode.ScaleToFit, true);
            }
        }

        private static void DrawSolidRect(Rect rect)
        {
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
        }

        private static GUIStyle CreateLabelStyle(TextAnchor alignment, int fontSize, FontStyle fontStyle)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = alignment,
                fontSize = fontSize,
                fontStyle = fontStyle,
                wordWrap = true,
                clipping = TextClipping.Clip
            };
            KoreanFontManager.ApplyFont(style);
            return style;
        }

        private static int ResolveFontSize(int preferred, int minimum, int maximum)
        {
            float scale = Mathf.Clamp(Mathf.Min(Screen.width, Screen.height) / 720f, 0.82f, 1.35f);
            return Mathf.Clamp(Mathf.RoundToInt(preferred * scale), minimum, maximum);
        }
    }
}
