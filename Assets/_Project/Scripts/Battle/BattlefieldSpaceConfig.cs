using UnityEngine;

namespace RuneGate
{
    [CreateAssetMenu(fileName = "BattlefieldSpaceConfig", menuName = "RuneGate/Battle/Battlefield Space Config")]
    public sealed class BattlefieldSpaceConfig : ScriptableObject
    {
        [Header("Camera And Playable Bounds")]
        [SerializeField] private float minimumWorldWidth = 12.5f;
        [SerializeField] private float minimumWorldHeight = 7.5f;
        [SerializeField] private Vector4 playablePadding = new Vector4(2.15f, 0.45f, 0.3f, 0.3f);
        [SerializeField] private Rect heroHomeNormalizedRect = new Rect(0.08f, 0.08f, 0.46f, 0.84f);
        [SerializeField] private Rect riftSpawnNormalizedRect = new Rect(0.86f, 0.18f, 0.1f, 0.64f);

        [Header("Formation Anchors")]
        [SerializeField] private float[] formationRowV = { 0.22f, 0.5f, 0.78f };
        [SerializeField] private float backAnchorU = 0.2f;
        [SerializeField] private float middleAnchorU = 0.3f;
        [SerializeField] private float frontAnchorU = 0.4f;

        [Header("Enemy Spawn Bands")]
        [SerializeField] private Vector2[] spawnBandV =
        {
            new Vector2(0.26f, 0.5f),
            new Vector2(0.38f, 0.62f),
            new Vector2(0.5f, 0.74f)
        };

        [Header("Hero Leash Ratios")]
        [SerializeField] private Vector3 tankMeleeLeash = new Vector3(0.28f, 0.08f, 0.24f);
        [SerializeField] private Vector3 assassinLeash = new Vector3(0.36f, 0.1f, 0.32f);
        [SerializeField] private Vector3 rangedMageLeash = new Vector3(0.18f, 0.12f, 0.26f);
        [SerializeField] private Vector3 supportLeash = new Vector3(0.12f, 0.12f, 0.22f);

        [Header("Crystal Approach")]
        [SerializeField] private float[] approachPointV = { 0.18f, 0.29f, 0.4f, 0.5f, 0.6f, 0.71f, 0.82f };
        [SerializeField] private float approachBaseU = 0.045f;
        [SerializeField] private float approachArcU = 0.08f;
        [SerializeField] private float approachCongestionCost = 1.1f;
        [SerializeField] private float approachBandBias = 0.35f;

        [Header("Steering")]
        [SerializeField, Range(0f, 0.35f)] private float separationWeight = 0.35f;
        [SerializeField] private float targetLockSlack = 0.4f;

        public float MinimumWorldWidth => Mathf.Max(1f, minimumWorldWidth);
        public float MinimumWorldHeight => Mathf.Max(1f, minimumWorldHeight);
        public Vector4 PlayablePadding => new Vector4(
            Mathf.Max(0f, playablePadding.x),
            Mathf.Max(0f, playablePadding.y),
            Mathf.Max(0f, playablePadding.z),
            Mathf.Max(0f, playablePadding.w));
        public Rect HeroHomeNormalizedRect => ClampNormalizedRect(heroHomeNormalizedRect);
        public Rect RiftSpawnNormalizedRect => ClampNormalizedRect(riftSpawnNormalizedRect);
        public float SeparationWeight => Mathf.Clamp(separationWeight, 0f, 0.35f);
        public float TargetLockSlack => Mathf.Max(0f, targetLockSlack);
        public int ApproachPointCount => approachPointV != null ? approachPointV.Length : 0;
        public float ApproachCongestionCost => Mathf.Max(0f, approachCongestionCost);
        public float ApproachBandBias => Mathf.Max(0f, approachBandBias);

        public float GetFormationRowV(int rowIndex)
        {
            if (formationRowV == null || formationRowV.Length == 0)
            {
                return 0.5f;
            }

            return Mathf.Clamp01(formationRowV[Mathf.Clamp(rowIndex, 0, formationRowV.Length - 1)]);
        }

        public float GetFormationU(HeroPositionType positionType)
        {
            switch (positionType)
            {
                case HeroPositionType.Back:
                    return Mathf.Clamp01(backAnchorU);
                case HeroPositionType.Front:
                    return Mathf.Clamp01(frontAnchorU);
                default:
                    return Mathf.Clamp01(middleAnchorU);
            }
        }

        public Vector2 GetSpawnBandV(int bandIndex)
        {
            if (spawnBandV == null || spawnBandV.Length == 0)
            {
                return new Vector2(0.38f, 0.62f);
            }

            Vector2 band = spawnBandV[Mathf.Clamp(bandIndex, 0, spawnBandV.Length - 1)];
            float min = Mathf.Clamp01(Mathf.Min(band.x, band.y));
            float max = Mathf.Clamp01(Mathf.Max(band.x, band.y));
            return new Vector2(min, max);
        }

        public float GetApproachPointV(int index)
        {
            if (approachPointV == null || approachPointV.Length == 0)
            {
                return 0.5f;
            }

            return Mathf.Clamp01(approachPointV[Mathf.Clamp(index, 0, approachPointV.Length - 1)]);
        }

        public float GetApproachPointU(int index)
        {
            float verticalDistance = Mathf.Abs(GetApproachPointV(index) - 0.5f) * 2f;
            return Mathf.Clamp01(approachBaseU + verticalDistance * Mathf.Max(0f, approachArcU));
        }

        public Vector3 GetRoleLeash(HeroRole role)
        {
            switch (role)
            {
                case HeroRole.Tank:
                case HeroRole.MeleeDps:
                    return SanitizeLeash(tankMeleeLeash);
                case HeroRole.Assassin:
                    return SanitizeLeash(assassinLeash);
                case HeroRole.RangedDps:
                case HeroRole.Mage:
                    return SanitizeLeash(rangedMageLeash);
                default:
                    return SanitizeLeash(supportLeash);
            }
        }

        public bool HasValidLayout()
        {
            if (formationRowV == null || formationRowV.Length != 3 ||
                spawnBandV == null || spawnBandV.Length != 3 ||
                approachPointV == null || approachPointV.Length != 7)
            {
                return false;
            }

            for (int i = 0; i < formationRowV.Length; i++)
            {
                for (int j = i + 1; j < formationRowV.Length; j++)
                {
                    if (Mathf.Abs(formationRowV[i] - formationRowV[j]) < 0.01f)
                    {
                        return false;
                    }
                }
            }

            return Mathf.Abs(backAnchorU - middleAnchorU) >= 0.01f
                && Mathf.Abs(middleAnchorU - frontAnchorU) >= 0.01f
                && Mathf.Abs(backAnchorU - frontAnchorU) >= 0.01f;
        }

        private static Vector3 SanitizeLeash(Vector3 value)
        {
            return new Vector3(Mathf.Max(0f, value.x), Mathf.Max(0f, value.y), Mathf.Max(0f, value.z));
        }

        private static Rect ClampNormalizedRect(Rect value)
        {
            float xMin = Mathf.Clamp01(Mathf.Min(value.xMin, value.xMax));
            float xMax = Mathf.Clamp01(Mathf.Max(value.xMin, value.xMax));
            float yMin = Mathf.Clamp01(Mathf.Min(value.yMin, value.yMax));
            float yMax = Mathf.Clamp01(Mathf.Max(value.yMin, value.yMax));
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }
    }
}
