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
        private const string HeroHpPercent = "hero_hp_percent";
        private const string TankHpPercent = "tank_hp_percent";
        private const string HealingPercent = "healing_percent";
        private const string AllHeroStatsPercent = "all_hero_stats_percent";
        private const string SacrificeCrystalForAttack = "sacrifice_crystal_for_attack";
        private const string MageAreaPercent = "mage_area_percent";
        private const string RangedChainShotPlaceholder = "ranged_chain_shot_placeholder";
        private const string TurretAttackPercent = "turret_attack_percent";

        public void ApplyRune(RuneData runeData, IReadOnlyList<HeroController> heroes, CrystalController crystalController)
        {
            if (runeData == null)
            {
                Debug.LogWarning("RuneEffectApplier cannot apply a null rune.");
                return;
            }

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
                    ApplyToActiveMonsters(monster => monster.ApplySlowPercent(runeData.Value));
                    break;
                case MageAreaPercent:
                    ApplyToHeroes(heroes, hero => hero.ApplyAttackPercent(runeData.Value * 0.8f));
                    Debug.Log("Mage area rune uses a v1.0 placeholder by applying a smaller global attack bonus.");
                    break;
                case TankHpPercent:
                    ApplyToHeroes(heroes, hero => hero.ApplyHeroHpPercent(runeData.Value));
                    break;
                case RangedChainShotPlaceholder:
                    ApplyToHeroes(heroes, hero => hero.ApplyAttackSpeedPercent(Mathf.Max(0f, runeData.Value) * 0.5f));
                    Debug.Log("Ranged chain shot is reserved as a v1.1 combat hook. Applying temporary attack speed support.");
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
                    Debug.Log($"Rune effect '{runeData.EffectKey}' is reserved as a prototype hook.");
                    break;
                default:
                    Debug.Log($"RuneEffectApplier reserved rune effect key '{runeData.EffectKey}' for a later prototype.");
                    break;
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
    }
}
