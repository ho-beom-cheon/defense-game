using UnityEngine;

namespace RuneGate
{
    public static class RuntimeSpritePolicy
    {
        public static float GetHeroTargetHeight(HeroData heroData)
        {
            return GetHeroTargetHeight(heroData, GameFrameLayout.IsPortrait);
        }

        public static float GetHeroTargetHeight(HeroData heroData, bool portrait)
        {
            float baseHeight;
            if (heroData == null)
            {
                baseHeight = 1.35f;
                return baseHeight * ResolvePortraitScale(portrait, false);
            }

            switch (heroData.Role)
            {
                case HeroRole.Tank:
                    baseHeight = 1.48f;
                    break;
                case HeroRole.Engineer:
                    baseHeight = 1.6f;
                    break;
                case HeroRole.Healer:
                    baseHeight = 1.4f;
                    break;
                case HeroRole.Assassin:
                    baseHeight = 1.38f;
                    break;
                case HeroRole.Mage:
                case HeroRole.RangedDps:
                    baseHeight = 1.35f;
                    break;
                default:
                    baseHeight = 1.32f;
                    break;
            }

            return baseHeight * ResolvePortraitScale(portrait, false);
        }

        public static Vector2 GetMonsterHpBarSize(MonsterData monsterData)
        {
            Vector2 baseSize;
            if (monsterData == null)
            {
                baseSize = new Vector2(0.72f, 0.07f);
                return baseSize * ResolvePortraitScale(GameFrameLayout.IsPortrait, false);
            }

            if (monsterData.IsBoss)
            {
                baseSize = new Vector2(1.55f, 0.13f);
                return baseSize * ResolvePortraitScale(GameFrameLayout.IsPortrait, true);
            }

            switch (monsterData.MonsterType)
            {
                case MonsterType.Tank:
                    baseSize = new Vector2(0.95f, 0.09f);
                    break;
                case MonsterType.Fast:
                case MonsterType.Flying:
                    baseSize = new Vector2(0.64f, 0.07f);
                    break;
                default:
                    baseSize = new Vector2(0.72f, 0.075f);
                    break;
            }

            return baseSize * ResolvePortraitScale(GameFrameLayout.IsPortrait, false);
        }

        public static float GetMonsterTargetHeight(MonsterData monsterData)
        {
            return GetMonsterTargetHeight(monsterData, GameFrameLayout.IsPortrait);
        }

        public static float GetMonsterTargetHeight(MonsterData monsterData, bool portrait)
        {
            float baseHeight;
            if (monsterData == null)
            {
                return ResolvePortraitScale(portrait, false);
            }

            if (monsterData.IsBoss)
            {
                return 2.55f * ResolvePortraitScale(portrait, true);
            }

            switch (monsterData.MonsterType)
            {
                case MonsterType.Tank:
                    baseHeight = 1.38f;
                    break;
                case MonsterType.Fast:
                case MonsterType.Flying:
                    baseHeight = 0.95f;
                    break;
                case MonsterType.Splitter:
                    baseHeight = 1f;
                    break;
                case MonsterType.Undead:
                    baseHeight = 1.08f;
                    break;
                default:
                    baseHeight = 1.02f;
                    break;
            }

            return baseHeight * ResolvePortraitScale(portrait, false);
        }

        private static float ResolvePortraitScale(bool portrait, bool boss)
        {
            if (!portrait)
            {
                return 1f;
            }

            return boss ? 1.1f : 1.18f;
        }

        public static float GetMonsterHpBarYOffset(MonsterData monsterData)
        {
            return GetMonsterTargetHeight(monsterData) * (monsterData != null && monsterData.IsBoss ? 0.58f : 0.64f);
        }

        public static float GetMonsterEstimatedHalfWidth(MonsterData monsterData)
        {
            float height = GetMonsterTargetHeight(monsterData);
            if (monsterData != null && monsterData.IsBoss)
            {
                return height * 0.46f;
            }

            if (monsterData != null && monsterData.MonsterType == MonsterType.Tank)
            {
                return height * 0.42f;
            }

            return height * 0.34f;
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
