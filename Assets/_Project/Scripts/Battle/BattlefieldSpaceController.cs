using System;
using UnityEngine;

namespace RuneGate
{
    [DisallowMultipleComponent]
    public sealed class BattlefieldSpaceController : MonoBehaviour
    {
        private static readonly float[] SpawnOffsets = { 0f, 0.72f, -0.72f, 0.36f, -0.36f, 1f, -1f };

        [SerializeField] private BattlefieldCameraFitter cameraFitter;
        [SerializeField] private CrystalController crystalController;
        [SerializeField] private BattlefieldSpaceConfig config;

        private BattlefieldBounds currentBounds;
        private bool subscribed;

        public bool IsReady => cameraFitter != null && crystalController != null && config != null && config.HasValidLayout() && currentBounds.IsValid;
        public BattlefieldBounds CurrentBounds => currentBounds;
        public BattlefieldSpaceConfig Config => config;

        public event Action<BattlefieldBounds, BattlefieldBounds> LayoutChanged;

        public void Configure(
            BattlefieldCameraFitter nextCameraFitter,
            CrystalController nextCrystalController,
            BattlefieldSpaceConfig nextConfig)
        {
            UnbindCamera();
            cameraFitter = nextCameraFitter;
            crystalController = nextCrystalController;
            config = nextConfig;
            BindCamera();
            RefreshLayout();
        }

        public void RefreshLayout()
        {
            if (cameraFitter == null || config == null)
            {
                return;
            }

            Bounds cameraBounds = cameraFitter.CurrentWorldBounds;
            if (cameraBounds.size.x <= 0.01f || cameraBounds.size.y <= 0.01f)
            {
                return;
            }

            Vector4 padding = config.PlayablePadding;
            float minX = cameraBounds.min.x + padding.x;
            float maxX = cameraBounds.max.x - padding.y;
            float minY = cameraBounds.min.y + padding.z;
            float maxY = cameraBounds.max.y - padding.w;
            if (maxX <= minX + 1f || maxY <= minY + 1f)
            {
                Debug.LogError("BattlefieldSpaceController could not resolve a valid PlayableBounds from the current viewport.", this);
                return;
            }

            Rect playable = Rect.MinMaxRect(minX, minY, maxX, maxY);
            BattlefieldBounds provisional = new BattlefieldBounds(playable, playable, playable);
            BattlefieldBounds next = new BattlefieldBounds(
                playable,
                provisional.NormalizedRectToWorld(config.HeroHomeNormalizedRect),
                provisional.NormalizedRectToWorld(config.RiftSpawnNormalizedRect));
            BattlefieldBounds previous = currentBounds;
            currentBounds = next;
            if (!previous.IsValid || RectChanged(previous.PlayableRect, next.PlayableRect))
            {
                LayoutChanged?.Invoke(previous, next);
            }
        }

        public Vector2 ResolveFormationAnchor(FormationSlot slot)
        {
            if (slot == null || !IsReady)
            {
                return Vector2.zero;
            }

            return currentBounds.ToWorld(new Vector2(
                config.GetFormationU(slot.PositionType),
                config.GetFormationRowV(slot.LaneIndex)));
        }

        public Vector2 ResolveFormationAnchor(int rowIndex, HeroPositionType positionType)
        {
            if (!IsReady)
            {
                return Vector2.zero;
            }

            return currentBounds.ToWorld(new Vector2(
                config.GetFormationU(positionType),
                config.GetFormationRowV(rowIndex)));
        }

        public Vector2 ResolveEnemySpawn(
            int spawnBandIndex,
            int waveNumber,
            int groupIndex,
            int spawnOrdinal,
            Vector2 halfExtents)
        {
            if (!IsReady)
            {
                return Vector2.zero;
            }

            Vector2 band = config.GetSpawnBandV(spawnBandIndex);
            int sequenceIndex = PositiveModulo(spawnOrdinal + groupIndex * 2 + Mathf.Max(0, waveNumber - 1), SpawnOffsets.Length);
            float center = (band.x + band.y) * 0.5f;
            float halfRange = (band.y - band.x) * 0.5f;
            float v = Mathf.Clamp(center + SpawnOffsets[sequenceIndex] * halfRange, band.x, band.y);
            float u = Mathf.Lerp(config.RiftSpawnNormalizedRect.xMin, config.RiftSpawnNormalizedRect.xMax, 0.72f);
            return currentBounds.Clamp(currentBounds.ToWorld(new Vector2(u, v)), halfExtents);
        }

        public Vector2 Clamp(Vector2 position, Vector2 halfExtents)
        {
            return currentBounds.IsValid ? currentBounds.Clamp(position, halfExtents) : position;
        }

        private void OnEnable()
        {
            BindCamera();
            RefreshLayout();
        }

        private void OnDisable()
        {
            UnbindCamera();
        }

        private void HandleWorldBoundsChanged(Bounds _)
        {
            RefreshLayout();
        }

        private void BindCamera()
        {
            if (subscribed || cameraFitter == null)
            {
                return;
            }

            cameraFitter.WorldBoundsChanged += HandleWorldBoundsChanged;
            subscribed = true;
        }

        private void UnbindCamera()
        {
            if (!subscribed || cameraFitter == null)
            {
                subscribed = false;
                return;
            }

            cameraFitter.WorldBoundsChanged -= HandleWorldBoundsChanged;
            subscribed = false;
        }

        private static int PositiveModulo(int value, int divisor)
        {
            int result = value % divisor;
            return result < 0 ? result + divisor : result;
        }

        private static bool RectChanged(Rect a, Rect b)
        {
            return Mathf.Abs(a.x - b.x) > 0.001f ||
                Mathf.Abs(a.y - b.y) > 0.001f ||
                Mathf.Abs(a.width - b.width) > 0.001f ||
                Mathf.Abs(a.height - b.height) > 0.001f;
        }
    }
}
