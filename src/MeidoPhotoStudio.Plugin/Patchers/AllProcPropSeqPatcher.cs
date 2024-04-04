using System;

using HarmonyLib;

namespace MeidoPhotoStudio.Plugin;

#pragma warning disable SA1313, IDE0051
public static class AllProcPropSeqPatcher
{
    public static event EventHandler<ProcStartEventArgs> SequenceStarting;

    public static event EventHandler<ProcStartEventArgs> SequenceEnded;

    [HarmonyPatch(typeof(Maid), nameof(Maid.AllProcPropSeqStart))]
    [HarmonyPrefix]
    private static void AllProcPropSeqStartPrefix(Maid __instance) =>
        SequenceStarting?.Invoke(null, new(__instance));

    [HarmonyPatch(typeof(Maid), "AllProcPropSeq")]
    [HarmonyPostfix]
    private static void AllProcPropSeqPostfix(Maid __instance)
    {
        if (__instance is not { AllProcProp2Fase: 5, IsAllProcPropBusy: false })
            return;

        SequenceEnded?.Invoke(null, new(__instance));
    }

    [HarmonyPatch(typeof(Maid), nameof(Maid.AllProcProp))]
    [HarmonyPrefix]
    private static void AllProcPropPrefix(Maid __instance) =>
        SequenceStarting?.Invoke(null, new(__instance));

    [HarmonyPatch(typeof(Maid), nameof(Maid.AllProcProp))]
    [HarmonyPostfix]
    private static void AllProcPropPostfix(Maid __instance) =>
        SequenceEnded?.Invoke(null, new(__instance));
}
#pragma warning restore
