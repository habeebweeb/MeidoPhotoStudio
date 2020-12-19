using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using Input = InputManager;
    public class EnvironmentManager : IManager
    {
        private static readonly BgMgr bgMgr = GameMain.Instance.BgMgr;
        public const string header = "ENVIRONMENT";
        public const string defaultBg = "Theater";
        private const string myRoomPrefix = "マイルーム:";
        private static bool cubeActive;
        public static bool CubeActive
        {
            get => cubeActive;
            set
            {
                if (value == cubeActive) return;
                cubeActive = value;
                CubeActiveChange?.Invoke(null, EventArgs.Empty);
            }
        }
        private static bool cubeSmall;
        public static bool CubeSmall
        {
            get => cubeSmall;
            set
            {
                if (value == cubeSmall) return;
                cubeSmall = value;
                CubeSmallChange?.Invoke(null, EventArgs.Empty);
            }
        }
        private static event EventHandler CubeActiveChange;
        private static event EventHandler CubeSmallChange;
        private Transform bg;
        private GameObject bgObject;
        private DragPointBG bgDragPoint;
        public string CurrentBgAsset { get; private set; } = defaultBg;
        private bool bgVisible = true;
        public bool BGVisible
        {
            get => bgVisible;
            set
            {
                bgVisible = value;
                bgObject.SetActive(bgVisible);
            }
        }

        public EnvironmentManager()
        {
            DragPointLight.EnvironmentManager = this;
            Activate();
        }

        public void Update() { }

        public void Activate()
        {
            BgMgrPatcher.ChangeBgBegin += OnChangeBegin;
            BgMgrPatcher.ChangeBgEnd += OnChangeEnd;

            bgObject = bgMgr.Parent;

            bgObject.SetActive(true);
            
            if (MeidoPhotoStudio.EditMode) UpdateBG();
            else ChangeBackground(defaultBg);

            CubeSmallChange += OnCubeSmall;
            CubeActiveChange += OnCubeActive;
        }

        public void Deactivate()
        {
            BgMgrPatcher.ChangeBgBegin -= OnChangeBegin;
            BgMgrPatcher.ChangeBgEnd -= OnChangeEnd;

            DestroyDragPoint();
            BGVisible = true;

            if (MeidoPhotoStudio.EditMode) bgMgr.ChangeBg(defaultBg);
            else
            {
                var isNight = GameMain.Instance.CharacterMgr.status.GetFlag("時間帯") == 3;
                bgMgr.ChangeBg(isNight ? "ShinShitsumu_ChairRot_Night" : "ShinShitsumu_ChairRot");
            }

            if (bgMgr.BgObject) bgMgr.BgObject.transform.localScale = Vector3.one;

            CubeSmallChange -= OnCubeSmall;
            CubeActiveChange -= OnCubeActive;
        }

        public void ChangeBackground(string assetName, bool creative = false)
        {
            if (creative) bgMgr.ChangeBgMyRoom(assetName);
            else bgMgr.ChangeBg(assetName);
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

        private void OnChangeBegin(object sender, EventArgs args) => DestroyDragPoint();

        private void OnChangeEnd(object sender, EventArgs args) => UpdateBG();

        private void UpdateBG()
        {
            if (!bgMgr.BgObject) return;

            CurrentBgAsset = bgMgr.GetBGName();
            if (CurrentBgAsset.StartsWith(myRoomPrefix))
                CurrentBgAsset = CurrentBgAsset.Replace(myRoomPrefix, string.Empty);
            bg = bgMgr.BgObject.transform;
            AttachDragPoint(bg);
        }

        private void DestroyDragPoint()
        {
            if (bgDragPoint) Object.Destroy(bgDragPoint.gameObject);
        }

        private void OnCubeSmall(object sender, EventArgs args)
        {
            bgDragPoint.DragPointScale = CubeSmall ? DragPointGeneral.smallCube : 1f;
        }

        private void OnCubeActive(object sender, EventArgs args)
        {
            bgDragPoint.gameObject.SetActive(CubeActive);
        }
    }
}
