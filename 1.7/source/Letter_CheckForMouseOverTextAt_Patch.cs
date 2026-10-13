using HarmonyLib;
using UnityEngine;
using Verse;

namespace Left_Side_Alerts
{
    [HarmonyPatch(typeof(Letter), nameof(Letter.CheckForMouseOverTextAt))]
    internal static class Letter_CheckForMouseOverTextAt_Patch
    {
        [HarmonyPostfix]
        [HarmonyInfix(typeof(UI), nameof(UI.screenWidth), InnerTargetKind.FieldRead, Positions = new[] { 1 })]
        private static int LeftAnchor(int __result)
        {
            return LetterLayout.GetLeftAnchorWidth();
        }

        [HarmonyPostfix]
        [HarmonyInfix(38f)]
        private static float ButtonWidth(float __result)
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
        private static float Padding(float __result)
        {
            return LetterLayout.Scale(__result);
        }

        [HarmonyPrefix]
        [HarmonyInfix(typeof(Text), nameof(Text.Font), InnerTargetKind.Setter)]
        private static void LetterFont(ref GameFont value)
        {
            value = ModSettings.GetLetterFont();
        }

        [HarmonyPostfix]
        [HarmonyInfix(310f)]
        private static float TextWidth(float __result)
        {
            return LetterLayout.Scale(__result);
        }

        [HarmonyPostfix]
        [HarmonyInfix(20f)]
        private static float TextPadding(float __result)
        {
            return LetterLayout.Scale(__result);
        }

        [HarmonyPostfix]
        [HarmonyInfix(330f, Positions = new[] { 1 })]
        private static float TooltipOffsetFromButton(float __result)
        {
            return -LetterLayout.Scale(38f);
        }

        [HarmonyPostfix]
        [HarmonyInfix(10f)]
        private static float TooltipGap(float __result)
        {
            return -LetterLayout.Scale(__result);
        }

        [HarmonyPostfix]
        [HarmonyInfix(330f, Positions = new[] { 2 })]
        private static float TooltipWidth(float __result)
        {
            return LetterLayout.Scale(__result);
        }

        [HarmonyPrefix]
        [HarmonyInfix(typeof(WindowStack), nameof(WindowStack.ImmediateWindow))]
        private static void KeepTooltipOnScreen(ref Rect rect)
        {
            rect.x = Mathf.Clamp(rect.x, 0f, Mathf.Max(UI.screenWidth - rect.width, 0f));
        }
    }
}
