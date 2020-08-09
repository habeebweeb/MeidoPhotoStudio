using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class ComboBox : BaseControl
    {
        private TextField textField = new TextField();
        public Dropdown BaseDropDown { get; private set; }
        public string Value
        {
            get => textField.Value;
            set => textField.Value = value;
        }

        public ComboBox(string[] itemList)
        {
            this.BaseDropDown = new Dropdown("▾", itemList);
            this.BaseDropDown.SelectionChange += (s, a) => textField.Value = BaseDropDown.SelectedItem;
            this.Value = itemList[0];
        }

        public void SetDropdownItems(string[] itemList)
        {
            string oldValue = this.Value;
            BaseDropDown.SetDropdownItems(itemList);
            this.Value = oldValue;
        }

        public void SetDropdownItem(int index, string newItem) => BaseDropDown.SetDropdownItem(index, newItem);

        public void SetDropdownItem(string newItem) => BaseDropDown.SetDropdownItem(newItem);

        public void Draw(float buttonSize, GUIStyle textFieldStyle, params GUILayoutOption[] layoutOptions)
        {
            GUILayout.BeginHorizontal();
            textField.Draw(textFieldStyle, layoutOptions);
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.alignment = TextAnchor.MiddleCenter;
            BaseDropDown.Draw(buttonStyle, GUILayout.Width(buttonSize), GUILayout.Height(buttonSize));
            GUILayout.EndHorizontal();
        }

        public void Draw(float buttonSize, params GUILayoutOption[] layoutOptions)
        {
            Draw(buttonSize, new GUIStyle(GUI.skin.textField), layoutOptions);
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            GUILayout.BeginHorizontal();
            textField.Draw(new GUIStyle(GUI.skin.textField), layoutOptions);
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.alignment = TextAnchor.MiddleCenter;
            BaseDropDown.Draw(buttonStyle, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }
    }
}
