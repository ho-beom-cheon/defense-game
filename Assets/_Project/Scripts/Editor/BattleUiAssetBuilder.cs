using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RuneGate.Editor
{
    public static class BattleUiAssetBuilder
    {
        public const string ThemePath = "Assets/_Project/Data/UI/RuneGateUiTheme.asset";
        public const string PrefabPath = "Assets/_Project/Prefabs/UI/Battle/BattleCanvas.prefab";

        private const string RegularFontPath = "Assets/_Project/Fonts/NotoSansKR-Regular.ttf";
        private const string SemiboldFontPath = "Assets/_Project/Fonts/NotoSansKR-SemiBold.ttf";
        private const string BoldFontPath = "Assets/_Project/Fonts/NotoSansKR-Bold.ttf";
        private const string PanelSpritePath = "Assets/_Project/Art/RuntimePixel/UI/ui_panel_dark.png";
        private const string ButtonSpritePath = "Assets/_Project/Art/RuntimePixel/UI/ui_button_skill.png";
        private const string RuneSpritePath = "Assets/_Project/Art/RuntimePixel/UI/ui_rune_card_base.png";
        private const string PauseSpritePath = "Assets/_Project/Art/RuntimePixel/UI/System/ui_icon_settings.png";

        [MenuItem("Tools/RuneGate/Build Battle uGUI Assets")]
        public static void BuildAssets()
        {
            EnsureTmpEssentials();
            EnsureFolder("Assets/_Project/Data/UI");
            EnsureFolder("Assets/_Project/Prefabs/UI/Battle");
            ConfigureSpriteBorder(PanelSpritePath, new Vector4(24f, 24f, 24f, 24f));
            ConfigureSpriteBorder(ButtonSpritePath, new Vector4(12f, 12f, 12f, 12f));
            ConfigureSpriteBorder(RuneSpritePath, new Vector4(24f, 24f, 24f, 24f));

            RuneGateUiTheme theme = AssetDatabase.LoadAssetAtPath<RuneGateUiTheme>(ThemePath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<RuneGateUiTheme>();
                AssetDatabase.CreateAsset(theme, ThemePath);
            }

            SerializedObject themeObject = new SerializedObject(theme);
            SetObject(themeObject, "regularFont", AssetDatabase.LoadAssetAtPath<Font>(RegularFontPath));
            SetObject(themeObject, "semiboldFont", AssetDatabase.LoadAssetAtPath<Font>(SemiboldFontPath));
            SetObject(themeObject, "boldFont", AssetDatabase.LoadAssetAtPath<Font>(BoldFontPath));
            SetObject(themeObject, "panelSprite", AssetDatabase.LoadAssetAtPath<Sprite>(PanelSpritePath));
            SetObject(themeObject, "buttonSprite", AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSpritePath));
            SetObject(themeObject, "runeCardSprite", AssetDatabase.LoadAssetAtPath<Sprite>(RuneSpritePath));
            SetObject(themeObject, "pauseIcon", AssetDatabase.LoadAssetAtPath<Sprite>(PauseSpritePath));
            themeObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(theme);

            GameObject root = new GameObject("BattleCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            try
            {
                BattleCanvasController controller = root.AddComponent<BattleCanvasController>();
                controller.AssignTheme(theme);
                controller.RebuildView();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"RuneGate Battle uGUI assets built: {ThemePath}, {PrefabPath}");
        }

        private static void EnsureTmpEssentials()
        {
            const string settingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
            if (AssetDatabase.LoadAssetAtPath<TMP_Settings>(settingsPath) != null)
            {
                return;
            }

            throw new InvalidOperationException(
                $"TMP Essential Resources are missing: {settingsPath}. " +
                "Import them from Window > TextMeshPro > Import TMP Essential Resources before rebuilding Battle UI assets.");
        }

        private static void ConfigureSpriteBorder(string path, Vector4 border)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Battle UI sprite importer is missing: {path}");
            }

            bool changed = importer.textureType != TextureImporterType.Sprite || importer.spriteBorder != border;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = border;
            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = System.IO.Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(parent) || string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException($"Invalid Unity folder path: {path}");
            }

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static void SetObject(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"Missing serialized property {propertyName} on {serializedObject.targetObject.name}.");
            }

            property.objectReferenceValue = value;
        }
    }
}
