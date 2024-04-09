using MeidoPhotoStudio.Database.Character;
using MeidoPhotoStudio.Plugin.Core.Schema.Character;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class BlendSetModelSchemaBuilder
    : ISchemaBuilder<IBlendSetModelSchema, IBlendSetModel>,
    ISchemaBuilder<GameBlendSetSchema, GameBlendSetModel>,
    ISchemaBuilder<CustomBlendSetSchema, CustomBlendSetModel>
{
    public IBlendSetModelSchema Build(IBlendSetModel value)
    {
        _ = value ?? throw new ArgumentNullException(nameof(value));

        return value switch
        {
            GameBlendSetModel gameBlendSet => Build(gameBlendSet),
            CustomBlendSetModel customBlendSet => Build(customBlendSet),
            _ => throw new NotImplementedException($"'{value.GetType()}' is not implemented"),
        };
    }

    public GameBlendSetSchema Build(GameBlendSetModel value)
    {
        _ = value ?? throw new ArgumentNullException(nameof(value));

        return new()
        {
            ID = value.ID,
        };
    }

    public CustomBlendSetSchema Build(CustomBlendSetModel value)
    {
        _ = value ?? throw new ArgumentNullException(nameof(value));

        return new()
        {
            ID = value.ID,
        };
    }
}
