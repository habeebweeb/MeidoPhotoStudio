using System;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    public class MessageWindow : BaseWindow
    {
        private readonly MessageWindowManager messageWindowManager;
        private readonly TextField nameTextField;
        private readonly Slider fontSizeSlider;
        private readonly TextArea messageTextArea;
        private readonly Button okButton;
        public override Rect WindowRect
        {
            set
            {
                value.width = Mathf.Clamp(Screen.width * 0.4f, 440, Mathf.Infinity);
                value.height = Mathf.Clamp(Screen.height * 0.15f, 150, Mathf.Infinity);
                base.WindowRect = value;
            }
        }
        private int fontSize = 25;

        public MessageWindow(MessageWindowManager messageWindowManager)
        {
            WindowRect = WindowRect;
            windowRect.x = MiddlePosition.x;
            windowRect.y = Screen.height - WindowRect.height;
            this.messageWindowManager = messageWindowManager;
            nameTextField = new TextField();

            fontSizeSlider = new Slider(MessageWindowManager.fontBounds);
            fontSizeSlider.ControlEvent += ChangeFontSize;

            messageTextArea = new TextArea();

            okButton = new Button("OK");
            okButton.ControlEvent += ShowMessage;
        }

        private void ToggleVisibility()
        {
            if (messageWindowManager.ShowingMessage) messageWindowManager.CloseMessagePanel();
            else Visible = !Visible;
        }

        private void ChangeFontSize(object sender, EventArgs args)
        {
            fontSize = (int)fontSizeSlider.Value;
            messageWindowManager.FontSize = fontSize;
        }

        private void ShowMessage(object sender, EventArgs args)
        {
            Visible = false;
            messageWindowManager.ShowMessage(nameTextField.Value, messageTextArea.Value);
        }

        public override void Update()
        {
            base.Update();
            if (InputManager.GetKeyDown(MpsKey.ToggleMessage)) ToggleVisibility();
        }

        public override void Draw()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Name", GUILayout.ExpandWidth(false));
            nameTextField.Draw(GUILayout.Width(120));

            GUILayout.Space(30);

            GUILayout.Label("Font Size", GUILayout.ExpandWidth(false));
            fontSizeSlider.Draw(GUILayout.Width(120), GUILayout.ExpandWidth(false));
            GUILayout.Label($"{fontSize}pt");
            GUILayout.EndHorizontal();

            messageTextArea.Draw(GUILayout.MinHeight(90));
            okButton.Draw(GUILayout.ExpandWidth(false), GUILayout.Width(30));
        }

        public override void Deactivate()
        {
            messageWindowManager.CloseMessagePanel();
            Visible = false;
        }
    }
}
