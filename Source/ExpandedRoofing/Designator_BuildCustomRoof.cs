using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ExpandedRoofing;

public class Designator_BuildCustomRoof(BuildableDef entDef, RoofDef rDef) : Designator_Build(entDef)
{
    private readonly FieldInfo fiStuffDef = AccessTools.Field(typeof(Designator_Build), "stuffDef");

    private readonly RoofDef roofDef = rDef;

    public override BuildableDef PlacingDef => entDef;

    public override AcceptanceReport CanDesignateCell(IntVec3 loc)
    {
        if (loc.GetFirstThing(Map, entDef.blueprintDef) != null)
        {
            return false;
        }

        var roofAt = Map.roofGrid.RoofAt(loc);
        if (roofAt == null)
        {
            return true;
        }

        return (!roofAt.isThickRoof || ResearchProjectDefOf.ThickStoneRoofRemoval.IsFinished) && roofAt != roofDef;
    }

    public override void DesignateSingleCell(IntVec3 c)
    {
        if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(new EventPack(TutorTagDesignate, c)))
        {
            return;
        }

        if (DebugSettings.godMode)
        {
            if (entDef is TerrainDef def)
            {
                Map.terrainGrid.SetTerrain(c, def);
            }
            else
            {
                ThingDef stuff = null;
                var value = fiStuffDef.GetValue(this);
                if (value != null)
                {
                    stuff = (ThingDef)value;
                }

                var thing = ThingMaker.MakeThing((ThingDef)entDef, stuff);
                thing.SetFactionDirect(Faction.OfPlayer);
                GenSpawn.Spawn(thing, c, Map, placingRot);
            }
        }
        else
        {
            Map.areaManager.NoRoof[c] = false;
            ThingDef stuff2 = null;
            var value2 = fiStuffDef.GetValue(this);
            if (value2 != null)
            {
                stuff2 = (ThingDef)value2;
            }

            GenConstruct.PlaceBlueprintForBuild(entDef, c, Map, placingRot, Faction.OfPlayer, stuff2);
        }

        if (entDef is ThingDef { IsOrbitalTradeBeacon: true })
        {
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.BuildOrbitalTradeBeacon, KnowledgeAmount.Total);
        }

        if (TutorSystem.TutorialMode)
        {
            TutorSystem.Notify_Event(new EventPack(TutorTagDesignate, c));
        }

        if (entDef.PlaceWorkers == null)
        {
            return;
        }

        foreach (var placeWorker in entDef.PlaceWorkers)
        {
            placeWorker.PostPlace(Map, entDef, c, placingRot);
        }
    }

    public override void SelectedUpdate()
    {
        GenUI.RenderMouseoverBracket();
    }
}