using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Character;

namespace MeidoPhotoStudio.Plugin;

public class GravityControlPane : BasePane
{
    private readonly GravityDragHandleService gravityDragHandleService;
    private readonly GlobalGravityService globalGravityService;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly PaneHeader paneHeader;
    private readonly Toggle hairGravityEnabledToggle;
    private readonly Toggle hairGravityDragHandleEnabledToggle;
    private readonly Toggle clothingGravityEnabledToggle;
    private readonly Toggle clothingGravityDragHandleEnabledToggle;
    private readonly Toggle globalGravityEnabledToggle;

    public GravityControlPane(
        GravityDragHandleService gravityDragHandleService,
        GlobalGravityService globalGravityService,
        SelectionController<CharacterController> characterSelectionController)
    {
        this.gravityDragHandleService = gravityDragHandleService ?? throw new ArgumentNullException(nameof(gravityDragHandleService));
        this.globalGravityService = globalGravityService ?? throw new ArgumentNullException(nameof(globalGravityService));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.characterSelectionController.Selecting += OnCharacterSelectionChanging;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;
        this.globalGravityService.PropertyChanged += OnGlobalGravityPropertyChanged;

        paneHeader = new(Translation.Get("gravityControlPane", "header"), true);
        hairGravityEnabledToggle = new(Translation.Get("gravityControlPane", "hairToggle"));
        hairGravityEnabledToggle.ControlEvent += OnHairGravityEnabledChanged;

        hairGravityDragHandleEnabledToggle = new(Translation.Get("gravityControlPane", "hairDragHandleToggle"));
        hairGravityDragHandleEnabledToggle.ControlEvent += OnHairGravityDragHandleEnabledChanged;

        clothingGravityEnabledToggle = new(Translation.Get("gravityControlPane", "clothingToggle"));
        clothingGravityEnabledToggle.ControlEvent += OnClothingGravityEnabledChanged;

        clothingGravityDragHandleEnabledToggle = new(Translation.Get("gravityControlPane", "clothingDragHandleToggle"));
        clothingGravityDragHandleEnabledToggle.ControlEvent += OnClothingGravityDragHandleEnabledChanged;

        globalGravityEnabledToggle = new(Translation.Get("gravityControlPane", "globalToggle"));
        globalGravityEnabledToggle.ControlEvent += OnGlobalGravityEnabledToggleChanged;
    }

    private ClothingController CurrentClothing =>
        characterSelectionController.Current?.Clothing;

    private GravityDragHandleSet CurrentDragHandleSet =>
        characterSelectionController.Current is null
            ? null
            : gravityDragHandleService[characterSelectionController.Current];

    public override void Draw()
    {
        var enabled = characterSelectionController.Current is not null;

        GUI.enabled = enabled;

        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        GUILayout.BeginHorizontal();

        var hairGravityValid = CurrentClothing?.HairGravityController.Valid ?? false;

        GUI.enabled = enabled && hairGravityValid;
        hairGravityEnabledToggle.Draw();

        GUI.enabled = enabled && hairGravityValid && hairGravityEnabledToggle.Value;
        hairGravityDragHandleEnabledToggle.Draw();

        GUILayout.EndHorizontal();

        MpsGui.BlackLine();

        GUILayout.BeginHorizontal();

        var clothingGravityValid = CurrentClothing?.ClothingGravityController.Valid ?? false;

        GUI.enabled = enabled && clothingGravityValid;
        clothingGravityEnabledToggle.Draw();

        GUI.enabled = enabled && clothingGravityValid && clothingGravityEnabledToggle.Value;
        clothingGravityDragHandleEnabledToggle.Draw();

        GUILayout.EndHorizontal();

        MpsGui.BlackLine();

        GUI.enabled = enabled;
        globalGravityEnabledToggle.Draw();
    }

    protected override void ReloadTranslation()
    {
        paneHeader.Label = Translation.Get("gravityControlPane", "header");
        hairGravityEnabledToggle.Label = Translation.Get("gravityControlPane", "hairToggle");
        hairGravityDragHandleEnabledToggle.Label = Translation.Get("gravityControlPane", "hairDragHandleToggle");
        clothingGravityEnabledToggle.Label = Translation.Get("gravityControlPane", "clothingToggle");
        clothingGravityDragHandleEnabledToggle.Label = Translation.Get("gravityControlPane", "clothingDragHandleToggle");
        globalGravityEnabledToggle.Label = Translation.Get("gravityControlPane", "globalToggle");
    }

    private void OnHairGravityEnabledChanged(object sender, EventArgs e)
    {
        if (CurrentClothing is null)
            return;

        CurrentClothing.HairGravityController.Enabled = hairGravityEnabledToggle.Value;
        CurrentDragHandleSet.HairDragHandle.Enabled = hairGravityEnabledToggle.Value;
        hairGravityDragHandleEnabledToggle.SetEnabledWithoutNotify(hairGravityEnabledToggle.Value);
    }

    private void OnHairGravityDragHandleEnabledChanged(object sender, EventArgs e)
    {
        if (CurrentClothing is null)
            return;

        if (!CurrentClothing.HairGravityController.Valid)
            return;

        CurrentDragHandleSet.HairDragHandle.Enabled = hairGravityDragHandleEnabledToggle.Value;
    }

    private void OnClothingGravityEnabledChanged(object sender, EventArgs e)
    {
        if (CurrentClothing is null)
            return;

        CurrentClothing.ClothingGravityController.Enabled = clothingGravityEnabledToggle.Value;
        CurrentDragHandleSet.ClothingDragHandle.Enabled = clothingGravityEnabledToggle.Value;
        clothingGravityDragHandleEnabledToggle.SetEnabledWithoutNotify(clothingGravityEnabledToggle.Value);
    }

    private void OnClothingGravityDragHandleEnabledChanged(object sender, EventArgs e)
    {
        if (CurrentClothing is null)
            return;

        if (!CurrentClothing.ClothingGravityController.Valid)
            return;

        CurrentDragHandleSet.ClothingDragHandle.Enabled = clothingGravityDragHandleEnabledToggle.Value;
    }

    private void OnGlobalGravityEnabledToggleChanged(object sender, EventArgs e)
    {
        if (CurrentClothing is null)
            return;

        globalGravityService.Enabled = globalGravityEnabledToggle.Value;
    }

    private void OnCharacterSelectionChanging(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        var clothing = e.Selected.Clothing;

        clothing.HairGravityController.PropertyChanged -= OnGravityPropertyChanged;
        clothing.ClothingGravityController.PropertyChanged -= OnGravityPropertyChanged;

        var dragHandles = gravityDragHandleService[e.Selected];

        dragHandles.HairDragHandle.PropertyChanged -= OnHairDragHandlePropertyChanged;
        dragHandles.ClothingDragHandle.PropertyChanged -= OnClothingDragHandlePropertyChanged;
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        var clothing = e.Selected.Clothing;

        clothing.HairGravityController.PropertyChanged += OnGravityPropertyChanged;
        clothing.ClothingGravityController.PropertyChanged += OnGravityPropertyChanged;

        var dragHandles = gravityDragHandleService[e.Selected];

        dragHandles.HairDragHandle.PropertyChanged -= OnHairDragHandlePropertyChanged;
        dragHandles.ClothingDragHandle.PropertyChanged -= OnClothingDragHandlePropertyChanged;

        hairGravityEnabledToggle.SetEnabledWithoutNotify(CurrentClothing.HairGravityController.Enabled);
        hairGravityDragHandleEnabledToggle.SetEnabledWithoutNotify(CurrentDragHandleSet.HairDragHandle.Enabled);
        clothingGravityEnabledToggle.SetEnabledWithoutNotify(CurrentClothing.ClothingGravityController.Enabled);
        clothingGravityDragHandleEnabledToggle.SetEnabledWithoutNotify(CurrentDragHandleSet.ClothingDragHandle.Enabled);
    }

    private void OnGravityPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not nameof(GravityController.Enabled))
            return;

        if (sender is HairGravityController hairController)
        {
            hairGravityEnabledToggle.SetEnabledWithoutNotify(hairController.Enabled);
        }
        else if (sender is ClothingGravityController clothingController)
        {
            clothingGravityEnabledToggle.SetEnabledWithoutNotify(clothingController.Enabled);
        }
    }

    private void OnHairDragHandlePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not nameof(GravityDragHandleController.Enabled))
            return;

        var controller = (GravityDragHandleController)sender;

        hairGravityDragHandleEnabledToggle.SetEnabledWithoutNotify(controller.Enabled);
    }

    private void OnClothingDragHandlePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not nameof(GravityDragHandleController.Enabled))
            return;

        var controller = (GravityDragHandleController)sender;

        clothingGravityDragHandleEnabledToggle.SetEnabledWithoutNotify(controller.Enabled);
    }

    private void OnGlobalGravityPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not nameof(GlobalGravityService.Enabled))
            return;

        var service = (GlobalGravityService)sender;

        globalGravityEnabledToggle.SetEnabledWithoutNotify(service.Enabled);
    }
}
