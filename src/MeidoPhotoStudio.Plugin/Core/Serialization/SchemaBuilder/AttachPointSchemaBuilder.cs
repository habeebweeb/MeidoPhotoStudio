using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Core.Schema.Props;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class AttachPointSchemaBuilder : ISchemaBuilder<AttachPointSchema, AttachPointInfo>
{
    public AttachPointSchema Build(AttachPointInfo attachPointInfo) =>
        new()
        {
            AttachPoint = attachPointInfo.AttachPoint,
            CharacterIndex = attachPointInfo.MaidIndex,
            CharacterID = attachPointInfo.MaidGuid,
        };
}
