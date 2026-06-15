using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RuneGate
{
    public static class PrototypeAssetLoader
    {
        public static List<StageData> LoadStages()
        {
#if UNITY_EDITOR
            return LoadAssets<StageData>("Assets/_Project/Data/Stages/Goblin Forest *.asset", 10);
#else
            return new List<StageData>();
#endif
        }

        public static StageData LoadDefaultStage()
        {
            List<StageData> stages = LoadStages();
            return stages.Count > 0 ? stages[0] : null;
        }

        public static List<RuneData> LoadRunes()
        {
#if UNITY_EDITOR
            return LoadAssets<RuneData>("Assets/_Project/Data/Runes/*.asset", 20);
#else
            return new List<RuneData>();
#endif
        }

        public static List<UpgradeData> LoadUpgrades()
        {
#if UNITY_EDITOR
            return LoadAssets<UpgradeData>("Assets/_Project/Data/Upgrades/*.asset", 4);
#else
            return new List<UpgradeData>();
#endif
        }

        public static HeroRosterData LoadHeroRoster()
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<HeroRosterData>("Assets/_Project/Data/Rosters/MVP Hero Roster.asset");
#else
            return null;
#endif
        }

        public static FormationData LoadDefaultFormation()
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<FormationData>("Assets/_Project/Data/Formations/Default Formation.asset");
#else
            return null;
#endif
        }

#if UNITY_EDITOR
        private static List<T> LoadAssets<T>(string searchPattern, int expectedMaxCount) where T : Object
        {
            List<T> assets = new List<T>();
            string directory = System.IO.Path.GetDirectoryName(searchPattern)?.Replace('\\', '/');
            string pattern = System.IO.Path.GetFileName(searchPattern);
            if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(pattern))
            {
                return assets;
            }

            string absoluteDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "..", directory));
            if (!System.IO.Directory.Exists(absoluteDirectory))
            {
                return assets;
            }

            string[] paths = System.IO.Directory.GetFiles(absoluteDirectory, pattern);
            System.Array.Sort(paths, System.StringComparer.OrdinalIgnoreCase);
            int count = expectedMaxCount > 0 ? Mathf.Min(paths.Length, expectedMaxCount) : paths.Length;
            for (int i = 0; i < count; i++)
            {
                string assetPath = paths[i].Replace('\\', '/');
                int assetsIndex = assetPath.IndexOf("Assets/", System.StringComparison.OrdinalIgnoreCase);
                if (assetsIndex >= 0)
                {
                    assetPath = assetPath.Substring(assetsIndex);
                }

                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets;
        }
#endif
    }
}
