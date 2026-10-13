using UnityEngine;

namespace Left_Side_Alerts
{
    internal static class LetterLayout
    {
        public static float Scale(float value)
        {
            return value * ModSettings.LetterScale;
        }

        public static int GetLeftAnchorWidth()
        {
            return Mathf.RoundToInt(50f + ModSettings.LeftOffset);
        }
    }
}
