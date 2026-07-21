using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ExpandedRoofing;

[StaticConstructorOnStartup]
internal class DynamicDefs
{
    private static readonly MethodInfo miNewBlueprintDefThing;

    private static readonly MethodInfo miNewFrameDefThing;

    static DynamicDefs()
    {
        miNewBlueprintDefThing = AccessTools.Method(typeof(ThingDefGenerator_Buildings), "NewBlueprintDef_Thing");
        miNewFrameDefThing = AccessTools.Method(typeof(ThingDefGenerator_Buildings), "NewFrameDef_Thing");
        Log.Message("ExpandedRoofing: generating dynamic defs");
        InjectedDefHasher.PrepareReflection();
        impliedBlueprintAndFrameDefs(ThingDefOf.RoofTransparentFraming);
        impliedBlueprintAndFrameDefs(ThingDefOf.RoofSolarFraming);
        impliedBlueprintAndFrameDefs(ThingDefOf.ThickStoneRoofFraming);

        foreach (var thingDef in DefDatabase<ThingDef>.AllDefsListForReading.Where(def =>
                     def.IsStuff &&
                     def.stuffProps?.categories?.Any(categoryDef => categoryDef == StuffCategoryDefOf.Stony) ==
                     true &&
                     def.modContentPack?.IsOfficialMod == false))
        {
            var newRoof = new RoofDef
            {
                isThickRoof = true,
                collapseLeavingThingDef = thingDef,
                defName = $"{thingDef.defName.Replace("Blocks", "")}ThickStoneRoof",
                label = $"{thingDef.LabelCap.Replace("blocks", "").Trim()} Thick Stone Roof",
                modContentPack = thingDef.modContentPack,
                modExtensions =
                [
                    new RoofExtension { spawnerDef = ThingDefOf.ThickStoneRoofFraming, sourceStuff = thingDef }
                ]
            };

            DefGenerator.AddImpliedDef(newRoof);
            InjectedDefHasher.GiveShortHasToDef(newRoof, typeof(RoofDef));
        }

        // H-12: RoofGrid scribes roofs BY shortHash and resolves them on load via
        // DefDatabase<RoofDef>.GetByShortHash, which reads defsByShortHash — a dictionary vanilla
        // builds ONCE (ShortHashGiver.GiveAllShortHashes), before StaticConstructorOnStartup runs.
        // Defs generated here therefore never enter it, and every dynamically generated stone roof
        // was silently deleted from the map on the next save/load. Rebuilding the dictionary after
        // generation (public + idempotent) registers them.
        DefDatabase<RoofDef>.InitializeShortHashDictionary();
    }

    private static void impliedBlueprintAndFrameDefs(ThingDef thingDef)
    {
        var thingDef2 = miNewBlueprintDefThing.Invoke(null, [thingDef, false, null, false]) as ThingDef;
        InjectedDefHasher.GiveShortHasToDef(thingDef2, typeof(ThingDef));
        DefDatabase<ThingDef>.Add(thingDef2);
        thingDef2 = miNewFrameDefThing.Invoke(null, [thingDef, false]) as ThingDef;
        InjectedDefHasher.GiveShortHasToDef(thingDef2, typeof(ThingDef));
        if (thingDef.MadeFromStuff)
        {
            thingDef2?.stuffCategories = thingDef.stuffCategories;
        }

        DefDatabase<ThingDef>.Add(thingDef2);
    }
}