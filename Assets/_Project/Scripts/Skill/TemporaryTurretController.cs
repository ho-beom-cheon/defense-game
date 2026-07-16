using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class TemporaryTurretController : MonoBehaviour
    {
        private static readonly List<TemporaryTurretController> activeTurrets = new List<TemporaryTurretController>();

        private HeroController owner;
        private int laneIndex;
        private BattlefieldSpaceController battlefieldSpace;
        private BattlefieldAgentRegistry battlefieldAgentRegistry;
        private BattlefieldAgent battlefieldAgent;
        private int attackDamage;
        private float attackRange;
        private float attackInterval;
        private float lifetimeRemaining;
        private float attackCooldown;
        private int shotsFired;

        public HeroController Owner => owner;
        public int LaneIndex => laneIndex;
        public int ShotsFired => shotsFired;
        public float LifetimeRemaining => lifetimeRemaining;
        public static IReadOnlyList<TemporaryTurretController> ActiveTurrets => activeTurrets;

        private void OnEnable()
        {
            if (!activeTurrets.Contains(this))
            {
                activeTurrets.Add(this);
            }
        }

        private void OnDisable()
        {
            activeTurrets.Remove(this);
            battlefieldAgentRegistry?.Unregister(battlefieldAgent);
        }

        public void Initialize(HeroController caster, int assignedLaneIndex, int damage, float range, float lifetime, float interval)
        {
            owner = caster;
            laneIndex = Mathf.Max(0, assignedLaneIndex);
            attackDamage = Mathf.Max(1, damage);
            attackRange = Mathf.Max(1f, range);
            lifetimeRemaining = Mathf.Max(0.5f, lifetime);
            attackInterval = Mathf.Max(0.15f, interval);
            attackCooldown = 0f;
            shotsFired = 0;
            battlefieldSpace = FindAnyObjectByType<BattlefieldSpaceController>();
            battlefieldAgentRegistry = FindAnyObjectByType<BattlefieldAgentRegistry>();

            Vector3 origin = caster != null ? caster.transform.position : transform.position;
            Vector2 position = (Vector2)origin + new Vector2(0.52f, 0f);
            if (battlefieldSpace != null && battlefieldSpace.IsReady)
            {
                position = battlefieldSpace.Clamp(position, new Vector2(0.26f, 0.2f));
            }

            transform.position = new Vector3(position.x, position.y, origin.z - 0.01f);
            CreateVisual();
            ConfigureSpatialPresentation();
        }

        private void Update()
        {
            lifetimeRemaining -= Time.deltaTime;
            if (lifetimeRemaining <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            attackCooldown -= Time.deltaTime;
            if (attackCooldown > 0f)
            {
                return;
            }

            MonsterController target = FindTarget();
            if (target == null)
            {
                return;
            }

            int damage = owner != null ? owner.CalculateDamageAgainst(attackDamage, target) : attackDamage;
            target.TakeDamage(damage);
            CombatVisualEffectFactory.SpawnTurretShot(transform.position + Vector3.up * 0.12f, target.transform.position);
            shotsFired++;
            attackCooldown = attackInterval;
        }

        private MonsterController FindTarget()
        {
            IReadOnlyList<MonsterController> monsters = MonsterController.ActiveMonsters;
            MonsterController selected = null;
            for (int i = 0; i < monsters.Count; i++)
            {
                MonsterController candidate = monsters[i];
                if (candidate == null || !candidate.IsAlive)
                {
                    continue;
                }

                if (!CombatGeometry.IsCenterInRange(transform.position, candidate.transform.position, attackRange))
                {
                    continue;
                }

                float candidateProgress = candidate.Agent != null ? candidate.Agent.ObjectiveProgress : 0f;
                float selectedProgress = selected != null && selected.Agent != null ? selected.Agent.ObjectiveProgress : -1f;
                if (selected == null || candidateProgress > selectedProgress)
                {
                    selected = candidate;
                }
            }

            return selected;
        }

        private void CreateVisual()
        {
            SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = 9;
            Sprite sprite = RuntimePixelAssetLoader.LoadSprite(RuntimePixelAssetLoader.EffectTurretShot);
            if (sprite != null)
            {
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = new Color(0.95f, 0.78f, 0.35f, 1f);
                RuntimeSpriteFitter fitter = gameObject.AddComponent<RuntimeSpriteFitter>();
                fitter.TargetHeight = 0.52f;
                fitter.FitNow();
                return;
            }

            PlaceholderSprite placeholder = gameObject.AddComponent<PlaceholderSprite>();
            placeholder.Configure(new Color(0.68f, 0.48f, 0.24f, 1f), new Vector2(0.52f, 0.38f), 9);
        }

        private void ConfigureSpatialPresentation()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            Vector2 halfExtents = renderer != null
                ? new Vector2(renderer.bounds.extents.x, renderer.bounds.extents.y)
                : new Vector2(0.26f, 0.2f);
            if (battlefieldAgentRegistry != null && battlefieldSpace != null && battlefieldSpace.IsReady)
            {
                battlefieldAgent = gameObject.AddComponent<BattlefieldAgent>();
                int stableId = gameObject.GetHashCode();
                battlefieldAgent.Configure(
                    BattlefieldAgentKind.Deployable,
                    BattlefieldFaction.Hero,
                    stableId == 0 ? 1 : stableId,
                    Mathf.Max(0.18f, halfExtents.x * 0.7f),
                    halfExtents,
                    transform.position,
                    battlefieldSpace.CurrentBounds.PlayableRect);
                battlefieldAgent.AttachRegistry(battlefieldAgentRegistry);
            }

            BattlefieldDepthSorter depthSorter = gameObject.AddComponent<BattlefieldDepthSorter>();
            depthSorter.Configure(renderer, null, battlefieldSpace, gameObject.GetHashCode());
        }
    }
}
