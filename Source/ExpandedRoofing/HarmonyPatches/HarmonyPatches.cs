using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace ExpandedRoofing.HarmonyPatches;

[StaticConstructorOnStartup]
internal class HarmonyPatches
{
    public static readonly int SectionLayer_LightingOverlay__Regenerate__RoofDef__LocalIndex;
    public static readonly FieldInfo FI_RoofGrid_map;

    static HarmonyPatches()
    {
        SectionLayer_LightingOverlay__Regenerate__RoofDef__LocalIndex = AccessTools
            .Method(typeof(SectionLayer_LightingOverlay), "Regenerate").GetMethodBody()!.LocalVariables
            .First(lvi => lvi.LocalType == typeof(RoofDef)).LocalIndex;
        FI_RoofGrid_map = AccessTools.Field(typeof(RoofGrid), "map");
        new Harmony("rimworld.whyisthat.expandedroofing.main").PatchAll(Assembly.GetExecutingAssembly());
    }
}

//public static IEnumerable<CodeInstruction> PlantLightingFix(IEnumerable<CodeInstruction> instructions,
//        ILGenerator il)
//    {
//        var FI_GlowGrid_map = AccessTools.Field(typeof(GlowGrid), "map");
//        var MI_CheckTransparency =
//            AccessTools.Method(typeof(TranspilerHelper), nameof(TranspilerHelper.CheckTransparency));
//        var instructionList = instructions.ToList();
//        int j;
//        for (j = 0; j < instructionList.Count; j++)
//        {
//            if (instructionList[j].opcode == OpCodes.Ldarg_2)
//            {
//                yield return new CodeInstruction(OpCodes.Ldarg_0)
//                {
//                    labels = instructionList[j].labels
//                };
//                yield return new CodeInstruction(OpCodes.Dup);
//                yield return new CodeInstruction(OpCodes.Ldfld, FI_GlowGrid_map);
//                yield return new CodeInstruction(OpCodes.Ldarg_1);
//                yield return new CodeInstruction(OpCodes.Ldloca, 0);
//                yield return new CodeInstruction(OpCodes.Call, MI_CheckTransparency);
//                var @continue = il.DefineLabel();
//                yield return new CodeInstruction(OpCodes.Brfalse, @continue);
//                yield return new CodeInstruction(OpCodes.Ldloc_0);
//                yield return new CodeInstruction(OpCodes.Ret);
//                yield return new CodeInstruction(instructionList[j].opcode, instructionList[j].operand)
//                {
//                    labels = { @continue }
//                };
//                break;
//            }

//            yield return instructionList[j];
//        }

//        for (j++; j < instructionList.Count; j++)
//        {
//            yield return instructionList[j];
//        }
//    }