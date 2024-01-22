﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace RottingGraphicRecolor {
    [StaticConstructorOnStartup]
    public class RottingGraphicRecolor {
        static RottingGraphicRecolor() {
            Log.Message("[RottingGraphicRecolor] Now active");

            var harmony = new Harmony("kaitorisenkou.RottingGraphicRecolor");
            //ManualPatch(harmony);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("[RottingGraphicRecolor] Harmony patch complete!");
        }
    }

    [HarmonyPatch(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveAllGraphics))]
    public static class Patch_ResolveAllGraphics {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var instructionList = instructions.ToList();
            instructionList.InsertRange(0, new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld,AccessTools.Field(typeof(PawnGraphicSet),nameof(PawnGraphicSet.pawn))),
                new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(Patch_ResolveAllGraphics),nameof(SetRottingColor)))
            });

            int stage = 0;
            var targetInfo = AccessTools.Field(typeof(PawnGraphicSet), nameof(PawnGraphicSet.RottingColorDefault));
            var instructionToSwap = new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(Patch_ResolveAllGraphics), nameof(rottingColor)));
            for (int i = 0; i < instructionList.Count; i++) {
                if (instructionList[i].opcode == OpCodes.Ldsfld && (FieldInfo)instructionList[i].operand == targetInfo) {
                    var ins = new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(Patch_ResolveAllGraphics), nameof(rottingColor)));
                    ins.labels = instructionList[i].labels;
                    instructionList[i] = ins;
                    stage++;
                    //break;
                }
            }
            if (stage < 1) {
                Log.Error("[RottingGraphicRecolor] Patch_ResolveAllGraphics failed (stage:" + stage + ")");
            } else {
                Log.Message("[RottingGraphicRecolor] Patch_ResolveAllGraphics :" + stage);
            }
            return instructionList;
        }

        public static Color rottingColor = new Color(0.34f, 0.32f, 0.3f);
        public static void SetRottingColor(Pawn pawn) {
            var geneTracker = pawn.genes;
            if (geneTracker == null) {
                rottingColor = PawnGraphicSet.RottingColorDefault;
                return;
            }
            var gene = geneTracker.GenesListForReading.FirstOrDefault(t => t.def.HasModExtension<GeneOverrideRottingColor>());
            if (gene == null) {
                rottingColor = PawnGraphicSet.RottingColorDefault;
                return;
            }
            rottingColor = gene.def.GetModExtension<GeneOverrideRottingColor>().GetColor();
            return;
        }
    }
}