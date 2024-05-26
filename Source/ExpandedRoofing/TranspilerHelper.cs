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
        var text = curRoof.defName.Replace("ThickStoneRoof", "");
        var thingDef = text != "Jade"
            ? DefDatabase<ThingDef>.GetNamed($"Blocks{text}", false)
            : DefDatabase<ThingDef>.GetNamed(text, false);
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
            if (!thingOwner.TryDrop(thingOwner[0], list[num2], map, ThingPlaceMode.Near, out var _))
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

    public static bool IsBuildableThickRoof(IntVec3 cell, Map map)
    {
        return cell.GetRoof(map) != RimWorld.RoofDefOf.RoofRockThick;
    }

    public static void FixRoofedPowerOutputFactor(CompPowerPlantSolar comp, IntVec3 c, ref int coveredCells)
    {
        var roofDef = comp.parent.Map.roofGrid.RoofAt(c);
        if (roofDef != null && roofDef != RoofDefOf.RoofTransparent)
        {
            coveredCells++;
        }
    }
}