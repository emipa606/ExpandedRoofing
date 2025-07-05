using System.Reflection;
using HarmonyLib;
using Verse;

namespace ExpandedRoofing.HarmonyPatches;

[StaticConstructorOnStartup]
internal class HarmonyPatches
{
    public static readonly FieldInfo FI_RoofGrid_map;

    static HarmonyPatches()
    {
        FI_RoofGrid_map = AccessTools.Field(typeof(RoofGrid), "map");
        new Harmony("rimworld.whyisthat.expandedroofing.main").PatchAll(Assembly.GetExecutingAssembly());
    }
}