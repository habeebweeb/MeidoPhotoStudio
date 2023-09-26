using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class MessageWindowManager : IManager
{
    public const string Header = "TEXTBOX";

    public static readonly SliderProp FontBounds = new(25f, 60f);

    private readonly MessageWindowMgr messageWindowMgr;
    private readonly GameObject subtitlesDisplayPanel;
    private readonly GameObject hitRetSprite;
    private readonly GameObject messageBox;
    private readonly GameObject messageButtons;
    private readonly UILabel messageLabel;
    private readonly UILabel speakerLabel;

    public MessageWindowManager()
    {
        messageWindowMgr = GameMain.Instance.MsgWnd;

        var messageWindowPanel =
            Utility.GetFieldValue<MessageWindowMgr, GameObject>(messageWindowMgr, "m_goMessageWindowPanel");

        var msgParent = UTY.GetChildObject(messageWindowPanel, "MessageViewer/MsgParent");

        messageButtons = UTY.GetChildObject(msgParent, "Buttons");
        hitRetSprite = UTY.GetChildObject(msgParent, "Hitret");
        subtitlesDisplayPanel = UTY.GetChildObject(msgParent, "SubtitlesDisplayPanel");

        messageBox = UTY.GetChildObject(msgParent, "MessageBox");
        speakerLabel = UTY.GetChildObject(msgParent, "SpeakerName/Name").GetComponent<UILabel>();
        messageLabel = UTY.GetChildObject(msgParent, "Message").GetComponent<UILabel>();
    }

    public bool ShowingMessage
    {
        get => messageWindowMgr.IsVisibleMessageViewer;
        private set
        {
            if (value)
                messageWindowMgr.OpenMessageWindowPanel();
            else
                messageWindowMgr.CloseMessageWindowPanel();
        }
    }

    public string MessageName
    {
        get => speakerLabel.text;
        private set => speakerLabel.text = value;
    }

    public string MessageText
    {
        get => messageLabel.text;
        private set => messageLabel.text = value;
    }

    public int FontSize
    {
        get => messageLabel.fontSize;
        set => messageLabel.fontSize = (int)Mathf.Clamp(value, FontBounds.Left, FontBounds.Right);
    }

    public void Update()
    {
    }

    public void Activate()
    {
        if (Product.supportMultiLanguage)
            subtitlesDisplayPanel.SetActive(false);

        ResetMessageBoxProperties();

        SetMessageBoxActive(true);

        SetMessageBoxExtrasActive(false);

        CloseMessagePanel();
    }

    public void Deactivate()
    {
        if (Product.supportMultiLanguage)
        {
            subtitlesDisplayPanel.SetActive(true);

            SetMessageBoxActive(false);
        }

        ResetMessageBoxProperties();

        SetMessageBoxExtrasActive(true);

        CloseMessagePanel();
    }

    public void ShowMessage(string name, string message)
    {
        MessageName = name;
        MessageText = message;
        ShowingMessage = true;
    }

    public void CloseMessagePanel()
    {
        if (!ShowingMessage)
            return;

        ShowingMessage = false;
    }

    private void SetMessageBoxActive(bool active)
    {
        messageBox.SetActive(active);
        messageLabel.gameObject.SetActive(active);
        speakerLabel.gameObject.SetActive(active);
    }

    private void SetMessageBoxExtrasActive(bool active)
    {
        messageButtons.SetActive(active);
        hitRetSprite.SetActive(active);
    }

    private void ResetMessageBoxProperties()
    {
        FontSize = 25;
        MessageName = string.Empty;
        MessageText = string.Empty;
    }
}
