using System.ComponentModel;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class GlobalGravityService : INotifyPropertyChanged
{
    private readonly CharacterService characterService;

    private bool enabled;

    public GlobalGravityService(CharacterService characterService)
    {
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));

        this.characterService.CallingCharacters += OnCharactersCalling;
        this.characterService.CalledCharacters += OnCharactersCalled;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public bool Enabled
    {
        get => enabled;
        set
        {
            enabled = value;

            UpdateCharacterGravity();

            RaisePropertyChanged(nameof(Enabled));
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

            RaisePropertyChanged(nameof(HairGravityPosition));
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

            RaisePropertyChanged(nameof(ClothingGravityPosition));
        }
    }

    private void OnCharactersCalling(object sender, CharacterServiceEventArgs e)
    {
        foreach (var clothing in characterService.Select(character => character.Clothing))
        {
            clothing.HairGravityController.Moved -= OnGravityControlMoved;
            clothing.HairGravityController.EnabledChanged -= OnGravityControlEnabledChanged;

            clothing.ClothingGravityController.Moved -= OnGravityControlMoved;
            clothing.ClothingGravityController.EnabledChanged -= OnGravityControlEnabledChanged;
        }
    }

    private void OnCharactersCalled(object sender, CharacterServiceEventArgs e)
    {
        foreach (var clothing in e.LoadedCharacters.Select(character => character.Clothing))
        {
            clothing.HairGravityController.Moved += OnGravityControlMoved;
            clothing.HairGravityController.EnabledChanged += OnGravityControlEnabledChanged;

            clothing.ClothingGravityController.Moved += OnGravityControlMoved;
            clothing.ClothingGravityController.EnabledChanged += OnGravityControlEnabledChanged;
        }
    }

    private void OnGravityControlMoved(object sender, EventArgs e)
    {
        if (!Enabled)
            return;

        var gravityController = (GravityController)sender;

        if (!gravityController.Enabled)
            return;

        foreach (var controller in GetSameGravityControllers(gravityController))
            controller.SetPositionWithoutNotify(gravityController.Position);
    }

    private void OnGravityControlEnabledChanged(object sender, EventArgs e)
    {
        if (!Enabled)
            return;

        var gravityController = (GravityController)sender;

        if (!gravityController.Enabled)
            return;

        var firstEnabledController = GetSameGravityControllers(gravityController)
            .FirstOrDefault(controller => controller.Enabled);

        var position = firstEnabledController?.Position ?? gravityController.Position;

        gravityController.SetPositionWithoutNotify(position);
    }

    private IEnumerable<GravityController> GetSameGravityControllers(GravityController controller)
    {
        var controllers = controller is HairGravityController
            ? characterService.Select(character => character.Clothing.HairGravityController)
            : characterService.Select(character => character.Clothing.ClothingGravityController);

        return controllers.Where(otherController => controller != otherController);
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

    private void RaisePropertyChanged(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        PropertyChanged?.Invoke(this, new(name));
    }
}
