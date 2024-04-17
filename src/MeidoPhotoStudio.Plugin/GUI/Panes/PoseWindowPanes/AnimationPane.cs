using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Character;

namespace MeidoPhotoStudio.Plugin;

public class AnimationPane : BasePane
{
    private const string PlayIconBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAM0lEQVQ4EWP4//8/AyWYYdAZAAMUG0C0QYQMIGgQsQbgNIhUAzAMorsB9A9E+iekIZiZABgcOPIp+HO6AAAAAElFTkSuQmCC";

    private const string PauseIconBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAIUlEQVQ4EWP4//8/AyWYYVAagAzwiY0aMGoAbQ0YYpkJANk+OPKm3865AAAAAElFTkSuQmCC";

    private static Texture2D playIcon;
    private static Texture2D pauseIcon;

    private readonly Slider animationSlider;
    private readonly Toggle playPauseButton;
    private readonly Button stepLeftButton;
    private readonly Button stepRightButton;
    private readonly NumericalTextField stepAmountField;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly Toggle paneHeader;

    public AnimationPane(SelectionController<CharacterController> characterSelectionController)
    {
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.characterSelectionController.Selecting += OnCharacterSelectionChanging;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        animationSlider = new Slider(string.Empty, 0f, 1f)
        {
            HasTextField = true,
        };

        animationSlider.ControlEvent += OnAnimationSliderChange;

        playPauseButton = new(PauseIcon, true);
        playPauseButton.ControlEvent += OnPlayPauseButtonPushed;

        stepLeftButton = new("<");
        stepLeftButton.ControlEvent += OnStepLeftButtonPushed;

        stepRightButton = new(">");
        stepRightButton.ControlEvent += OnStepRightButtonPushed;

        stepAmountField = new(0.01f);
        stepAmountField.ControlEvent += OnStepAmountFieldChanged;

        paneHeader = new(Translation.Get("characterAnimationPane", "header"), true);
    }

    private static Texture2D PlayIcon =>
        playIcon ? playIcon : playIcon = LoadIconFromBase64(PlayIconBase64);

    private static Texture2D PauseIcon =>
        pauseIcon ? pauseIcon : pauseIcon = LoadIconFromBase64(PauseIconBase64);

    private AnimationController CurrentAnimation =>
        characterSelectionController.Current?.Animation;

    private bool Playing =>
        playPauseButton.Value;

    public override void Draw()
    {
        var currentAnimation = CurrentAnimation;

        GUI.enabled = currentAnimation is not null;

        paneHeader.Draw();
        MpsGui.WhiteLine();

        if (!paneHeader.Value)
            return;

        if (currentAnimation is not null)
        {
            if (currentAnimation.Playing)
            {
                animationSlider.SetValueWithoutNotify(currentAnimation.Time);
            }
            else
            {
                if (Playing)
                {
                    playPauseButton.SetEnabledWithoutNotify(false);
                    UpdatePlayPauseButtonIcon();
                }
            }
        }

        animationSlider.Draw();

        GUILayout.BeginHorizontal();

        var noExpandWidth = GUILayout.ExpandWidth(false);

        playPauseButton.Draw(new GUIStyle(GUI.skin.button), GUILayout.Width(45f));
        stepLeftButton.Draw(noExpandWidth);
        stepRightButton.Draw(noExpandWidth);

        GUILayout.FlexibleSpace();

        stepAmountField.Draw(GUILayout.Width(60f));

        GUILayout.EndHorizontal();
    }

    public override void UpdatePane()
    {
        if (CurrentAnimation is null)
            return;

        playPauseButton.SetEnabledWithoutNotify(CurrentAnimation.Playing);

        UpdatePlayPauseButtonIcon();

        animationSlider.SetValueWithoutNotify(CurrentAnimation.Time);
    }

    protected override void ReloadTranslation() =>
        paneHeader.Label = Translation.Get("characterAnimationPane", "header");

    private static Texture2D LoadIconFromBase64(string base64)
    {
        var icon = new Texture2D(16, 16, TextureFormat.RGB24, false)
        {
            filterMode = FilterMode.Point,
        };

        icon.LoadImage(Convert.FromBase64String(base64));

        icon.Apply();

        return icon;
    }

    private void OnCharacterSelectionChanging(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (CurrentAnimation is null)
            return;

        CurrentAnimation.ChangedAnimation -= OnAnimationChanged;
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (CurrentAnimation is null)
            return;

        CurrentAnimation.ChangedAnimation += OnAnimationChanged;

        UpdateSliderRange();

        UpdatePane();
    }

    private void OnAnimationChanged(object sender, EventArgs e)
    {
        if (CurrentAnimation is null)
            return;

        UpdateSliderRange();

        UpdatePane();
    }

    private void UpdateSliderRange() =>
        animationSlider.Right = CurrentAnimation.Length - 0.0001f;

    private void OnPlayPauseButtonPushed(object sender, EventArgs e)
    {
        if (CurrentAnimation is null)
            return;

        UpdatePlayPauseButtonIcon();

        CurrentAnimation.Playing = Playing;

        UpdatePane();
    }

    private void OnAnimationSliderChange(object sender, EventArgs e)
    {
        if (CurrentAnimation is null)
            return;

        CurrentAnimation.Time = animationSlider.Value;
    }

    private void OnStepRightButtonPushed(object sender, EventArgs e) =>
        animationSlider.Value += stepAmountField.Value;

    private void OnStepLeftButtonPushed(object sender, EventArgs e) =>
        animationSlider.Value -= stepAmountField.Value;

    private void OnStepAmountFieldChanged(object sender, EventArgs e)
    {
        if (stepAmountField.Value < 0f)
            stepAmountField.SetValueWithoutNotify(0f);
    }

    private void UpdatePlayPauseButtonIcon() =>
        playPauseButton.Icon = Playing ? PauseIcon : PlayIcon;
}
