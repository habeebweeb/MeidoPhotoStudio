using System;
using System.Collections.Generic;
using System.Linq;

using BepInEx.Configuration;
using UnityEngine;

using UInput = UnityEngine.Input;

namespace MeidoPhotoStudio.Plugin;

public enum MpsKey
{
    // MeidoPhotoStudio
    Activate,
    Screenshot,
    ToggleUI,
    ToggleMessage,

    // MeidoManager
    MeidoUndressing,

    // Camera
    CameraLayer,
    CameraReset,
    CameraSave,
    CameraLoad,

    // Dragpoint
    DragSelect,
    DragDelete,
    DragMove,
    DragRotate,
    DragScale,
    DragFinger,

    // Scene management
    SaveScene,
    LoadScene,
    OpenSceneManager,
}

public static class InputManager
{
    public const KeyCode UpperKeyCode = KeyCode.F15;
    public const string ConfigHeader = "Controls";

    public static readonly AcceptableValueBase ControlRange;

    private static readonly Dictionary<MpsKey, KeyCode> ActionKeys = new();
    private static readonly Dictionary<MpsKey, ConfigEntry<KeyCode>> ConfigEntries = new();

    private static InputListener inputListener;

    static InputManager() =>
        ControlRange = new AcceptableValueRange<KeyCode>(default, UpperKeyCode);

    public static event EventHandler KeyChange;

    public static KeyCode CurrentKeyCode { get; private set; }

    public static bool Listening { get; private set; }

    public static bool Control =>
        UInput.GetKey(KeyCode.LeftControl) || UInput.GetKey(KeyCode.RightControl);

    public static bool Alt =>
        UInput.GetKey(KeyCode.LeftAlt) || UInput.GetKey(KeyCode.RightAlt);

    public static bool Shift =>
        UInput.GetKey(KeyCode.LeftShift) || UInput.GetKey(KeyCode.RightShift);

    public static void Register(MpsKey action, KeyCode key, string description)
    {
        key = Clamp(key, default, UpperKeyCode);

        if (ConfigEntries.ContainsKey(action))
        {
            Rebind(action, key);
        }
        else
        {
            var configDescription = new ConfigDescription(description, ControlRange);

            ConfigEntries[action] =
                Configuration.Config.Bind(ConfigHeader, action.ToString(), key, configDescription);

            key = ConfigEntries[action].Value;
            ActionKeys[action] = key;
        }
    }

    public static void Rebind(MpsKey action, KeyCode key)
    {
        key = Clamp(key, default, UpperKeyCode);

        if (ConfigEntries.ContainsKey(action))
            ConfigEntries[action].Value = key;

        ActionKeys[action] = key;
    }

    public static KeyCode Clamp(KeyCode value, KeyCode min, KeyCode max) =>
        value < min ? min : value > max ? max : value;

    public static KeyCode GetActionKey(MpsKey action) =>
        ActionKeys.TryGetValue(action, out var keyCode) ? keyCode : default;

    public static void StartListening()
    {
        if (!inputListener)
            inputListener = new GameObject().AddComponent<InputListener>();
        else if (inputListener.gameObject.activeSelf)
            StopListening();

        inputListener.gameObject.SetActive(true);
        inputListener.KeyChange += OnKeyChange;
        CurrentKeyCode = KeyCode.None;
        Listening = true;
    }

    public static void StopListening()
    {
        if (!inputListener || !inputListener.gameObject.activeSelf)
            return;

        inputListener.gameObject.SetActive(false);
        inputListener.KeyChange -= OnKeyChange;
        CurrentKeyCode = KeyCode.None;
        Listening = false;
        UInput.ResetInputAxes();
    }

    public static bool GetKey(MpsKey action) =>
        !Listening && ActionKeys.ContainsKey(action) && UInput.GetKey(ActionKeys[action]);

    public static bool GetKeyDown(MpsKey action) =>
        !Listening && ActionKeys.ContainsKey(action) && UInput.GetKeyDown(ActionKeys[action]);

    public static void Deactivate()
    {
        StopListening();

        // TODO: Null propagation does not work with UnityEngine.Object
        UnityEngine.Object.Destroy(inputListener?.gameObject);
        inputListener = null;
    }

    private static void OnKeyChange(object sender, KeyChangeEventArgs args)
    {
        CurrentKeyCode = args.Key;
        KeyChange?.Invoke(null, EventArgs.Empty);
        StopListening();
    }

    /* Listener taken from https://forum.unity.com/threads/find-out-which-key-was-pressed.385250/ */
    private class InputListener : MonoBehaviour
    {
        private static readonly KeyCode[] KeyCodes;

        static InputListener() =>
            KeyCodes = Enum.GetValues(typeof(KeyCode))
            .Cast<KeyCode>()
            .Where(keyCode => keyCode <= UpperKeyCode)
            .ToArray();

        public event EventHandler<KeyChangeEventArgs> KeyChange;

        private void Awake() =>
            DontDestroyOnLoad(this);

        private void Update()
        {
            if (!UInput.anyKeyDown)
                return;

            foreach (var key in KeyCodes)
            {
                if (!UInput.GetKeyDown(key))
                    continue;

                KeyChange?.Invoke(this, new(key));

                break;
            }
        }
    }

    private class KeyChangeEventArgs : EventArgs
    {
        public KeyChangeEventArgs(KeyCode key) =>
            Key = key;

        public KeyCode Key { get; }
    }
}
