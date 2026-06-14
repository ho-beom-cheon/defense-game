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

        public int LaneCount => Mathf.Max(1, laneCount);

        public bool IsValidLaneIndex(int laneIndex)
        {
            return laneIndex >= 0 && laneIndex < LaneCount;
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

        private int ClampLaneIndex(int laneIndex, string context)
        {
            if (IsValidLaneIndex(laneIndex))
            {
                return laneIndex;
            }

            Debug.LogWarning($"LaneManager received invalid {context} lane index {laneIndex}.");
            return Mathf.Clamp(laneIndex, 0, LaneCount - 1);
        }
    }
}
