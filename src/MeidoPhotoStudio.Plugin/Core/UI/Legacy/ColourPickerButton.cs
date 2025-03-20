using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class ColourPickerButton(Color colour) : BaseControl
{
    private static readonly LazyStyle ButtonStyle = new(
        0,
        static () => new(GUI.skin.box)
        {
            normal = { background = Texture2D.whiteTexture, },
        });

    private static readonly LazyStyle ColourPreviewBackgroundStyle = new(
        0,
        static () => new(GUI.skin.box)
        {
            normal = { background = Texture2D.whiteTexture },
        });

    private Color colour = colour;

    public event EventHandler<ColourPickerButtonEventArgs> PickedColour;

    public Color Colour
    {
        get => colour;
        set
        {
            if (colour == value)
                return;

            colour = value;
        }
    }

    internal static ColourPickerModal ColourPickerModal { get; set; }

    public override void Draw(params GUILayoutOption[] layoutOptions)
    {
        GUILayout.Box(GUIContent.none, ColourPreviewBackgroundStyle, layoutOptions);

        var previewRect = GUILayoutUtility.GetLastRect();
        var originalColour = GUI.backgroundColor;

        GUI.backgroundColor = Colour;

        if (GUI.Button(previewRect, GUIContent.none, ButtonStyle))
        {
            if (ColourPickerModal is not null)
                ColourPickerModal.PickColour(OnColourPicked, Colour);
            else
                Plugin.Logger.LogWarning($"'{nameof(ColourPickerModal)}' is null!");
        }

        GUI.backgroundColor = originalColour;
    }

    private void OnColourPicked(Color colour)
    {
        Colour = colour;

        PickedColour?.Invoke(this, new(Colour));
    }
}
