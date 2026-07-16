using UnityEngine;

namespace RuneGate
{
    public static class CombatGeometry
    {
        public static float CenterDistance(Vector2 a, Vector2 b)
        {
            return Vector2.Distance(a, b);
        }

        public static bool IsCenterInRange(Vector2 a, Vector2 b, float range)
        {
            float safeRange = Mathf.Max(0f, range);
            return (a - b).sqrMagnitude <= safeRange * safeRange;
        }

        public static float ContactDistance(
            Vector2 a,
            float radiusA,
            Vector2 b,
            float radiusB)
        {
            return Mathf.Max(0f, Vector2.Distance(a, b) - Mathf.Max(0f, radiusA) - Mathf.Max(0f, radiusB));
        }

        public static float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            Vector2 segment = end - start;
            float lengthSquared = segment.sqrMagnitude;
            if (lengthSquared <= 0.0001f)
            {
                return Vector2.Distance(point, start);
            }

            float t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSquared);
            return Vector2.Distance(point, start + segment * t);
        }
    }
}
