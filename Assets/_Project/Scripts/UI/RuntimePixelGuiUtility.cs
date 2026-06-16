using UnityEngine;

namespace RuneGate
{
    public static class RuntimePixelGuiUtility
    {
        public static GUIStyle CreateBoxStyle(GUIStyle fallbackStyle, string spritePath)
        {
            KoreanFontManager.ApplyToGuiSkin();
            GUIStyle style = new GUIStyle(fallbackStyle);
            KoreanFontManager.ApplyFont(style);
            Texture2D texture = LoadTexture(spritePath);
            if (texture != null)
            {
                style.normal.background = texture;
                style.padding = new RectOffset(10, 10, 8, 8);
            }

            return style;
        }

        public static GUIStyle CreateButtonStyle(GUIStyle fallbackStyle, string spritePath)
        {
            KoreanFontManager.ApplyToGuiSkin();
            GUIStyle style = new GUIStyle(fallbackStyle);
            KoreanFontManager.ApplyFont(style);
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
                style.wordWrap = true;
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
    }
}
