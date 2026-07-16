using UnityEditor;
using UnityEngine;

namespace RuneGate.Editor
{
    public static class BattlefieldSpaceAssetBuilder
    {
        public const string ConfigPath = "Assets/_Project/Data/Battlefield/DefaultBattlefieldSpaceConfig.asset";

        [MenuItem("Tools/RuneGate/Build Battlefield Space Config")]
        public static void BuildDefaultConfigMenu()
        {
            BattlefieldSpaceConfig config = BuildAssets();
            Selection.activeObject = config;
            Debug.Log("RuneGate default battlefield space config built.");
        }

        public static BattlefieldSpaceConfig BuildAssets()
        {
            EnsureFolder("Assets/_Project/Data/Battlefield");
            BattlefieldSpaceConfig config = AssetDatabase.LoadAssetAtPath<BattlefieldSpaceConfig>(ConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<BattlefieldSpaceConfig>();
                AssetDatabase.CreateAsset(config, ConfigPath);
            }

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            return config;
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
    }
}
