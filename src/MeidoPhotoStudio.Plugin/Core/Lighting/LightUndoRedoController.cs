using MeidoPhotoStudio.Plugin.Core.UndoRedo;
using MeidoPhotoStudio.Plugin.Framework;

namespace MeidoPhotoStudio.Plugin.Core.Lighting;

public class LightUndoRedoController(
    int id,
    LightUndoRedoService.LightResolver lightResolver,
    LightController lightController,
    UndoRedoService undoRedoService)
    : UndoRedoControllerBase(undoRedoService)
{
    private readonly LightController lightController = lightController ?? throw new ArgumentNullException(nameof(lightController));
    private readonly LightUndoRedoService.LightResolver lightResolver = lightResolver ?? throw new ArgumentNullException(nameof(lightResolver));

    private ITransactionalUndoRedo<TransformBackup> transformUndoRedoController;
    private ISettableTransactionalUndoRedo<LightType> lightTypeController;
    private ISettableTransactionalUndoRedo<float> intensityController;
    private ISettableTransactionalUndoRedo<float> rangeController;
    private ISettableTransactionalUndoRedo<float> spotAngleController;
    private ISettableTransactionalUndoRedo<float> shadowStrengthController;
    private ISettableTransactionalUndoRedo<Color> colourController;

    public int ID { get; } = id;

    public ISettableTransactionalUndoRedo<LightType> Type =>
        lightTypeController ??= MakeTransactionalUndoRedo<LightController, LightType>(lightController, nameof(LightController.Type));

    public ISettableTransactionalUndoRedo<float> Intensity =>
        intensityController ??= MakeTransactionalUndoRedo<LightController, float>(lightController, nameof(LightController.Intensity));

    public ISettableTransactionalUndoRedo<float> Range =>
        rangeController ??= MakeTransactionalUndoRedo<LightController, float>(lightController, nameof(LightController.Range));

    public ISettableTransactionalUndoRedo<float> SpotAngle =>
        spotAngleController ??= MakeTransactionalUndoRedo<LightController, float>(lightController, nameof(LightController.SpotAngle));

    public ISettableTransactionalUndoRedo<float> ShadowStrength =>
        shadowStrengthController ??= MakeTransactionalUndoRedo<LightController, float>(lightController, nameof(LightController.ShadowStrength));

    public ISettableTransactionalUndoRedo<Color> Colour =>
        colourController ??= MakeTransactionalUndoRedo<LightController, Color>(lightController, nameof(LightController.Colour));

    public ITransactionalUndoRedo<TransformBackup> Transform =>
        transformUndoRedoController ??= MakeCustomTransactionalUndoRedo(
            () => new TransformBackup(lightResolver.ResolveLight(ID).Light.transform),
            backup => backup.Apply(lightResolver.ResolveLight(ID).Light.transform));
}
