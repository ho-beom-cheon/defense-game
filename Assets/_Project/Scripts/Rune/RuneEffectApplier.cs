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
        private const string SkillCooldownPercent = "skill_cooldown_percent";
        private const string BossDamagePercent = "boss_damage_percent";

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
                case CrystalShieldFlat:
                case MonsterSlowPercent:
                case BossDamagePercent:
                    Debug.Log($"Rune effect '{runeData.EffectKey}' is reserved as a prototype hook.");
                    break;
                default:
                    Debug.LogWarning($"RuneEffectApplier does not recognize rune effect key '{runeData.EffectKey}'.");
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
    }
}
