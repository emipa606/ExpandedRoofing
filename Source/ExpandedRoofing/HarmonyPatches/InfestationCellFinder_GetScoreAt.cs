using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace ExpandedRoofing.HarmonyPatches;

[HarmonyPatch(typeof(InfestationCellFinder), "GetScoreAt")]
public static class InfestationCellFinder_GetScoreAt
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        var MI_IsBuildableThickroof =
            AccessTools.Method(typeof(TranspilerHelper), nameof(TranspilerHelper.IsBuildableThickRoof));
        var instructionList = instructions.ToList();
        int i;
        for (i = 0; i < instructionList.Count - 2 && !(instructionList[i + 2].opcode == OpCodes.Ldc_I4_6); i++)
        {
            yield return instructionList[i];
        }

        yield return new CodeInstruction(OpCodes.Ldarg_0);
        yield return new CodeInstruction(OpCodes.Ldarg_1);
        yield return new CodeInstruction(OpCodes.Call, MI_IsBuildableThickroof);
        var @continue = il.DefineLabel();
        yield return new CodeInstruction(OpCodes.Brfalse, @continue);
        yield return new CodeInstruction(OpCodes.Ldc_R4, 0f);
        yield return new CodeInstruction(OpCodes.Ret);
        instructionList[i].labels.Add(@continue);
        for (; i < instructionList.Count; i++)
        {
            yield return instructionList[i];
        }
    }
}