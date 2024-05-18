using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace ExpandedRoofing;

internal static class ClosestThingReachableHelper
{
    public static Thing ClosestThingReachableWrapper(IntVec3 root, Map map, ThingRequest thingReq, PathEndMode peMode,
        TraverseParms traverseParams, float maxDistance, Predicate<Thing> validator,
        IEnumerable<Thing> customGlobalSearchSet, int searchRegionsMin, int searchRegionsMax, bool forceGlobalSearch,
        RegionType traversableRegionTypes, bool ignoreEntirelyForbiddenRegions)
    {
        if (thingReq.group != ThingRequestGroup.BuildingFrame)
        {
            return GenClosest.ClosestThingReachable(root, map, thingReq, peMode, traverseParams, maxDistance, validator,
                customGlobalSearchSet, searchRegionsMin, searchRegionsMax, forceGlobalSearch, traversableRegionTypes,
                ignoreEntirelyForbiddenRegions);
        }

        var predicate = validator;
        validator = s => predicate(s) && Extra(s);

        return GenClosest.ClosestThingReachable(root, map, thingReq, peMode, traverseParams, maxDistance, validator,
            customGlobalSearchSet, searchRegionsMin, searchRegionsMax, forceGlobalSearch, traversableRegionTypes,
            ignoreEntirelyForbiddenRegions);

        bool Extra(Thing t)
        {
            return !t.def.defName.EndsWith("Framing_Frame") ||
                   RoofCollapseUtility.WithinRangeOfRoofHolder(t.Position, t.Map) &&
                   RoofCollapseUtility.ConnectedToRoofHolder(t.Position, t.Map, true);
        }
    }
}