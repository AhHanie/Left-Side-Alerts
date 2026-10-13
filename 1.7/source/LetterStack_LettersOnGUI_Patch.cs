using HarmonyLib;
using Verse;

namespace Left_Side_Alerts
{
    [HarmonyPatch(typeof(LetterStack), nameof(LetterStack.LettersOnGUI))]
    internal static class LetterStack_LettersOnGUI_Patch
    {
        private static void Prefix(ref float baseY)
        {
            LetterStackBaseYCapture.Capture(baseY);
            baseY += ModSettings.StartYOffset;
        }
    }

    [HarmonyPatch(typeof(LetterStack), nameof(LetterStack.LettersOnGUI))]
    internal static class LetterStack_LettersOnGUI_Layout_Patch
    {
        [HarmonyPostfix]
        [HarmonyInfix(42f)]
        private static float SlotHeight(float __result)
        {
            return LetterLayout.Scale(__result);
        }

        [HarmonyPostfix]
        [HarmonyInfix(30f)]
        private static float ButtonHeight(float __result)
        {
            return LetterLayout.Scale(__result);
        }

        [HarmonyPostfix]
        [HarmonyInfix(12f)]
        private static float Spacing(float __result)
        {
            return LetterLayout.Scale(__result);
        }
    }

    internal static class LetterStackBaseYCapture
    {
        private static float lastBaseY;
        private static bool hasValue;

        public static void Capture(float baseY)
        {
            lastBaseY = baseY;
            hasValue = true;
        }

        public static bool TryGet(out float baseY)
        {
            baseY = lastBaseY;
            return hasValue;
        }
    }
}
