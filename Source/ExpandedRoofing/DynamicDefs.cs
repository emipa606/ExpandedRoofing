using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ExpandedRoofing;

[StaticConstructorOnStartup]
internal class DynamicDefs
{
    private static readonly MethodInfo MI_NewBlueprintDef_Thing;

    private static readonly MethodInfo MI_NewFrameDef_Thing;

    static DynamicDefs()
    {
        MI_NewBlueprintDef_Thing = AccessTools.Method(typeof(ThingDefGenerator_Buildings), "NewBlueprintDef_Thing");
        MI_NewFrameDef_Thing = AccessTools.Method(typeof(ThingDefGenerator_Buildings), "NewFrameDef_Thing");
        Log.Message("ExpandedRoofing: generating dynamic defs");
        InjectedDefHasher.PrepareReflection();
        ImpliedBlueprintAndFrameDefs(ThingDefOf.RoofTransparentFraming);
        ImpliedBlueprintAndFrameDefs(ThingDefOf.RoofSolarFraming);
        ImpliedBlueprintAndFrameDefs(ThingDefOf.ThickStoneRoofFraming);
    }

    private static void ImpliedBlueprintAndFrameDefs(ThingDef thingDef)
    {
        var thingDef2 = MI_NewBlueprintDef_Thing.Invoke(null, [thingDef, false, null, false]) as ThingDef;
        InjectedDefHasher.GiveShortHasToDef(thingDef2, typeof(ThingDef));
        DefDatabase<ThingDef>.Add(thingDef2);
        thingDef2 = MI_NewFrameDef_Thing.Invoke(null, [thingDef, false]) as ThingDef;
        InjectedDefHasher.GiveShortHasToDef(thingDef2, typeof(ThingDef));
        if (thingDef.MadeFromStuff)
        {
            if (thingDef2 != null)
            {
                thingDef2.stuffCategories = thingDef.stuffCategories;
            }
        }

        DefDatabase<ThingDef>.Add(thingDef2);
    }
}