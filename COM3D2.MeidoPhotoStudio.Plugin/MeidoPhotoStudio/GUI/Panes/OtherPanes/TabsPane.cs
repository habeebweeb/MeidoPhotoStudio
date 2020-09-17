using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class TabsPane : BasePane
    {
        private static readonly string[] tabNames = { "call", "pose", "face", "bg", "bg2" };
        private readonly SelectionGrid Tabs;
        private Constants.Window selectedTab;
        public Constants.Window SelectedTab
        {
            get => selectedTab;
            set => Tabs.SelectedItemIndex = (int)value;
        }
        public event EventHandler TabChange;

        public TabsPane()
        {
            Translation.ReloadTranslationEvent += (s, a) => ReloadTranslation();
            Tabs = new SelectionGrid(Translation.GetArray("tabs", tabNames));
            Tabs.ControlEvent += (s, a) => OnChangeTab();
        }

        protected override void ReloadTranslation()
        {
            updating = true;
            Tabs.SetItems(Translation.GetArray("tabs", tabNames), Tabs.SelectedItemIndex);
            updating = false;
        }

        private void OnChangeTab()
        {
            if (updating) return;
            selectedTab = (Constants.Window)Tabs.SelectedItemIndex;
            TabChange?.Invoke(null, EventArgs.Empty);
        }

        public override void Draw()
        {
            Tabs.Draw(GUILayout.ExpandWidth(false));
            MpsGui.BlackLine();
        }
    }
}
