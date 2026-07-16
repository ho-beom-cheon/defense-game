using UnityEngine;

namespace RuneGate
{
    public enum BattlefieldAgentKind
    {
        Hero,
        GroundMonster,
        FlyingMonster,
        Boss,
        Deployable
    }

    public enum BattlefieldFaction
    {
        Hero,
        Monster,
        Neutral
    }

    [DisallowMultipleComponent]
    public sealed class BattlefieldAgent : MonoBehaviour
    {
        [SerializeField] private int stableId;
        [SerializeField] private BattlefieldFaction faction;
        [SerializeField] private BattlefieldAgentKind kind;
        [SerializeField] private float radius = 0.35f;
        [SerializeField] private Vector2 halfExtents = new Vector2(0.35f, 0.5f);
        [SerializeField] private Vector2 anchor;
        [SerializeField] private Rect leashRect;

        private BattlefieldAgentRegistry registry;
        private float objectiveProgress;
        private bool configured;

        public int StableId => stableId;
        public BattlefieldFaction Faction => faction;
        public BattlefieldAgentKind Kind => kind;
        public float Radius => Mathf.Max(0f, radius);
        public Vector2 HalfExtents => new Vector2(Mathf.Max(0f, halfExtents.x), Mathf.Max(0f, halfExtents.y));
        public Vector2 Anchor => anchor;
        public Rect LeashRect => leashRect;
        public float ObjectiveProgress => objectiveProgress;
        public bool IsConfigured => configured;

        public void Configure(
            BattlefieldAgentKind nextKind,
            BattlefieldFaction nextFaction,
            int nextStableId,
            float nextRadius,
            Vector2 nextHalfExtents,
            Vector2 nextAnchor,
            Rect nextLeashRect)
        {
            kind = nextKind;
            faction = nextFaction;
            stableId = nextStableId;
            radius = Mathf.Max(0f, nextRadius);
            halfExtents = new Vector2(Mathf.Max(0f, nextHalfExtents.x), Mathf.Max(0f, nextHalfExtents.y));
            anchor = nextAnchor;
            leashRect = nextLeashRect;
            configured = true;
        }

        public void AttachRegistry(BattlefieldAgentRegistry targetRegistry)
        {
            if (registry == targetRegistry)
            {
                return;
            }

            registry?.Unregister(this);
            registry = targetRegistry;
            if (isActiveAndEnabled && configured)
            {
                registry?.Register(this);
            }
        }

        public void SetObjectiveProgress(float value)
        {
            objectiveProgress = Mathf.Clamp01(value);
        }

        public void Remap(BattlefieldBounds previous, BattlefieldBounds next)
        {
            if (!previous.IsValid || !next.IsValid)
            {
                return;
            }

            Vector2 normalizedPosition = previous.ToNormalized(transform.position);
            Vector2 normalizedAnchor = previous.ToNormalized(anchor);
            Vector2 leashMin = previous.ToNormalized(leashRect.min);
            Vector2 leashMax = previous.ToNormalized(leashRect.max);
            Vector2 nextPosition = next.Clamp(next.ToWorld(normalizedPosition), HalfExtents);
            transform.position = new Vector3(nextPosition.x, nextPosition.y, transform.position.z);
            anchor = next.ToWorld(normalizedAnchor);
            Vector2 nextLeashMin = next.ToWorld(leashMin);
            Vector2 nextLeashMax = next.ToWorld(leashMax);
            leashRect = Rect.MinMaxRect(nextLeashMin.x, nextLeashMin.y, nextLeashMax.x, nextLeashMax.y);
        }

        private void OnEnable()
        {
            if (configured)
            {
                registry?.Register(this);
            }
        }

        private void OnDisable()
        {
            registry?.Unregister(this);
        }
    }
}
