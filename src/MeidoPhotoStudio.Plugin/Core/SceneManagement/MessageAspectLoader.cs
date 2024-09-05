using MeidoPhotoStudio.Plugin.Core.Schema.Message;

namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class MessageAspectLoader(MessageWindowManager messageWindowManager) : ISceneAspectLoader<MessageWindowSchema>
{
    private readonly MessageWindowManager messageWindowManager = messageWindowManager
        ?? throw new ArgumentNullException(nameof(messageWindowManager));

    public void Load(MessageWindowSchema messageWindowSchema, LoadOptions loadOptions)
    {
        if (!loadOptions.Message)
            return;

        if (messageWindowSchema is null)
            return;

        messageWindowManager.CloseMessagePanel();

        messageWindowManager.FontSize = messageWindowSchema.FontSize;

        messageWindowManager.MessageAlignment = messageWindowSchema.Alignment;

        if (messageWindowSchema.ShowingMessage)
            messageWindowManager.ShowMessage(messageWindowSchema.Name, messageWindowSchema.MessageBody);
    }
}
