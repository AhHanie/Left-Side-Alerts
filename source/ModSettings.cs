using Verse;

namespace Left_Side_Alerts
{
    public class ModSettings : Verse.ModSettings
    {
        public const float DefaultLeftOffset = 40f;
        public const float DefaultStartYOffset = 0f;
        public const float DefaultLetterScale = 1f;
        public const int DefaultFontMode = 0;

        public static float LeftOffset = DefaultLeftOffset;
        public static float StartYOffset = DefaultStartYOffset;
        public static float LetterScale = DefaultLetterScale;
        public static int FontMode = DefaultFontMode;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref LeftOffset, "LeftOffset", DefaultLeftOffset);
            Scribe_Values.Look(ref StartYOffset, "StartYOffset", DefaultStartYOffset);
            Scribe_Values.Look(ref LetterScale, "LetterScale", DefaultLetterScale);
            Scribe_Values.Look(ref FontMode, "FontMode", DefaultFontMode);
        }

        public static void ResetToDefaults()
        {
            LeftOffset = DefaultLeftOffset;
            StartYOffset = DefaultStartYOffset;
            LetterScale = DefaultLetterScale;
            FontMode = DefaultFontMode;
        }

        public static GameFont GetLetterFont()
        {
            switch (FontMode)
            {
                case 1:
                    return GameFont.Tiny;
                case 2:
                    return GameFont.Small;
                case 3:
                    return GameFont.Medium;
                default:
                    return GetAutoFont();
            }
        }

        private static GameFont GetAutoFont()
        {
            float scale = LetterScale;
            if (scale <= 0.85f)
            {
                return GameFont.Tiny;
            }

            if (scale >= 1.15f)
            {
                return GameFont.Medium;
            }

            return GameFont.Small;
        }
    }
}
