using TMPro;
using UnityEngine;

namespace RuneGate
{
    public sealed class DamageText : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.6f;
        [SerializeField] private Color textColor = new Color(1f, 0.92f, 0.35f, 1f);

        private float remainingLifetime;
        private string displayValue;
        private TextMeshPro fallbackText;
        private Vector3 startPosition;

        public string DisplayValue => displayValue;

        private void Update()
        {
            if (remainingLifetime <= 0f || fallbackText == null)
            {
                return;
            }

            remainingLifetime -= Time.deltaTime;
            float progress = 1f - Mathf.Clamp01(remainingLifetime / Mathf.Max(0.01f, lifetime));
            transform.position = startPosition + Vector3.up * (progress * 0.55f);
            Color color = textColor;
            color.a = 1f - progress;
            fallbackText.color = color;
            if (remainingLifetime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        public void Show(int amount, Vector3 worldPosition)
        {
            displayValue = amount.ToString();
            BattleCanvasController canvasController = FindAnyObjectByType<BattleCanvasController>();
            if (canvasController != null && canvasController.ShowDamageNumber(displayValue, worldPosition, textColor))
            {
                Destroy(gameObject);
                return;
            }

            startPosition = worldPosition;
            transform.position = worldPosition;
            remainingLifetime = lifetime;
            fallbackText = GetComponent<TextMeshPro>();
            if (fallbackText == null)
            {
                fallbackText = gameObject.AddComponent<TextMeshPro>();
            }

            fallbackText.text = displayValue;
            fallbackText.alignment = TextAlignmentOptions.Center;
            fallbackText.fontSize = 3.2f;
            fallbackText.fontStyle = FontStyles.Bold;
            fallbackText.color = textColor;
            fallbackText.textWrappingMode = TextWrappingModes.NoWrap;
            MeshRenderer renderer = fallbackText.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 40;
            }

            gameObject.SetActive(true);
        }
    }
}
