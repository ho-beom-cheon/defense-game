using System;
using UnityEditor;
using UnityEngine;

namespace RuneGate.Editor
{
    public static class BattlefieldArtAssetBuilder
    {
        public const string ArtFolder = "Assets/_Project/Art/RuntimePixel/Battlefield/Stage01";
        public const string ThemePath = "Assets/_Project/Data/Battlefield/Stage01BattlefieldArtTheme.asset";

        private static readonly BattlefieldTextureDefinition[] Textures =
        {
            new BattlefieldTextureDefinition("bg_stage01_sealed_forest.png", 1536, 1536, false, false),
            new BattlefieldTextureDefinition("ground_stage01_lane.png", 1024, 192, true, true),
            new BattlefieldTextureDefinition("obj_stage01_seal_crystal.png", 384, 640, true, false),
            new BattlefieldTextureDefinition("obj_stage01_spawn_rift.png", 448, 720, true, false),
            new BattlefieldTextureDefinition("decal_unit_shadow.png", 256, 96, true, false),
            new BattlefieldTextureDefinition("decal_hero_slot_rune.png", 192, 96, true, false),
            new BattlefieldTextureDefinition("fx_crystal_shield_ring.png", 384, 384, true, false),
            new BattlefieldTextureDefinition("fx_rift_pulse.png", 384, 512, true, false)
        };

        [MenuItem("Tools/RuneGate/Build Stage 1 Battlefield Art")]
        public static void BuildStage01Assets()
        {
            BuildAssets();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<BattlefieldArtTheme>(ThemePath);
            Debug.Log("RuneGate Stage 1 battlefield art assets built.");
        }

        public static BattlefieldArtTheme BuildAssets()
        {
            EnsureFolder("Assets/_Project/Data/Battlefield");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            for (int i = 0; i < Textures.Length; i++)
            {
                ConfigureTexture(Textures[i]);
            }

            BattlefieldArtTheme theme = AssetDatabase.LoadAssetAtPath<BattlefieldArtTheme>(ThemePath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<BattlefieldArtTheme>();
                AssetDatabase.CreateAsset(theme, ThemePath);
            }

            SerializedObject serializedTheme = new SerializedObject(theme);
            SetSprite(serializedTheme, "background", "bg_stage01_sealed_forest.png");
            SetSprite(serializedTheme, "lane", "ground_stage01_lane.png");
            SetSprite(serializedTheme, "crystal", "obj_stage01_seal_crystal.png");
            SetSprite(serializedTheme, "rift", "obj_stage01_spawn_rift.png");
            SetSprite(serializedTheme, "unitShadow", "decal_unit_shadow.png");
            SetSprite(serializedTheme, "heroSlotRune", "decal_hero_slot_rune.png");
            SetSprite(serializedTheme, "crystalShieldRing", "fx_crystal_shield_ring.png");
            SetSprite(serializedTheme, "riftPulse", "fx_rift_pulse.png");
            serializedTheme.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            return theme;
        }

        private static void ConfigureTexture(BattlefieldTextureDefinition definition)
        {
            string path = $"{ArtFolder}/{definition.FileName}";
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Stage 1 battlefield texture is missing: {path}");
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = definition.Repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = definition.HasAlpha;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.sRGBTexture = true;

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);

            TextureImporterPlatformSettings androidSettings = importer.GetPlatformTextureSettings("Android");
            androidSettings.name = "Android";
            androidSettings.overridden = true;
            androidSettings.maxTextureSize = 2048;
            androidSettings.format = definition.HasAlpha ? TextureImporterFormat.RGBA32 : TextureImporterFormat.RGB24;
            androidSettings.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SetPlatformTextureSettings(androidSettings);
            importer.SaveAndReimport();

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null || texture.width != definition.Width || texture.height != definition.Height)
            {
                string actual = texture == null ? "missing" : $"{texture.width}x{texture.height}";
                throw new InvalidOperationException($"Stage 1 battlefield texture has invalid dimensions: {path} ({actual}, expected {definition.Width}x{definition.Height})");
            }
        }

        private static void SetSprite(SerializedObject serializedTheme, string propertyName, string fileName)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtFolder}/{fileName}");
            if (sprite == null)
            {
                throw new InvalidOperationException($"Stage 1 battlefield sprite could not be loaded: {fileName}");
            }

            SerializedProperty property = serializedTheme.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"BattlefieldArtTheme property is missing: {propertyName}");
            }

            property.objectReferenceValue = sprite;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private readonly struct BattlefieldTextureDefinition
        {
            public BattlefieldTextureDefinition(string fileName, int width, int height, bool hasAlpha, bool repeat)
            {
                FileName = fileName;
                Width = width;
                Height = height;
                HasAlpha = hasAlpha;
                Repeat = repeat;
            }

            public string FileName { get; }
            public int Width { get; }
            public int Height { get; }
            public bool HasAlpha { get; }
            public bool Repeat { get; }
        }
    }
}
