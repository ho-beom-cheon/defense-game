using UnityEngine;

namespace RuneGate
{
    [DisallowMultipleComponent]
    public sealed class BattlefieldVisualController : MonoBehaviour
    {
        private const string ShadowObjectName = "Unit Ground Shadow";

        [SerializeField] private BattlefieldArtTheme theme;
        [SerializeField] private BattlefieldSpaceController battlefieldSpace;
        [SerializeField] private CrystalController crystalController;
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private TutorialManager tutorialManager;
        [SerializeField] private CrystalApproachPointProvider approachProvider;

        private SpriteRenderer backgroundRenderer;
        private GroundFieldRenderer groundFieldRenderer;
        private SpriteRenderer[] slotRuneRenderers;
        private SpriteRenderer[] approachBarrierRenderers;
        private SpriteRenderer crystalRenderer;
        private SpriteRenderer riftRenderer;
        private SpriteRenderer shieldRenderer;
        private SpriteRenderer riftPulseRenderer;
        private Transform crystalVisualRoot;
        private Transform riftVisualRoot;
        private Vector3 riftBaseScale = Vector3.one;
        private Vector3 riftPulseBaseScale = Vector3.one;
        private BattlefieldRiftState riftState = BattlefieldRiftState.Idle;
        private BattlefieldCrystalState crystalState = BattlefieldCrystalState.Normal;
        private BattlefieldCrystalState crystalStateBeforeDamage = BattlefieldCrystalState.Normal;
        private float riftStateStartedAt;
        private float crystalStateStartedAt;
        private bool eventsBound;
        private bool hierarchyReady;

        public bool IsReady => hierarchyReady
            && battlefieldSpace != null
            && battlefieldSpace.IsReady
            && approachProvider != null
            && approachProvider.IsReady
            && backgroundRenderer != null
            && groundFieldRenderer != null
            && groundFieldRenderer.IsReady
            && crystalRenderer != null
            && riftRenderer != null;
        public BattlefieldArtTheme Theme => theme;

        private void Awake()
        {
            InitializeFromSerializedReferences();
        }

        private void OnEnable()
        {
            if (!IsReady)
            {
                InitializeFromSerializedReferences();
            }
            else
            {
                BindEvents();
            }
        }

        private void OnDisable()
        {
            UnbindEvents();
        }

        private void Update()
        {
            if (!IsReady)
            {
                return;
            }

            UpdateRiftMotion();
            UpdateCrystalMotion();
        }

        public void Configure(
            BattlefieldArtTheme artTheme,
            BattlefieldSpaceController space,
            CrystalController crystal)
        {
            UnbindEvents();
            theme = artTheme;
            battlefieldSpace = space;
            crystalController = crystal;
            InitializeFromSerializedReferences();
        }

        public void AssignRuntimeServices(
            WaveManager waves,
            BattleManager battle,
            TutorialManager tutorial,
            CrystalApproachPointProvider approaches)
        {
            UnbindEvents();
            waveManager = waves;
            battleManager = battle;
            tutorialManager = tutorial;
            approachProvider = approaches;
            InitializeFromSerializedReferences();
        }

        public void RefreshLayout()
        {
            if (!IsReady || Camera.main == null)
            {
                return;
            }

            Bounds cameraBounds = RuntimeSpriteBoundsUtility.GetCameraWorldBounds(Camera.main);
            ApplyBackgroundCover(cameraBounds);
            groundFieldRenderer.RefreshLayout(cameraBounds);
            ApplyObjectiveLayout(cameraBounds);
            ApplySlotRuneLayout();
            ApplyApproachBarrierLayout();
        }

        public void SetRiftState(BattlefieldRiftState state)
        {
            riftState = state;
            riftStateStartedAt = Time.unscaledTime;
            ApplyRiftPalette();
        }

        public void SetCrystalState(BattlefieldCrystalState state)
        {
            if (state == BattlefieldCrystalState.Damaged && crystalState != BattlefieldCrystalState.Damaged)
            {
                crystalStateBeforeDamage = crystalState;
            }

            crystalState = state;
            crystalStateStartedAt = Time.unscaledTime;
            if (shieldRenderer != null)
            {
                shieldRenderer.gameObject.SetActive(state == BattlefieldCrystalState.Shielded);
            }

            ApplyCrystalPalette();
        }

        public void SetHeroSlotRunesVisible(bool visible)
        {
            if (slotRuneRenderers == null)
            {
                return;
            }

            for (int i = 0; i < slotRuneRenderers.Length; i++)
            {
                if (slotRuneRenderers[i] != null)
                {
                    slotRuneRenderers[i].gameObject.SetActive(visible);
                }
            }
        }

        public SpriteRenderer CreateUnitShadow(Transform unitRoot, SpriteRenderer sourceRenderer, UnitVisualKind kind)
        {
            if (!IsReady || unitRoot == null || sourceRenderer == null || theme.UnitShadow == null)
            {
                return null;
            }

            Transform existing = unitRoot.Find(ShadowObjectName);
            if (existing != null)
            {
                return existing.GetComponent<SpriteRenderer>();
            }

            GameObject shadowObject = new GameObject(ShadowObjectName);
            shadowObject.transform.SetParent(unitRoot, false);
            SpriteRenderer renderer = shadowObject.AddComponent<SpriteRenderer>();
            renderer.sprite = theme.UnitShadow;
            renderer.sortingOrder = Mathf.Min(theme.UnitShadowSortingOrder, sourceRenderer.sortingOrder - 1);

            float widthRatio;
            float alpha;
            switch (kind)
            {
                case UnitVisualKind.Hero:
                    widthRatio = 0.55f;
                    alpha = 0.42f;
                    break;
                case UnitVisualKind.FlyingMonster:
                    widthRatio = 0.48f;
                    alpha = 0.24f;
                    break;
                case UnitVisualKind.Boss:
                    widthRatio = 0.58f;
                    alpha = 0.48f;
                    break;
                default:
                    widthRatio = 0.5f;
                    alpha = 0.36f;
                    break;
            }

            Color shadowColor = theme.ShadowTint;
            shadowColor.a = alpha;
            renderer.color = shadowColor;

            float spriteWidth = Mathf.Max(0.01f, theme.UnitShadow.bounds.size.x);
            float targetWidth = Mathf.Max(0.1f, sourceRenderer.bounds.size.x * widthRatio);
            float uniformScale = targetWidth / spriteWidth;
            shadowObject.transform.localScale = Vector3.one * uniformScale;
            shadowObject.transform.position = new Vector3(
                sourceRenderer.bounds.center.x,
                unitRoot.position.y + 0.02f,
                unitRoot.position.z + 0.02f);
            return renderer;
        }

        private void InitializeFromSerializedReferences()
        {
            if (theme == null || battlefieldSpace == null || crystalController == null)
            {
                Debug.LogError("BattlefieldVisualController requires a Stage 1 theme, BattlefieldSpaceController, and CrystalController.", this);
                return;
            }

            if (!theme.HasRequiredAssets)
            {
                Debug.LogError("BattlefieldVisualController cannot initialize because the battlefield theme is missing required art.", theme);
                return;
            }

            if (waveManager == null)
            {
                Debug.LogError("BattlefieldVisualController requires a direct WaveManager reference.", this);
                return;
            }

            if (backgroundRenderer == null)
            {
                BuildHierarchy();
            }

            hierarchyReady = backgroundRenderer != null
                && groundFieldRenderer != null
                && groundFieldRenderer.IsReady
                && crystalRenderer != null
                && riftRenderer != null;
            if (!hierarchyReady)
            {
                Debug.LogError("BattlefieldVisualController failed to create the battlefield art hierarchy.", this);
                return;
            }

            BindEvents();
            RefreshCrystalState();
            SetRiftState(BattlefieldRiftState.Idle);
            RefreshRuneVisibility();
            RefreshLayout();
        }

        private void BuildHierarchy()
        {
            Transform backdropLayer = CreateLayer("BackdropLayer");
            CreateLayer("FarAtmosphereLayer");
            Transform groundLayer = CreateLayer("GroundLayer");
            Transform decalLayer = CreateLayer("GroundDecalLayer");
            Transform objectiveLayer = CreateLayer("ObjectiveLayer");
            Transform effectLayer = CreateLayer("WorldEffectLayer");

            backgroundRenderer = CreateRenderer("Sealed Forest Backdrop", backdropLayer, theme.Background, theme.BackgroundSortingOrder, theme.BackgroundTint);

            GameObject groundFieldObject = new GameObject("Stage 1 Ground Field");
            groundFieldObject.transform.SetParent(groundLayer, false);
            groundFieldRenderer = groundFieldObject.AddComponent<GroundFieldRenderer>();
            groundFieldRenderer.Configure(
                theme.GroundField,
                theme.GroundFieldTint,
                theme.GroundFieldSortingOrder,
                theme.GroundFieldUniformScale);

            slotRuneRenderers = new SpriteRenderer[9];
            for (int laneIndex = 0; laneIndex < 3; laneIndex++)
            {
                for (int slotIndex = 0; slotIndex < 3; slotIndex++)
                {
                    int flatIndex = laneIndex * 3 + slotIndex;
                    SpriteRenderer renderer = CreateRenderer(
                        $"Formation {laneIndex}-{slotIndex} Rune",
                        decalLayer,
                        theme.HeroSlotRune,
                        theme.DecalSortingOrder,
                        new Color(1f, 1f, 1f, 0.54f));
                    renderer.gameObject.SetActive(false);
                    slotRuneRenderers[flatIndex] = renderer;
                }
            }

            int approachPointCount = battlefieldSpace.Config != null
                ? battlefieldSpace.Config.ApproachPointCount
                : 7;
            approachBarrierRenderers = new SpriteRenderer[Mathf.Max(0, approachPointCount)];
            for (int i = 0; i < approachBarrierRenderers.Length; i++)
            {
                approachBarrierRenderers[i] = CreateRenderer(
                    $"Crystal Approach Seal {i}",
                    decalLayer,
                    theme.HeroSlotRune,
                    theme.DecalSortingOrder + 1,
                    theme.ApproachBarrierTint);
            }

            crystalVisualRoot = new GameObject("SealCrystalVisual").transform;
            crystalVisualRoot.SetParent(objectiveLayer, false);
            crystalRenderer = CreateRenderer("Crystal Body", crystalVisualRoot, theme.Crystal, theme.ObjectiveSortingOrder, theme.CrystalTint);
            HitFlashController hitFlash = crystalRenderer.gameObject.AddComponent<HitFlashController>();
            crystalController.BindVisual(crystalRenderer, hitFlash);

            riftVisualRoot = new GameObject("SpawnRiftVisual").transform;
            riftVisualRoot.SetParent(objectiveLayer, false);
            riftRenderer = CreateRenderer("Rift Body", riftVisualRoot, theme.Rift, theme.ObjectiveSortingOrder + 1, theme.RiftTint);

            shieldRenderer = CreateRenderer("Crystal Shield Ring", effectLayer, theme.CrystalShieldRing, theme.WorldEffectSortingOrder, theme.ShieldTint);
            shieldRenderer.gameObject.SetActive(false);
            riftPulseRenderer = CreateRenderer("Rift Pulse", effectLayer, theme.RiftPulse, theme.WorldEffectSortingOrder + 1, theme.RiftIdleTint);
        }

        private Transform CreateLayer(string layerName)
        {
            GameObject layer = new GameObject(layerName);
            layer.transform.SetParent(transform, false);
            return layer.transform;
        }

        private static SpriteRenderer CreateRenderer(string objectName, Transform parent, Sprite sprite, int sortingOrder, Color color)
        {
            GameObject visual = new GameObject(objectName);
            visual.transform.SetParent(parent, false);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            renderer.color = color;
            return renderer;
        }

        private void ApplyBackgroundCover(Bounds cameraBounds)
        {
            Vector2 spriteSize = backgroundRenderer.sprite.bounds.size;
            float uniformScale = Mathf.Max(
                cameraBounds.size.x / Mathf.Max(0.01f, spriteSize.x),
                cameraBounds.size.y / Mathf.Max(0.01f, spriteSize.y));
            backgroundRenderer.transform.localScale = Vector3.one * uniformScale;
            backgroundRenderer.transform.position = new Vector3(cameraBounds.center.x, cameraBounds.center.y, 0.4f);
        }

        private void ApplyObjectiveLayout(Bounds cameraBounds)
        {
            float screenScale = GameFrameLayout.IsPortrait ? theme.PortraitObjectiveScale : theme.LandscapeObjectiveScale;
            crystalVisualRoot.localScale = Vector3.one;
            riftVisualRoot.localScale = Vector3.one;
            ApplyUniformHeight(crystalRenderer, theme.CrystalWorldHeight * screenScale);
            ApplyUniformHeight(riftRenderer, theme.RiftWorldHeight * screenScale);
            ApplyUniformHeight(shieldRenderer, crystalRenderer.bounds.size.y * 0.72f);
            ApplyUniformHeight(riftPulseRenderer, riftRenderer.bounds.size.y * 0.68f);

            Rect playable = battlefieldSpace.CurrentBounds.PlayableRect;
            float crystalHalfWidth = Mathf.Max(GetWorldHalfWidth(crystalRenderer), GetWorldHalfWidth(shieldRenderer));
            float riftHalfWidth = Mathf.Max(GetWorldHalfWidth(riftRenderer), GetWorldHalfWidth(riftPulseRenderer));
            float crystalX = Mathf.Max(
                crystalController.transform.position.x,
                cameraBounds.min.x + crystalHalfWidth + theme.ObjectiveEdgePadding);
            Vector2 riftAnchor = battlefieldSpace.CurrentBounds.ToWorld(
                battlefieldSpace.Config.RiftSpawnNormalizedRect.center);
            float riftX = Mathf.Min(
                riftAnchor.x,
                cameraBounds.max.x - riftHalfWidth - theme.ObjectiveEdgePadding);

            crystalVisualRoot.position = new Vector3(crystalX, playable.center.y, 0.1f);
            riftVisualRoot.position = new Vector3(riftX, playable.center.y, 0.1f);
            riftBaseScale = riftVisualRoot.localScale;

            shieldRenderer.transform.position = crystalVisualRoot.position + new Vector3(0f, 0.08f, -0.02f);
            riftPulseRenderer.transform.position = riftVisualRoot.position + new Vector3(0f, 0.02f, -0.02f);
            riftPulseBaseScale = riftPulseRenderer.transform.localScale;

            crystalRenderer.sortingOrder = BattlefieldDepthSorter.CalculateWorldOrder(
                crystalVisualRoot.position,
                theme.ObjectiveSortingOrder);
            shieldRenderer.sortingOrder = crystalRenderer.sortingOrder + 2;
            riftRenderer.sortingOrder = BattlefieldDepthSorter.CalculateWorldOrder(
                riftVisualRoot.position,
                theme.ObjectiveSortingOrder + 1);
            riftPulseRenderer.sortingOrder = riftRenderer.sortingOrder + 2;
        }

        private void ApplySlotRuneLayout()
        {
            float targetWidth = 1.2f;
            float scale = targetWidth / Mathf.Max(0.01f, theme.HeroSlotRune.bounds.size.x);
            for (int laneIndex = 0; laneIndex < 3; laneIndex++)
            {
                for (int slotIndex = 0; slotIndex < 3; slotIndex++)
                {
                    int flatIndex = laneIndex * 3 + slotIndex;
                    SpriteRenderer renderer = slotRuneRenderers[flatIndex];
                    renderer.transform.localScale = Vector3.one * scale;
                    HeroPositionType positionType = slotIndex == 0
                        ? HeroPositionType.Front
                        : slotIndex == 2
                            ? HeroPositionType.Back
                            : HeroPositionType.Middle;
                    Vector3 position = battlefieldSpace.ResolveFormationAnchor(laneIndex, positionType);
                    renderer.transform.position = new Vector3(position.x, position.y + 0.02f, 0.18f);
                }
            }
        }

        private void ApplyApproachBarrierLayout()
        {
            if (approachBarrierRenderers == null || approachProvider == null)
            {
                return;
            }

            float scale = theme.ApproachBarrierWorldWidth /
                Mathf.Max(0.01f, theme.HeroSlotRune.bounds.size.x);
            for (int i = 0; i < approachBarrierRenderers.Length; i++)
            {
                SpriteRenderer renderer = approachBarrierRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                bool hasPoint = approachProvider.TryGetPoint(i, out Vector2 position);
                renderer.gameObject.SetActive(hasPoint);
                if (!hasPoint)
                {
                    continue;
                }

                renderer.transform.localScale = Vector3.one * scale;
                renderer.transform.position = new Vector3(position.x, position.y + 0.01f, 0.17f);
            }
        }

        private static void ApplyUniformHeight(SpriteRenderer renderer, float targetHeight)
        {
            float spriteHeight = Mathf.Max(0.01f, renderer.sprite.bounds.size.y);
            renderer.transform.localScale = Vector3.one * (Mathf.Max(0.01f, targetHeight) / spriteHeight);
        }

        private static float GetWorldHalfWidth(SpriteRenderer renderer)
        {
            return renderer.sprite.bounds.extents.x * Mathf.Abs(renderer.transform.lossyScale.x);
        }

        private void BindEvents()
        {
            if (eventsBound || crystalController == null || waveManager == null)
            {
                return;
            }

            crystalController.HpChanged += HandleCrystalHpChanged;
            crystalController.ShieldChanged += HandleCrystalShieldChanged;
            crystalController.Damaged += HandleCrystalDamaged;
            crystalController.Destroyed += HandleCrystalDestroyed;
            waveManager.WaveStarted += HandleWaveStarted;
            waveManager.WaveCompleted += HandleWaveCompleted;
            if (battleManager != null)
            {
                battleManager.BattleStateChanged += HandleBattleStateChanged;
            }

            if (tutorialManager != null)
            {
                tutorialManager.VisibilityChanged += HandleTutorialVisibilityChanged;
            }

            eventsBound = true;
        }

        private void UnbindEvents()
        {
            if (!eventsBound)
            {
                return;
            }

            if (crystalController != null)
            {
                crystalController.HpChanged -= HandleCrystalHpChanged;
                crystalController.ShieldChanged -= HandleCrystalShieldChanged;
                crystalController.Damaged -= HandleCrystalDamaged;
                crystalController.Destroyed -= HandleCrystalDestroyed;
            }

            if (waveManager != null)
            {
                waveManager.WaveStarted -= HandleWaveStarted;
                waveManager.WaveCompleted -= HandleWaveCompleted;
            }

            if (battleManager != null)
            {
                battleManager.BattleStateChanged -= HandleBattleStateChanged;
            }

            if (tutorialManager != null)
            {
                tutorialManager.VisibilityChanged -= HandleTutorialVisibilityChanged;
            }

            eventsBound = false;
        }

        private void HandleCrystalHpChanged(int currentHp, int maxHp)
        {
            RefreshCrystalState();
        }

        private void HandleCrystalShieldChanged(int shieldHp)
        {
            RefreshCrystalState();
        }

        private void HandleCrystalDamaged(int damage, int currentHp, int maxHp)
        {
            SetCrystalState(BattlefieldCrystalState.Damaged);
        }

        private void HandleCrystalDestroyed()
        {
            SetCrystalState(BattlefieldCrystalState.Destroyed);
        }

        private void HandleWaveStarted(WaveData wave)
        {
            bool bossWave = false;
            if (wave != null && wave.Spawns != null)
            {
                for (int i = 0; i < wave.Spawns.Count; i++)
                {
                    MonsterData data = wave.Spawns[i] != null ? wave.Spawns[i].MonsterData : null;
                    if (data != null && data.IsBoss)
                    {
                        bossWave = true;
                        break;
                    }
                }
            }

            SetRiftState(bossWave ? BattlefieldRiftState.BossWarning : BattlefieldRiftState.WaveWarning);
        }

        private void HandleWaveCompleted(WaveData wave)
        {
            SetRiftState(BattlefieldRiftState.Idle);
        }

        private void HandleBattleStateChanged(BattleState state)
        {
            RefreshRuneVisibility();
        }

        private void HandleTutorialVisibilityChanged(bool visible)
        {
            RefreshRuneVisibility();
        }

        private void RefreshRuneVisibility()
        {
            bool tutorialVisible = tutorialManager != null && tutorialManager.IsVisible;
            bool preparing = battleManager != null && battleManager.CurrentState == BattleState.Preparing;
            SetHeroSlotRunesVisible(tutorialVisible || preparing);
        }

        private void RefreshCrystalState()
        {
            if (crystalController == null)
            {
                return;
            }

            if (crystalController.IsDestroyed)
            {
                SetCrystalState(BattlefieldCrystalState.Destroyed);
            }
            else if (crystalController.ShieldHp > 0)
            {
                SetCrystalState(BattlefieldCrystalState.Shielded);
            }
            else if (crystalController.MaxHp > 0 && crystalController.CurrentHp <= Mathf.CeilToInt(crystalController.MaxHp * 0.3f))
            {
                SetCrystalState(BattlefieldCrystalState.Critical);
            }
            else
            {
                SetCrystalState(BattlefieldCrystalState.Normal);
            }
        }

        private void UpdateRiftMotion()
        {
            float elapsed = Time.unscaledTime - riftStateStartedAt;
            if ((riftState == BattlefieldRiftState.WaveWarning || riftState == BattlefieldRiftState.BossWarning)
                && elapsed >= theme.RiftWarningSeconds)
            {
                SetRiftState(BattlefieldRiftState.Idle);
                elapsed = 0f;
            }

            float pulse = riftState == BattlefieldRiftState.Dormant
                ? 0f
                : Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f / theme.RiftPulseSeconds) * theme.RiftPulseScale;
            float warningBoost = 0f;
            if (riftState == BattlefieldRiftState.WaveWarning || riftState == BattlefieldRiftState.BossWarning)
            {
                float intensity = 1f - Mathf.Clamp01(elapsed / theme.RiftWarningSeconds);
                warningBoost = intensity * (riftState == BattlefieldRiftState.BossWarning ? 0.1f : 0.08f);
            }

            float scale = 1f + pulse + warningBoost;
            riftVisualRoot.localScale = riftBaseScale * scale;
            riftPulseRenderer.transform.localScale = riftPulseBaseScale * scale;
        }

        private void ApplyRiftPalette()
        {
            if (riftRenderer == null || riftPulseRenderer == null)
            {
                return;
            }

            Color pulseColor;
            switch (riftState)
            {
                case BattlefieldRiftState.WaveWarning:
                    pulseColor = theme.RiftWarningTint;
                    riftRenderer.color = Color.Lerp(theme.RiftTint, theme.RiftWarningTint, 0.18f);
                    break;
                case BattlefieldRiftState.BossWarning:
                    pulseColor = theme.RiftBossTint;
                    riftRenderer.color = Color.Lerp(theme.RiftTint, theme.RiftBossTint, 0.24f);
                    break;
                case BattlefieldRiftState.Dormant:
                    pulseColor = theme.RiftIdleTint;
                    pulseColor.a = 0.08f;
                    riftRenderer.color = new Color(theme.RiftTint.r, theme.RiftTint.g, theme.RiftTint.b, 0.52f);
                    break;
                default:
                    pulseColor = theme.RiftIdleTint;
                    riftRenderer.color = theme.RiftTint;
                    break;
            }

            riftPulseRenderer.color = pulseColor;
        }

        private void UpdateCrystalMotion()
        {
            float elapsed = Time.unscaledTime - crystalStateStartedAt;
            if (crystalState == BattlefieldCrystalState.Damaged && elapsed >= theme.CrystalDamageSeconds)
            {
                crystalState = crystalStateBeforeDamage;
                crystalStateStartedAt = Time.unscaledTime;
                ApplyCrystalPalette();
                return;
            }

            if (crystalState == BattlefieldCrystalState.Critical)
            {
                float pulse = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f / theme.CrystalCriticalPulseSeconds) + 1f) * 0.5f;
                crystalRenderer.color = Color.Lerp(theme.CrystalTint, theme.CrystalCriticalTint, 0.28f + pulse * 0.38f);
            }
        }

        private void ApplyCrystalPalette()
        {
            if (crystalRenderer == null)
            {
                return;
            }

            switch (crystalState)
            {
                case BattlefieldCrystalState.Damaged:
                    crystalRenderer.color = Color.Lerp(theme.CrystalTint, Color.white, 0.7f);
                    break;
                case BattlefieldCrystalState.Critical:
                    crystalRenderer.color = Color.Lerp(theme.CrystalTint, theme.CrystalCriticalTint, 0.35f);
                    break;
                case BattlefieldCrystalState.Destroyed:
                    crystalRenderer.color = new Color(0.2f, 0.22f, 0.22f, 0.48f);
                    break;
                default:
                    crystalRenderer.color = theme.CrystalTint;
                    break;
            }
        }
    }
}
