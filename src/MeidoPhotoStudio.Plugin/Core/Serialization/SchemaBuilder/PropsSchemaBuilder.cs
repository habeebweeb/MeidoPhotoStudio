using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Schema.Props;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class PropsSchemaBuilder(
    PropService propService,
    PropDragHandleService propDragHandleService,
    PropAttachmentService propAttachmentService,
    ISchemaBuilder<PropControllerSchema, PropController> propControllerSchemaBuilder,
    ISchemaBuilder<DragHandleSchema, DragHandleControllerBase> dragHandleSchemaBuilder,
    ISchemaBuilder<AttachPointSchema, AttachPointInfo> propAttachmentSchemaBuilder)
    : ISceneSchemaAspectBuilder<PropsSchema>
{
    private readonly PropService propService = propService
        ?? throw new ArgumentNullException(nameof(propService));

    private readonly PropDragHandleService propDragHandleService = propDragHandleService
        ?? throw new ArgumentNullException(nameof(propDragHandleService));

    private readonly PropAttachmentService propAttachmentService = propAttachmentService
        ?? throw new ArgumentNullException(nameof(propAttachmentService));

    private readonly ISchemaBuilder<PropControllerSchema, PropController> propControllerSchemaBuilder = propControllerSchemaBuilder
        ?? throw new ArgumentNullException(nameof(propControllerSchemaBuilder));

    private readonly ISchemaBuilder<DragHandleSchema, DragHandleControllerBase> dragHandleSchemaBuilder = dragHandleSchemaBuilder
        ?? throw new ArgumentNullException(nameof(dragHandleSchemaBuilder));

    private readonly ISchemaBuilder<AttachPointSchema, AttachPointInfo> propAttachmentSchemaBuilder = propAttachmentSchemaBuilder
        ?? throw new ArgumentNullException(nameof(propAttachmentSchemaBuilder));

    public PropsSchema Build() =>
        new()
        {
            Props = propService.Select(propControllerSchemaBuilder.Build).ToList(),
            DragHandleSettings = propService.Select(propController => propDragHandleService[propController]).Select(dragHandleSchemaBuilder.Build).ToList(),
            PropAttachment = propService.Select(propController =>
            {
                return propAttachmentService.PropIsAttached(propController)
                    ? propAttachmentService[propController]
                    : AttachPointInfo.Empty;
            })
            .Select(propAttachmentSchemaBuilder.Build)
            .ToList(),
        };
}
