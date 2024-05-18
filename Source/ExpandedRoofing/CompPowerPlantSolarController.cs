using RimWorld;
using UnityEngine;
using Verse;
using static Verse.GenDraw;

namespace ExpandedRoofing;

[StaticConstructorOnStartup]
public class CompPowerPlantSolarController : CompPowerPlant
{
    private static readonly Color color = new Color(0.3f, 1f, 0.4f);

    private static readonly Vector2 BarSize = new Vector2(1.4f, 0.07f);

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

    public float WattagePerSolarPanel => ExpandedRoofingMod.settings.solarController_wattagePerSolarPanel;

    public float MaxOutput => netId.HasValue ? ExpandedRoofingMod.settings.solarController_maxOutput : 0f;

    public int RoofCount => netId.HasValue ? solarRoofingTracker.GetCellSets(netId).RoofCount : 0;

    public int ControllerCount => solarRoofingTracker.GetCellSets(netId).ControllerCount;

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

    public Color GetCellExtraColor(int index)
    {
        return Color.white;
    }

    public override void PostDeSpawn(Map map)
    {
        base.PostDeSpawn(map);
        map.GetComponent<SolarRoofing_MapComponent>().tracker.RemoveController(parent);
    }

    public override void PostDraw()
    {
        base.PostDraw();
        var fillableBarRequest = default(FillableBarRequest);
        var postition = parent.DrawPos + (Vector3.up * 0.1f);
        postition.z += -0.895f;
        fillableBarRequest.center = postition;
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
}