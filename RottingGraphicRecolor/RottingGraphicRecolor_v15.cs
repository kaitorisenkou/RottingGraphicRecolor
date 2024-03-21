#if v15

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;

namespace RottingGraphicRecolor {
    [StaticConstructorOnStartup]
    public class RottingGraphicRecolor {
        public static MethodInfo methodInfo_GetRottenColor;
        static RottingGraphicRecolor() {
            Log.Message("[RottingGraphicRecolor] Now active");
            methodInfo_GetRottenColor = AccessTools.Method(typeof(PawnRenderUtility), nameof(PawnRenderUtility.GetRottenColor));
            var harmony = new Harmony("kaitorisenkou.RottingGraphicRecolor");
            harmony.Patch(
                AccessTools.Method(typeof(PawnRenderNode), nameof(PawnRenderNode.ColorFor), null, null),
                null,
                null,
                new HarmonyMethod(typeof(RottingGraphicRecolor), nameof(Patch_RenderNodeColorFor), null),
                null
                );
            harmony.Patch(
                AccessTools.PropertyGetter(typeof(Pawn_StoryTracker), "HairColor"),
                null,
                null,
                new HarmonyMethod(typeof(RottingGraphicRecolor), nameof(Patch_StoryTrackerGetColor), null),
                null
                );
            harmony.Patch(
                AccessTools.PropertyGetter(typeof(Apparel), "DrawColor"),
                null,
                null,
                new HarmonyMethod(typeof(RottingGraphicRecolor), nameof(Patch_ApparelColor), null),
                null
                );
            Log.Message("[RottingGraphicRecolor] Harmony patch complete!");
        }

        public static Color GetRottingColor(Color baseColor, Pawn pawn) {
            if (pawn == null) {
                return PawnRenderUtility.GetRottenColor(baseColor);
            }
            var gene = pawn.genes.GenesListForReading.FirstOrDefault(t => t.Active && t.def.HasModExtension<GeneOverrideRottingColor>());
            if (gene == null) {
                return PawnRenderUtility.GetRottenColor(baseColor);
            }
            var ext = gene.def.GetModExtension<GeneOverrideRottingColor>();
            Color result = baseColor * 0.25f;
            return Color.Lerp(baseColor, ext.GetColor(), 0.75f);
        }


        public static IEnumerable<CodeInstruction> Patch_RenderNodeColorFor(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            var instructionList = instructions.ToList();

            int stage = 0;
            for (int i = 0; i < instructionList.Count; i++) {
                if (instructionList[i].opcode == OpCodes.Call && (MethodInfo)instructionList[i].operand== methodInfo_GetRottenColor) {
                    stage++;
                    instructionList.RemoveAt(i);
                    instructionList.InsertRange(i, new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(RottingGraphicRecolor),nameof(GetRottingColor)))
                    });
                }
            }
            if (stage < 1) {
                Log.Error("[RottingGraphicRecolor] Patch_RenderNodeColorFor failed (stage:" + stage + ")");
            }
            return instructionList;
        }
        public static IEnumerable<CodeInstruction> Patch_StoryTrackerGetColor(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            var instructionList = instructions.ToList();

            int stage = 0;
            for (int i = 0; i < instructionList.Count; i++) {
                if (instructionList[i].opcode == OpCodes.Call && (MethodInfo)instructionList[i].operand == methodInfo_GetRottenColor) {
                    stage++;
                    instructionList.RemoveAt(i);
                    instructionList.InsertRange(i, new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld,AccessTools.Field(typeof(Pawn_StoryTracker),"pawn")),
                        new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(RottingGraphicRecolor),nameof(GetRottingColor)))
                    });
                }
            }
            if (stage < 1) {
                Log.Error("[RottingGraphicRecolor] Patch_StoryTrackerGetColor failed (stage:" + stage + ")");
            }
            return instructionList;
        }
        public static IEnumerable<CodeInstruction> Patch_ApparelColor(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            var instructionList = instructions.ToList();

            int stage = 0;
            for (int i = 0; i < instructionList.Count; i++) {
                if (instructionList[i].opcode == OpCodes.Call && (MethodInfo)instructionList[i].operand == methodInfo_GetRottenColor) {
                    stage++;
                    instructionList.RemoveAt(i);
                    instructionList.InsertRange(i, new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(Apparel),"get_Wearer")),
                        new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(RottingGraphicRecolor),nameof(GetRottingColor)))
                    });
                }
            }
            if (stage < 1) {
                Log.Error("[RottingGraphicRecolor] Patch_ApparelColor failed (stage:" + stage + ")");
            }
            return instructionList;
        }
    }
}

#endif