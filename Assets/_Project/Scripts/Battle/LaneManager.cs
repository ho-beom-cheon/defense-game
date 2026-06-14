using UnityEngine;

namespace RuneGate
{
    public sealed class LaneManager : MonoBehaviour
    {
        [SerializeField] private int laneCount = 3;
        [SerializeField] private float laneSpacing = 2.5f;
        [SerializeField] private float spawnX = 5.5f;
        [SerializeField] private float crystalX = -5.5f;
        [SerializeField] private Transform[] laneSpawnPoints;
        [SerializeField] private Transform[] crystalTargetPoints;

        public int LaneCount => Mathf.Max(1, laneCount);

        public bool IsValidLaneIndex(int laneIndex)
        {
            return laneIndex >= 0 && laneIndex < LaneCount;
        }

        public Vector3 GetSpawnPosition(int laneIndex)
        {
            if (!IsValidLaneIndex(laneIndex))
            {
                Debug.LogWarning($"LaneManager received invalid spawn lane index {laneIndex}.");
                laneIndex = Mathf.Clamp(laneIndex, 0, LaneCount - 1);
            }

            if (laneSpawnPoints != null && laneIndex < laneSpawnPoints.Length && laneSpawnPoints[laneIndex] != null)
            {
                return laneSpawnPoints[laneIndex].position;
            }

            return new Vector3(spawnX, GetLaneY(laneIndex), 0f);
        }

        public Vector3 GetCrystalTargetPosition(int laneIndex)
        {
            if (!IsValidLaneIndex(laneIndex))
            {
                Debug.LogWarning($"LaneManager received invalid crystal target lane index {laneIndex}.");
                laneIndex = Mathf.Clamp(laneIndex, 0, LaneCount - 1);
            }

            if (crystalTargetPoints != null && laneIndex < crystalTargetPoints.Length && crystalTargetPoints[laneIndex] != null)
            {
                return crystalTargetPoints[laneIndex].position;
            }

            return new Vector3(crystalX, GetLaneY(laneIndex), 0f);
        }

        private float GetLaneY(int laneIndex)
        {
            float centerOffset = (LaneCount - 1) * 0.5f;
            return (laneIndex - centerOffset) * laneSpacing;
        }
    }
}
