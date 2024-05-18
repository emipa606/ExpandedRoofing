using System;
using System.Collections.Generic;
using Mlie;
using UnityEngine;
using Verse;

namespace ExpandedRoofing;

internal class ExpandedRoofingMod : Mod
{
    private const string dontTemptMe_ModName = "Don't Tempt Me!";

    private const string glassLights_ModName = "Glass+Lights";
    public static ExpandedRoofingSettings settings;

    private static string currentVersion;

    private bool dontTemptMe;

    private string maxOutputBuffer = "";

    private string wattagePerSolarPanel = "";

    public ExpandedRoofingMod(ModContentPack content)
        : base(content)
    {
        settings = GetSettings<ExpandedRoofingSettings>();
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
        var dictionary = new Dictionary<string, Action>
        {
            {
                dontTemptMe_ModName,
                delegate { dontTemptMe = true; }
            },
            {
                glassLights_ModName,
                delegate { GlassLights = true; }
            }
        };
        foreach (var allInstalledMod in ModLister.AllInstalledMods)
        {
            if (allInstalledMod.Active && dictionary.ContainsKey(allInstalledMod.Name))
            {
                dictionary[allInstalledMod.Name]();
            }
        }
    }

    public static bool GlassLights { get; set; }

    public override string SettingsCategory()
    {
        return "ER_ExpandedRoofing".Translate();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        if (dontTemptMe)
        {
            return;
        }

        if (maxOutputBuffer == "")
        {
            maxOutputBuffer = settings.solarController_maxOutput.ToString("0.00");
            wattagePerSolarPanel = settings.solarController_wattagePerSolarPanel.ToString("0.00");
        }

        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(inRect);
        listing_Standard.TextFieldNumericLabeled("ER_MaxOutputLabel".Translate(),
            ref settings.solarController_maxOutput, ref maxOutputBuffer);
        listing_Standard.TextFieldNumericLabeled("ER_WattagePerSolarPanelLabel".Translate(),
            ref settings.solarController_wattagePerSolarPanel, ref wattagePerSolarPanel);
        listing_Standard.CheckboxLabeled("ER_GlassLights".Translate(), ref settings.glassLights);
        listing_Standard.CheckboxLabeled("ER_RoofMaintenance".Translate(), ref settings.roofMaintenance);
        if (currentVersion != null)
        {
            listing_Standard.Gap();
            GUI.contentColor = Color.gray;
            listing_Standard.Label("ER_CurrentModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listing_Standard.End();
    }
}