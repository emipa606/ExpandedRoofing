using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ExpandedRoofing;

public class WorkGiver_PerformRoofMaintenance : WorkGiver_Scanner
{
    public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
    {
        return pawn.Map.GetComponent<RoofMaintenance_MapComponenent>()?.MaintenanceRequired;
    }

    public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
    {
        if (c.IsForbidden(pawn))
        {
            return false;
        }

        if (!pawn.CanReserve(c, 1, -1, ReservationLayerDefOf.Ceiling))
        {
            return false;
        }

        if (!pawn.CanReach(c, PathEndMode.Touch, pawn.NormalMaxDanger()))
        {
            return false;
        }

        if (!pawn.Map.roofGrid.RoofAt(c).IsBuildableThickRoof())
        {
            return false;
        }

        return pawn.Map.GetComponent<RoofMaintenance_MapComponenent>()?.MaintenanceNeeded(c) ?? false;
    }

    public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
    {
        return new Job(JobDefOf.PerformRoofMaintenance, c);
    }
}