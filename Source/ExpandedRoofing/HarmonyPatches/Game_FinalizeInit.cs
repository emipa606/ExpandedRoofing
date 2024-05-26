using HarmonyLib;
using Verse;

namespace ExpandedRoofing.HarmonyPatches;

[HarmonyPatch(typeof(Game), nameof(Game.FinalizeInit))]
public static class Game_FinalizeInit
{
    public static void Postfix()
    {
        if (!ExpandedRoofingMod.settings.roofMaintenance)
        {
            AccessTools.Method(typeof(DefDatabase<JobDef>), "Remove")
                .Invoke(null, [JobDefOf.PerformRoofMaintenance]);
        }

        if (!ExpandedRoofingMod.GlassLights)
        {
            return;
        }

        var methodInfo = AccessTools.Method(typeof(DefDatabase<ThingDef>), "Remove");
        var named = DefDatabase<ThingDef>.GetNamed("Glass");
        var roofTransparentFraming = ThingDefOf.RoofTransparentFraming;
        if (named == null)
        {
            Log.Error("ExpandedRoofing: Error with configuring defs with Glass+Lights");
            return;
        }

        methodInfo.Invoke(null, [ThingDefOf.RoofTransparentFraming]);
        roofTransparentFraming.costList = [new ThingDefCountClass(named, 1)];
        DefDatabase<ThingDef>.Add(roofTransparentFraming);
        Log.Message("ExpandedRoofing: Glass+Lights configuration done.");
        ExpandedRoofingMod.GlassLights = true;
    }
}