using HarmonyLib;
using RimWorld;
using Verse;

namespace ExpandedRoofing.HarmonyPatches;

[HarmonyPatch(typeof(InfestationCellFinder), "GetScoreAt")]
public static class InfestationCellFinder_GetScoreAt
{
    // Zero the infestation score under player-built (buildable) thick stone roofs. Natural overhead
    // mountain (RoofRockThick) has no RoofExtension, so IsBuildableThickRoof leaves it untouched.
    public static void Postfix(IntVec3 cell, Map map, ref float __result)
    {
        if (__result > 0f && cell.GetRoof(map).IsBuildableThickRoof())
        {
            __result = 0f;
        }
    }
}
