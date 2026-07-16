using UnityEngine;

namespace RuneGate
{
    [DisallowMultipleComponent]
    public sealed class BattlefieldDepthSorter : MonoBehaviour
    {
        private const int DefaultBaseOrder = 100;
        private const float DefaultResolution = 20f;

        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer shadowRenderer;
        [SerializeField] private BattlefieldSpaceController battlefieldSpace;
        [SerializeField] private int stableTieBreak;

        private SpriteRenderer[] relatedRenderers;

        public int CurrentBodyOrder { get; private set; }

        public void Configure(
            SpriteRenderer body,
            SpriteRenderer shadow,
            BattlefieldSpaceController space,
            int stableId)
        {
            bodyRenderer = body;
            shadowRenderer = shadow;
            battlefieldSpace = space;
            stableTieBreak = Mathf.Abs(stableId % 3);
            relatedRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            ApplySorting();
        }

        private void LateUpdate()
        {
            ApplySorting();
        }

        private void ApplySorting()
        {
            if (bodyRenderer == null)
            {
                return;
            }

            Bounds worldBounds = battlefieldSpace != null && battlefieldSpace.IsReady
                ? new Bounds(
                    battlefieldSpace.CurrentBounds.PlayableRect.center,
                    new Vector3(
                        battlefieldSpace.CurrentBounds.PlayableRect.width,
                        battlefieldSpace.CurrentBounds.PlayableRect.height,
                        1f))
                : RuntimeSpriteBoundsUtility.GetCameraWorldBounds();
            CurrentBodyOrder = CalculateOrder(transform.position, worldBounds, stableTieBreak);
            bodyRenderer.sortingOrder = CurrentBodyOrder;
            if (shadowRenderer != null)
            {
                shadowRenderer.sortingOrder = CurrentBodyOrder - 1;
            }

            if (relatedRenderers == null || relatedRenderers.Length == 0)
            {
                relatedRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            }

            for (int i = 0; i < relatedRenderers.Length; i++)
            {
                SpriteRenderer renderer = relatedRenderers[i];
                if (renderer == null || renderer == bodyRenderer || renderer == shadowRenderer)
                {
                    continue;
                }

                bool hpBar = renderer.transform.IsChildOf(transform) &&
                    renderer.transform.parent != null &&
                    renderer.transform.parent.name.Contains("HP Bar");
                renderer.sortingOrder = hpBar ? CurrentBodyOrder + 2 + i % 2 : CurrentBodyOrder + 1;
            }
        }

        public static int CalculateOrder(Vector3 position, Bounds worldBounds, int tieBreak = 0)
        {
            return DefaultBaseOrder +
                Mathf.RoundToInt((worldBounds.max.y - position.y) * DefaultResolution) +
                Mathf.Clamp(tieBreak, 0, 2);
        }

        public static int CalculateWorldOrder(Vector3 position, int relativeOffset = 0)
        {
            Bounds bounds = RuntimeSpriteBoundsUtility.GetCameraWorldBounds();
            return CalculateOrder(position, bounds) + relativeOffset;
        }
    }
}
