using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class CustomFloorHeightPane : BasePane
{
    private readonly FloorHeightDragHandleService floorHeightDragHandleService;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly Toggle customFloorHeightToggle;
    private readonly RepeatButton decreaseFloorHeightButton;
    private readonly RepeatButton increaseFloorHeightButton;
    private readonly NumericalTextField floorHeightTextfield;
    private readonly Button resetFloorHeightButton;
    private readonly Toggle floorHeightDragHandleToggle;

    public CustomFloorHeightPane(
        Translation translation,
        FloorHeightDragHandleService floorHeightDragHandleService,
        SelectionController<CharacterController> characterSelectionController)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.floorHeightDragHandleService = floorHeightDragHandleService ?? throw new ArgumentNullException(nameof(floorHeightDragHandleService));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.characterSelectionController.Selecting += OnCharacterSelectionChanging;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        customFloorHeightToggle = new(
            new LocalizableGUIContent(translation, "customFloorHeightPane", "enabledToggle"), false);

        customFloorHeightToggle.ControlEvent += OnCustomFloorHeightToggleChanged;

        decreaseFloorHeightButton = new(Symbols.Minus, 3f);
        decreaseFloorHeightButton.ControlEvent += OnDecreaseFloorHeightButtonPushed;

        increaseFloorHeightButton = new(Symbols.Plus, 3f);
        increaseFloorHeightButton.ControlEvent += OnIncreaseFloorHeightButtonPushed;

        floorHeightTextfield = new(0f);
        floorHeightTextfield.ControlEvent += OnFloorHeightChanged;

        resetFloorHeightButton = new("|");
        resetFloorHeightButton.ControlEvent += OnResetFloorHeightButtonPushed;

        floorHeightDragHandleToggle = new(
            new LocalizableGUIContent(translation, "customFloorHeightPane", "dragHandleEnabledToggle"), false);

        floorHeightDragHandleToggle.ControlEvent += OnFloorHeightDragHandleToggleChanged;
    }

    private CharacterController CurrentCharacter =>
        characterSelectionController.Current;

    private FloorHeightDragHandleController CurrentDragHandle =>
        characterSelectionController.Current is null
             ? null
             : floorHeightDragHandleService[CurrentCharacter];

    public override void Draw()
    {
        var enabled = Parent.Enabled && characterSelectionController.Current is not null;

        GUI.enabled = enabled;

        GUILayout.BeginHorizontal();

        customFloorHeightToggle.Draw();

        GUI.enabled = enabled && customFloorHeightToggle.Value;

        var buttonSize = GUILayout.Width(UIUtility.Scaled(25));

        decreaseFloorHeightButton.Draw(Symbols.IconButtonStyle, buttonSize);
        increaseFloorHeightButton.Draw(Symbols.IconButtonStyle, buttonSize);

        floorHeightTextfield.Draw(GUILayout.Width(UIUtility.Scaled(65)));

        resetFloorHeightButton.Draw(GUILayout.ExpandWidth(false));

        GUILayout.EndHorizontal();

        floorHeightDragHandleToggle.Draw();
    }

    private void OnCharacterSelectionChanging(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        var clothing = e.Selected.Clothing;

        clothing.PropertyChanged -= OnClothingPropertyChanged;
        floorHeightDragHandleService[e.Selected].PropertyChanged -= OnDragHandlePropertyChanged;
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        var clothing = e.Selected.Clothing;

        clothing.PropertyChanged += OnClothingPropertyChanged;

        var floorHeightDragHandleController = floorHeightDragHandleService[e.Selected];

        floorHeightDragHandleController.PropertyChanged += OnDragHandlePropertyChanged;

        customFloorHeightToggle.SetEnabledWithoutNotify(clothing.CustomFloorHeight);
        floorHeightTextfield.SetValueWithoutNotify(clothing.FloorHeight);

        floorHeightDragHandleToggle.SetEnabledWithoutNotify(floorHeightDragHandleController.DragHandleEnabled);
    }

    private void OnCustomFloorHeightToggleChanged(object sender, EventArgs e)
    {
        if (CurrentCharacter is null)
            return;

        CurrentCharacter.Clothing.CustomFloorHeight = customFloorHeightToggle.Value;

        if (!CurrentCharacter.Clothing.CustomFloorHeight)
        {
            CurrentDragHandle.DragHandleEnabled = false;
            floorHeightDragHandleToggle.SetEnabledWithoutNotify(false);
        }
    }

    private void OnIncreaseFloorHeightButtonPushed(object sender, EventArgs e)
    {
        if (CurrentCharacter is null)
            return;

        if (!CurrentCharacter.Clothing.CustomFloorHeight)
            return;

        CurrentCharacter.Clothing.FloorHeight += 0.01f;
    }

    private void OnDecreaseFloorHeightButtonPushed(object sender, EventArgs e)
    {
        if (CurrentCharacter is null)
            return;

        if (!CurrentCharacter.Clothing.CustomFloorHeight)
            return;

        CurrentCharacter.Clothing.FloorHeight -= 0.01f;
    }

    private void OnFloorHeightChanged(object sender, EventArgs e)
    {
        if (CurrentCharacter is null)
            return;

        CurrentCharacter.Clothing.FloorHeight = floorHeightTextfield.Value;
    }

    private void OnResetFloorHeightButtonPushed(object sender, EventArgs e)
    {
        if (CurrentCharacter is null)
            return;

        CurrentCharacter.Clothing.FloorHeight = 0f;
    }

    private void OnFloorHeightDragHandleToggleChanged(object sender, EventArgs e)
    {
        if (CurrentCharacter is not { Clothing.CustomFloorHeight: true })
            return;

        CurrentDragHandle.DragHandleEnabled = floorHeightDragHandleToggle.Value;
    }

    private void OnClothingPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var clothingController = (ClothingController)sender;

        if (e.PropertyName is nameof(ClothingController.CustomFloorHeight))
            customFloorHeightToggle.SetEnabledWithoutNotify(clothingController.CustomFloorHeight);
        else if (e.PropertyName is nameof(ClothingController.FloorHeight))
            floorHeightTextfield.SetValueWithoutNotify(clothingController.FloorHeight);
    }

    private void OnDragHandlePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not nameof(FloorHeightDragHandleController.DragHandleEnabled))
            return;

        var controller = (FloorHeightDragHandleController)sender;

        floorHeightDragHandleToggle.SetEnabledWithoutNotify(controller.DragHandleEnabled);
    }
}
