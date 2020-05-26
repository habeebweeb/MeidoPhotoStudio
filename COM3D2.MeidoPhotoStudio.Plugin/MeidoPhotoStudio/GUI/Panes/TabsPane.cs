using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class TabsPane : BasePane
    {
        private static SelectionGrid Tabs;
        private static Constants.Window selectedTab;
        public static Constants.Window SelectedTab
        {
            get => selectedTab;
            set => Tabs.SelectedItem = (int)value;

        }
        public static event EventHandler TabChange;
        static TabsPane()
        {
            string[] tabs = { "Call", "Pose", "Face", "BG", "BG2" };
            Tabs = new SelectionGrid(tabs, tabs.Length);
            Tabs.ControlEvent += (s, a) => OnChangeTab();
        }

        private static void OnChangeTab()
        {
            selectedTab = (Constants.Window)Tabs.SelectedItem;
            EventHandler handler = TabChange;
            if (handler != null) handler(null, EventArgs.Empty);
        }

        public static void Draw()
        {
            GUIStyle tabStyle = new GUIStyle(GUI.skin.toggle);
            tabStyle.padding.right = -6;
            Tabs.Draw(tabStyle, GUILayout.ExpandWidth(false));
            MiscGUI.BlackLine();
        }

    }
}
