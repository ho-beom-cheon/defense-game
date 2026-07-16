using UnityEngine;

namespace RuneGate
{
    public static class RuntimeSpriteBoundsUtility
    {
        public static Bounds GetCameraWorldBounds(Camera camera = null)
        {
            Camera targetCamera = camera != null ? camera : Camera.main;
            if (targetCamera == null)
            {
                return new Bounds(Vector3.zero, new Vector3(12f, 8f, 1f));
            }

            if (targetCamera.orthographic)
            {
                float height = targetCamera.orthographicSize * 2f;
                float width = height * targetCamera.aspect;
                return new Bounds(targetCamera.transform.position, new Vector3(width, height, 1f));
            }

            Vector3 bottomLeft = targetCamera.ViewportToWorldPoint(new Vector3(0f, 0f, Mathf.Abs(targetCamera.transform.position.z)));
            Vector3 topRight = targetCamera.ViewportToWorldPoint(new Vector3(1f, 1f, Mathf.Abs(targetCamera.transform.position.z)));
            Bounds bounds = new Bounds((bottomLeft + topRight) * 0.5f, new Vector3(Mathf.Abs(topRight.x - bottomLeft.x), Mathf.Abs(topRight.y - bottomLeft.y), 1f));
            return bounds;
        }

        public static void AlignVisualBottomToGround(SpriteRenderer spriteRenderer, Transform rootTransform, float groundY)
        {
            if (spriteRenderer == null || rootTransform == null || spriteRenderer.transform == rootTransform)
            {
                return;
            }

            Bounds bounds = spriteRenderer.bounds;
            if (bounds.size.y <= 0.0001f)
            {
                return;
            }

            float deltaY = groundY - bounds.min.y;
            spriteRenderer.transform.position += new Vector3(0f, deltaY, 0f);
        }

        public static void ClampRootInsideBounds(Transform rootTransform, SpriteRenderer spriteRenderer, Bounds safeBounds, float padding = 0.02f)
        {
            if (rootTransform == null || spriteRenderer == null)
            {
                return;
            }

            Bounds spriteBounds = spriteRenderer.bounds;
            if (spriteBounds.size.x <= 0.0001f || spriteBounds.size.y <= 0.0001f)
            {
                return;
            }

            Vector3 delta = Vector3.zero;
            float minX = safeBounds.min.x + padding;
            float maxX = safeBounds.max.x - padding;
            float minY = safeBounds.min.y + padding;
            float maxY = safeBounds.max.y - padding;

            if (spriteBounds.min.x < minX)
            {
                delta.x = minX - spriteBounds.min.x;
            }
            else if (spriteBounds.max.x > maxX)
            {
                delta.x = maxX - spriteBounds.max.x;
            }

            if (spriteBounds.min.y < minY)
            {
                delta.y = minY - spriteBounds.min.y;
            }
            else if (spriteBounds.max.y > maxY)
            {
                delta.y = maxY - spriteBounds.max.y;
            }

            rootTransform.position += delta;
        }

        public static float ClampWorldYInsideCamera(float worldY, float topPadding = 0.12f, float bottomPadding = 0.12f)
        {
            Bounds cameraBounds = GetCameraWorldBounds();
            return Mathf.Clamp(worldY, cameraBounds.min.y + bottomPadding, cameraBounds.max.y - topPadding);
        }
    }
}
