using HarmonyLib;
using UnityEngine;
using Verse;

namespace Left_Side_Alerts
{
    [HarmonyPatch(typeof(Letter), nameof(Letter.DrawButtonAt))]
    internal static class Letter_DrawButtonAt_Font_Patch
    {
        private static void Prefix(out GameFont __state)
        {
            __state = Text.Font;
            Text.Font = ModSettings.GetLetterFont();
        }

        private static void Finalizer(GameFont __state)
        {
            Text.Font = __state;
        }
    }

    [HarmonyPatch(typeof(Letter), nameof(Letter.DrawButtonAt))]
    internal static class Letter_DrawButtonAt_Layout_Patch
    {
        private static float pendingLabelShift;

        [HarmonyPostfix]
        [HarmonyInfix(typeof(UI), nameof(UI.screenWidth), InnerTargetKind.FieldRead, Positions = new[] { 1 })]
        private static int LeftAnchor(int __result)
        {
            return LetterLayout.GetLeftAnchorWidth();
        }

        [HarmonyPostfix]
        [HarmonyInfix(0.06f)]
        private static float BounceTowardsRight(float __result)
        {
            return -__result;
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

        [HarmonyPostfix]
        [HarmonyInfix(6f)]
        private static float LabelPadding(float __result)
        {
            return LetterLayout.Scale(__result);
        }

        [HarmonyPostfix]
        [HarmonyInfix(16f)]
        private static float LabelHeight(float __result)
        {
            return LetterLayout.Scale(__result);
        }

        [HarmonyPostfix]
        [HarmonyInfix(4f)]
        private static float LabelYOffset(float __result)
        {
            return LetterLayout.Scale(__result);
        }

        [HarmonyPrefix]
        [HarmonyInfix(typeof(GUI), nameof(GUI.DrawTexture), typeof(Rect), typeof(Texture), Positions = new[] { 2 })]
        private static void ShiftLabelBackground(ref Rect position)
        {
            float labelLeft = position.x + LetterLayout.Scale(6f) + 1f;
            float shiftedLeft = Mathf.Max(labelLeft + ModSettings.TextOffset, 2f);
            pendingLabelShift = shiftedLeft - labelLeft;
            position.x += pendingLabelShift;
        }

        [HarmonyPrefix]
        [HarmonyInfix(typeof(Widgets), nameof(Widgets.Label), typeof(Rect), typeof(string))]
        private static void ShiftLabel(ref Rect rect)
        {
            rect.x += pendingLabelShift;
            pendingLabelShift = 0f;
        }
    }
}
