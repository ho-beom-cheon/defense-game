using UnityEngine;

namespace RuneGate
{
    public sealed class PanelClampToScreen : MonoBehaviour
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private Vector2 margin = new Vector2(16f, 16f);

        private void Awake()
        {
            if (target == null)
            {
                target = transform as RectTransform;
            }
        }

        private void LateUpdate()
        {
            Clamp();
        }

        public void Clamp()
        {
            if (target == null)
            {
                return;
            }

            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);
            Vector3 offset = Vector3.zero;

            if (corners[0].x < margin.x)
            {
                offset.x = margin.x - corners[0].x;
            }
            else if (corners[2].x > Screen.width - margin.x)
            {
                offset.x = Screen.width - margin.x - corners[2].x;
            }

            if (corners[0].y < margin.y)
            {
                offset.y = margin.y - corners[0].y;
            }
            else if (corners[2].y > Screen.height - margin.y)
            {
                offset.y = Screen.height - margin.y - corners[2].y;
            }

            target.position += offset;
        }
    }
}
