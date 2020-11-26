using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class MessageWindowManager : IManager, ISerializable
    {
        public const string header = "TEXTBOX";
        public static readonly SliderProp fontBounds = new SliderProp(25f, 60f);
        private static GameObject sysRoot;
        private readonly MessageClass msgClass;
        private readonly MessageWindowMgr msgWnd;
        private readonly UILabel msgLabel;
        private readonly UILabel nameLabel;
        private readonly GameObject msgGameObject;
        public bool ShowingMessage { get; private set; }
        private string messageName = string.Empty;
        private string messageText = string.Empty;
        public int FontSize
        {
            get => msgLabel.fontSize;
            set => msgLabel.fontSize = (int)Mathf.Clamp(value, fontBounds.Left, fontBounds.Right);
        }

        static MessageWindowManager()
        {
            InputManager.Register(MpsKey.ToggleMessage, KeyCode.M, "Show/hide message box");
        }

        public MessageWindowManager()
        {
            sysRoot = GameObject.Find("__GameMain__/SystemUI Root");
            msgWnd = GameMain.Instance.MsgWnd;
            msgGameObject = sysRoot.transform.Find("MessageWindowPanel").gameObject;
            msgClass = new MessageClass(msgGameObject, msgWnd);
            nameLabel = UTY.GetChildObject(msgGameObject, "MessageViewer/MsgParent/SpeakerName/Name")
                .GetComponent<UILabel>();
            msgLabel = UTY.GetChildObject(msgGameObject, "MessageViewer/MsgParent/Message")
                .GetComponent<UILabel>();
            Utility.SetFieldValue(msgClass, "message_label_", msgLabel);
            Utility.SetFieldValue(msgClass, "name_label_", nameLabel);
            Activate();
        }

        public void Activate() => SetPhotoMessageWindowActive(true);

        public void Deactivate()
        {
            msgWnd.CloseMessageWindowPanel();
            SetPhotoMessageWindowActive(false);
        }

        public void Serialize(System.IO.BinaryWriter binaryWriter)
        {
            binaryWriter.Write(header);
            binaryWriter.Write(ShowingMessage);
            binaryWriter.Write(FontSize);
            binaryWriter.WriteNullableString(messageName);
            binaryWriter.WriteNullableString(messageText);
        }

        public void Deserialize(System.IO.BinaryReader binaryReader)
        {
            CloseMessagePanel();
            var showingMessage = binaryReader.ReadBoolean();
            FontSize = binaryReader.ReadInt32();
            messageName = binaryReader.ReadNullableString();
            messageText = binaryReader.ReadNullableString();
            if (showingMessage) ShowMessage(messageName, messageText);
        }

        public void Update() { }

        private void SetPhotoMessageWindowActive(bool active)
        {
            UTY.GetChildObject(msgGameObject, "MessageViewer/MsgParent/MessageBox").SetActive(active);
            UTY.GetChildObject(msgGameObject, "MessageViewer/MsgParent/Hitret")
                .GetComponent<UISprite>().enabled = !active;
            nameLabel.gameObject.SetActive(active);
            msgLabel.gameObject.SetActive(active);

            Transform transform = sysRoot.transform.Find("MessageWindowPanel/MessageViewer/MsgParent/Buttons");
            var msgButtons = new[]
            {
                MessageWindowMgr.MessageWindowUnderButton.Skip,
                MessageWindowMgr.MessageWindowUnderButton.Auto,
                MessageWindowMgr.MessageWindowUnderButton.Voice,
                MessageWindowMgr.MessageWindowUnderButton.BackLog,
                MessageWindowMgr.MessageWindowUnderButton.Config
            };
            foreach (MessageWindowMgr.MessageWindowUnderButton msgButton in msgButtons)
            {
                transform.Find(msgButton.ToString()).gameObject.SetActive(!active);
            }

            if (!msgClass.subtitles_manager_) return;

            msgClass.subtitles_manager_.visible = false;
            msgClass.subtitles_manager_ = null;
        }

        public void ShowMessage(string name, string message)
        {
            messageName = name;
            messageText = message;
            ShowingMessage = true;
            msgWnd.OpenMessageWindowPanel();
            msgLabel.ProcessText();
            msgClass.SetText(name, message, "", 0, AudioSourceMgr.Type.System);
            msgClass.FinishChAnime();
        }

        public void CloseMessagePanel()
        {
            ShowingMessage = false;
            msgWnd.CloseMessageWindowPanel();
        }
    }
}
