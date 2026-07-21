using Verse;

namespace ExpandedRoofing;

public class CompMaintainableRoof : CompCustomRoof
{
    public override void CompTick()
    {
        if (parent.Stuff != null)
        {
            var named = ResolveRoofDef(parent.Stuff);
            if (named == null)
            {
                // Do NOT self-destruct silently: leave the framing standing so the player can see that
                // something is misconfigured rather than losing the resources to nothing.
                Log.ErrorOnce(
                    $"ExpandedRoofing: could not resolve a thick stone roof for stuff '{parent.Stuff.defName}' " +
                    $"(framing '{parent.def.defName}'). Roof not placed; framing left standing.",
                    parent.def.shortHash ^ parent.Stuff.shortHash);
                return;
            }

            parent.Map.roofGrid.SetRoof(parent.Position, named);
        }

        base.CompTick();
    }

    private static RoofDef ResolveRoofDef(ThingDef stuff)
    {
        // Robust path: a generated roof records its source stuff on its extension.
        foreach (var roofDef in DefDatabase<RoofDef>.AllDefsListForReading)
        {
            if (roofDef.GetModExtension<RoofExtension>()?.sourceStuff == stuff)
            {
                return roofDef;
            }
        }

        // Fallback for XML-authored vanilla-stone roofs (Sandstone/.../Jade/Vacstone/Obsidian).
        return DefDatabase<RoofDef>.GetNamed($"{stuff.defName.Replace("Blocks", "")}ThickStoneRoof", false);
    }
}
