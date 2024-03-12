using MeidoPhotoStudio.Plugin.Core.Schema.Message;

namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class MessageAspectLoader : ISceneAspectLoader<MessageWindowSchema>
{
    private readonly MessageWindowManager messageWindowManager;

    public MessageAspectLoader(MessageWindowManager messageWindowManager) =>
        this.messageWindowManager = messageWindowManager ?? throw new System.ArgumentNullException(nameof(messageWindowManager));

    public void Load(MessageWindowSchema messageWindowSchema, LoadOptions loadOptions)
    {
        if (!loadOptions.Message)
            return;

        messageWindowManager.CloseMessagePanel();

        messageWindowManager.FontSize = messageWindowSchema.FontSize;

        if (messageWindowSchema.ShowingMessage)
            messageWindowManager.ShowMessage(messageWindowSchema.Name, messageWindowSchema.MessageBody);
    }
}
