namespace MeidoPhotoStudio.Plugin;

public static class Modal
{
    private static BaseWindow currentModal;

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

        var windowStyle = new GUIStyle(GUI.skin.box);

        currentModal.WindowRect =
            GUI.ModalWindow(currentModal.WindowID, currentModal.WindowRect, currentModal.GUIFunc, string.Empty, windowStyle);
    }
}
