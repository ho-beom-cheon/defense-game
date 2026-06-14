using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    [CreateAssetMenu(fileName = "HeroRosterData", menuName = "RuneGate/Data/Hero Roster")]
    public sealed class HeroRosterData : ScriptableObject
    {
        [SerializeField] private List<HeroData> heroes = new List<HeroData>();

        public IReadOnlyList<HeroData> Heroes => heroes;

        public HeroData FindHeroById(string heroId)
        {
            if (string.IsNullOrWhiteSpace(heroId))
            {
                return null;
            }

            for (int i = 0; i < heroes.Count; i++)
            {
                HeroData heroData = heroes[i];
                if (heroData != null && heroData.HeroId == heroId)
                {
                    return heroData;
                }
            }

            return null;
        }
    }
}
