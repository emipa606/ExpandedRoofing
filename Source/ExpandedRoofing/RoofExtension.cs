using Verse;

namespace ExpandedRoofing;

internal class RoofExtension : DefModExtension
{
    // These fields are populated by XML def deserialization (and, for sourceStuff, by DynamicDefs),
    // so they are never assigned directly in ordinary code paths.
#pragma warning disable CS0649
    public ThingDef spawnerDef;
    public float transparency;

    // Set on dynamically generated thick-stone roofs so material resolution never has to round-trip
    // through defName string surgery. Null on XML-authored roofs (they resolve by name convention).
    public ThingDef sourceStuff;
#pragma warning restore CS0649
}
