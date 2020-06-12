using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class SelectionGrid : BaseControl
    {
        private SimpleToggle[] toggles;
        private int selectedItem;
        public int SelectedItem
        {
            get => selectedItem;
            set
            {
                this.selectedItem = Mathf.Clamp(value, 0, this.toggles.Length - 1);
                foreach (SimpleToggle toggle in toggles)
                {
                    toggle.value = toggle.toggleIndex == this.selectedItem;
                }
                OnControlEvent(EventArgs.Empty);
            }
        }

        public SelectionGrid(string[] items, int selected = 0)
        {
            this.selectedItem = Mathf.Clamp(selected, 0, items.Length - 1);
            toggles = MakeToggles(items);
        }

        private SimpleToggle[] MakeToggles(string[] items)
        {
            SimpleToggle[] toggles = new SimpleToggle[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                SimpleToggle toggle = new SimpleToggle(items[i], i == SelectedItem);
                toggle.toggleIndex = i;
                toggle.ControlEvent += (s, a) => this.SelectedItem = (s as SimpleToggle).toggleIndex;
                toggles[i] = toggle;
            }
            return toggles;
        }

        public void SetItems(string[] items, int selectedItem = 0)
        {
            this.SelectedItem = Mathf.Clamp(selectedItem, 0, items.Length - 1);
            if (items.Length != toggles.Length)
            {
                this.toggles = MakeToggles(items);
            }
            else
            {
                for (int i = 0; i < items.Length; i++)
                {
                    string item = items[i];
                    this.toggles[i].value = i == SelectedItem;
                    this.toggles[i].label = item;
                }
            }
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            GUILayout.BeginHorizontal();
            foreach (SimpleToggle toggle in toggles)
            {
                toggle.Draw(layoutOptions);
            }
            GUILayout.EndHorizontal();
        }

        private class SimpleToggle
        {
            public int toggleIndex;
            public bool value;
            public string label;
            public event EventHandler ControlEvent;

            public SimpleToggle(string label, bool value = false)
            {
                this.label = label;
                this.value = value;
            }

            public void Draw(params GUILayoutOption[] layoutOptions)
            {
                bool value = GUILayout.Toggle(this.value, label, layoutOptions);
                if (value != this.value)
                {
                    this.value = value;
                    ControlEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
