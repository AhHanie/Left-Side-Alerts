using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace Left_Side_Alerts
{
    [HarmonyPatch]
    internal static class Letter_DrawButtonAt_Patch
    {
        private static bool loggedClampInsert;

        public static MethodBase TargetMethod()
        {
            MethodBase method = AccessTools.Method(typeof(Verse.Letter), "DrawButtonAt", new[] { typeof(float) });
            if (method == null)
            {
                Logger.Error("Letter.DrawButtonAt not found; patch failed.");
            }

            return method;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> list = new List<CodeInstruction>(instructions);
            list = Letter_Transpiler_Util.ReplaceScreenWidth(
                list,
                "Letter.DrawButtonAt");

            MethodInfo heightProvider = AccessTools.Method(typeof(Letter_DrawButtonAt_Patch), nameof(GetScaledLetterHeight));
            MethodInfo widthProvider = AccessTools.Method(typeof(Letter_DrawButtonAt_Patch), nameof(GetScaledLetterWidth));
            MethodInfo paddingProvider = AccessTools.Method(typeof(Letter_DrawButtonAt_Patch), nameof(GetScaledPadding));
            MethodInfo labelPaddingProvider = AccessTools.Method(typeof(Letter_DrawButtonAt_Patch), nameof(GetScaledLabelPadding));
            MethodInfo labelHeightProvider = AccessTools.Method(typeof(Letter_DrawButtonAt_Patch), nameof(GetScaledLabelHeight));
            MethodInfo labelYOffsetProvider = AccessTools.Method(typeof(Letter_DrawButtonAt_Patch), nameof(GetScaledLabelYOffset));
            int heightReplacements = Letter_Transpiler_Util.ReplaceFloatConstant(list, 30f, heightProvider);
            int widthReplacements = Letter_Transpiler_Util.ReplaceFloatConstant(list, 38f, widthProvider);
            int paddingReplacements = Letter_Transpiler_Util.ReplaceFloatConstant(list, 12f, paddingProvider);
            int labelPaddingReplacements = Letter_Transpiler_Util.ReplaceFloatConstant(list, 6f, labelPaddingProvider);
            int labelHeightReplacements = Letter_Transpiler_Util.ReplaceFloatConstant(list, 16f, labelHeightProvider);
            int labelYOffsetReplacements = Letter_Transpiler_Util.ReplaceFloatConstant(list, 4f, labelYOffsetProvider);

            LocalBuilder previousFont = generator.DeclareLocal(typeof(GameFont));
            MethodInfo pushFont = AccessTools.Method(typeof(Letter_DrawButtonAt_Patch), nameof(PushLetterFont));
            MethodInfo popFont = AccessTools.Method(typeof(Letter_DrawButtonAt_Patch), nameof(PopLetterFont));
            bool fontInjected = false;

            list.InsertRange(0, new[]
            {
                new CodeInstruction(OpCodes.Call, pushFont),
                Letter_Transpiler_Util.MakeStloc(previousFont.LocalIndex)
            });

            bool inserted = false;
            int grayIndex = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].opcode == OpCodes.Ldsfld
                    && list[i].operand is FieldInfo field
                    && field.Name == "GrayTextBG")
                {
                    grayIndex = i;
                    break;
                }
            }

            int? vector2Local = null;
            int? labelWidthLocal = null;
            if (grayIndex > 0)
            {
                for (int i = 0; i < grayIndex - 1; i++)
                {
                    if (list[i].opcode == OpCodes.Newobj
                        && list[i].operand is ConstructorInfo ctor
                        && ctor.DeclaringType == typeof(Vector2))
                    {
                        vector2Local = Letter_Transpiler_Util.GetLocalIndex(list[i + 1]);
                    }

                    if (list[i].opcode == OpCodes.Ldfld
                        && list[i].operand is FieldInfo field
                        && field.DeclaringType == typeof(Vector2)
                        && field.Name == "x")
                    {
                        labelWidthLocal = Letter_Transpiler_Util.GetLocalIndex(list[i + 1]);
                    }
                }
            }

            if (!vector2Local.HasValue)
            {
                vector2Local = 11;
            }

            if (!labelWidthLocal.HasValue)
            {
                labelWidthLocal = 9;
            }

            if (grayIndex > 0 && vector2Local.HasValue && labelWidthLocal.HasValue)
            {
                list.InsertRange(grayIndex, new[]
                {
                    Letter_Transpiler_Util.MakeLdloca(vector2Local.Value),
                    Letter_Transpiler_Util.MakeLdloc(labelWidthLocal.Value),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Letter_DrawButtonAt_Patch), nameof(ClampLabelPosition)))
                });
                inserted = true;
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].opcode == OpCodes.Ret)
                {
                    list.InsertRange(i, new[]
                    {
                        Letter_Transpiler_Util.MakeLdloc(previousFont.LocalIndex),
                        new CodeInstruction(OpCodes.Call, popFont)
                    });
                    fontInjected = true;
                    break;
                }
            }

            if (!loggedClampInsert)
            {
                loggedClampInsert = true;
                if (inserted)
                {
                    Logger.Message("Letter.DrawButtonAt label clamp injected.");
                }
                else
                {
                    Logger.Error("Letter.DrawButtonAt label clamp injection failed.");
                }

                if (heightReplacements > 0)
                {
                    Logger.Message("Letter.DrawButtonAt height scaling injected.");
                }
                else
                {
                    Logger.Error("Letter.DrawButtonAt height scaling injection failed.");
                }

                if (widthReplacements > 0)
                {
                    Logger.Message("Letter.DrawButtonAt width scaling injected.");
                }
                else
                {
                    Logger.Error("Letter.DrawButtonAt width scaling injection failed.");
                }

                if (paddingReplacements > 0)
                {
                    Logger.Message("Letter.DrawButtonAt padding scaling injected.");
                }
                else
                {
                    Logger.Error("Letter.DrawButtonAt padding scaling injection failed.");
                }

                if (labelPaddingReplacements > 0)
                {
                    Logger.Message("Letter.DrawButtonAt label padding scaling injected.");
                }
                else
                {
                    Logger.Error("Letter.DrawButtonAt label padding scaling injection failed.");
                }

                if (labelHeightReplacements > 0)
                {
                    Logger.Message("Letter.DrawButtonAt label height scaling injected.");
                }
                else
                {
                    Logger.Error("Letter.DrawButtonAt label height scaling injection failed.");
                }

                if (labelYOffsetReplacements > 0)
                {
                    Logger.Message("Letter.DrawButtonAt label Y offset scaling injected.");
                }
                else
                {
                    Logger.Error("Letter.DrawButtonAt label Y offset scaling injection failed.");
                }

                if (fontInjected)
                {
                    Logger.Message("Letter.DrawButtonAt font override injected.");
                }
                else
                {
                    Logger.Error("Letter.DrawButtonAt font override injection failed.");
                }
            }

            return list;
        }

        private static void ClampLabelPosition(ref Vector2 position, float labelWidth)
        {
            float halfWidth = labelWidth * 0.5f;
            float minX = halfWidth + 2f;
            if (position.x < minX)
            {
                position.x = minX;
            }
        }

        private static float GetScaledLetterHeight()
        {
            return 30f * ModSettings.LetterScale;
        }

        private static float GetScaledLetterWidth()
        {
            return 38f * ModSettings.LetterScale;
        }

        private static float GetScaledPadding()
        {
            return 12f * ModSettings.LetterScale;
        }

        private static float GetScaledLabelPadding()
        {
            return 6f * ModSettings.LetterScale;
        }

        private static float GetScaledLabelHeight()
        {
            return 16f * ModSettings.LetterScale;
        }

        private static float GetScaledLabelYOffset()
        {
            return 4f * ModSettings.LetterScale;
        }

        private static GameFont PushLetterFont()
        {
            GameFont previous = Text.Font;
            Text.Font = ModSettings.GetLetterFont();
            return previous;
        }

        private static void PopLetterFont(GameFont previous)
        {
            Text.Font = previous;
        }

        // Label clamp injected directly before label background draw.
    }

    [HarmonyPatch]
    internal static class Letter_CheckForMouseOverTextAt_Patch
    {
        public static MethodBase TargetMethod()
        {
            MethodBase method = AccessTools.Method(typeof(Verse.Letter), "CheckForMouseOverTextAt", new[] { typeof(float) });
            if (method == null)
            {
                Log.Error("Left Side Alerts: Letter.CheckForMouseOverTextAt not found; patch failed.");
            }

            return method;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = new List<CodeInstruction>(instructions);
            list = Letter_Transpiler_Util.ReplaceScreenWidth(
                list,
                "Letter.CheckForMouseOverTextAt");

            bool adjusted = false;
            for (int i = 0; i < list.Count - 4; i++)
            {
                if (list[i].opcode == OpCodes.Ldloc_0
                    && list[i + 1].opcode == OpCodes.Ldc_R4 && (float)list[i + 1].operand == 330f
                    && list[i + 2].opcode == OpCodes.Sub
                    && list[i + 3].opcode == OpCodes.Ldc_R4 && (float)list[i + 3].operand == 10f
                    && list[i + 4].opcode == OpCodes.Sub)
                {
                    // Move tooltip to the right of the letter: num + 38 + 10
                    list[i + 1] = new CodeInstruction(
                        OpCodes.Call,
                        AccessTools.Method(typeof(Letter_CheckForMouseOverTextAt_Patch), nameof(GetTooltipOffset)));
                    list[i + 2] = new CodeInstruction(OpCodes.Add);
                    list[i + 3] = new CodeInstruction(
                        OpCodes.Call,
                        AccessTools.Method(typeof(Letter_CheckForMouseOverTextAt_Patch), nameof(GetTooltipPadding)));
                    list[i + 4] = new CodeInstruction(OpCodes.Add);
                    adjusted = true;
                    break;
                }
            }

            MethodInfo fontProvider = AccessTools.Method(typeof(Letter_CheckForMouseOverTextAt_Patch), nameof(GetScaledFont));
            bool fontAdjusted = ReplaceFontSize(list, fontProvider);

            MethodInfo buttonWidthProvider = AccessTools.Method(typeof(Letter_CheckForMouseOverTextAt_Patch), nameof(GetScaledButtonWidth));
            MethodInfo buttonHeightProvider = AccessTools.Method(typeof(Letter_CheckForMouseOverTextAt_Patch), nameof(GetScaledButtonHeight));
            MethodInfo tooltipWidthProvider = AccessTools.Method(typeof(Letter_CheckForMouseOverTextAt_Patch), nameof(GetScaledTooltipWidth));
            MethodInfo tooltipPaddingProvider = AccessTools.Method(typeof(Letter_CheckForMouseOverTextAt_Patch), nameof(GetScaledTooltipPadding));
            MethodInfo tooltipOffsetProvider = AccessTools.Method(typeof(Letter_CheckForMouseOverTextAt_Patch), nameof(GetScaledTooltipOffset));

            int widthReplacements = Letter_Transpiler_Util.ReplaceFloatConstant(list, 38f, buttonWidthProvider);
            int heightReplacements = Letter_Transpiler_Util.ReplaceFloatConstant(list, 30f, buttonHeightProvider);
            int tooltipWidthReplacements = Letter_Transpiler_Util.ReplaceFloatConstant(list, 310f, tooltipWidthProvider);
            int tooltipPaddingReplacements = Letter_Transpiler_Util.ReplaceFloatConstant(list, 20f, tooltipPaddingProvider);
            int tooltipOffsetReplacements = Letter_Transpiler_Util.ReplaceFloatConstant(list, 330f, tooltipOffsetProvider);

            if (!Letter_Transpiler_Util.LoggedContains("Letter.CheckForMouseOverTextAt:Tooltip"))
            {
                Letter_Transpiler_Util.LoggedAdd("Letter.CheckForMouseOverTextAt:Tooltip");
                if (adjusted)
                {
                    Logger.Message("Letter.CheckForMouseOverTextAt tooltip moved to the right.");
                }
                else
                {
                    Logger.Error("Letter.CheckForMouseOverTextAt tooltip move failed.");
                }

                if (fontAdjusted)
                {
                    Logger.Message("Letter.CheckForMouseOverTextAt font scaling injected.");
                }
                else
                {
                    Logger.Error("Letter.CheckForMouseOverTextAt font scaling injection failed.");
                }

                if (widthReplacements > 0)
                {
                    Logger.Message("Letter.CheckForMouseOverTextAt button width scaling injected.");
                }
                else
                {
                    Logger.Error("Letter.CheckForMouseOverTextAt button width scaling injection failed.");
                }

                if (heightReplacements > 0)
                {
                    Logger.Message("Letter.CheckForMouseOverTextAt button height scaling injected.");
                }
                else
                {
                    Logger.Error("Letter.CheckForMouseOverTextAt button height scaling injection failed.");
                }

                if (tooltipWidthReplacements > 0)
                {
                    Logger.Message("Letter.CheckForMouseOverTextAt tooltip width scaling injected.");
                }
                else
                {
                    Logger.Error("Letter.CheckForMouseOverTextAt tooltip width scaling injection failed.");
                }

                if (tooltipPaddingReplacements > 0)
                {
                    Logger.Message("Letter.CheckForMouseOverTextAt tooltip padding scaling injected.");
                }
                else
                {
                    Logger.Error("Letter.CheckForMouseOverTextAt tooltip padding scaling injection failed.");
                }

                if (tooltipOffsetReplacements > 0)
                {
                    Logger.Message("Letter.CheckForMouseOverTextAt tooltip offset scaling injected.");
                }
                else
                {
                    Logger.Error("Letter.CheckForMouseOverTextAt tooltip offset scaling injection failed.");
                }
            }
            return list;
        }

        private static float GetTooltipOffset()
        {
            return GetScaledTooltipOffset();
        }

        private static float GetTooltipPadding()
        {
            return GetScaledTooltipInset();
        }

        private static bool ReplaceFontSize(List<CodeInstruction> list, MethodInfo scaledFontProvider)
        {
            MethodInfo setFont = AccessTools.PropertySetter(typeof(Text), "Font");
            bool replaced = false;

            if (setFont == null)
            {
                return false;
            }

            for (int i = 1; i < list.Count; i++)
            {
                if (list[i].opcode == OpCodes.Call
                    && list[i].operand is MethodInfo method
                    && method == setFont)
                {
                    CodeInstruction previous = list[i - 1];
                    if (Letter_Transpiler_Util.IsLdcI4(previous))
                    {
                        list[i - 1] = new CodeInstruction(OpCodes.Call, scaledFontProvider);
                        replaced = true;
                    }
                }
            }

            return replaced;
        }

        private static GameFont GetScaledFont()
        {
            return ModSettings.GetLetterFont();
        }

        private static float GetScaledButtonWidth()
        {
            return 38f * ModSettings.LetterScale;
        }

        private static float GetScaledButtonHeight()
        {
            return 30f * ModSettings.LetterScale;
        }

        private static float GetScaledTooltipWidth()
        {
            return 310f * ModSettings.LetterScale;
        }

        private static float GetScaledTooltipPadding()
        {
            return 20f * ModSettings.LetterScale;
        }

        private static float GetScaledTooltipInset()
        {
            return 10f * ModSettings.LetterScale;
        }

        private static float GetScaledTooltipOffset()
        {
            return 330f * ModSettings.LetterScale;
        }
    }

    [HarmonyPatch]
    internal static class Letter_TooltipWindow_Patch
    {
        private static MethodBase TargetMethod()
        {
            System.Type displayClass = typeof(Verse.Letter).GetNestedType("<>c__DisplayClass41_0", BindingFlags.NonPublic);
            if (displayClass == null)
            {
                Logger.Error("Letter tooltip display class not found; patch failed.");
                return null;
            }

            MethodInfo method = AccessTools.Method(displayClass, "<CheckForMouseOverTextAt>b__0");
            if (method == null)
            {
                Logger.Error("Letter tooltip window method not found; patch failed.");
            }

            return method;
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = new List<CodeInstruction>(instructions);
            MethodInfo fontProvider = AccessTools.Method(typeof(Letter_TooltipWindow_Patch), nameof(GetScaledFont));
            bool fontAdjusted = ReplaceFontSize(list, fontProvider);

            MethodInfo insetProvider = AccessTools.Method(typeof(Letter_TooltipWindow_Patch), nameof(GetScaledTooltipInset));
            int insetReplacements = Letter_Transpiler_Util.ReplaceFloatConstant(list, 10f, insetProvider);

            if (!Letter_Transpiler_Util.LoggedContains("Letter.TooltipWindow:Font"))
            {
                Letter_Transpiler_Util.LoggedAdd("Letter.TooltipWindow:Font");
                if (fontAdjusted)
                {
                    Logger.Message("Letter tooltip font scaling injected.");
                }
                else
                {
                    Logger.Error("Letter tooltip font scaling injection failed.");
                }

                if (insetReplacements > 0)
                {
                    Logger.Message("Letter tooltip inset scaling injected.");
                }
                else
                {
                    Logger.Error("Letter tooltip inset scaling injection failed.");
                }
            }

            return list;
        }

        private static bool ReplaceFontSize(List<CodeInstruction> list, MethodInfo scaledFontProvider)
        {
            MethodInfo setFont = AccessTools.PropertySetter(typeof(Text), "Font");
            bool replaced = false;

            if (setFont == null)
            {
                return false;
            }

            for (int i = 1; i < list.Count; i++)
            {
                if (list[i].opcode == OpCodes.Call
                    && list[i].operand is MethodInfo method
                    && method == setFont)
                {
                    CodeInstruction previous = list[i - 1];
                    if (Letter_Transpiler_Util.IsLdcI4(previous))
                    {
                        list[i - 1] = new CodeInstruction(OpCodes.Call, scaledFontProvider);
                        replaced = true;
                    }
                }
            }

            return replaced;
        }

        private static GameFont GetScaledFont()
        {
            return ModSettings.GetLetterFont();
        }

        private static float GetScaledTooltipInset()
        {
            return 10f * ModSettings.LetterScale;
        }
    }

    [HarmonyPatch(typeof(Verse.LetterStack), "LettersOnGUI")]
    internal static class LetterStack_LettersOnGUI_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = new List<CodeInstruction>(instructions);
            MethodInfo adjustBaseY = AccessTools.Method(typeof(LetterStack_LettersOnGUI_Patch), nameof(AdjustBaseY));
            bool baseYAdjusted = false;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].opcode == OpCodes.Ldarg_1)
                {
                    list.Insert(i + 1, new CodeInstruction(OpCodes.Call, adjustBaseY));
                    i++;
                    baseYAdjusted = true;
                }
            }

            MethodInfo slotHeightProvider = AccessTools.Method(typeof(LetterStack_LettersOnGUI_Patch), nameof(GetLetterSlotHeight));
            MethodInfo heightProvider = AccessTools.Method(typeof(LetterStack_LettersOnGUI_Patch), nameof(GetLetterButtonHeight));
            MethodInfo spacingProvider = AccessTools.Method(typeof(LetterStack_LettersOnGUI_Patch), nameof(GetLetterSpacing));

            int slotHeightReplacements = Letter_Transpiler_Util.ReplaceFloatConstant(list, 42f, slotHeightProvider);
            int heightReplacements = Letter_Transpiler_Util.ReplaceFloatConstant(list, 30f, heightProvider);
            int spacingReplacements = Letter_Transpiler_Util.ReplaceFloatConstant(list, 12f, spacingProvider);

            if (!Letter_Transpiler_Util.LoggedContains("LetterStack.LettersOnGUI:Layout"))
            {
                Letter_Transpiler_Util.LoggedAdd("LetterStack.LettersOnGUI:Layout");
                if (baseYAdjusted)
                {
                    Logger.Message("LetterStack.LettersOnGUI baseY offset injected.");
                }
                else
                {
                    Logger.Error("LetterStack.LettersOnGUI baseY offset injection failed.");
                }

                if (slotHeightReplacements > 0)
                {
                    Logger.Message("LetterStack.LettersOnGUI slot height scaling injected.");
                }
                else
                {
                    Logger.Error("LetterStack.LettersOnGUI slot height scaling injection failed.");
                }

                if (heightReplacements > 0)
                {
                    Logger.Message("LetterStack.LettersOnGUI button height scaling injected.");
                }
                else
                {
                    Logger.Error("LetterStack.LettersOnGUI button height scaling injection failed.");
                }

                if (spacingReplacements > 0)
                {
                    Logger.Message("LetterStack.LettersOnGUI spacing scaling injected.");
                }
                else
                {
                    Logger.Error("LetterStack.LettersOnGUI spacing scaling injection failed.");
                }
            }

            return list;
        }

        private static float AdjustBaseY(float baseY)
        {
            return baseY + ModSettings.StartYOffset;
        }

        private static float GetLetterButtonHeight()
        {
            return 30f * ModSettings.LetterScale;
        }

        private static float GetLetterSpacing()
        {
            return 12f * ModSettings.LetterScale;
        }

        private static float GetLetterSlotHeight()
        {
            return GetLetterButtonHeight() + GetLetterSpacing();
        }
    }

    internal static class Letter_Transpiler_Util
    {
        private static readonly HashSet<string> Logged = new HashSet<string>();

        public static List<CodeInstruction> ReplaceScreenWidth(
            List<CodeInstruction> list,
            string methodName)
        {
            bool replaced = false;
            MethodInfo widthProvider = AccessTools.Method(typeof(Letter_Transpiler_Util), nameof(GetLeftAnchorWidth));
            for (int i = 0; i < list.Count; i++)
            {
                CodeInstruction instruction = list[i];
                if (!replaced
                    && instruction.opcode == OpCodes.Ldsfld
                    && instruction.operand is FieldInfo field
                    && field.DeclaringType == typeof(UI)
                    && field.Name == "screenWidth")
                {
                    list[i] = new CodeInstruction(OpCodes.Call, widthProvider);
                    replaced = true;
                    if (i + 1 < list.Count && list[i + 1].opcode == OpCodes.Conv_R4)
                    {
                        list.RemoveAt(i + 1);
                    }
                    break;
                }
            }

            if (!Logged.Contains(methodName))
            {
                Logged.Add(methodName);
                if (replaced)
                {
                    Logger.Message(methodName + " IL replacement succeeded.");
                }
                else
                {
                    Logger.Error(methodName + " IL replacement failed.");
                }
            }

            return list;
        }

        public static int ReplaceFloatConstant(
            List<CodeInstruction> list,
            float value,
            MethodInfo replacement)
        {
            int count = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].opcode == OpCodes.Ldc_R4
                    && list[i].operand is float floatValue
                    && Mathf.Approximately(floatValue, value))
                {
                    list[i] = new CodeInstruction(OpCodes.Call, replacement);
                    count++;
                }
            }

            return count;
        }

        public static float GetLeftAnchorWidth()
        {
            float offset = ModSettings.LeftOffset;
            return 50f + offset;
        }

        public static int? GetLocalIndex(CodeInstruction instruction)
        {
            if (instruction.opcode == OpCodes.Stloc_0) return 0;
            if (instruction.opcode == OpCodes.Stloc_1) return 1;
            if (instruction.opcode == OpCodes.Stloc_2) return 2;
            if (instruction.opcode == OpCodes.Stloc_3) return 3;
            if (instruction.opcode == OpCodes.Stloc_S || instruction.opcode == OpCodes.Stloc)
            {
                if (instruction.operand is LocalBuilder localBuilder) return localBuilder.LocalIndex;
                if (instruction.operand is int intIndex) return intIndex;
                if (instruction.operand is byte byteIndex) return byteIndex;
            }

            return null;
        }

        public static CodeInstruction MakeLdloca(int index)
        {
            if (index <= byte.MaxValue)
            {
                return new CodeInstruction(OpCodes.Ldloca_S, (byte)index);
            }

            return new CodeInstruction(OpCodes.Ldloca, index);
        }

        public static CodeInstruction MakeLdloc(int index)
        {
            if (index == 0) return new CodeInstruction(OpCodes.Ldloc_0);
            if (index == 1) return new CodeInstruction(OpCodes.Ldloc_1);
            if (index == 2) return new CodeInstruction(OpCodes.Ldloc_2);
            if (index == 3) return new CodeInstruction(OpCodes.Ldloc_3);
            if (index <= byte.MaxValue) return new CodeInstruction(OpCodes.Ldloc_S, (byte)index);
            return new CodeInstruction(OpCodes.Ldloc, index);
        }

        public static CodeInstruction MakeStloc(int index)
        {
            if (index == 0) return new CodeInstruction(OpCodes.Stloc_0);
            if (index == 1) return new CodeInstruction(OpCodes.Stloc_1);
            if (index == 2) return new CodeInstruction(OpCodes.Stloc_2);
            if (index == 3) return new CodeInstruction(OpCodes.Stloc_3);
            if (index <= byte.MaxValue) return new CodeInstruction(OpCodes.Stloc_S, (byte)index);
            return new CodeInstruction(OpCodes.Stloc, index);
        }

        // Label clamp helper removed in favor of direct injection.

        public static bool IsLdcI4(CodeInstruction instruction)
        {
            OpCode opcode = instruction.opcode;
            return opcode == OpCodes.Ldc_I4
                || opcode == OpCodes.Ldc_I4_S
                || opcode == OpCodes.Ldc_I4_0
                || opcode == OpCodes.Ldc_I4_1
                || opcode == OpCodes.Ldc_I4_2
                || opcode == OpCodes.Ldc_I4_3
                || opcode == OpCodes.Ldc_I4_4
                || opcode == OpCodes.Ldc_I4_5
                || opcode == OpCodes.Ldc_I4_6
                || opcode == OpCodes.Ldc_I4_7
                || opcode == OpCodes.Ldc_I4_8
                || opcode == OpCodes.Ldc_I4_M1;
        }

        public static bool LoggedContains(string key)
        {
            return Logged.Contains(key);
        }

        public static void LoggedAdd(string key)
        {
            Logged.Add(key);
        }
    }
}
