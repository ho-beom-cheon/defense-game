using UnityEngine;

namespace RuneGate
{
    [CreateAssetMenu(fileName = "KoreanFontCatalog", menuName = "RuneGate/UI/Korean Font Catalog")]
    public sealed class KoreanFontCatalog : ScriptableObject
    {
        [SerializeField] private Font regularFont;
        [SerializeField] private Font semiBoldFont;
        [SerializeField] private Font boldFont;

        public Font RegularFont => regularFont;
        public Font SemiBoldFont => semiBoldFont;
        public Font BoldFont => boldFont;
    }
}
