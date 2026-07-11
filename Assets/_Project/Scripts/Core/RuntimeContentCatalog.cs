using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    [CreateAssetMenu(fileName = "RuntimeContentCatalog", menuName = "RuneGate/Runtime Content Catalog")]
    public sealed class RuntimeContentCatalog : ScriptableObject
    {
        [SerializeField] private List<StageData> stages = new List<StageData>();
        [SerializeField] private List<RuneData> runes = new List<RuneData>();
        [SerializeField] private List<UpgradeData> upgrades = new List<UpgradeData>();
        [SerializeField] private HeroRosterData heroRoster;
        [SerializeField] private FormationData defaultFormation;

        public IReadOnlyList<StageData> Stages => stages;
        public IReadOnlyList<RuneData> Runes => runes;
        public IReadOnlyList<UpgradeData> Upgrades => upgrades;
        public HeroRosterData HeroRoster => heroRoster;
        public FormationData DefaultFormation => defaultFormation;
    }
}
