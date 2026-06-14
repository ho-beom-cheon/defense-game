using UnityEngine;

namespace RuneGate
{
    public sealed class AutoDestroyEffect : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.35f;
        [SerializeField] private bool fadeOut = true;
        [SerializeField] private bool scaleOut = true;

        private SpriteRenderer[] spriteRenderers;
        private Color[] startColors;
        private Vector3 startScale;
        private float elapsed;

        private void Awake()
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            startColors = new Color[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                startColors[i] = spriteRenderers[i] != null ? spriteRenderers[i].color : Color.white;
            }

            startScale = transform.localScale;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float percent = lifetime > 0f ? Mathf.Clamp01(elapsed / lifetime) : 1f;

            if (fadeOut)
            {
                ApplyFade(percent);
            }

            if (scaleOut)
            {
                transform.localScale = Vector3.Lerp(startScale, startScale * 1.35f, percent);
            }

            if (elapsed >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        public void Configure(float nextLifetime)
        {
            lifetime = Mathf.Max(0.01f, nextLifetime);
        }

        private void ApplyFade(float percent)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                SpriteRenderer spriteRenderer = spriteRenderers[i];
                if (spriteRenderer == null)
                {
                    continue;
                }

                Color color = i < startColors.Length ? startColors[i] : spriteRenderer.color;
                color.a = Mathf.Lerp(color.a, 0f, percent);
                spriteRenderer.color = color;
            }
        }
    }
}
