using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class FloorHeightDragHandleService
{
    private static GameObject floorHeightTargetParent;

    private readonly FloorHeightDragHandleInputHandler floorHeightDragHandleInputHandler;
    private readonly CharacterService characterService;
    private readonly SelectionController<CharacterController> selectionController;
    private readonly TabSelectionController tabSelectionController;
    private readonly Dictionary<CharacterController, FloorHeightDragHandleController> dragHandles = [];

    private bool autoSelect;
    private bool autoSelectTab;
    private Color dragHandleColour;

    public FloorHeightDragHandleService(
        FloorHeightDragHandleInputHandler floorHeightDragHandleInputHandler,
        CharacterService characterService,
        SelectionController<CharacterController> selectionController,
        TabSelectionController tabSelectionController)
    {
        this.floorHeightDragHandleInputHandler = floorHeightDragHandleInputHandler ?? throw new ArgumentNullException(nameof(floorHeightDragHandleInputHandler));
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.selectionController = selectionController ?? throw new ArgumentNullException(nameof(selectionController));
        this.tabSelectionController = tabSelectionController ?? throw new ArgumentNullException(nameof(tabSelectionController));

        this.characterService.PreCalledCharacters += OnCharactersPreCalled;
        this.characterService.Deactivating += OnDeactivating;
    }

    public bool AutoSelect
    {
        get => autoSelect;
        set
        {
            if (value == autoSelect)
                return;

            autoSelect = value;

            foreach (var dragHandle in dragHandles.Values)
                dragHandle.AutoSelect = autoSelect;
        }
    }

    public bool AutoSelectTab
    {
        get => autoSelectTab;
        set
        {
            if (autoSelectTab == value)
                return;

            autoSelectTab = value;

            foreach (var dragHandle in dragHandles.Values)
                dragHandle.AutoSelectTab = autoSelectTab;
        }
    }

    public Color DragHandleColour
    {
        get => dragHandleColour;
        set
        {
            if (dragHandleColour == value)
                return;

            dragHandleColour = value;

            foreach (var dragHandle in dragHandles.Values)
                dragHandle.DragHandleColour = dragHandleColour;
        }
    }

    private static GameObject TargetParent
    {
        get
        {
            if (floorHeightTargetParent)
                return floorHeightTargetParent;

            const string dragHandleTargetParentName = "[MPS Floor Height Target Parent]";

            var foundParent = GameObject.Find(dragHandleTargetParentName);

            return floorHeightTargetParent = foundParent ? foundParent : new(dragHandleTargetParentName);
        }
    }

    public FloorHeightDragHandleController this[CharacterController characterController] =>
        characterController is null
            ? throw new ArgumentNullException(nameof(characterController))
            : dragHandles[characterController];

    internal static void DestroyParent()
    {
        if (!floorHeightTargetParent)
            return;

        Object.Destroy(floorHeightTargetParent);
    }

    private void OnCharactersPreCalled(object sender, CharacterServiceEventArgs e)
    {
        var oldCharacters = dragHandles.Keys.ToArray();

        foreach (var character in oldCharacters.Except(e.LoadedCharacters))
        {
            DestroyDragHandle(dragHandles[character]);
            dragHandles.Remove(character);
        }

        foreach (var character in e.LoadedCharacters.Except(oldCharacters))
        {
            var floorHeightDragHandleController = InitializeDragHandle(character);

            if (floorHeightDragHandleController is null)
                continue;

            dragHandles[character] = floorHeightDragHandleController;
        }

        FloorHeightDragHandleController InitializeDragHandle(CharacterController character)
        {
            if (character.Clothing is not ClothingController clothing)
                return null;

            var transform = character.Transform;
            var target = new GameObject($"[Floor Height Drag Handle Target ({character})]");

            target.transform.SetParent(TargetParent.transform, false);

            var dragHandle = new DragHandle.Builder()
            {
                Name = $"[Floor Height Drag Handle ({character})]",
                Shape = PrimitiveType.Cube,
                Target = target.transform,
                Color = DragHandleColour,
                Scale = new(1f, 0.01f, 1f),
                PositionDelegate = () => new(transform.position.x, clothing.FloorHeight - 0.03f, transform.position.z),
            }.Build();

            var controller = new FloorHeightDragHandleController(dragHandle, character, target, selectionController, tabSelectionController)
            {
                DragHandleEnabled = false,
                AutoSelect = AutoSelect,
                AutoSelectTab = AutoSelectTab,
            };

            floorHeightDragHandleInputHandler.AddController(controller);

            return controller;
        }
    }

    private void DestroyDragHandle(FloorHeightDragHandleController controller)
    {
        controller.Destroy();
        floorHeightDragHandleInputHandler.RemoveController(controller);
    }

    private void OnDeactivating(object sender, EventArgs e)
    {
        foreach (var dragHandle in dragHandles.Values)
            DestroyDragHandle(dragHandle);

        dragHandles.Clear();
    }
}
