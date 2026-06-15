using UnityEngine;

namespace RuneGate
{
    public sealed class RuntimeSpriteFitter : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float targetHeight = 1f;

        public float TargetHeight
        {
            get => targetHeight;
            set => targetHeight = value;
        }

        private void Awake()
        {
            FitNow();
        }

        private void LateUpdate()
        {
            FitNow();
        }

        public void FitNow()
        {
            if (targetHeight <= 0f)
            {
                return;
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                return;
            }

            float currentHeight = spriteRenderer.bounds.size.y;
            if (currentHeight <= 0.0001f)
            {
                return;
            }

            float scaleFactor = targetHeight / currentHeight;
            Vector3 localScale = transform.localScale;
            transform.localScale = new Vector3(localScale.x * scaleFactor, localScale.y * scaleFactor, localScale.z);
        }
    }
}
