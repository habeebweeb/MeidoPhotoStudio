using MeidoPhotoStudio.Plugin.Core.Schema.Message;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class MessageWindowSchemaBuilder
{
    private readonly MessageWindowManager messageWindowManager;

    public MessageWindowSchemaBuilder(MessageWindowManager messageWindowManager) =>
        this.messageWindowManager = messageWindowManager ?? throw new System.ArgumentNullException(nameof(messageWindowManager));

    public MessageWindowSchema Build() =>
        new()
        {
            ShowingMessage = messageWindowManager.ShowingMessage,
            FontSize = messageWindowManager.FontSize,
            Name = messageWindowManager.MessageName,
            MessageBody = messageWindowManager.MessageText,
        };
}
