using Verse;

namespace ExpandedRoofing;

public class SolarRoofing_MapComponent(Map map) : MapComponent(map)
{
    public readonly SolarRoofingTracker tracker = new SolarRoofingTracker(map);
}