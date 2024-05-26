using HarmonyLib;
using Verse;

namespace ExpandedRoofing.HarmonyPatches;

[HarmonyPatch(typeof(GlowGrid), nameof(GlowGrid.GroundGlowAt))]
public static class GlowGrid_GroundGlowAt
{
    public static void Postfix(ref float __result, IntVec3 c, Map ___map)
    {
        var newResult = 0f;
        if (TranspilerHelper.CheckTransparency(___map, c, ref newResult) && newResult > __result)
        {
            __result = newResult;
        }
    }
}