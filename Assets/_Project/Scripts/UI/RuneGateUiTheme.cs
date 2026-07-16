using TMPro;
using UnityEngine;

namespace RuneGate
{
    [CreateAssetMenu(fileName = "RuneGateUiTheme", menuName = "RuneGate/UI Theme")]
    public sealed class RuneGateUiTheme : ScriptableObject
    {
        [Header("봉문 전술 기록서 색상")]
        [SerializeField] private Color background = new Color32(11, 17, 23, 255);
        [SerializeField] private Color surface = new Color32(17, 28, 34, 255);
        [SerializeField] private Color alternateSurface = new Color32(24, 38, 45, 255);
        [SerializeField] private Color brass = new Color32(185, 144, 72, 255);
        [SerializeField] private Color runeBlue = new Color32(69, 183, 217, 255);
        [SerializeField] private Color success = new Color32(76, 207, 122, 255);
        [SerializeField] private Color danger = new Color32(214, 92, 92, 255);
        [SerializeField] private Color primaryText = new Color32(242, 240, 232, 255);
        [SerializeField] private Color mutedText = new Color32(169, 183, 188, 255);

        [Header("Noto Sans KR")]
        [SerializeField] private Font regularFont;
        [SerializeField] private Font semiboldFont;
        [SerializeField] private Font boldFont;

        [Header("UI 스프라이트")]
        [SerializeField] private Sprite panelSprite;
        [SerializeField] private Sprite buttonSprite;
        [SerializeField] private Sprite runeCardSprite;
        [SerializeField] private Sprite pauseIcon;

        [Header("모션")]
        [SerializeField] private float buttonPressDuration = 0.08f;
        [SerializeField] private float modalFadeDuration = 0.15f;
        [SerializeField] private float waveEnterDuration = 0.25f;
        [SerializeField] private float waveHoldDuration = 0.8f;
        [SerializeField] private float waveExitDuration = 0.25f;
        [SerializeField] private float resultCountDuration = 0.4f;
        [SerializeField] private float damageTextDuration = 0.6f;

        private TMP_FontAsset runtimeRegular;
        private TMP_FontAsset runtimeSemibold;
        private TMP_FontAsset runtimeBold;

        public Color Background => background;
        public Color Surface => surface;
        public Color AlternateSurface => alternateSurface;
        public Color Brass => brass;
        public Color RuneBlue => runeBlue;
        public Color Success => success;
        public Color Danger => danger;
        public Color PrimaryText => primaryText;
        public Color MutedText => mutedText;
        public Sprite PanelSprite => panelSprite;
        public Sprite ButtonSprite => buttonSprite;
        public Sprite RuneCardSprite => runeCardSprite;
        public Sprite PauseIcon => pauseIcon;
        public float ButtonPressDuration => buttonPressDuration;
        public float ModalFadeDuration => modalFadeDuration;
        public float WaveEnterDuration => waveEnterDuration;
        public float WaveHoldDuration => waveHoldDuration;
        public float WaveExitDuration => waveExitDuration;
        public float ResultCountDuration => resultCountDuration;
        public float DamageTextDuration => damageTextDuration;

        public TMP_FontAsset Regular => GetOrCreateFont(ref runtimeRegular, regularFont);
        public TMP_FontAsset Semibold => GetOrCreateFont(ref runtimeSemibold, semiboldFont != null ? semiboldFont : regularFont);
        public TMP_FontAsset Bold => GetOrCreateFont(ref runtimeBold, boldFont != null ? boldFont : semiboldFont != null ? semiboldFont : regularFont);

        private static TMP_FontAsset GetOrCreateFont(ref TMP_FontAsset cached, Font source)
        {
            if (cached != null || source == null)
            {
                return cached;
            }

            cached = TMP_FontAsset.CreateFontAsset(source);
            if (cached != null)
            {
                cached.name = $"{source.name} Runtime TMP";
                cached.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                cached.isMultiAtlasTexturesEnabled = true;
                cached.hideFlags = HideFlags.DontSave;
            }

            return cached;
        }
    }
}
