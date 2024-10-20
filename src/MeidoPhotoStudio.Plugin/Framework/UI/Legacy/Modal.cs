namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public static class Modal
{
    private static BaseWindow currentModal;
    private static GUIStyle windowStyle;

    internal static bool Visible
    {
        get => currentModal?.Visible ?? false;
        set
        {
            if (currentModal is null)
                return;

            currentModal.Visible = value;
        }
    }

    private static GUIStyle WindowStyle =>
        windowStyle ??= new(GUI.skin.box);

    internal static void Show(BaseWindow modalWindow)
    {
        if (currentModal is not null)
            Close();

        currentModal = modalWindow;
        Visible = true;
    }

    internal static void Close()
    {
        Visible = false;
        currentModal = null;
    }

    internal static void Update()
    {
        if (UnityEngine.Input.mouseScrollDelta.y is 0f || !Visible)
            return;

        var mousePos = new Vector2(UnityEngine.Input.mousePosition.x, Screen.height - UnityEngine.Input.mousePosition.y);

        if (currentModal.WindowRect.Contains(mousePos))
            UnityEngine.Input.ResetInputAxes();
    }

    internal static void Draw()
    {
        if (!Visible)
            return;

        currentModal.WindowRect =
            GUI.ModalWindow(currentModal.WindowID, currentModal.WindowRect, currentModal.GUIFunc, string.Empty, WindowStyle);
    }

    internal static bool MouseOverModal()
    {
        if (!Visible)
            return false;

        var mousePosition = new Vector2(UnityEngine.Input.mousePosition.x, Screen.height - UnityEngine.Input.mousePosition.y);

        return currentModal.WindowRect.Contains(mousePosition);
    }
}
