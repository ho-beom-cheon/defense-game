using UnityEngine;

namespace RuneGate
{
    public readonly struct BattleCanvasRects
    {
        public BattleCanvasRects(Rect root, Rect hud, Rect battlefield, Rect skills, Rect overlay)
        {
            Root = root;
            Hud = hud;
            Battlefield = battlefield;
            Skills = skills;
            Overlay = overlay;
        }

        public Rect Root { get; }
        public Rect Hud { get; }
        public Rect Battlefield { get; }
        public Rect Skills { get; }
        public Rect Overlay { get; }
    }

    public static class BattleCanvasLayout
    {
        public const float ReferenceWidth = 1080f;
        public const float ReferenceHeight = 1920f;
        public const float MaximumRootHeight = 2160f;
        public const float HudHeight = 152f;
        public const float SkillHeight = 400f;
        public const float Gap = 16f;
        public const float MinimumTouchHeight = 88f;
        public const float ModalMaximumWidth = 920f;
        public const float ModalMargin = 40f;

        public static BattleCanvasRects Calculate(float width, float height, Rect safeArea)
        {
            Rect safe = NormalizeSafeArea(width, height, safeArea);
            float rootWidth = Mathf.Min(safe.width, ReferenceWidth);
            float rootHeight = Mathf.Min(safe.height, MaximumRootHeight);
            Rect root = new Rect(
                safe.x + (safe.width - rootWidth) * 0.5f,
                safe.y + (safe.height - rootHeight) * 0.5f,
                rootWidth,
                rootHeight);

            float scale = Mathf.Clamp(rootHeight / ReferenceHeight, 0.55f, 1f);
            float hudHeight = Mathf.Max(96f, HudHeight * scale);
            float skillHeight = Mathf.Max(276f, SkillHeight * scale);
            float gap = Gap * scale;
            float minimumBattlefield = 240f * scale;
            float reserved = hudHeight + skillHeight + gap * 2f;
            if (rootHeight - reserved < minimumBattlefield)
            {
                float availableBands = Mathf.Max(1f, rootHeight - minimumBattlefield - gap * 2f);
                float bandRatio = availableBands / Mathf.Max(1f, HudHeight + SkillHeight);
                hudHeight = HudHeight * bandRatio;
                skillHeight = SkillHeight * bandRatio;
            }

            Rect skills = new Rect(root.x, root.y, root.width, skillHeight);
            Rect hud = new Rect(root.x, root.yMax - hudHeight, root.width, hudHeight);
            Rect battlefield = new Rect(
                root.x,
                skills.yMax + gap,
                root.width,
                Mathf.Max(1f, hud.y - gap - (skills.yMax + gap)));
            return new BattleCanvasRects(root, hud, battlefield, skills, root);
        }

        private static Rect NormalizeSafeArea(float width, float height, Rect safeArea)
        {
            float safeWidth = Mathf.Max(1f, width);
            float safeHeight = Mathf.Max(1f, height);
            if (safeArea.width <= 0f || safeArea.height <= 0f)
            {
                return new Rect(0f, 0f, safeWidth, safeHeight);
            }

            float x = Mathf.Clamp(safeArea.x, 0f, safeWidth - 1f);
            float y = Mathf.Clamp(safeArea.y, 0f, safeHeight - 1f);
            float maxWidth = Mathf.Max(1f, safeWidth - x);
            float maxHeight = Mathf.Max(1f, safeHeight - y);
            return new Rect(x, y, Mathf.Clamp(safeArea.width, 1f, maxWidth), Mathf.Clamp(safeArea.height, 1f, maxHeight));
        }
    }

    public enum BattleOverlayState
    {
        None = 0,
        Pause = 1,
        Tutorial = 2,
        RuneSelection = 3,
        Result = 4
    }
}
