using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace ExpandedRoofing.HarmonyPatches;

[StaticConstructorOnStartup]
internal class RoofMaintenance_Patches
{
    static RoofMaintenance_Patches()
    {
        new Harmony("rimworld.whyisthat.expandedroofing.roofmaintenance").Patch(
            AccessTools.Method(typeof(RoofGrid), nameof(RoofGrid.SetRoof)), null, null,
            new HarmonyMethod(typeof(RoofMaintenance_Patches), nameof(SetRoofTranspiler)));
    }

    public static IEnumerable<CodeInstruction> SetRoofTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        AccessTools.Field(typeof(RoofGrid), "roofGrid");
        var instructionsList = instructions.ToList();
        int i;
        for (i = 0; i < instructionsList.Count - 1; i++)
        {
            if (instructionsList[i].opcode == OpCodes.Bne_Un_S)
            {
                yield return instructionsList[i++];
                yield return instructionsList[i++];
                break;
            }

            yield return instructionsList[i];
        }

        yield return instructionsList[i++];
        yield return new CodeInstruction(OpCodes.Ldfld, HarmonyPatches.FI_RoofGrid_map);
        yield return new CodeInstruction(OpCodes.Ldarg_1);
        yield return new CodeInstruction(OpCodes.Ldarg_2);
        yield return new CodeInstruction(OpCodes.Call,
            AccessTools.Method(typeof(RoofMaintenance_Patches), nameof(SetRoofHelper)));
        yield return new CodeInstruction(OpCodes.Ldarg_0);
        for (; i < instructionsList.Count - 1; i++)
        {
            yield return instructionsList[i];
        }

        yield return instructionsList[i];
    }

    public static void SetRoofHelper(Map map, IntVec3 c, RoofDef def)
    {
        if (def == null)
        {
            return;
        }

        if (def.IsBuildableThickRoof())
        {
            map.GetComponent<RoofMaintenance_MapComponenent>()?.AddMaintainableRoof(c);
            return;
        }

        if (map.roofGrid.RoofAt(c).IsBuildableThickRoof())
        {
            map.GetComponent<RoofMaintenance_MapComponenent>()?.RemoveMaintainableRoof(c);
        }
    }
}