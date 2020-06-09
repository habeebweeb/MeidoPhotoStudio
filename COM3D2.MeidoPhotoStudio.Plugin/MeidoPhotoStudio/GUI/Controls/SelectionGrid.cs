using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class SelectionGrid : BaseControl
    {
        public string[] Items { get; set; }
        public int XCount { get; set; }
        private int selectedItem;
        public int SelectedItem
        {
            get => selectedItem;
            set
            {
                this.selectedItem = value;
                OnControlEvent(EventArgs.Empty);
            }
        }

        public SelectionGrid(string[] items, int xCount, int selectedTab = 0)
        {
            Items = items;
            XCount = xCount;
            this.selectedItem = selectedTab;
        }

        public void SetItems(string[] items, int selectedIndex = 0)
        {
            this.Items = items;
            this.SelectedItem = selectedIndex;
        }

        public void Draw(GUIStyle gridStyle, params GUILayoutOption[] layoutOptions)
        {
            if (!Visible) return;
            GUILayout.BeginHorizontal();
            int selected;
            selected = GUILayout.SelectionGrid(SelectedItem, Items, XCount, gridStyle, layoutOptions);
            GUILayout.EndHorizontal();
            if (selected != SelectedItem) SelectedItem = selected;
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            this.Draw(new GUIStyle(GUI.skin.button));
        }
    }
}
