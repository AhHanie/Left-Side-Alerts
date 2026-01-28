using Verse;

namespace Left_Side_Alerts
{
    public class ModSettings : Verse.ModSettings
    {
        public static float LeftOffset = 40f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref LeftOffset, "LeftOffset", 40f);
        }
    }
}
