using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class MessageWindow : BaseWindow
    {
        private MessageWindowManager messageWindowManager;
        private TextField nameTextField;
        private Slider fontSizeSlider;
        private TextArea messageTextArea;
        private Button okButton;
        public override Rect WindowRect
        {
            get
            {
                windowRect.width = Mathf.Clamp(Screen.width * 0.4f, 440, Mathf.Infinity);
                windowRect.height = Mathf.Clamp(Screen.height * 0.15f, 150, Mathf.Infinity);
                windowRect.x = Mathf.Clamp(
                    windowRect.x,
                    -windowRect.width + Utility.GetPix(20),
                    Screen.width - Utility.GetPix(20)
                );
                windowRect.y = Mathf.Clamp(
                    windowRect.y,
                    -windowRect.height + Utility.GetPix(20),
                    Screen.height - Utility.GetPix(20)
                );
                return windowRect;
            }
            set => windowRect = value;
        }
        private int fontSize = 25;
        private bool showingMessage = false;

        public MessageWindow(MessageWindowManager messageWindowManager) : base()
        {
            windowRect = new Rect(Screen.width / 2f - 220f, Screen.height - 150f, 440f, 150f);
            this.messageWindowManager = messageWindowManager;
            nameTextField = new TextField();
            Controls.Add(nameTextField);

            fontSizeSlider = new Slider(MessageWindowManager.fontBounds);
            fontSizeSlider.ControlEvent += ChangeFontSize;
            Controls.Add(fontSizeSlider);

            messageTextArea = new TextArea();
            Controls.Add(messageTextArea);

            okButton = new Button("OK");
            okButton.ControlEvent += ShowMessage;
            Controls.Add(okButton);
        }

        public void ToggleVisibility()
        {
            if (showingMessage)
            {
                messageWindowManager.CloseMessagePanel();
                showingMessage = false;
            }
            else
            {
                Visible = !Visible;
            }
        }

        private void ChangeFontSize(object sender, EventArgs args)
        {
            fontSize = (int)fontSizeSlider.Value;
            messageWindowManager.SetFontSize(fontSize);
        }

        private void ShowMessage(object sender, EventArgs args)
        {
            Visible = false;
            showingMessage = true;
            messageWindowManager.ShowMessage(nameTextField.Value, messageTextArea.Value);
        }

        public override void Update()
        {
            base.Update();
            if (Input.GetKeyDown(KeyCode.M))
            {
                this.ToggleVisibility();
            }
        }

        public override void Draw()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Name", GUILayout.ExpandWidth(false));
            nameTextField.Draw(GUILayout.Width(120));

            GUILayout.Space(30);

            GUILayout.Label("Font Size", GUILayout.ExpandWidth(false));
            fontSizeSlider.Draw(GUILayout.Width(120), GUILayout.ExpandWidth(false));
            GUILayout.Label($"{(int)fontSize}pt");
            GUILayout.EndHorizontal();

            messageTextArea.Draw(GUILayout.MinHeight(90));
            okButton.Draw(GUILayout.ExpandWidth(false), GUILayout.Width(30));
        }

        public override void Deactivate()
        {
            if (showingMessage)
            {
                messageWindowManager.CloseMessagePanel();
                showingMessage = false;
            }

            Visible = false;
        }
    }
}
