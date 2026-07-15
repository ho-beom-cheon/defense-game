using UnityEngine;

namespace RuneGate
{
    public readonly struct BattleLane
    {
        public BattleLane(int laneId, float yPosition, float heroFrontAnchorX, float heroMiddleAnchorX, float heroBackAnchorX, float enemySpawnX, float crystalTargetX, float minCombatX, float maxCombatX, float laneDepthOffset)
        {
            LaneId = laneId;
            YPosition = yPosition;
            HeroFrontAnchorX = heroFrontAnchorX;
            HeroMiddleAnchorX = heroMiddleAnchorX;
            HeroBackAnchorX = heroBackAnchorX;
            EnemySpawnX = enemySpawnX;
            CrystalTargetX = crystalTargetX;
            MinCombatX = minCombatX;
            MaxCombatX = maxCombatX;
            LaneDepthOffset = laneDepthOffset;
        }

        public int LaneId { get; }
        public float YPosition { get; }
        public float HeroFrontAnchorX { get; }
        public float HeroMiddleAnchorX { get; }
        public float HeroBackAnchorX { get; }
        public float EnemySpawnX { get; }
        public float CrystalTargetX { get; }
        public float MinCombatX { get; }
        public float MaxCombatX { get; }
        public float LaneDepthOffset { get; }
    }

    public sealed class LaneManager : MonoBehaviour
    {
        [SerializeField] private int laneCount = 3;
        [SerializeField] private float laneSpacing = 2.15f;
        [SerializeField] private float spawnX = 5.75f;
        [SerializeField] private float crystalX = -5.15f;
        [SerializeField] private Transform[] laneSpawnPoints;
        [SerializeField] private Transform[] crystalTargetPoints;
        [SerializeField] private int heroSlotsPerLane = 3;
        [SerializeField] private float heroFrontSlotX = -0.55f;
        [SerializeField] private float heroSlotSpacingX = 0.95f;
        [SerializeField] private Transform[] heroSlotPoints;
        [Header("Battlefield Safe Bounds")]
        [SerializeField] private float cameraLeftPadding = 2.45f;
        [SerializeField] private float cameraRightPadding = 0.55f;
        [SerializeField] private float cameraTopPadding = 0.35f;
        [SerializeField] private float cameraBottomPadding = 0.35f;
        [SerializeField] private float crystalLeftPadding = 0.22f;
        [SerializeField] private float laneDepthSpacing = 0.015f;
        [Header("Runtime Visuals")]
        [SerializeField] private bool createRuntimeBattlefieldVisuals = true;
        [SerializeField] private float laneStripHeight = 0.72f;
        [SerializeField] private float heroSlotMarkerYOffset = -0.62f;

        public int LaneCount => Mathf.Max(1, laneCount);
        public int HeroSlotsPerLane => Mathf.Max(1, heroSlotsPerLane);

        private const string RuntimeVisualRootName = "Runtime Battlefield Visuals";

        private void Awake()
        {
            EnsureBattlefieldCameraLayout();
            EnsureRuntimeBattlefieldVisuals();
        }

        private void EnsureBattlefieldCameraLayout()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("LaneManager could not find the main camera for battlefield layout.");
                return;
            }

            BattlefieldCameraFitter fitter = mainCamera.GetComponent<BattlefieldCameraFitter>();
            if (fitter == null)
            {
                fitter = mainCamera.gameObject.AddComponent<BattlefieldCameraFitter>();
            }

            float worldWidth = Mathf.Abs(spawnX - crystalX) + 1.6f;
            float worldHeight = Mathf.Max(7.5f, Mathf.Abs(GetLaneY(LaneCount - 1) - GetLaneY(0)) + 3.2f);
            fitter.Configure(new Vector2((spawnX + crystalX) * 0.5f, 0f), worldWidth, worldHeight);
        }

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

        public float GetCrystalContactX(int laneIndex)
        {
            // The raw crystal target can sit behind the UI-safe combat edge on portrait layouts.
            return Mathf.Max(GetCrystalTargetPosition(laneIndex).x, GetMinCombatX());
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

        public BattleLane GetLaneDefinition(int laneIndex)
        {
            int safeLaneIndex = ClampLaneIndex(laneIndex, "lane definition");
            Bounds safeBounds = GetBattlefieldSafeBounds();
            float frontX = GetHeroSlotPosition(safeLaneIndex, HeroPositionType.Front).x;
            float middleX = GetHeroSlotPosition(safeLaneIndex, HeroPositionType.Middle).x;
            float backX = GetHeroSlotPosition(safeLaneIndex, HeroPositionType.Back).x;
            return new BattleLane(
                safeLaneIndex,
                GetLaneY(safeLaneIndex),
                frontX,
                middleX,
                backX,
                GetSafeSpawnPosition(safeLaneIndex, 0f).x,
                GetCrystalTargetPosition(safeLaneIndex).x,
                safeBounds.min.x,
                safeBounds.max.x,
                GetLaneDepthOffset(safeLaneIndex));
        }

        public float GetMinCombatX()
        {
            return GetBattlefieldSafeBounds().min.x;
        }

        public float GetMaxCombatX()
        {
            return GetBattlefieldSafeBounds().max.x;
        }

        public float GetLaneDepthOffset(int laneIndex)
        {
            int safeLaneIndex = Mathf.Clamp(laneIndex, 0, LaneCount - 1);
            return -safeLaneIndex * Mathf.Max(0f, laneDepthSpacing);
        }

        public Bounds GetBattlefieldSafeBounds()
        {
            Bounds cameraBounds = RuntimeSpriteBoundsUtility.GetCameraWorldBounds();
            float minX = Mathf.Max(cameraBounds.min.x + Mathf.Max(0f, cameraLeftPadding), crystalX - Mathf.Max(0f, crystalLeftPadding));
            float maxX = cameraBounds.max.x - Mathf.Max(0f, cameraRightPadding);
            if (maxX <= minX + 0.5f)
            {
                minX = cameraBounds.min.x + 0.25f;
                maxX = cameraBounds.max.x - 0.25f;
            }

            float minY = cameraBounds.min.y + Mathf.Max(0f, cameraBottomPadding);
            float maxY = cameraBounds.max.y - Mathf.Max(0f, cameraTopPadding);
            Bounds safeBounds = new Bounds();
            safeBounds.SetMinMax(new Vector3(minX, minY, -0.5f), new Vector3(maxX, maxY, 0.5f));
            return safeBounds;
        }

        public void ClampUnitInsideBattlefield(Transform rootTransform, SpriteRenderer spriteRenderer)
        {
            RuntimeSpriteBoundsUtility.ClampRootInsideBounds(rootTransform, spriteRenderer, GetBattlefieldSafeBounds(), 0.02f);
        }

        public Vector3 GetSafeSpawnPosition(int laneIndex, float estimatedHalfWidth)
        {
            Vector3 spawnPosition = GetSpawnPosition(laneIndex);
            Bounds safeBounds = GetBattlefieldSafeBounds();
            spawnPosition.x = Mathf.Min(spawnPosition.x, safeBounds.max.x - Mathf.Max(0f, estimatedHalfWidth));
            spawnPosition.x = Mathf.Max(spawnPosition.x, safeBounds.min.x + Mathf.Max(0f, estimatedHalfWidth));
            spawnPosition.y = GetLaneY(laneIndex);
            return spawnPosition;
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

        private void EnsureRuntimeBattlefieldVisuals()
        {
            if (!createRuntimeBattlefieldVisuals)
            {
                return;
            }

            if (transform.Find(RuntimeVisualRootName) != null)
            {
                return;
            }

            GameObject visualRoot = new GameObject(RuntimeVisualRootName);
            visualRoot.transform.SetParent(transform);
            visualRoot.transform.localPosition = Vector3.zero;

            float minLaneY = GetLaneY(0) - 1f;
            float maxLaneY = GetLaneY(LaneCount - 1) + 1f;
            float height = Mathf.Max(6.2f, maxLaneY - minLaneY);
            float pathCenterX = (spawnX + crystalX) * 0.5f;
            float pathWidth = Mathf.Abs(spawnX - crystalX);

            Bounds cameraBounds = RuntimeSpriteBoundsUtility.GetCameraWorldBounds();
            Vector2 viewportBackdropSize = new Vector2(
                Mathf.Max(pathWidth + 1.2f, cameraBounds.size.x + 0.2f),
                Mathf.Max(height + 0.8f, cameraBounds.size.y + 0.2f));
            CreateRuntimeVisual("Battlefield Viewport Backdrop", visualRoot.transform,
                new Vector3(cameraBounds.center.x, cameraBounds.center.y, 0.4f),
                new Color(0.018f, 0.052f, 0.043f, 1f), viewportBackdropSize, -21);

            Sprite backgroundSprite = RuntimePixelAssetLoader.LoadSprite(RuntimePixelAssetLoader.BackgroundGoblinForestLanes);
            if (backgroundSprite != null)
            {
                CreateRuntimeSpriteVisual("Goblin Forest Lane Background", visualRoot.transform, backgroundSprite, new Vector3(pathCenterX, 0f, 0.35f),
                    new Color(0.74f, 0.8f, 0.72f, 0.78f), new Vector2(pathWidth + 1.2f, height + 0.8f), -20);
            }
            else
            {
                CreateRuntimeVisual("Battlefield Backdrop", visualRoot.transform, new Vector3(pathCenterX, 0f, 0.35f),
                    new Color(0.035f, 0.052f, 0.06f, 1f), new Vector2(pathWidth + 1.2f, height + 0.8f), -20);
            }

            CreateRuntimeVisual("Crystal Ward Zone", visualRoot.transform, new Vector3(crystalX, 0f, 0.3f),
                new Color(0.12f, 0.46f, 0.62f, 0.24f), new Vector2(0.7f, height), -15);
            CreateRuntimeVisual("Spawn Rift Zone", visualRoot.transform, new Vector3(spawnX, 0f, 0.3f),
                new Color(0.48f, 0.14f, 0.18f, 0.22f), new Vector2(0.75f, height), -15);

            for (int laneIndex = 0; laneIndex < LaneCount; laneIndex++)
            {
                float y = GetLaneY(laneIndex);
                CreateRuntimeVisual($"Lane {laneIndex} Ground", visualRoot.transform, new Vector3(pathCenterX, y, 0.25f),
                    new Color(0.14f, 0.17f, 0.16f, 0.72f), new Vector2(pathWidth, laneStripHeight), -12);
                CreateRuntimeVisual($"Lane {laneIndex} Center Line", visualRoot.transform, new Vector3(pathCenterX, y, 0.2f),
                    new Color(0.36f, 0.42f, 0.44f, 0.55f), new Vector2(pathWidth, 0.055f), -11);

                for (int slotIndex = 0; slotIndex < HeroSlotsPerLane; slotIndex++)
                {
                    Vector3 slotPosition = GetHeroSlotPosition(laneIndex, slotIndex);
                    CreateRuntimeVisual($"Lane {laneIndex} Slot {slotIndex} Marker", visualRoot.transform,
                        new Vector3(slotPosition.x, slotPosition.y + heroSlotMarkerYOffset, 0.18f),
                        new Color(0.22f, 0.5f, 0.7f, 0.28f), new Vector2(0.48f, 0.1f), -10);
                }
            }
        }

        private static GameObject CreateRuntimeVisual(string objectName, Transform parent, Vector3 position, Color color, Vector2 size, int sortingOrder)
        {
            GameObject visualObject = new GameObject(objectName);
            visualObject.transform.SetParent(parent);
            visualObject.transform.localPosition = position;
            visualObject.AddComponent<SpriteRenderer>();
            PlaceholderSprite placeholderSprite = visualObject.AddComponent<PlaceholderSprite>();
            placeholderSprite.Configure(color, size, sortingOrder);
            return visualObject;
        }

        private static GameObject CreateRuntimeSpriteVisual(string objectName, Transform parent, Sprite sprite, Vector3 position, Color color, Vector2 targetSize, int sortingOrder)
        {
            GameObject visualObject = new GameObject(objectName);
            visualObject.transform.SetParent(parent);
            visualObject.transform.localPosition = position;
            SpriteRenderer spriteRenderer = visualObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = sortingOrder;

            Bounds bounds = sprite.bounds;
            float width = Mathf.Max(0.01f, bounds.size.x);
            float height = Mathf.Max(0.01f, bounds.size.y);
            visualObject.transform.localScale = new Vector3(
                Mathf.Max(0.01f, targetSize.x) / width,
                Mathf.Max(0.01f, targetSize.y) / height,
                1f);
            return visualObject;
        }
    }
}
