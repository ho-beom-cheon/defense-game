using UnityEngine;

namespace RuneGate
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class GroundFieldRenderer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private float baseUniformScale = 1.1f;
        [SerializeField] private float layoutPadding = 0.6f;

        public bool IsReady => targetRenderer != null && targetRenderer.sprite != null;
        public SpriteRenderer Renderer => targetRenderer;

        public void Configure(
            Sprite sprite,
            Color tint,
            int sortingOrder,
            float uniformScale)
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<SpriteRenderer>();
            }

            baseUniformScale = Mathf.Max(0.1f, uniformScale);
            targetRenderer.sprite = sprite;
            targetRenderer.color = tint;
            targetRenderer.sortingOrder = sortingOrder;
            targetRenderer.drawMode = SpriteDrawMode.Tiled;
            targetRenderer.tileMode = SpriteTileMode.Continuous;
        }

        public void RefreshLayout(Bounds worldBounds)
        {
            if (!IsReady || worldBounds.size.x <= 0.01f || worldBounds.size.y <= 0.01f)
            {
                return;
            }

            Vector2 paddedSize = new Vector2(
                worldBounds.size.x + Mathf.Max(0f, layoutPadding),
                worldBounds.size.y + Mathf.Max(0f, layoutPadding));
            transform.localScale = Vector3.one * baseUniformScale;
            targetRenderer.size = paddedSize / baseUniformScale;
            transform.position = new Vector3(worldBounds.center.x, worldBounds.center.y, 0.3f);
        }
    }
}
