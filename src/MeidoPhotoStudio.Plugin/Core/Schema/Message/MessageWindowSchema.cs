namespace MeidoPhotoStudio.Plugin.Core.Schema.Message;

public class MessageWindowSchema
{
    public const short SchemaVersion = 1;

    public MessageWindowSchema(short version = SchemaVersion) =>
        Version = version;

    public short Version { get; }

    public bool ShowingMessage { get; init; }

    public int FontSize { get; init; }

    public string Name { get; init; }

    public string MessageBody { get; init; }
}
