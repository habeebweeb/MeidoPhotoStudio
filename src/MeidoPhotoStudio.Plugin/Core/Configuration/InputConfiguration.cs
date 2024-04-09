using BepInEx.Configuration;
using MeidoPhotoStudio.Plugin.Input;

using KeyboardShortcut = MeidoPhotoStudio.Plugin.Input.KeyboardShortcut;

namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class InputConfiguration
{
    public const string Section = "Input";

    private readonly ConfigFile configFile;
    private readonly Dictionary<Shortcut, ConfigDefinition> shortcutDefinitions = new();
    private readonly Dictionary<Hotkey, ConfigDefinition> hotkeyDefinitions = new();
    private readonly Dictionary<Hotkey, KeyboardHotkey> keyboardHotkeys = new(EnumEqualityComparer<Hotkey>.Instance);
    private readonly Dictionary<Shortcut, KeyboardShortcut> keyboardShortcuts = new(EnumEqualityComparer<Shortcut>.Instance);

    public InputConfiguration(ConfigFile configFile)
    {
        this.configFile = configFile ?? throw new ArgumentNullException(nameof(configFile));

        // Plugin
        BindShortcut(Shortcut.ActivatePlugin, "Toggle Plugin Active", new KeyboardShortcut(KeyCode.F6));

        // Screenshot
        BindShortcut(Shortcut.Screenshot, "Take Screenshot", new KeyboardShortcut(KeyCode.S));

        // UI Windows
        BindShortcut(Shortcut.ToggleMessageWindow, "Toggle Message Window Visible", new KeyboardShortcut(KeyCode.M));
        BindShortcut(Shortcut.ToggleMainWindow, "Toggle Main Window Visible", new KeyboardShortcut(KeyCode.Tab));
        BindShortcut(Shortcut.ToggleSceneWindow, "Toggle Scene Window Visible", new KeyboardShortcut(KeyCode.F8));

        // Meido Manager
        BindShortcut(Shortcut.CycleMaidDressing, "Cycle All Maid Dressing", new KeyboardShortcut(KeyCode.H));

        // Scene management
        BindShortcut(Shortcut.QuickSaveScene, "Quick Save Scene", new KeyboardShortcut(KeyCode.S, KeyCode.LeftControl));
        BindShortcut(Shortcut.QuickLoadScene, "Quick Load Scene", new KeyboardShortcut(KeyCode.A, KeyCode.LeftControl));

        // Cameras
        BindShortcut(Shortcut.SaveCamera, "Quick Save Camera", new KeyboardShortcut(KeyCode.S, KeyCode.Q));
        BindShortcut(Shortcut.LoadCamera, "Quick Load Camera", new KeyboardShortcut(KeyCode.A, KeyCode.Q));
        BindShortcut(Shortcut.ResetCamera, "Reset Camera", new KeyboardShortcut(KeyCode.R, KeyCode.Q));
        BindShortcut(Shortcut.ToggleCamera1, "Switch to Camera Slot 1", new KeyboardShortcut(KeyCode.Alpha1, KeyCode.Q));
        BindShortcut(Shortcut.ToggleCamera2, "Switch to Camera Slot 2", new KeyboardShortcut(KeyCode.Alpha2, KeyCode.Q));
        BindShortcut(Shortcut.ToggleCamera3, "Switch to Camera Slot 3", new KeyboardShortcut(KeyCode.Alpha3, KeyCode.Q));
        BindShortcut(Shortcut.ToggleCamera4, "Switch to Camera Slot 4", new KeyboardShortcut(KeyCode.Alpha4, KeyCode.Q));
        BindShortcut(Shortcut.ToggleCamera5, "Switch to Camera Slot 5", new KeyboardShortcut(KeyCode.Alpha5, KeyCode.Q));
        BindHotkey(Hotkey.FastCamera, "Fast Camera Movement", new KeyboardHotkey(KeyCode.LeftShift));
        BindHotkey(Hotkey.SlowCamera, "Slow Camera Movement", new KeyboardHotkey(KeyCode.LeftControl));

        // General drag handles
        BindHotkey(Hotkey.Select, "Select Object", new KeyboardHotkey(KeyCode.A));
        BindHotkey(Hotkey.Delete, "Delete Object", new KeyboardHotkey(KeyCode.D));
        BindHotkey(Hotkey.MoveWorldXZ, "Move Object World XZ", new KeyboardHotkey(KeyCode.Z));
        BindHotkey(Hotkey.MoveWorldY, "Move Object World Y", new KeyboardHotkey(KeyCode.Z, KeyCode.LeftControl));
        BindHotkey(Hotkey.RotateWorldY, "Rotate Object World Y", new KeyboardHotkey(KeyCode.Z, KeyCode.LeftShift));
        BindHotkey(Hotkey.RotateLocalY, "Rotate Object Local Y", new KeyboardHotkey(KeyCode.X, KeyCode.LeftShift));
        BindHotkey(Hotkey.RotateLocalXZ, "Rotate Object Local XZ", new KeyboardHotkey(KeyCode.X));
        BindHotkey(Hotkey.Scale, "Scale Object", new KeyboardHotkey(KeyCode.C));

        // Maid drag handles
        BindHotkey(Hotkey.DragFinger, "Drag All Fingers", new KeyboardHotkey(KeyCode.Space));
        BindHotkey(Hotkey.DragFinger1, "Drag First Finger", new KeyboardHotkey(KeyCode.Space, KeyCode.Alpha1));
        BindHotkey(Hotkey.DragFinger2, "Drag Second Finger", new KeyboardHotkey(KeyCode.Space, KeyCode.Alpha2));
        BindHotkey(Hotkey.DragFinger3, "Drag Third Finger", new KeyboardHotkey(KeyCode.Space, KeyCode.Alpha3));
        BindHotkey(Hotkey.DragFinger4, "Drag Fourth Finger", new KeyboardHotkey(KeyCode.Space, KeyCode.Alpha4));
        BindHotkey(Hotkey.DragFinger5, "Drag Fifth Finger", new KeyboardHotkey(KeyCode.Space, KeyCode.Alpha5));
        BindHotkey(Hotkey.RotateFinger, "Rotate Fingers", new KeyboardHotkey(KeyCode.Space, KeyCode.LeftShift));
        BindHotkey(Hotkey.RotateFinger1, "Rotate First Finger", new KeyboardHotkey(KeyCode.Space, KeyCode.LeftShift, KeyCode.Alpha1));
        BindHotkey(Hotkey.RotateFinger2, "Rotate Second Finger", new KeyboardHotkey(KeyCode.Space, KeyCode.LeftShift, KeyCode.Alpha2));
        BindHotkey(Hotkey.RotateFinger3, "Rotate Third Finger", new KeyboardHotkey(KeyCode.Space, KeyCode.LeftShift, KeyCode.Alpha3));
        BindHotkey(Hotkey.RotateFinger4, "Rotate Fourth Finger", new KeyboardHotkey(KeyCode.Space, KeyCode.LeftShift, KeyCode.Alpha4));
        BindHotkey(Hotkey.RotateFinger5, "Rotate Fifth Finger", new KeyboardHotkey(KeyCode.Space, KeyCode.LeftShift, KeyCode.Alpha5));
        BindHotkey(Hotkey.RotateEyesChest, "Rotate Eyes or Chest", new KeyboardHotkey(KeyCode.LeftControl, KeyCode.LeftAlt));
        BindHotkey(Hotkey.RotateEyesChestAlternate, "Alternate Rotate Eyes or Chest", new KeyboardHotkey(KeyCode.LeftControl, KeyCode.LeftAlt, KeyCode.LeftShift));
        BindHotkey(Hotkey.RotateBody, "Body Rotation", new KeyboardHotkey(KeyCode.LeftAlt));
        BindHotkey(Hotkey.RotateBodyAlternate, "Body Rotation Alternate", new KeyboardHotkey(KeyCode.LeftAlt, KeyCode.LeftShift));
        BindHotkey(Hotkey.DragLowerLimb, "Drag Lower Limb", new KeyboardHotkey(KeyCode.LeftControl));
        BindHotkey(Hotkey.DragLowerBone, "Drag Lower Bone", new KeyboardHotkey(KeyCode.LeftAlt));
        BindHotkey(Hotkey.DragMiddleBone, "Drag Middle Bone", new KeyboardHotkey(KeyCode.LeftControl, KeyCode.LeftAlt));
        BindHotkey(Hotkey.DragUpperBone, "Drag Upper Bone", new KeyboardHotkey(KeyCode.LeftAlt, KeyCode.LeftShift));
        BindHotkey(Hotkey.SpineBoneRotation, "Spine Bone Rotation", new KeyboardHotkey(KeyCode.LeftShift));
        BindHotkey(Hotkey.SpineBoneGizmoRotation, "Spine Bone Gizmo Rotation", new KeyboardHotkey(KeyCode.LeftControl));
        BindHotkey(Hotkey.HipBoneRotation, "Hip Bone Rotation", new KeyboardHotkey(KeyCode.LeftShift));
        BindHotkey(Hotkey.MoveLocalY, "Move Local Y", new KeyboardHotkey(KeyCode.LeftControl));

        // Gravity drag handles
        BindHotkey(Hotkey.MoveGravityWorldY, "Move Gravity World Y", new KeyboardHotkey(KeyCode.LeftControl));

        KeyPool = BuildKeyPool();

        void BindShortcut(Shortcut shortcut, string key, KeyboardShortcut keyboardShortcut)
        {
            var definition = new ConfigDefinition(Section, key);

            shortcutDefinitions[shortcut] = definition;

            var configEntry = configFile.Bind(definition, keyboardShortcut);

            keyboardShortcuts[shortcut] = configEntry.Value;

            configEntry.SettingChanged += OnSettingChanged;

            void OnSettingChanged(object sender, EventArgs args)
            {
                KeyPool = BuildKeyPool();

                keyboardShortcuts[shortcut] = configEntry.Value;
            }
        }

        void BindHotkey(Hotkey hotkey, string key, KeyboardHotkey keyboardHotkey)
        {
            var definition = new ConfigDefinition(Section, key);

            hotkeyDefinitions[hotkey] = definition;

            var configEntry = configFile.Bind(definition, keyboardHotkey);

            keyboardHotkeys[hotkey] = configEntry.Value;

            configEntry.SettingChanged += OnSettingChanged;

            void OnSettingChanged(object sender, EventArgs args)
            {
                KeyPool = BuildKeyPool();

                keyboardHotkeys[hotkey] = configEntry.Value;
            }
        }

        IEnumerable<KeyCode> BuildKeyPool() =>
            keyboardHotkeys.Values.Select(hotkey => hotkey.KeyList)
                .Concat(keyboardShortcuts.Values.Select(shortcut => shortcut.KeyList))
                .SelectMany(key => key)
                .OrderBy(key => key)
                .Distinct()
                .ToArray();
    }

    public IEnumerable<KeyCode> KeyPool { get; private set; }

    public KeyboardHotkey this[Hotkey hotkey]
    {
        get => keyboardHotkeys.TryGetValue(hotkey, out var keyboardHotkey)
            ? keyboardHotkey
            : throw new ArgumentException(nameof(keyboardHotkey));
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (!hotkeyDefinitions.TryGetValue(hotkey, out var configDefinition))
                throw new ArgumentException(nameof(hotkey));

            var configEntryBase = configFile[configDefinition];

            if (configEntryBase is not ConfigEntry<KeyboardHotkey> configEntry)
                throw new InvalidOperationException($"{nameof(KeyboardHotkey)} cannot be assigned to {hotkey}");

            configEntry.Value = value;
        }
    }

    public KeyboardShortcut this[Shortcut shortcut]
    {
        get => keyboardShortcuts.TryGetValue(shortcut, out var keyboardShortcut)
            ? keyboardShortcut
            : throw new ArgumentException(nameof(keyboardShortcut));
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (!shortcutDefinitions.TryGetValue(shortcut, out var configDefinition))
                throw new ArgumentException(nameof(shortcut));

            var configEntryBase = configFile[configDefinition];

            if (configEntryBase is not ConfigEntry<KeyboardShortcut> configEntry)
                throw new InvalidOperationException($"{nameof(KeyboardHotkey)} cannot be assigned to {shortcut}");

            configEntry.Value = value;
        }
    }
}
