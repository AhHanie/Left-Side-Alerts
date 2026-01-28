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

            listing.End();
        }
    }
}
