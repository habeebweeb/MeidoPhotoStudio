using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class ColourPickerModal : BaseWindow
{
    private static readonly (float Width, float Height) WindowDimensions = (375, 360);

    private readonly Header colourPickerHeader;
    private readonly Slider redSlider;
    private readonly Slider greenSlider;
    private readonly Slider blueSlider;
    private readonly Slider alphaSlider;
    private readonly Label hexLabel;
    private readonly TextField hexTextField;
    private readonly Button cancelButton;
    private readonly Button okButton;
    private readonly LazyStyle colourPreviewBackgroundStyle = new(
        0,
        static () => new(GUI.skin.box)
        {
            normal = { background = Texture2D.whiteTexture },
        });

    private readonly LazyStyle colourPreviewStyle = new(
        0,
        static () => new(GUI.skin.box)
        {
            normal = { background = Texture2D.whiteTexture },
        });

    private readonly Translation translation;
    private Color colour;
    private Action<Color> colourPickedCallback;

    public ColourPickerModal(Translation translation, Color? initialColour = null)
    {
        this.translation = translation ?? throw new ArgumentNullException(nameof(translation));

        colour = initialColour ?? Color.white;

        colourPickerHeader = new(new LocalizableGUIContent(this.translation, "colourPickerModal", "header"));

        redSlider = new(0f, 1f, colour.r)
        {
            HasTextField = true,
            HasReset = true,
            Content = new LocalizableGUIContent(this.translation, "colourPickerModal", "redSliderLabel"),
        };

        redSlider.ControlEvent += OnRGBASliderChanged;

        greenSlider = new(0f, 1f, colour.g)
        {
            HasTextField = true,
            HasReset = true,
            Content = new LocalizableGUIContent(this.translation, "colourPickerModal", "greenSliderLabel"),
        };

        greenSlider.ControlEvent += OnRGBASliderChanged;

        blueSlider = new(0f, 1f, colour.b)
        {
            HasTextField = true,
            HasReset = true,
            Content = new LocalizableGUIContent(this.translation, "colourPickerModal", "blueSliderLabel"),
        };

        blueSlider.ControlEvent += OnRGBASliderChanged;

        alphaSlider = new(0f, 1f, colour.a)
        {
            HasTextField = true,
            HasReset = true,
            Content = new LocalizableGUIContent(this.translation, "colourPickerModal", "alphaSliderLabel"),
        };

        alphaSlider.ControlEvent += OnRGBASliderChanged;

        hexLabel = new(new LocalizableGUIContent(this.translation, "colourPickerModal", "hexLabel"));
        hexTextField = new(ColorUtility.ToHtmlStringRGB(colour));
        hexTextField.ChangedValue += OnHexTextFieldChanged;
        hexTextField.LostFocus += OnHexTextFieldFocusLost;

        okButton = new(new LocalizableGUIContent(this.translation, "colourPickerModal", "okButton"));
        okButton.ControlEvent += OnOKButtonPushed;

        cancelButton = new(new LocalizableGUIContent(this.translation, "colourPickerModal", "cancelButton"));
        cancelButton.ControlEvent += OnCancelButtonPushed;

        WindowRect = UIUtility.MiddlePosition(UIUtility.ScaledMinimum(WindowDimensions.Width), UIUtility.ScaledMinimum(WindowDimensions.Height));
    }

    public Color Colour
    {
        get => colour;
        set => SetColour(value);
    }

    public override void OnScreenDimensionsChanged(Vector2 newScreenDimensions)
    {
        base.OnScreenDimensionsChanged(newScreenDimensions);

        WindowRect = WindowRect with
        {
            width = UIUtility.ScaledMinimum(WindowDimensions.Width),
            height = UIUtility.ScaledMinimum(WindowDimensions.Height),
        };
    }

    public void PickColour(Action<Color> colourPickedCallback, Color? startingColour = null)
    {
        this.colourPickedCallback = colourPickedCallback ?? throw new ArgumentNullException(nameof(colourPickedCallback));

        if (startingColour is Color colour)
        {
            Colour = colour;
            UpdateControls();
        }

        WindowRect = WindowRect with
        {
            x = Screen.width / 2f - WindowRect.width / 2f,
            y = Screen.height / 2f - WindowRect.height / 2f,
        };

        Modal.Show(this);
    }

    public override void Draw()
    {
        GUILayout.BeginArea(new(10, 10, WindowRect.width - 10 * 2, WindowRect.height - 10 * 2));

        GUILayout.FlexibleSpace();

        colourPickerHeader.Draw();

        redSlider.Draw();
        greenSlider.Draw();
        blueSlider.Draw();
        alphaSlider.Draw();

        UIUtility.DrawBlackLine();

        GUILayout.BeginHorizontal();

        hexLabel.Draw(GUILayout.ExpandWidth(false));

        hexTextField.Draw(GUILayout.Width(UIUtility.Scaled(120)));

        GUILayout.Box(GUIContent.none, colourPreviewBackgroundStyle, GUILayout.Height(UIUtility.Scaled(25)));

        var previewRect = GUILayoutUtility.GetLastRect();
        var oldColour = GUI.backgroundColor;

        GUI.backgroundColor = Colour;
        GUI.Box(previewRect, GUIContent.none, colourPreviewStyle);
        GUI.backgroundColor = oldColour;

        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        okButton.Draw(GUILayout.MinWidth(UIUtility.Scaled(50)));
        cancelButton.Draw(GUILayout.MinWidth(UIUtility.Scaled(110)));

        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    private void OnRGBASliderChanged(object sender, EventArgs e)
    {
        SetColour(new(redSlider.Value, greenSlider.Value, blueSlider.Value, alphaSlider.Value));

        hexTextField.SetValueWithoutNotify(ColorUtility.ToHtmlStringRGBA(Colour));
    }

    private void OnHexTextFieldChanged(object sender, EventArgs e)
    {
        var value = hexTextField.Value;

        if (value.Length is 0)
            return;

        if (value[0] is not '#')
            value = value.Insert(0, "#");

        if (!ColorUtility.TryParseHtmlString(value, out var colour))
            return;

        redSlider.SetValueWithoutNotify(colour.r);
        greenSlider.SetValueWithoutNotify(colour.g);
        blueSlider.SetValueWithoutNotify(colour.b);
        alphaSlider.SetValueWithoutNotify(colour.a);

        SetColour(colour);
    }

    private void OnHexTextFieldFocusLost(object sender, EventArgs e)
    {
        var value = hexTextField.Value;

        if (ColorUtility.TryParseHtmlString(value, out _))
            return;

        hexTextField.SetValueWithoutNotify(ColorUtility.ToHtmlStringRGBA(Colour));
    }

    private void OnCancelButtonPushed(object sender, EventArgs e)
    {
        Modal.Close();

        colourPickedCallback = null;
    }

    private void OnOKButtonPushed(object sender, EventArgs e)
    {
        Modal.Close();

        colourPickedCallback?.Invoke(Colour);
    }

    private void UpdateControls()
    {
        redSlider.SetValueWithoutNotify(colour.r);
        greenSlider.SetValueWithoutNotify(colour.g);
        blueSlider.SetValueWithoutNotify(colour.b);
        alphaSlider.SetValueWithoutNotify(colour.a);
        hexTextField.SetValueWithoutNotify(ColorUtility.ToHtmlStringRGBA(Colour));
    }

    private void SetColour(Color colour, bool notify = true)
    {
        if (Colour == colour)
            return;

        this.colour = colour;

        if (!notify)
            return;
    }
}
