using UnityEngine;

namespace RuneGate
{
    public static class DifficultyRules
    {
        public const string Easy = "easy";
        public const string Normal = "normal";
        public const string Hard = "hard";
        public const string Nightmare = "nightmare";

        public static string CurrentDifficultyId => Normalize(SaveManager.Current.selectedDifficultyId);

        public static string Normalize(string difficultyId)
        {
            string value = string.IsNullOrWhiteSpace(difficultyId) ? Normal : difficultyId.Trim().ToLowerInvariant();
            switch (value)
            {
                case Easy:
                case Hard:
                case Nightmare:
                    return value;
                default:
                    return Normal;
            }
        }

        public static int ApplyMonsterHp(int baseHp)
        {
            return ApplyMonsterHp(baseHp, CurrentDifficultyId);
        }

        public static int ApplyMonsterHp(int baseHp, string difficultyId)
        {
            return Mathf.Max(1, Mathf.RoundToInt(Mathf.Max(1, baseHp) * MonsterHpMultiplier(difficultyId)));
        }

        public static float ApplyMonsterMoveSpeed(float baseSpeed)
        {
            return ApplyMonsterMoveSpeed(baseSpeed, CurrentDifficultyId);
        }

        public static float ApplyMonsterMoveSpeed(float baseSpeed, string difficultyId)
        {
            return Mathf.Max(0.05f, Mathf.Max(0.05f, baseSpeed) * MonsterSpeedMultiplier(difficultyId));
        }

        public static int ApplyMonsterCrystalDamage(int baseDamage)
        {
            return ApplyMonsterCrystalDamage(baseDamage, CurrentDifficultyId);
        }

        public static int ApplyMonsterCrystalDamage(int baseDamage, string difficultyId)
        {
            return Mathf.Max(0, Mathf.RoundToInt(Mathf.Max(0, baseDamage) * MonsterDamageMultiplier(difficultyId)));
        }

        public static int ApplyMonsterRewardGold(int baseReward)
        {
            return ApplyMonsterRewardGold(baseReward, CurrentDifficultyId);
        }

        public static int ApplyMonsterRewardGold(int baseReward, string difficultyId)
        {
            return Mathf.Max(0, Mathf.RoundToInt(Mathf.Max(0, baseReward) * RewardMultiplier(difficultyId)));
        }

        public static int ApplyCrystalMaxHp(int baseCrystalHp)
        {
            return ApplyCrystalMaxHp(baseCrystalHp, CurrentDifficultyId);
        }

        public static int ApplyCrystalMaxHp(int baseCrystalHp, string difficultyId)
        {
            return Mathf.Max(1, Mathf.RoundToInt(Mathf.Max(1, baseCrystalHp) * CrystalHpMultiplier(difficultyId)));
        }

        public static float RewardMultiplier(string difficultyId)
        {
            switch (Normalize(difficultyId))
            {
                case Easy:
                    return 0.9f;
                case Hard:
                    return 1.2f;
                case Nightmare:
                    return 1.45f;
                default:
                    return 1f;
            }
        }

        private static float MonsterHpMultiplier(string difficultyId)
        {
            switch (Normalize(difficultyId))
            {
                case Easy:
                    return 0.85f;
                case Hard:
                    return 1.18f;
                case Nightmare:
                    return 1.36f;
                default:
                    return 1f;
            }
        }

        private static float MonsterSpeedMultiplier(string difficultyId)
        {
            switch (Normalize(difficultyId))
            {
                case Easy:
                    return 0.92f;
                case Hard:
                    return 1.08f;
                case Nightmare:
                    return 1.16f;
                default:
                    return 1f;
            }
        }

        private static float MonsterDamageMultiplier(string difficultyId)
        {
            switch (Normalize(difficultyId))
            {
                case Easy:
                    return 0.8f;
                case Hard:
                    return 1.25f;
                case Nightmare:
                    return 1.5f;
                default:
                    return 1f;
            }
        }

        private static float CrystalHpMultiplier(string difficultyId)
        {
            switch (Normalize(difficultyId))
            {
                case Easy:
                    return 1.15f;
                case Hard:
                    return 0.95f;
                case Nightmare:
                    return 0.9f;
                default:
                    return 1f;
            }
        }
    }
}
