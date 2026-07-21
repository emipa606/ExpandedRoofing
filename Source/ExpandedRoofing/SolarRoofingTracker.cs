using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace ExpandedRoofing;

public class SolarRoofingTracker
{
    private static readonly FieldInfo fiRoofGridRoofGrid = AccessTools.Field(typeof(RoofGrid), "roofGrid");

    private static readonly FieldInfo fiNetId = AccessTools.Field(typeof(CompPowerPlantSolarController), "netId");

    private static int nextId;

    private readonly Dictionary<int, SolarGridSet> cellSets = new();

    private readonly List<Thing> isolatedControllers = [];

    public SolarRoofingTracker(Map map)
    {
        RefreshController(map);
    }

    private static int NextId => nextId++;

    public void RefreshController(Map map)
    {
        if (fiRoofGridRoofGrid.GetValue(map.roofGrid) is RoofDef[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] == RoofDefOf.RoofSolar)
                {
                    AddSolarCell(map.cellIndices.IndexToCell(i));
                }
            }
        }

        foreach (var item in map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.SolarController))
        {
            AddController(item);
        }
    }

    // Deferred full rebuild: called from the MapComponent's FinalizeInit (after map load / generation /
    // gravship landing) when the roof grid and buildings are fully present. The constructor scan runs
    // during Map.ConstructComponents before RoofGrid data exists, so it finds nothing on load.
    public void Rebuild(Map map)
    {
        foreach (var set in cellSets.Values)
        {
            foreach (var controller in set.controllers)
            {
                NullNetId(controller);
            }
        }

        foreach (var controller in isolatedControllers)
        {
            NullNetId(controller);
        }

        cellSets.Clear();
        isolatedControllers.Clear();
        RefreshController(map);
    }

    public void AddSolarCell(IntVec3 cell)
    {
        var hashSet = new HashSet<int>();
        foreach (var cellSet in cellSets)
        {
            for (var i = 0; i < 5; i++)
            {
                if (!cellSet.Value.set.Contains(cell + GenAdj.CardinalDirectionsAndInside[i]))
                {
                    continue;
                }

                hashSet.Add(cellSet.Key);
                break;
            }
        }

        var num = 0;
        switch (hashSet.Count)
        {
            case 0:
            {
                var value = new SolarGridSet(cell);
                num = NextId;
                cellSets.Add(num, value);
                break;
            }
            case 1:
                num = hashSet.First();
                cellSets[num].set.Add(cell);
                break;
            default:
            {
                var num2 = hashSet.ElementAt(0);
                cellSets[num2].set.Add(cell);
                for (var j = 1; j < hashSet.Count; j++)
                {
                    foreach (var controller in cellSets[hashSet.ElementAt(j)].controllers)
                    {
                        SetNetId(controller, num2);
                    }

                    cellSets[num2].UnionWith(cellSets[hashSet.ElementAt(j)]);
                    cellSets.Remove(hashSet.ElementAt(j));
                }

                num = num2;
                break;
            }
        }

        var list = new List<Thing>();
        foreach (var isolatedController in isolatedControllers)
        {
            var foundNetId = false;
            for (var k = -1; k < isolatedController.RotatedSize.x + 1; k++)
            {
                if (foundNetId)
                {
                    break;
                }

                for (var l = -1; l < isolatedController.RotatedSize.z + 1; l++)
                {
                    if (foundNetId)
                    {
                        break;
                    }

                    if (cell != isolatedController.Position + new IntVec3(k, 0, l))
                    {
                        continue;
                    }

                    cellSets[num].controllers.Add(isolatedController);
                    list.Add(isolatedController);
                    SetNetId(isolatedController, num);
                    foundNetId = true;
                }
            }
        }

        foreach (var item in list)
        {
            isolatedControllers.Remove(item);
        }
    }

    public void RemoveSolarCell(IntVec3 cell)
    {
        int? foundKey = null;
        foreach (var pair in cellSets)
        {
            if (!pair.Value.set.Contains(cell))
            {
                continue;
            }

            foundKey = pair.Key;
            break;
        }

        if (!foundKey.HasValue)
        {
            Log.Warning($"ExpandedRoofing: SolarRoofingTracker.Remove on a bad cell ({cell}).");
            return;
        }

        var key = foundKey.Value;
        var set = cellSets[key];
        set.set.Remove(cell);

        // (a) set is now empty: drop it and orphan its controllers.
        if (set.set.Count == 0)
        {
            foreach (var controller in set.controllers)
            {
                NullNetId(controller);
                isolatedControllers.Add(controller);
            }

            cellSets.Remove(key);
            return;
        }

        // (b) the removed cell may have split the set into disconnected components.
        var components = ConnectedComponents(set.set);
        if (components.Count <= 1)
        {
            return;
        }

        // Keep the largest component under the existing id; spin up new ids for the rest.
        components.Sort((a, b) => b.Count - a.Count);
        var controllers = set.controllers.ToList();

        var componentSets = new List<KeyValuePair<int, SolarGridSet>>();
        set.set.Clear();
        foreach (var c in components[0])
        {
            set.set.Add(c);
        }

        set.controllers.Clear();
        componentSets.Add(new KeyValuePair<int, SolarGridSet>(key, set));

        for (var ci = 1; ci < components.Count; ci++)
        {
            var gridSet = new SolarGridSet(components[ci].First());
            foreach (var c in components[ci])
            {
                gridSet.set.Add(c);
            }

            var newId = NextId;
            cellSets.Add(newId, gridSet);
            componentSets.Add(new KeyValuePair<int, SolarGridSet>(newId, gridSet));
        }

        foreach (var controller in controllers)
        {
            var footprint = FootprintCells(controller);
            var attached = false;
            foreach (var componentSet in componentSets)
            {
                if (!footprint.Any(iv3 => componentSet.Value.set.Contains(iv3)))
                {
                    continue;
                }

                componentSet.Value.controllers.Add(controller);
                SetNetId(controller, componentSet.Key);
                attached = true;
                break;
            }

            if (!attached)
            {
                NullNetId(controller);
                isolatedControllers.Add(controller);
            }
        }
    }

    public void AddController(Thing controller)
    {
        RemoveController(controller);

        var footprint = FootprintCells(controller);

        var matching = new List<int>();
        foreach (var pair in cellSets)
        {
            if (footprint.Any(iv3 => pair.Value.set.Contains(iv3)))
            {
                matching.Add(pair.Key);
            }
        }

        if (matching.Count == 0)
        {
            isolatedControllers.Add(controller);
            return;
        }

        var survivor = matching[0];
        for (var m = 1; m < matching.Count; m++)
        {
            var other = matching[m];
            foreach (var otherController in cellSets[other].controllers)
            {
                SetNetId(otherController, survivor);
            }

            cellSets[survivor].UnionWith(cellSets[other]);
            cellSets.Remove(other);
        }

        cellSets[survivor].controllers.Add(controller);
        SetNetId(controller, survivor);
    }

    public void RemoveController(Thing controller)
    {
        isolatedControllers.Remove(controller);
        foreach (var set in cellSets.Values)
        {
            set.controllers.Remove(controller);
        }

        NullNetId(controller);
    }

    public SolarGridSet GetCellSets(int? netId)
    {
        return !netId.HasValue ? null : cellSets.GetValueOrDefault(netId.Value);
    }

    private static HashSet<IntVec3> FootprintCells(Thing controller)
    {
        var footprint = new HashSet<IntVec3>();
        for (var i = -1; i < controller.RotatedSize.x + 1; i++)
        {
            for (var j = -1; j < controller.RotatedSize.z + 1; j++)
            {
                footprint.Add(controller.Position + new IntVec3(i, 0, j));
            }
        }

        return footprint;
    }

    private static List<HashSet<IntVec3>> ConnectedComponents(HashSet<IntVec3> cells)
    {
        var remaining = new HashSet<IntVec3>(cells);
        var components = new List<HashSet<IntVec3>>();
        var queue = new Queue<IntVec3>();
        while (remaining.Count > 0)
        {
            var start = remaining.First();
            remaining.Remove(start);
            var component = new HashSet<IntVec3> { start };
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                for (var i = 0; i < 4; i++)
                {
                    var neighbor = current + GenAdj.CardinalDirections[i];
                    if (!remaining.Remove(neighbor))
                    {
                        continue;
                    }

                    component.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            components.Add(component);
        }

        return components;
    }

    private static void SetNetId(Thing controller, int id)
    {
        var comp = controller.TryGetComp<CompPowerPlantSolarController>();
        if (comp != null)
        {
            comp.NetId = id;
        }
    }

    private static void NullNetId(Thing controller)
    {
        var comp = controller.TryGetComp<CompPowerPlantSolarController>();
        if (comp != null)
        {
            fiNetId.SetValue(comp, null);
        }
    }

    public class SolarGridSet
    {
        public readonly HashSet<Thing> controllers = [];
        public readonly HashSet<IntVec3> set = [];

        public SolarGridSet(IntVec3 cell)
        {
            set.Add(cell);
        }

        public int RoofCount => set.Count;

        public int ControllerCount => controllers.Count;

        public void UnionWith(SolarGridSet other)
        {
            set.UnionWith(other.set);
            controllers.UnionWith(other.controllers);
        }
    }
}
