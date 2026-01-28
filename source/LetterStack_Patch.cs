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

        private static MethodBase TargetMethod()
        {
            MethodBase method = AccessTools.Method(typeof(Verse.Letter), "DrawButtonAt", new[] { typeof(float) });
            if (method == null)
            {
                Logger.Error("Letter.DrawButtonAt not found; patch failed.");
            }

            return method;
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = new List<CodeInstruction>(instructions);
            list = Letter_Transpiler_Util.ReplaceScreenWidth(
                list,
                "Letter.DrawButtonAt");

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

        // Label clamp injected directly before label background draw.
    }

    [HarmonyPatch]
    internal static class Letter_CheckForMouseOverTextAt_Patch
    {
        private static MethodBase TargetMethod()
        {
            MethodBase method = AccessTools.Method(typeof(Verse.Letter), "CheckForMouseOverTextAt", new[] { typeof(float) });
            if (method == null)
            {
                Log.Error("Left Side Alerts: Letter.CheckForMouseOverTextAt not found; patch failed.");
            }

            return method;
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
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
                    list[i + 1] = new CodeInstruction(OpCodes.Ldc_R4, 38f);
                    list[i + 2] = new CodeInstruction(OpCodes.Add);
                    list[i + 3] = new CodeInstruction(OpCodes.Ldc_R4, 10f);
                    list[i + 4] = new CodeInstruction(OpCodes.Add);
                    adjusted = true;
                    break;
                }
            }

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
            }
            return list;
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

        // Label clamp helper removed in favor of direct injection.

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
