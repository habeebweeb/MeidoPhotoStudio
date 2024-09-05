using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI;

using Alignment = NGUIText.Alignment;

namespace MeidoPhotoStudio.Plugin;

/// <summary>Message window UI.</summary>
public partial class MessageWindow : BaseWindow
{
    private readonly MessageWindowManager messageWindowManager;
    private readonly TextField nameTextField;
    private readonly Slider fontSizeSlider;
    private readonly TextArea messageTextArea;
    private readonly Button okButton;
    private readonly Button closeButton;
    private readonly Label alignmentLabel;
    private readonly Dropdown<Alignment> alignmentDropdown;
    private readonly Label nameLabel;
    private readonly Label fontSizeLabel;
    private readonly Label fontPointLabel;
    private readonly LazyStyle textAreaStyle;

    public MessageWindow(MessageWindowManager messageWindowManager)
    {
        this.messageWindowManager = messageWindowManager;
        this.messageWindowManager.PropertyChanged += OnMessageWindowPropertyChanged;

        WindowRect = WindowRect;
        windowRect.x = MiddlePosition.x;
        windowRect.y = Screen.height - WindowRect.height;

        nameTextField = new();

        fontSizeSlider = new(MessageWindowManager.FontBounds);
        fontSizeSlider.ControlEvent += ChangeFontSize;

        messageTextArea = new();

        okButton = new(Translation.Get("messageWindow", "okButton"));
        okButton.ControlEvent += ShowMessage;

        alignmentLabel = new(Translation.Get("messageWindow", "alignmentLabel"));

        alignmentDropdown = new(
            new Alignment[] { Alignment.Left, Alignment.Center, Alignment.Right },
            formatter: AlignmentFormatter);
        alignmentDropdown.SelectionChanged += OnAlignmentChanged;

        closeButton = new("X");
        closeButton.ControlEvent += OnCloseButtonPushed;

        nameLabel = new(Translation.Get("messageWindow", "name"));
        fontSizeLabel = new(Translation.Get("messageWindow", "fontSize"));
        fontPointLabel = new($"{messageWindowManager.FontSize}pt");

        textAreaStyle = new(
            messageWindowManager.FontSize,
            () => new(GUI.skin.textArea)
            {
                alignment = NGUIAlignmentToTextAnchor(messageWindowManager.MessageAlignment),
            });

        static string AlignmentFormatter(Alignment alignment, int index) =>
            Translation.Get("messageWindow", string.Concat("align", alignment.ToString()));
    }

    public override Rect WindowRect
    {
        set
        {
            value.width = Mathf.Clamp(Screen.width * 0.5f, 440, Mathf.Infinity);
            value.height = Mathf.Clamp(Screen.height * 0.17f, 150, Mathf.Infinity);
            base.WindowRect = value;
        }
    }

    public override void Draw()
    {
        GUILayout.BeginHorizontal();

        nameLabel.Draw(GUILayout.ExpandWidth(false));
        nameTextField.Draw(GUILayout.Width(Utility.GetPix(150)));

        GUILayout.Space(Utility.GetPix(20));

        fontSizeLabel.Draw(GUILayout.ExpandWidth(false));
        fontSizeSlider.Draw(GUILayout.Width(Utility.GetPix(120)), GUILayout.ExpandWidth(false));
        fontPointLabel.Draw();

        GUILayout.Space(Utility.GetPix(20));

        alignmentLabel.Draw(GUILayout.ExpandWidth(false));
        alignmentDropdown.Draw(GUILayout.Width(Utility.GetPix(120)));

        GUILayout.FlexibleSpace();

        closeButton.Draw();

        GUILayout.EndHorizontal();

        scrollPos = GUILayout.BeginScrollView(scrollPos);

        messageTextArea.Draw(textAreaStyle, GUILayout.ExpandHeight(true));

        GUILayout.EndScrollView();

        okButton.Draw(GUILayout.ExpandWidth(false), GUILayout.Width(Utility.GetPix(60)));
    }

    public override void Deactivate()
    {
        messageWindowManager.CloseMessagePanel();
        Visible = false;
    }

    public override void OnScreenDimensionsChanged(Vector2 newScreenDimensions)
    {
        base.OnScreenDimensionsChanged(newScreenDimensions);

        if (WindowRect.xMax > newScreenDimensions.x)
            WindowRect = WindowRect with
            {
                x = newScreenDimensions.x - WindowRect.width - 5f,
            };

        if (WindowRect.yMax > newScreenDimensions.y)
            WindowRect = WindowRect with
            {
                y = newScreenDimensions.y - WindowRect.height - 5f,
            };
    }

    protected override void ReloadTranslation()
    {
        okButton.Label = Translation.Get("messageWindow", "okButton");
        nameLabel.Text = Translation.Get("messageWindow", "name");
        fontSizeLabel.Text = Translation.Get("messageWindow", "fontSize");
        alignmentLabel.Text = Translation.Get("messageWindow", "alignmentLabel");
        alignmentDropdown.Reformat();
    }

    private static TextAnchor NGUIAlignmentToTextAnchor(Alignment alignment) =>
        alignment switch
        {
            Alignment.Left => TextAnchor.UpperLeft,
            Alignment.Right => TextAnchor.UpperRight,
            Alignment.Center => TextAnchor.UpperCenter,
            _ => TextAnchor.UpperLeft,
        };

    private void ToggleVisibility()
    {
        if (messageWindowManager.ShowingMessage)
            messageWindowManager.CloseMessagePanel();
        else
            Visible = !Visible;
    }

    private void OnMessageWindowPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MessageWindowManager.MessageName) && !nameTextField.HasFocus)
        {
            nameTextField.Value = messageWindowManager.MessageName;
        }
        else if (e.PropertyName is nameof(MessageWindowManager.MessageText) && !messageTextArea.HasFocus)
        {
            messageTextArea.Value = messageWindowManager.MessageText;
        }
        else if (e.PropertyName is nameof(MessageWindowManager.FontSize))
        {
            fontSizeSlider.SetValueWithoutNotify(messageWindowManager.FontSize);
            textAreaStyle.FontSize = messageWindowManager.FontSize;
            fontPointLabel.Text = $"{messageWindowManager.FontSize}pt";
        }
        else if (e.PropertyName is nameof(MessageWindowManager.MessageAlignment))
        {
            alignmentDropdown.SetSelectedIndexWithoutNotify(
                alignmentDropdown.IndexOf(alignment => alignment == messageWindowManager.MessageAlignment));

            textAreaStyle.TrySet(style => style.alignment = NGUIAlignmentToTextAnchor(messageWindowManager.MessageAlignment));
        }
    }

    private void ChangeFontSize(object sender, EventArgs args)
    {
        messageWindowManager.FontSize = (int)fontSizeSlider.Value;
        textAreaStyle.FontSize = (int)fontSizeSlider.Value;
        fontPointLabel.Text = $"{(int)fontSizeSlider.Value}pt";
    }

    private void ShowMessage(object sender, EventArgs args)
    {
        Visible = false;
        messageWindowManager.ShowMessage(nameTextField.Value, messageTextArea.Value);
    }

    private void OnCloseButtonPushed(object sender, EventArgs e) =>
        Visible = false;

    private void OnAlignmentChanged(object sender, DropdownEventArgs<Alignment> e)
    {
        messageWindowManager.MessageAlignment = e.Item;
        textAreaStyle.TrySet(style => style.alignment = NGUIAlignmentToTextAnchor(e.Item));
    }
}
