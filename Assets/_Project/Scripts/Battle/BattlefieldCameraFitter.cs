using UnityEngine;

namespace RuneGate
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class BattlefieldCameraFitter : MonoBehaviour
    {
        [SerializeField] private Vector2 worldCenter = new Vector2(0.3f, 0f);
        [SerializeField] private float minimumWorldWidth = 12.5f;
        [SerializeField] private float minimumWorldHeight = 7.5f;

        private Camera targetCamera;
        private int lastScreenWidth = -1;
        private int lastScreenHeight = -1;
        private Rect lastSafeArea;

        public void Configure(Vector2 center, float worldWidth, float worldHeight)
        {
            worldCenter = center;
            minimumWorldWidth = Mathf.Max(1f, worldWidth);
            minimumWorldHeight = Mathf.Max(1f, worldHeight);
            ApplyLayout();
        }

        private void OnEnable()
        {
            targetCamera = GetComponent<Camera>();
            ApplyLayout();
        }

        private void LateUpdate()
        {
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight || Screen.safeArea != lastSafeArea)
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

            Rect battlefield = GameFrameLayout.BattleFrame().BattleFieldFrame;
            if (battlefield.width <= 1f || battlefield.height <= 1f)
            {
                return;
            }

            targetCamera.rect = new Rect(
                battlefield.x / Screen.width,
                (Screen.height - battlefield.yMax) / Screen.height,
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
        }
    }
}
