using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Character;

namespace MeidoPhotoStudio.Plugin;

public class FreeLookPane : BasePane
{
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly Toggle paneHeader;
    private readonly Toggle freeLookToggle;
    private readonly Slider offsetLookXSlider;
    private readonly Slider offsetLookYSlider;
    private readonly Toggle eyeToCameraToggle;
    private readonly Toggle headToCameraToggle;

    private string bindLabel = string.Empty;

    public FreeLookPane(SelectionController<CharacterController> characterSelectionController)
    {
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        paneHeader = new(Translation.Get("freeLookPane", "header"), true);

        freeLookToggle = new(Translation.Get("freeLookPane", "freeLookToggle"), false);
        freeLookToggle.ControlEvent += OnFreeLookToggleChanged;

        offsetLookXSlider = new(Translation.Get("freeLookPane", "xSlider"), -0.6f, 0.6f)
        {
            HasReset = true,
        };

        offsetLookXSlider.ControlEvent += OnEyeXSliderChanged;

        offsetLookYSlider = new(Translation.Get("freeLookPane", "ySlider"), 0.5f, -0.55f)
        {
            HasReset = true,
        };

        offsetLookYSlider.ControlEvent += OnEyeYSliderChanged;

        eyeToCameraToggle = new(Translation.Get("freeLookPane", "eyeToCamToggle"), true);
        eyeToCameraToggle.ControlEvent += OnBindEyeToggleChanged;

        headToCameraToggle = new(Translation.Get("freeLookPane", "headToCamToggle"), true);
        headToCameraToggle.ControlEvent += OnBindHeadToggleChanged;

        bindLabel = Translation.Get("freeLookPane", "bindLabel");
    }

    private HeadController CurrentHead =>
        characterSelectionController.Current?.Head;

    public override void Draw()
    {
        var enabled = CurrentHead is not null;

        GUI.enabled = enabled;

        paneHeader.Draw();
        MpsGui.WhiteLine();

        if (!paneHeader.Value)
            return;

        var eitherBindingEnabled = eyeToCameraToggle.Value || headToCameraToggle.Value;

        GUI.enabled = enabled && eitherBindingEnabled;

        freeLookToggle.Draw();

        var sliderWidth = GUILayout.Width(parent.WindowRect.width / 2 - 15f);

        GUI.enabled = enabled && eitherBindingEnabled && freeLookToggle.Value;

        GUILayout.BeginHorizontal();

        offsetLookXSlider.Draw(sliderWidth);
        offsetLookYSlider.Draw(sliderWidth);

        GUILayout.EndHorizontal();

        GUI.enabled = enabled;

        GUILayout.BeginHorizontal();

        GUILayout.Label(bindLabel, GUILayout.ExpandWidth(false));

        eyeToCameraToggle.Draw();
        headToCameraToggle.Draw();

        GUILayout.EndHorizontal();
    }

    protected override void ReloadTranslation()
    {
        paneHeader.Label = Translation.Get("freeLookPane", "header");
        freeLookToggle.Label = Translation.Get("freeLookPane", "freeLookToggle");
        offsetLookXSlider.Label = Translation.Get("freeLookPane", "xSlider");
        offsetLookYSlider.Label = Translation.Get("freeLookPane", "ySlider");
        eyeToCameraToggle.Label = Translation.Get("freeLookPane", "eyeToCamToggle");
        headToCameraToggle.Label = Translation.Get("freeLookPane", "headToCamToggle");
        bindLabel = Translation.Get("freeLookPane", "bindLabel");
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        freeLookToggle.SetEnabledWithoutNotify(CurrentHead.FreeLook);
        offsetLookXSlider.SetValueWithoutNotify(CurrentHead.OffsetLookTarget.x);
        offsetLookYSlider.SetValueWithoutNotify(CurrentHead.OffsetLookTarget.y);
        eyeToCameraToggle.SetEnabledWithoutNotify(CurrentHead.EyeToCamera);
        headToCameraToggle.SetEnabledWithoutNotify(CurrentHead.HeadToCamera);
    }

    private void OnFreeLookToggleChanged(object sender, EventArgs e)
    {
        if (CurrentHead is null)
            return;

        CurrentHead.FreeLook = freeLookToggle.Value;
    }

    private void OnEyeXSliderChanged(object sender, EventArgs e)
    {
        if (CurrentHead is null)
            return;

        CurrentHead.OffsetLookTarget = new(offsetLookXSlider.Value, offsetLookYSlider.Value);
    }

    private void OnEyeYSliderChanged(object sender, EventArgs e)
    {
        if (CurrentHead is null)
            return;

        CurrentHead.OffsetLookTarget = new(offsetLookXSlider.Value, offsetLookYSlider.Value);
    }

    private void OnBindEyeToggleChanged(object sender, EventArgs e)
    {
        if (CurrentHead is null)
            return;

        CurrentHead.EyeToCamera = eyeToCameraToggle.Value;

        freeLookToggle.SetEnabledWithoutNotify(CurrentHead.FreeLook);
    }

    private void OnBindHeadToggleChanged(object sender, EventArgs e)
    {
        if (CurrentHead is null)
            return;

        CurrentHead.HeadToCamera = headToCameraToggle.Value;

        freeLookToggle.SetEnabledWithoutNotify(CurrentHead.FreeLook);
    }
}
