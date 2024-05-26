using HarmonyLib;
using RimWorld;
using Verse;

namespace ExpandedRoofing.HarmonyPatches;

[HarmonyPatch(typeof(ListerBuildingsRepairable), nameof(ListerBuildingsRepairable.Notify_BuildingRepaired))]
public static class ListerBuildingsRepairable_Notify_BuildingRepaired
{
    public static void Postfix(Building b)
    {
        var comp = b.GetComp<CompMaintainable>();
        if (comp != null)
        {
            comp.ticksSinceMaintain = 0;
        }
    }
}