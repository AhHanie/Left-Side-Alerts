using UnityEngine;
using Verse;

namespace Left_Side_Alerts
{
    public static class ModSettingsWindow
    {
        public static void Draw(Rect parent)
        {
            var listing = new Listing_Standard();
            listing.Begin(parent);

            listing.Label("LeftSideAlerts.LeftOffsetLabel".Translate(ModSettings.LeftOffset.ToString("0")));
            ModSettings.LeftOffset = listing.Slider(ModSettings.LeftOffset, 0f, 1000f);

            listing.GapLine();

            listing.Label("LeftSideAlerts.StartYOffsetLabel".Translate(ModSettings.StartYOffset.ToString("0")));
            ModSettings.StartYOffset = listing.Slider(ModSettings.StartYOffset, -500f, 500f);

            listing.GapLine();

            float letterScalePercent = Mathf.Round(ModSettings.LetterScale * 100f);
            listing.Label("LeftSideAlerts.LetterScaleLabel".Translate(letterScalePercent.ToString("0")));
            ModSettings.LetterScale = listing.Slider(ModSettings.LetterScale, 0.5f, 2.0f);

            listing.GapLine();

            string fontModeLabel = GetFontModeLabel(ModSettings.FontMode);
            listing.Label("LeftSideAlerts.FontModeLabel".Translate(fontModeLabel));
            float fontMode = listing.Slider(ModSettings.FontMode, 0f, 3f);
            ModSettings.FontMode = Mathf.RoundToInt(fontMode);

            listing.GapLine();

            if (listing.ButtonText("LeftSideAlerts.ResetDefaults".Translate()))
            {
                ModSettings.ResetToDefaults();
            }

            listing.End();
        }

        private static string GetFontModeLabel(int mode)
        {
            switch (mode)
            {
                case 1:
                    return "LeftSideAlerts.FontModeTiny".Translate();
                case 2:
                    return "LeftSideAlerts.FontModeSmall".Translate();
                case 3:
                    return "LeftSideAlerts.FontModeMedium".Translate();
                default:
                    return "LeftSideAlerts.FontModeAuto".Translate();
            }
        }
    }
}
