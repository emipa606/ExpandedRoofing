using RimWorld;
using UnityEngine;
using Verse;

namespace ExpandedRoofing;

public class Designator_AreaNoThickRoof : Designator_AreaNoRoof
{
    public Designator_AreaNoThickRoof()
    {
        defaultLabel = "DesignatorAreaNoThickRoof".Translate();
        defaultDesc = "DesignatorAreaNoThickRoofDesc".Translate();
        icon = ContentFinder<Texture2D>.Get("UI/Designators/NoRoofArea");
        soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
        soundDragChanged = null;
        soundSucceeded = SoundDefOf.Designate_ZoneAdd;
        useMouseIcon = true;
    }

    public override bool Visible => DebugSettings.godMode || ResearchProjectDefOf.ThickStoneRoofRemoval.IsFinished;

    public override AcceptanceReport CanDesignateCell(IntVec3 c)
    {
        if (!c.InBounds(Map))
        {
            return false;
        }

        if (c.Fogged(Map))
        {
            return false;
        }

        return !Map.areaManager.NoRoof[c];
    }
}