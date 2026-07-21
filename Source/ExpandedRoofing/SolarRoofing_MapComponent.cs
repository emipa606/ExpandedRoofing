using Verse;

namespace ExpandedRoofing;

public class SolarRoofing_MapComponent(Map map) : MapComponent(map)
{
    public readonly SolarRoofingTracker tracker = new(map);

    // FinalizeInit runs after map load AND after map generation (and after gravship landing), once the
    // roof grid and buildings are fully present. Do the one authoritative scan here.
    public override void FinalizeInit()
    {
        base.FinalizeInit();
        tracker.Rebuild(map);
    }
}
