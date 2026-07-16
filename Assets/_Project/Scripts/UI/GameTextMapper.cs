namespace RuneGate
{
    public static class GameTextMapper
    {
        public static string BattleStateName(BattleState state)
        {
            switch (state)
            {
                case BattleState.Preparing:
                    return "\uc900\ube44 \uc911";
                case BattleState.WaveRunning:
                    return "\uc804\ud22c \uc911";
                case BattleState.RuneSelection:
                    return "\ub8ec \uc120\ud0dd \uc911";
                case BattleState.Victory:
                    return "\uc2b9\ub9ac";
                case BattleState.Defeat:
                    return "\ud328\ubc30";
                default:
                    return "\ub300\uae30 \uc911";
            }
        }

        public static string Difficulty(string difficultyId)
        {
            switch (difficultyId)
            {
                case "easy":
                    return "\uc26c\uc6c0";
                case "hard":
                    return "\uc5b4\ub824\uc6c0";
                case "nightmare":
                    return "\uc545\ubabd";
                case "normal":
                    return "\ubcf4\ud1b5";
                default:
                    return "\ubcf4\ud1b5";
            }
        }

        public static string StageName(StageData stageData)
        {
            if (stageData == null)
            {
                return "\uc2a4\ud14c\uc774\uc9c0";
            }

            if (!string.IsNullOrWhiteSpace(stageData.DisplayNameKorean))
            {
                return stageData.DisplayNameKorean;
            }

            return string.IsNullOrWhiteSpace(stageData.DisplayName) ? "\uc2a4\ud14c\uc774\uc9c0" : stageData.DisplayName;
        }

        public static string StageName(string stageId)
        {
            if (string.IsNullOrWhiteSpace(stageId))
            {
                return string.Empty;
            }

            const string stagePrefix = "stage_goblin_forest_";
            if (stageId.StartsWith(stagePrefix))
            {
                string suffix = stageId.Substring(stagePrefix.Length);
                if (int.TryParse(suffix, out int stageNumber))
                {
                    return $"\uc7ac\ubb38 \uc232 {stageNumber}";
                }
            }

            return stageId;
        }

        public static string SkillName(SkillData skillData)
        {
            if (skillData == null)
            {
                return "\uc2a4\ud0ac";
            }

            switch (skillData.SkillId)
            {
                case "skill_shield_bash":
                    return "\ubc29\ud328 \uac15\ud0c0";
                case "skill_rapid_shot":
                    return "\uc5f0\uc18d \uc0ac\uaca9";
                case "skill_meteor":
                    return "\uc6b4\uc11d \ub099\ud558";
                case "skill_holy_heal":
                    return "\uc131\uc2a4\ub7ec\uc6b4 \ud68c\ubcf5";
                case "skill_build_turret":
                    return "\uc784\uc2dc \ud3ec\ud0d1";
                case "skill_shadow_strike":
                    return "\uadf8\ub9bc\uc790 \uae09\uc2b5";
                default:
                    return string.IsNullOrWhiteSpace(skillData.DisplayName) ? "\uc2a4\ud0ac" : skillData.DisplayName;
            }
        }

        public static string HeroRoleName(HeroRole role)
        {
            switch (role)
            {
                case HeroRole.Tank:
                    return "방어";
                case HeroRole.MeleeDps:
                    return "근접 공격";
                case HeroRole.RangedDps:
                    return "원거리 공격";
                case HeroRole.Mage:
                    return "광역 마법";
                case HeroRole.Healer:
                    return "회복";
                case HeroRole.Support:
                    return "지원";
                case HeroRole.Engineer:
                    return "설치";
                case HeroRole.Assassin:
                    return "암살";
                default:
                    return "전투";
            }
        }

        public static string DifficultyUnlockRequirement(string difficultyId)
        {
            switch (DifficultyRules.Normalize(difficultyId))
            {
                case DifficultyRules.Hard:
                    return "보통 재문 숲 10 클리어";
                case DifficultyRules.Nightmare:
                    return "어려움 재문 숲 10 클리어";
                default:
                    return "기본 해금";
            }
        }

        public static string HeroPositionName(HeroPositionType positionType)
        {
            switch (positionType)
            {
                case HeroPositionType.Front:
                    return "전열";
                case HeroPositionType.Back:
                    return "후열";
                case HeroPositionType.Middle:
                default:
                    return "중열";
            }
        }

        public static string RuneRarityName(RuneRarity rarity)
        {
            switch (rarity)
            {
                case RuneRarity.Rare:
                    return "\ud76c\uadc0";
                case RuneRarity.Epic:
                    return "\uc601\uc6c5";
                default:
                    return "\uc77c\ubc18";
            }
        }

        public static string SkillStatus(BattleState battleState, float cooldownRemaining, bool hasSkill)
        {
            if (battleState == BattleState.Victory || battleState == BattleState.Defeat)
            {
                return "\uc804\ud22c \uc885\ub8cc";
            }

            if (battleState == BattleState.RuneSelection)
            {
                return "\ub8ec \uc120\ud0dd \uc911";
            }

            if (!hasSkill)
            {
                return "\uc0ac\uc6a9 \ubd88\uac00";
            }

            if (cooldownRemaining > 0f)
            {
                return $"\uc7ac\uc0ac\uc6a9 \ub300\uae30 {cooldownRemaining:0}\ucd08";
            }

            return "\uc900\ube44 \uc644\ub8cc";
        }

        public static string UpgradeEffectName(string effectKey)
        {
            switch (effectKey)
            {
                case UpgradeManager.CrystalMaxHpFlat:
                    return "\ud06c\ub9ac\uc2a4\ud0c8 \ucd5c\ub300 HP";
                case UpgradeManager.HeroAttackPercent:
                    return "\uc601\uc6c5 \uacf5\uaca9\ub825";
                case UpgradeManager.HeroAttackSpeedPercent:
                    return "\uc601\uc6c5 \uacf5\uaca9 \uc18d\ub3c4";
                case UpgradeManager.SkillCooldownPercent:
                    return "\uc2a4\ud0ac \uc7ac\uc0ac\uc6a9 \ub300\uae30";
                default:
                    return "\uc804\ud22c \ub2a5\ub825";
            }
        }

        public static string UpgradeName(UpgradeData upgradeData)
        {
            if (upgradeData == null)
            {
                return "\uc5c5\uadf8\ub808\uc774\ub4dc";
            }

            switch (upgradeData.EffectKey)
            {
                case UpgradeManager.CrystalMaxHpFlat:
                    return "\ud06c\ub9ac\uc2a4\ud0c8 \uac15\ud654";
                case UpgradeManager.HeroAttackPercent:
                    return "\uc601\uc6c5 \ud6c8\ub828";
                case UpgradeManager.HeroAttackSpeedPercent:
                    return "\uc804\ud22c \ub9ac\ub4ec";
                case UpgradeManager.SkillCooldownPercent:
                    return "\uc2a4\ud0ac \uc5f0\uc2b5";
                default:
                    return upgradeData.DisplayName;
            }
        }

        public static string UpgradeDescription(UpgradeData upgradeData)
        {
            if (upgradeData == null)
            {
                return string.Empty;
            }

            float value = upgradeData.ValuePerLevel < 0f ? -upgradeData.ValuePerLevel : upgradeData.ValuePerLevel;
            string percent = $"{value * 100f:0.#}%";
            switch (upgradeData.EffectKey)
            {
                case UpgradeManager.CrystalMaxHpFlat:
                    return $"\ud06c\ub9ac\uc2a4\ud0c8 \ucd5c\ub300 HP\uac00 \ub808\ubca8\ub9c8\ub2e4 {value:0.#} \uc99d\uac00\ud569\ub2c8\ub2e4.";
                case UpgradeManager.HeroAttackPercent:
                    return $"\ubaa8\ub4e0 \uc601\uc6c5\uc758 \uacf5\uaca9\ub825\uc774 \ub808\ubca8\ub9c8\ub2e4 {percent} \uc99d\uac00\ud569\ub2c8\ub2e4.";
                case UpgradeManager.HeroAttackSpeedPercent:
                    return $"\ubaa8\ub4e0 \uc601\uc6c5\uc758 \uacf5\uaca9 \uc18d\ub3c4\uac00 \ub808\ubca8\ub9c8\ub2e4 {percent} \uc99d\uac00\ud569\ub2c8\ub2e4.";
                case UpgradeManager.SkillCooldownPercent:
                    return $"\ubaa8\ub4e0 \uc601\uc6c5\uc758 \uc2a4\ud0ac \uc7ac\uc0ac\uc6a9 \ub300\uae30\uc2dc\uac04\uc774 \ub808\ubca8\ub9c8\ub2e4 {percent} \uac10\uc18c\ud569\ub2c8\ub2e4.";
                default:
                    return upgradeData.Description;
            }
        }
    }
}
