using MeidoPhotoStudio.Database.Background;
using MeidoPhotoStudio.Plugin.Core.Schema.Background;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class BackgroundModelSchemaBuilder : ISchemaBuilder<BackgroundModelSchema, BackgroundModel>
{
    public BackgroundModelSchema Build(BackgroundModel background) =>
        new()
        {
            ID = background.ID,
            Category = background.Category,
            AssetName = background.AssetName,
            Name = background.Name,
        };
}
