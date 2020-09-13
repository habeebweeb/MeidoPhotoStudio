using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class InputManager
    {
        private static InputListener inputListener;
        public static KeyCode CurrentKeyCode { get; private set; }
        public static bool Listening { get; private set; }
        public static event EventHandler KeyChange;
        public static bool Control => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        public static bool Alt => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        public static bool Shift => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        private static readonly Dictionary<MpsKey, KeyCode> Actions = new Dictionary<MpsKey, KeyCode>();

        public static void Register(MpsKey action, KeyCode key) => Actions[action] = key;

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
            if (inputListener == null || !inputListener.gameObject.activeSelf) return;
            inputListener.gameObject.SetActive(false);
            inputListener.KeyChange -= OnKeyChange;
            CurrentKeyCode = KeyCode.None;
        }

        public static bool GetKey(MpsKey action)
        {
            return (Listening || !Actions.ContainsKey(action)) ? false : Input.GetKey(Actions[action]);
        }

        public static bool GetKeyDown(MpsKey action)
        {
            return (Listening || !Actions.ContainsKey(action)) ? false : Input.GetKeyDown(Actions[action]);
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
                    .Where(keyCode => keyCode < KeyCode.Numlock)
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
            public KeyChangeEventArgs(KeyCode key) => this.Key = key;
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
        DragSelect, DragDelete, DragMove, DragRotate, DragScale,
        DragFinger,
        // Scene management
        SaveScene, LoadScene, OpenSceneManager
    }
}
