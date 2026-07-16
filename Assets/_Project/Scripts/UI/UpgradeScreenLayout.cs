using UnityEngine;

namespace RuneGate
{
    public readonly struct UpgradeScreenLayoutRects
    {
        private readonly int itemCount;

        public UpgradeScreenLayoutRects(
            Rect headerTitle,
            Rect headerSummary,
            Rect goldWallet,
            Rect viewport,
            Rect content,
            Rect footerFeedback,
            Rect footerButton,
            int columns,
            float gap,
            float cardHeight,
            float firstRowY,
            int itemCount)
        {
            HeaderTitle = headerTitle;
            HeaderSummary = headerSummary;
            GoldWallet = goldWallet;
            Viewport = viewport;
            Content = content;
            FooterFeedback = footerFeedback;
            FooterButton = footerButton;
            Columns = columns;
            Gap = gap;
            CardHeight = cardHeight;
            FirstRowY = firstRowY;
            this.itemCount = itemCount;
        }

        public Rect HeaderTitle { get; }
        public Rect HeaderSummary { get; }
        public Rect GoldWallet { get; }
        public Rect Viewport { get; }
        public Rect Content { get; }
        public Rect FooterFeedback { get; }
        public Rect FooterButton { get; }
        public int Columns { get; }
        public float Gap { get; }
        public float CardHeight { get; }
        public float FirstRowY { get; }
        public int ItemCount => itemCount;

        public Rect CardRect(int index)
        {
            if (index < 0 || index >= itemCount)
            {
                return Rect.zero;
            }

            int column = index % Columns;
            int row = index / Columns;
            float cardWidth = (Content.width - Gap * (Columns - 1)) / Columns;
            return new Rect(
                column * (cardWidth + Gap),
                FirstRowY + row * (CardHeight + Gap),
                cardWidth,
                CardHeight);
        }
    }

    public readonly struct UpgradeCardLayoutRects
    {
        public UpgradeCardLayoutRects(
            Rect icon,
            Rect title,
            Rect category,
            Rect level,
            Rect levelProgress,
            Rect description,
            Rect currentEffect,
            Rect nextEffect,
            Rect cost,
            Rect action)
        {
            Icon = icon;
            Title = title;
            Category = category;
            Level = level;
            LevelProgress = levelProgress;
            Description = description;
            CurrentEffect = currentEffect;
            NextEffect = nextEffect;
            Cost = cost;
            Action = action;
        }

        public Rect Icon { get; }
        public Rect Title { get; }
        public Rect Category { get; }
        public Rect Level { get; }
        public Rect LevelProgress { get; }
        public Rect Description { get; }
        public Rect CurrentEffect { get; }
        public Rect NextEffect { get; }
        public Rect Cost { get; }
        public Rect Action { get; }
    }

    public static class UpgradeScreenLayout
    {
        public const int ExpectedUpgradeCount = 4;

        public static UpgradeScreenLayoutRects Calculate(ScreenFrameRects frame, float screenWidth, float screenHeight, int itemCount)
        {
            int safeItemCount = Mathf.Max(0, itemCount);
            bool portrait = screenHeight >= screenWidth;
            bool useTwoColumns = portrait && frame.MainArea.width >= 820f && frame.MainArea.height >= 760f;
            int columns = useTwoColumns ? 2 : 1;
            float gap = Mathf.Clamp(Mathf.Min(frame.MainArea.width, frame.MainArea.height) * 0.018f, 8f, 18f);
            float panelInset = Mathf.Clamp(gap * 0.6f, 6f, 10f);
            Rect viewport = Inset(frame.MainArea, panelInset);

            float cardHeight;
            if (columns == 2)
            {
                cardHeight = Mathf.Clamp((viewport.height - gap) * 0.5f, 360f, 560f);
            }
            else
            {
                float heightByWidth = viewport.width * (portrait ? 0.48f : 0.34f);
                cardHeight = Mathf.Clamp(heightByWidth, portrait ? 290f : 220f, portrait ? 350f : 280f);
            }

            int rows = safeItemCount == 0 ? 0 : Mathf.CeilToInt(safeItemCount / (float)columns);
            float cardsHeight = rows > 0 ? rows * cardHeight + (rows - 1) * gap : 0f;
            float contentHeight = Mathf.Max(viewport.height, cardsHeight);
            float scrollbarReserve = cardsHeight > viewport.height + 0.5f ? 18f : 0f;
            Rect content = new Rect(0f, 0f, Mathf.Max(1f, viewport.width - scrollbarReserve), contentHeight);
            float firstRowY = Mathf.Max(0f, (contentHeight - cardsHeight) * 0.5f);

            float headerInset = Mathf.Clamp(frame.HeaderArea.height * 0.12f, 8f, 16f);
            Rect headerInner = Inset(frame.HeaderArea, headerInset);
            float walletWidth = Mathf.Clamp(headerInner.width * 0.31f, 180f, 300f);
            Rect goldWallet = new Rect(headerInner.xMax - walletWidth, headerInner.y, walletWidth, headerInner.height);
            Rect headerText = new Rect(headerInner.x, headerInner.y, Mathf.Max(1f, goldWallet.x - headerInner.x - gap), headerInner.height);
            Rect headerTitle = new Rect(headerText.x, headerText.y, headerText.width, headerText.height * 0.55f);
            Rect headerSummary = new Rect(headerText.x, headerTitle.yMax, headerText.width, headerText.height - headerTitle.height);

            float footerInset = Mathf.Clamp(frame.FooterArea.height * 0.12f, 6f, 12f);
            Rect footerInner = Inset(frame.FooterArea, footerInset);
            float buttonWidth = Mathf.Clamp(footerInner.width * 0.34f, 220f, 340f);
            Rect footerButton = new Rect(footerInner.xMax - buttonWidth, footerInner.y, buttonWidth, footerInner.height);
            Rect footerFeedback = new Rect(footerInner.x, footerInner.y, Mathf.Max(1f, footerButton.x - footerInner.x - gap), footerInner.height);

            return new UpgradeScreenLayoutRects(
                headerTitle,
                headerSummary,
                goldWallet,
                viewport,
                content,
                footerFeedback,
                footerButton,
                columns,
                gap,
                cardHeight,
                firstRowY,
                safeItemCount);
        }

        public static UpgradeScreenLayoutRects CalculateForSize(float screenWidth, float screenHeight, int itemCount = ExpectedUpgradeCount)
        {
            ScreenFrameRects frame = GameFrameLayout.UpgradeFrameForSize(screenWidth, screenHeight);
            return Calculate(frame, screenWidth, screenHeight, itemCount);
        }

        public static UpgradeCardLayoutRects CardLayout(Rect card)
        {
            bool compact = card.height < 390f;
            float padding = Mathf.Clamp(card.width * 0.035f, 12f, 18f);
            Rect inner = Inset(card, padding);
            float iconSize = Mathf.Clamp(inner.width * (compact ? 0.11f : 0.15f), compact ? 46f : 58f, compact ? 64f : 74f);
            float headerHeight = Mathf.Clamp(iconSize, compact ? 54f : 66f, compact ? 68f : 82f);
            Rect icon = new Rect(inner.x, inner.y, iconSize, iconSize);
            Rect headerText = new Rect(icon.xMax + padding * 0.7f, inner.y, Mathf.Max(1f, inner.xMax - icon.xMax - padding * 0.7f), headerHeight);
            Rect title = new Rect(headerText.x, headerText.y, headerText.width * 0.67f, headerText.height * 0.56f);
            Rect level = new Rect(title.xMax, headerText.y, headerText.width - title.width, title.height);
            Rect category = new Rect(headerText.x, title.yMax, headerText.width, headerText.height - title.height);

            float progressY = Mathf.Max(icon.yMax, headerText.yMax) + padding * 0.55f;
            float progressHeight = compact ? 9f : 12f;
            Rect levelProgress = new Rect(inner.x, progressY, inner.width, progressHeight);
            float descriptionY = levelProgress.yMax + padding * 0.65f;
            float descriptionHeight = compact ? 44f : 70f;
            Rect description = new Rect(inner.x, descriptionY, inner.width, descriptionHeight);

            float actionHeight = Mathf.Clamp(card.height * 0.12f, compact ? 44f : 52f, compact ? 54f : 66f);
            float footerY = inner.yMax - actionHeight;
            float costWidth = Mathf.Clamp(inner.width * 0.43f, 120f, 190f);
            Rect cost = new Rect(inner.x, footerY, costWidth, actionHeight);
            Rect action = new Rect(cost.xMax + padding * 0.7f, footerY, Mathf.Max(1f, inner.xMax - cost.xMax - padding * 0.7f), actionHeight);

            float effectTop = description.yMax + padding * 0.45f;
            float effectBottom = footerY - padding * 0.55f;
            float effectGap = compact ? 4f : 8f;
            float effectHeight = Mathf.Max(22f, (effectBottom - effectTop - effectGap) * 0.5f);
            Rect currentEffect = new Rect(inner.x, effectTop, inner.width, effectHeight);
            Rect nextEffect = new Rect(inner.x, currentEffect.yMax + effectGap, inner.width, effectHeight);

            return new UpgradeCardLayoutRects(
                icon,
                title,
                category,
                level,
                levelProgress,
                description,
                currentEffect,
                nextEffect,
                cost,
                action);
        }

        private static Rect Inset(Rect rect, float inset)
        {
            float safeInset = Mathf.Max(0f, inset);
            return new Rect(
                rect.x + safeInset,
                rect.y + safeInset,
                Mathf.Max(1f, rect.width - safeInset * 2f),
                Mathf.Max(1f, rect.height - safeInset * 2f));
        }
    }
}
