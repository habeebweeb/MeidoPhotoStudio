using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Core.Message;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

using Alignment = NGUIText.Alignment;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

/// <summary>Message window UI.</summary>
public partial class MessageWindow : BaseWindow
{
    private readonly MessageWindowManager messageWindowManager;
    private readonly InputRemapper inputRemapper;
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

    private Vector2 scrollPosition;

    public MessageWindow(
        Translation translation, MessageWindowManager messageWindowManager, InputRemapper inputRemapper)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.messageWindowManager = messageWindowManager ?? throw new ArgumentNullException(nameof(messageWindowManager));
        this.inputRemapper = inputRemapper ? inputRemapper : throw new ArgumentNullException(nameof(inputRemapper));

        this.messageWindowManager.PropertyChanged += OnMessageWindowPropertyChanged;

        var width = Mathf.Max(Screen.width * 0.5f, 440);
        var height = Mathf.Max(Screen.height * 0.17f, 150);

        WindowRect = new(
            Screen.width / 2f - width / 2f,
            Screen.height - height,
            width,
            height);

        nameTextField = new();

        var (left, right) = MessageWindowManager.FontBounds;

        fontSizeSlider = new(left, right);
        fontSizeSlider.ControlEvent += ChangeFontSize;

        messageTextArea = new();

        okButton = new(new LocalizableGUIContent(translation, "messageWindow", "okButton"));
        okButton.ControlEvent += ShowMessage;

        alignmentLabel = new(new LocalizableGUIContent(translation, "messageWindow", "alignmentLabel"));

        alignmentDropdown = new(
            new[] { Alignment.Left, Alignment.Center, Alignment.Right },
            formatter: AlignmentFormatter);

        alignmentDropdown.SelectionChanged += OnAlignmentChanged;

        translation.Initialized += OnTranslationInitialized;

        closeButton = new("X");
        closeButton.ControlEvent += OnCloseButtonPushed;

        nameLabel = new(new LocalizableGUIContent(translation, "messageWindow", "name"));
        fontSizeLabel = new(new LocalizableGUIContent(translation, "messageWindow", "fontSize"));
        fontPointLabel = new($"{messageWindowManager.FontSize}pt");

        textAreaStyle = new(
            messageWindowManager.FontSize,
            () => new(GUI.skin.textArea)
            {
                alignment = NGUIAlignmentToTextAnchor(messageWindowManager.MessageAlignment),
            });

        LabelledDropdownItem AlignmentFormatter(Alignment alignment, int index) =>
            new(translation["messageAlignmentTypes", alignment.ToLower()]);

        void OnTranslationInitialized(object sender, EventArgs e) =>
            alignmentDropdown.Reformat();
    }

    public override bool Enabled =>
        base.Enabled && !inputRemapper.Listening;

    public override void Draw()
    {
        GUILayout.BeginHorizontal();

        nameLabel.Draw(GUILayout.ExpandWidth(false));
        nameTextField.Draw(GUILayout.Width(UIUtility.Scaled(150)));

        GUILayout.Space(UIUtility.Scaled(20));

        fontSizeLabel.Draw(GUILayout.ExpandWidth(false));
        fontSizeSlider.Draw(GUILayout.Width(UIUtility.Scaled(120)), GUILayout.ExpandWidth(false));
        fontPointLabel.Draw();

        GUILayout.Space(UIUtility.Scaled(20));

        alignmentLabel.Draw(GUILayout.ExpandWidth(false));
        alignmentDropdown.Draw(GUILayout.Width(UIUtility.Scaled(120)));

        GUILayout.FlexibleSpace();

        closeButton.Draw();

        GUILayout.EndHorizontal();

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        messageTextArea.Draw(textAreaStyle, GUILayout.ExpandHeight(true));

        GUILayout.EndScrollView();

        okButton.Draw(GUILayout.ExpandWidth(false), GUILayout.Width(UIUtility.Scaled(60)));
    }

    public override void OnScreenDimensionsChanged(Vector2 newScreenDimensions)
    {
        var newRect = WindowRect with
        {
            width = Mathf.Max(Screen.width * 0.5f, 440f),
            height = Mathf.Max(Screen.height * 0.17f, 150f),
        };

        if (newRect.xMax > newScreenDimensions.x)
            newRect = newRect with
            {
                x = newScreenDimensions.x - WindowRect.width - 5f,
            };

        if (newRect.yMax > newScreenDimensions.y)
            newRect = WindowRect with
            {
                y = newScreenDimensions.y - WindowRect.height - 5f,
            };

        WindowRect = newRect;
    }

    public override void Deactivate()
    {
        messageWindowManager.CloseMessagePanel();
        Visible = false;
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
            alignmentDropdown.SetSelectedIndexWithoutNotify(alignmentDropdown.IndexOf(messageWindowManager.MessageAlignment));

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
