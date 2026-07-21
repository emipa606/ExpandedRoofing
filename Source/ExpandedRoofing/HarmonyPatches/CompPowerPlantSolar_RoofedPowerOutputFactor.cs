using HarmonyLib;
using RimWorld;
using Verse;

namespace ExpandedRoofing.HarmonyPatches;

[HarmonyPatch(typeof(CompPowerPlantSolar), "RoofedPowerOutputFactor", MethodType.Getter)]
public static class CompPowerPlantSolar_RoofedPowerOutputFactor
{
    // Recompute the factor from scratch: transparent roof counts as unroofed for vanilla solar panels.
    public static void Postfix(CompPowerPlantSolar __instance, ref float __result)
    {
        var map = __instance.parent.Map;
        if (map == null)
        {
            return;
        }

        var roofGrid = map.roofGrid;
        var total = 0;
        var covered = 0;
        foreach (var c in __instance.parent.OccupiedRect())
        {
            total++;
            var roof = roofGrid.RoofAt(c);
            if (roof != null && roof != RoofDefOf.RoofTransparent)
            {
                covered++;
            }
        }

        if (total == 0)
        {
            return;
        }

        __result = (total - covered) / (float)total;
    }
}
