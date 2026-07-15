using UnityEngine;

namespace RuneGate
{
    public readonly struct PetContractScreenLayoutRects
    {
        private readonly int itemCount;

        public PetContractScreenLayoutRects(
            Rect popup,
            Rect header,
            Rect headerTitle,
            Rect headerSummary,
            Rect closeButton,
            Rect equippedSummary,
            Rect viewport,
            Rect content,
            Rect footerFeedback,
            Rect footerUnequip,
            int columns,
            float gap,
            float cardHeight,
            int itemCount)
        {
            Popup = popup;
            Header = header;
            HeaderTitle = headerTitle;
            HeaderSummary = headerSummary;
            CloseButton = closeButton;
            EquippedSummary = equippedSummary;
            Viewport = viewport;
            Content = content;
            FooterFeedback = footerFeedback;
            FooterUnequip = footerUnequip;
            Columns = columns;
            Gap = gap;
            CardHeight = cardHeight;
            this.itemCount = itemCount;
        }

        public Rect Popup { get; }
        public Rect Header { get; }
        public Rect HeaderTitle { get; }
        public Rect HeaderSummary { get; }
        public Rect CloseButton { get; }
        public Rect EquippedSummary { get; }
        public Rect Viewport { get; }
        public Rect Content { get; }
        public Rect FooterFeedback { get; }
        public Rect FooterUnequip { get; }
        public int Columns { get; }
        public float Gap { get; }
        public float CardHeight { get; }
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
                row * (CardHeight + Gap),
                cardWidth,
                CardHeight);
        }
    }

    public readonly struct PetContractCardLayoutRects
    {
        public PetContractCardLayoutRects(
            Rect portrait,
            Rect passiveIcon,
            Rect title,
            Rect state,
            Rect passive,
            Rect shardBar,
            Rect shardText,
            Rect action)
        {
            Portrait = portrait;
            PassiveIcon = passiveIcon;
            Title = title;
            State = state;
            Passive = passive;
            ShardBar = shardBar;
            ShardText = shardText;
            Action = action;
        }

        public Rect Portrait { get; }
        public Rect PassiveIcon { get; }
        public Rect Title { get; }
        public Rect State { get; }
        public Rect Passive { get; }
        public Rect ShardBar { get; }
        public Rect ShardText { get; }
        public Rect Action { get; }
    }

    public static class PetContractScreenLayout
    {
        public const int ExpectedPetCount = 7;

        public static PetContractScreenLayoutRects Calculate(Rect popup, float screenWidth, float screenHeight, int itemCount)
        {
            int safeItemCount = Mathf.Max(0, itemCount);
            bool portrait = screenHeight >= screenWidth;
            bool useTwoColumns = portrait && popup.width >= 820f && popup.height >= 1100f;
            int columns = useTwoColumns ? 2 : 1;
            float gap = Mathf.Clamp(Mathf.Min(popup.width, popup.height) * 0.016f, 10f, 18f);
            float inset = Mathf.Clamp(gap, 12f, 18f);
            Rect inner = Inset(popup, inset);

            float headerHeight = Mathf.Clamp(inner.height * 0.085f, 82f, 124f);
            float equippedHeight = Mathf.Clamp(inner.height * 0.075f, 72f, 104f);
            float footerHeight = Mathf.Clamp(inner.height * 0.065f, 62f, 86f);
            Rect header = new Rect(inner.x, inner.y, inner.width, headerHeight);
            float closeSize = Mathf.Clamp(headerHeight * 0.48f, 44f, 58f);
            Rect closeButton = new Rect(header.xMax - closeSize, header.y + (header.height - closeSize) * 0.5f, closeSize, closeSize);
            Rect headerText = new Rect(header.x, header.y, Mathf.Max(1f, closeButton.x - header.x - gap), header.height);
            Rect headerTitle = new Rect(headerText.x, headerText.y, headerText.width, headerText.height * 0.58f);
            Rect headerSummary = new Rect(headerText.x, headerTitle.yMax, headerText.width, headerText.height - headerTitle.height);

            Rect equippedSummary = new Rect(inner.x, header.yMax + gap * 0.45f, inner.width, equippedHeight);
            Rect footer = new Rect(inner.x, inner.yMax - footerHeight, inner.width, footerHeight);
            float footerButtonWidth = Mathf.Clamp(footer.width * 0.30f, 150f, 260f);
            Rect footerUnequip = new Rect(footer.xMax - footerButtonWidth, footer.y, footerButtonWidth, footer.height);
            Rect footerFeedback = new Rect(footer.x, footer.y, Mathf.Max(1f, footerUnequip.x - footer.x - gap), footer.height);

            float viewportY = equippedSummary.yMax + gap;
            Rect viewport = new Rect(inner.x, viewportY, inner.width, Mathf.Max(1f, footer.y - gap - viewportY));
            float cardHeight = columns == 2
                ? Mathf.Clamp(viewport.width * 0.31f, 270f, 320f)
                : Mathf.Clamp(viewport.width * (portrait ? 0.39f : 0.30f), 250f, 320f);
            int rows = safeItemCount == 0 ? 0 : Mathf.CeilToInt(safeItemCount / (float)columns);
            float cardsHeight = rows > 0 ? rows * cardHeight + (rows - 1) * gap : 0f;
            float scrollbarReserve = cardsHeight > viewport.height + 0.5f ? 18f : 0f;
            Rect content = new Rect(0f, 0f, Mathf.Max(1f, viewport.width - scrollbarReserve), Mathf.Max(viewport.height, cardsHeight));

            return new PetContractScreenLayoutRects(
                popup,
                header,
                headerTitle,
                headerSummary,
                closeButton,
                equippedSummary,
                viewport,
                content,
                footerFeedback,
                footerUnequip,
                columns,
                gap,
                cardHeight,
                safeItemCount);
        }

        public static PetContractScreenLayoutRects CalculateForSize(float screenWidth, float screenHeight, int itemCount = ExpectedPetCount)
        {
            Rect popup = GameFrameLayout.PopupFrameForSize(screenWidth, screenHeight, 960f, 1680f, 0.94f, 0.84f);
            return Calculate(popup, screenWidth, screenHeight, itemCount);
        }

        public static PetContractCardLayoutRects CardLayout(Rect card)
        {
            float padding = Mathf.Clamp(card.width * 0.035f, 12f, 18f);
            Rect inner = Inset(card, padding);
            float portraitSize = Mathf.Clamp(Mathf.Min(inner.width * 0.23f, inner.height * 0.48f), 76f, 126f);
            Rect portrait = new Rect(inner.x, inner.y, portraitSize, portraitSize);
            float passiveIconSize = Mathf.Clamp(portraitSize * 0.34f, 30f, 42f);
            Rect passiveIcon = new Rect(portrait.xMax - passiveIconSize, portrait.yMax - passiveIconSize, passiveIconSize, passiveIconSize);

            float textX = portrait.xMax + padding * 0.8f;
            float textWidth = Mathf.Max(1f, inner.xMax - textX);
            float titleHeight = Mathf.Clamp(inner.height * 0.16f, 42f, 56f);
            Rect title = new Rect(textX, inner.y, textWidth, titleHeight);
            Rect state = new Rect(textX, title.yMax, textWidth, Mathf.Clamp(inner.height * 0.12f, 32f, 42f));
            Rect passive = new Rect(textX, state.yMax, textWidth, Mathf.Max(42f, portrait.yMax - state.yMax));

            float actionHeight = Mathf.Clamp(inner.height * 0.19f, 48f, 62f);
            float actionWidth = Mathf.Clamp(inner.width * 0.31f, 132f, 190f);
            Rect action = new Rect(inner.xMax - actionWidth, inner.yMax - actionHeight, actionWidth, actionHeight);
            float shardTextHeight = Mathf.Clamp(actionHeight * 0.52f, 26f, 34f);
            Rect shardText = new Rect(inner.x, inner.yMax - shardTextHeight, Mathf.Max(1f, action.x - inner.x - padding), shardTextHeight);
            Rect shardBar = new Rect(inner.x, shardText.y - 16f, shardText.width, 12f);

            return new PetContractCardLayoutRects(portrait, passiveIcon, title, state, passive, shardBar, shardText, action);
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
