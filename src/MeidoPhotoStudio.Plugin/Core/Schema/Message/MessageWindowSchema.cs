namespace MeidoPhotoStudio.Plugin.Core.Schema.Message;

public class MessageWindowSchema(short version = MessageWindowSchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public bool ShowingMessage { get; init; }

    public int FontSize { get; init; }

    public string Name { get; init; }

    public string MessageBody { get; init; }

    public NGUIText.Alignment Alignment { get; init; } = NGUIText.Alignment.Left;
}
