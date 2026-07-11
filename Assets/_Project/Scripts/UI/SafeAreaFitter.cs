using UnityEngine;

namespace RuneGate
{
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private bool applyOnStart = true;
        [SerializeField] private bool useDebugSafeArea;
        [SerializeField] private Rect debugSafeArea = new Rect(0f, 80f, 1080f, 1760f);

        private Rect lastSafeArea;

        private void Awake()
        {
            if (target == null)
            {
                target = transform as RectTransform;
            }
        }

        private void Start()
        {
            if (applyOnStart)
            {
                ApplySafeArea();
            }
        }

        private void Update()
        {
            Rect safeArea = GetSafeArea();
            if (target == null || lastSafeArea == safeArea)
            {
                return;
            }

            ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            if (target == null)
            {
                return;
            }

            lastSafeArea = GetSafeArea();
            Vector2 anchorMin = lastSafeArea.position;
            Vector2 anchorMax = lastSafeArea.position + lastSafeArea.size;
            float width = Mathf.Max(1f, Screen.width);
            float height = Mathf.Max(1f, Screen.height);
            anchorMin.x /= width;
            anchorMin.y /= height;
            anchorMax.x /= width;
            anchorMax.y /= height;
            target.anchorMin = anchorMin;
            target.anchorMax = anchorMax;
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;
        }

        private Rect GetSafeArea()
        {
            Rect safeArea = useDebugSafeArea ? debugSafeArea : Screen.safeArea;
            if (safeArea.width <= 0f || safeArea.height <= 0f)
            {
                safeArea = new Rect(0f, 0f, Screen.width, Screen.height);
            }

            return safeArea;
        }
    }
}
