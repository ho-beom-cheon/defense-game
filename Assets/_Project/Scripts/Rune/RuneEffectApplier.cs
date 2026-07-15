using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class RuneEffectApplier : MonoBehaviour
    {
        private const string HeroAttackPercent = "hero_attack_percent";
        private const string HeroAttackSpeedPercent = "hero_attack_speed_percent";
        private const string CrystalHealFlat = "crystal_heal_flat";
        private const string CrystalShieldFlat = "crystal_shield_flat";
        private const string MonsterSlowPercent = "monster_slow_percent";
        private const string EnemySlowPercent = "enemy_slow_percent";
        private const string SkillCooldownPercent = "skill_cooldown_percent";
        private const string BossDamagePercent = "boss_damage_percent";
        private const string HeroMaxHpPercent = "hero_max_hp_percent";
        private const string HeroHpPercent = "hero_hp_percent";
        private const string TankHpPercent = "tank_hp_percent";
        private const string TankDefensePercent = "tank_defense_percent";
        private const string HealingPercent = "healing_percent";
        private const string AllHeroStatsPercent = "all_hero_stats_percent";
        private const string SacrificeCrystalForAttack = "sacrifice_crystal_for_attack";
        private const string MageAreaPercent = "mage_area_percent";
        private const string LightningChainPercent = "lightning_chain_percent";
        private const string SplashDamagePercent = "splash_damage_percent";
        private const string PurificationPercent = "purification_percent";
        private const string CrushDamagePercent = "crush_damage_percent";
        private const string RangedChainDamagePercent = "ranged_chain_damage_percent";
        private const string LightningPlaceholder = "lightning_placeholder";
        private const string BlastPlaceholder = "blast_placeholder";
        private const string PurifyPlaceholder = "purify_placeholder";
        private const string CrushPlaceholder = "crush_placeholder";
        private const string RangedChainShotPlaceholder = "ranged_chain_shot_placeholder";
        private const string TurretAttackPercent = "turret_attack_percent";

        private WaveManager waveManager;
        private float accumulatedMonsterSlowPercent;

        public float AccumulatedMonsterSlowPercent => accumulatedMonsterSlowPercent;
        public int AppliedRuneCount { get; private set; }

        private void OnEnable()
        {
            BindWaveManager();
        }

        private void OnDisable()
        {
            UnbindWaveManager();
        }

        public void ResetForBattle(WaveManager activeWaveManager)
        {
            UnbindWaveManager();
            waveManager = activeWaveManager;
            accumulatedMonsterSlowPercent = 0f;
            AppliedRuneCount = 0;
            BindWaveManager();
        }

        public void ApplyRune(RuneData runeData, IReadOnlyList<HeroController> heroes, CrystalController crystalController)
        {
            if (runeData == null)
            {
                Debug.LogWarning("RuneEffectApplier cannot apply a null rune.");
                return;
            }

            AppliedRuneCount++;

            switch (runeData.EffectKey)
            {
                case HeroAttackPercent:
                    ApplyToHeroes(heroes, hero => hero.ApplyAttackPercent(runeData.Value));
                    break;
                case HeroAttackSpeedPercent:
                    ApplyToHeroes(heroes, hero => hero.ApplyAttackSpeedPercent(runeData.Value));
                    break;
                case CrystalHealFlat:
                    if (crystalController != null)
                    {
                        crystalController.Heal(Mathf.RoundToInt(runeData.Value));
                    }
                    break;
                case SkillCooldownPercent:
                    ApplyToHeroes(heroes, hero => hero.ApplySkillCooldownPercent(runeData.Value));
                    break;
                case HeroMaxHpPercent:
                case HeroHpPercent:
                    ApplyToHeroes(heroes, hero => hero.ApplyHeroHpPercent(runeData.Value));
                    break;
                case BossDamagePercent:
                    ApplyToHeroes(heroes, hero => hero.ApplyBossDamagePercent(runeData.Value));
                    break;
                case HealingPercent:
                    ApplyToHeroes(heroes, hero => hero.ApplyHealingPercent(runeData.Value));
                    break;
                case MonsterSlowPercent:
                case EnemySlowPercent:
                    AccumulateMonsterSlow(runeData.Value);
                    break;
                case MageAreaPercent:
                    ApplyToHeroes(heroes, hero => hero.ApplyAttackPercent(runeData.Value * 0.8f));
                    Debug.Log("Mage area rune uses a v1.0 placeholder by applying a smaller global attack bonus.");
                    break;
                case TankDefensePercent:
                    ApplyTankDefense(heroes, runeData.Value);
                    break;
                case TankHpPercent:
                    ApplyToHeroes(heroes, hero => hero.ApplyHeroHpPercent(runeData.Value));
                    break;
                case LightningChainPercent:
                case LightningPlaceholder:
                    ApplyToHeroes(heroes, hero => hero.ApplyLightningDamagePercent(runeData.Value));
                    break;
                case SplashDamagePercent:
                case BlastPlaceholder:
                    ApplyToHeroes(heroes, hero => hero.ApplySplashDamagePercent(runeData.Value));
                    break;
                case PurificationPercent:
                case PurifyPlaceholder:
                    ApplyPurification(heroes, crystalController, runeData.Value);
                    break;
                case CrushDamagePercent:
                case CrushPlaceholder:
                    ApplyToHeroes(heroes, hero => hero.ApplyCrushDamagePercent(runeData.Value));
                    break;
                case RangedChainDamagePercent:
                case RangedChainShotPlaceholder:
                    ApplyToHeroes(heroes, hero => hero.ApplyChainDamagePercent(runeData.Value));
                    break;
                case TurretAttackPercent:
                    ApplyToHeroes(heroes, hero => hero.ApplyAttackPercent(Mathf.Max(0f, runeData.Value) * 0.6f));
                    break;
                case AllHeroStatsPercent:
                    ApplyToHeroes(heroes, hero =>
                    {
                        hero.ApplyAttackPercent(runeData.Value);
                        hero.ApplyAttackSpeedPercent(runeData.Value * 0.5f);
                        hero.ApplyHeroHpPercent(runeData.Value);
                    });
                    break;
                case SacrificeCrystalForAttack:
                    if (crystalController != null)
                    {
                        crystalController.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(runeData.Value)));
                    }

                    ApplyToHeroes(heroes, hero => hero.ApplyAttackPercent(0.25f));
                    break;
                case CrystalShieldFlat:
                    if (crystalController != null)
                    {
                        crystalController.AddShield(Mathf.RoundToInt(runeData.Value));
                    }
                    break;
                default:
                    Debug.Log($"RuneEffectApplier reserved rune effect key '{runeData.EffectKey}' for a later prototype.");
                    break;
            }
        }

        public static bool IsImplementedEffectKey(string effectKey)
        {
            switch (effectKey)
            {
                case HeroAttackPercent:
                case HeroAttackSpeedPercent:
                case CrystalHealFlat:
                case CrystalShieldFlat:
                case MonsterSlowPercent:
                case EnemySlowPercent:
                case SkillCooldownPercent:
                case BossDamagePercent:
                case HeroMaxHpPercent:
                case HeroHpPercent:
                case TankHpPercent:
                case TankDefensePercent:
                case HealingPercent:
                case AllHeroStatsPercent:
                case SacrificeCrystalForAttack:
                case MageAreaPercent:
                case LightningChainPercent:
                case SplashDamagePercent:
                case PurificationPercent:
                case CrushDamagePercent:
                case RangedChainDamagePercent:
                case TurretAttackPercent:
                case LightningPlaceholder:
                case BlastPlaceholder:
                case PurifyPlaceholder:
                case CrushPlaceholder:
                case RangedChainShotPlaceholder:
                    return true;
                default:
                    return false;
            }
        }

        private void AccumulateMonsterSlow(float percent)
        {
            accumulatedMonsterSlowPercent = CombineSlowPercent(accumulatedMonsterSlowPercent, percent);
            EnsureWaveManager();
            ApplyToActiveMonsters(ApplyAccumulatedSlow);
        }

        public static float CombineSlowPercent(float current, float added)
        {
            float currentPercent = Mathf.Clamp(current, 0f, 0.8f);
            float addedPercent = Mathf.Clamp(added, 0f, 0.8f);
            return Mathf.Clamp(1f - (1f - currentPercent) * (1f - addedPercent), 0f, 0.8f);
        }

        private void ApplyPurification(
            IReadOnlyList<HeroController> heroes,
            CrystalController crystalController,
            float value)
        {
            float healPercent = Mathf.Clamp(value, 0f, 1f);
            ApplyToHeroes(heroes, hero =>
            {
                hero.Heal(Mathf.Max(1, Mathf.RoundToInt(hero.MaxHp * healPercent)));
                hero.ApplyHealingPercent(healPercent * 0.5f);
            });

            if (crystalController != null)
            {
                crystalController.Heal(Mathf.Max(1, Mathf.RoundToInt(crystalController.MaxHp * healPercent)));
            }
        }

        private void EnsureWaveManager()
        {
            if (waveManager == null)
            {
                waveManager = FindAnyObjectByType<WaveManager>();
            }

            BindWaveManager();
        }

        private void BindWaveManager()
        {
            if (waveManager == null)
            {
                return;
            }

            waveManager.MonsterSpawned -= HandleMonsterSpawned;
            waveManager.MonsterSpawned += HandleMonsterSpawned;
        }

        private void UnbindWaveManager()
        {
            if (waveManager != null)
            {
                waveManager.MonsterSpawned -= HandleMonsterSpawned;
            }
        }

        private void HandleMonsterSpawned(MonsterController monster)
        {
            ApplyAccumulatedSlow(monster);
        }

        private void ApplyAccumulatedSlow(MonsterController monster)
        {
            if (monster != null && monster.IsAlive && accumulatedMonsterSlowPercent > 0f)
            {
                monster.ApplySlowPercent(accumulatedMonsterSlowPercent);
            }
        }

        private void ApplyToHeroes(IReadOnlyList<HeroController> heroes, System.Action<HeroController> apply)
        {
            if (heroes == null)
            {
                Debug.LogWarning("RuneEffectApplier cannot apply hero rune because hero list is missing.");
                return;
            }

            for (int i = 0; i < heroes.Count; i++)
            {
                HeroController hero = heroes[i];
                if (hero != null)
                {
                    apply(hero);
                }
            }
        }

        private void ApplyToActiveMonsters(System.Action<MonsterController> apply)
        {
            MonsterController[] monsters = FindObjectsByType<MonsterController>(FindObjectsInactive.Exclude);
            for (int i = 0; i < monsters.Length; i++)
            {
                MonsterController monster = monsters[i];
                if (monster != null && monster.IsAlive)
                {
                    apply(monster);
                }
            }
        }

        private void ApplyTankDefense(IReadOnlyList<HeroController> heroes, float value)
        {
            ApplyToHeroes(heroes, hero =>
            {
                if (hero != null && hero.Data != null && hero.Data.Role == HeroRole.Tank)
                {
                    hero.ApplyHeroHpPercent(value);
                    return;
                }

                hero.ApplyHeroHpPercent(value * 0.35f);
            });
        }
    }
}
