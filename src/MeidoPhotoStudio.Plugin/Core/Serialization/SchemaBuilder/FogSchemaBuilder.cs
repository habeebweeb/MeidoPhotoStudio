using MeidoPhotoStudio.Plugin.Core.Effects;
using MeidoPhotoStudio.Plugin.Core.Schema.Effects;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class FogSchemaBuilder : ISchemaBuilder<FogSchema, FogController>
{
    public FogSchema Build(FogController fog) =>
        new()
        {
            Active = fog.Active,
            Distance = fog.Distance,
            Density = fog.Density,
            HeightScale = fog.HeightScale,
            Height = fog.Height,
            FogColour = fog.FogColour,
        };
}
