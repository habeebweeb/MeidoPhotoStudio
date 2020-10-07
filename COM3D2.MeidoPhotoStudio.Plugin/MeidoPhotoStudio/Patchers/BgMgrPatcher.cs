using System;
using HarmonyLib;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public static class BgMgrPatcher
    {
        public static event EventHandler ChangeBgBegin;
        public static event EventHandler ChangeBgEnd;

        [HarmonyPatch(typeof(BgMgr), nameof(BgMgr.ChangeBg))]
        [HarmonyPatch(typeof(BgMgr), nameof(BgMgr.ChangeBgMyRoom))]
        [HarmonyPrefix]
        private static void NotifyBeginChangeBg() => ChangeBgBegin?.Invoke(null, EventArgs.Empty);

        [HarmonyPatch(typeof(BgMgr), nameof(BgMgr.ChangeBg))]
        [HarmonyPatch(typeof(BgMgr), nameof(BgMgr.ChangeBgMyRoom))]
        [HarmonyPostfix]
        private static void NotifyEndChangeBg() => ChangeBgEnd?.Invoke(null, EventArgs.Empty);
    }
}
