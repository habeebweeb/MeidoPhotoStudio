using System;
using System.Linq;

using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Schema.Props;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class PropsSchemaBuilder : ISceneSchemaAspectBuilder<PropsSchema>
{
    private readonly PropService propService;
    private readonly PropDragHandleService propDragHandleService;
    private readonly PropAttachmentService propAttachmentService;
    private readonly ISchemaBuilder<PropControllerSchema, PropController> propControllerSchemaBuilder;
    private readonly ISchemaBuilder<DragHandleSchema, DragHandleControllerBase> dragHandleSchemaBuilder;
    private readonly ISchemaBuilder<AttachPointSchema, AttachPointInfo> propAttachmentSchemaBuilder;

    public PropsSchemaBuilder(
        PropService propService,
        PropDragHandleService propDragHandleService,
        PropAttachmentService propAttachmentService,
        ISchemaBuilder<PropControllerSchema, PropController> propControllerSchemaBuilder,
        ISchemaBuilder<DragHandleSchema, DragHandleControllerBase> dragHandleSchemaBuilder,
        ISchemaBuilder<AttachPointSchema, AttachPointInfo> propAttachmentSchemaBuilder)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.propDragHandleService = propDragHandleService ?? throw new ArgumentNullException(nameof(propDragHandleService));
        this.propAttachmentService = propAttachmentService ?? throw new ArgumentNullException(nameof(propAttachmentService));
        this.propControllerSchemaBuilder = propControllerSchemaBuilder ?? throw new ArgumentNullException(nameof(propControllerSchemaBuilder));
        this.dragHandleSchemaBuilder = dragHandleSchemaBuilder ?? throw new ArgumentNullException(nameof(dragHandleSchemaBuilder));
        this.propAttachmentSchemaBuilder = propAttachmentSchemaBuilder ?? throw new ArgumentNullException(nameof(propAttachmentSchemaBuilder));
    }

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
