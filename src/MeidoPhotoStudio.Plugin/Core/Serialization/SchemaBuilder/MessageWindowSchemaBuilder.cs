using MeidoPhotoStudio.Plugin.Core.Message;
using MeidoPhotoStudio.Plugin.Core.Schema.Message;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class MessageWindowSchemaBuilder(MessageWindowManager messageWindowManager)
    : ISceneSchemaAspectBuilder<MessageWindowSchema>
{
    private readonly MessageWindowManager messageWindowManager = messageWindowManager
        ?? throw new ArgumentNullException(nameof(messageWindowManager));

    public MessageWindowSchema Build() =>
        new()
        {
            ShowingMessage = messageWindowManager.ShowingMessage,
            FontSize = messageWindowManager.FontSize,
            Name = messageWindowManager.MessageName,
            MessageBody = messageWindowManager.MessageText,
            Alignment = messageWindowManager.MessageAlignment,
        };
}
