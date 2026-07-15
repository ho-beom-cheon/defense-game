using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public static class RuntimePixelAssetLoader
    {
        public const string AppSplashBackground = "Assets/_Project/Art/RuntimePixel/App/splash_background_1080x1920.png";
        public const string BackgroundGoblinForestLanes = "Assets/_Project/Art/RuntimePixel/Backgrounds/bg_goblin_forest_lanes.png";
        public const string EffectShieldBash = "Assets/_Project/Art/RuntimePixel/Effects/fx_shield_bash.png";
        public const string EffectRapidShot = "Assets/_Project/Art/RuntimePixel/Effects/fx_rapid_shot.png";
        public const string EffectMeteorImpact = "Assets/_Project/Art/RuntimePixel/Effects/fx_meteor_impact.png";
        public const string EffectHolyHeal = "Assets/_Project/Art/RuntimePixel/Effects/fx_holy_heal.png";
        public const string EffectTurretShot = "Assets/_Project/Art/RuntimePixel/Effects/fx_turret_shot.png";
        public const string EffectShadowSlash = "Assets/_Project/Art/RuntimePixel/Effects/fx_shadow_slash.png";
        public const string EffectHitSpark = "Assets/_Project/Art/RuntimePixel/Effects/fx_hit_spark.png";
        public const string EffectDeathPuff = "Assets/_Project/Art/RuntimePixel/Effects/fx_death_puff.png";
        public const string UiPanelDark = "Assets/_Project/Art/RuntimePixel/UI/ui_panel_dark.png";
        public const string UiButtonSkill = "Assets/_Project/Art/RuntimePixel/UI/ui_button_skill.png";
        public const string UiRuneCardBase = "Assets/_Project/Art/RuntimePixel/UI/ui_rune_card_base.png";
        public const string UiTutorialArrow = "Assets/_Project/Art/RuntimePixel/UI/Tutorial/ui_tutorial_arrow.png";
        public const string UiTapIndicator = "Assets/_Project/Art/RuntimePixel/UI/Tutorial/ui_tap_indicator.png";
        public const string UiStageNodeUnlocked = "Assets/_Project/Art/RuntimePixel/UI/StageSelect/ui_stage_node_unlocked.png";
        public const string UiStageNodeLocked = "Assets/_Project/Art/RuntimePixel/UI/StageSelect/ui_stage_node_locked.png";
        public const string UiStageNodeCleared = "Assets/_Project/Art/RuntimePixel/UI/StageSelect/ui_stage_node_cleared.png";
        public const string UiUpgradeCrystalHp = "Assets/_Project/Art/RuntimePixel/UI/Upgrade/icon_upgrade_crystal_hp.png";
        public const string UiUpgradeHeroAttack = "Assets/_Project/Art/RuntimePixel/UI/Upgrade/icon_upgrade_hero_attack.png";
        public const string UiUpgradeAttackSpeed = "Assets/_Project/Art/RuntimePixel/UI/Upgrade/icon_upgrade_attack_speed.png";
        public const string UiUpgradeSkillCooldown = "Assets/_Project/Art/RuntimePixel/UI/Upgrade/icon_upgrade_skill_cooldown.png";
        public const string UiIconSettings = "Assets/_Project/Art/RuntimePixel/UI/System/ui_icon_settings.png";
        public const string UiIconResetSave = "Assets/_Project/Art/RuntimePixel/UI/System/ui_icon_reset_save.png";
        public const string UiIconSave = "Assets/_Project/Art/RuntimePixel/UI/System/ui_icon_save.png";
        public const string UiBadgeEasy = "Assets/_Project/Art/RuntimePixel/UI/Difficulty/badge_easy.png";
        public const string UiBadgeNormal = "Assets/_Project/Art/RuntimePixel/UI/Difficulty/badge_normal.png";
        public const string UiBadgeHard = "Assets/_Project/Art/RuntimePixel/UI/Difficulty/badge_hard.png";
        public const string UiBadgeNightmare = "Assets/_Project/Art/RuntimePixel/UI/Difficulty/badge_nightmare.png";
        public const string UiPetShadowShard = "Assets/_Project/Art/RuntimePixel/UI/PetContract/icon_shadow_shard.png";
        public const string UiPetContractSeal = "Assets/_Project/Art/RuntimePixel/UI/PetContract/icon_contract_seal.png";
        public const string UiPetSlotEmpty = "Assets/_Project/Art/RuntimePixel/UI/PetContract/ui_pet_slot_empty.png";
        public const string UiPetSlotEquipped = "Assets/_Project/Art/RuntimePixel/UI/PetContract/ui_pet_slot_equipped.png";
        public const string UiPetPanelBg = "Assets/_Project/Art/RuntimePixel/UI/PetContract/ui_contract_panel_bg.png";
        public const string UiPetAttackBoost = "Assets/_Project/Art/RuntimePixel/UI/PetContract/icon_pet_attack_boost.png";
        public const string UiPetGoldBoost = "Assets/_Project/Art/RuntimePixel/UI/PetContract/icon_pet_gold_boost.png";
        public const string UiPetCrystalGuard = "Assets/_Project/Art/RuntimePixel/UI/PetContract/icon_pet_crystal_guard.png";
        public const string UiPetSlowAura = "Assets/_Project/Art/RuntimePixel/UI/PetContract/icon_pet_slow_aura.png";

        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();
        private static RuntimePixelVisualCatalog visualCatalog;

        public static Sprite LoadSprite(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return null;
            }

            if (SpriteCache.TryGetValue(assetPath, out Sprite cachedSprite))
            {
                return cachedSprite;
            }

            Sprite sprite = LoadFromCatalog(assetPath);
#if UNITY_EDITOR
            if (sprite == null)
            {
                sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            }
#endif
            SpriteCache[assetPath] = sprite;
            return sprite;
        }

        private static Sprite LoadFromCatalog(string assetPath)
        {
            if (visualCatalog == null)
            {
                visualCatalog = Resources.Load<RuntimePixelVisualCatalog>("RuntimePixelVisualCatalog");
            }

            return visualCatalog != null ? visualCatalog.GetSprite(assetPath) : null;
        }
    }
}
