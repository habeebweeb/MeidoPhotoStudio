using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Framework.UI;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;
using UnityEngine.UI;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class WindowManager : MonoBehaviour, IActivateable
{
    private const string NormalBase64 = """
        iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAQklEQVQ4y2NkQID/DKQBRgYGBgYW
        mGZxBrVGUnS/ZLj1n4GBgZGRHM1IhtQzMVAIRg0YNWBwGMBIQWaqh2UmirIzAAVvDp4SaVoYAAAA
        AElFTkSuQmCC
        """;

    private const string HoverBase64 =
        """
        iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAQklEQVQ4y2NkQID/DKQBRgYGBgYW
        mGZxBrVbpOh+yXDrPwMDAyMjOZqRDFFjYqAQjBowasDgMICRgsykBstMFGVnAHhxDvcAWCRZAAAA
        AElFTkSuQmCC
        """;

    private static readonly LazyStyle HoverWindowStyle = new(
        0,
        static () => new(GUI.skin.box)
        {
            normal = { background = BackgroundHover },
        });

    private static readonly LazyStyle NormalWindowStyle = new(
        0,
        static () => new(GUI.skin.box)
        {
            normal = { background = BackgroundNormal },
        });

    private static readonly LazyStyle DropdownWindowStyle = new(
        0,
        static () => new(GUI.skin.box)
        {
            normal = { background = BackgroundHover },
        });

    private static readonly Texture2D BackgroundNormal = UIUtility.LoadTextureFromBase64(16, 16, NormalBase64);

    private static readonly Texture2D BackgroundHover = UIUtility.LoadTextureFromBase64(16, 16, HoverBase64);

    private readonly Dictionary<Window, BaseWindow> windows = [];

    private GameObject blockerCanvasContainer;
    private Graphic uguiBlocker;
    private bool blockingOtherUIs;
#if !DEBUG
    private bool visible = true;
#endif

    public enum Window
    {
        Main,
        Message,
        Save,
        Settings,
    }

    internal PluginCore PluginCore { get; set; }

    internal CharacterService CharacterService { get; set; }

    private bool Visible
#if DEBUG
    { get; set; } = true;
#else
    {
        get => visible && PluginCore.Active && GameMain.Instance.SysDlg.IsDecided && !CharacterService.Busy;
        set => visible = value;
    }
#endif

    public BaseWindow this[Window id]
    {
        get => windows[id];
        set => windows[id] = value;
    }

    public bool MouseOverAnyWindow()
    {
        var mousePosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

        foreach (var window in windows.Values.Where(static window => window.Visible))
            if (window.WindowRect.Contains(mousePosition))
                return true;

        if (Modal.MouseOverModal(mousePosition))
            return true;

        if (DropdownHelper.MouseOverDropdown(mousePosition))
            return true;

        return false;
    }

    void IActivateable.Activate()
    {
        BlockOtherUIs(false);

        foreach (var window in windows.Values)
            window.Activate();

        enabled = true;
    }

    void IActivateable.Deactivate()
    {
        BlockOtherUIs(false);

        foreach (var window in windows.Values)
            window.Deactivate();

        DropdownHelper.CloseDropdown();
        Modal.Close();

        enabled = false;
    }

    private void Awake() =>
        ScreenSizeChecker.ScreenSizeChanged += OnScreenSizeChanged;

    private void Start()
    {
        if (!PluginCore)
            throw new InvalidOperationException($"{nameof(PluginCore)} cannot be null");

        if (CharacterService is null)
            throw new InvalidOperationException($"{nameof(CharacterService)} cannot be null");

        (blockerCanvasContainer, uguiBlocker) = InitializeBlocker();

        enabled = false;

        static (GameObject BlockerCanvas, Graphic BlockerGraphic) InitializeBlocker()
        {
            var blockerCanvas = new GameObject("[MPS Click Blocker Canvas]", typeof(RectTransform))
            {
                layer = 5,
            };

            var canvas = blockerCanvas.AddComponent<Canvas>();

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;

            DontDestroyOnLoad(blockerCanvas);
            blockerCanvas.AddComponent<GraphicRaycaster>();

            var blockerGameObject = new GameObject("Blocker Graphic", typeof(RectTransform))
            {
                layer = 5,
            };

            blockerGameObject.transform.SetParent(blockerCanvas.transform, true);

            var rectTransform = (RectTransform)blockerGameObject.transform;

            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = new(0f, 0f);
            rectTransform.anchorMin = new(0f, 0f);
            rectTransform.anchorMax = new(1f, 1f);
            rectTransform.pivot = new(0f, 1f);

            var blockerGraphic = blockerGameObject.AddComponent<RaycastTarget>();

            blockerGraphic.color = Color.black with { a = 0.6f };
            blockerGraphic.raycastTarget = true;

            blockerGraphic.enabled = false;

            return (blockerCanvas, blockerGraphic);
        }
    }

    private void OnDestroy()
    {
        ScreenSizeChecker.ScreenSizeChanged -= OnScreenSizeChanged;

        if (CharacterService is null)
            return;

        BlockOtherUIs(false);

        if (blockerCanvasContainer)
            Destroy(blockerCanvasContainer);
    }

    private void OnGUI()
    {
        if (!Visible)
            return;

        var mousePosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

        foreach (var window in windows.Values)
        {
            if (!window.Visible)
                continue;

            var windowStyle = GetStyleForWindow(window.WindowRect);

            window.WindowRect = GUI.Window(window.ID, window.WindowRect, window.GUIFunc, string.Empty, windowStyle);
        }

        if (Modal.Visible)
            Modal.Draw(GetStyleForWindow(Modal.WindowRect));

        if (DropdownHelper.Visible)
            DropdownHelper.DrawDropdown(DropdownWindowStyle);

        if (Event.current.type is EventType.Repaint)
        {
            var mouseOverUI = MouseOverAnyWindow();

            if (!blockingOtherUIs && mouseOverUI)
                BlockOtherUIs(true);
            else if (blockingOtherUIs && !mouseOverUI)
                BlockOtherUIs(false);
        }

        GUIStyle GetStyleForWindow(Rect windowRect) =>
            windowRect.Contains(mousePosition)
                ? HoverWindowStyle
                : NormalWindowStyle;
    }

    private void Update()
    {
        if (!Visible)
        {
            if (blockingOtherUIs)
                BlockOtherUIs(false);

            return;
        }

        if (Input.mouseScrollDelta.y is not 0f && MouseOverAnyWindow())
            Input.ResetInputAxes();
    }

    private void BlockOtherUIs(bool block)
    {
        if (blockingOtherUIs == block)
            return;

        blockingOtherUIs = block;

        BlockNGUI(block);
        uguiBlocker.enabled = block;

        static void BlockNGUI(bool block)
        {
            foreach (var camera in UICamera.list)
            {
                if (!camera.enabled || !NGUITools.GetActive(camera.gameObject) || !camera.EnableProcess)
                    continue;

                if (block && UICamera.mHover)
                    camera.Hover = false;

                camera.useMouse = !block;
            }

            if (block && UICamera.mHover != UICamera.fallThrough)
            {
                UICamera.Notify(UICamera.mHover, "OnHover", false);

                var lastHover = UICamera.mHover;
                UICamera.mHover = UICamera.fallThrough;

                for (var i = 0; i < UICamera.mMouse.Length; i++)
                {
                    UICamera.mMouse[i].last = lastHover;
                    UICamera.mMouse[i].current = UICamera.fallThrough;
                }
            }
        }
    }

    private void OnScreenSizeChanged(object sender, EventArgs e)
    {
        foreach (var window in windows.Values)
            window.OnScreenDimensionsChanged(new(Screen.width, Screen.height));
    }

    private class RaycastTarget : Graphic
    {
        public override void UpdateGeometry()
        {
        }

        public override void SetMaterialDirty()
        {
        }

        public override void SetVerticesDirty()
        {
        }
    }
}
