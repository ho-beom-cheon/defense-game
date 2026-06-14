using System.Collections;
using UnityEngine;

namespace RuneGate
{
    public sealed class HitFlashController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] spriteRenderers;
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float flashDuration = 0.08f;

        private Color[] originalColors;
        private Coroutine flashRoutine;

        private void Awake()
        {
            EnsureSpriteRenderers();
            CaptureOriginalColors();
        }

        public void Flash()
        {
            Flash(flashColor, flashDuration);
        }

        public void Flash(Color color, float duration)
        {
            EnsureSpriteRenderers();
            if (spriteRenderers == null || spriteRenderers.Length == 0 || duration <= 0f)
            {
                return;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(FlashRoutine(color, duration));
        }

        private void EnsureSpriteRenderers()
        {
            if (spriteRenderers == null || spriteRenderers.Length == 0)
            {
                spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            }
        }

        private void CaptureOriginalColors()
        {
            EnsureSpriteRenderers();
            if (spriteRenderers == null)
            {
                originalColors = new Color[0];
                return;
            }

            originalColors = new Color[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                originalColors[i] = spriteRenderers[i] != null ? spriteRenderers[i].color : Color.white;
            }
        }

        private IEnumerator FlashRoutine(Color color, float duration)
        {
            if (originalColors == null || originalColors.Length != spriteRenderers.Length)
            {
                CaptureOriginalColors();
            }

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    spriteRenderers[i].color = color;
                }
            }

            yield return new WaitForSeconds(duration);

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null && i < originalColors.Length)
                {
                    spriteRenderers[i].color = originalColors[i];
                }
            }

            flashRoutine = null;
        }
    }
}
