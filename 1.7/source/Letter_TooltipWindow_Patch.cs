using HarmonyLib;
using System.Linq;
using System.Reflection;
using Verse;

namespace Left_Side_Alerts
{
    [HarmonyPatch]
    internal static class Letter_TooltipWindow_Patch
    {
        private static MethodBase TargetMethod()
        {
            MethodInfo owner = AccessTools.Method(typeof(Letter), nameof(Letter.CheckForMouseOverTextAt));
            return AccessTools.Lambdas(owner).Single();
        }

        [HarmonyPrefix]
        [HarmonyInfix(typeof(Text), nameof(Text.Font), InnerTargetKind.Setter)]
        private static void LetterFont(ref GameFont value)
        {
            value = ModSettings.GetLetterFont();
        }

        [HarmonyPostfix]
        [HarmonyInfix(10f)]
        private static float TooltipInset(float __result)
        {
            return LetterLayout.Scale(__result);
        }
    }
}
