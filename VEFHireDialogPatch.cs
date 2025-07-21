using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using Verse;
using RimWorld;
using HarmonyLib;
#if v15
using VFECore.Misc;
#else
using VEF.Planet;
#endif
using UnityEngine;
using Verse.Noise;
using System.Reflection;

namespace VEFHireDialogPatch {
    [StaticConstructorOnStartup]
    public class VEFHireDialogPatch {
        static VEFHireDialogPatch() {
            Log.Message("[VEFHireDialogPatch] Now active");

            var harmony = new Harmony("kaitorisenkou.VEFHireDialogPatch");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("[VEFHireDialogPatch] Harmony patch complete!");
        }

    }

    [HarmonyPatch(typeof(Dialog_Hire), "DoWindowContents")]
    public static class Patch_Dialog_Hire_DoWindowContents {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            var instructionList = instructions.ToList();
            int i = 0;
            //Log.Message("0:"+instructionList.Count);
            do { i++; } while (instructionList[i].opcode != OpCodes.Ldloca_S);
            object operand_InRect = instructionList[i].operand;
            do { i++; } while (instructionList[i].opcode != OpCodes.Br_S);
            do { i--; } while (instructionList[i].opcode != OpCodes.Ldarg_0);
            instructionList.InsertRange(i, new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldloca_S,operand_InRect),
                new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(Patch_Dialog_Hire_DoWindowContents),nameof(HireableScrollBegin)))
            });
            //Log.Message("1:"+i);

#if v15
            MethodInfo methodInfo = AccessTools.Method(typeof(VFECore.UItils.UIUtility), nameof(VFECore.UItils.UIUtility.TakeTopPart));
#else
            MethodInfo methodInfo = AccessTools.Method(typeof(VEF.Utils.UIUtility), nameof(VEF.Utils.UIUtility.TakeTopPart));
#endif
            do { i++; } while (instructionList[i].opcode != OpCodes.Call || (MethodInfo)instructionList[i].operand != methodInfo);
            instructionList.InsertRange(i-3, new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldloca_S,operand_InRect),
                new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(Patch_Dialog_Hire_DoWindowContents),nameof(HireableScrollEnd)))
            });
            //Log.Message("2:" + i);
            return instructionList;
        }

        static Vector2 scrollPos = new Vector2();
        static Rect rectInner = new Rect(0, 0, 700, 190);
        static void HireableScrollBegin(ref Rect inRect) {
            Rect rectOuter = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - 200);
            inRect.yMin -= 70;
            Widgets.BeginScrollView(rectOuter, ref scrollPos, rectInner);
        }
        static void HireableScrollEnd(ref Rect inRect) {
            rectInner = new Rect(0, 0, inRect.width - 16f, inRect.yMin);
            Widgets.EndScrollView();
            inRect = new Rect(0, 430, inRect.width, 190);
            //Log.Message("inRect:"+inRect);
        }
    }
}
