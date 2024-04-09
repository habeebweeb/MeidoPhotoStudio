using System;
using System.Linq;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class GlobalGravityService
{
    private readonly CharacterService characterService;

    private bool enabled;

    public GlobalGravityService(CharacterService characterService)
    {
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));

        this.characterService.CallingCharacters += OnCharactersCalling;
        this.characterService.CalledCharacters += OnCharactersCalled;
    }

    public bool Enabled
    {
        get => enabled;
        set
        {
            enabled = value;

            UpdateCharacterGravity();
        }
    }

    public Vector3 HairGravityPosition
    {
        get
        {
            if (characterService.Count is 0)
                return Vector3.zero;

            var controller = characterService
                .Select(character => character.Clothing.HairGravityController)
                .FirstOrDefault(controller => controller.Valid);

            return controller?.Position ?? Vector3.zero;
        }

        set
        {
            foreach (var controller in characterService
                .Select(character => character.Clothing.HairGravityController))
                controller.SetPositionWithoutNotify(value);
        }
    }

    public Vector3 ClothingGravityPosition
    {
        get
        {
            if (characterService.Count is 0)
                return Vector3.zero;

            var controller = characterService
                .Select(character => character.Clothing.ClothingGravityController)
                .FirstOrDefault(controller => controller.Valid);

            return controller?.Position ?? Vector3.zero;
        }

        set
        {
            foreach (var controller in characterService
                .Select(character => character.Clothing.ClothingGravityController))
                controller.SetPositionWithoutNotify(value);
        }
    }

    private void OnCharactersCalling(object sender, CharacterServiceEventArgs e)
    {
        foreach (var clothing in characterService.Select(character => character.Clothing))
        {
            clothing.HairGravityController.Moved -= OnHairControlMoved;
            clothing.HairGravityController.EnabledChanged -= OnHairEnabledChanged;

            clothing.ClothingGravityController.Moved -= OnClothingControlMoved;
            clothing.ClothingGravityController.EnabledChanged -= OnClothingEnabledChanged;
        }
    }

    private void OnCharactersCalled(object sender, CharacterServiceEventArgs e)
    {
        foreach (var clothing in e.LoadedCharacters.Select(character => character.Clothing))
        {
            clothing.HairGravityController.Moved += OnHairControlMoved;
            clothing.HairGravityController.EnabledChanged += OnHairEnabledChanged;

            clothing.ClothingGravityController.Moved += OnClothingControlMoved;
            clothing.ClothingGravityController.EnabledChanged += OnClothingEnabledChanged;
        }
    }

    private void OnHairEnabledChanged(object sender, EventArgs e)
    {
        if (!Enabled)
            return;

        var gravityController = (GravityController)sender;

        if (!gravityController.Enabled)
            return;

        var firstEnabledController = characterService
            .Select(character => character.Clothing.HairGravityController)
            .Where(controller => gravityController != controller)
            .FirstOrDefault(controller => controller.Enabled);

        var position = firstEnabledController?.Position ?? gravityController.Position;

        gravityController.SetPositionWithoutNotify(position);
    }

    private void OnHairControlMoved(object sender, EventArgs e)
    {
        if (!Enabled)
            return;

        var gravityController = (GravityController)sender;

        foreach (var controller in characterService
            .Select(character => character.Clothing.HairGravityController)
            .Where(hairController => hairController != gravityController))
            controller.SetPositionWithoutNotify(gravityController.Position);
    }

    private void OnClothingEnabledChanged(object sender, EventArgs e)
    {
        if (!Enabled)
            return;

        var gravityController = (GravityController)sender;

        if (!gravityController.Enabled)
            return;

        var firstEnabledController = characterService
            .Select(character => character.Clothing.ClothingGravityController)
            .Where(controller => gravityController != controller)
            .FirstOrDefault(controller => controller.Enabled);

        var position = firstEnabledController?.Position ?? gravityController.Position;

        gravityController.SetPositionWithoutNotify(position);
    }

    private void OnClothingControlMoved(object sender, EventArgs e)
    {
        if (!Enabled)
            return;

        var gravityController = (GravityController)sender;

        foreach (var controller in characterService
            .Select(character => character.Clothing.ClothingGravityController)
            .Where(hairController => hairController != gravityController))
            controller.SetPositionWithoutNotify(gravityController.Position);
    }

    private void UpdateCharacterGravity()
    {
        if (!Enabled)
            return;

        var clothing = characterService.Select(character => character.Clothing).ToArray();

        var enabledHairController = clothing
            .Select(clothing => clothing.HairGravityController)
            .FirstOrDefault(controller => controller.Enabled);

        var hairPosition = enabledHairController?.Position ?? Vector3.zero;

        var enabledClothingController = clothing
            .Select(clothing => clothing.ClothingGravityController)
            .FirstOrDefault(controller => controller.Enabled);

        var clothingPosition = enabledClothingController?.Position ?? Vector3.zero;

        var controllers = clothing.Select(clothing =>
            (clothing.HairGravityController, clothing.ClothingGravityController));

        foreach (var (hairController, clothingController) in controllers)
        {
            if (enabledHairController != hairController && hairController.Enabled)
                hairController.SetPositionWithoutNotify(hairPosition);

            if (enabledClothingController != clothingController && clothingController.Enabled)
                clothingController.SetPositionWithoutNotify(clothingPosition);
        }
    }
}
