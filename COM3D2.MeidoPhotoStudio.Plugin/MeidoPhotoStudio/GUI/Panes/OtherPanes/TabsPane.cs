using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class TabsPane : BasePane
    {
        private SelectionGrid Tabs;
        private Constants.Window selectedTab;
        public Constants.Window SelectedTab
        {
            get => selectedTab;
            set => Tabs.SelectedItem = (int)value;

        }
        public event EventHandler TabChange;
        private new bool updating;
        private readonly string[] tabNames = { "call", "pose", "face", "bg", "bg2" };

        public TabsPane()
        {
            Translation.ReloadTranslationEvent += (s, a) => ReloadTranslation();
            Tabs = new SelectionGrid(Translation.GetArray("tabs", tabNames));
            Tabs.ControlEvent += (s, a) => OnChangeTab();
        }

        protected override void ReloadTranslation()
        {
            updating = true;
            Tabs.SetItems(Translation.GetArray("tabs", tabNames), Tabs.SelectedItem);
            updating = false;
        }

        private void OnChangeTab()
        {
            if (updating) return;
            selectedTab = (Constants.Window)Tabs.SelectedItem;
            TabChange?.Invoke(null, EventArgs.Empty);
        }

        public override void Draw()
        {
            Tabs.Draw(GUILayout.ExpandWidth(false));
            MiscGUI.BlackLine();
        }
    }
}
