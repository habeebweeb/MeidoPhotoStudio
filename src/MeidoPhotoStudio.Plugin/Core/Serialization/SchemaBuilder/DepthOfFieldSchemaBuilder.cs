using MeidoPhotoStudio.Plugin.Core.Schema.Effects;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class DepthOfFieldSchemaBuilder : ISchemaBuilder<DepthOfFieldSchema, DepthOfFieldEffectManager>
{
    public DepthOfFieldSchema Build(DepthOfFieldEffectManager dof) =>
        new()
        {
            Active = dof.Active,
            FocalLength = dof.FocalLength,
            FocalSize = dof.FocalSize,
            Aperture = dof.Aperture,
            MaxBlurSize = dof.MaxBlurSize,
            VisualizeFocus = dof.VisualizeFocus,
        };
}
