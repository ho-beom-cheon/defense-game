using UnityEngine;

namespace RuneGate
{
    [CreateAssetMenu(fileName = "RuntimePixelVisualCatalog", menuName = "RuneGate/Runtime Pixel Visual Catalog")]
    public sealed class RuntimePixelVisualCatalog : ScriptableObject
    {
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

        public Sprite GetSprite(string assetPath)
        {
            switch (assetPath)
            {
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
                default:
                    return null;
            }
        }
    }
}
