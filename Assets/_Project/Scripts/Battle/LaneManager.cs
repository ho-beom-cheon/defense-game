using UnityEngine;

namespace RuneGate
{
    public sealed class LaneManager : MonoBehaviour
    {
        [SerializeField] private int laneCount = 3;
        [SerializeField] private float laneSpacing = 2.4f;
        [SerializeField] private float spawnX = 5.6f;
        [SerializeField] private float crystalX = -5.4f;
        [SerializeField] private Transform[] laneSpawnPoints;
        [SerializeField] private Transform[] crystalTargetPoints;
        [SerializeField] private int heroSlotsPerLane = 3;
        [SerializeField] private float heroFrontSlotX = -1.25f;
        [SerializeField] private float heroSlotSpacingX = 0.75f;
        [SerializeField] private Transform[] heroSlotPoints;

        public int LaneCount => Mathf.Max(1, laneCount);
        public int HeroSlotsPerLane => Mathf.Max(1, heroSlotsPerLane);

        public bool IsValidLaneIndex(int laneIndex)
        {
            return laneIndex >= 0 && laneIndex < LaneCount;
        }

        public bool IsValidHeroSlotIndex(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < HeroSlotsPerLane;
        }

        public Vector3 GetSpawnPosition(int laneIndex)
        {
            int safeLaneIndex = ClampLaneIndex(laneIndex, "spawn");
            if (laneSpawnPoints != null && safeLaneIndex < laneSpawnPoints.Length && laneSpawnPoints[safeLaneIndex] != null)
            {
                return laneSpawnPoints[safeLaneIndex].position;
            }

            return new Vector3(spawnX, GetLaneY(safeLaneIndex), 0f);
        }

        public Vector3 GetCrystalTargetPosition(int laneIndex)
        {
            int safeLaneIndex = ClampLaneIndex(laneIndex, "crystal target");
            if (crystalTargetPoints != null && safeLaneIndex < crystalTargetPoints.Length && crystalTargetPoints[safeLaneIndex] != null)
            {
                return crystalTargetPoints[safeLaneIndex].position;
            }

            return new Vector3(crystalX, GetLaneY(safeLaneIndex), 0f);
        }

        public float GetLaneY(int laneIndex)
        {
            int safeLaneIndex = Mathf.Clamp(laneIndex, 0, LaneCount - 1);
            float centerOffset = (LaneCount - 1) * 0.5f;
            return (safeLaneIndex - centerOffset) * laneSpacing;
        }

        public Vector3 GetHeroSlotPosition(int laneIndex, int slotIndex)
        {
            int safeLaneIndex = ClampLaneIndex(laneIndex, "hero slot");
            int safeSlotIndex = ClampHeroSlotIndex(slotIndex);
            int flatIndex = safeLaneIndex * HeroSlotsPerLane + safeSlotIndex;
            if (heroSlotPoints != null && flatIndex < heroSlotPoints.Length && heroSlotPoints[flatIndex] != null)
            {
                return heroSlotPoints[flatIndex].position;
            }

            float x = heroFrontSlotX - safeSlotIndex * Mathf.Abs(heroSlotSpacingX);
            return new Vector3(x, GetLaneY(safeLaneIndex), 0f);
        }

        public Vector3 GetHeroSlotPosition(int laneIndex, HeroPositionType positionType)
        {
            return GetHeroSlotPosition(laneIndex, FormationSlot.ToSlotIndex(positionType));
        }

        private int ClampLaneIndex(int laneIndex, string context)
        {
            if (IsValidLaneIndex(laneIndex))
            {
                return laneIndex;
            }

            Debug.LogWarning($"LaneManager received invalid {context} lane index {laneIndex}.");
            return Mathf.Clamp(laneIndex, 0, LaneCount - 1);
        }

        private int ClampHeroSlotIndex(int slotIndex)
        {
            if (IsValidHeroSlotIndex(slotIndex))
            {
                return slotIndex;
            }

            Debug.LogWarning($"LaneManager received invalid hero slot index {slotIndex}.");
            return Mathf.Clamp(slotIndex, 0, HeroSlotsPerLane - 1);
        }
    }
}
