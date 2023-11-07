namespace MeidoPhotoStudio.Plugin.Core.Schema;

public class SceneSchema
{
    public const short SchemaVersion = 3;

    public Message.MessageWindowSchema MessageWindow { get; init; }

    public Camera.CameraSchema Camera { get; init; }

    public Light.LightRepositorySchema Lights { get; init; }

    public Effects.EffectsSchema Effects { get; init; }

    public Background.BackgroundSchema Background { get; init; }
}
