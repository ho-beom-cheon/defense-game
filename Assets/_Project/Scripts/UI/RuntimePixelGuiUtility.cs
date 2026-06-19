using UnityEngine;

namespace RuneGate
{
    public static class RuntimePixelGuiUtility
    {
        private static Texture2D solidPanelTexture;
        private static Texture2D solidPanelAltTexture;
        private static Texture2D solidButtonTexture;

        public static GUIStyle CreateBoxStyle(GUIStyle fallbackStyle, string spritePath)
        {
            KoreanFontManager.ApplyToGuiSkin();
            GUIStyle style = new GUIStyle(fallbackStyle);
            KoreanFontManager.ApplyFont(style);
            style.wordWrap = true;
            style.clipping = TextClipping.Clip;
            Texture2D texture = LoadTexture(spritePath);
            if (texture != null)
            {
                style.normal.background = texture;
                style.padding = new RectOffset(10, 10, 8, 8);
            }

            return style;
        }

        public static GUIStyle CreateSolidPanelStyle(GUIStyle fallbackStyle, bool alternate = false)
        {
            KoreanFontManager.ApplyToGuiSkin();
            GUIStyle style = new GUIStyle(fallbackStyle);
            KoreanFontManager.ApplyFont(style);
            style.wordWrap = true;
            style.clipping = TextClipping.Clip;
            style.padding = new RectOffset(10, 10, 8, 8);
            style.normal.background = alternate
                ? GetOrCreateSolidTexture(ref solidPanelAltTexture, new Color(0.03f, 0.06f, 0.07f, 0.92f))
                : GetOrCreateSolidTexture(ref solidPanelTexture, new Color(0.015f, 0.025f, 0.03f, 0.94f));
            style.normal.textColor = Color.white;
            return style;
        }

        public static GUIStyle CreateSolidButtonStyle(GUIStyle fallbackStyle)
        {
            KoreanFontManager.ApplyToGuiSkin();
            GUIStyle style = new GUIStyle(fallbackStyle);
            KoreanFontManager.ApplyFont(style);
            style.wordWrap = true;
            style.clipping = TextClipping.Clip;
            Texture2D texture = GetOrCreateSolidTexture(ref solidButtonTexture, new Color(0.08f, 0.14f, 0.12f, 0.96f));
            style.normal.background = texture;
            style.hover.background = texture;
            style.active.background = texture;
            style.focused.background = texture;
            style.normal.textColor = Color.white;
            style.hover.textColor = Color.white;
            style.active.textColor = Color.white;
            return style;
        }

        public static GUIStyle CreateButtonStyle(GUIStyle fallbackStyle, string spritePath)
        {
            KoreanFontManager.ApplyToGuiSkin();
            GUIStyle style = new GUIStyle(fallbackStyle);
            KoreanFontManager.ApplyFont(style);
            style.wordWrap = true;
            style.clipping = TextClipping.Clip;
            Texture2D texture = LoadTexture(spritePath);
            if (texture != null)
            {
                style.normal.background = texture;
                style.hover.background = texture;
                style.active.background = texture;
                style.focused.background = texture;
                style.normal.textColor = Color.white;
                style.hover.textColor = Color.white;
                style.active.textColor = Color.white;
            }

            return style;
        }

        public static void DrawIcon(string spritePath, float size)
        {
            Texture2D texture = LoadTexture(spritePath);
            Rect rect = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size), GUILayout.Height(size));
            if (texture != null)
            {
                GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, true);
            }
        }

        public static Texture2D LoadTexture(string spritePath)
        {
            Sprite sprite = RuntimePixelAssetLoader.LoadSprite(spritePath);
            return sprite != null ? sprite.texture : null;
        }

        private static Texture2D GetOrCreateSolidTexture(ref Texture2D texture, Color color)
        {
            if (texture != null)
            {
                return texture;
            }

            texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
