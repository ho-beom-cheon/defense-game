using UnityEngine;

namespace RuneGate
{
    public static class UIResponsiveLayout
    {
        public const float ReferenceWidth = 1080f;
        public const float ReferenceHeight = 1920f;

        public static float Margin => Mathf.Clamp(Mathf.Min(Screen.width, Screen.height) * 0.025f, 8f, 28f);
        public static float SmallGap => Mathf.Clamp(Margin * 0.5f, 4f, 12f);
        public static float Gap => Mathf.Clamp(Margin * 0.75f, 8f, 18f);
        public static float ReadabilityScale
        {
            get
            {
                if (!Application.isMobilePlatform)
                {
                    return 1f;
                }

                float dpiScale = Screen.dpi > 0f ? Screen.dpi / 280f : 0f;
                float resolutionScale = Mathf.Min(Screen.width, Screen.height) / 720f;
                return Mathf.Clamp(Mathf.Max(dpiScale, resolutionScale), 1f, 1.5f);
            }
        }

        public static Rect SafeRect()
        {
            Rect safeArea = Screen.safeArea;
            if (safeArea.width <= 0f || safeArea.height <= 0f)
            {
                safeArea = new Rect(0f, 0f, Screen.width, Screen.height);
            }

            safeArea = new Rect(safeArea.x, Screen.height - safeArea.yMax, safeArea.width, safeArea.height);
            float margin = Margin;
            float x = safeArea.x + margin;
            float y = safeArea.y + margin;
            float width = Mathf.Max(1f, safeArea.width - margin * 2f);
            float height = Mathf.Max(1f, safeArea.height - margin * 2f);
            return new Rect(x, y, width, height);
        }

        public static Rect MainPanel(float maxWidth = 1040f, float maxHeight = 1800f)
        {
            Rect safe = SafeRect();
            float width = Mathf.Min(safe.width, maxWidth);
            float height = Mathf.Min(safe.height, maxHeight);
            return ClampToScreen(new Rect(safe.x + (safe.width - width) * 0.5f, safe.y + (safe.height - height) * 0.5f, width, height));
        }

        public static Rect Centered(float preferredWidth, float preferredHeight, float widthRatio = 0.92f, float heightRatio = 0.82f)
        {
            Rect safe = SafeRect();
            float width = Mathf.Min(Mathf.Max(1f, preferredWidth), safe.width * Mathf.Clamp01(widthRatio));
            float height = Mathf.Min(Mathf.Max(1f, preferredHeight), safe.height * Mathf.Clamp01(heightRatio));
            return ClampToScreen(new Rect(safe.x + (safe.width - width) * 0.5f, safe.y + (safe.height - height) * 0.5f, width, height));
        }

        public static Rect LeftColumn(float preferredWidth, float preferredHeight)
        {
            Rect safe = SafeRect();
            float width = Mathf.Min(preferredWidth, safe.width * 0.34f);
            float height = Mathf.Min(preferredHeight, safe.height);
            return ClampToScreen(new Rect(safe.x, safe.y, width, height));
        }

        public static Rect ClampToScreen(Rect rect)
        {
            Rect safe = SafeRect();
            float width = Mathf.Min(rect.width, Mathf.Max(1f, safe.width));
            float height = Mathf.Min(rect.height, Mathf.Max(1f, safe.height));
            float x = Mathf.Clamp(rect.x, safe.x, Mathf.Max(safe.x, safe.xMax - width));
            float y = Mathf.Clamp(rect.y, safe.y, Mathf.Max(safe.y, safe.yMax - height));
            return new Rect(x, y, width, height);
        }

        public static float Scale(float min = 0.75f, float max = 1.15f)
        {
            float widthScale = Screen.width / ReferenceWidth;
            float heightScale = Screen.height / ReferenceHeight;
            return Mathf.Clamp(Mathf.Min(widthScale, heightScale), min, max);
        }

        public static void ApplyReadableDefaults()
        {
            KoreanFontManager.ApplyToGuiSkin();
            ApplyLabelDefaults(GUI.skin.label);
            ApplyLabelDefaults(GUI.skin.box);
            ApplyButtonDefaults(GUI.skin.button);
        }

        public static float TouchHeight(float baseHeight)
        {
            if (!Application.isMobilePlatform)
            {
                return baseHeight;
            }

            return Mathf.Max(48f, baseHeight * ReadabilityScale);
        }

        private static void ApplyLabelDefaults(GUIStyle style)
        {
            if (style == null)
            {
                return;
            }

            style.wordWrap = true;
            style.clipping = TextClipping.Clip;
            style.fontSize = Mathf.Max(style.fontSize, Mathf.RoundToInt(14f * ReadabilityScale));
        }

        private static void ApplyButtonDefaults(GUIStyle style)
        {
            if (style == null)
            {
                return;
            }

            style.wordWrap = true;
            style.clipping = TextClipping.Clip;
            style.fontSize = Mathf.Max(style.fontSize, Mathf.RoundToInt(14f * ReadabilityScale));
        }
    }
}
