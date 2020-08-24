using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal static class Modal
    {
        private static BaseWindow currentModal;
        public static bool Visible
        {
            get => currentModal?.Visible ?? false;
            set
            {
                if (currentModal == null) return;
                currentModal.Visible = value;
            }
        }

        public static void Show(BaseWindow modalWindow)
        {
            if (currentModal != null) Close();
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
            GUIStyle windowStyle = new GUIStyle(GUI.skin.box);
            currentModal.WindowRect = GUI.ModalWindow(
                currentModal.windowID, currentModal.WindowRect, currentModal.GUIFunc, "", windowStyle
            );
        }
    }
}
