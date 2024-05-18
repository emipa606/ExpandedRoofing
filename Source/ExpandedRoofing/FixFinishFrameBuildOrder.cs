using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ExpandedRoofing;

[StaticConstructorOnStartup]
internal class FixFinishFrameBuildOrder
{
    private static readonly MethodInfo MI_ClosestThingReachable;

    private static readonly MethodInfo MI_ClosestThingReachableWrapper;

    static FixFinishFrameBuildOrder()
    {
        MI_ClosestThingReachable = AccessTools.Method(typeof(GenClosest), nameof(GenClosest.ClosestThingReachable));
        MI_ClosestThingReachableWrapper =
            AccessTools.Method(typeof(ClosestThingReachableHelper),
                nameof(ClosestThingReachableHelper.ClosestThingReachableWrapper));
        new Harmony("rimworld.whyisthat.expandedroofing.fixbuildorder").Patch(
            AccessTools.Method(typeof(JobGiver_Work), nameof(JobGiver_Work.TryIssueJobPackage)), null, null,
            new HarmonyMethod(typeof(FixFinishFrameBuildOrder), nameof(Transpiler)));
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Call && (MethodInfo)instruction.operand == MI_ClosestThingReachable)
            {
                yield return new CodeInstruction(OpCodes.Call, MI_ClosestThingReachableWrapper);
            }
            else
            {
                yield return instruction;
            }
        }
    }
}