namespace MeidoPhotoStudio.Plugin;

/// <summary>Message window UI.</summary>
public partial class MessageWindow : BaseWindow
{
    private readonly MessageWindowManager messageWindowManager;
    private readonly TextField nameTextField;
    private readonly Slider fontSizeSlider;
    private readonly TextArea messageTextArea;
    private readonly Button okButton;

    private int fontSize = 25;

    public MessageWindow(MessageWindowManager messageWindowManager)
    {
        this.messageWindowManager = messageWindowManager;

        WindowRect = WindowRect;
        windowRect.x = MiddlePosition.x;
        windowRect.y = Screen.height - WindowRect.height;

        nameTextField = new();

        fontSizeSlider = new(MessageWindowManager.FontBounds);
        fontSizeSlider.ControlEvent += ChangeFontSize;

        messageTextArea = new();

        okButton = new("OK");
        okButton.ControlEvent += ShowMessage;
    }

    public override Rect WindowRect
    {
        set
        {
            value.width = Mathf.Clamp(Screen.width * 0.4f, 440, Mathf.Infinity);
            value.height = Mathf.Clamp(Screen.height * 0.15f, 150, Mathf.Infinity);
            base.WindowRect = value;
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
        GUILayout.Label($"{fontSize}pt");
        GUILayout.EndHorizontal();

        messageTextArea.Draw(GUILayout.MinHeight(90));
        okButton.Draw(GUILayout.ExpandWidth(false), GUILayout.Width(30));
    }

    public override void Deactivate()
    {
        messageWindowManager.CloseMessagePanel();
        Visible = false;
        ResetUI();
    }

    public override void Activate() =>
        ResetUI();

    private void ToggleVisibility()
    {
        if (messageWindowManager.ShowingMessage)
            messageWindowManager.CloseMessagePanel();
        else
            Visible = !Visible;
    }

    private void ChangeFontSize(object sender, EventArgs args)
    {
        fontSize = (int)fontSizeSlider.Value;

        if (updating)
            return;

        messageWindowManager.FontSize = fontSize;
    }

    private void ShowMessage(object sender, EventArgs args)
    {
        Visible = false;
        messageWindowManager.ShowMessage(nameTextField.Value, messageTextArea.Value);
    }

    private void ResetUI()
    {
        updating = true;

        fontSizeSlider.Value = MessageWindowManager.FontBounds.Left;
        nameTextField.Value = string.Empty;
        messageTextArea.Value = string.Empty;

        updating = false;
    }
}
