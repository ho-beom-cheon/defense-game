using UnityEngine;

namespace RuneGate
{
    public enum BattlefieldMode
    {
        LegacyLanes,
        Continuous2D
    }

    public readonly struct BattlefieldBounds
    {
        public BattlefieldBounds(Rect playableRect, Rect heroHomeRect, Rect riftSpawnRect)
        {
            PlayableRect = playableRect;
            HeroHomeRect = heroHomeRect;
            RiftSpawnRect = riftSpawnRect;
        }

        public Rect PlayableRect { get; }
        public Rect HeroHomeRect { get; }
        public Rect RiftSpawnRect { get; }
        public bool IsValid => PlayableRect.width > 0.01f && PlayableRect.height > 0.01f;

        public Vector2 Clamp(Vector2 position, Vector2 halfExtents)
        {
            float halfWidth = Mathf.Clamp(Mathf.Abs(halfExtents.x), 0f, PlayableRect.width * 0.49f);
            float halfHeight = Mathf.Clamp(Mathf.Abs(halfExtents.y), 0f, PlayableRect.height * 0.49f);
            return new Vector2(
                Mathf.Clamp(position.x, PlayableRect.xMin + halfWidth, PlayableRect.xMax - halfWidth),
                Mathf.Clamp(position.y, PlayableRect.yMin + halfHeight, PlayableRect.yMax - halfHeight));
        }

        public Vector2 ToWorld(Vector2 normalized)
        {
            return new Vector2(
                Mathf.Lerp(PlayableRect.xMin, PlayableRect.xMax, Mathf.Clamp01(normalized.x)),
                Mathf.Lerp(PlayableRect.yMin, PlayableRect.yMax, Mathf.Clamp01(normalized.y)));
        }

        public Vector2 ToNormalized(Vector2 world)
        {
            if (!IsValid)
            {
                return Vector2.zero;
            }

            return new Vector2(
                Mathf.InverseLerp(PlayableRect.xMin, PlayableRect.xMax, world.x),
                Mathf.InverseLerp(PlayableRect.yMin, PlayableRect.yMax, world.y));
        }

        public Rect NormalizedRectToWorld(Rect normalizedRect)
        {
            Vector2 min = ToWorld(normalizedRect.min);
            Vector2 max = ToWorld(normalizedRect.max);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }
}
