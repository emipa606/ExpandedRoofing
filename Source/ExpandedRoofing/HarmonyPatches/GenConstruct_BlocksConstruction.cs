using HarmonyLib;
using Verse;

namespace ExpandedRoofing.HarmonyPatches;

[HarmonyPatch(typeof(RimWorld.GenConstruct), nameof(RimWorld.GenConstruct.BlocksConstruction))]
public static class GenConstruct_BlocksConstruction
{
    // Plants (including trees) never block this mod's roof framing, so a frame can be built over grass/
    // bushes/trees without a clear job. Everything else falls through to vanilla, which restores normal
    // tree-blocking for all other construction.
    public static bool Prefix(Thing constructible, Thing t, ref bool __result)
    {
        if (t?.def?.category != ThingCategory.Plant)
        {
            return true;
        }

        var d = constructible?.def;
        if (d == null)
        {
            return true;
        }

        var built = d.entityDefToBuild as ThingDef ?? d;
        if (built == ThingDefOf.RoofTransparentFraming ||
            built == ThingDefOf.RoofSolarFraming ||
            built == ThingDefOf.ThickStoneRoofFraming)
        {
            __result = false;
            return false;
        }

        return true;
    }
}
