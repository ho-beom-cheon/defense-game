using UnityEngine;

namespace RuneGate
{
    public sealed class DamageText : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.6f;
        [SerializeField] private Vector2 screenOffset = new Vector2(-18f, -18f);
        [SerializeField] private Color textColor = new Color(1f, 0.92f, 0.35f, 1f);

        private float remainingLifetime;
        private string displayValue;
        private Camera mainCamera;

        public string DisplayValue => displayValue;

        private void Update()
        {
            if (remainingLifetime <= 0f)
            {
                return;
            }

            remainingLifetime -= Time.deltaTime;
            if (remainingLifetime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnGUI()
        {
            if (remainingLifetime <= 0f || string.IsNullOrWhiteSpace(displayValue))
            {
                return;
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera == null)
            {
                return;
            }

            Vector3 screenPosition = mainCamera.WorldToScreenPoint(transform.position);
            if (screenPosition.z < 0f)
            {
                return;
            }

            Color previousColor = GUI.color;
            GUI.color = textColor;
            Rect rect = new Rect(screenPosition.x + screenOffset.x, Screen.height - screenPosition.y + screenOffset.y, 80f, 24f);
            GUI.Label(rect, displayValue);
            GUI.color = previousColor;
        }

        public void Show(int amount, Vector3 worldPosition)
        {
            displayValue = amount.ToString();
            transform.position = worldPosition;
            remainingLifetime = lifetime;
            gameObject.SetActive(true);
        }
    }
}
