using UnityEngine;

namespace RuneGate
{
    public sealed class ResponsiveLayoutSwitcher : MonoBehaviour
    {
        [SerializeField] private GameObject portraitRoot;
        [SerializeField] private GameObject landscapeRoot;

        private bool? lastPortrait;

        private void OnEnable()
        {
            Apply();
        }

        private void Update()
        {
            Apply();
        }

        public void Apply()
        {
            bool portrait = GameFrameLayout.IsPortrait;
            if (lastPortrait.HasValue && lastPortrait.Value == portrait)
            {
                return;
            }

            lastPortrait = portrait;
            if (portraitRoot != null)
            {
                portraitRoot.SetActive(portrait);
            }

            if (landscapeRoot != null)
            {
                landscapeRoot.SetActive(!portrait);
            }
        }
    }
}
