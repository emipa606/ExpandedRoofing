using HarmonyLib;
using Verse;

namespace ExpandedRoofing.HarmonyPatches;

[HarmonyPatch(typeof(RoofGrid), nameof(RoofGrid.SetRoof))]
public static class RoofGrid_SetRoof
{
    public static void RoofLeavings(RoofGrid __instance, IntVec3 c, RoofDef def, Map ___map)
    {
        var roofDef = __instance.RoofAt(c);
        if (roofDef != null && def != roofDef)
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