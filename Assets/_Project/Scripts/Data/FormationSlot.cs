using System;
using UnityEngine;

namespace RuneGate
{
    [Serializable]
    public sealed class FormationSlot
    {
        [SerializeField] private int laneIndex;
        [SerializeField] private HeroPositionType positionType = HeroPositionType.Middle;
        [SerializeField] private string heroId = string.Empty;

        public FormationSlot()
        {
        }

        public FormationSlot(int laneIndex, HeroPositionType positionType, string heroId)
        {
            this.laneIndex = laneIndex;
            this.positionType = positionType;
            this.heroId = heroId ?? string.Empty;
        }

        public int LaneIndex => laneIndex;
        public HeroPositionType PositionType => positionType;
        public string HeroId => heroId;
        public int SlotIndex => ToSlotIndex(positionType);

        public void Set(int nextLaneIndex, HeroPositionType nextPositionType, string nextHeroId)
        {
            laneIndex = nextLaneIndex;
            positionType = nextPositionType;
            heroId = nextHeroId ?? string.Empty;
        }

        public static int ToSlotIndex(HeroPositionType positionType)
        {
            switch (positionType)
            {
                case HeroPositionType.Front:
                    return 0;
                case HeroPositionType.Back:
                    return 2;
                case HeroPositionType.Middle:
                default:
                    return 1;
            }
        }
    }
}
