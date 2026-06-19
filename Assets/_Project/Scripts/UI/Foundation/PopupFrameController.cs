using UnityEngine;

namespace RuneGate
{
    public sealed class PopupFrameController : MonoBehaviour
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private Vector2 preferredSize = new Vector2(760f, 980f);
        [SerializeField] private Vector2 screenRatio = new Vector2(0.92f, 0.78f);

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

            Rect popup = GameFrameLayout.PopupFrame(preferredSize.x, preferredSize.y, screenRatio.x, screenRatio.y);
            target.anchorMin = new Vector2(0.5f, 0.5f);
            target.anchorMax = new Vector2(0.5f, 0.5f);
            target.pivot = new Vector2(0.5f, 0.5f);
            target.sizeDelta = popup.size;
            target.anchoredPosition = Vector2.zero;
        }
    }
}
