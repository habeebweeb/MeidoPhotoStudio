using System;
using System.Diagnostics.CodeAnalysis;

using HarmonyLib;

namespace MeidoPhotoStudio.Plugin;

// TODO: Extend this further to potentially reduce the need for coroutines that wait for maid proc state
public static class AllProcPropSeqStartPatcher
{
    public static event EventHandler<ProcStartEventArgs> SequenceStart;

    [HarmonyPatch(typeof(Maid), nameof(Maid.AllProcPropSeqStart))]
    [HarmonyPostfix]
    [SuppressMessage("StyleCop.Analyzers.NamingRules", "SA1313", Justification = "Harmony parameter")]
    private static void NotifyProcStart(Maid __instance) =>
        SequenceStart?.Invoke(null, new(__instance));
}
