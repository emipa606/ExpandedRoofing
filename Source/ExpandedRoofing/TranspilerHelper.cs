using System.Linq;
using RimWorld;
using Verse;

namespace ExpandedRoofing;

internal static class TranspilerHelper
{
    public static bool CheckTransparency(Map map, IntVec3 c, ref float num)
    {
        var roofExtension = map.roofGrid.RoofAt(c)?.GetModExtension<RoofExtension>();
        if (roofExtension == null)
        {
            return false;
        }

        num = map.skyManager.CurSkyGlow * roofExtension.transparency;
        return true;
    }

    private static int KillFinalize(int count)
    {
        return GenMath.RoundRandom(count * 0.5f);
    }

    public static void DoLeavings(RoofDef curRoof, ThingDef spawnerDef, Map map, CellRect leavingsRect)
    {
        if (curRoof.defName == "ThickStoneRoof")
        {
            return;
        }

        var thingOwner = new ThingOwner<Thing>();

        // Prefer the source stuff recorded on the roof's extension (set on dynamically generated roofs);
        // fall back to the defName string convention for XML-authored vanilla-stone roofs.
        var thingDef = curRoof.GetModExtension<RoofExtension>()?.sourceStuff;
        if (thingDef == null)
        {
            var text = curRoof.defName.Replace("ThickStoneRoof", "");
            thingDef = text != "Jade"
                ? DefDatabase<ThingDef>.GetNamed($"Blocks{text}", false)
                : DefDatabase<ThingDef>.GetNamed(text, false);
        }

        // Only stuff-built spawners (thick stone framing) need a resolved material; solar/transparent
        // framings have fixed cost lists and CostListAdjusted(null) is valid for them.
        if (thingDef == null && spawnerDef.MadeFromStuff)
        {
            Log.Error($"ExpandedRoofing: could not resolve source material for roof '{curRoof.defName}'; " +
                      "no leavings dropped.");
            return;
        }

        foreach (var item in spawnerDef.CostListAdjusted(thingDef))
        {
            var num = KillFinalize(item.count);
            if (num <= 0)
            {
                continue;
            }

            var thing = ThingMaker.MakeThing(item.thingDef);
            thing.stackCount = num;
            thingOwner.TryAdd(thing);
        }

        var list = leavingsRect.Cells.InRandomOrder().ToList();
        var num2 = 0;
        while (thingOwner.Count > 0)
        {
            if (!thingOwner.TryDrop(thingOwner[0], list[num2], map, ThingPlaceMode.Near, out _))
            {
                Log.Warning($"Failed to place all leavings for destroyed thing {curRoof} at {leavingsRect.CenterCell}");
                break;
            }

            if (++num2 >= list.Count)
            {
                num2 = 0;
            }
        }
    }

    public static bool SkipRoofRendering(RoofDef roofDef)
    {
        return roofDef == RoofDefOf.RoofTransparent;
    }

}