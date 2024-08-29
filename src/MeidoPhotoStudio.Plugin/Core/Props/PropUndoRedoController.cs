using MeidoPhotoStudio.Plugin.Core.UndoRedo;
using MeidoPhotoStudio.Plugin.Framework;

namespace MeidoPhotoStudio.Plugin.Core.Props;

public class PropUndoRedoController(
    int id,
    PropUndoRedoService.PropResolver propResolver,
    PropController propController,
    UndoRedoService undoRedoService)
    : UndoRedoControllerBase(undoRedoService)
{
    private readonly PropController propController = propController ?? throw new ArgumentNullException(nameof(propController));
    private readonly PropUndoRedoService.PropResolver propResolver = propResolver ?? throw new ArgumentNullException(nameof(propResolver));

    private ITransactionalUndoRedo<TransformBackup> transformUndoRedoController;
    private ValueMapUndoRedoController<string, float> shapeKeyUndoRedoController;

    public int ID { get; } = id;

    public ITransactionalUndoRedo<TransformBackup> Transform =>
        transformUndoRedoController ??= MakeCustomTransactionalUndoRedo(
            () => new TransformBackup(propResolver.ResolveProp(ID).GameObject.transform),
            backup => backup.Apply(propResolver.ResolveProp(ID).GameObject.transform));

    public ValueMapUndoRedoController<string, float> ShapeKeys =>
        propController.ShapeKeyController is null
            ? null
            : shapeKeyUndoRedoController ??= new(
                undoRedoService,
                (key, value) => propResolver.ResolveProp(ID).ShapeKeyController[key] = value,
                key => propResolver.ResolveProp(ID).ShapeKeyController[key]);
}
