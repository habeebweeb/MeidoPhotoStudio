using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class MessageWindow : BaseWindow
    {
        TextField nameTextField;
        Slider fontSizeSlider;
        TextArea messageTextArea;
        Button okButton;
        private int fontSize = 25;
        private bool showingMessage = false;

        public MessageWindow() : base()
        {
            nameTextField = new TextField();
            Controls.Add(nameTextField);

            fontSizeSlider = new Slider(25, 60);
            fontSizeSlider.ControlEvent += ChangeFontSize;
            Controls.Add(fontSizeSlider);

            messageTextArea = new TextArea();
            Controls.Add(messageTextArea);

            okButton = new Button("OK");
            okButton.ControlEvent += SetMessage;
            Controls.Add(okButton);
        }

        public void SetVisibility()
        {
            if (showingMessage)
            {
                GameObject messageGameObject = GameObject.Find("__GameMain__/SystemUI Root").transform.Find("MessageWindowPanel").gameObject;
                MessageWindowMgr messageWindowMgr = GameMain.Instance.ScriptMgr.adv_kag.MessageWindowMgr;
                messageWindowMgr.CloseMessageWindowPanel();
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

            GameObject gameObject = GameObject.Find("__GameMain__/SystemUI Root").transform.Find("MessageWindowPanel").gameObject;
            UILabel uiLabel = UTY.GetChildObject(gameObject, "MessageViewer/MsgParent/Message", false).GetComponent<UILabel>();
            Utility.SetFieldValue<UILabel, int>(uiLabel, "mFontSize", fontSize);
        }

        private void SetMessage(object sender, EventArgs args)
        {
            Visible = false;
            showingMessage = true;
            GameObject messageGameObject = GameObject.Find("__GameMain__/SystemUI Root").transform.Find("MessageWindowPanel").gameObject;
            MessageWindowMgr messageWindowMgr = GameMain.Instance.ScriptMgr.adv_kag.MessageWindowMgr;
            messageWindowMgr.OpenMessageWindowPanel();

            UILabel component = UTY.GetChildObject(messageGameObject, "MessageViewer/MsgParent/Message", false).GetComponent<UILabel>();
            UILabel nameComponent = UTY.GetChildObject(messageGameObject, "MessageViewer/MsgParent/SpeakerName/Name", false).GetComponent<UILabel>();

            MessageClass inst = new MessageClass(messageGameObject, messageWindowMgr);
            // Fix for ENG version: reconfigure MessageClass to behave as in JP game
            inst.subtitles_manager_.visible = false;
            inst.subtitles_manager_ = null;
            component.gameObject.SetActive(true);
            nameComponent.gameObject.SetActive(true);
            UTY.GetChildObject(messageGameObject, "MessageViewer/MsgParent/MessageBox", false).SetActive(true);
            Utility.SetFieldValue<MessageClass, UILabel>(inst, "message_label_", component);
            Utility.SetFieldValue<MessageClass, UILabel>(inst, "name_label_", nameComponent);

            component.ProcessText();
            Utility.SetFieldValue<UILabel, int>(component, "mFontSize", fontSize);

            inst.SetText(nameTextField.Value, messageTextArea.Value, "", 0, AudioSourceMgr.Type.System);
            inst.FinishChAnime();
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
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
    }
}
