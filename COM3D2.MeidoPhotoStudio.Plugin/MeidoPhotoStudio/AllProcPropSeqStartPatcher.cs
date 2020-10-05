using System;
using HarmonyLib;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    // TODO: Extend this further to potentially reduce the need for coroutines that wait for maid proc state
    public static class AllProcPropSeqStartPatcher
    {
        public static event EventHandler<ProcStartEventArgs> SequenceStart;

        [HarmonyPatch(typeof(Maid), nameof(Maid.AllProcPropSeqStart))]
        [HarmonyPostfix]
        private static void NotifyProcStart(Maid __instance)
        {
            SequenceStart?.Invoke(null, new ProcStartEventArgs(__instance));
        }
    }

    public class ProcStartEventArgs : EventArgs
    {
        public readonly Maid maid;
        public ProcStartEventArgs(Maid maid) => this.maid = maid;
    }
}
