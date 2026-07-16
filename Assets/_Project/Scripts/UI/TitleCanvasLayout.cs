using UnityEngine;

namespace RuneGate
{
    public enum TitleViewMode
    {
        Main = 0,
        Settings = 1
    }

    public enum TitleConfirmAction
    {
        None = 0,
        NewGame = 1,
        ResetSave = 2
    }

    public readonly struct TitleViewData
    {
        public TitleViewData(bool hasExistingProgress, string progressSummary, string primaryActionLabel, string feedbackMessage)
        {
            HasExistingProgress = hasExistingProgress;
            ProgressSummary = progressSummary ?? string.Empty;
            PrimaryActionLabel = primaryActionLabel ?? string.Empty;
            FeedbackMessage = feedbackMessage ?? string.Empty;
        }

        public bool HasExistingProgress { get; }
        public string ProgressSummary { get; }
        public string PrimaryActionLabel { get; }
        public string FeedbackMessage { get; }
    }

    public readonly struct TitleCanvasRects
    {
        public TitleCanvasRects(
            Rect safeArea,
            Rect frameRoot,
            Rect brandArea,
            Rect menuPanel,
            Rect headerArea,
            Rect bodyArea,
            Rect actionArea,
            Rect statusFooter,
            Rect modalPanel,
            bool footerVisible,
            bool bodyScrollRequired)
        {
            SafeArea = safeArea;
            FrameRoot = frameRoot;
            BrandArea = brandArea;
            MenuPanel = menuPanel;
            HeaderArea = headerArea;
            BodyArea = bodyArea;
            ActionArea = actionArea;
            StatusFooter = statusFooter;
            ModalPanel = modalPanel;
            FooterVisible = footerVisible;
            BodyScrollRequired = bodyScrollRequired;
        }

        public Rect SafeArea { get; }
        public Rect FrameRoot { get; }
        public Rect BrandArea { get; }
        public Rect MenuPanel { get; }
        public Rect HeaderArea { get; }
        public Rect BodyArea { get; }
        public Rect ActionArea { get; }
        public Rect StatusFooter { get; }
        public Rect ModalPanel { get; }
        public bool FooterVisible { get; }
        public bool BodyScrollRequired { get; }
    }

    public static class TitleCanvasLayout
    {
        public const float MaximumPanelWidth = 920f;
        public const float MaximumModalWidth = 760f;
        public const float EdgeMargin = 40f;
        public const float CompactEdgeMargin = 24f;
        public const float ContentInset = 48f;
        public const float HeaderHeight = 72f;
        public const float FooterHeight = 48f;
        public const float PanelFooterGap = 16f;
        public const float BrandPanelGap = 16f;
        public const float LandscapeThreshold = 1.15f;

        public static TitleCanvasRects Calculate(Vector2 safeSize, float canvasScaleFactor, TitleViewMode mode)
        {
            float width = Mathf.Max(1f, safeSize.x);
            float height = Mathf.Max(1f, safeSize.y);
            Rect safe = new Rect(0f, 0f, width, height);
            float edge = width >= 800f ? EdgeMargin : CompactEdgeMargin;
            float effectiveTouch = Mathf.Max(
                UiFrameTokens.MinimumTouchHeight,
                UiFrameTokens.MinimumScreenTouchPixels / Mathf.Max(0.01f, canvasScaleFactor));

            return width / height >= LandscapeThreshold
                ? CalculateLandscape(safe, edge, effectiveTouch, mode)
                : CalculatePortrait(safe, edge, effectiveTouch, mode);
        }

        private static TitleCanvasRects CalculatePortrait(Rect safe, float edge, float touch, TitleViewMode mode)
        {
            Rect frame = Inset(safe, edge, edge);
            float idealPanel = mode == TitleViewMode.Settings ? 880f : 640f;
            float minimumPanel = mode == TitleViewMode.Settings ? 640f : 560f;
            float idealBrand = 340f;
            float minimumBrand = 160f;
            float requiredWithFooter = minimumPanel + minimumBrand + FooterHeight + PanelFooterGap + BrandPanelGap;
            bool footerVisible = frame.height >= requiredWithFooter;
            float reservedFooter = footerVisible ? FooterHeight + PanelFooterGap : 0f;
            float availableForPanelAndBrand = Mathf.Max(1f, frame.height - reservedFooter - BrandPanelGap);
            float panelHeight = Mathf.Min(idealPanel, availableForPanelAndBrand - minimumBrand);
            panelHeight = Mathf.Clamp(panelHeight, Mathf.Min(minimumPanel, availableForPanelAndBrand), idealPanel);
            float brandHeight = Mathf.Clamp(availableForPanelAndBrand - panelHeight, 0f, idealBrand);
            if (brandHeight < minimumBrand && availableForPanelAndBrand >= minimumPanel + minimumBrand)
            {
                brandHeight = minimumBrand;
                panelHeight = availableForPanelAndBrand - brandHeight;
            }

            float panelWidth = Mathf.Min(MaximumPanelWidth, frame.width);
            float footerY = frame.y;
            Rect footer = footerVisible
                ? new Rect(frame.center.x - panelWidth * 0.5f, footerY, panelWidth, FooterHeight)
                : new Rect(frame.center.x, footerY, 0f, 0f);
            float panelY = footerVisible ? footer.yMax + PanelFooterGap : frame.y;
            Rect panel = new Rect(frame.center.x - panelWidth * 0.5f, panelY, panelWidth, panelHeight);
            Rect brand = new Rect(frame.x, panel.yMax + BrandPanelGap, frame.width, Mathf.Max(1f, frame.yMax - panel.yMax - BrandPanelGap));
            bool scroll = mode == TitleViewMode.Settings && panelHeight < idealPanel - 0.1f;
            return BuildResult(safe, frame, brand, panel, footer, touch, mode, scroll);
        }

        private static TitleCanvasRects CalculateLandscape(Rect safe, float edge, float touch, TitleViewMode mode)
        {
            Rect available = Inset(safe, edge, edge);
            float frameWidth = Mathf.Min(1520f, available.width);
            float frameHeight = Mathf.Min(820f, available.height);
            Rect frame = new Rect(
                available.center.x - frameWidth * 0.5f,
                available.center.y - frameHeight * 0.5f,
                frameWidth,
                frameHeight);
            const float columnGap = 32f;
            float brandWidth = Mathf.Max(1f, (frame.width - columnGap) * 0.44f);
            Rect brand = new Rect(frame.x, frame.y, brandWidth, frame.height);
            Rect menuColumn = new Rect(brand.xMax + columnGap, frame.y, Mathf.Max(1f, frame.xMax - brand.xMax - columnGap), frame.height);
            float idealPanel = mode == TitleViewMode.Settings ? 720f : 640f;
            bool footerVisible = menuColumn.height >= Mathf.Min(idealPanel, menuColumn.height) + FooterHeight + PanelFooterGap;
            float reservedFooter = footerVisible ? FooterHeight + PanelFooterGap : 0f;
            float panelHeight = Mathf.Min(idealPanel, menuColumn.height - reservedFooter);
            float panelWidth = Mathf.Min(720f, menuColumn.width);
            Rect footer = footerVisible
                ? new Rect(menuColumn.center.x - panelWidth * 0.5f, menuColumn.y, panelWidth, FooterHeight)
                : new Rect(menuColumn.center.x, menuColumn.y, 0f, 0f);
            float panelY = footerVisible ? footer.yMax + PanelFooterGap : menuColumn.center.y - panelHeight * 0.5f;
            Rect panel = new Rect(menuColumn.center.x - panelWidth * 0.5f, panelY, panelWidth, panelHeight);
            bool scroll = mode == TitleViewMode.Settings && panelHeight < idealPanel - 0.1f;
            return BuildResult(safe, frame, brand, panel, footer, touch, mode, scroll);
        }

        private static TitleCanvasRects BuildResult(
            Rect safe,
            Rect frame,
            Rect brand,
            Rect panel,
            Rect footer,
            float touch,
            TitleViewMode mode,
            bool scroll)
        {
            float inset = Mathf.Min(ContentInset, Mathf.Max(24f, panel.width * 0.08f));
            Rect inner = Inset(panel, inset, inset);
            float actionHeight = mode == TitleViewMode.Main
                ? Mathf.Max(224f, touch * 2f + UiFrameTokens.Space16)
                : Mathf.Max(104f, touch);
            actionHeight = Mathf.Min(actionHeight, Mathf.Max(touch, inner.height - HeaderHeight - touch));
            Rect header = new Rect(inner.x, inner.yMax - HeaderHeight, inner.width, HeaderHeight);
            Rect action = new Rect(inner.x, inner.y, inner.width, actionHeight);
            Rect body = new Rect(inner.x, action.yMax, inner.width, Mathf.Max(1f, header.y - action.yMax));
            float modalWidth = Mathf.Min(MaximumModalWidth, Mathf.Max(320f, safe.width - edgeForModal(safe.width) * 2f));
            float modalHeight = Mathf.Min(430f, Mathf.Max(300f, safe.height - edgeForModal(safe.width) * 2f));
            Rect modal = new Rect(safe.center.x - modalWidth * 0.5f, safe.center.y - modalHeight * 0.5f, modalWidth, modalHeight);
            return new TitleCanvasRects(safe, frame, brand, panel, header, body, action, footer, modal, footer.width > 0f, scroll);
        }

        private static float edgeForModal(float width)
        {
            return width >= 800f ? EdgeMargin : CompactEdgeMargin;
        }

        private static Rect Inset(Rect rect, float horizontal, float vertical)
        {
            return new Rect(
                rect.x + horizontal,
                rect.y + vertical,
                Mathf.Max(1f, rect.width - horizontal * 2f),
                Mathf.Max(1f, rect.height - vertical * 2f));
        }
    }
}
