using HarmonyLib;
using RimWorld;
using Verse;

namespace ExpandedRoofing.HarmonyPatches;

[HarmonyPatch(typeof(SanguophageUtility), nameof(SanguophageUtility.InSunlight))]
public static class SanguophageUtility_InSunlight
{
    private static void Postfix(ref bool __result, IntVec3 cell, Map map)
    {
        if (__result || !cell.InBounds(map))
        {
            return;
        }

        var roof = cell.GetRoof(map);

        if (roof == RoofDefOf.RoofTransparent)
        {
            __result = map.skyManager.CurSkyGlow > 0.1f;
        }
    }
}