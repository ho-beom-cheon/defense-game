using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class HeroPlacementManager : MonoBehaviour
    {
        [SerializeField] private HeroRosterData heroRoster;
        [SerializeField] private FormationData defaultFormation;
        [SerializeField] private Transform heroRoot;
        [SerializeField] private bool useSavedFormation = true;
        [SerializeField] private bool writeDefaultFormationToSave = true;
        [SerializeField] private Vector2 heroPlaceholderSize = new Vector2(0.72f, 0.72f);

        private readonly List<HeroController> spawnedHeroes = new List<HeroController>();

        public HeroRosterData HeroRoster => heroRoster;
        public FormationData DefaultFormation => defaultFormation;
        public IReadOnlyList<HeroController> SpawnedHeroes => spawnedHeroes;

        public IReadOnlyList<HeroController> BuildRuntimeFormation(LaneManager laneManager)
        {
            ClearSpawnedHeroes();
            EnsureFallbackContent();

            if (laneManager == null)
            {
                Debug.LogWarning("HeroPlacementManager cannot build formation because LaneManager is missing.");
                return spawnedHeroes;
            }

            EnsureHeroRoot();
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

        public void ClearSpawnedHeroes()
        {
            for (int i = spawnedHeroes.Count - 1; i >= 0; i--)
            {
                HeroController hero = spawnedHeroes[i];
                if (hero != null)
                {
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
            else if (spriteRenderer.sprite == null && heroObject.GetComponentInChildren<PlaceholderSprite>() == null)
            {
                PlaceholderSprite placeholder = spriteRenderer.gameObject.AddComponent<PlaceholderSprite>();
                placeholder.Configure(GetHeroColor(heroData), heroPlaceholderSize, 4);
            }

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

            return heroController;
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

        private Color GetHeroColor(HeroData heroData)
        {
            if (heroData == null)
            {
                return Color.white;
            }

            switch (heroData.Role)
            {
                case HeroRole.Tank:
                    return new Color(0.45f, 0.62f, 1f);
                case HeroRole.RangedDps:
                    return new Color(0.95f, 0.78f, 0.28f);
                case HeroRole.Mage:
                    return new Color(1f, 0.35f, 0.18f);
                case HeroRole.Healer:
                    return new Color(0.8f, 0.95f, 0.55f);
                case HeroRole.Engineer:
                    return new Color(0.7f, 0.55f, 0.38f);
                case HeroRole.Assassin:
                    return new Color(0.6f, 0.45f, 0.95f);
                default:
                    return Color.white;
            }
        }
    }
}
