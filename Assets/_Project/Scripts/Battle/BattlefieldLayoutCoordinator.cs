using UnityEngine;

namespace RuneGate
{
    [DisallowMultipleComponent]
    public sealed class BattlefieldLayoutCoordinator : MonoBehaviour
    {
        [SerializeField] private BattlefieldCameraFitter cameraFitter;
        [SerializeField] private BattlefieldSpaceController battlefieldSpace;
        [SerializeField] private BattlefieldAgentRegistry agentRegistry;
        [SerializeField] private CrystalApproachPointProvider approachProvider;
        [SerializeField] private BattlefieldVisualController battlefieldVisuals;

        private bool subscribed;

        public bool IsReady => cameraFitter != null &&
            battlefieldSpace != null && battlefieldSpace.IsReady &&
            agentRegistry != null && agentRegistry.IsReady &&
            approachProvider != null && approachProvider.IsReady &&
            battlefieldVisuals != null && battlefieldVisuals.IsReady;

        public void Configure(
            BattlefieldCameraFitter fitter,
            BattlefieldSpaceController space,
            BattlefieldAgentRegistry registry,
            CrystalApproachPointProvider approaches,
            BattlefieldVisualController visuals)
        {
            Unbind();
            cameraFitter = fitter;
            battlefieldSpace = space;
            agentRegistry = registry;
            approachProvider = approaches;
            battlefieldVisuals = visuals;
            Bind();
            RefreshLayout();
        }

        public void RefreshLayout()
        {
            cameraFitter?.RefreshLayout();
            battlefieldSpace?.RefreshLayout();
            approachProvider?.RefreshLayout();
            battlefieldVisuals?.RefreshLayout();
        }

        private void OnEnable()
        {
            Bind();
        }

        private void OnDisable()
        {
            Unbind();
        }

        private void HandleLayoutChanged(BattlefieldBounds previous, BattlefieldBounds next)
        {
            if (previous.IsValid)
            {
                agentRegistry?.RemapAgents(previous, next);
            }

            approachProvider?.RefreshLayout();
            battlefieldVisuals?.RefreshLayout();
        }

        private void Bind()
        {
            if (subscribed || battlefieldSpace == null)
            {
                return;
            }

            battlefieldSpace.LayoutChanged += HandleLayoutChanged;
            subscribed = true;
        }

        private void Unbind()
        {
            if (!subscribed || battlefieldSpace == null)
            {
                subscribed = false;
                return;
            }

            battlefieldSpace.LayoutChanged -= HandleLayoutChanged;
            subscribed = false;
        }
    }
}
