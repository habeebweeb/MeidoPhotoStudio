using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BepInEx.Configuration;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal static class InputManager
    {
        private static InputListener inputListener;
        private static readonly Dictionary<MpsKey, KeyCode> ActionKeys = new Dictionary<MpsKey, KeyCode>();
        private static readonly Dictionary<MpsKey, ConfigEntry<KeyCode>> ConfigEntries
            = new Dictionary<MpsKey, ConfigEntry<KeyCode>>();
        public static KeyCode CurrentKeyCode { get; private set; }
        public static bool Listening { get; private set; }
        public static event EventHandler KeyChange;
        public static bool Control => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        public static bool Alt => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        public static bool Shift => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        public static readonly AcceptableValueBase controlRange;
        public const KeyCode upperKeyCode = KeyCode.F15;
        public const string configHeader = "Controls";

        static InputManager() => controlRange = new AcceptableValueRange<KeyCode>(default, upperKeyCode);

        public static void Register(MpsKey action, KeyCode key, string description)
        {
            key = Clamp(key, default, upperKeyCode);
            if (ConfigEntries.ContainsKey(action)) Rebind(action, key);
            else
            {
                ConfigDescription configDescription = new ConfigDescription(description, controlRange);
                ConfigEntries[action] = Configuration.Config.Bind(
                    configHeader, action.ToString(), key, configDescription
                );
                key = ConfigEntries[action].Value;
                ActionKeys[action] = key;
            }
        }

        public static void Rebind(MpsKey action, KeyCode key)
        {
            key = Clamp(key, default, upperKeyCode);
            if (ConfigEntries.ContainsKey(action)) ConfigEntries[action].Value = key;
            ActionKeys[action] = key;
        }

        public static KeyCode Clamp(KeyCode value, KeyCode min, KeyCode max)
        {
            return value < min ? min : value > max ? max : value;
        }

        public static KeyCode GetActionKey(MpsKey action)
        {
            ActionKeys.TryGetValue(action, out KeyCode keyCode);
            return keyCode;
        }

        public static void StartListening()
        {
            if (inputListener == null) inputListener = new GameObject().AddComponent<InputListener>();
            else if (inputListener.gameObject.activeSelf) StopListening();

            inputListener.gameObject.SetActive(true);
            inputListener.KeyChange += OnKeyChange;
            CurrentKeyCode = KeyCode.None;
            Listening = true;
        }

        public static void StopListening()
        {
            if (!inputListener || !inputListener.gameObject.activeSelf) return;
            inputListener.gameObject.SetActive(false);
            inputListener.KeyChange -= OnKeyChange;
            CurrentKeyCode = KeyCode.None;
            Listening = false;
            Input.ResetInputAxes();
        }

        public static bool GetKey(MpsKey action)
        {
            return !Listening && ActionKeys.ContainsKey(action) && Input.GetKey(ActionKeys[action]);
        }

        public static bool GetKeyDown(MpsKey action)
        {
            return !Listening && ActionKeys.ContainsKey(action) && Input.GetKeyDown(ActionKeys[action]);
        }

        public static void Deactivate()
        {
            StopListening();
            GameObject.Destroy(inputListener?.gameObject);
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
            private static readonly KeyCode[] keyCodes;
            public event EventHandler<KeyChangeEventArgs> KeyChange;

            static InputListener()
            {
                keyCodes = Enum.GetValues(typeof(KeyCode))
                    .Cast<KeyCode>()
                    .Where(keyCode => keyCode <= upperKeyCode)
                    .ToArray();
            }

            private void Awake() => DontDestroyOnLoad(this);

            private void Update()
            {
                if (Input.anyKeyDown)
                {
                    foreach (KeyCode key in keyCodes)
                    {
                        if (Input.GetKeyDown(key))
                        {
                            KeyChange?.Invoke(this, new KeyChangeEventArgs(key));
                            break;
                        }
                    }
                }
            }
        }

        private class KeyChangeEventArgs : EventArgs
        {
            public KeyCode Key { get; }
            public KeyChangeEventArgs(KeyCode key) => Key = key;
        }
    }

    internal enum MpsKey
    {
        // MeidoPhotoStudio
        Activate, Screenshot, ToggleUI, ToggleMessage,
        // MeidoManager
        MeidoUndressing,
        // Camera
        CameraLayer, CameraReset, CameraSave, CameraLoad,
        // Dragpoint
        DragSelect, DragDelete, DragMove, DragRotate, DragScale, DragFinger,
        // Scene management
        SaveScene, LoadScene, OpenSceneManager
    }
}
