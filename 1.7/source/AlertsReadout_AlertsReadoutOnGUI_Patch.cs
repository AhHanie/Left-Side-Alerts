using HarmonyLib;
using RimWorld;
using Verse;

namespace Left_Side_Alerts
{
    [HarmonyPatch(typeof(AlertsReadout), nameof(AlertsReadout.AlertsReadoutOnGUI))]
    internal static class AlertsReadout_AlertsReadoutOnGUI_Patch
    {
        [HarmonyPostfix]
        [HarmonyInfix(typeof(LetterStack), nameof(LetterStack.LastTopY), InnerTargetKind.Getter)]
        private static float UnshiftedLastTopY(float __result)
        {
            if (LetterStackBaseYCapture.TryGet(out float baseY))
            {
                return baseY + 10f;
            }

            return __result - ModSettings.StartYOffset;
        }
    }
}
