using System;
using System.Text.RegularExpressions;

using MeidoPhotoStudio.Plugin.Core.Background;
using MeidoPhotoStudio.Plugin.Core.Camera;
using MeidoPhotoStudio.Plugin.Core.Lighting;
using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Schema.Background;
using MeidoPhotoStudio.Plugin.Core.Schema.Camera;
using MeidoPhotoStudio.Plugin.Core.Schema.Effects;
using MeidoPhotoStudio.Plugin.Core.Schema.Light;
using MeidoPhotoStudio.Plugin.Core.Schema.Message;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class SceneLoader
{
    private readonly MessageWindowManager messageWindowManager;
    private readonly CameraSaveSlotController cameraSaveSlotController;
    private readonly LightRepository lightRepository;
    private readonly EffectManager effectManager;
    private readonly BackgroundService backgroundService;

    public SceneLoader(
        MessageWindowManager messageWindowManager,
        CameraSaveSlotController cameraSaveSlotController,
        LightRepository lightRepository,
        EffectManager effectManager,
        BackgroundService backgroundService)
    {
        this.messageWindowManager = messageWindowManager ?? throw new ArgumentNullException(nameof(messageWindowManager));
        this.cameraSaveSlotController = cameraSaveSlotController ?? throw new ArgumentNullException(nameof(cameraSaveSlotController));
        this.lightRepository = lightRepository ?? throw new ArgumentNullException(nameof(lightRepository));
        this.effectManager = effectManager ?? throw new ArgumentNullException(nameof(effectManager));
        this.backgroundService = backgroundService ?? throw new ArgumentNullException(nameof(backgroundService));
    }

    public void LoadScene(SceneSchema sceneSchema, LoadOptions loadOptions)
    {
        if (sceneSchema is null)
            throw new ArgumentNullException(nameof(sceneSchema));

        if (loadOptions.Message)
            ApplyMessageWindow(sceneSchema.MessageWindow);

        if (loadOptions.Camera)
            ApplyCamera(sceneSchema.Camera);

        if (loadOptions.Lights)
            ApplyLights(sceneSchema.Lights);

        if (loadOptions.Effects)
            ApplyEffects(sceneSchema.Effects);

        if (loadOptions.Background)
            ApplyBackground(sceneSchema.Background);
    }

    private void ApplyBackground(BackgroundSchema backgroundSchema)
    {
        if (backgroundSchema.Version is 1)
        {
            if (IsGuidString(backgroundSchema.BackgroundName))
                GameMain.Instance.BgMgr.ChangeBgMyRoom(backgroundSchema.BackgroundName);
            else
                GameMain.Instance.BgMgr.ChangeBg(backgroundSchema.BackgroundName);
        }
        else if (backgroundSchema.Version >= 2)
        {
            var modelSchema = backgroundSchema.Background;

            backgroundService.ChangeBackground(new(modelSchema.Category, modelSchema.AssetName, modelSchema.Name));
        }

        if (backgroundService.BackgroundTransform)
        {
            backgroundService.BackgroundTransform.SetPositionAndRotation(
                backgroundSchema.Transform.Position, backgroundSchema.Transform.Rotation);
            backgroundService.BackgroundTransform.localScale = backgroundSchema.Transform.LocalScale;
        }

        if (backgroundSchema.Version >= 2)
            backgroundService.BackgroundColour = backgroundSchema.Colour;

        static bool IsGuidString(string guid)
        {
            var guidRegEx =
                new Regex(@"^[a-f0-9]{8}(\-[a-f0-9]{4}){3}\-[a-f0-9]{12}$", RegexOptions.IgnoreCase);

            return !string.IsNullOrEmpty(guid) && guid.Length is 36 && guidRegEx.IsMatch(guid);
        }
    }

    private void ApplyEffects(EffectsSchema effectsSchema)
    {
        ApplyBloom(effectsSchema.Bloom);
        ApplyDepthOfField(effectsSchema.DepthOfField);
        ApplyFog(effectsSchema.Fog);
        ApplyVignette(effectsSchema.Vignette);
        ApplySepiaTone(effectsSchema.SepiaTone);
        ApplyBlur(effectsSchema.Blur);

        void ApplyBloom(BloomSchema bloomSchema)
        {
            var bloom = effectManager.Get<BloomEffectManager>();

            bloom.SetEffectActive(bloomSchema.Active);
            bloom.BloomValue = bloomSchema.BloomValue;
            bloom.BlurIterations = bloomSchema.BlurIterations;
            bloom.BloomThresholdColour = bloomSchema.BloomThresholdColour;
            bloom.BloomHDR = bloomSchema.BloomHDR;
        }

        void ApplyDepthOfField(DepthOfFieldSchema depthOfFieldSchema)
        {
            var dof = effectManager.Get<DepthOfFieldEffectManager>();

            dof.SetEffectActive(depthOfFieldSchema.Active);
            dof.FocalLength = depthOfFieldSchema.FocalLength;
            dof.FocalSize = depthOfFieldSchema.FocalSize;
            dof.Aperture = depthOfFieldSchema.Aperture;
            dof.MaxBlurSize = depthOfFieldSchema.MaxBlurSize;
            dof.VisualizeFocus = depthOfFieldSchema.VisualizeFocus;
        }

        void ApplyFog(FogSchema fogSchema)
        {
            var fog = effectManager.Get<FogEffectManager>();

            fog.SetEffectActive(fogSchema.Active);
            fog.Distance = fogSchema.Distance;
            fog.Density = fogSchema.Density;
            fog.HeightScale = fogSchema.HeightScale;
            fog.Height = fogSchema.Height;
            fog.FogColour = fogSchema.FogColour;
        }

        void ApplyVignette(VignetteSchema vignetteSchema)
        {
            var vignette = effectManager.Get<VignetteEffectManager>();

            vignette.SetEffectActive(vignetteSchema.Active);
            vignette.Intensity = vignetteSchema.Intensity;
            vignette.Blur = vignetteSchema.Blur;
            vignette.BlurSpread = vignetteSchema.BlurSpread;
            vignette.ChromaticAberration = vignetteSchema.ChromaticAberration;
        }

        void ApplySepiaTone(SepiaToneSchema sepiaToneSchema)
        {
            var sepiaTone = effectManager.Get<SepiaToneEffectManager>();

            sepiaTone.SetEffectActive(sepiaToneSchema.Active);
        }

        void ApplyBlur(BlurSchema blurSchema)
        {
            var blur = effectManager.Get<BlurEffectManager>();

            blur.SetEffectActive(blurSchema.Active);
            blur.BlurSize = blurSchema.BlurSize;
        }
    }

    private void ApplyLights(LightRepositorySchema lightRepositorySchema)
    {
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

    private void ApplyCamera(CameraSchema cameraSchema)
    {
        cameraSaveSlotController.CurrentCameraSlot = cameraSchema.CurrentCameraSlot;

        for (var i = 0; i < cameraSaveSlotController.SaveSlotCount; i++)
        {
            if (i >= cameraSchema.CameraInfo.Count)
                break;

            var cameraInfoSchema = cameraSchema.CameraInfo[i];

            cameraSaveSlotController[i] = new(
                cameraInfoSchema.TargetPosition,
                cameraInfoSchema.Rotation,
                cameraInfoSchema.Distance,
                cameraInfoSchema.FOV);
        }
    }

    private void ApplyMessageWindow(MessageWindowSchema messageWindowSchema)
    {
        messageWindowManager.CloseMessagePanel();

        messageWindowManager.FontSize = messageWindowSchema.FontSize;

        if (messageWindowSchema.ShowingMessage)
            messageWindowManager.ShowMessage(messageWindowSchema.Name, messageWindowSchema.MessageBody);
    }
}
