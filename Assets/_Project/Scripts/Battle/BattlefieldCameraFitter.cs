using UnityEngine;

namespace RuneGate
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class BattlefieldCameraFitter : MonoBehaviour
    {
        [SerializeField] private Vector2 worldCenter = new Vector2(0.15f, 0f);
        [SerializeField] private float minimumWorldWidth = 12.5f;
        [SerializeField] private float minimumWorldHeight = 7.5f;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private Canvas viewportCanvas;

        private Camera targetCamera;
        private Camera backgroundCamera;
        private int lastScreenWidth = -1;
        private int lastScreenHeight = -1;
        private Rect lastSafeArea;
        private Rect lastViewportRect;

        public Rect CurrentViewportScreenRect => lastViewportRect;

        public void Configure(Vector2 center, float worldWidth, float worldHeight)
        {
            worldCenter = center;
            minimumWorldWidth = Mathf.Max(1f, worldWidth);
            minimumWorldHeight = Mathf.Max(1f, worldHeight);
            ApplyLayout();
        }

        public void ConfigureViewport(RectTransform targetViewport, Canvas canvas)
        {
            viewport = targetViewport;
            viewportCanvas = canvas;
            ApplyLayout();
        }

        private void OnEnable()
        {
            targetCamera = GetComponent<Camera>();
            EnsureBackgroundCamera();
            ApplyLayout();
        }

        private void LateUpdate()
        {
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight || Screen.safeArea != lastSafeArea || ViewportChanged())
            {
                ApplyLayout();
            }
        }

        private void ApplyLayout()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }

            if (targetCamera == null || !targetCamera.orthographic || Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            if (!TryGetViewportScreenRect(out Rect battlefield))
            {
                return;
            }

            targetCamera.rect = new Rect(
                battlefield.x / Screen.width,
                battlefield.y / Screen.height,
                battlefield.width / Screen.width,
                battlefield.height / Screen.height);

            float viewportAspect = battlefield.width / battlefield.height;
            targetCamera.orthographicSize = Mathf.Max(
                minimumWorldHeight * 0.5f,
                minimumWorldWidth / (2f * Mathf.Max(0.01f, viewportAspect)));

            Vector3 cameraPosition = targetCamera.transform.position;
            cameraPosition.x = worldCenter.x;
            cameraPosition.y = worldCenter.y;
            targetCamera.transform.position = cameraPosition;

            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            lastSafeArea = Screen.safeArea;
            lastViewportRect = battlefield;
        }

        private void EnsureBackgroundCamera()
        {
            if (!Application.isPlaying || targetCamera == null || backgroundCamera != null)
            {
                return;
            }

            GameObject backgroundObject = new GameObject("Battle Background Camera");
            backgroundObject.transform.SetParent(transform, false);
            backgroundCamera = backgroundObject.AddComponent<Camera>();
            backgroundCamera.clearFlags = CameraClearFlags.SolidColor;
            backgroundCamera.backgroundColor = targetCamera.backgroundColor;
            backgroundCamera.cullingMask = 0;
            backgroundCamera.depth = targetCamera.depth - 1f;
            backgroundCamera.rect = new Rect(0f, 0f, 1f, 1f);
            backgroundCamera.allowHDR = false;
            backgroundCamera.allowMSAA = false;
            backgroundCamera.targetDisplay = targetCamera.targetDisplay;
        }

        private bool ViewportChanged()
        {
            return TryGetViewportScreenRect(out Rect current) && current != lastViewportRect;
        }

        private bool TryGetViewportScreenRect(out Rect screenRect)
        {
            screenRect = default;
            if (viewport == null || !viewport.gameObject.activeInHierarchy || Screen.width <= 0 || Screen.height <= 0)
            {
                return false;
            }

            Vector3[] corners = new Vector3[4];
            viewport.GetWorldCorners(corners);
            Camera eventCamera = viewportCanvas != null && viewportCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? viewportCanvas.worldCamera
                : null;
            Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[0]);
            Vector2 topRight = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[2]);
            float xMin = Mathf.Clamp(Mathf.Min(bottomLeft.x, topRight.x), 0f, Screen.width);
            float xMax = Mathf.Clamp(Mathf.Max(bottomLeft.x, topRight.x), 0f, Screen.width);
            float yMin = Mathf.Clamp(Mathf.Min(bottomLeft.y, topRight.y), 0f, Screen.height);
            float yMax = Mathf.Clamp(Mathf.Max(bottomLeft.y, topRight.y), 0f, Screen.height);
            screenRect = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
            return screenRect.width > 1f && screenRect.height > 1f;
        }
    }
}
