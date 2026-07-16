using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RuneGate
{
    public static class PrototypeAssetLoader
    {
        private const string RuntimeContentCatalogResourcePath = "RuntimeContentCatalog";
        private static RuntimeContentCatalog cachedRuntimeContentCatalog;

        public static List<StageData> LoadStages()
        {
            RuntimeContentCatalog catalog = LoadRuntimeContentCatalog();
            List<StageData> runtimeStages = CopyAssets(catalog != null ? catalog.Stages : null);
            if (runtimeStages.Count > 0)
            {
                SortStagesByStageId(runtimeStages);
                return runtimeStages;
            }

#if UNITY_EDITOR
            List<StageData> stages = LoadAssets<StageData>("Assets/_Project/Data/Stages/Goblin Forest *.asset", 10);
            SortStagesByStageId(stages);
            return stages;
#else
            return new List<StageData>();
#endif
        }

        public static void SortStagesByStageId(List<StageData> stages)
        {
            if (stages == null)
            {
                return;
            }

            stages.Sort(CompareStages);
        }

        public static int CompareStages(StageData left, StageData right)
        {
            int numberComparison = GetStageNumber(left).CompareTo(GetStageNumber(right));
            if (numberComparison != 0)
            {
                return numberComparison;
            }

            string leftId = left != null ? left.StageId : string.Empty;
            string rightId = right != null ? right.StageId : string.Empty;
            int idComparison = string.Compare(leftId, rightId, System.StringComparison.OrdinalIgnoreCase);
            if (idComparison != 0)
            {
                return idComparison;
            }

            string leftName = left != null ? left.name : string.Empty;
            string rightName = right != null ? right.name : string.Empty;
            return string.Compare(leftName, rightName, System.StringComparison.OrdinalIgnoreCase);
        }

        public static int GetStageNumber(StageData stageData)
        {
            if (stageData == null || string.IsNullOrWhiteSpace(stageData.StageId))
            {
                return int.MaxValue;
            }

            string stageId = stageData.StageId;
            int end = stageId.Length - 1;
            while (end >= 0 && char.IsDigit(stageId[end]))
            {
                end--;
            }

            if (end >= stageId.Length - 1)
            {
                return int.MaxValue;
            }

            string numberText = stageId.Substring(end + 1);
            return int.TryParse(numberText, out int stageNumber) ? stageNumber : int.MaxValue;
        }

        public static StageData LoadDefaultStage()
        {
            List<StageData> stages = LoadStages();
            return stages.Count > 0 ? stages[0] : null;
        }

        public static List<RuneData> LoadRunes()
        {
            RuntimeContentCatalog catalog = LoadRuntimeContentCatalog();
            List<RuneData> runtimeRunes = CopyAssets(catalog != null ? catalog.Runes : null);
            if (runtimeRunes.Count > 0)
            {
                return runtimeRunes;
            }

#if UNITY_EDITOR
            return LoadAssets<RuneData>("Assets/_Project/Data/Runes/*.asset", 20);
#else
            return new List<RuneData>();
#endif
        }

        public static List<UpgradeData> LoadUpgrades()
        {
            RuntimeContentCatalog catalog = LoadRuntimeContentCatalog();
            List<UpgradeData> runtimeUpgrades = CopyAssets(catalog != null ? catalog.Upgrades : null);
            if (runtimeUpgrades.Count > 0)
            {
                return runtimeUpgrades;
            }

#if UNITY_EDITOR
            return LoadAssets<UpgradeData>("Assets/_Project/Data/Upgrades/*.asset", 4);
#else
            return new List<UpgradeData>();
#endif
        }

        public static HeroRosterData LoadHeroRoster()
        {
            RuntimeContentCatalog catalog = LoadRuntimeContentCatalog();
            if (catalog != null && catalog.HeroRoster != null)
            {
                return catalog.HeroRoster;
            }

#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<HeroRosterData>("Assets/_Project/Data/Rosters/MVP Hero Roster.asset");
#else
            return null;
#endif
        }

        public static FormationData LoadDefaultFormation()
        {
            RuntimeContentCatalog catalog = LoadRuntimeContentCatalog();
            if (catalog != null && catalog.DefaultFormation != null)
            {
                return catalog.DefaultFormation;
            }

#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<FormationData>("Assets/_Project/Data/Formations/Default Formation.asset");
#else
            return null;
#endif
        }

        private static RuntimeContentCatalog LoadRuntimeContentCatalog()
        {
            if (cachedRuntimeContentCatalog == null)
            {
                cachedRuntimeContentCatalog = Resources.Load<RuntimeContentCatalog>(RuntimeContentCatalogResourcePath);
            }

            return cachedRuntimeContentCatalog;
        }

        private static List<T> CopyAssets<T>(IReadOnlyList<T> source) where T : Object
        {
            List<T> assets = new List<T>();
            if (source == null)
            {
                return assets;
            }

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] != null)
                {
                    assets.Add(source[i]);
                }
            }

            return assets;
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
