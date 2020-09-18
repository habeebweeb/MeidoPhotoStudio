using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class MainWindow : BaseWindow
    {
        private readonly MeidoManager meidoManager;
        private readonly Dictionary<Constants.Window, BaseMainWindowPane> windowPanes;
        private readonly PropManager propManager;
        private readonly LightManager lightManager;
        private readonly TabsPane tabsPane;
        private readonly Button settingsButton;
        private BaseMainWindowPane currentWindowPane;
        public override Rect WindowRect
        {
            set
            {
                value.width = 240f;
                value.height = Screen.height * 0.9f;
                value.x = Mathf.Clamp(value.x, 0, Screen.width - value.width);
                value.y = Mathf.Clamp(value.y, -value.height + 30, Screen.height - 50);
                windowRect = value;
            }
        }
        private Constants.Window selectedWindow;

        public BaseMainWindowPane this[Constants.Window id]
        {
            get => windowPanes[id];
            set => AddWindow(id, value);
        }

        // TODO: Find a better way of doing this
        public MainWindow(MeidoManager meidoManager, PropManager propManager, LightManager lightManager)
        {
            this.meidoManager = meidoManager;
            this.meidoManager.UpdateMeido += UpdateMeido;

            this.propManager = propManager;
            this.propManager.DoguSelectChange += (s, a) => ChangeWindow(Constants.Window.BG2);

            this.lightManager = lightManager;
            this.lightManager.Select += (s, a) => ChangeWindow(Constants.Window.BG);

            windowPanes = new Dictionary<Constants.Window, BaseMainWindowPane>();
            WindowRect = new Rect(Screen.width, Screen.height * 0.08f, 240f, Screen.height * 0.9f);

            tabsPane = new TabsPane();
            tabsPane.TabChange += (s, a) => ChangeTab();

            settingsButton = new Button("Settings");
            settingsButton.ControlEvent += (s, a) =>
            {
                if (selectedWindow == Constants.Window.Settings) ChangeTab();
                else
                {
                    settingsButton.Label = "Close";
                    SetCurrentWindow(Constants.Window.Settings);
                }
            };
        }

        public override void Activate()
        {
            updating = true;
            tabsPane.SelectedTab = Constants.Window.Call;
            updating = false;
            Visible = true;
        }

        public void AddWindow(Constants.Window id, BaseMainWindowPane window)
        {
            if (windowPanes.ContainsKey(id))
            {
                Panes.Remove(windowPanes[id]);
            }
            windowPanes[id] = window;
            windowPanes[id].SetTabsPane(tabsPane);
            windowPanes[id].SetParent(this);
            Panes.Add(windowPanes[id]);
        }

        private void ChangeTab()
        {
            settingsButton.Label = "Settings";
            SetCurrentWindow(tabsPane.SelectedTab);
        }

        private void SetCurrentWindow(Constants.Window window)
        {
            if (currentWindowPane != null) currentWindowPane.ActiveWindow = false;
            selectedWindow = window;
            currentWindowPane = windowPanes[selectedWindow];
            currentWindowPane.ActiveWindow = true;
            currentWindowPane.UpdatePanes();
        }

        public override void Update()
        {
            base.Update();
            if (InputManager.GetKeyDown(MpsKey.ToggleUI)) Visible = !Visible;
        }

        public override void Draw()
        {
            currentWindowPane?.Draw();

            GUI.enabled = true;

            GUILayout.FlexibleSpace();

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                alignment = TextAnchor.LowerLeft
            };

            GUILayout.BeginHorizontal();
            GUILayout.Label(MeidoPhotoStudio.pluginString, labelStyle);
            GUILayout.FlexibleSpace();
            settingsButton.Draw(GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }

        private void UpdateMeido(object sender, MeidoUpdateEventArgs args)
        {
            if (args.FromMeido)
            {
                Constants.Window newWindow = args.IsBody ? Constants.Window.Pose : Constants.Window.Face;
                ChangeWindow(newWindow);
            }
            else currentWindowPane.UpdatePanes();
        }

        private void ChangeWindow(Constants.Window window)
        {
            if (selectedWindow == window) currentWindowPane.UpdatePanes();
            else tabsPane.SelectedTab = window;
        }
    }
}
