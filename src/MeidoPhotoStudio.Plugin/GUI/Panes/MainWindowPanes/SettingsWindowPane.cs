using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class SettingsWindowPane : BaseMainWindowPane
{
    private static readonly string[] HeaderTranslationKeys =
    {
        "controls", "controlsGeneral", "controlsMaids", "controlsCamera", "controlsDragPoint", "controlsScene",
    };

    private static readonly Dictionary<string, string> Headers = new();
    private static readonly string[] ActionTranslationKeys;
    private static readonly string[] ActionLabels;

    private readonly Button reloadTranslationButton;
    private readonly Button reloadAllPresetsButton;
    private readonly KeyRebindButton[] rebindButtons;

    static SettingsWindowPane()
    {
        ActionTranslationKeys = Enum.GetNames(typeof(MpsKey))
            .Select(action => char.ToLowerInvariant(action[0]) + action.Substring(1))
            .ToArray();

        ActionLabels = new string[ActionTranslationKeys.Length];
    }

    public SettingsWindowPane()
    {
        rebindButtons = new KeyRebindButton[ActionTranslationKeys.Length];

        for (var i = 0; i < rebindButtons.Length; i++)
        {
            var action = (MpsKey)i;
            var button = new KeyRebindButton(KeyCode.None);

            button.ControlEvent += (_, _) =>
                InputManager.Rebind(action, button.KeyCode);

            rebindButtons[i] = button;

            ActionLabels[i] = Translation.Get("controls", ActionTranslationKeys[i]);
        }

        for (var i = 0; i < HeaderTranslationKeys.Length; i++)
            Headers[HeaderTranslationKeys[i]] = Translation.Get("settingsHeaders", HeaderTranslationKeys[i]);

        reloadTranslationButton = new(Translation.Get("settingsLabels", "reloadTranslation"));
        reloadTranslationButton.ControlEvent += (_, _) =>
            Translation.ReinitializeTranslation();

        reloadAllPresetsButton = new(Translation.Get("settingsLabels", "reloadAllPresets"));
        reloadAllPresetsButton.ControlEvent += (_, _) =>
        {
            Constants.InitializeCustomFaceBlends();
            Constants.InitializeHandPresets();
            Constants.InitializeCustomPoses();
        };
    }

    public override void Draw()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);

        MpsGui.Header(Headers["controls"]);
        MpsGui.WhiteLine();

        MpsGui.Header(Headers["controlsGeneral"]);
        MpsGui.WhiteLine();

        for (var key = MpsKey.Activate; key <= MpsKey.ToggleMessage; key++)
            DrawSetting(key);

        MpsGui.Header(Headers["controlsMaids"]);
        MpsGui.WhiteLine();
        DrawSetting(MpsKey.MeidoUndressing);

        MpsGui.Header(Headers["controlsCamera"]);
        MpsGui.WhiteLine();

        for (var key = MpsKey.CameraLayer; key <= MpsKey.CameraLoad; key++)
            DrawSetting(key);

        MpsGui.Header(Headers["controlsDragPoint"]);
        MpsGui.WhiteLine();

        for (var key = MpsKey.DragSelect; key <= MpsKey.DragFinger; key++)
            DrawSetting(key);

        MpsGui.Header(Headers["controlsScene"]);
        MpsGui.WhiteLine();

        for (var key = MpsKey.SaveScene; key <= MpsKey.OpenSceneManager; key++)
            DrawSetting(key);

        GUI.enabled = !InputManager.Listening;

        // Translation settings
        MpsGui.WhiteLine();
        reloadTranslationButton.Draw();

        reloadAllPresetsButton.Draw();

        GUILayout.EndScrollView();

        GUI.enabled = true;
    }

    public override void UpdatePanes()
    {
        for (var i = 0; i < rebindButtons.Length; i++)
            rebindButtons[i].KeyCode = InputManager.GetActionKey((MpsKey)i);
    }

    protected override void ReloadTranslation()
    {
        for (var i = 0; i < rebindButtons.Length; i++)
            ActionLabels[i] = Translation.Get("controls", ActionTranslationKeys[i]);

        for (var i = 0; i < HeaderTranslationKeys.Length; i++)
            Headers[HeaderTranslationKeys[i]] = Translation.Get("settingsHeaders", HeaderTranslationKeys[i]);

        reloadTranslationButton.Label = Translation.Get("settingsLabels", "reloadTranslation");
        reloadAllPresetsButton.Label = Translation.Get("settingsLabels", "reloadAllPresets");
    }

    private void DrawSetting(MpsKey key)
    {
        var keyIndex = (int)key;

        GUILayout.BeginHorizontal();
        GUILayout.Label(ActionLabels[keyIndex]);
        GUILayout.FlexibleSpace();
        rebindButtons[keyIndex].Draw(GUILayout.Width(90f));

        if (GUILayout.Button("×", GUILayout.ExpandWidth(false)))
        {
            rebindButtons[keyIndex].KeyCode = KeyCode.None;
            InputManager.Rebind(key, KeyCode.None);
        }

        GUILayout.EndHorizontal();
    }
}
