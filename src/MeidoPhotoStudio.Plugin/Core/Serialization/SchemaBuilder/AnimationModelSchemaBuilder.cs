using MeidoPhotoStudio.Plugin.Core.Database.Character;
using MeidoPhotoStudio.Plugin.Core.Schema.Character;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class AnimationModelSchemaBuilder
    : ISchemaBuilder<IAnimationModelSchema, IAnimationModel>,
    ISchemaBuilder<GameAnimationSchema, GameAnimationModel>,
    ISchemaBuilder<CustomAnimationSchema, CustomAnimationModel>
{
    public IAnimationModelSchema Build(IAnimationModel value)
    {
        _ = value ?? throw new ArgumentNullException(nameof(value));

        return value switch
        {
            CustomAnimationModel customAnimation => Build(customAnimation),
            GameAnimationModel gameAnimation => Build(gameAnimation),
            _ => throw new NotImplementedException($"'{value.GetType()}' is not implemented"),
        };
    }

    public CustomAnimationSchema Build(CustomAnimationModel value)
    {
        _ = value ?? throw new ArgumentNullException(nameof(value));

        return new()
        {
            ID = value.ID,
        };
    }

    public GameAnimationSchema Build(GameAnimationModel value)
    {
        _ = value ?? throw new ArgumentNullException(nameof(value));

        return new()
        {
            ID = value.ID,
        };
    }
}
