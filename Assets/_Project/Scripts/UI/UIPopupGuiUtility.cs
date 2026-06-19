using UnityEngine;

namespace RuneGate
{
    public static class UIPopupGuiUtility
    {
        public static Rect PopupRect(float preferredWidth, float preferredHeight, float widthRatio = 0.92f, float heightRatio = 0.82f)
        {
            return GameFrameLayout.PopupFrame(preferredWidth, preferredHeight, widthRatio, heightRatio);
        }

        public static void DrawDimOverlay(float alpha = 0.58f)
        {
            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previousColor;
        }

        public static bool DrawHeader(string title)
        {
            bool closeRequested = false;
            GUILayout.BeginHorizontal();
            GUILayout.Label(title);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("\ub2eb\uae30", GUILayout.Width(82f), GUILayout.Height(30f)))
            {
                closeRequested = true;
            }

            GUILayout.EndHorizontal();
            return closeRequested;
        }
    }
}
