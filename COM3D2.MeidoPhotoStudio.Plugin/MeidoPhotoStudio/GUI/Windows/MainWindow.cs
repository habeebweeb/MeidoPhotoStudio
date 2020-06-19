using System;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class MainWindow : BaseWindow
    {
        private MeidoManager meidoManager;
        private Dictionary<Constants.Window, BaseWindowPane> windowPanes;
        private TabsPane tabsPane;
        private Button ReloadTranslationButton;
        private BaseWindowPane currentWindowPane;
        public override Rect WindowRect
        {
            get
            {
                windowRect.width = 230f;
                windowRect.height = Screen.height * 0.9f;
                windowRect.x = Mathf.Clamp(windowRect.x, 0, Screen.width - windowRect.width);
                windowRect.y = Mathf.Clamp(windowRect.y, -windowRect.height + 30, Screen.height - 50);
                return windowRect;
            }
            set => windowRect = value;
        }
        private Constants.Window selectedWindow = Constants.Window.Call;

        public BaseWindowPane this[Constants.Window id]
        {
            get => windowPanes[id];
            set => AddWindow(id, value);
        }

        public MainWindow(MeidoManager meidoManager) : base()
        {
            this.meidoManager = meidoManager;
            this.meidoManager.UpdateMeido += UpdateMeido;

            windowPanes = new Dictionary<Constants.Window, BaseWindowPane>();
            windowRect = new Rect(Screen.width, Screen.height * 0.08f, 230f, Screen.height * 0.9f);

            tabsPane = new TabsPane();
            tabsPane.TabChange += (s, a) => ChangeTab();

            ReloadTranslationButton = new Button("Reload Translation");
            ReloadTranslationButton.ControlEvent += (s, a) =>
            {
                Translation.ReloadTranslation();
            };
        }

        public override void Activate()
        {
            this.updating = true;
            tabsPane.SelectedTab = Constants.Window.Call;
            this.updating = false;
            this.Visible = true;
        }

        public void AddWindow(Constants.Window id, BaseWindowPane window)
        {
            if (windowPanes.ContainsKey(id))
            {
                Panes.Remove(windowPanes[id]);
            }
            windowPanes[id] = window;
            Panes.Add(windowPanes[id]);
        }

        private void ChangeTab()
        {
            this.selectedWindow = (Constants.Window)tabsPane.SelectedTab;
            SetCurrentWindow();
        }

        private void SetCurrentWindow()
        {
            if (currentWindowPane != null) currentWindowPane.ActiveWindow = false;
            currentWindowPane = windowPanes[selectedWindow];
            currentWindowPane.ActiveWindow = true;
            currentWindowPane.UpdatePanes();
        }

        public override void Update()
        {
            base.Update();
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                this.Visible = !this.Visible;
            }
        }

        public override void Draw()
        {
            tabsPane.Draw();

            currentWindowPane?.Draw();

            GUI.enabled = true;

            GUILayout.FlexibleSpace();

            ReloadTranslationButton.Draw();

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 10;
            labelStyle.alignment = TextAnchor.LowerLeft;

            GUILayout.Label("MeidoPhotoStudio 1.0.0", labelStyle);
            GUI.DragWindow();
        }


        private void UpdateMeido(object sender, MeidoUpdateEventArgs args)
        {
            if (args.FromMeido)
            {
                Constants.Window newWindow = args.IsBody ? Constants.Window.Pose : Constants.Window.Face;
                if (this.selectedWindow == newWindow) currentWindowPane.UpdatePanes();
                else tabsPane.SelectedTab = newWindow;
            }
            else currentWindowPane.UpdatePanes();
        }
    }
}
