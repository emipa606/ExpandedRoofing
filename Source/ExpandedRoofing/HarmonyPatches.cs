using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ExpandedRoofing;

[StaticConstructorOnStartup]
internal class HarmonyPatches
{
    public static readonly FieldInfo FI_RoofGrid_map;

    private static readonly int SectionLayer_LightingOverlay__Regenerate__RoofDef__LocalIndex;

    static HarmonyPatches()
    {
        FI_RoofGrid_map = AccessTools.Field(typeof(RoofGrid), "map");
        SectionLayer_LightingOverlay__Regenerate__RoofDef__LocalIndex = AccessTools
            .Method(typeof(SectionLayer_LightingOverlay), "Regenerate").GetMethodBody()!.LocalVariables
            .First(lvi => lvi.LocalType == typeof(RoofDef)).LocalIndex;
        var harmony = new Harmony("rimworld.whyisthat.expandedroofing.main");
        //harmony.Patch(AccessTools.Method(typeof(GlowGrid), nameof(GlowGrid.GroundGlowAt)), null, null,
        //    new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(PlantLightingFix))));
        harmony.Patch(AccessTools.Method(typeof(GlowGrid), nameof(GlowGrid.GroundGlowAt)),
            postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(GroundGlowAt_TransparancyFix)));
        harmony.Patch(AccessTools.Method(typeof(RoofGrid), nameof(RoofGrid.SetRoof)),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(RoofLeavings)));
        harmony.Patch(
            AccessTools.Method(typeof(SectionLayer_LightingOverlay), nameof(SectionLayer_LightingOverlay.Regenerate)),
            null, null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(TransparentRoofLightingOverlayFix)));
        harmony.Patch(AccessTools.Method(typeof(InfestationCellFinder), "GetScoreAt"), null, null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(ThickRoofInfestationFix)));
        harmony.Patch(
            AccessTools.Method(typeof(ListerBuildingsRepairable),
                nameof(ListerBuildingsRepairable.Notify_BuildingRepaired)), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(BuildingRepairedPostfix)));
        harmony.Patch(AccessTools.Method(typeof(GenConstruct), nameof(GenConstruct.BlocksConstruction)), null, null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(FixClearBuildingArea)));
        harmony.Patch(AccessTools.Property(typeof(CompPowerPlantSolar), "RoofedPowerOutputFactor").GetGetMethod(true),
            null, null, new HarmonyMethod(typeof(HarmonyPatches), nameof(TransparentRoofOutputFactorFix)));
        harmony.Patch(AccessTools.Method(typeof(Game), nameof(Game.FinalizeInit)), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(GameInited)));
    }

    public static void GroundGlowAt_TransparancyFix(ref float __result, IntVec3 c, Map ___map)
    {
        var newResult = 0f;
        if (TranspilerHelper.CheckTransparency(___map, c, ref newResult) && newResult > __result)
        {
            __result = newResult;
        }
    }

    public static IEnumerable<CodeInstruction> PlantLightingFix(IEnumerable<CodeInstruction> instructions,
        ILGenerator il)
    {
        var FI_GlowGrid_map = AccessTools.Field(typeof(GlowGrid), "map");
        var MI_CheckTransparency =
            AccessTools.Method(typeof(TranspilerHelper), nameof(TranspilerHelper.CheckTransparency));
        var instructionList = instructions.ToList();
        int j;
        for (j = 0; j < instructionList.Count; j++)
        {
            if (instructionList[j].opcode == OpCodes.Ldarg_2)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0)
                {
                    labels = instructionList[j].labels
                };
                yield return new CodeInstruction(OpCodes.Dup);
                yield return new CodeInstruction(OpCodes.Ldfld, FI_GlowGrid_map);
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Ldloca, 0);
                yield return new CodeInstruction(OpCodes.Call, MI_CheckTransparency);
                var @continue = il.DefineLabel();
                yield return new CodeInstruction(OpCodes.Brfalse, @continue);
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                yield return new CodeInstruction(OpCodes.Ret);
                yield return new CodeInstruction(instructionList[j].opcode, instructionList[j].operand)
                {
                    labels = { @continue }
                };
                break;
            }

            yield return instructionList[j];
        }

        for (j++; j < instructionList.Count; j++)
        {
            yield return instructionList[j];
        }
    }

    public static void RoofLeavings(RoofGrid __instance, IntVec3 c, RoofDef def)
    {
        var roofDef = __instance.RoofAt(c);
        var map = FI_RoofGrid_map.GetValue(__instance) as Map;
        if (roofDef != null && def != roofDef)
        {
            var modExtension = roofDef.GetModExtension<RoofExtension>();
            if (modExtension != null)
            {
                TranspilerHelper.DoLeavings(roofDef, modExtension.spawnerDef, map,
                    GenAdj.OccupiedRect(c, Rot4.North, modExtension.spawnerDef.size));
            }

            if (roofDef == RoofDefOf.RoofSolar)
            {
                map?.GetComponent<SolarRoofing_MapComponent>().tracker.RemoveSolarCell(c);
            }
        }

        if (def == RoofDefOf.RoofSolar)
        {
            map?.GetComponent<SolarRoofing_MapComponent>().tracker.AddSolarCell(c);
        }
    }

    public static IEnumerable<CodeInstruction> TransparentRoofLightingOverlayFix(
        IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        AccessTools.Method(typeof(RoofGrid), nameof(RoofGrid.RoofAt), [typeof(int), typeof(int)]);
        var MI_SkipRoofRendering =
            AccessTools.Method(typeof(TranspilerHelper), nameof(TranspilerHelper.SkipRoofRendering));
        var instructionList = instructions.ToList();
        for (var j = 0; j < instructionList.Count; j++)
        {
            yield return instructionList[j];
            if (!(instructionList[j].opcode == OpCodes.Stloc_S) ||
                instructionList[j].operand is not LocalBuilder localBuilder || localBuilder.LocalIndex !=
                SectionLayer_LightingOverlay__Regenerate__RoofDef__LocalIndex)
            {
                continue;
            }

            var num = j + 1;
            j = num;
            yield return instructionList[num];
            if (instructionList[j].opcode != OpCodes.Ldloc_S)
            {
                break;
            }

            var load = new CodeInstruction(instructionList[j].opcode, instructionList[j].operand);
            num = j + 1;
            j = num;
            yield return instructionList[num];
            if (instructionList[j].opcode != OpCodes.Brfalse_S)
            {
                break;
            }

            yield return load;
            yield return new CodeInstruction(OpCodes.Call, MI_SkipRoofRendering);
            var @continue = il.DefineLabel();
            yield return new CodeInstruction(OpCodes.Brtrue_S, @continue);
            while (true)
            {
                num = j + 1;
                j = num;
                if (!(instructionList[num].opcode != OpCodes.Stloc_S))
                {
                    break;
                }

                yield return instructionList[j];
            }

            yield return instructionList[j++];
            instructionList[j].labels.Add(@continue);
            yield return instructionList[j];
        }
    }

    public static IEnumerable<CodeInstruction> ThickRoofInfestationFix(IEnumerable<CodeInstruction> instructions,
        ILGenerator il)
    {
        var MI_IsBuildableThickroof =
            AccessTools.Method(typeof(TranspilerHelper), nameof(TranspilerHelper.IsBuildableThickRoof));
        var instructionList = instructions.ToList();
        int i;
        for (i = 0; i < instructionList.Count - 2 && !(instructionList[i + 2].opcode == OpCodes.Ldc_I4_6); i++)
        {
            yield return instructionList[i];
        }

        yield return new CodeInstruction(OpCodes.Ldarg_0);
        yield return new CodeInstruction(OpCodes.Ldarg_1);
        yield return new CodeInstruction(OpCodes.Call, MI_IsBuildableThickroof);
        var @continue = il.DefineLabel();
        yield return new CodeInstruction(OpCodes.Brfalse, @continue);
        yield return new CodeInstruction(OpCodes.Ldc_R4, 0f);
        yield return new CodeInstruction(OpCodes.Ret);
        instructionList[i].labels.Add(@continue);
        for (; i < instructionList.Count; i++)
        {
            yield return instructionList[i];
        }
    }

    public static void BuildingRepairedPostfix(Building b)
    {
        var comp = b.GetComp<CompMaintainable>();
        if (comp != null)
        {
            comp.ticksSinceMaintain = 0;
        }
    }

    public static IEnumerable<CodeInstruction> FixClearBuildingArea(IEnumerable<CodeInstruction> instructions)
    {
        var list = instructions.ToList();
        var num = 0;
        var num2 = 0;
        var num3 = 0;
        for (var i = 0; i < list.Count; i++)
        {
            if (num2 == 0 && list[i].opcode == OpCodes.Ldc_I4_4)
            {
                var num4 = i;
                while (list[--num4].opcode != OpCodes.Ldarg_1)
                {
                }

                num2 = num4;
                num4 = i;
                while (list[num4++].opcode != OpCodes.Ble_Un_S)
                {
                }

                num4 += 4;
                while (list[num4++].opcode != OpCodes.Ldloc_0)
                {
                }

                num3 = num4 - 1;
            }

            if (num == 0 && list[i].opcode == OpCodes.Stloc_1)
            {
                var num4 = i;
                while (list[--num4].opcode != OpCodes.Ldloc_0)
                {
                }

                num = num4;
            }

            if (num != 0 && num2 != 0)
            {
                break;
            }
        }

        var num5 = num3 - num2;
        var range = list.GetRange(num2, num5);
        list.RemoveRange(num2, num5);
        (range[0].labels, list[num2].labels) = (list[num2].labels, range[0].labels);
        list.InsertRange(num - num5, range);
        return list;
    }

    public static IEnumerable<CodeInstruction> TransparentRoofOutputFactorFix(IEnumerable<CodeInstruction> instructions)
    {
        var MI_FixRoofedPowerOutputFactor = AccessTools.Method(typeof(TranspilerHelper),
            nameof(TranspilerHelper.FixRoofedPowerOutputFactor));
        var instructionList = instructions.ToList();
        var skipping = false;
        for (var i = 0; i < instructionList.Count; i++)
        {
            if (skipping)
            {
                if (instructionList[i].opcode != OpCodes.Add || instructionList[i + 1].opcode != OpCodes.Stloc_1)
                {
                    continue;
                }

                i++;
                skipping = false;
            }
            else if (instructionList[i].opcode == OpCodes.Add && instructionList[i + 1].opcode == OpCodes.Stloc_0)
            {
                yield return instructionList[i++];
                yield return instructionList[i++];
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldloc_2);
                yield return new CodeInstruction(OpCodes.Ldloca, 1);
                yield return new CodeInstruction(OpCodes.Call, MI_FixRoofedPowerOutputFactor);
                skipping = true;
            }
            else
            {
                yield return instructionList[i];
            }
        }
    }

    public static void GameInited()
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