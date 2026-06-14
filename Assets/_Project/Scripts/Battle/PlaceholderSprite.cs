using UnityEngine;

namespace RuneGate
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlaceholderSprite : MonoBehaviour
    {
        [SerializeField] private Color color = Color.white;
        [SerializeField] private Vector2 size = Vector2.one;
        [SerializeField] private int sortingOrder;

        private static Sprite sharedSprite;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            ApplyVisual();
        }

        public void Configure(Color newColor, Vector2 newSize, int newSortingOrder)
        {
            color = newColor;
            size = newSize;
            sortingOrder = newSortingOrder;
            ApplyVisual();
        }

        private void ApplyVisual()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.sprite = GetSharedSprite();
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = sortingOrder;
            transform.localScale = new Vector3(Mathf.Max(0.01f, size.x), Mathf.Max(0.01f, size.y), 1f);
        }

        private static Sprite GetSharedSprite()
        {
            if (sharedSprite != null)
            {
                return sharedSprite;
            }

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Point;
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            sharedSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            sharedSprite.hideFlags = HideFlags.HideAndDontSave;
            return sharedSprite;
        }
    }
}
