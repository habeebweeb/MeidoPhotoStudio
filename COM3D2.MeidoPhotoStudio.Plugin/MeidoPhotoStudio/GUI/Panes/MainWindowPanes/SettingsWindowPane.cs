using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class SettingsWindowPane : BaseMainWindowPane
    {
        private Button reloadTranslationButton;
        private KeyRebindButton[] rebindButtons;
        private static readonly string[] actionTranslationKeys;
        private static readonly string[] actionLabels;
        private static readonly string[] headerTranslationKeys = {
            "controls", "controlsGeneral", "controlsMaids", "controlsCamera", "controlsDragPoint", "controlsScene"
        };
        private static readonly Dictionary<string, string> headers = new Dictionary<string, string>();

        static SettingsWindowPane()
        {
            actionTranslationKeys = Enum.GetNames(typeof(MpsKey))
                .Select(action => char.ToLowerInvariant(action[0]) + action.Substring(1))
                .ToArray();
            actionLabels = new string[actionTranslationKeys.Length];
        }

        public SettingsWindowPane()
        {
            reloadTranslationButton = new Button("Reload Translation");
            reloadTranslationButton.ControlEvent += (s, a) => Translation.ReinitializeTranslation();

            rebindButtons = new KeyRebindButton[actionTranslationKeys.Length];

            for (int i = 0; i < rebindButtons.Length; i++)
            {
                MpsKey action = (MpsKey)i;
                KeyRebindButton button = new KeyRebindButton(KeyCode.None);
                button.ControlEvent += (s, a) => InputManager.Rebind(action, button.KeyCode);
                rebindButtons[i] = button;

                actionLabels[i] = Translation.Get("controls", actionTranslationKeys[i]);
            }

            for (int i = 0; i < headerTranslationKeys.Length; i++)
            {
                headers[headerTranslationKeys[i]] = Translation.Get("settingsHeaders", headerTranslationKeys[i]);
            }
        }

        protected override void ReloadTranslation()
        {
            for (int i = 0; i < rebindButtons.Length; i++)
            {
                actionLabels[i] = Translation.Get("controls", actionTranslationKeys[i]);
            }

            for (int i = 0; i < headerTranslationKeys.Length; i++)
            {
                headers[headerTranslationKeys[i]] = Translation.Get("settingsHeaders", headerTranslationKeys[i]);
            }
        }

        public override void Draw()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            MiscGUI.Header(headers["controls"]);
            MiscGUI.WhiteLine();

            MiscGUI.Header(headers["controlsGeneral"]);
            MiscGUI.WhiteLine();
            for (MpsKey key = MpsKey.Activate; key <= MpsKey.ToggleMessage; key++)
            {
                DrawSetting(key);
            }

            MiscGUI.Header(headers["controlsMaids"]);
            MiscGUI.WhiteLine();
            DrawSetting(MpsKey.MeidoUndressing);

            MiscGUI.Header(headers["controlsCamera"]);
            MiscGUI.WhiteLine();
            for (MpsKey key = MpsKey.CameraLayer; key <= MpsKey.CameraLoad; key++)
            {
                DrawSetting(key);
            }

            MiscGUI.Header(headers["controlsDragPoint"]);
            MiscGUI.WhiteLine();
            for (MpsKey key = MpsKey.DragSelect; key <= MpsKey.DragFinger; key++)
            {
                DrawSetting(key);
            }

            MiscGUI.Header(headers["controlsScene"]);
            MiscGUI.WhiteLine();
            for (MpsKey key = MpsKey.SaveScene; key <= MpsKey.OpenSceneManager; key++)
            {
                DrawSetting(key);
            }

            MiscGUI.WhiteLine();
            reloadTranslationButton.Draw();
            GUILayout.EndScrollView();
        }

        private void DrawSetting(MpsKey key)
        {
            int keyIndex = (int)key;
            GUILayout.BeginHorizontal();
            GUILayout.Label(actionLabels[keyIndex]);
            GUILayout.FlexibleSpace();
            rebindButtons[keyIndex].Draw(GUILayout.Width(90f));
            if (GUILayout.Button("×", GUILayout.ExpandWidth(false)))
            {
                rebindButtons[keyIndex].KeyCode = KeyCode.None;
                InputManager.Rebind(key, KeyCode.None);
            }
            GUILayout.EndHorizontal();
        }

        public override void UpdatePanes()
        {
            for (int i = 0; i < rebindButtons.Length; i++)
            {
                rebindButtons[i].KeyCode = InputManager.GetActionKey((MpsKey)i);
            }
        }
    }
}
