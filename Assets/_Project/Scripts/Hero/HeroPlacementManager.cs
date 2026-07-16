using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class HeroPlacementManager : MonoBehaviour
    {
        [SerializeField] private HeroRosterData heroRoster;
        [SerializeField] private FormationData defaultFormation;
        [SerializeField] private Transform heroRoot;
        [SerializeField] private BattlefieldVisualController battlefieldVisualController;
        [SerializeField] private bool useSavedFormation = true;
        [SerializeField] private bool writeDefaultFormationToSave = true;
        [SerializeField] private Vector2 heroPlaceholderSize = new Vector2(0.72f, 0.72f);

        private readonly List<HeroController> spawnedHeroes = new List<HeroController>();
        private LaneManager activeLaneManager;
        private BattlefieldSpaceController activeBattlefieldSpace;
        private BattlefieldAgentRegistry activeAgentRegistry;

        public HeroRosterData HeroRoster => heroRoster;
        public FormationData DefaultFormation => defaultFormation;
        public IReadOnlyList<HeroController> SpawnedHeroes => spawnedHeroes;

        public IReadOnlyList<HeroController> BuildRuntimeFormation(LaneManager laneManager)
        {
            activeBattlefieldSpace = null;
            activeAgentRegistry = null;
            ClearSpawnedHeroes();
            EnsureFallbackContent();

            if (laneManager == null)
            {
                Debug.LogWarning("HeroPlacementManager cannot build formation because LaneManager is missing.");
                return spawnedHeroes;
            }

            EnsureHeroRoot();
            activeLaneManager = laneManager;
            IReadOnlyList<FormationSlot> formationSlots = ResolveFormationSlots();
            HashSet<string> occupiedSlots = new HashSet<string>();
            HashSet<string> placedHeroes = new HashSet<string>();

            for (int i = 0; i < formationSlots.Count; i++)
            {
                FormationSlot slot = formationSlots[i];
                if (slot == null || string.IsNullOrWhiteSpace(slot.HeroId))
                {
                    continue;
                }

                if (!laneManager.IsValidLaneIndex(slot.LaneIndex))
                {
                    Debug.LogWarning($"HeroPlacementManager skipped {slot.HeroId} because lane {slot.LaneIndex} is invalid.");
                    continue;
                }

                string slotKey = $"{slot.LaneIndex}:{slot.PositionType}";
                if (occupiedSlots.Contains(slotKey) || placedHeroes.Contains(slot.HeroId))
                {
                    Debug.LogWarning($"HeroPlacementManager skipped duplicate formation entry {slot.HeroId} at {slotKey}.");
                    continue;
                }

                HeroData heroData = heroRoster != null ? heroRoster.FindHeroById(slot.HeroId) : null;
                if (heroData == null)
                {
                    Debug.LogWarning($"HeroPlacementManager could not find HeroData for id '{slot.HeroId}'.");
                    continue;
                }

                Vector3 position = laneManager.GetHeroSlotPosition(slot.LaneIndex, slot.PositionType);
                HeroController heroController = CreateRuntimeHero(heroData, position, slot.LaneIndex, slot.SlotIndex);
                spawnedHeroes.Add(heroController);
                occupiedSlots.Add(slotKey);
                placedHeroes.Add(slot.HeroId);
            }

            return spawnedHeroes;
        }

        public IReadOnlyList<HeroController> BuildRuntimeFormation(
            BattlefieldSpaceController battlefieldSpace,
            BattlefieldAgentRegistry agentRegistry)
        {
            ClearSpawnedHeroes();
            EnsureFallbackContent();

            if (battlefieldSpace == null || !battlefieldSpace.IsReady || agentRegistry == null || !agentRegistry.IsReady)
            {
                Debug.LogWarning("HeroPlacementManager cannot build a spatial formation because battlefield services are not ready.");
                return spawnedHeroes;
            }

            EnsureHeroRoot();
            activeBattlefieldSpace = battlefieldSpace;
            activeAgentRegistry = agentRegistry;
            activeLaneManager = battlefieldSpace.GetComponentInParent<LaneManager>();
            IReadOnlyList<FormationSlot> formationSlots = ResolveFormationSlots();
            HashSet<string> occupiedSlots = new HashSet<string>();
            HashSet<string> placedHeroes = new HashSet<string>();

            for (int i = 0; i < formationSlots.Count; i++)
            {
                FormationSlot slot = formationSlots[i];
                if (slot == null || string.IsNullOrWhiteSpace(slot.HeroId))
                {
                    continue;
                }

                string slotKey = $"{slot.LaneIndex}:{slot.PositionType}";
                if (occupiedSlots.Contains(slotKey) || placedHeroes.Contains(slot.HeroId))
                {
                    Debug.LogWarning($"HeroPlacementManager skipped duplicate formation entry {slot.HeroId} at {slotKey}.");
                    continue;
                }

                HeroData heroData = heroRoster != null ? heroRoster.FindHeroById(slot.HeroId) : null;
                if (heroData == null)
                {
                    Debug.LogWarning($"HeroPlacementManager could not find HeroData for id '{slot.HeroId}'.");
                    continue;
                }

                Vector2 position = battlefieldSpace.ResolveFormationAnchor(slot);
                HeroController heroController = CreateRuntimeHero(heroData, position, slot.LaneIndex, slot.SlotIndex);
                AttachBattlefieldAgent(heroController, heroData, position);
                spawnedHeroes.Add(heroController);
                occupiedSlots.Add(slotKey);
                placedHeroes.Add(slot.HeroId);
            }

            return spawnedHeroes;
        }

        public void ClearSpawnedHeroes()
        {
            for (int i = spawnedHeroes.Count - 1; i >= 0; i--)
            {
                HeroController hero = spawnedHeroes[i];
                if (hero != null)
                {
                    BattlefieldAgent agent = hero.GetComponent<BattlefieldAgent>();
                    activeAgentRegistry?.Unregister(agent);
                    Destroy(hero.gameObject);
                }
            }

            spawnedHeroes.Clear();
        }

        private IReadOnlyList<FormationSlot> ResolveFormationSlots()
        {
            List<FormationSlot> savedSlots = useSavedFormation ? SaveManager.GetFormationSlots() : new List<FormationSlot>();
            if (savedSlots.Count > 0)
            {
                return savedSlots;
            }

            List<FormationSlot> defaults = GetDefaultFormationSlots();
            if (useSavedFormation && writeDefaultFormationToSave && defaults.Count > 0)
            {
                SaveManager.SetFormationSlots(defaults);
            }

            return defaults;
        }

        private List<FormationSlot> GetDefaultFormationSlots()
        {
            List<FormationSlot> slots = new List<FormationSlot>();
            if (defaultFormation != null)
            {
                IReadOnlyList<FormationSlot> sourceSlots = defaultFormation.Slots;
                for (int i = 0; i < sourceSlots.Count; i++)
                {
                    FormationSlot slot = sourceSlots[i];
                    if (slot != null)
                    {
                        slots.Add(new FormationSlot(slot.LaneIndex, slot.PositionType, slot.HeroId));
                    }
                }
            }

            if (slots.Count == 0)
            {
                slots.AddRange(SaveManager.CreateDefaultFormationSlots());
            }

            return slots;
        }

        private void EnsureHeroRoot()
        {
            if (heroRoot != null)
            {
                return;
            }

            GameObject root = new GameObject("Runtime Heroes");
            root.transform.SetParent(transform);
            heroRoot = root.transform;
        }

        private HeroController CreateRuntimeHero(HeroData heroData, Vector3 position, int laneIndex, int slotIndex)
        {
            GameObject heroObject = heroData.Prefab != null
                ? Instantiate(heroData.Prefab)
                : new GameObject($"Hero_{heroData.DisplayName}");

            heroObject.name = $"Hero_{heroData.DisplayName}";
            heroObject.transform.SetParent(heroRoot);
            heroObject.transform.position = position;

            SpriteRenderer spriteRenderer = heroObject.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                GameObject visualObject = new GameObject("Visual");
                visualObject.transform.SetParent(heroObject.transform);
                visualObject.transform.localPosition = Vector3.zero;
                spriteRenderer = visualObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sortingOrder = 4;
            if (heroData.BattleSprite != null)
            {
                spriteRenderer.sprite = heroData.BattleSprite;
            }
            else
            {
                PlaceholderSprite placeholder = spriteRenderer.gameObject.GetComponent<PlaceholderSprite>();
                if (placeholder == null)
                {
                    placeholder = spriteRenderer.gameObject.AddComponent<PlaceholderSprite>();
                }

                Vector2 placeholderSize = Vector2.one * RuntimeSpritePolicy.GetHeroTargetHeight(heroData);
                if (heroPlaceholderSize.sqrMagnitude > 0.001f)
                {
                    placeholderSize = new Vector2(
                        Mathf.Max(heroPlaceholderSize.x, placeholderSize.x),
                        Mathf.Max(heroPlaceholderSize.y, placeholderSize.y));
                }

                placeholder.Configure(RuntimeSpritePolicy.GetHeroColor(heroData), placeholderSize, 4);
            }

            RuntimeSpriteFitter fitter = spriteRenderer.gameObject.GetComponent<RuntimeSpriteFitter>();
            if (fitter == null)
            {
                fitter = spriteRenderer.gameObject.AddComponent<RuntimeSpriteFitter>();
            }

            float presentationScale = GameFrameLayout.IsPortrait ? 0.86f : 0.96f;
            fitter.TargetHeight = RuntimeSpritePolicy.GetHeroTargetHeight(heroData) * presentationScale;
            fitter.FitNow();
            RuntimeSpriteBoundsUtility.AlignVisualBottomToGround(spriteRenderer, heroObject.transform, position.y);
            Vector3 visualPosition = spriteRenderer.transform.localPosition;
            visualPosition.x += ResolveVisualSlotOffset(slotIndex);
            spriteRenderer.transform.localPosition = visualPosition;
            activeLaneManager?.ClampUnitInsideBattlefield(heroObject.transform, spriteRenderer);

            Animator animator = heroObject.GetComponentInChildren<Animator>();
            if (heroData.AnimatorController != null && animator == null)
            {
                animator = spriteRenderer.gameObject.AddComponent<Animator>();
                animator.runtimeAnimatorController = heroData.AnimatorController;
            }
            else if (animator != null && heroData.AnimatorController != null)
            {
                animator.runtimeAnimatorController = heroData.AnimatorController;
            }

            CharacterVisualController visualController = heroObject.GetComponentInChildren<CharacterVisualController>();
            if (visualController == null)
            {
                visualController = heroObject.AddComponent<CharacterVisualController>();
            }

            visualController.Initialize(heroData.BattleSprite, heroData.AnimatorController);

            if (heroObject.GetComponentInChildren<HitFlashController>() == null)
            {
                heroObject.AddComponent<HitFlashController>();
            }

            SkillController skillController = heroObject.GetComponent<SkillController>();
            if (skillController == null)
            {
                skillController = heroObject.AddComponent<SkillController>();
            }

            HeroController heroController = heroObject.GetComponent<HeroController>();
            if (heroController == null)
            {
                heroController = heroObject.AddComponent<HeroController>();
            }

            heroController.SetLogicalPlacement(laneIndex, slotIndex);
            heroController.Initialize(heroData);
            heroController.RefreshVisualAnchors(true);
            activeLaneManager?.ClampUnitInsideBattlefield(heroObject.transform, spriteRenderer);
            battlefieldVisualController?.CreateUnitShadow(heroObject.transform, spriteRenderer, UnitVisualKind.Hero);

            return heroController;
        }

        private void AttachBattlefieldAgent(HeroController heroController, HeroData heroData, Vector2 anchor)
        {
            if (heroController == null || heroData == null || activeBattlefieldSpace == null || activeAgentRegistry == null)
            {
                return;
            }

            SpriteRenderer spriteRenderer = heroController.GetComponentInChildren<SpriteRenderer>();
            Vector2 halfExtents = spriteRenderer != null
                ? new Vector2(spriteRenderer.bounds.extents.x, spriteRenderer.bounds.extents.y)
                : heroPlaceholderSize * 0.5f;
            Vector3 leash = activeBattlefieldSpace.Config.GetRoleLeash(heroData.Role);
            Rect playable = activeBattlefieldSpace.CurrentBounds.PlayableRect;
            Rect leashRect = Rect.MinMaxRect(
                Mathf.Max(playable.xMin, anchor.x - playable.width * leash.y),
                Mathf.Max(playable.yMin, anchor.y - playable.height * leash.z),
                Mathf.Min(playable.xMax, anchor.x + playable.width * leash.x),
                Mathf.Min(playable.yMax, anchor.y + playable.height * leash.z));

            BattlefieldAgent agent = heroController.GetComponent<BattlefieldAgent>();
            if (agent == null)
            {
                agent = heroController.gameObject.AddComponent<BattlefieldAgent>();
            }

            agent.Configure(
                BattlefieldAgentKind.Hero,
                BattlefieldFaction.Hero,
                StableHash(heroData.HeroId),
                Mathf.Max(0.2f, halfExtents.x * 0.7f),
                halfExtents,
                anchor,
                leashRect);
            agent.AttachRegistry(activeAgentRegistry);
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                uint hash = 2166136261;
                string source = value ?? string.Empty;
                for (int i = 0; i < source.Length; i++)
                {
                    hash ^= source[i];
                    hash *= 16777619;
                }

                int result = (int)(hash & 0x7fffffff);
                return result == 0 ? 1 : result;
            }
        }

        private static float ResolveVisualSlotOffset(int slotIndex)
        {
            switch (slotIndex)
            {
                case 0:
                    return 0.38f;
                case 2:
                    return -0.38f;
                default:
                    return 0f;
            }
        }

        private void EnsureFallbackContent()
        {
            if (heroRoster == null)
            {
                heroRoster = PrototypeAssetLoader.LoadHeroRoster();
            }

            if (defaultFormation == null)
            {
                defaultFormation = PrototypeAssetLoader.LoadDefaultFormation();
            }
        }

    }
}
