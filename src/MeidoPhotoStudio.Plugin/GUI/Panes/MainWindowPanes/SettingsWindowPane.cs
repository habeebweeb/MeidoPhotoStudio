using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    public class SettingsWindowPane : BaseMainWindowPane
    {
        private static readonly string[] headerTranslationKeys = {
            "controls", "controlsGeneral", "controlsMaids", "controlsCamera", "controlsDragPoint", "controlsScene"
        };
        private static readonly Dictionary<string, string> headers = new Dictionary<string, string>();
        private static readonly string[] actionTranslationKeys;
        private static readonly string[] actionLabels;
        private readonly Button reloadTranslationButton;
        private readonly Button reloadAllPresetsButton;
        private readonly KeyRebindButton[] rebindButtons;

        static SettingsWindowPane()
        {
            actionTranslationKeys = Enum.GetNames(typeof(MpsKey))
                .Select(action => char.ToLowerInvariant(action[0]) + action.Substring(1))
                .ToArray();
            actionLabels = new string[actionTranslationKeys.Length];
        }

        public SettingsWindowPane()
        {
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

            reloadTranslationButton = new Button(Translation.Get("settingsLabels", "reloadTranslation"));
            reloadTranslationButton.ControlEvent += (s, a) => Translation.ReinitializeTranslation();

            reloadAllPresetsButton = new Button(Translation.Get("settingsLabels", "reloadAllPresets"));
            reloadAllPresetsButton.ControlEvent += (s, a) =>
            {
                Constants.InitializeCustomFaceBlends();
                Constants.InitializeHandPresets();
                Constants.InitializeCustomPoses();
            };
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

            reloadTranslationButton.Label = Translation.Get("settingsLabels", "reloadTranslation");
            reloadAllPresetsButton.Label = Translation.Get("settingsLabels", "reloadAllPresets");
        }

        public override void Draw()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            MpsGui.Header(headers["controls"]);
            MpsGui.WhiteLine();

            MpsGui.Header(headers["controlsGeneral"]);
            MpsGui.WhiteLine();
            for (MpsKey key = MpsKey.Activate; key <= MpsKey.ToggleMessage; key++)
            {
                DrawSetting(key);
            }

            MpsGui.Header(headers["controlsMaids"]);
            MpsGui.WhiteLine();
            DrawSetting(MpsKey.MeidoUndressing);

            MpsGui.Header(headers["controlsCamera"]);
            MpsGui.WhiteLine();
            for (MpsKey key = MpsKey.CameraLayer; key <= MpsKey.CameraLoad; key++)
            {
                DrawSetting(key);
            }

            MpsGui.Header(headers["controlsDragPoint"]);
            MpsGui.WhiteLine();
            for (MpsKey key = MpsKey.DragSelect; key <= MpsKey.DragFinger; key++)
            {
                DrawSetting(key);
            }

            MpsGui.Header(headers["controlsScene"]);
            MpsGui.WhiteLine();
            for (MpsKey key = MpsKey.SaveScene; key <= MpsKey.OpenSceneManager; key++)
            {
                DrawSetting(key);
            }

            GUI.enabled = !InputManager.Listening;

            // Translation settings
            MpsGui.WhiteLine();
            reloadTranslationButton.Draw();

            reloadAllPresetsButton.Draw();

            GUILayout.EndScrollView();

            GUI.enabled = true;
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
