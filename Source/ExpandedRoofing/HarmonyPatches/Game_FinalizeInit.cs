using HarmonyLib;
using Verse;

namespace ExpandedRoofing.HarmonyPatches;

[HarmonyPatch(typeof(Game), nameof(Game.FinalizeInit))]
public static class Game_FinalizeInit
{
    private static bool defSurgeryDone;

    public static void Postfix()
    {
        // Def surgery is idempotent but ordering-fragile, so run it exactly once per game session.
        if (defSurgeryDone)
        {
            return;
        }

        defSurgeryDone = true;

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
    }
}
