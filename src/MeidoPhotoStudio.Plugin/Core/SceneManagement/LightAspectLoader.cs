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

    public void Load(LightsSchema lightsSchema, LoadOptions loadOptions)
    {
        if (!loadOptions.Lights)
            return;

        lightService.RemoveAllLights();

        lightService.AddedLight += ApplyLightSchema;

        for (var i = 0; i < lightsSchema.Lights.Count; i++)
        {
            if (i == 0)
                lightService.AddLight(GameMain.Instance.MainLight.GetComponent<Light>());
            else
                lightService.AddLight();
        }

        lightService.AddedLight -= ApplyLightSchema;

        void ApplyLightSchema(object sender, LightServiceEventArgs e)
        {
            var light = e.LightController;
            var lightSchema = lightsSchema.Lights[e.LightIndex];

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
