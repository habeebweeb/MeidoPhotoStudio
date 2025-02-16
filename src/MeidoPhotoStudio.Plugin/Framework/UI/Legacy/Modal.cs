namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public static class Modal
{
    private static BaseWindow currentModal;
    private static GUIStyle windowStyle;

    internal static bool Visible { get; private set; }

    internal static int ID =>
        Visible ? currentModal.ID : -1;

    internal static Rect WindowRect =>
        Visible ? currentModal.WindowRect : Rect.zero;

    private static GUIStyle WindowStyle =>
        windowStyle ??= new(GUI.skin.box);

    internal static void Show(BaseWindow modalWindow)
    {
        _ = modalWindow ?? throw new ArgumentNullException(nameof(modalWindow));

        Close();
        currentModal = modalWindow;
        currentModal.Visible = true;
        Visible = true;
    }

    internal static void Close()
    {
        Visible = false;
        currentModal = null;
    }

    internal static void Draw() =>
        Draw(WindowStyle);

    internal static void Draw(GUIStyle style)
    {
        if (!Visible)
            return;

        currentModal.WindowRect =
            GUI.Window(currentModal.ID, currentModal.WindowRect, currentModal.GUIFunc, string.Empty, style);

        GUI.BringWindowToFront(currentModal.ID);
    }

    internal static bool MouseOverModal(Vector3 mousePosition) =>
        Visible && currentModal.WindowRect.Contains(mousePosition);
}
