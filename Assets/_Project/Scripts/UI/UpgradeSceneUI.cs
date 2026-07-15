using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuneGate
{
    public sealed class UpgradeSceneUI : MonoBehaviour
    {
        [SerializeField] private UpgradeManager upgradeManager;
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private Rect panelRect = new Rect(190f, 54f, 660f, 500f);
        [SerializeField] private string stageSelectSceneName = "StageSelectScene";

        private string feedbackMessage = string.Empty;
        private Vector2 scrollPosition;
        private bool sceneTransitionRequested;
        private string lastPurchasedUpgradeId = string.Empty;
        private float feedbackExpiresAt;

        private void OnEnable()
        {
            if (upgradeManager == null)
            {
                upgradeManager = FindAnyObjectByType<UpgradeManager>();
            }

            SaveManager.LoadOrCreate();
            if (upgradeManager != null)
            {
                SaveManager.ClampUpgradeLevels(upgradeManager.AvailableUpgrades);
            }

            sceneTransitionRequested = false;
            feedbackMessage = string.Empty;
            lastPurchasedUpgradeId = string.Empty;
            feedbackExpiresAt = 0f;
        }

        private void OnGUI()
        {
            if (!drawRuntimeGui)
            {
                return;
            }

            UIResponsiveLayout.ApplyReadableDefaults();
            if (upgradeManager == null)
            {
                upgradeManager = FindAnyObjectByType<UpgradeManager>();
            }

            ScreenFrameRects frame = GameFrameLayout.UpgradeFrame();
            int upgradeCount = upgradeManager != null ? upgradeManager.AvailableUpgrades.Count : 0;
            UpgradeScreenLayoutRects layout = UpgradeScreenLayout.Calculate(frame, Screen.width, Screen.height, upgradeCount);
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUIStyle sectionStyle = RuntimePixelGuiUtility.CreateSolidPanelStyle(GUI.skin.box);
            GUIStyle alternateSectionStyle = RuntimePixelGuiUtility.CreateSolidPanelStyle(GUI.skin.box, true);
            GUIStyle buttonStyle = RuntimePixelGuiUtility.CreateButtonStyle(GUI.skin.button, RuntimePixelAssetLoader.UiButtonSkill);
            GUI.Box(frame.FrameRoot, GUIContent.none, panelStyle);
            GUI.Box(frame.HeaderArea, GUIContent.none, alternateSectionStyle);
            GUI.Box(frame.MainArea, GUIContent.none, sectionStyle);
            GUI.Box(frame.FooterArea, GUIContent.none, alternateSectionStyle);

            int totalLevels = GetTotalPurchasedLevels();
            int totalMaxLevels = GetTotalMaximumLevels();
            GUI.Label(layout.HeaderTitle, "\ubd09\ubb38 \uc815\ube44\uc18c", CreateLabelStyle(TextAnchor.MiddleLeft, 24f, FontStyle.Bold));
            GUI.Label(
                layout.HeaderSummary,
                $"\uc804\uc120 \uac15\ud654 {totalLevels}/{totalMaxLevels}  \u00b7  \uace8\ub4dc\ub85c \ubc29\uc5b4 \uae30\ub85d\uc744 \uc644\uc131\ud558\uc138\uc694.",
                CreateLabelStyle(TextAnchor.UpperLeft, 12f, FontStyle.Normal));
            GUI.Box(layout.GoldWallet, GUIContent.none, sectionStyle);
            GUI.Label(
                layout.GoldWallet,
                $"\ubcf4\uc720 \uace8\ub4dc\n{SaveManager.Current.totalGold:N0}",
                CreateLabelStyle(TextAnchor.MiddleCenter, 16f, FontStyle.Bold));

            if (upgradeManager == null || upgradeManager.AvailableUpgrades.Count == 0)
            {
                GUI.Label(
                    layout.Viewport,
                    "\uac15\ud654 \uae30\ub85d\uc744 \ubd88\ub7ec\uc624\uc9c0 \ubabb\ud588\uc2b5\ub2c8\ub2e4.\nTools/RuneGate/Validate Project\ub97c \ud655\uc778\ud558\uc138\uc694.",
                    CreateLabelStyle(TextAnchor.MiddleCenter, 16f, FontStyle.Normal));
            }
            else
            {
                scrollPosition = GUI.BeginScrollView(layout.Viewport, scrollPosition, layout.Content);
                for (int i = 0; i < upgradeManager.AvailableUpgrades.Count; i++)
                {
                    DrawUpgrade(upgradeManager.AvailableUpgrades[i], layout.CardRect(i), buttonStyle);
                }

                GUI.EndScrollView();
            }

            if (feedbackExpiresAt > 0f && Time.realtimeSinceStartup > feedbackExpiresAt)
            {
                feedbackMessage = string.Empty;
                lastPurchasedUpgradeId = string.Empty;
                feedbackExpiresAt = 0f;
            }

            string footerMessage = string.IsNullOrWhiteSpace(feedbackMessage)
                ? "\uac15\ud654 \uacb0\uacfc\ub294 \uad6c\ub9e4 \uc989\uc2dc \uc800\uc7a5\ub429\ub2c8\ub2e4."
                : feedbackMessage;
            GUI.Label(
                layout.FooterFeedback,
                footerMessage,
                CreateLabelStyle(TextAnchor.MiddleLeft, string.IsNullOrWhiteSpace(feedbackMessage) ? 11f : 12f, string.IsNullOrWhiteSpace(feedbackMessage) ? FontStyle.Normal : FontStyle.Bold));

            using (new GuiEnabledScope(!sceneTransitionRequested))
            {
                if (GUI.Button(layout.FooterButton, "\uc804\uc120\uc73c\ub85c \ub3cc\uc544\uac00\uae30", buttonStyle))
                {
                    LoadSceneOnce(stageSelectSceneName);
                    GUIUtility.ExitGUI();
                }
            }
        }

        private void DrawUpgrade(UpgradeData upgradeData, Rect cardRect, GUIStyle buttonStyle)
        {
            if (upgradeData == null)
            {
                GUI.Box(cardRect, "\uac15\ud654 \uae30\ub85d \uc5c6\uc74c", RuntimePixelGuiUtility.CreateSolidPanelStyle(GUI.skin.box));
                return;
            }

            int level = upgradeManager.GetLevel(upgradeData);
            int maxLevel = Mathf.Max(0, upgradeData.MaxLevel);
            int cost = upgradeManager.GetCost(upgradeData);
            bool maxed = level >= maxLevel;
            bool canAfford = SaveManager.Current.totalGold >= cost;
            bool recentlyPurchased = lastPurchasedUpgradeId == upgradeData.UpgradeId && Time.realtimeSinceStartup <= feedbackExpiresAt;
            Color accentColor = ResolveAccentColor(upgradeData.EffectKey, maxed, canAfford);
            if (recentlyPurchased)
            {
                float pulse = 0.35f + Mathf.PingPong(Time.realtimeSinceStartup * 2.5f, 0.35f);
                accentColor = Color.Lerp(accentColor, Color.white, pulse);
            }

            GUIStyle cardStyle = RuntimePixelGuiUtility.CreateSolidPanelStyle(GUI.skin.box, true);
            GUI.Box(cardRect, GUIContent.none, cardStyle);
            DrawColoredRect(new Rect(cardRect.x, cardRect.y, 6f, cardRect.height), accentColor);
            UpgradeCardLayoutRects card = UpgradeScreenLayout.CardLayout(cardRect);
            DrawUpgradeIcon(card.Icon, GetUpgradeIconPath(upgradeData), accentColor);

            string displayName = GameTextMapper.UpgradeName(upgradeData);
            GUI.Label(card.Title, displayName, CreateLabelStyle(TextAnchor.MiddleLeft, 16f, FontStyle.Bold));
            GUI.Label(card.Level, $"Lv. {level}/{maxLevel}", CreateLabelStyle(TextAnchor.MiddleRight, 13f, FontStyle.Bold));
            GUI.Label(card.Category, ResolveCategoryLabel(upgradeData.EffectKey), CreateColoredLabelStyle(TextAnchor.UpperLeft, 10f, FontStyle.Bold, accentColor));
            DrawLevelProgress(card.LevelProgress, level, maxLevel, accentColor);
            GUI.Label(card.Description, GameTextMapper.UpgradeDescription(upgradeData), CreateLabelStyle(TextAnchor.UpperLeft, 11f, FontStyle.Normal));
            DrawEffectRow(card.CurrentEffect, "\ud604\uc7ac", FormatTotalEffectForDisplay(upgradeData, level), new Color(0.10f, 0.16f, 0.17f, 0.92f), Color.white);
            DrawEffectRow(
                card.NextEffect,
                maxed ? "\uc644\ub8cc" : "\ub2e4\uc74c",
                maxed ? "\ucd5c\uc885 \ub2e8\uacc4 \ub3c4\ub2ec" : FormatTotalEffectForDisplay(upgradeData, level + 1),
                maxed ? new Color(0.17f, 0.15f, 0.08f, 0.94f) : new Color(0.06f, 0.19f, 0.15f, 0.94f),
                maxed ? new Color(1f, 0.83f, 0.35f) : new Color(0.42f, 1f, 0.72f));
            GUI.Label(
                card.Cost,
                maxed ? "\ube44\uc6a9\n-" : $"\ud544\uc694 \uace8\ub4dc\n{cost:N0}",
                CreateColoredLabelStyle(TextAnchor.MiddleLeft, 12f, FontStyle.Bold, maxed || canAfford ? Color.white : new Color(1f, 0.55f, 0.45f)));

            using (new GuiEnabledScope(!maxed && canAfford))
            {
                string buttonLabel = maxed ? "\uac15\ud654 \uc644\ub8cc" : canAfford ? "\uac15\ud654\ud558\uae30" : "\uace8\ub4dc \ubd80\uc871";
                Color previousBackground = GUI.backgroundColor;
                GUI.backgroundColor = maxed ? new Color(0.70f, 0.58f, 0.26f) : canAfford ? accentColor : new Color(0.35f, 0.35f, 0.35f);
                bool clicked = GUI.Button(card.Action, buttonLabel, buttonStyle);
                GUI.backgroundColor = previousBackground;
                if (clicked)
                {
                    int previousGold = SaveManager.Current.totalGold;
                    bool purchased = upgradeManager.TryPurchase(upgradeData);
                    if (purchased)
                    {
                        AudioManager.Play(SfxKey.UpgradePurchase);
                        int newLevel = upgradeManager.GetLevel(upgradeData);
                        int spentGold = Mathf.Max(0, previousGold - SaveManager.Current.totalGold);
                        feedbackMessage = $"{displayName} Lv.{newLevel} \uac15\ud654 \uc644\ub8cc  \u00b7  {FormatTotalEffectForDisplay(upgradeData, newLevel)}  \u00b7  -{spentGold:N0} \uace8\ub4dc";
                        lastPurchasedUpgradeId = upgradeData.UpgradeId;
                        feedbackExpiresAt = Time.realtimeSinceStartup + 4f;
                    }
                    else
                    {
                        feedbackMessage = $"{displayName} \uac15\ud654\uc5d0 \ud544\uc694\ud55c \uace8\ub4dc\uac00 \ubd80\uc871\ud569\ub2c8\ub2e4.";
                        lastPurchasedUpgradeId = string.Empty;
                        feedbackExpiresAt = Time.realtimeSinceStartup + 3f;
                    }

                    GUIUtility.ExitGUI();
                }
            }
        }

        public static string FormatTotalEffectForDisplay(UpgradeData upgradeData, int level)
        {
            if (upgradeData == null)
            {
                return "-";
            }

            float value = upgradeData.ValuePerLevel * Mathf.Max(0, level);
            string valueText;
            if (upgradeData.EffectKey == UpgradeManager.CrystalMaxHpFlat)
            {
                valueText = $"+{value:0.#}";
            }
            else if (upgradeData.EffectKey == UpgradeManager.SkillCooldownPercent)
            {
                valueText = $"-{Mathf.Abs(value) * 100f:0.#}%";
            }
            else
            {
                valueText = $"+{value * 100f:0.#}%";
            }
            return $"{GameTextMapper.UpgradeEffectName(upgradeData.EffectKey)} {valueText}";
        }

        private int GetTotalPurchasedLevels()
        {
            if (upgradeManager == null)
            {
                return 0;
            }

            int total = 0;
            for (int i = 0; i < upgradeManager.AvailableUpgrades.Count; i++)
            {
                UpgradeData upgrade = upgradeManager.AvailableUpgrades[i];
                if (upgrade != null)
                {
                    total += Mathf.Max(0, upgradeManager.GetLevel(upgrade));
                }
            }

            return total;
        }

        private int GetTotalMaximumLevels()
        {
            if (upgradeManager == null)
            {
                return 0;
            }

            int total = 0;
            for (int i = 0; i < upgradeManager.AvailableUpgrades.Count; i++)
            {
                UpgradeData upgrade = upgradeManager.AvailableUpgrades[i];
                if (upgrade != null)
                {
                    total += Mathf.Max(0, upgrade.MaxLevel);
                }
            }

            return total;
        }

        private static void DrawUpgradeIcon(Rect rect, string iconPath, Color accentColor)
        {
            DrawColoredRect(rect, new Color(accentColor.r * 0.28f, accentColor.g * 0.28f, accentColor.b * 0.28f, 0.96f));
            Texture2D texture = RuntimePixelGuiUtility.LoadTexture(iconPath);
            if (texture == null)
            {
                return;
            }

            float inset = Mathf.Clamp(rect.width * 0.12f, 4f, 8f);
            Rect iconRect = new Rect(rect.x + inset, rect.y + inset, rect.width - inset * 2f, rect.height - inset * 2f);
            GUI.DrawTexture(iconRect, texture, ScaleMode.ScaleToFit, true);
        }

        private static void DrawLevelProgress(Rect rect, int level, int maxLevel, Color accentColor)
        {
            int safeMaxLevel = Mathf.Max(1, maxLevel);
            float gap = Mathf.Clamp(rect.width * 0.012f, 3f, 6f);
            float segmentWidth = Mathf.Max(2f, (rect.width - gap * (safeMaxLevel - 1)) / safeMaxLevel);
            for (int i = 0; i < safeMaxLevel; i++)
            {
                Rect segment = new Rect(rect.x + i * (segmentWidth + gap), rect.y, segmentWidth, rect.height);
                DrawColoredRect(segment, i < level ? accentColor : new Color(0.12f, 0.16f, 0.17f, 0.95f));
            }
        }

        private static void DrawEffectRow(Rect rect, string label, string value, Color backgroundColor, Color valueColor)
        {
            DrawColoredRect(rect, backgroundColor);
            float padding = Mathf.Clamp(rect.height * 0.12f, 4f, 8f);
            Rect inner = new Rect(rect.x + padding, rect.y, Mathf.Max(1f, rect.width - padding * 2f), rect.height);
            float labelWidth = Mathf.Clamp(inner.width * 0.23f, 54f, 90f);
            GUI.Label(new Rect(inner.x, inner.y, labelWidth, inner.height), label, CreateColoredLabelStyle(TextAnchor.MiddleLeft, 10f, FontStyle.Bold, new Color(0.72f, 0.78f, 0.78f)));
            GUI.Label(new Rect(inner.x + labelWidth, inner.y, inner.width - labelWidth, inner.height), value, CreateColoredLabelStyle(TextAnchor.MiddleRight, 11f, FontStyle.Bold, valueColor));
        }

        private static Color ResolveAccentColor(string effectKey, bool maxed, bool canAfford)
        {
            if (maxed)
            {
                return new Color(0.96f, 0.72f, 0.22f, 1f);
            }

            if (!canAfford)
            {
                return new Color(0.43f, 0.49f, 0.50f, 1f);
            }

            switch (effectKey)
            {
                case UpgradeManager.CrystalMaxHpFlat:
                    return new Color(0.24f, 0.82f, 0.96f, 1f);
                case UpgradeManager.HeroAttackPercent:
                    return new Color(0.96f, 0.56f, 0.25f, 1f);
                case UpgradeManager.HeroAttackSpeedPercent:
                    return new Color(0.32f, 0.94f, 0.58f, 1f);
                case UpgradeManager.SkillCooldownPercent:
                    return new Color(0.72f, 0.46f, 0.98f, 1f);
                default:
                    return new Color(0.30f, 0.78f, 0.68f, 1f);
            }
        }

        private static string ResolveCategoryLabel(string effectKey)
        {
            switch (effectKey)
            {
                case UpgradeManager.CrystalMaxHpFlat:
                    return "\uc218\ud638 \u00b7 \ud06c\ub9ac\uc2a4\ud0c8";
                case UpgradeManager.HeroAttackPercent:
                    return "\uacf5\uc138 \u00b7 \uc601\uc6c5";
                case UpgradeManager.HeroAttackSpeedPercent:
                    return "\uae30\ub3d9 \u00b7 \uc601\uc6c5";
                case UpgradeManager.SkillCooldownPercent:
                    return "\uc804\uc220 \u00b7 \uc2a4\ud0ac";
                default:
                    return "\ubd09\ubb38 \u00b7 \uac15\ud654";
            }
        }

        private static GUIStyle CreateLabelStyle(TextAnchor alignment, float baseFontSize, FontStyle fontStyle)
        {
            return CreateColoredLabelStyle(alignment, baseFontSize, fontStyle, Color.white);
        }

        private static GUIStyle CreateColoredLabelStyle(TextAnchor alignment, float baseFontSize, FontStyle fontStyle, Color color)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = alignment,
                fontSize = Mathf.Max(10, Mathf.RoundToInt(baseFontSize * UIResponsiveLayout.ReadabilityScale)),
                fontStyle = fontStyle,
                wordWrap = true,
                clipping = TextClipping.Clip
            };
            KoreanFontManager.ApplyFont(style);
            style.normal.textColor = color;
            return style;
        }

        private static void DrawColoredRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previousColor;
        }

        private static string GetUpgradeIconPath(UpgradeData upgradeData)
        {
            if (upgradeData == null)
            {
                return RuntimePixelAssetLoader.UiUpgradeHeroAttack;
            }

            switch (upgradeData.EffectKey)
            {
                case UpgradeManager.CrystalMaxHpFlat:
                    return RuntimePixelAssetLoader.UiUpgradeCrystalHp;
                case UpgradeManager.HeroAttackSpeedPercent:
                    return RuntimePixelAssetLoader.UiUpgradeAttackSpeed;
                case UpgradeManager.SkillCooldownPercent:
                    return RuntimePixelAssetLoader.UiUpgradeSkillCooldown;
                default:
                    return RuntimePixelAssetLoader.UiUpgradeHeroAttack;
            }
        }

        private void LoadSceneOnce(string sceneName)
        {
            if (sceneTransitionRequested)
            {
                return;
            }

            sceneTransitionRequested = true;
            SceneManager.LoadScene(sceneName);
        }

        private readonly struct GuiEnabledScope : System.IDisposable
        {
            private readonly bool previousValue;

            public GuiEnabledScope(bool enabled)
            {
                previousValue = GUI.enabled;
                GUI.enabled = enabled;
            }

            public void Dispose()
            {
                GUI.enabled = previousValue;
            }
        }
    }
}
