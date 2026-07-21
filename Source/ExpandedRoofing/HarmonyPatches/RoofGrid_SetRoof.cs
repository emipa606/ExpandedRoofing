using HarmonyLib;
using Verse;

namespace ExpandedRoofing.HarmonyPatches;

[HarmonyPatch(typeof(RoofGrid), nameof(RoofGrid.SetRoof))]
public static class RoofGrid_SetRoof
{
    // Runs as a PREFIX: RoofGrid.SetRoof writes the internal roof array before any postfix would run,
    // so we must read the OLD roof here (pre-write) to drop leavings and unregister solar cells.
    public static void Prefix(RoofGrid __instance, IntVec3 c, RoofDef def, Map ___map)
    {
        var roofDef = __instance.RoofAt(c);
        if (roofDef == def)
        {
            return;
        }

        if (roofDef != null)
        {
            var modExtension = roofDef.GetModExtension<RoofExtension>();
            if (modExtension != null)
            {
                TranspilerHelper.DoLeavings(roofDef, modExtension.spawnerDef, ___map,
                    GenAdj.OccupiedRect(c, Rot4.North, modExtension.spawnerDef.size));
            }

            if (roofDef == RoofDefOf.RoofSolar)
            {
                ___map?.GetComponent<SolarRoofing_MapComponent>().tracker.RemoveSolarCell(c);
            }
        }

        if (def == RoofDefOf.RoofSolar)
        {
            ___map?.GetComponent<SolarRoofing_MapComponent>().tracker.AddSolarCell(c);
        }
    }
}
