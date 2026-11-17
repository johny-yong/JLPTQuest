using UnityEngine;

namespace JLPTQuest
{
    //The greybox palette: ink on paper, vermilion and matcha only where they mean something
    public static class InkColors
    {
        public static readonly Color Paper = Hex(0xF4EFE4);
        public static readonly Color Panel = Hex(0xECE5D6);
        public static readonly Color Card = Hex(0xFCF9F2);
        public static readonly Color ParryTint = Hex(0xF7E4DE);
        public static readonly Color Tile = Hex(0xE2D9C6);
        public static readonly Color Ink = Hex(0x1A1714);
        public static readonly Color InkSoft = Hex(0x6B6157);
        public static readonly Color InkFaint = Hex(0xA89E8E);
        public static readonly Color Vermilion = Hex(0xC23B22);
        public static readonly Color Matcha = Hex(0x6E8B3D);
        public static readonly Color RightTint = Hex(0xD6E5C6);
        public static readonly Color WrongTint = Hex(0xF1CFC7);

        public static string ToTag(Color color)
        {
            return "#" + ColorUtility.ToHtmlStringRGB(color);
        }

        private static Color Hex(int rgb)
        {
            return new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);
        }
    }
}
