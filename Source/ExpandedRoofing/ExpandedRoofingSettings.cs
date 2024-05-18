using Verse;

namespace ExpandedRoofing;

public class ExpandedRoofingSettings : ModSettings
{
    private const float maxOutput_default = 2500f;

    private const float wattagePerSolarPanel_default = 200f;

    public bool glassLights = true;

    public bool roofMaintenance = true;

    public float solarController_maxOutput = maxOutput_default;

    public float solarController_wattagePerSolarPanel = wattagePerSolarPanel_default;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref solarController_maxOutput, "solarController_maxOutput", maxOutput_default);
        Scribe_Values.Look(ref solarController_wattagePerSolarPanel, "solarController_wattagePerSolarPanel",
            wattagePerSolarPanel_default);
        Scribe_Values.Look(ref glassLights, "glassLights", true);
        Scribe_Values.Look(ref roofMaintenance, "roofMaintenance", true);
    }
}