using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class TemporaryTurretController : MonoBehaviour
    {
        private static readonly List<TemporaryTurretController> activeTurrets = new List<TemporaryTurretController>();

        private HeroController owner;
        private LaneManager laneManager;
        private int laneIndex;
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
            laneManager = FindAnyObjectByType<LaneManager>();

            Vector3 origin = caster != null ? caster.transform.position : transform.position;
            float laneY = laneManager != null ? laneManager.GetLaneY(laneIndex) : origin.y;
            transform.position = new Vector3(origin.x + 0.52f, laneY, origin.z - 0.01f);
            CreateVisual();
            laneManager?.ClampUnitInsideBattlefield(transform, GetComponent<SpriteRenderer>());
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
                if (candidate == null || !candidate.IsAlive || candidate.LaneIndex != laneIndex)
                {
                    continue;
                }

                if (Mathf.Abs(candidate.transform.position.x - transform.position.x) > attackRange)
                {
                    continue;
                }

                if (selected == null || candidate.transform.position.x < selected.transform.position.x)
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
    }
}
