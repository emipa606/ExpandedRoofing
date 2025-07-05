using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace ExpandedRoofing;

public class SolarRoofingTracker
{
    private static readonly FieldInfo fiRoofGridRoofGrid = AccessTools.Field(typeof(RoofGrid), "roofGrid");

    private static int nextId;

    private readonly Dictionary<int, SolarGridSet> cellSets = new();

    private readonly List<Thing> isolatedControllers = [];

    public SolarRoofingTracker(Map map)
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

    private int NextId => nextId++;

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
                var num2 = hashSet.ElementAt(num);
                cellSets[num2].set.Add(cell);
                for (var j = 1; j < hashSet.Count; j++)
                {
                    foreach (var controller in cellSets[hashSet.ElementAt(j)].controllers)
                    {
                        controller.TryGetComp<CompPowerPlantSolarController>().NetId = num2;
                    }

                    cellSets[num2].UnionWith(cellSets[hashSet.ElementAt(j)]);
                    cellSets.Remove(hashSet.ElementAt(j));
                }

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
                    isolatedController.TryGetComp<CompPowerPlantSolarController>().NetId = num;
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
        foreach (var value in cellSets.Values)
        {
            if (!value.set.Contains(cell))
            {
                continue;
            }

            value.set.Remove(cell);
            return;
        }

        Log.Error($"ExpandedRoofing: SolarRoofingTracker.Remove on a bad cell ({cell}).");
    }

    public void AddController(Thing controller)
    {
        var hashSet = new HashSet<IntVec3>();
        for (var i = -1; i < controller.RotatedSize.x + 1; i++)
        {
            for (var j = -1; j < controller.RotatedSize.z + 1; j++)
            {
                hashSet.Add(controller.Position + new IntVec3(i, 0, j));
            }
        }

        foreach (var pair in cellSets)
        {
            if (!hashSet.Any(iv3 => pair.Value.set.Contains(iv3)))
            {
                continue;
            }

            pair.Value.controllers.Add(controller);
            controller.TryGetComp<CompPowerPlantSolarController>().NetId = pair.Key;
            return;
        }

        isolatedControllers.Add(controller);
    }

    public void RemoveController(Thing controller)
    {
        var foundNetId = controller.TryGetComp<CompPowerPlantSolarController>().NetId;
        if (cellSets.TryGetValue(foundNetId, out var set))
        {
            set.controllers.Remove(controller);
        }
    }

    public SolarGridSet GetCellSets(int? netId)
    {
        return !netId.HasValue ? null : cellSets[netId.Value];
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