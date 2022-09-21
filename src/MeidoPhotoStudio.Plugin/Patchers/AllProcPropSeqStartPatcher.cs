using System;
using System.Diagnostics.CodeAnalysis;

using HarmonyLib;

namespace MeidoPhotoStudio.Plugin;

public static class AllProcPropSeqStartPatcher
{
    public static event EventHandler<ProcStartEventArgs> SequenceStarting;

    public static event EventHandler<ProcStartEventArgs> SequenceEnded;

    [HarmonyPatch(typeof(Maid), "AllProcPropSeq")]
    [HarmonyPrefix]
    [SuppressMessage("StyleCop.Analyzers.NamingRules", "SA1313", Justification = "Harmony parameter")]
    private static void NotifyAllProcPropStarting(Maid __instance)
    {
        // TODO: Consider sending a patch to EditBodyLoadFix rather than relying on this brittle hack.
        // The check for boModelChg is needed because AllProcProp2Cnt gets reset to 0 before the next phase which causes
        // a second false start
        if (__instance.AllProcProp2Fase == 0 && __instance.AllProcProp2Cnt == 0 && !__instance.boModelChg)
        {
            SequenceStarting?.Invoke(null, new(__instance));
            if (__instance.GetProp(MPN.head).boDut)
                Utility.LogDebug("Face is being initialized");
        }
    }

    [HarmonyPatch(typeof(Maid), "AllProcPropSeq")]
    [HarmonyPostfix]
    [SuppressMessage("StyleCop.Analyzers.NamingRules", "SA1313", Justification = "Harmony parameter")]
    private static void NotifyAllProcPropEnded(Maid __instance)
    {
        if (__instance.AllProcProp2Fase == 5 && !__instance.IsAllProcPropBusy)
            SequenceEnded?.Invoke(null, new(__instance));
    }
}
