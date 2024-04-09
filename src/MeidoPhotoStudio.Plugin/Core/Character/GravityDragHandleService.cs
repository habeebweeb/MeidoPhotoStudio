using System;
using System.Collections.Generic;
using System.Linq;

using MeidoPhotoStudio.Plugin.Framework.UIGizmo;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class GravityDragHandleService
{
    private readonly GravityDragHandleInputService gravityDragHandleInputService;
    private readonly CharacterService characterService;
    private readonly Dictionary<CharacterController, GravityDragHandleSet> dragHandleSets = [];

    public GravityDragHandleService(
        GravityDragHandleInputService gravityDragHandleInputService,
        CharacterService characterService)
    {
        this.gravityDragHandleInputService = gravityDragHandleInputService ?? throw new ArgumentNullException(nameof(gravityDragHandleInputService));
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));

        this.characterService.CalledCharacters += OnCharactersCalled;
        this.characterService.Deactivating += OnDeactivating;
    }

    public GravityDragHandleSet this[CharacterController characterController] =>
        characterController is null
            ? throw new ArgumentNullException(nameof(characterController))
            : dragHandleSets[characterController];

    private void OnDeactivating(object sender, EventArgs e)
    {
        foreach (var dragHandleSet in dragHandleSets.Values)
            DestroyDragHandleSet(dragHandleSet);

        dragHandleSets.Clear();
    }

    private void OnCharactersCalled(object sender, CharacterServiceEventArgs e)
    {
        var oldCharacters = dragHandleSets.Keys.ToArray();

        foreach (var character in oldCharacters.Except(e.LoadedCharacters))
        {
            DestroyDragHandleSet(dragHandleSets[character]);
            character.ProcessingCharacterProps -= OnCharacterProcessing;
            dragHandleSets.Remove(character);
        }

        foreach (var character in e.LoadedCharacters.Except(oldCharacters))
        {
            dragHandleSets[character] = InitializeDragHandleSet(character);
            character.ProcessingCharacterProps += OnCharacterProcessing;
        }
    }

    private void OnCharacterProcessing(object sender, CharacterProcessingEventArgs e)
    {
        var character = sender as CharacterController;

        if (!dragHandleSets.ContainsKey(character))
        {
            character.ProcessingCharacterProps -= OnCharacterProcessing;

            return;
        }

        var mpnStart = (MPN)Enum.Parse(typeof(MPN_TYPE_RANGE), nameof(MPN_TYPE_RANGE.BODY_RELOAD_START)) - 1;
        var mpnEnd = (MPN)Enum.Parse(typeof(MPN_TYPE_RANGE), nameof(MPN_TYPE_RANGE.BODY_RELOAD_END));

        if (!e.ChangingSlots.Any(slot => slot >= mpnStart || slot <= mpnEnd))
            return;

        DestroyDragHandleSet(dragHandleSets[character]);

        character.ProcessedCharacterProps += OnCharacterProcessed;

        void OnCharacterProcessed(object sender, CharacterProcessingEventArgs e)
        {
            dragHandleSets[character] = InitializeDragHandleSet(character);

            character.ProcessedCharacterProps -= OnCharacterProcessed;
        }
    }

    private GravityDragHandleSet InitializeDragHandleSet(CharacterController character)
    {
        if (character.Clothing is null)
            Utility.LogDebug("clothing is null");

        if (character.Clothing?.ClothingGravityController is null)
            Utility.LogDebug("Clothing gravity controller is null");

        var clothingDragHandle = BuildDragHandle(character.Clothing.ClothingGravityController);
        var hairDraghandle = BuildDragHandle(character.Clothing.HairGravityController);

        gravityDragHandleInputService.AddController(clothingDragHandle);
        gravityDragHandleInputService.AddController(hairDraghandle);

        return new()
        {
            ClothingDragHandle = clothingDragHandle,
            HairDragHandle = hairDraghandle,
        };

        GravityDragHandleController BuildDragHandle(GravityController gravityController)
        {
            var gravityControl = gravityController.Transform;

            var dragHandle = new DragHandle.Builder()
            {
                Name = $"[{gravityController.Name}]",
                Target = gravityControl,
                Priority = 10,
                Scale = Vector3.one * 0.12f,
                PositionDelegate = () => gravityControl.position,
            }.Build();

            return new GravityDragHandleController(gravityController, dragHandle)
            {
                Enabled = false,
            };
        }
    }

    private void DestroyDragHandleSet(GravityDragHandleSet dragHandleSet)
    {
        dragHandleSet.HairDragHandle.Destroy();
        dragHandleSet.ClothingDragHandle.Destroy();

        gravityDragHandleInputService.RemoveController(dragHandleSet.HairDragHandle);
        gravityDragHandleInputService.RemoveController(dragHandleSet.ClothingDragHandle);
    }
}
