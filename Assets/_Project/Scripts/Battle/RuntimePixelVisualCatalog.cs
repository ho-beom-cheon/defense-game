using UnityEngine;

namespace RuneGate
{
    [CreateAssetMenu(fileName = "RuntimePixelVisualCatalog", menuName = "RuneGate/Runtime Pixel Visual Catalog")]
    public sealed class RuntimePixelVisualCatalog : ScriptableObject
    {
        [SerializeField] private Sprite appSplashBackground;
        [SerializeField] private Sprite bgGoblinForestLanes;
        [SerializeField] private Sprite fxShieldBash;
        [SerializeField] private Sprite fxRapidShot;
        [SerializeField] private Sprite fxMeteorImpact;
        [SerializeField] private Sprite fxHolyHeal;
        [SerializeField] private Sprite fxTurretShot;
        [SerializeField] private Sprite fxShadowSlash;
        [SerializeField] private Sprite fxHitSpark;
        [SerializeField] private Sprite fxDeathPuff;
        [SerializeField] private Sprite uiPanelDark;
        [SerializeField] private Sprite uiButtonSkill;
        [SerializeField] private Sprite uiRuneCardBase;
        [SerializeField] private Sprite uiTutorialArrow;
        [SerializeField] private Sprite uiTapIndicator;
        [SerializeField] private Sprite uiStageNodeUnlocked;
        [SerializeField] private Sprite uiStageNodeLocked;
        [SerializeField] private Sprite uiStageNodeCleared;
        [SerializeField] private Sprite uiUpgradeCrystalHp;
        [SerializeField] private Sprite uiUpgradeHeroAttack;
        [SerializeField] private Sprite uiUpgradeAttackSpeed;
        [SerializeField] private Sprite uiUpgradeSkillCooldown;
        [SerializeField] private Sprite uiIconSettings;
        [SerializeField] private Sprite uiIconResetSave;
        [SerializeField] private Sprite uiIconSave;
        [SerializeField] private Sprite uiBadgeEasy;
        [SerializeField] private Sprite uiBadgeNormal;
        [SerializeField] private Sprite uiBadgeHard;
        [SerializeField] private Sprite uiBadgeNightmare;
        [SerializeField] private Sprite uiPetShadowShard;
        [SerializeField] private Sprite uiPetContractSeal;
        [SerializeField] private Sprite uiPetSlotEmpty;
        [SerializeField] private Sprite uiPetSlotEquipped;
        [SerializeField] private Sprite uiPetPanelBg;
        [SerializeField] private Sprite uiPetAttackBoost;
        [SerializeField] private Sprite uiPetGoldBoost;
        [SerializeField] private Sprite uiPetCrystalGuard;
        [SerializeField] private Sprite uiPetSlowAura;

        public Sprite GetSprite(string assetPath)
        {
            switch (assetPath)
            {
                case RuntimePixelAssetLoader.AppSplashBackground:
                    return appSplashBackground;
                case RuntimePixelAssetLoader.BackgroundGoblinForestLanes:
                    return bgGoblinForestLanes;
                case RuntimePixelAssetLoader.EffectShieldBash:
                    return fxShieldBash;
                case RuntimePixelAssetLoader.EffectRapidShot:
                    return fxRapidShot;
                case RuntimePixelAssetLoader.EffectMeteorImpact:
                    return fxMeteorImpact;
                case RuntimePixelAssetLoader.EffectHolyHeal:
                    return fxHolyHeal;
                case RuntimePixelAssetLoader.EffectTurretShot:
                    return fxTurretShot;
                case RuntimePixelAssetLoader.EffectShadowSlash:
                    return fxShadowSlash;
                case RuntimePixelAssetLoader.EffectHitSpark:
                    return fxHitSpark;
                case RuntimePixelAssetLoader.EffectDeathPuff:
                    return fxDeathPuff;
                case RuntimePixelAssetLoader.UiPanelDark:
                    return uiPanelDark;
                case RuntimePixelAssetLoader.UiButtonSkill:
                    return uiButtonSkill;
                case RuntimePixelAssetLoader.UiRuneCardBase:
                    return uiRuneCardBase;
                case RuntimePixelAssetLoader.UiTutorialArrow:
                    return uiTutorialArrow;
                case RuntimePixelAssetLoader.UiTapIndicator:
                    return uiTapIndicator;
                case RuntimePixelAssetLoader.UiStageNodeUnlocked:
                    return uiStageNodeUnlocked;
                case RuntimePixelAssetLoader.UiStageNodeLocked:
                    return uiStageNodeLocked;
                case RuntimePixelAssetLoader.UiStageNodeCleared:
                    return uiStageNodeCleared;
                case RuntimePixelAssetLoader.UiUpgradeCrystalHp:
                    return uiUpgradeCrystalHp;
                case RuntimePixelAssetLoader.UiUpgradeHeroAttack:
                    return uiUpgradeHeroAttack;
                case RuntimePixelAssetLoader.UiUpgradeAttackSpeed:
                    return uiUpgradeAttackSpeed;
                case RuntimePixelAssetLoader.UiUpgradeSkillCooldown:
                    return uiUpgradeSkillCooldown;
                case RuntimePixelAssetLoader.UiIconSettings:
                    return uiIconSettings;
                case RuntimePixelAssetLoader.UiIconResetSave:
                    return uiIconResetSave;
                case RuntimePixelAssetLoader.UiIconSave:
                    return uiIconSave;
                case RuntimePixelAssetLoader.UiBadgeEasy:
                    return uiBadgeEasy;
                case RuntimePixelAssetLoader.UiBadgeNormal:
                    return uiBadgeNormal;
                case RuntimePixelAssetLoader.UiBadgeHard:
                    return uiBadgeHard;
                case RuntimePixelAssetLoader.UiBadgeNightmare:
                    return uiBadgeNightmare;
                case RuntimePixelAssetLoader.UiPetShadowShard:
                    return uiPetShadowShard;
                case RuntimePixelAssetLoader.UiPetContractSeal:
                    return uiPetContractSeal;
                case RuntimePixelAssetLoader.UiPetSlotEmpty:
                    return uiPetSlotEmpty;
                case RuntimePixelAssetLoader.UiPetSlotEquipped:
                    return uiPetSlotEquipped;
                case RuntimePixelAssetLoader.UiPetPanelBg:
                    return uiPetPanelBg;
                case RuntimePixelAssetLoader.UiPetAttackBoost:
                    return uiPetAttackBoost;
                case RuntimePixelAssetLoader.UiPetGoldBoost:
                    return uiPetGoldBoost;
                case RuntimePixelAssetLoader.UiPetCrystalGuard:
                    return uiPetCrystalGuard;
                case RuntimePixelAssetLoader.UiPetSlowAura:
                    return uiPetSlowAura;
                default:
                    return null;
            }
        }
    }
}
