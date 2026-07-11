using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class UpgradeManager : MonoBehaviour
    {
        public const string CrystalMaxHpFlat = "crystal_max_hp_flat";
        public const string HeroAttackPercent = "hero_attack_percent";
        public const string HeroAttackSpeedPercent = "hero_attack_speed_percent";
        public const string SkillCooldownPercent = "skill_cooldown_percent";

        [SerializeField] private List<UpgradeData> availableUpgrades = new List<UpgradeData>();

        public IReadOnlyList<UpgradeData> AvailableUpgrades => availableUpgrades;

        private void Awake()
        {
            EnsureAvailableUpgrades();
        }

        private void OnEnable()
        {
            EnsureAvailableUpgrades();
        }

        public int GetLevel(UpgradeData upgradeData)
        {
            return upgradeData == null ? 0 : SaveManager.GetUpgradeLevel(upgradeData.UpgradeId);
        }

        public int GetCost(UpgradeData upgradeData)
        {
            return CalculateCost(upgradeData, GetLevel(upgradeData));
        }

        public bool CanPurchase(UpgradeData upgradeData)
        {
            if (upgradeData == null)
            {
                return false;
            }

            int currentLevel = GetLevel(upgradeData);
            return currentLevel < upgradeData.MaxLevel && SaveManager.Current.totalGold >= CalculateCost(upgradeData, currentLevel);
        }

        public bool TryPurchase(UpgradeData upgradeData)
        {
            if (upgradeData == null)
            {
                Debug.LogWarning("UpgradeManager cannot purchase a missing upgrade.");
                return false;
            }

            int currentLevel = GetLevel(upgradeData);
            if (currentLevel >= upgradeData.MaxLevel)
            {
                return false;
            }

            int cost = CalculateCost(upgradeData, currentLevel);
            if (!SaveManager.SpendGold(cost))
            {
                return false;
            }

            SaveManager.SetUpgradeLevel(upgradeData.UpgradeId, currentLevel + 1);
            return true;
        }

        public static int CalculateCost(UpgradeData upgradeData, int currentLevel)
        {
            if (upgradeData == null)
            {
                return 0;
            }

            float multiplier = Mathf.Max(1f, upgradeData.CostMultiplier);
            return Mathf.Max(0, Mathf.RoundToInt(upgradeData.BaseCost * Mathf.Pow(multiplier, Mathf.Max(0, currentLevel))));
        }

        public static int GetCrystalMaxHpBonus(IReadOnlyList<UpgradeData> upgrades)
        {
            return Mathf.RoundToInt(GetTotalEffectValue(upgrades, CrystalMaxHpFlat));
        }

        public static void ApplyHeroUpgradeEffects(IReadOnlyList<UpgradeData> upgrades, IReadOnlyList<HeroController> heroes)
        {
            if (heroes == null)
            {
                return;
            }

            float attackPercent = GetTotalEffectValue(upgrades, HeroAttackPercent);
            float attackSpeedPercent = GetTotalEffectValue(upgrades, HeroAttackSpeedPercent);
            float skillCooldownPercent = GetTotalEffectValue(upgrades, SkillCooldownPercent);

            for (int i = 0; i < heroes.Count; i++)
            {
                HeroController hero = heroes[i];
                if (hero == null)
                {
                    continue;
                }

                if (attackPercent != 0f)
                {
                    hero.ApplyAttackPercent(attackPercent);
                }

                if (attackSpeedPercent != 0f)
                {
                    hero.ApplyAttackSpeedPercent(attackSpeedPercent);
                }

                if (skillCooldownPercent != 0f)
                {
                    hero.ApplySkillCooldownPercent(skillCooldownPercent);
                }
            }
        }

        public static float GetTotalEffectValue(IReadOnlyList<UpgradeData> upgrades, string effectKey)
        {
            if (upgrades == null || string.IsNullOrWhiteSpace(effectKey))
            {
                return 0f;
            }

            float total = 0f;
            for (int i = 0; i < upgrades.Count; i++)
            {
                UpgradeData upgrade = upgrades[i];
                if (upgrade == null || upgrade.EffectKey != effectKey)
                {
                    continue;
                }

                int level = SaveManager.GetUpgradeLevel(upgrade.UpgradeId);
                total += upgrade.ValuePerLevel * Mathf.Max(0, level);
            }

            return total;
        }

        private void EnsureAvailableUpgrades()
        {
            int validCount = 0;
            for (int i = 0; i < availableUpgrades.Count; i++)
            {
                if (availableUpgrades[i] != null)
                {
                    validCount++;
                }
            }

            if (validCount >= 4)
            {
                return;
            }

            List<UpgradeData> loadedUpgrades = PrototypeAssetLoader.LoadUpgrades();
            if (loadedUpgrades.Count == 0)
            {
                Debug.LogWarning("UpgradeManager could not load progression upgrades from RuntimeContentCatalog.");
                return;
            }

            availableUpgrades.Clear();
            for (int i = 0; i < loadedUpgrades.Count; i++)
            {
                if (loadedUpgrades[i] != null)
                {
                    availableUpgrades.Add(loadedUpgrades[i]);
                }
            }
        }
    }
}
