using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace ExpandedRoofing.HarmonyPatches;

[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.BlocksConstruction))]
public static class GenConstruct_BlocksConstruction
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
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
}