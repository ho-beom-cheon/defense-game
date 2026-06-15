using UnityEngine;

namespace RuneGate
{
    public static class RuntimeSpritePolicy
    {
        public static float GetHeroTargetHeight(HeroData heroData)
        {
            if (heroData == null)
            {
                return 1.1f;
            }

            switch (heroData.Role)
            {
                case HeroRole.Tank:
                    return 1.25f;
                case HeroRole.Engineer:
                    return 1.15f;
                default:
                    return 1.05f;
            }
        }

        public static float GetMonsterTargetHeight(MonsterData monsterData)
        {
            if (monsterData == null)
            {
                return 0.85f;
            }

            if (monsterData.IsBoss)
            {
                return 2.25f;
            }

            switch (monsterData.MonsterType)
            {
                case MonsterType.Tank:
                    return 1.35f;
                case MonsterType.Fast:
                case MonsterType.Flying:
                    return 0.8f;
                default:
                    return 0.9f;
            }
        }

        public static Color GetHeroColor(HeroData heroData)
        {
            if (heroData == null)
            {
                return Color.white;
            }

            switch (heroData.Role)
            {
                case HeroRole.Tank:
                    return new Color(0.28f, 0.48f, 1f);
                case HeroRole.RangedDps:
                    return new Color(0.28f, 0.8f, 0.34f);
                case HeroRole.Mage:
                    return new Color(1f, 0.28f, 0.2f);
                case HeroRole.Healer:
                    return new Color(1f, 0.92f, 0.55f);
                case HeroRole.Engineer:
                    return new Color(0.62f, 0.48f, 0.32f);
                case HeroRole.Assassin:
                    return new Color(0.26f, 0.16f, 0.34f);
                default:
                    return Color.white;
            }
        }

        public static Color GetMonsterColor(MonsterData monsterData)
        {
            if (monsterData == null)
            {
                return Color.gray;
            }

            if (monsterData.IsBoss)
            {
                return new Color(0.22f, 0.16f, 0.18f);
            }

            switch (monsterData.MonsterType)
            {
                case MonsterType.Tank:
                    return new Color(0.48f, 0.32f, 0.18f);
                case MonsterType.Fast:
                    return new Color(0.42f, 0.42f, 0.36f);
                case MonsterType.Flying:
                    return new Color(0.16f, 0.42f, 0.34f);
                case MonsterType.Splitter:
                    return new Color(0.36f, 0.8f, 0.62f);
                case MonsterType.Undead:
                    return new Color(0.72f, 0.72f, 0.68f);
                default:
                    return new Color(0.28f, 0.72f, 0.28f);
            }
        }
    }
}
