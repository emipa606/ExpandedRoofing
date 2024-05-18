using Verse;

namespace ExpandedRoofing;

public class CompMaintainableRoof : CompCustomRoof
{
    public override void CompTick()
    {
        if (parent.Stuff != null)
        {
            var named = DefDatabase<RoofDef>.GetNamed(parent.Stuff.defName.Replace("Blocks", "") + "ThickStoneRoof",
                false);
            parent.Map.roofGrid.SetRoof(parent.Position, named);
        }

        base.CompTick();
    }
}