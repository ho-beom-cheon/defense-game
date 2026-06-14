using UnityEngine;

namespace RuneGate
{
    public sealed class HeroPlacementSlot : MonoBehaviour
    {
        [SerializeField] private int laneIndex;
        [SerializeField] private HeroPositionType positionType = HeroPositionType.Middle;
        [SerializeField] private HeroController currentHero;

        public int LaneIndex => laneIndex;
        public HeroPositionType PositionType => positionType;
        public int SlotIndex => FormationSlot.ToSlotIndex(positionType);
        public HeroController CurrentHero => currentHero;
        public bool IsOccupied => currentHero != null;
        public Vector3 WorldPosition => transform.position;

        public void Configure(int nextLaneIndex, HeroPositionType nextPositionType)
        {
            laneIndex = nextLaneIndex;
            positionType = nextPositionType;
        }

        public void Assign(HeroController hero)
        {
            currentHero = hero;
        }

        public void Clear()
        {
            currentHero = null;
        }
    }
}
