using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace ExpandedRoofing;

public static class InjectedDefHasher
{
    private static GiveShortHash giveShortHashDelegate;

    internal static void PrepareReflection()
    {
        if (typeof(ShortHashGiver).GetField("takenHashesPerDeftype", BindingFlags.Static | BindingFlags.NonPublic)
                ?.GetValue(null) is not Dictionary<Type, HashSet<ushort>> takenHashesDictionary)
        {
            throw new Exception("taken hashes");
        }

        var method = typeof(ShortHashGiver).GetMethod("GiveShortHash", BindingFlags.Static | BindingFlags.NonPublic,
            null, [
                typeof(Def),
                typeof(Type),
                typeof(HashSet<ushort>)
            ], null);
        if (method == null)
        {
            throw new Exception("hashing method");
        }

        var hashDelegate = (GiveShortHashTakenHashes)Delegate.CreateDelegate(typeof(GiveShortHashTakenHashes), method);
        giveShortHashDelegate = delegate(Def def, Type defType)
        {
            var hashSet = takenHashesDictionary.TryGetValue(defType);
            if (hashSet == null)
            {
                hashSet = [];
                takenHashesDictionary.Add(defType, hashSet);
            }

            hashDelegate(def, defType, hashSet);
        };
    }

    public static void GiveShortHasToDef(Def newDef, Type defType)
    {
        if (giveShortHashDelegate == null)
        {
            throw new Exception("Hasher not initalized");
        }

        giveShortHashDelegate(newDef, defType);
    }

    private delegate void GiveShortHashTakenHashes(Def def, Type defType, HashSet<ushort> takenHashes);

    private delegate void GiveShortHash(Def def, Type defType);
}