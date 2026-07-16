using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class RuneSelectionUI : MonoBehaviour
    {
        private const float PanelPadding = 14f;
        private const float CardGap = 12f;

        [SerializeField] private BattleManager battleManager;
        [SerializeField] private RuneManager runeManager;
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private bool showDebugEffectKey;
        [SerializeField] private Rect panelRect = new Rect(380f, 78f, 520f, 360f);

        private readonly List<RuneData> displayedRunes = new List<RuneData>();
        private bool isVisible;
        private bool selectionRequested;
        private int requestedIndex = -1;

        public event Action<bool> VisibilityChanged;
        public event Action<IReadOnlyList<RuneData>> OptionsChanged;

        public IReadOnlyList<RuneData> DisplayedRunes => displayedRunes;
        public bool IsVisible => isVisible;

        public void SetRuntimeGuiEnabled(bool enabled)
        {
            drawRuntimeGui = enabled;
        }

        private void OnEnable()
        {
            AutoAssignReferences();

            if (battleManager != null)
            {
                battleManager.RuneOptionsOffered += ShowOptions;
                battleManager.BattleStateChanged += HandleBattleStateChanged;
            }
        }

        private void OnDisable()
        {
            if (battleManager != null)
            {
                battleManager.RuneOptionsOffered -= ShowOptions;
                battleManager.BattleStateChanged -= HandleBattleStateChanged;
            }
        }

        private void OnGUI()
        {
            if (!drawRuntimeGui || !isVisible)
            {
                return;
            }

            UIResponsiveLayout.ApplyReadableDefaults();
            UIPopupGuiUtility.DrawDimOverlay(0.45f);

            Rect drawRect = CenteredPanelRect();
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUIStyle cardStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiRuneCardBase);
            GUI.SetNextControlName("PopupLayer_RuneSelectionPopup");
            GUI.Box(drawRect, GUIContent.none, panelStyle);
            float titleHeight = Mathf.Clamp(drawRect.height * 0.08f, 34f, 54f);
            float subtitleHeight = Mathf.Clamp(drawRect.height * 0.06f, 28f, 42f);
            GUI.Label(new Rect(drawRect.x + PanelPadding, drawRect.y + 8f, drawRect.width - PanelPadding * 2f, titleHeight),
                "전술 기록서 · 룬 선택", CreateLabelStyle(TextAnchor.MiddleCenter, true, 18f, false));
            GUIStyle subtitleStyle = CreateLabelStyle(TextAnchor.MiddleCenter, false, 11f, true);
            subtitleStyle.normal.textColor = new Color(0.72f, 0.82f, 0.82f, 1f);
            GUI.Label(new Rect(drawRect.x + PanelPadding, drawRect.y + 8f + titleHeight, drawRect.width - PanelPadding * 2f, subtitleHeight),
                "다음 웨이브를 버틸 기록 하나를 선택하세요.", subtitleStyle);

            float contentTop = drawRect.y + 12f + titleHeight + subtitleHeight;
            Rect contentRect = new Rect(
                drawRect.x + PanelPadding,
                contentTop,
                drawRect.width - PanelPadding * 2f,
                Mathf.Max(1f, drawRect.yMax - contentTop - PanelPadding));
            for (int i = 0; i < displayedRunes.Count; i++)
            {
                RuneData runeData = displayedRunes[i];
                if (runeData == null)
                {
                    continue;
                }

                DrawRuneCard(runeData, i, CalculateCardRect(contentRect, i, displayedRunes.Count), cardStyle);
            }
        }

        public static int ResolveColumnCount(float availableWidth, int itemCount)
        {
            int preferredColumns = availableWidth >= 540f ? 3 : availableWidth >= 360f ? 2 : 1;
            return Mathf.Clamp(preferredColumns, 1, Mathf.Max(1, itemCount));
        }

        public static Rect CalculateCardRect(Rect contentRect, int index, int itemCount)
        {
            int safeCount = Mathf.Max(1, itemCount);
            int columns = ResolveColumnCount(contentRect.width, safeCount);
            int rows = Mathf.Max(1, Mathf.CeilToInt(safeCount / (float)columns));
            int safeIndex = Mathf.Clamp(index, 0, safeCount - 1);
            float width = Mathf.Max(1f, (contentRect.width - CardGap * (columns - 1)) / columns);
            float height = Mathf.Max(1f, (contentRect.height - CardGap * (rows - 1)) / rows);
            int column = safeIndex % columns;
            int row = safeIndex / columns;
            return new Rect(
                contentRect.x + column * (width + CardGap),
                contentRect.y + row * (height + CardGap),
                width,
                height);
        }

        public static Color RarityColor(RuneRarity rarity)
        {
            switch (rarity)
            {
                case RuneRarity.Rare:
                    return new Color(0.26f, 0.68f, 1f, 1f);
                case RuneRarity.Epic:
                    return new Color(0.78f, 0.42f, 1f, 1f);
                default:
                    return new Color(0.72f, 0.78f, 0.76f, 1f);
            }
        }

        public static Color ElementColor(ElementType element)
        {
            switch (element)
            {
                case ElementType.Fire:
                    return new Color(0.95f, 0.28f, 0.18f, 1f);
                case ElementType.Ice:
                    return new Color(0.32f, 0.76f, 1f, 1f);
                case ElementType.Lightning:
                    return new Color(0.96f, 0.82f, 0.22f, 1f);
                case ElementType.Earth:
                    return new Color(0.56f, 0.72f, 0.30f, 1f);
                case ElementType.Light:
                    return new Color(1f, 0.86f, 0.42f, 1f);
                case ElementType.Dark:
                    return new Color(0.62f, 0.34f, 0.82f, 1f);
                case ElementType.Poison:
                    return new Color(0.42f, 0.84f, 0.30f, 1f);
                case ElementType.Wind:
                    return new Color(0.30f, 0.86f, 0.68f, 1f);
                default:
                    return new Color(0.60f, 0.70f, 0.72f, 1f);
            }
        }

        public static string ElementGlyph(ElementType element)
        {
            switch (element)
            {
                case ElementType.Fire:
                    return "화";
                case ElementType.Ice:
                    return "빙";
                case ElementType.Lightning:
                    return "뢰";
                case ElementType.Earth:
                    return "지";
                case ElementType.Light:
                    return "광";
                case ElementType.Dark:
                    return "영";
                case ElementType.Poison:
                    return "독";
                case ElementType.Wind:
                    return "풍";
                default:
                    return "무";
            }
        }

        private void DrawRuneCard(RuneData runeData, int index, Rect cardRect, GUIStyle cardStyle)
        {
            bool isRequested = selectionRequested && requestedIndex == index;
            Color rarityColor = RarityColor(runeData.Rarity);
            Color elementColor = ElementColor(runeData.Element);
            Color previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = isRequested ? Color.Lerp(rarityColor, Color.white, 0.22f) : rarityColor;
            GUI.Box(cardRect, GUIContent.none, cardStyle);
            GUI.backgroundColor = previousBackground;

            if (selectionRequested && !isRequested)
            {
                DrawSolidRect(new Rect(cardRect.x + 3f, cardRect.y + 3f, cardRect.width - 6f, cardRect.height - 6f), new Color(0f, 0f, 0f, 0.48f));
            }

            DrawSolidRect(new Rect(cardRect.x + 4f, cardRect.y + 4f, cardRect.width - 8f, 7f), rarityColor);
            if (isRequested)
            {
                DrawBorder(cardRect, 4f, new Color(1f, 0.88f, 0.46f, 1f));
            }

            float padding = Mathf.Clamp(cardRect.width * 0.055f, 8f, 14f);
            float titleHeight = Mathf.Clamp(cardRect.height * 0.09f, 34f, 52f);
            float metaHeight = Mathf.Clamp(cardRect.height * 0.06f, 24f, 34f);
            Rect titleRect = new Rect(cardRect.x + padding, cardRect.y + 14f, cardRect.width - padding * 2f, titleHeight);
            GUI.Label(titleRect, runeData.DisplayName, CreateLabelStyle(TextAnchor.MiddleCenter, true, 16f, false));

            GUIStyle metaStyle = CreateLabelStyle(TextAnchor.MiddleCenter, true, 10f, false);
            metaStyle.normal.textColor = rarityColor;
            GUI.Label(new Rect(titleRect.x, titleRect.yMax, titleRect.width, metaHeight),
                $"{GameTextMapper.RuneRarityName(runeData.Rarity)} · {ElementGlyph(runeData.Element)} 속성", metaStyle);

            float sealSize = Mathf.Clamp(Mathf.Min(cardRect.width * 0.42f, cardRect.height * 0.18f), 54f, 104f);
            Rect sealRect = new Rect(cardRect.center.x - sealSize * 0.5f, titleRect.yMax + metaHeight + 5f, sealSize, sealSize);
            DrawRuneSeal(sealRect, runeData, elementColor);

            float buttonHeight = Mathf.Clamp(cardRect.height * 0.11f, 48f, 64f);
            float effectHeight = Mathf.Clamp(cardRect.height * 0.075f, 34f, 48f);
            Rect buttonRect = new Rect(cardRect.x + padding, cardRect.yMax - buttonHeight - padding, cardRect.width - padding * 2f, buttonHeight);
            Rect effectRect = new Rect(cardRect.x + padding, buttonRect.y - effectHeight - 7f, cardRect.width - padding * 2f, effectHeight);
            DrawSolidRect(effectRect, new Color(elementColor.r * 0.32f, elementColor.g * 0.32f, elementColor.b * 0.32f, 0.94f));
            GUI.Label(effectRect, $"효과 {FormatRuneValue(runeData)}", CreateLabelStyle(TextAnchor.MiddleCenter, true, 12f, false));

            Rect descriptionRect = new Rect(
                cardRect.x + padding,
                sealRect.yMax + 8f,
                cardRect.width - padding * 2f,
                Mathf.Max(1f, effectRect.y - sealRect.yMax - 14f));
            GUIStyle descriptionStyle = CreateLabelStyle(TextAnchor.UpperLeft, false, 11f, true);
            descriptionStyle.normal.textColor = new Color(0.88f, 0.91f, 0.90f, 1f);
            string description = showDebugEffectKey
                ? $"{runeData.Description}\n\nDEBUG {runeData.EffectKey}: {runeData.Value:0.##}"
                : runeData.Description;
            GUI.Label(descriptionRect, description, descriptionStyle);

            bool previousEnabled = GUI.enabled;
            GUI.enabled = !selectionRequested;
            GUIStyle buttonStyle = RuntimePixelGuiUtility.CreateButtonStyle(GUI.skin.button, RuntimePixelAssetLoader.UiButtonSkill);
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.fontSize = Mathf.RoundToInt(13f * UIResponsiveLayout.ReadabilityScale);
            KoreanFontManager.ApplyFont(buttonStyle);
            string buttonText = isRequested ? "적용 중" : selectionRequested ? "선택 잠금" : "선택";
            if (GUI.Button(buttonRect, buttonText, buttonStyle))
            {
                SelectOption(index);
            }

            GUI.enabled = previousEnabled;
        }

        private static void DrawRuneSeal(Rect rect, RuneData runeData, Color elementColor)
        {
            DrawSolidRect(rect, new Color(0.015f, 0.035f, 0.04f, 0.98f));
            DrawBorder(rect, Mathf.Clamp(rect.width * 0.06f, 3f, 6f), elementColor);
            Rect inner = new Rect(rect.x + rect.width * 0.18f, rect.y + rect.height * 0.18f, rect.width * 0.64f, rect.height * 0.64f);
            DrawBorder(inner, Mathf.Clamp(rect.width * 0.035f, 2f, 4f), new Color(elementColor.r, elementColor.g, elementColor.b, 0.72f));
            if (runeData.Icon != null && runeData.Icon.texture != null)
            {
                DrawSprite(new Rect(rect.x + 8f, rect.y + 8f, rect.width - 16f, rect.height - 16f), runeData.Icon);
                return;
            }

            GUIStyle glyphStyle = CreateLabelStyle(TextAnchor.MiddleCenter, true, 24f, false);
            glyphStyle.normal.textColor = elementColor;
            GUI.Label(rect, ElementGlyph(runeData.Element), glyphStyle);
        }

        private static void DrawSprite(Rect rect, Sprite sprite)
        {
            Rect textureRect = sprite.textureRect;
            Rect uv = new Rect(
                textureRect.x / sprite.texture.width,
                textureRect.y / sprite.texture.height,
                textureRect.width / sprite.texture.width,
                textureRect.height / sprite.texture.height);
            GUI.DrawTextureWithTexCoords(rect, sprite.texture, uv, true);
        }

        private static void DrawBorder(Rect rect, float thickness, Color color)
        {
            DrawSolidRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            DrawSolidRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            DrawSolidRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            DrawSolidRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        private static void DrawSolidRect(Rect rect, Color color)
        {
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
            GUI.color = previousColor;
        }

        private static GUIStyle CreateLabelStyle(TextAnchor alignment, bool bold, float baseFontSize, bool wordWrap)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = alignment,
                fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
                fontSize = Mathf.RoundToInt(baseFontSize * UIResponsiveLayout.ReadabilityScale),
                clipping = TextClipping.Clip,
                wordWrap = wordWrap,
                padding = new RectOffset(0, 0, 0, 0)
            };
            KoreanFontManager.ApplyFont(style);
            return style;
        }

        public void SelectOption(int index)
        {
            if (runeManager == null)
            {
                Debug.LogWarning("RuneSelectionUI cannot select a rune because RuneManager is missing.");
                return;
            }

            if (selectionRequested)
            {
                return;
            }

            selectionRequested = true;
            requestedIndex = index;
            runeManager.SelectRuneAt(index);
        }

        private void ShowOptions(IReadOnlyList<RuneData> options)
        {
            displayedRunes.Clear();
            selectionRequested = false;
            requestedIndex = -1;
            for (int i = 0; i < options.Count; i++)
            {
                displayedRunes.Add(options[i]);
            }

            isVisible = displayedRunes.Count > 0;
            OptionsChanged?.Invoke(displayedRunes);
            VisibilityChanged?.Invoke(isVisible);
        }

        private void HandleBattleStateChanged(BattleState battleState)
        {
            if (battleState != BattleState.RuneSelection)
            {
                Hide();
            }
        }

        private void Hide()
        {
            bool changed = isVisible || displayedRunes.Count > 0;
            isVisible = false;
            selectionRequested = false;
            requestedIndex = -1;
            displayedRunes.Clear();
            if (changed)
            {
                OptionsChanged?.Invoke(displayedRunes);
                VisibilityChanged?.Invoke(false);
            }
        }

        private void AutoAssignReferences()
        {
            if (battleManager == null)
            {
                battleManager = FindAnyObjectByType<BattleManager>();
            }

            if (runeManager == null)
            {
                runeManager = FindAnyObjectByType<RuneManager>();
            }
        }

        private Rect CenteredPanelRect()
        {
            bool mobilePortrait = Application.isMobilePlatform && GameFrameLayout.IsPortrait;
            float preferredWidth = mobilePortrait ? 760f : 620f;
            float preferredHeight = mobilePortrait ? 760f : 520f;
            return GameFrameLayout.PopupFrame(Mathf.Max(panelRect.width, preferredWidth), Mathf.Max(panelRect.height, preferredHeight), 0.92f, 0.78f);
        }

        private static string FormatRuneValue(RuneData runeData)
        {
            if (runeData == null)
            {
                return "-";
            }

            float value = runeData.Value;
            return Mathf.Abs(value) < 1f ? $"{value * 100f:0.#}%" : $"+{value:0.#}";
        }

    }
}
