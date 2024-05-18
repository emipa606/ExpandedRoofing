using System.Collections.Generic;
using Verse;

namespace ExpandedRoofing;

public class RoofMaintenance_MapComponenent(Map map) : MapComponent(map)
{
    private RoofMaintenanceGrid roofMaintenanceGrid = new RoofMaintenanceGrid(map);

    public IEnumerable<IntVec3> MaintenanceRequired => roofMaintenanceGrid.CurrentlyRequiresMaintenance;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref roofMaintenanceGrid, "roofMaintenanceGrid", map);
        if (Scribe.mode == LoadSaveMode.LoadingVars && roofMaintenanceGrid == null)
        {
            roofMaintenanceGrid = new RoofMaintenanceGrid(map);
        }
    }

    public override void MapComponentTick()
    {
        if (ExpandedRoofingMod.settings.roofMaintenance)
        {
            roofMaintenanceGrid.Tick();
        }
    }

    public void AddMaintainableRoof(IntVec3 c)
    {
        roofMaintenanceGrid.Add(c);
    }

    public void RemoveMaintainableRoof(IntVec3 c)
    {
        roofMaintenanceGrid.Remove(c);
    }

    public void DoMaintenance(IntVec3 c)
    {
        roofMaintenanceGrid.Reset(c);
    }

    public bool MaintenanceNeeded(IntVec3 c)
    {
        return roofMaintenanceGrid.MaintenanceNeeded(c);
    }
}