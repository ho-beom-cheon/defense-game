using UnityEngine;

namespace RuneGate
{
    public static class RuntimePixelGuiUtility
    {
        public static GUIStyle CreateBoxStyle(GUIStyle fallbackStyle, string spritePath)
        {
            GUIStyle style = new GUIStyle(fallbackStyle);
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
            GUIStyle style = new GUIStyle(fallbackStyle);
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

        private static Texture2D LoadTexture(string spritePath)
        {
            Sprite sprite = RuntimePixelAssetLoader.LoadSprite(spritePath);
            return sprite != null ? sprite.texture : null;
        }
    }
}
