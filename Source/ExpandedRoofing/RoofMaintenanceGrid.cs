using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ExpandedRoofing;

internal sealed class RoofMaintenanceGrid(Map map) : IExposable
{
    private const int long_TickInterval = 2000;

    private const int minTicksBeforeMaintenance = 5000;

    private const int minTicksBeforeMTBCollapses = 7500;

    private Dictionary<int, int> grid = new();

    public IEnumerable<IntVec3> CurrentlyRequiresMaintenance
    {
        get
        {
            foreach (var item in grid)
            {
                if (item.Value > minTicksBeforeMaintenance)
                {
                    yield return GetIntVec3(item.Key);
                }
            }
        }
    }

    public void ExposeData()
    {
        Scribe_Collections.Look(ref grid, "grid");
        if (Scribe.mode != LoadSaveMode.LoadingVars || grid != null)
        {
            return;
        }

        grid = new Dictionary<int, int>();
        InitExistingMap();
    }

    private void InitExistingMap()
    {
        foreach (var allCell in map.AllCells)
        {
            var roofDef = map.roofGrid.RoofAt(allCell);
            if (roofDef != null && roofDef.IsBuildableThickRoof())
            {
                Add(allCell);
            }
        }
    }

    private IntVec3 GetIntVec3(int index)
    {
        return map.cellIndices.IndexToCell(index);
    }

    private int GetCell(IntVec3 c)
    {
        return map.cellIndices.CellToIndex(c);
    }

    public void Add(IntVec3 c)
    {
        var cell = GetCell(c);
        if (!grid.TryAdd(cell, 0))
        {
            Reset(c);
        }
    }

    public void Remove(IntVec3 c)
    {
        grid.Remove(map.cellIndices.CellToIndex(c));
    }

    public void Reset(IntVec3 c)
    {
        grid[map.cellIndices.CellToIndex(c)] = 0;
    }

    public bool MaintenanceNeeded(IntVec3 c)
    {
        return grid[GetCell(c)] > minTicksBeforeMaintenance;
    }

    public void Tick()
    {
        foreach (var item in grid
                     .Where(kp => Find.TickManager.TicksGame + (kp.Key.HashOffset() % long_TickInterval) == 0).ToList())
        {
            grid[item.Key]++;
            if (grid[item.Key] > minTicksBeforeMTBCollapses && Rand.MTBEventOccurs(3.5f, 60000f, long_TickInterval))
            {
                map.roofCollapseBuffer.MarkToCollapse(GetIntVec3(item.Key));
            }
        }
    }
}