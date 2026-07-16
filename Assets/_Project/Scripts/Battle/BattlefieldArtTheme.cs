using UnityEngine;

namespace RuneGate
{
    [CreateAssetMenu(fileName = "BattlefieldArtTheme", menuName = "RuneGate/Art/Battlefield Theme")]
    public sealed class BattlefieldArtTheme : ScriptableObject
    {
        [Header("Stage Art")]
        [SerializeField] private Sprite background;
        [SerializeField] private Sprite lane;
        [SerializeField] private Sprite crystal;
        [SerializeField] private Sprite rift;
        [SerializeField] private Sprite unitShadow;
        [SerializeField] private Sprite heroSlotRune;
        [SerializeField] private Sprite crystalShieldRing;
        [SerializeField] private Sprite riftPulse;

        [Header("Palette")]
        [SerializeField] private Color backgroundTint = new Color(0.82f, 0.88f, 0.86f, 1f);
        [SerializeField] private Color laneTint = new Color(0.78f, 0.8f, 0.76f, 1f);
        [SerializeField] private Color crystalTint = new Color(0.72f, 0.8f, 0.82f, 1f);
        [SerializeField] private Color riftTint = new Color(0.68f, 0.7f, 0.74f, 1f);
        [SerializeField] private Color shadowTint = new Color(0.08f, 0.17f, 0.17f, 1f);
        [SerializeField] private Color shieldTint = new Color(0.27f, 0.72f, 0.85f, 0.7f);
        [SerializeField] private Color riftIdleTint = new Color(0.55f, 0.36f, 0.88f, 0.38f);
        [SerializeField] private Color riftWarningTint = new Color(0.66f, 0.4f, 0.95f, 0.68f);
        [SerializeField] private Color riftBossTint = new Color(0.84f, 0.36f, 0.55f, 0.82f);
        [SerializeField] private Color crystalCriticalTint = new Color(0.84f, 0.36f, 0.36f, 1f);

        [Header("Sorting")]
        [SerializeField] private int backgroundSortingOrder = -30;
        [SerializeField] private int laneSortingOrder = -20;
        [SerializeField] private int decalSortingOrder = -10;
        [SerializeField] private int objectiveSortingOrder = 2;
        [SerializeField] private int unitShadowSortingOrder = 3;
        [SerializeField] private int worldEffectSortingOrder = 10;

        [Header("World Sizing")]
        [SerializeField] private float laneWorldHeight = 1.05f;
        [SerializeField] private float crystalWorldHeight = 4.8f;
        [SerializeField] private float riftWorldHeight = 4.9f;
        [SerializeField] private float portraitObjectiveScale = 0.92f;
        [SerializeField] private float landscapeObjectiveScale = 0.82f;
        [SerializeField] private float objectiveEdgePadding = 0.28f;

        [Header("Motion")]
        [SerializeField] private float riftPulseSeconds = 1.8f;
        [SerializeField] private float riftPulseScale = 0.018f;
        [SerializeField] private float riftWarningSeconds = 0.8f;
        [SerializeField] private float crystalDamageSeconds = 0.18f;
        [SerializeField] private float crystalCriticalPulseSeconds = 0.65f;

        public Sprite Background => background;
        public Sprite Lane => lane;
        public Sprite Crystal => crystal;
        public Sprite Rift => rift;
        public Sprite UnitShadow => unitShadow;
        public Sprite HeroSlotRune => heroSlotRune;
        public Sprite CrystalShieldRing => crystalShieldRing;
        public Sprite RiftPulse => riftPulse;
        public Color BackgroundTint => backgroundTint;
        public Color LaneTint => laneTint;
        public Color CrystalTint => crystalTint;
        public Color RiftTint => riftTint;
        public Color ShadowTint => shadowTint;
        public Color ShieldTint => shieldTint;
        public Color RiftIdleTint => riftIdleTint;
        public Color RiftWarningTint => riftWarningTint;
        public Color RiftBossTint => riftBossTint;
        public Color CrystalCriticalTint => crystalCriticalTint;
        public int BackgroundSortingOrder => backgroundSortingOrder;
        public int LaneSortingOrder => laneSortingOrder;
        public int DecalSortingOrder => decalSortingOrder;
        public int ObjectiveSortingOrder => objectiveSortingOrder;
        public int UnitShadowSortingOrder => unitShadowSortingOrder;
        public int WorldEffectSortingOrder => worldEffectSortingOrder;
        public float LaneWorldHeight => Mathf.Max(0.2f, laneWorldHeight);
        public float CrystalWorldHeight => Mathf.Max(0.5f, crystalWorldHeight);
        public float RiftWorldHeight => Mathf.Max(0.5f, riftWorldHeight);
        public float PortraitObjectiveScale => Mathf.Max(0.1f, portraitObjectiveScale);
        public float LandscapeObjectiveScale => Mathf.Max(0.1f, landscapeObjectiveScale);
        public float ObjectiveEdgePadding => Mathf.Max(0f, objectiveEdgePadding);
        public float RiftPulseSeconds => Mathf.Max(0.1f, riftPulseSeconds);
        public float RiftPulseScale => Mathf.Clamp(riftPulseScale, 0f, 0.08f);
        public float RiftWarningSeconds => Mathf.Max(0.1f, riftWarningSeconds);
        public float CrystalDamageSeconds => Mathf.Max(0.05f, crystalDamageSeconds);
        public float CrystalCriticalPulseSeconds => Mathf.Max(0.1f, crystalCriticalPulseSeconds);

        public bool HasRequiredAssets => background != null
            && lane != null
            && crystal != null
            && rift != null
            && unitShadow != null
            && heroSlotRune != null
            && crystalShieldRing != null
            && riftPulse != null;
    }
}
