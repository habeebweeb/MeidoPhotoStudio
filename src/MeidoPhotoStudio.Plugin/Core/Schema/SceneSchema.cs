namespace MeidoPhotoStudio.Plugin.Core.Schema;

public class SceneSchema
{
    public const short SchemaVersion = 4;

    public Message.MessageWindowSchema MessageWindow { get; init; }

    public Camera.CameraSchema Camera { get; init; }

    public Light.LightRepositorySchema Lights { get; init; }

    public Effects.EffectsSchema Effects { get; init; }

    public Background.BackgroundSchema Background { get; init; }

    public Props.PropsSchema Props { get; init; }
}
