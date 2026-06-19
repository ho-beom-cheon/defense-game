using UnityEngine;

namespace RuneGate
{
    public sealed class FrameRootLimiter : MonoBehaviour
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private Vector2 maxSize = new Vector2(GameFrameLayout.ReferenceWidth, GameFrameLayout.ReferenceHeight);

        private void Awake()
        {
            if (target == null)
            {
                target = transform as RectTransform;
            }
        }

        private void LateUpdate()
        {
            Apply();
        }

        public void Apply()
        {
            if (target == null)
            {
                return;
            }

            Rect safeArea = Screen.safeArea;
            float width = safeArea.width > 0f ? safeArea.width : Screen.width;
            float height = safeArea.height > 0f ? safeArea.height : Screen.height;
            target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Min(width, Mathf.Max(1f, maxSize.x)));
            target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Min(height, Mathf.Max(1f, maxSize.y)));
            target.anchoredPosition = Vector2.zero;
        }
    }
}
