using System.Diagnostics.CodeAnalysis;

using HarmonyLib;
using MeidoPhotoStudio.Database.Background;

namespace MeidoPhotoStudio.Plugin.Core.Background;

public class BackgroundService
{
    private static bool internalBackgroundChange;
    private readonly BackgroundRepository backgroundRepository;

    public BackgroundService(BackgroundRepository backgroundRepository)
    {
        this.backgroundRepository = backgroundRepository ?? throw new ArgumentNullException(nameof(backgroundRepository));

        ChangingBackgroundExternal += OnChangingBackgroundExternal;
        ChangedBackgroundExternal += OnChangedBackgroundExternal;
    }

    public event EventHandler<BackgroundChangeEventArgs> ChangingBackground;

    public event EventHandler<BackgroundChangeEventArgs> ChangedBackground;

    private static event EventHandler<ExternalBackgroundChangeEventArgs> ChangingBackgroundExternal;

    private static event EventHandler<ExternalBackgroundChangeEventArgs> ChangedBackgroundExternal;

    public static BackgroundModel DefaultBackgroundModel { get; } = new(BackgroundCategory.COM3D2, "Theater");

    public Transform BackgroundTransform =>
        BackgroundManager.BgObject ? BackgroundManager.BgObject.transform : null;

    public BackgroundModel CurrentBackground { get; private set; }

    public bool BackgroundVisible
    {
        get
        {
            var backgroundObject = BackgroundManager.BgObject;

            return backgroundObject && backgroundObject.activeSelf;
        }

        set
        {
            var backgroundObject = BackgroundManager.BgObject;

            if (!backgroundObject)
                return;

            backgroundObject.SetActive(value);
        }
    }

    public Color BackgroundColour
    {
        get => GameMain.Instance.MainCamera.camera.backgroundColor;
        set => GameMain.Instance.MainCamera.camera.backgroundColor = value;
    }

    private static BgMgr BackgroundManager =>
        GameMain.Instance.BgMgr;

    public void ChangeBackground(BackgroundModel backgroundModel)
    {
        internalBackgroundChange = true;

        ChangingBackground?.Invoke(this, new(backgroundModel, BackgroundTransform));

        if (TryChangeBackground(backgroundModel))
        {
            CurrentBackground = backgroundModel;

            ChangedBackground?.Invoke(this, new(backgroundModel, BackgroundTransform));
        }
        else
        {
            Utility.LogWarning($"Could not change background {backgroundModel}");

            BackgroundManager.ChangeBg(DefaultBackgroundModel.AssetName);

            CurrentBackground = DefaultBackgroundModel;

            ChangedBackground?.Invoke(this, new(DefaultBackgroundModel, BackgroundTransform));
        }

        internalBackgroundChange = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BgMgr), nameof(BgMgr.ChangeBg))]
    [HarmonyPatch(typeof(BgMgr), nameof(BgMgr.ChangeBgMyRoom))]
    [SuppressMessage("StyleCop.Analyzers.NamingRules", "SA1313", Justification = "Harmony parameter")]
    private static void BgMgrChangingBackground(string __0)
    {
        if (internalBackgroundChange)
            return;

        ChangingBackgroundExternal?.Invoke(null, new(__0));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(BgMgr), nameof(BgMgr.ChangeBg))]
    [HarmonyPatch(typeof(BgMgr), nameof(BgMgr.ChangeBgMyRoom))]
    [SuppressMessage("StyleCop.Analyzers.NamingRules", "SA1313", Justification = "Harmony parameter")]
    private static void BgMgrChangedBackground(string __0)
    {
        if (internalBackgroundChange)
            return;

        ChangedBackgroundExternal?.Invoke(null, new(__0));
    }

    private bool TryChangeBackground(BackgroundModel backgroundInfo)
    {
        if (backgroundInfo.Category is BackgroundCategory.COM3D2 or BackgroundCategory.CM3D2)
            BackgroundManager.ChangeBg(backgroundInfo.AssetName);
        else if (backgroundInfo.Category is BackgroundCategory.MyRoomCustom)
            BackgroundManager.ChangeBgMyRoom(backgroundInfo.AssetName);

        return BackgroundTransform;
    }

    private void OnChangingBackgroundExternal(object sender, ExternalBackgroundChangeEventArgs args)
    {
        var model = backgroundRepository.GetByID(args.AssetName);

        if (model is null)
        {
            Utility.LogDebug($"Could not find background with id {args.AssetName}");

            return;
        }

        ChangingBackground?.Invoke(this, new(model, BackgroundTransform));
    }

    private void OnChangedBackgroundExternal(object sender, ExternalBackgroundChangeEventArgs args)
    {
        var model = backgroundRepository.GetByID(args.AssetName);

        if (model is null)
        {
            Utility.LogDebug($"Could not find background with id {args.AssetName}");

            return;
        }

        CurrentBackground = model;

        ChangedBackground?.Invoke(this, new(model, BackgroundTransform));
    }

    private class ExternalBackgroundChangeEventArgs : EventArgs
    {
        public ExternalBackgroundChangeEventArgs(string assetName) =>
            AssetName = assetName;

        public string AssetName { get; }
    }
}
