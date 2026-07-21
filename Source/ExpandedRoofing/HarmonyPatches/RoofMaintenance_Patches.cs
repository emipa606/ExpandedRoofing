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
        var instructionsList = instructions.ToList();
        if (!instructionsList.Any(ci => ci.opcode == OpCodes.Bne_Un_S))
        {
            Log.Error("[ExpandedRoofing] SetRoof transpiler anchor not found — maintenance tracking disabled");
            foreach (var ci in instructionsList)
            {
                yield return ci;
            }

            yield break;
        }

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
        // Injected before SetRoof writes the array, so RoofAt(c) still returns the OLD roof here.
        var oldRoof = map.roofGrid.RoofAt(c);

        // Roof removal / collapse (def == null): purge any tracked cell so the grid never leaks.
        if (def == null)
        {
            if (oldRoof.IsBuildableThickRoof())
            {
                map.GetComponent<RoofMaintenance_MapComponenent>()?.RemoveMaintainableRoof(c);
            }

            return;
        }

        if (def.IsBuildableThickRoof())
        {
            map.GetComponent<RoofMaintenance_MapComponenent>()?.AddMaintainableRoof(c);
            return;
        }

        if (oldRoof.IsBuildableThickRoof())
        {
            map.GetComponent<RoofMaintenance_MapComponenent>()?.RemoveMaintainableRoof(c);
        }
    }
}