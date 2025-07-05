using RimWorld;
using Verse;

namespace ExpandedRoofing;

public class CompCustomRoof : ThingComp
{
    private CompProperties_CustomRoof Props => (CompProperties_CustomRoof)props;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (respawningAfterLoad)
        {
            return;
        }

        parent.Map.roofGrid.SetRoof(parent.Position, Props.roofDef);
        MoteMaker.PlaceTempRoof(parent.Position, parent.Map);
    }

    public override void CompTick()
    {
        base.CompTick();
        if (!parent.Destroyed)
        {
            parent.Destroy();
        }
    }
}