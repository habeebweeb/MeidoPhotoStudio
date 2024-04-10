using MeidoPhotoStudio.Plugin.Core.Background;
using MeidoPhotoStudio.Plugin.Core.Lighting;
using MeidoPhotoStudio.Plugin.Core.Schema.Light;

namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class LightAspectLoader(LightRepository lightRepository, BackgroundService backgroundService)
    : ISceneAspectLoader<LightRepositorySchema>
{
    private readonly LightRepository lightRepository = lightRepository
        ?? throw new ArgumentNullException(nameof(lightRepository));

    private readonly BackgroundService backgroundService = backgroundService
        ?? throw new ArgumentNullException(nameof(backgroundService));

    public void Load(LightRepositorySchema lightRepositorySchema, LoadOptions loadOptions)
    {
        if (!loadOptions.Lights)
            return;

        lightRepository.RemoveAllLights();

        lightRepository.AddedLight += ApplyLightSchema;

        for (var i = 0; i < lightRepositorySchema.Lights.Count; i++)
        {
            if (i == 0)
                lightRepository.AddLight(GameMain.Instance.MainLight.GetComponent<Light>());
            else
                lightRepository.AddLight();
        }

        lightRepository.AddedLight -= ApplyLightSchema;

        void ApplyLightSchema(object sender, LightRepositoryEventArgs e)
        {
            var light = e.LightController;
            var lightSchema = lightRepositorySchema.Lights[e.LightIndex];

            light.Position = lightSchema.Position;
            light.Type = lightSchema.Type;
            light.Enabled = lightSchema.Enabled;
            light[LightType.Directional] = MakeLightProperties(lightSchema.DirectionalProperties);
            light[LightType.Spot] = MakeLightProperties(lightSchema.SpotProperties);
            light[LightType.Point] = MakeLightProperties(lightSchema.PointProperties);

            // NOTE: Camera background colour was set through main light in older version
            if (lightSchema.ColourMode)
                backgroundService.BackgroundColour = lightSchema.DirectionalProperties.Colour;

            LightProperties MakeLightProperties(LightPropertiesSchema lightPropertiesSchema) =>
                new()
                {
                    Rotation = lightPropertiesSchema.Rotation,
                    Intensity = lightPropertiesSchema.Intensity,
                    Range = lightPropertiesSchema.Range,
                    SpotAngle = lightPropertiesSchema.SpotAngle,
                    ShadowStrength = lightPropertiesSchema.ShadowStrength,
                    Colour = lightPropertiesSchema.Colour,
                };
        }
    }
}
