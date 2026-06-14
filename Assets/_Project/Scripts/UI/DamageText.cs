using UnityEngine;

namespace RuneGate
{
    public sealed class DamageText : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.6f;

        private float remainingLifetime;
        private string displayValue;

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
                gameObject.SetActive(false);
            }
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
