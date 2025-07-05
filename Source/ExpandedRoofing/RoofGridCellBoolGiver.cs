using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace ExpandedRoofing;

[StaticConstructorOnStartup]
public class RoofGridCellBoolGiver
{
    private static readonly Color ThinRockRoofColor;

    private static readonly Color ThickRockRoofColor;

    private static readonly Color LightGreen;

    static RoofGridCellBoolGiver()
    {
        ThinRockRoofColor = new Color(0.6f, 0.6f, 0.6f);
        ThickRockRoofColor = new Color(0.75f, 0.375f, 0.25f);
        LightGreen = new Color(0.3f, 1f, 0.4f);
        var harmony = new Harmony("rimworld.whyisthat.expandedroofing.roofgridcellboolgiver");
        harmony.Patch(AccessTools.Property(typeof(RoofGrid), "Color").GetGetMethod(),
            new HarmonyMethod(typeof(RoofGridCellBoolGiver), "RoofGridColorDetour"));
        harmony.Patch(AccessTools.Method(typeof(RoofGrid), "GetCellExtraColor"), null, null,
            new HarmonyMethod(typeof(RoofGridCellBoolGiver).GetMethod("RoofGridExtraColorDetour")));
    }

    public static bool RoofGridColorDetour(RoofGrid __instance, ref Color __result)
    {
        __result = Color.white;
        return false;
    }

    public static IEnumerable<CodeInstruction> RoofGridExtraColorDetour(IEnumerable<CodeInstruction> instructions,
        ILGenerator il)
    {
        var fiRoofGridRoofGrid = AccessTools.Field(typeof(RoofGrid), "roofGrid");
        var miGetCellExtraColor = typeof(RoofGridCellBoolGiver).GetMethod("GetCellExtraColor");
        yield return new CodeInstruction(OpCodes.Ldarg_0);
        yield return new CodeInstruction(OpCodes.Ldfld, fiRoofGridRoofGrid);
        yield return new CodeInstruction(OpCodes.Ldarg_1);
        yield return new CodeInstruction(OpCodes.Ldelem_Ref);
        yield return new CodeInstruction(OpCodes.Call, miGetCellExtraColor);
        yield return new CodeInstruction(OpCodes.Ret);
    }

    public static Color GetCellExtraColor(RoofDef roofCell)
    {
        if (roofCell == RimWorld.RoofDefOf.RoofConstructed)
        {
            return LightGreen;
        }

        if (roofCell == RoofDefOf.RoofTransparent)
        {
            return Color.yellow;
        }

        if (roofCell == RoofDefOf.RoofSolar)
        {
            return Color.cyan;
        }

        if (roofCell == RimWorld.RoofDefOf.RoofRockThin)
        {
            return ThinRockRoofColor;
        }

        return roofCell == RimWorld.RoofDefOf.RoofRockThick ? ThickRockRoofColor : Color.magenta;
    }
}