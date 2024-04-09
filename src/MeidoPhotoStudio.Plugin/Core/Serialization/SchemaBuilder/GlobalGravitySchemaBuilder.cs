using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Schema.Character;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class GlobalGravitySchemaBuilder : ISchemaBuilder<GlobalGravitySchema, GlobalGravityService>
{
    public GlobalGravitySchema Build(GlobalGravityService globalGravityService) =>
        new()
        {
            Enabled = globalGravityService.Enabled,
            HairGravityPosition = globalGravityService.HairGravityPosition,
            ClothingGravityPosition = globalGravityService.ClothingGravityPosition,
        };
}
