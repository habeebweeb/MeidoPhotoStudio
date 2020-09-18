using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class ComboBox : BaseControl
    {
        private readonly TextField textField = new TextField();
        public Dropdown BaseDropDown { get; }
        public string Value
        {
            get => textField.Value;
            set => textField.Value = value;
        }

        public ComboBox(string[] itemList)
        {
            BaseDropDown = new Dropdown("▾", itemList);
            BaseDropDown.SelectionChange += (s, a) => textField.Value = BaseDropDown.SelectedItem;
            Value = itemList[0];
        }

        public void SetDropdownItems(string[] itemList)
        {
            string oldValue = Value;
            BaseDropDown.SetDropdownItems(itemList);
            Value = oldValue;
        }

        public void SetDropdownItem(int index, string newItem) => BaseDropDown.SetDropdownItem(index, newItem);

        public void SetDropdownItem(string newItem) => BaseDropDown.SetDropdownItem(newItem);

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter };
            Draw(buttonStyle, layoutOptions);
        }

        public void Draw(GUIStyle style, params GUILayoutOption[] layoutOptions)
        {
            GUILayout.BeginHorizontal();
            textField.Draw(new GUIStyle(GUI.skin.textField), layoutOptions);
            BaseDropDown.Draw(style, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }
    }
}
