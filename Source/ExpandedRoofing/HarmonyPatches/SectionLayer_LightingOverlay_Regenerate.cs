using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace ExpandedRoofing.HarmonyPatches;

[HarmonyPatch(typeof(SectionLayer_LightingOverlay), nameof(SectionLayer_LightingOverlay.Regenerate))]
public static class SectionLayer_LightingOverlay_Regenerate
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        AccessTools.Method(typeof(RoofGrid), nameof(RoofGrid.RoofAt), [typeof(int), typeof(int)]);
        var MI_SkipRoofRendering =
            AccessTools.Method(typeof(TranspilerHelper), nameof(TranspilerHelper.SkipRoofRendering));
        var instructionList = instructions.ToList();
        for (var j = 0; j < instructionList.Count; j++)
        {
            yield return instructionList[j];
            if (!(instructionList[j].opcode == OpCodes.Stloc_S) ||
                instructionList[j].operand is not LocalBuilder localBuilder || localBuilder.LocalIndex !=
                HarmonyPatches.SectionLayer_LightingOverlay__Regenerate__RoofDef__LocalIndex)
            {
                continue;
            }

            var num = j + 1;
            j = num;
            yield return instructionList[num];
            if (instructionList[j].opcode != OpCodes.Ldloc_S)
            {
                break;
            }

            var load = new CodeInstruction(instructionList[j].opcode, instructionList[j].operand);
            num = j + 1;
            j = num;
            yield return instructionList[num];
            if (instructionList[j].opcode != OpCodes.Brfalse_S)
            {
                break;
            }

            yield return load;
            yield return new CodeInstruction(OpCodes.Call, MI_SkipRoofRendering);
            var @continue = il.DefineLabel();
            yield return new CodeInstruction(OpCodes.Brtrue_S, @continue);
            while (true)
            {
                num = j + 1;
                j = num;
                if (!(instructionList[num].opcode != OpCodes.Stloc_S))
                {
                    break;
                }

                yield return instructionList[j];
            }

            yield return instructionList[j++];
            instructionList[j].labels.Add(@continue);
            yield return instructionList[j];
        }
    }
}