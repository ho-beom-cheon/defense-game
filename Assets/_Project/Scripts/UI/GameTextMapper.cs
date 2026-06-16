namespace RuneGate
{
    public static class GameTextMapper
    {
        public static string BattleStateName(BattleState state)
        {
            switch (state)
            {
                case RuneGate.BattleState.Preparing:
                    return "준비 중";
                case RuneGate.BattleState.WaveRunning:
                    return "전투 중";
                case RuneGate.BattleState.RuneSelection:
                    return "룬 선택 중";
                case RuneGate.BattleState.Victory:
                    return "승리";
                case RuneGate.BattleState.Defeat:
                    return "패배";
                default:
                    return "대기 중";
            }
        }

        public static string Difficulty(string difficultyId)
        {
            switch (difficultyId)
            {
                case "easy":
                    return "쉬움";
                case "hard":
                    return "어려움";
                case "nightmare":
                    return "악몽";
                case "normal":
                    return "보통";
                default:
                    return "보통";
            }
        }

        public static string StageName(StageData stageData)
        {
            if (stageData == null)
            {
                return "스테이지";
            }

            if (!string.IsNullOrWhiteSpace(stageData.DisplayNameKorean))
            {
                return stageData.DisplayNameKorean;
            }

            return string.IsNullOrWhiteSpace(stageData.DisplayName) ? "스테이지" : stageData.DisplayName;
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
                    return $"재문 숲 {stageNumber}";
                }
            }

            return stageId;
        }

        public static string SkillName(SkillData skillData)
        {
            if (skillData == null)
            {
                return "스킬";
            }

            switch (skillData.SkillId)
            {
                case "skill_shield_bash":
                    return "방패 강타";
                case "skill_rapid_shot":
                    return "연속 사격";
                case "skill_meteor":
                    return "운석 낙하";
                case "skill_holy_heal":
                    return "성스러운 회복";
                case "skill_build_turret":
                    return "임시 포탑";
                case "skill_shadow_strike":
                    return "그림자 급습";
                default:
                    return string.IsNullOrWhiteSpace(skillData.DisplayName) ? "스킬" : skillData.DisplayName;
            }
        }

        public static string RuneRarityName(RuneRarity rarity)
        {
            switch (rarity)
            {
                case RuneGate.RuneRarity.Rare:
                    return "희귀";
                case RuneGate.RuneRarity.Epic:
                    return "영웅";
                default:
                    return "일반";
            }
        }

        public static string SkillStatus(BattleState battleState, float cooldownRemaining, bool hasSkill)
        {
            if (battleState == RuneGate.BattleState.Victory || battleState == RuneGate.BattleState.Defeat)
            {
                return "전투 종료";
            }

            if (battleState == RuneGate.BattleState.RuneSelection)
            {
                return "룬 선택 중";
            }

            if (!hasSkill)
            {
                return "사용 불가";
            }

            if (cooldownRemaining > 0f)
            {
                return $"재사용 대기 {cooldownRemaining:0}초";
            }

            return "준비 완료";
        }
    }
}
