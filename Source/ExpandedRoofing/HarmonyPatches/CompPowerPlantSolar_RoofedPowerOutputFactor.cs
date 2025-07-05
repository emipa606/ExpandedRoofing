using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace ExpandedRoofing.HarmonyPatches;

[HarmonyPatch(typeof(CompPowerPlantSolar), "RoofedPowerOutputFactor", MethodType.Getter)]
public static class CompPowerPlantSolar_RoofedPowerOutputFactor
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var miFixRoofedPowerOutputFactor = AccessTools.Method(typeof(TranspilerHelper),
            nameof(TranspilerHelper.FixRoofedPowerOutputFactor));
        var instructionList = instructions.ToList();
        var skipping = false;
        for (var i = 0; i < instructionList.Count; i++)
        {
            if (skipping)
            {
                if (instructionList[i].opcode != OpCodes.Add || instructionList[i + 1].opcode != OpCodes.Stloc_1)
                {
                    continue;
                }

                i++;
                skipping = false;
            }
            else if (instructionList[i].opcode == OpCodes.Add && instructionList[i + 1].opcode == OpCodes.Stloc_0)
            {
                yield return instructionList[i++];
                yield return instructionList[i++];
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldloc_2);
                yield return new CodeInstruction(OpCodes.Ldloca, 1);
                yield return new CodeInstruction(OpCodes.Call, miFixRoofedPowerOutputFactor);
                skipping = true;
            }
            else
            {
                yield return instructionList[i];
            }
        }
    }
}