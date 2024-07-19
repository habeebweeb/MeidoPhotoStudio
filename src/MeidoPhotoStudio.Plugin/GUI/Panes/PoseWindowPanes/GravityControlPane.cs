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

        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

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

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        hairGravityEnabledToggle.SetEnabledWithoutNotify(CurrentClothing.HairGravityController.Enabled);
        hairGravityDragHandleEnabledToggle.SetEnabledWithoutNotify(CurrentDragHandleSet.HairDragHandle.Enabled);
        clothingGravityEnabledToggle.SetEnabledWithoutNotify(CurrentClothing.ClothingGravityController.Enabled);
        clothingGravityDragHandleEnabledToggle.SetEnabledWithoutNotify(CurrentDragHandleSet.ClothingDragHandle.Enabled);
    }
}
