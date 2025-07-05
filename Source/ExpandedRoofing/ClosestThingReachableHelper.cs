using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace ExpandedRoofing;

internal static class ClosestThingReachableHelper
{
    public static Thing ClosestThingReachableWrapper(IntVec3 root, Map map, ThingRequest thingReq, PathEndMode peMode,
        TraverseParms traverseParams, float maxDistance = 9999f, Predicate<Thing> validator = null,
        IEnumerable<Thing> customGlobalSearchSet = null, int searchRegionsMin = 0, int searchRegionsMax = -1,
        bool forceAllowGlobalSearch = false, RegionType traversableRegionTypes = RegionType.Set_Passable,
        bool ignoreEntirelyForbiddenRegions = false, bool lookInHaulSources = false)
    {
        if (thingReq.group != ThingRequestGroup.BuildingFrame)
        {
            return GenClosest.ClosestThingReachable(root, map, thingReq, peMode, traverseParams, maxDistance, validator,
                customGlobalSearchSet, searchRegionsMin, searchRegionsMax, forceAllowGlobalSearch,
                traversableRegionTypes,
                ignoreEntirelyForbiddenRegions);
        }

        var predicate = validator;
        validator = s => predicate != null && predicate(s) && extra(s);

        return GenClosest.ClosestThingReachable(root, map, thingReq, peMode, traverseParams, maxDistance, validator,
            customGlobalSearchSet, searchRegionsMin, searchRegionsMax, forceAllowGlobalSearch, traversableRegionTypes,
            ignoreEntirelyForbiddenRegions);

        static bool extra(Thing t)
        {
            return !t.def.defName.EndsWith("Framing_Frame") ||
                   RoofCollapseUtility.WithinRangeOfRoofHolder(t.Position, t.Map) &&
                   RoofCollapseUtility.ConnectedToRoofHolder(t.Position, t.Map, true);
        }
    }
}