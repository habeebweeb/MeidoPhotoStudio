using System;

using MeidoPhotoStudio.Plugin.Core;

using UnityEngine;

using Object = UnityEngine.Object;

namespace MeidoPhotoStudio.Plugin;

public class EnvironmentManager : IManager
{
    public const string Header = "ENVIRONMENT";
    public const string DefaultBg = "Theater";

    private const string MyRoomPrefix = "マイルーム:";

    private static readonly BgMgr BgMgr = GameMain.Instance.BgMgr;

    private static bool cubeActive;
    private static bool cubeSmall;

    private Transform bg;
    private GameObject bgObject;
    private DragPointBG bgDragPoint;
    private bool bgVisible = true;

    public EnvironmentManager()
    {
        DragPointLight.EnvironmentManager = this;
        Activate();
    }

    private static event EventHandler CubeActiveChange;

    private static event EventHandler CubeSmallChange;

    public static bool CubeActive
    {
        get => cubeActive;
        set
        {
            if (value == cubeActive)
                return;

            cubeActive = value;
            CubeActiveChange?.Invoke(null, EventArgs.Empty);
        }
    }

    public static bool CubeSmall
    {
        get => cubeSmall;
        set
        {
            if (value == cubeSmall)
                return;

            cubeSmall = value;
            CubeSmallChange?.Invoke(null, EventArgs.Empty);
        }
    }

    public string CurrentBgAsset { get; private set; } = DefaultBg;

    public bool BGVisible
    {
        get => bgVisible;
        set
        {
            bgVisible = value;
            bgObject.SetActive(bgVisible);
        }
    }

    public void Update()
    {
    }

    public void Activate()
    {
        BgMgrPatcher.ChangeBgBegin += OnChangeBegin;
        BgMgrPatcher.ChangeBgEnd += OnChangeEnd;

        bgObject = BgMgr.Parent;

        bgObject.SetActive(true);

        if (PluginCore.EditMode)
            UpdateBG();
        else
            ChangeBackground(DefaultBg);

        CubeSmallChange += OnCubeSmall;
        CubeActiveChange += OnCubeActive;
    }

    public void Deactivate()
    {
        BgMgrPatcher.ChangeBgBegin -= OnChangeBegin;
        BgMgrPatcher.ChangeBgEnd -= OnChangeEnd;

        DestroyDragPoint();
        BGVisible = true;

        if (PluginCore.EditMode)
        {
            BgMgr.ChangeBg(DefaultBg);
        }
        else
        {
            var isNight = GameMain.Instance.CharacterMgr.status.GetFlag("時間帯") is 3;

            BgMgr.ChangeBg(isNight ? "ShinShitsumu_ChairRot_Night" : "ShinShitsumu_ChairRot");
        }

        if (BgMgr.BgObject)
            BgMgr.BgObject.transform.localScale = Vector3.one;

        CubeSmallChange -= OnCubeSmall;
        CubeActiveChange -= OnCubeActive;
    }

    public void ChangeBackground(string assetName, bool creative = false)
    {
        if (creative)
            BgMgr.ChangeBgMyRoom(assetName);
        else
            BgMgr.ChangeBg(assetName);
    }

    private void AttachDragPoint(Transform bgTransform)
    {
        bgDragPoint = DragPoint.Make<DragPointBG>(PrimitiveType.Cube, Vector3.one * 0.12f);
        bgDragPoint.Initialize(() => bgTransform.position, () => Vector3.zero);
        bgDragPoint.Set(bgTransform);
        bgDragPoint.AddGizmo();
        bgDragPoint.ConstantScale = true;
        bgDragPoint.gameObject.SetActive(CubeActive);
    }

    private void OnChangeBegin(object sender, EventArgs args) =>
        DestroyDragPoint();

    private void OnChangeEnd(object sender, EventArgs args) =>
        UpdateBG();

    private void UpdateBG()
    {
        if (!BgMgr.BgObject)
            return;

        CurrentBgAsset = BgMgr.GetBGName();

        if (CurrentBgAsset.StartsWith(MyRoomPrefix))
            CurrentBgAsset = CurrentBgAsset.Replace(MyRoomPrefix, string.Empty);

        bg = BgMgr.BgObject.transform;

        AttachDragPoint(bg);
    }

    private void DestroyDragPoint()
    {
        if (bgDragPoint)
            Object.Destroy(bgDragPoint.gameObject);
    }

    private void OnCubeSmall(object sender, EventArgs args) =>
        bgDragPoint.DragPointScale = CubeSmall ? DragPointGeneral.SmallCube : 1f;

    private void OnCubeActive(object sender, EventArgs args) =>
        bgDragPoint.gameObject.SetActive(CubeActive);
}
