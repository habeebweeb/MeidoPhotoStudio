using System;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using Window = Constants.Window;
    public class WindowManager
    {
        private Dictionary<Window, BaseWindow> Windows;
        private static Window currentWindow = Window.Call;
        private static Window CurrentWindow
        {
            get => currentWindow;
            set
            {
                if (value > Window.BG2) currentWindow = Window.BG2;
                else if (value < Window.Call) currentWindow = Window.Call;
                else currentWindow = value;
            }
        }
        private Rect mainWindowRect;
        private Rect messageWindowRect;
        private MeidoManager meidoManager;
        private bool initializeWindows = false;
        public bool Visible { get; set; }
        public WindowManager(MeidoManager meidoManager, EnvironmentManager environmentManager, MessageWindowManager messageWindowManager)
        {
            TabsPane.TabChange += ChangeTab;
            this.meidoManager = meidoManager;
            this.meidoManager.SelectMeido += MeidoSelect;
            this.meidoManager.CalledMeidos += (s, a) => Visible = true;

            mainWindowRect.y = Screen.height * 0.08f;
            mainWindowRect.x = Screen.width;
            Windows = new Dictionary<Window, BaseWindow>()
            {
                [Window.Call] = new MaidCallWindow(meidoManager),
                [Window.Pose] = new MaidPoseWindow(meidoManager),
                [Window.Face] = new MaidFaceWindow(meidoManager),
                [Window.BG] = new BackgroundWindow(environmentManager),
                [Window.BG2] = new Background2Window(environmentManager),
                [Window.Message] = new MessageWindow(messageWindowManager)
            };
            Windows[Window.Message].Visible = false;
        }

        ~WindowManager()
        {
            TabsPane.TabChange -= ChangeTab;
        }

        private void MeidoSelect(object sender, MeidoChangeEventArgs args)
        {
            if (args.fromMeido)
                TabsPane.SelectedTab = args.isBody ? Window.Pose : Window.Face;
        }

        private void ChangeTab(object sender, EventArgs args)
        {
            CurrentWindow = TabsPane.SelectedTab;
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                (Windows[Window.Message] as MessageWindow).SetVisibility();
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                Visible = !Visible;
            }

            if (this.meidoManager.IsFade) Visible = false;

            HandleZoom();
        }

        private void HandleZoom()
        {
            bool mainWindowVisible = Windows[currentWindow].Visible;
            bool dropdownVisible = DropdownHelper.Visible;
            bool messageWindowVisible = Windows[currentWindow].Visible;
            if (mainWindowVisible || dropdownVisible || messageWindowVisible)
            {
                if (Input.mouseScrollDelta.y != 0f)
                {
                    Vector2 mousePos = Event.current.mousePosition;
                    if (mainWindowVisible && mainWindowRect.Contains(mousePos)
                        || dropdownVisible && DropdownHelper.dropdownWindow.Contains(mousePos)
                        || messageWindowVisible && messageWindowRect.Contains(mousePos)
                    )
                    {
                        GameMain.Instance.MainCamera.SetControl(false);
                        Input.ResetInputAxes();
                    }

                }
            }
        }

        public void OnGUI()
        {
            if (!Visible) return;

            GUIStyle windowStyle = new GUIStyle(GUI.skin.box);
            GameMain.Instance.MainCamera.SetControl(true);

            if (Windows[currentWindow].Visible)
            {
                mainWindowRect.width = 230;
                mainWindowRect.height = Screen.height * 0.8f;

                mainWindowRect.x = Mathf.Clamp(mainWindowRect.x, 0, Screen.width - mainWindowRect.width);
                mainWindowRect.y = Mathf.Clamp(mainWindowRect.y, -mainWindowRect.height + 30, Screen.height - 50);

                mainWindowRect = GUI.Window(Constants.mainWindowID, mainWindowRect, Windows[CurrentWindow].OnGUI, "", windowStyle);
            }
            if (Windows[Window.Message].Visible)
            {
                messageWindowRect.width = Mathf.Clamp(Screen.width * 0.4f, 440, Mathf.Infinity);
                messageWindowRect.height = Mathf.Clamp(Screen.height * 0.15f, 150, Mathf.Infinity);

                messageWindowRect.x = Mathf.Clamp(messageWindowRect.x, -messageWindowRect.width + Utility.GetPix(20), Screen.width - Utility.GetPix(20));
                messageWindowRect.y = Mathf.Clamp(messageWindowRect.y, -messageWindowRect.height + Utility.GetPix(20), Screen.height - Utility.GetPix(20));

                if (!initializeWindows)
                {
                    messageWindowRect.x = Screen.width / 2 - messageWindowRect.width / 2;
                    messageWindowRect.y = Screen.height - messageWindowRect.height;
                    initializeWindows = true;
                }

                messageWindowRect = GUI.Window(Constants.messageWindowID, messageWindowRect, Windows[Window.Message].OnGUI, "", windowStyle);
            }

            if (DropdownHelper.Visible) DropdownHelper.HandleDropdown();
        }
    }
}
