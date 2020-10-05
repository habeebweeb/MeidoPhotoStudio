using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class SelectionGrid : BaseControl
    {
        private SimpleToggle[] toggles;
        private int selectedItemIndex;
        public int SelectedItemIndex
        {
            get => selectedItemIndex;
            set
            {
                selectedItemIndex = Mathf.Clamp(value, 0, toggles.Length - 1);
                foreach (SimpleToggle toggle in toggles)
                {
                    toggle.value = toggle.toggleIndex == selectedItemIndex;
                }
                OnControlEvent(EventArgs.Empty);
            }
        }

        public SelectionGrid(string[] items, int selected = 0)
        {
            selectedItemIndex = Mathf.Clamp(selected, 0, items.Length - 1);
            toggles = MakeToggles(items);
        }

        private SimpleToggle[] MakeToggles(string[] items)
        {
            SimpleToggle[] toggles = new SimpleToggle[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                SimpleToggle toggle = new SimpleToggle(items[i], i == SelectedItemIndex) { toggleIndex = i };
                toggle.ControlEvent += (s, a) =>
                {
                    int value = (s as SimpleToggle).toggleIndex;
                    if (value != SelectedItemIndex) SelectedItemIndex = value;
                };
                toggles[i] = toggle;
            }
            return toggles;
        }

        public void SetItems(string[] items, int selectedItemIndex = -1)
        {
            if (selectedItemIndex < 0) selectedItemIndex = SelectedItemIndex;
            if (items.Length != toggles.Length)
            {
                toggles = MakeToggles(items);
            }
            else
            {
                for (int i = 0; i < items.Length; i++)
                {
                    string item = items[i];
                    toggles[i].value = i == SelectedItemIndex;
                    toggles[i].label = item;
                }
            }
            SelectedItemIndex = Mathf.Clamp(selectedItemIndex, 0, items.Length - 1);
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
                    if (!value) this.value = true;
                    else
                    {
                        this.value = value;
                        ControlEvent?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }
    }
}
