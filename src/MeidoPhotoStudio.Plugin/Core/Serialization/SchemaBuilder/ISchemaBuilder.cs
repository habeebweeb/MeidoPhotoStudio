namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public interface ISchemaBuilder<TSchema, TValue>
{
    TSchema Build(TValue value);
}
