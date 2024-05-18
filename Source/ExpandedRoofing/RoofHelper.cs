using Verse;

namespace ExpandedRoofing;

internal static class RoofHelper
{
    public static bool IsBuildableThickRoof(this RoofDef roofDef)
    {
        return roofDef is { isThickRoof: true } && roofDef.HasModExtension<RoofExtension>();
    }
}