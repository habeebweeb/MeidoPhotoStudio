using MeidoPhotoStudio.Plugin.Core.Background;
using MeidoPhotoStudio.Plugin.Core.Lighting;
using MeidoPhotoStudio.Plugin.Core.Schema.Light;

namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class LightAspectLoader(LightService lightService, BackgroundService backgroundService)
    : ISceneAspectLoader<LightsSchema>
{
    private readonly LightService lightService = lightService
        ?? throw new ArgumentNullException(nameof(lightService));

    private readonly BackgroundService backgroundService = backgroundService
        ?? throw new ArgumentNullException(nameof(backgroundService));

    public void Load(LightsSchema lightsSchema, ILoadOptions loadOptions)
    {
        if (!loadOptions["lights"].Enabled)
            return;

        if (lightsSchema is null)
            return;

        lightService.RemoveAllLights();

        lightService.AddedLight += OnLightAdded;

        for (var i = 1; i < lightsSchema.Lights.Count; i++)
            lightService.AddLight();

        lightService.AddedLight -= OnLightAdded;

        ApplyLightSchema(lightService[0], lightsSchema.Lights[0]);

        void OnLightAdded(object sender, LightServiceEventArgs e)
        {
            var light = e.LightController;
            var lightSchema = lightsSchema.Lights[e.LightIndex];

            ApplyLightSchema(light, lightSchema);
        }

        void ApplyLightSchema(LightController light, LightSchema lightSchema)
        {
            light.Position = lightSchema.Position;
            light.Type = lightSchema.Type;
            light.Enabled = lightSchema.Enabled;
            light[LightType.Directional] = MakeLightProperties(lightSchema.DirectionalProperties);
            light[LightType.Spot] = MakeLightProperties(lightSchema.SpotProperties);
            light[LightType.Point] = MakeLightProperties(lightSchema.PointProperties);

            // NOTE: Camera background colour was set through main light in older version
            if (lightSchema.ColourMode)
            {
                backgroundService.BackgroundColour = lightSchema.DirectionalProperties.Colour;
                backgroundService.BackgroundVisible = false;
            }

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
