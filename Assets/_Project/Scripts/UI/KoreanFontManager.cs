using System;
using System.Reflection;
using UnityEngine;

namespace RuneGate
{
    public sealed class KoreanFontManager : MonoBehaviour
    {
        private const string CatalogResourcePath = "KoreanFontCatalog";

        [SerializeField] private Font regularFont;
        [SerializeField] private Font semiBoldFont;
        [SerializeField] private Font boldFont;
        [SerializeField] private bool applyOnEnable = true;
        [SerializeField] private bool applyToInactiveObjects = true;

        private static KoreanFontCatalog cachedCatalog;
        private static Font cachedRegularFont;
        private static Font cachedSemiBoldFont;
        private static Font cachedBoldFont;
        private static bool warnedMissingFont;
        private static bool warnedTmpFontAsset;

        public static Font RegularFont => ResolveFont(ref cachedRegularFont, font => font.RegularFont);
        public static Font SemiBoldFont => ResolveFont(ref cachedSemiBoldFont, font => font.SemiBoldFont);
        public static Font BoldFont => ResolveFont(ref cachedBoldFont, font => font.BoldFont);

        public static string GetSkillDisplayName(SkillData skillData)
        {
            return GameTextMapper.SkillName(skillData);
        }

        private void OnEnable()
        {
            CacheSerializedFonts();
            if (!applyOnEnable)
            {
                return;
            }

            ApplyToGuiSkin();
            ApplyToSceneText();
        }

        public static void ApplyToGuiSkin()
        {
            Font font = RegularFont;
            if (font == null)
            {
                WarnMissingFont();
                return;
            }

            GUI.skin.font = font;
            ApplyFont(GUI.skin.label, font);
            ApplyFont(GUI.skin.box, font);
            ApplyFont(GUI.skin.button, font);
            ApplyFont(GUI.skin.toggle, font);
            ApplyFont(GUI.skin.textField, font);
            ApplyFont(GUI.skin.textArea, font);
            ApplyFont(GUI.skin.window, font);
        }

        public static void ApplyFont(GUIStyle style, Font font = null)
        {
            if (style == null)
            {
                return;
            }

            Font resolvedFont = font != null ? font : RegularFont;
            if (resolvedFont == null)
            {
                WarnMissingFont();
                return;
            }

            style.font = resolvedFont;
        }

        public void ApplyToSceneText()
        {
            Font font = regularFont != null ? regularFont : RegularFont;
            if (font == null)
            {
                WarnMissingFont();
                return;
            }

            FindObjectsInactive inactiveMode = applyToInactiveObjects ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;
            Component[] components = FindObjectsByType<Component>(inactiveMode);
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null)
                {
                    continue;
                }

                Type type = component.GetType();
                if (type.FullName == "UnityEngine.UI.Text")
                {
                    SetProperty(component, type, "font", font);
                    continue;
                }

                if (type.FullName == "TMPro.TMP_Text" || IsSubclassNamed(type, "TMPro.TMP_Text"))
                {
                    WarnTmpFontAsset();
                }
            }
        }

        private void CacheSerializedFonts()
        {
            if (regularFont != null)
            {
                cachedRegularFont = regularFont;
            }

            if (semiBoldFont != null)
            {
                cachedSemiBoldFont = semiBoldFont;
            }

            if (boldFont != null)
            {
                cachedBoldFont = boldFont;
            }
        }

        private static Font ResolveFont(ref Font cachedFont, Func<KoreanFontCatalog, Font> selector)
        {
            if (cachedFont != null)
            {
                return cachedFont;
            }

            KoreanFontCatalog catalog = ResolveCatalog();
            if (catalog != null)
            {
                cachedFont = selector(catalog);
            }

#if UNITY_EDITOR
            if (cachedFont == null)
            {
                cachedFont = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Fonts/NotoSansKR-Regular.ttf");
            }
#endif

            return cachedFont;
        }

        private static KoreanFontCatalog ResolveCatalog()
        {
            if (cachedCatalog == null)
            {
                cachedCatalog = Resources.Load<KoreanFontCatalog>(CatalogResourcePath);
            }

            return cachedCatalog;
        }

        private static void SetProperty(Component component, Type type, string propertyName, object value)
        {
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (property != null && property.CanWrite)
            {
                property.SetValue(component, value);
            }
        }

        private static bool IsSubclassNamed(Type type, string fullName)
        {
            Type current = type.BaseType;
            while (current != null)
            {
                if (current.FullName == fullName)
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        private static void WarnMissingFont()
        {
            if (warnedMissingFont)
            {
                return;
            }

            warnedMissingFont = true;
            Debug.LogWarning("KoreanFontManager could not find NotoSansKR font. Check Assets/_Project/Fonts and KoreanFontCatalog.");
        }

        private static void WarnTmpFontAsset()
        {
            if (warnedTmpFontAsset)
            {
                return;
            }

            warnedTmpFontAsset = true;
            Debug.LogWarning("TMP text was found, but this prototype has no generated TMP Font Asset yet. Create a Dynamic TMP Font Asset from NotoSansKR if TMP UI is added.");
        }
    }
}
