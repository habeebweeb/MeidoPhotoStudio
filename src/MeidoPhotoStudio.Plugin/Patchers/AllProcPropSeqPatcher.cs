using System;
using System.Diagnostics.CodeAnalysis;

using HarmonyLib;

namespace MeidoPhotoStudio.Plugin;

public static class AllProcPropSeqPatcher
{
    public static event EventHandler<ProcStartEventArgs> SequenceStarting;

    public static event EventHandler<ProcStartEventArgs> SequenceEnded;

    [HarmonyPatch(typeof(Maid), "AllProcPropSeq")]
    [HarmonyPrefix]
    [SuppressMessage("StyleCop.Analyzers.NamingRules", "SA1313", Justification = "Harmony parameter")]
    private static void NotifyAllProcPropStarting(Maid __instance)
    {
        if (__instance.AllProcProp2Fase is 0 && __instance.AllProcProp2Cnt is 0 && !__instance.boModelChg)
            SequenceStarting?.Invoke(null, new(__instance));
    }

    [HarmonyPatch(typeof(Maid), "AllProcPropSeq")]
    [HarmonyPostfix]
    [SuppressMessage("StyleCop.Analyzers.NamingRules", "SA1313", Justification = "Harmony parameter")]
    private static void NotifyAllProcPropEnded(Maid __instance)
    {
        if (__instance.AllProcProp2Fase is 5 && !__instance.IsAllProcPropBusy)
            SequenceEnded?.Invoke(null, new(__instance));
    }
}
