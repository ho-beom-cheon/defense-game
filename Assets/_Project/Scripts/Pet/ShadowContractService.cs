using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public static class ShadowContractService
    {
        public const int RequiredShardCount = 5;

        private static readonly Dictionary<string, int> LastBattleDropsByMonsterId = new Dictionary<string, int>();
        private static readonly ShadowPetDefinition[] Definitions =
        {
            new ShadowPetDefinition("monster_goblin_001", "\ubb38\ud2c8 \ub3c4\uae68\ube44", ShadowPetPassiveType.GoldRewardPercent, 0.05f),
            new ShadowPetDefinition("monster_orc_001", "\uc7ac\uac11 \ub3cc\uaca9\ubcd1", ShadowPetPassiveType.CrystalMaxHpPercent, 0.05f),
            new ShadowPetDefinition("monster_wolf_001", "\ubd80\uc2dd \ub291\ub300", ShadowPetPassiveType.HeroAttackSpeedPercent, 0.03f),
            new ShadowPetDefinition("monster_bat_001", "\uade0\uc5f4 \uae4c\ub9c8\uadc0", ShadowPetPassiveType.MonsterSlowPercent, 0.03f),
            new ShadowPetDefinition("monster_slime_001", "\ub8ec\ud575 \uc810\uc561", ShadowPetPassiveType.CrystalMaxHpPercent, 0.05f),
            new ShadowPetDefinition("monster_skeleton_001", "\ub9dd\uac01\uc758 \ubf08\ubcd1", ShadowPetPassiveType.HeroAttackPercent, 0.03f),
            new ShadowPetDefinition("boss_orc_warlord_001", "\uadf8\ub8f8\ubc14\ub974", ShadowPetPassiveType.HeroAttackPercent, 0.04f)
        };

        public static IReadOnlyDictionary<string, int> LastBattleDrops => LastBattleDropsByMonsterId;
        public static IReadOnlyList<ShadowPetDefinition> PetDefinitions => Definitions;

        public static void BeginBattle()
        {
            LastBattleDropsByMonsterId.Clear();
        }

        public static MonsterVariantType RollVariant(MonsterData monsterData, StageData stageData, WaveData waveData)
        {
            if (monsterData == null || monsterData.IsBoss)
            {
                return MonsterVariantType.Normal;
            }

            int stageNumber = ExtractStageNumber(stageData);
            float roll = Random.value;
            if (stageNumber <= 2)
            {
                return roll < 0.06f ? MonsterVariantType.Swift : MonsterVariantType.Normal;
            }

            if (stageNumber <= 5)
            {
                if (roll < 0.08f)
                {
                    return MonsterVariantType.Swift;
                }

                return roll < 0.12f ? MonsterVariantType.Elite : MonsterVariantType.Normal;
            }

            if (roll < 0.08f)
            {
                return MonsterVariantType.Swift;
            }

            if (roll < 0.14f)
            {
                return MonsterVariantType.Elite;
            }

            return roll < 0.18f ? MonsterVariantType.Cursed : MonsterVariantType.Normal;
        }

        public static int GetMaxHp(MonsterData monsterData, MonsterVariantType variantType)
        {
            int baseValue = monsterData != null ? Mathf.Max(1, monsterData.MaxHp) : 1;
            int variantValue;
            switch (variantType)
            {
                case MonsterVariantType.Swift:
                    variantValue = Mathf.Max(1, Mathf.RoundToInt(baseValue * 0.85f));
                    break;
                case MonsterVariantType.Elite:
                    variantValue = Mathf.Max(1, Mathf.RoundToInt(baseValue * 1.35f));
                    break;
                case MonsterVariantType.Cursed:
                    variantValue = Mathf.Max(1, Mathf.RoundToInt(baseValue * 1.18f));
                    break;
                default:
                    variantValue = baseValue;
                    break;
            }

            return DifficultyRules.ApplyMonsterHp(variantValue);
        }

        public static float GetMoveSpeed(MonsterData monsterData, MonsterVariantType variantType)
        {
            float baseValue = monsterData != null ? Mathf.Max(0f, monsterData.MoveSpeed) : 0f;
            float passiveSlow = GetEquippedPassiveType() == ShadowPetPassiveType.MonsterSlowPercent ? GetEquippedPassiveValue() : 0f;
            float variantMultiplier = variantType == MonsterVariantType.Swift ? 1.25f : 1f;
            return DifficultyRules.ApplyMonsterMoveSpeed(baseValue * variantMultiplier * Mathf.Clamp01(1f - passiveSlow));
        }

        public static int GetRewardGold(MonsterData monsterData, MonsterVariantType variantType)
        {
            int baseValue = monsterData != null ? Mathf.Max(0, monsterData.RewardGold) : 0;
            float multiplier = variantType == MonsterVariantType.Elite || variantType == MonsterVariantType.Cursed ? 1.25f : 1f;
            return DifficultyRules.ApplyMonsterRewardGold(Mathf.RoundToInt(baseValue * multiplier));
        }

        public static int GetDamageToCrystal(MonsterData monsterData, MonsterVariantType variantType)
        {
            int baseValue = monsterData != null ? Mathf.Max(0, monsterData.DamageToCrystal) : 0;
            float multiplier = variantType == MonsterVariantType.Cursed ? 1.15f : 1f;
            return DifficultyRules.ApplyMonsterCrystalDamage(Mathf.RoundToInt(baseValue * multiplier));
        }

        public static Color GetVariantTint(MonsterVariantType variantType)
        {
            switch (variantType)
            {
                case MonsterVariantType.Swift:
                    return new Color(0.74f, 0.95f, 1f, 1f);
                case MonsterVariantType.Elite:
                    return new Color(1f, 0.88f, 0.42f, 1f);
                case MonsterVariantType.Cursed:
                    return new Color(0.82f, 0.56f, 1f, 1f);
                default:
                    return Color.white;
            }
        }

        public static string VariantDisplayName(MonsterVariantType variantType)
        {
            switch (variantType)
            {
                case MonsterVariantType.Swift:
                    return "\uc2e0\uc18d";
                case MonsterVariantType.Elite:
                    return "\uc815\uc608";
                case MonsterVariantType.Cursed:
                    return "\uc800\uc8fc";
                default:
                    return "\uc77c\ubc18";
            }
        }

        public static bool RecordMonsterKilled(MonsterController monster)
        {
            if (monster == null || monster.Data == null)
            {
                return false;
            }

            string monsterId = monster.Data.MonsterId;
            float chance = GetShardDropChance(monster.Data, monster.VariantType);
            if (Random.value > chance)
            {
                return false;
            }

            SaveManager.AddMonsterShards(monsterId, 1);
            if (!LastBattleDropsByMonsterId.ContainsKey(monsterId))
            {
                LastBattleDropsByMonsterId[monsterId] = 0;
            }

            LastBattleDropsByMonsterId[monsterId]++;
            return true;
        }

        public static bool CanContract(string monsterId)
        {
            return !SaveManager.HasContractedPet(monsterId) && SaveManager.GetMonsterShardCount(monsterId) >= RequiredShardCount;
        }

        public static bool TryContract(string monsterId)
        {
            return SaveManager.TryContractPet(monsterId, RequiredShardCount);
        }

        public static void Equip(string monsterId)
        {
            SaveManager.EquipPet(monsterId);
        }

        public static void Unequip()
        {
            SaveManager.UnequipPet();
        }

        public static ShadowPetPassiveType GetEquippedPassiveType()
        {
            ShadowPetDefinition definition = GetDefinition(SaveManager.Current.equippedPetId);
            return string.IsNullOrWhiteSpace(definition.MonsterId) ? ShadowPetPassiveType.None : definition.PassiveType;
        }

        public static float GetEquippedPassiveValue()
        {
            ShadowPetDefinition definition = GetDefinition(SaveManager.Current.equippedPetId);
            return string.IsNullOrWhiteSpace(definition.MonsterId) ? 0f : Mathf.Max(0f, definition.PassiveValue);
        }

        public static int GetCrystalMaxHpBonus(int baseCrystalHp)
        {
            return GetEquippedPassiveType() == ShadowPetPassiveType.CrystalMaxHpPercent
                ? Mathf.RoundToInt(Mathf.Max(0, baseCrystalHp) * GetEquippedPassiveValue())
                : 0;
        }

        public static float GetGoldRewardMultiplier()
        {
            return GetEquippedPassiveType() == ShadowPetPassiveType.GoldRewardPercent ? 1f + GetEquippedPassiveValue() : 1f;
        }

        public static void ApplyHeroPassives(IReadOnlyList<HeroController> heroes)
        {
            if (heroes == null)
            {
                return;
            }

            ShadowPetPassiveType passiveType = GetEquippedPassiveType();
            float value = GetEquippedPassiveValue();
            for (int i = 0; i < heroes.Count; i++)
            {
                HeroController hero = heroes[i];
                if (hero == null)
                {
                    continue;
                }

                if (passiveType == ShadowPetPassiveType.HeroAttackPercent)
                {
                    hero.ApplyAttackPercent(value);
                }
                else if (passiveType == ShadowPetPassiveType.HeroAttackSpeedPercent)
                {
                    hero.ApplyAttackSpeedPercent(value);
                }
            }
        }

        public static ShadowPetDefinition GetDefinition(string monsterId)
        {
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                return default;
            }

            for (int i = 0; i < Definitions.Length; i++)
            {
                if (Definitions[i].MonsterId == monsterId)
                {
                    return Definitions[i];
                }
            }

            return default;
        }

        public static string GetMonsterDisplayName(string monsterId)
        {
            ShadowPetDefinition definition = GetDefinition(monsterId);
            return string.IsNullOrWhiteSpace(definition.DisplayName) ? monsterId : definition.DisplayName;
        }

        public static string GetPassiveDescription(ShadowPetDefinition definition)
        {
            switch (definition.PassiveType)
            {
                case ShadowPetPassiveType.HeroAttackPercent:
                    return $"\uc601\uc6c5 \uacf5\uaca9\ub825 +{definition.PassiveValue * 100f:0.#}%";
                case ShadowPetPassiveType.HeroAttackSpeedPercent:
                    return $"\uc601\uc6c5 \uacf5\uaca9 \uc18d\ub3c4 +{definition.PassiveValue * 100f:0.#}%";
                case ShadowPetPassiveType.GoldRewardPercent:
                    return $"\uc2b9\ub9ac \uace8\ub4dc +{definition.PassiveValue * 100f:0.#}%";
                case ShadowPetPassiveType.CrystalMaxHpPercent:
                    return $"\ud06c\ub9ac\uc2a4\ud0c8 \ucd5c\ub300 HP +{definition.PassiveValue * 100f:0.#}%";
                case ShadowPetPassiveType.MonsterSlowPercent:
                    return $"\ubaa8\ub4e0 \ubaac\uc2a4\ud130 \uc774\ub3d9 \uc18d\ub3c4 -{definition.PassiveValue * 100f:0.#}%";
                default:
                    return "\ud328\uc2dc\ube0c \uc5c6\uc74c";
            }
        }

        private static float GetShardDropChance(MonsterData monsterData, MonsterVariantType variantType)
        {
            if (monsterData != null && monsterData.IsBoss)
            {
                return 0.3f;
            }

            switch (variantType)
            {
                case MonsterVariantType.Swift:
                    return 0.06f;
                case MonsterVariantType.Elite:
                    return 0.12f;
                case MonsterVariantType.Cursed:
                    return 0.1f;
                default:
                    return 0.05f;
            }
        }

        private static int ExtractStageNumber(StageData stageData)
        {
            if (stageData == null || string.IsNullOrWhiteSpace(stageData.StageId))
            {
                return 1;
            }

            string id = stageData.StageId;
            int lastUnderscore = id.LastIndexOf('_');
            if (lastUnderscore >= 0 && lastUnderscore < id.Length - 1 && int.TryParse(id.Substring(lastUnderscore + 1), out int stageNumber))
            {
                return Mathf.Max(1, stageNumber);
            }

            return 1;
        }
    }
}
