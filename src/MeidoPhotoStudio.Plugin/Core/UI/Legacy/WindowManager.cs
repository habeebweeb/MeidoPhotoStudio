using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Framework.UI;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

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

    private bool visible = true;

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
    {
        get => visible && GameMain.Instance.SysDlg.IsDecided;
        set => visible = value;
    }

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
        foreach (var window in windows.Values)
            window.Activate();

        enabled = true;
    }

    void IActivateable.Deactivate()
    {
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

        CharacterService.CallingCharacters += OnCallingCharacters;

        enabled = false;
    }

    private void OnDestroy()
    {
        ScreenSizeChecker.ScreenSizeChanged -= OnScreenSizeChanged;

        if (CharacterService is null)
            return;

        CharacterService.CallingCharacters -= OnCallingCharacters;
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

        GUIStyle GetStyleForWindow(Rect windowRect) =>
            windowRect.Contains(mousePosition)
                ? HoverWindowStyle
                : NormalWindowStyle;
    }

    private void Update()
    {
        if (Input.mouseScrollDelta.y is not 0f && MouseOverAnyWindow())
            Input.ResetInputAxes();
    }

    private void OnScreenSizeChanged(object sender, EventArgs e)
    {
        foreach (var window in windows.Values)
            window.OnScreenDimensionsChanged(new(Screen.width, Screen.height));
    }

    private void OnCallingCharacters(object sender, CharacterServiceEventArgs e)
    {
#if DEBUG
        return;
#else
        if (!PluginCore.Active)
            return;

        visible = false;

        CharacterService.CalledCharacters += OnCharactersCalled;

        void OnCharactersCalled(object sender, CharacterServiceEventArgs e)
        {
            visible = true;

            CharacterService.CalledCharacters -= OnCharactersCalled;
        }
#endif
    }

    private void OnCalledCharacters(object sender, CharacterServiceEventArgs e) =>
        Visible = true;
}
