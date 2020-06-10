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
        private static new bool updating;
        private static readonly string[] tabNames = { "call", "pose", "face", "bg", "bg2" };
        static TabsPane()
        {
            Translation.ReloadTranslationEvent += (s, a) => ReloadTranslation();
            Tabs = new SelectionGrid(Translation.GetArray("tabs", tabNames), tabNames.Length);
            Tabs.ControlEvent += (s, a) => OnChangeTab();
        }

        protected static new void ReloadTranslation()
        {
            updating = true;
            Tabs.SetItems(Translation.GetArray("tabs", tabNames), Tabs.SelectedItem);
            updating = false;
        }

        private static void OnChangeTab()
        {
            if (updating) return;
            selectedTab = (Constants.Window)Tabs.SelectedItem;
            TabChange?.Invoke(null, EventArgs.Empty);
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
