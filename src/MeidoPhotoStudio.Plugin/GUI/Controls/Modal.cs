using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public static class Modal
{
    private static BaseWindow currentModal;

    public static bool Visible
    {
        get => currentModal?.Visible ?? false;
        set
        {
            if (currentModal is null)
                return;

            currentModal.Visible = value;
        }
    }

    public static void Show(BaseWindow modalWindow)
    {
        if (currentModal is not null)
            Close();

        currentModal = modalWindow;
        Visible = true;
    }

    public static void Close()
    {
        Visible = false;
        currentModal = null;
    }

    public static void Draw()
    {
        var windowStyle = new GUIStyle(GUI.skin.box);

        currentModal.WindowRect =
            GUI.ModalWindow(currentModal.WindowID, currentModal.WindowRect, currentModal.GUIFunc, string.Empty, windowStyle);
    }
}
