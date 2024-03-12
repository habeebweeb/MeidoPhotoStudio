namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public interface IPropModelSchema
{
    short Version { get; }

    PropType Type { get; }
}
