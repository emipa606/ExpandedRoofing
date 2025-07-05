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
    private static readonly MethodInfo miClosestThingReachable;

    private static readonly MethodInfo miClosestThingReachableWrapper;

    static FixFinishFrameBuildOrder()
    {
        miClosestThingReachable = AccessTools.Method(typeof(GenClosest), nameof(GenClosest.ClosestThingReachable));
        miClosestThingReachableWrapper =
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
            if (instruction.opcode == OpCodes.Call && (MethodInfo)instruction.operand == miClosestThingReachable)
            {
                yield return new CodeInstruction(OpCodes.Call, miClosestThingReachableWrapper);
            }
            else
            {
                yield return instruction;
            }
        }
    }
}