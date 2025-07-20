using RimWorld;
using UnityEngine;
using Verse;
using static Verse.GenDraw;

namespace ExpandedRoofing;

[StaticConstructorOnStartup]
public class CompPowerPlantSolarController : CompPowerPlant
{
    private static readonly Color color = new(0.3f, 1f, 0.4f);

    private static readonly Vector2 BarSize = new(1.4f, 0.07f);

    private static readonly Material PowerPlantSolarBarFilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.475f, 0.1f));

    private static readonly Material PowerPlantSolarBarOverloadFilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.8f, 0.475f, 0.1f));

    private static readonly Material PowerPlantSolarBarUnfilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f));

    private int? netId;

    private float powerOut;

    private SolarRoofingTracker solarRoofingTracker;

    public int NetId
    {
        get
        {
            if (netId != null)
            {
                return netId.Value;
            }

            return -1;
        }
        set => netId = value;
    }

    private float WattagePerSolarPanel => ExpandedRoofingMod.settings.solarController_wattagePerSolarPanel;

    private float MaxOutput => netId.HasValue ? ExpandedRoofingMod.settings.solarController_maxOutput : 0f;

    private int RoofCount => netId.HasValue ? solarRoofingTracker.GetCellSets(NetId).RoofCount : 0;

    private int ControllerCount => netId.HasValue ? solarRoofingTracker.GetCellSets(NetId).ControllerCount : 0;

    protected override float DesiredPowerOutput
    {
        get
        {
            if (!netId.HasValue)
            {
                return 0f;
            }

            powerOut = Mathf.Lerp(0f, WattagePerSolarPanel, parent.Map.skyManager.CurSkyGlow) *
                       (RoofCount / (float)ControllerCount);
            return powerOut > MaxOutput ? MaxOutput : powerOut;
        }
    }

    public Color Color => color;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        solarRoofingTracker = parent.Map.GetComponent<SolarRoofing_MapComponent>().tracker;
        solarRoofingTracker.AddController(parent);
    }

    public override string CompInspectStringExtra()
    {
        var returnString = $"{"SolarRoofArea".Translate()}: {RoofCount:###0}\n{base.CompInspectStringExtra()}";
        if (powerOut > MaxOutput)
        {
            returnString = $"{"SolarRoofTooLarge".Translate()}\n{returnString}";
        }

        return returnString;
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        base.PostDeSpawn(map, mode);
        map.GetComponent<SolarRoofing_MapComponent>().tracker.RemoveController(parent);
    }

    public override void PostDraw()
    {
        base.PostDraw();
        var fillableBarRequest = default(FillableBarRequest);
        var position = parent.DrawPos + (Vector3.up * 0.1f);
        position.z += -0.895f;
        fillableBarRequest.center = position;
        fillableBarRequest.size = BarSize;
        if (MaxOutput == 0)
        {
            fillableBarRequest.fillPercent = 0;
        }
        else
        {
            fillableBarRequest.fillPercent = DesiredPowerOutput / MaxOutput;
        }

        fillableBarRequest.filledMat =
            powerOut > MaxOutput ? PowerPlantSolarBarOverloadFilledMat : PowerPlantSolarBarFilledMat;

        fillableBarRequest.unfilledMat = PowerPlantSolarBarUnfilledMat;
        fillableBarRequest.margin = 0.05f;
        fillableBarRequest.rotation = parent.Rotation;
        DrawFillableBar(fillableBarRequest);
    }

    public override void CompTickInterval(int delta)
    {
        base.CompTickInterval(delta);
        if (!parent.IsHashIntervalTick(GenTicks.TickLongInterval, delta))
        {
            return;
        }

        solarRoofingTracker ??= parent.Map.GetComponent<SolarRoofing_MapComponent>().tracker;
        solarRoofingTracker?.RefreshController(parent.MapHeld);
    }
}