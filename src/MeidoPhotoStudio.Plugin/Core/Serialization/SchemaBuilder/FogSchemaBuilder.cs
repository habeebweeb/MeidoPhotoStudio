using MeidoPhotoStudio.Plugin.Core.Schema.Effects;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class FogSchemaBuilder : ISchemaBuilder<FogSchema, FogEffectManager>
{
    public FogSchema Build(FogEffectManager fog) =>
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
