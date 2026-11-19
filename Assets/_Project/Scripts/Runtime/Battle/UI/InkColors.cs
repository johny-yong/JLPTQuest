using UnityEngine;

namespace JLPTQuest
{
    /*
        The Night ink palette. Deep indigo grounds, washi only where you read, vermilion and gold where it matters.
        Light text names (Moon...) are for text on night and lacquer, Ink names are for text on washi.
        Lacquer and the tile tints colour the shaded tile sprite, the paper tints colour the washi card.
    */
    public static class InkColors
    {
        //Night
        public static readonly Color Night = Hex(0x13111C);
        public static readonly Color NightDeep = Hex(0x0B0A10);
        public static readonly Color Indigo = Hex(0x2B2752);

        //Lacquer surfaces on night
        public static readonly Color Lacquer = Hex(0x2B2641);
        public static readonly Color LacquerLow = Hex(0x1A1726);
        public static readonly Color GoldChip = Hex(0x3A2E1A);
        public static readonly Color TileRight = Hex(0x5E7A3C);
        public static readonly Color TileWrong = Hex(0x8A2414);

        //Text on night
        public static readonly Color Moon = Hex(0xF3EBDD);
        public static readonly Color MoonSoft = Hex(0xBDB4A6);
        public static readonly Color MoonFaint = Hex(0x8C836F);
        public static readonly Color MoonDim = Hex(0x5E5768);

        //Washi and the ink on it
        public static readonly Color Washi = Hex(0xF1E8D8);
        public static readonly Color Ink = Hex(0x1A1714);
        public static readonly Color InkSoft = Hex(0x6E5F49);
        public static readonly Color InkFaint = Hex(0xA89E8E);
        public static readonly Color ParryTint = Hex(0xF7DCD3);
        public static readonly Color RightTint = Hex(0xDCEBCB);
        public static readonly Color WrongTint = Hex(0xF4D2C9);

        //Accents
        public static readonly Color Vermilion = Hex(0xC9341F);
        public static readonly Color VermilionGlow = Hex(0xE0462E);
        public static readonly Color VermilionDeep = Hex(0x8A2414);
        public static readonly Color OnVermilion = Hex(0xFFF4E6);
        public static readonly Color Gold = Hex(0xD4AF5F);
        public static readonly Color GoldLight = Hex(0xF0D58C);
        public static readonly Color GoldDeep = Hex(0x8E6F2E);
        public static readonly Color StarOff = Hex(0x6E5A3E);
        public static readonly Color Matcha = Hex(0x8FB86A);
        public static readonly Color MatchaDeep = Hex(0x5E7A3C);

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

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
