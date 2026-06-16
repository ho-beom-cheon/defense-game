using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public static class RuntimePixelAssetLoader
    {
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
