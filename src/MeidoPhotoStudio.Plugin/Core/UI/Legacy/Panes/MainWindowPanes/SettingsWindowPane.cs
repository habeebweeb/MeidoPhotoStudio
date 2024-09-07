using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;
using MeidoPhotoStudio.Plugin.Input;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class SettingsWindowPane : BaseMainWindowPane
{
    private readonly Dictionary<Hotkey, string> hotkeyMapping;
    private readonly Dictionary<Hotkey, string> hotkeyName;
    private readonly InputConfiguration inputConfiguration;
    private readonly InputRemapper inputRemapper;
    private readonly Button reloadTranslationButton;
    private readonly Dictionary<SettingHeader, string> settingHeaders;
    private readonly Dictionary<Shortcut, string> shortcutMapping;
    private readonly Dictionary<Shortcut, string> shortcutName;
    private readonly LazyStyle headerStyle = new(
        14,
        () => new(GUI.skin.label)
        {
            padding = new(7, 0, 0, -5),
            normal = { textColor = Color.white },
        });

    private readonly LazyStyle labelStyle = new(13, () => new(GUI.skin.label));
    private readonly LazyStyle leftMargin = new(
        0,
        () => new()
        {
            margin = new(8, 0, 0, 0),
        });

    private Hotkey currentHotkey;
    private Shortcut currentShortcut;
    private bool listeningToShortcut;
    private string cancelRebindLabel = "Cancel";
    private string pushAnyKeyLabel = "Push any key combo";

    public SettingsWindowPane(InputConfiguration inputConfiguration, InputRemapper inputRemapper)
    {
        this.inputConfiguration = inputConfiguration ?? throw new ArgumentNullException(nameof(inputConfiguration));
        this.inputRemapper = inputRemapper ? inputRemapper : throw new ArgumentNullException(nameof(inputRemapper));

        settingHeaders = ((SettingHeader[])Enum.GetValues(typeof(SettingHeader)))
            .ToDictionary(
                setting => setting,
                setting => Translation.Get("settingsHeaders", EnumToLower(setting)),
                EnumEqualityComparer<SettingHeader>.Instance);

        var shortcutValues = (Shortcut[])Enum.GetValues(typeof(Shortcut));

        shortcutMapping = shortcutValues
            .ToDictionary(
                shortcut => shortcut,
                shortcut => inputConfiguration[shortcut].ToString(),
                EnumEqualityComparer<Shortcut>.Instance);

        shortcutName = shortcutValues
            .ToDictionary(
                shortcut => shortcut,
                shortcut => Translation.Get("controls", EnumToLower(shortcut)),
                EnumEqualityComparer<Shortcut>.Instance);

        var hotkeyValues = (Hotkey[])Enum.GetValues(typeof(Hotkey));

        hotkeyMapping = hotkeyValues
            .ToDictionary(
                hotkey => hotkey,
                hotkey => inputConfiguration[hotkey].ToString(),
                EnumEqualityComparer<Hotkey>.Instance);

        hotkeyName = hotkeyValues
            .ToDictionary(
                hotkey => hotkey,
                hotkey => Translation.Get("controls", EnumToLower(hotkey)),
                EnumEqualityComparer<Hotkey>.Instance);

        reloadTranslationButton = new(Translation.Get("settingsLabels", "reloadTranslation"));
        reloadTranslationButton.ControlEvent += (_, _) =>
            Translation.ReinitializeTranslation();

        pushAnyKeyLabel = Translation.Get("settingsLabels", "pushAnyKey");
        cancelRebindLabel = Translation.Get("settingsLabels", "cancelRebind");
    }

    private enum SettingHeader
    {
        Controls,
        Reload,
        GeneralControls,
        CameraControls,
        GeneralDragHandleControls,
        MaidDragHandleControls,
    }

    public override void Draw()
    {
        GUI.enabled = !inputRemapper.Listening;

        scrollPos = GUILayout.BeginScrollView(scrollPos);

        DrawHeader(SettingHeader.Controls);

        GUILayout.BeginVertical(leftMargin);

        DrawGeneralControls();
        DrawCameraControls();
        DrawGeneralDragHandleControls();
        DrawMaidDragHandleControls();

        GUILayout.EndVertical();

        DrawHeader(SettingHeader.Reload);

        GUILayout.BeginVertical(leftMargin);

        reloadTranslationButton.Draw();

        GUILayout.EndVertical();

        GUILayout.EndScrollView();

        GUI.enabled = true;

        void DrawGeneralControls()
        {
            DrawHeader(SettingHeader.GeneralControls);

            for (var shortcut = Shortcut.ActivatePlugin; shortcut <= Shortcut.Redo; shortcut++)
                DrawControl(shortcut);
        }

        void DrawCameraControls()
        {
            DrawHeader(SettingHeader.CameraControls);

            for (var shortcut = Shortcut.SaveCamera; shortcut <= Shortcut.ToggleCamera5; shortcut++)
                DrawControl(shortcut);

            for (var hotkey = Hotkey.FastCamera; hotkey <= Hotkey.SlowCamera; hotkey++)
                DrawControl(hotkey);
        }

        void DrawGeneralDragHandleControls()
        {
            DrawHeader(SettingHeader.GeneralDragHandleControls);

            for (var hotkey = Hotkey.Select; hotkey <= Hotkey.Scale; hotkey++)
                DrawControl(hotkey);
        }

        void DrawMaidDragHandleControls()
        {
            DrawHeader(SettingHeader.MaidDragHandleControls);

            for (var hotkey = Hotkey.DragFinger; hotkey <= Hotkey.MoveLocalY; hotkey++)
                DrawControl(hotkey);
        }

        void DrawHeader(SettingHeader settingHeader)
        {
            GUILayout.Label(settingHeaders[settingHeader], headerStyle);
            MpsGui.WhiteLine();
        }

        void DrawControl(Enum key)
        {
            var isShortcut = key.GetType() == typeof(Shortcut);

            DrawShortcutLabel(key, isShortcut);

            GUILayout.BeginHorizontal();

            if (CurrentControlIsListening(key, isShortcut))
            {
                GUILayout.Button(pushAnyKeyLabel, GUILayout.ExpandWidth(true));

                DrawCancelListeningButton();
            }
            else if (DrawControlButton(key, isShortcut))
            {
                ListenForNewKeyCombo(key, isShortcut);
            }

            if (GUILayout.Button("x", GUILayout.ExpandWidth(false)))
                ClearButtonCombo(key, isShortcut);

            GUILayout.EndHorizontal();

            bool CurrentControlIsListening(Enum key, bool isShortcut) =>
                inputRemapper.Listening && (isShortcut
                    ? listeningToShortcut && (Shortcut)key == currentShortcut
                    : !listeningToShortcut && (Hotkey)key == currentHotkey);

            bool DrawControlButton(Enum key, bool isShortcut)
            {
                var mapping = isShortcut
                    ? shortcutMapping[(Shortcut)key]
                    : hotkeyMapping[(Hotkey)key];

                return GUILayout.Button(mapping, GUILayout.ExpandWidth(true));
            }

            void ListenForNewKeyCombo(Enum key, bool isShortcut)
            {
                listeningToShortcut = isShortcut;

                if (isShortcut)
                {
                    inputRemapper.ListenForShortcut(OnControlRemapped);
                    currentShortcut = (Shortcut)key;
                }
                else
                {
                    inputRemapper.ListenForHotkey(OnControlRemapped);
                    currentHotkey = (Hotkey)key;
                }

                void OnControlRemapped(KeyboardInput input) =>
                    SetCombo(key, isShortcut, input);
            }

            void ClearButtonCombo(Enum key, bool isShortcut) =>
                SetCombo(key, isShortcut, isShortcut ? KeyboardShortcut.Empty : KeyboardHotkey.Empty);

            void SetCombo(Enum key, bool isShortcut, KeyboardInput input)
            {
                if (isShortcut)
                {
                    var shortcut = (KeyboardShortcut)input;

                    inputConfiguration[(Shortcut)key] = shortcut;
                    shortcutMapping[(Shortcut)key] = shortcut.ToString();
                }
                else
                {
                    var hotkey = (KeyboardHotkey)input;

                    inputConfiguration[(Hotkey)key] = hotkey;
                    hotkeyMapping[(Hotkey)key] = hotkey.ToString();
                }
            }

            void DrawCancelListeningButton()
            {
                GUI.enabled = true;

                if (GUILayout.Button(cancelRebindLabel))
                    inputRemapper.Cancel();

                GUI.enabled = false;
            }

            void DrawShortcutLabel(Enum key, bool isShortcut)
            {
                var keyName = isShortcut ? shortcutName[(Shortcut)key] : hotkeyName[(Hotkey)key];

                GUILayout.Label(keyName, labelStyle, GUILayout.ExpandWidth(false));
            }
        }
    }

    protected override void ReloadTranslation()
    {
        foreach (var shortcut in (Shortcut[])Enum.GetValues(typeof(Shortcut)))
            shortcutName[shortcut] = Translation.Get("controls", EnumToLower(shortcut));

        foreach (var hotkey in (Hotkey[])Enum.GetValues(typeof(Hotkey)))
            hotkeyName[hotkey] = Translation.Get("controls", EnumToLower(hotkey));

        foreach (var settingHeader in (SettingHeader[])Enum.GetValues(typeof(SettingHeader)))
            settingHeaders[settingHeader] = Translation.Get("settingsHeaders", EnumToLower(settingHeader));

        reloadTranslationButton.Label = Translation.Get("settingsLabels", "reloadTranslation");
        pushAnyKeyLabel = Translation.Get("settingsLabels", "pushAnyKey");
        cancelRebindLabel = Translation.Get("settingsLabels", "cancelRebind");
    }

    private static string EnumToLower(Enum enumValue)
    {
        var enumString = enumValue.ToString();

        return char.ToLower(enumString[0]) + enumString.Substring(1);
    }
}
