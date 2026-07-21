using System.Collections.Generic;
using Verse;

namespace ExpandedRoofing;

internal sealed class RoofMaintenanceGrid(Map map) : IExposable
{
    // NOTE: these thresholds are counted in ~2000-tick bucket cycles (one increment per cell per cycle),
    // NOT raw ticks. In real time that is roughly 166 in-game days before maintenance is required and
    // ~250 in-game days before collapse can occur — not 5000/7500 ticks.
    private const int long_TickInterval = 2000;

    private const int minTicksBeforeMaintenance = 5000;

    private const int minTicksBeforeMTBCollapses = 7500;

    // Source of truth for save/load: cellIndex -> maintenance counter.
    private Dictionary<int, int> grid = new();

    // Cells pre-bucketed by (cellIndex.HashOffset() % long_TickInterval) so Tick() only touches ~1/2000
    // of the tracked cells each map tick instead of copying the whole dictionary. Transient (rebuilt on load).
    private readonly List<int>[] buckets = new List<int>[long_TickInterval];

    // Cells whose counter has crossed the maintenance threshold. Transient, maintained incrementally.
    private readonly HashSet<int> dueCells = [];

    public IEnumerable<IntVec3> CurrentlyRequiresMaintenance
    {
        get
        {
            foreach (var cell in dueCells)
            {
                yield return GetIntVec3(cell);
            }
        }
    }

    public void ExposeData()
    {
        Scribe_Collections.Look(ref grid, "grid");
        if (Scribe.mode != LoadSaveMode.LoadingVars)
        {
            return;
        }

        if (grid == null)
        {
            grid = new Dictionary<int, int>();
            InitExistingMap();
        }

        RebuildIndex();
    }

    private void InitExistingMap()
    {
        foreach (var allCell in map.AllCells)
        {
            var roofDef = map.roofGrid.RoofAt(allCell);
            if (roofDef != null && roofDef.IsBuildableThickRoof())
            {
                grid[GetCell(allCell)] = 0;
            }
        }
    }

    private void RebuildIndex()
    {
        for (var i = 0; i < buckets.Length; i++)
        {
            buckets[i]?.Clear();
        }

        dueCells.Clear();
        foreach (var pair in grid)
        {
            (buckets[BucketIndex(pair.Key)] ??= []).Add(pair.Key);
            if (pair.Value > minTicksBeforeMaintenance)
            {
                dueCells.Add(pair.Key);
            }
        }
    }

    private static int BucketIndex(int cellIndex)
    {
        var b = cellIndex.HashOffset() % long_TickInterval;
        return b < 0 ? b + long_TickInterval : b;
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
        if (grid.TryAdd(cell, 0))
        {
            (buckets[BucketIndex(cell)] ??= []).Add(cell);
        }
        else
        {
            Reset(c);
        }
    }

    public void Remove(IntVec3 c)
    {
        var cell = GetCell(c);
        if (!grid.Remove(cell))
        {
            return;
        }

        buckets[BucketIndex(cell)]?.Remove(cell);
        dueCells.Remove(cell);
    }

    public void Reset(IntVec3 c)
    {
        var cell = GetCell(c);
        grid[cell] = 0;
        dueCells.Remove(cell);
    }

    public bool MaintenanceNeeded(IntVec3 c)
    {
        return grid.TryGetValue(GetCell(c), out var value) && value > minTicksBeforeMaintenance;
    }

    public void Tick()
    {
        var bucket = buckets[Find.TickManager.TicksGame % long_TickInterval];
        if (bucket == null)
        {
            return;
        }

        for (var i = 0; i < bucket.Count; i++)
        {
            var cell = bucket[i];
            var value = grid[cell] + 1;
            grid[cell] = value;
            if (value > minTicksBeforeMaintenance)
            {
                dueCells.Add(cell);
            }

            if (value > minTicksBeforeMTBCollapses && Rand.MTBEventOccurs(3.5f, 60000f, long_TickInterval))
            {
                map.roofCollapseBuffer.MarkToCollapse(GetIntVec3(cell));
            }
        }
    }
}
