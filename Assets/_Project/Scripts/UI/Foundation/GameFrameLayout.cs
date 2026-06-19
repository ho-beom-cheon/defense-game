using UnityEngine;

namespace RuneGate
{
    public readonly struct StageSelectFrameRects
    {
        public StageSelectFrameRects(Rect frameRoot, Rect headerArea, Rect difficultyArea, Rect mainArea, Rect stageListPanel, Rect stageDetailPanel, Rect petContractArea, Rect footerArea)
        {
            FrameRoot = frameRoot;
            HeaderArea = headerArea;
            DifficultyArea = difficultyArea;
            MainArea = mainArea;
            StageListPanel = stageListPanel;
            StageDetailPanel = stageDetailPanel;
            PetContractArea = petContractArea;
            FooterArea = footerArea;
        }

        public Rect FrameRoot { get; }
        public Rect HeaderArea { get; }
        public Rect DifficultyArea { get; }
        public Rect MainArea { get; }
        public Rect StageListPanel { get; }
        public Rect StageDetailPanel { get; }
        public Rect PetContractArea { get; }
        public Rect FooterArea { get; }
    }

    public readonly struct BattleFrameRects
    {
        public BattleFrameRects(Rect frameRoot, Rect headerArea, Rect battlefieldArea, Rect skillPanelArea, Rect footerArea, Rect popupLayer)
        {
            FrameRoot = frameRoot;
            HeaderArea = headerArea;
            BattleFieldFrame = battlefieldArea;
            SkillPanelArea = skillPanelArea;
            FooterArea = footerArea;
            PopupLayer = popupLayer;
        }

        public Rect FrameRoot { get; }
        public Rect HeaderArea { get; }
        public Rect BattleFieldFrame { get; }
        public Rect SkillPanelArea { get; }
        public Rect FooterArea { get; }
        public Rect PopupLayer { get; }
    }

    public readonly struct ScreenFrameRects
    {
        public ScreenFrameRects(Rect frameRoot, Rect headerArea, Rect mainArea, Rect footerArea)
        {
            FrameRoot = frameRoot;
            HeaderArea = headerArea;
            MainArea = mainArea;
            FooterArea = footerArea;
        }

        public Rect FrameRoot { get; }
        public Rect HeaderArea { get; }
        public Rect MainArea { get; }
        public Rect FooterArea { get; }
    }

    public static class GameFrameLayout
    {
        public const float ReferenceWidth = UIResponsiveLayout.ReferenceWidth;
        public const float ReferenceHeight = UIResponsiveLayout.ReferenceHeight;

        public static bool IsPortrait => Screen.height >= Screen.width;
        public static bool UseCompactStageSelect => !IsPortrait || UIResponsiveLayout.SafeRect().height < 900f;
        public static bool UseCompactBattle => !IsPortrait || UIResponsiveLayout.SafeRect().height < 900f;

        public static Rect FrameRoot(float maxWidth = 1040f, float maxHeight = 1800f)
        {
            return UIResponsiveLayout.MainPanel(maxWidth, maxHeight);
        }

        public static ScreenFrameRects TitleFrame(bool expanded)
        {
            Rect safeRect = UIResponsiveLayout.SafeRect();
            return TitleFrameForSafeRect(safeRect, ShouldUseCompact(Screen.width, Screen.height, safeRect), expanded);
        }

        public static ScreenFrameRects TitleFrameForSize(float width, float height, bool expanded)
        {
            Rect safeRect = SafeRectForSize(width, height);
            return TitleFrameForSafeRect(safeRect, ShouldUseCompact(width, height, safeRect), expanded);
        }

        public static ScreenFrameRects UpgradeFrame()
        {
            Rect safeRect = UIResponsiveLayout.SafeRect();
            return UpgradeFrameForSafeRect(safeRect, ShouldUseCompact(Screen.width, Screen.height, safeRect));
        }

        public static ScreenFrameRects UpgradeFrameForSize(float width, float height)
        {
            Rect safeRect = SafeRectForSize(width, height);
            return UpgradeFrameForSafeRect(safeRect, ShouldUseCompact(width, height, safeRect));
        }

        public static StageSelectFrameRects StageSelectFrame()
        {
            return StageSelectFrameForSafeRect(UIResponsiveLayout.SafeRect(), UseCompactStageSelect);
        }

        public static StageSelectFrameRects StageSelectFrameForSize(float width, float height)
        {
            Rect safeRect = SafeRectForSize(width, height);
            return StageSelectFrameForSafeRect(safeRect, ShouldUseCompact(width, height, safeRect));
        }

        public static StageSelectFrameRects StageSelectFrameForSafeRect(Rect safeRect, bool compact)
        {
            bool tightCompact = compact && (safeRect.width >= safeRect.height || safeRect.height < 900f);
            Rect frameRoot = FrameRootForSafeRect(safeRect, tightCompact ? 1120f : compact ? 980f : 1040f, tightCompact ? safeRect.height : compact ? 760f : 1800f);
            float gap = tightCompact ? Mathf.Clamp(SmallGapForSize(safeRect.width, safeRect.height), 6f, 10f) : compact ? SmallGapForSize(safeRect.width, safeRect.height) : GapForSize(safeRect.width, safeRect.height);
            float headerHeight = tightCompact ? Mathf.Clamp(frameRoot.height * 0.10f, 58f, 72f) : compact ? Mathf.Clamp(frameRoot.height * 0.11f, 64f, 84f) : Mathf.Clamp(frameRoot.height * 0.11f, 104f, 150f);
            float difficultyHeight = 0f;
            float petHeight = 0f;
            float footerHeight = tightCompact ? Mathf.Clamp(frameRoot.height * 0.08f, 44f, 58f) : compact ? Mathf.Clamp(frameRoot.height * 0.07f, 46f, 58f) : Mathf.Clamp(frameRoot.height * 0.06f, 54f, 82f);

            Rect headerArea = new Rect(frameRoot.x + gap, frameRoot.y + gap, frameRoot.width - gap * 2f, headerHeight);
            Rect difficultyArea = difficultyHeight > 0f
                ? new Rect(headerArea.x, headerArea.yMax + gap, headerArea.width, difficultyHeight)
                : new Rect(headerArea.x, headerArea.yMax, headerArea.width, 0f);
            Rect footerArea = new Rect(headerArea.x, frameRoot.yMax - footerHeight - gap, headerArea.width, footerHeight);
            Rect petArea = petHeight > 0f
                ? new Rect(headerArea.x, footerArea.y - petHeight - gap, headerArea.width, petHeight)
                : new Rect(headerArea.x, footerArea.y, headerArea.width, 0f);
            float mainBottom = petHeight > 0f ? petArea.y - gap : footerArea.y - gap;
            float mainTop = difficultyHeight > 0f ? difficultyArea.yMax + gap : headerArea.yMax + gap;
            Rect mainArea = new Rect(headerArea.x, mainTop, headerArea.width, Mathf.Max(1f, mainBottom - mainTop));

            float listRatio = tightCompact ? 0.47f : safeRect.height >= safeRect.width ? 0.48f : 0.50f;
            float minimumColumnWidth = tightCompact ? 340f : 300f;
            float listWidth = Mathf.Clamp(mainArea.width * listRatio, minimumColumnWidth, mainArea.width - minimumColumnWidth);
            float detailWidth = Mathf.Max(minimumColumnWidth, mainArea.width - listWidth - gap);
            Rect stageListPanel = new Rect(mainArea.x, mainArea.y, listWidth, mainArea.height);
            Rect stageDetailPanel = new Rect(stageListPanel.xMax + gap, mainArea.y, detailWidth, mainArea.height);

            return new StageSelectFrameRects(frameRoot, headerArea, difficultyArea, mainArea, stageListPanel, stageDetailPanel, petArea, footerArea);
        }

        public static BattleFrameRects BattleFrame()
        {
            return BattleFrameForSafeRect(UIResponsiveLayout.SafeRect(), UseCompactBattle);
        }

        public static BattleFrameRects BattleFrameForSize(float width, float height)
        {
            Rect safeRect = SafeRectForSize(width, height);
            return BattleFrameForSafeRect(safeRect, ShouldUseCompact(width, height, safeRect));
        }

        public static BattleFrameRects BattleFrameForSafeRect(Rect safeRect, bool compact)
        {
            Rect frameRoot = safeRect;
            float gap = compact ? SmallGapForSize(safeRect.width, safeRect.height) : GapForSize(safeRect.width, safeRect.height);
            float headerHeight = compact ? Mathf.Clamp(frameRoot.height * 0.10f, 64f, 86f) : Mathf.Clamp(frameRoot.height * 0.11f, 92f, 150f);
            float skillHeight = compact ? Mathf.Clamp(frameRoot.height * 0.16f, 104f, 148f) : Mathf.Clamp(frameRoot.height * 0.22f, 210f, 340f);
            float footerHeight = compact ? Mathf.Clamp(frameRoot.height * 0.04f, 26f, 42f) : Mathf.Clamp(frameRoot.height * 0.05f, 44f, 72f);
            Rect headerArea = new Rect(frameRoot.x, frameRoot.y, frameRoot.width, headerHeight);
            Rect footerArea = new Rect(frameRoot.x, frameRoot.yMax - footerHeight, frameRoot.width, footerHeight);
            Rect skillPanel = new Rect(frameRoot.x, footerArea.y - skillHeight - gap, frameRoot.width, skillHeight);
            Rect battlefield = new Rect(frameRoot.x, headerArea.yMax + gap, frameRoot.width, Mathf.Max(compact ? 140f : 220f, skillPanel.y - headerArea.yMax - gap * 2f));
            Rect popupLayer = frameRoot;
            return new BattleFrameRects(frameRoot, headerArea, battlefield, skillPanel, footerArea, popupLayer);
        }

        public static Rect PopupFrame(float preferredWidth, float preferredHeight, float widthRatio = 0.92f, float heightRatio = 0.78f)
        {
            if (!IsPortrait || UIResponsiveLayout.SafeRect().height < 900f)
            {
                heightRatio = Mathf.Min(heightRatio, 0.72f);
            }

            return UIResponsiveLayout.Centered(preferredWidth, preferredHeight, widthRatio, heightRatio);
        }

        public static Rect PopupFrameForSize(float screenWidth, float screenHeight, float preferredWidth, float preferredHeight, float widthRatio = 0.92f, float heightRatio = 0.78f)
        {
            Rect safeRect = SafeRectForSize(screenWidth, screenHeight);
            if (ShouldUseCompact(screenWidth, screenHeight, safeRect))
            {
                heightRatio = Mathf.Min(heightRatio, 0.72f);
            }

            float width = Mathf.Min(Mathf.Max(1f, preferredWidth), safeRect.width * Mathf.Clamp01(widthRatio));
            float height = Mathf.Min(Mathf.Max(1f, preferredHeight), safeRect.height * Mathf.Clamp01(heightRatio));
            return ClampToRect(new Rect(safeRect.x + (safeRect.width - width) * 0.5f, safeRect.y + (safeRect.height - height) * 0.5f, width, height), safeRect);
        }

        private static ScreenFrameRects TitleFrameForSafeRect(Rect safeRect, bool compact, bool expanded)
        {
            float maxWidth = compact ? 680f : 720f;
            float maxHeight = compact ? (expanded ? 520f : 390f) : (expanded ? 760f : 560f);
            Rect frameRoot = FrameRootForSafeRect(safeRect, maxWidth, maxHeight);
            float gap = compact ? SmallGapForSize(safeRect.width, safeRect.height) : GapForSize(safeRect.width, safeRect.height);
            float headerHeight = compact ? Mathf.Clamp(frameRoot.height * 0.18f, 58f, 86f) : Mathf.Clamp(frameRoot.height * 0.18f, 82f, 122f);
            float footerHeight = compact ? Mathf.Clamp(frameRoot.height * 0.10f, 34f, 48f) : Mathf.Clamp(frameRoot.height * 0.10f, 46f, 64f);
            Rect header = new Rect(frameRoot.x + gap, frameRoot.y + gap, frameRoot.width - gap * 2f, headerHeight);
            Rect footer = new Rect(header.x, frameRoot.yMax - footerHeight - gap, header.width, footerHeight);
            Rect main = new Rect(header.x, header.yMax + gap, header.width, Mathf.Max(120f, footer.y - header.yMax - gap));
            return new ScreenFrameRects(frameRoot, header, main, footer);
        }

        private static ScreenFrameRects UpgradeFrameForSafeRect(Rect safeRect, bool compact)
        {
            Rect frameRoot = FrameRootForSafeRect(safeRect, compact ? 980f : 920f, compact ? 720f : 1700f);
            float gap = compact ? SmallGapForSize(safeRect.width, safeRect.height) : GapForSize(safeRect.width, safeRect.height);
            float headerHeight = compact ? Mathf.Clamp(frameRoot.height * 0.13f, 70f, 96f) : Mathf.Clamp(frameRoot.height * 0.12f, 110f, 160f);
            float footerHeight = compact ? Mathf.Clamp(frameRoot.height * 0.08f, 44f, 58f) : Mathf.Clamp(frameRoot.height * 0.06f, 54f, 78f);
            Rect header = new Rect(frameRoot.x + gap, frameRoot.y + gap, frameRoot.width - gap * 2f, headerHeight);
            Rect footer = new Rect(header.x, frameRoot.yMax - footerHeight - gap, header.width, footerHeight);
            Rect main = new Rect(header.x, header.yMax + gap, header.width, Mathf.Max(160f, footer.y - header.yMax - gap));
            return new ScreenFrameRects(frameRoot, header, main, footer);
        }

        public static Rect SafeRectForSize(float screenWidth, float screenHeight)
        {
            float margin = MarginForSize(screenWidth, screenHeight);
            return new Rect(margin, margin, Mathf.Max(1f, screenWidth - margin * 2f), Mathf.Max(1f, screenHeight - margin * 2f));
        }

        private static bool ShouldUseCompact(float width, float height, Rect safeRect)
        {
            return height < width || safeRect.height < 900f;
        }

        private static Rect FrameRootForSafeRect(Rect safeRect, float maxWidth, float maxHeight)
        {
            float width = Mathf.Min(safeRect.width, maxWidth);
            float height = Mathf.Min(safeRect.height, maxHeight);
            return ClampToRect(new Rect(safeRect.x + (safeRect.width - width) * 0.5f, safeRect.y + (safeRect.height - height) * 0.5f, width, height), safeRect);
        }

        private static Rect ClampToRect(Rect rect, Rect bounds)
        {
            float width = Mathf.Min(rect.width, Mathf.Max(1f, bounds.width));
            float height = Mathf.Min(rect.height, Mathf.Max(1f, bounds.height));
            float x = Mathf.Clamp(rect.x, bounds.x, Mathf.Max(bounds.x, bounds.xMax - width));
            float y = Mathf.Clamp(rect.y, bounds.y, Mathf.Max(bounds.y, bounds.yMax - height));
            return new Rect(x, y, width, height);
        }

        private static float MarginForSize(float width, float height)
        {
            return Mathf.Clamp(Mathf.Min(width, height) * 0.025f, 8f, 28f);
        }

        private static float SmallGapForSize(float width, float height)
        {
            return Mathf.Clamp(MarginForSize(width, height) * 0.5f, 4f, 12f);
        }

        private static float GapForSize(float width, float height)
        {
            return Mathf.Clamp(MarginForSize(width, height) * 0.75f, 8f, 18f);
        }
    }
}
