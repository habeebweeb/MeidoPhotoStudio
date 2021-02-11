using System.IO;

namespace MeidoPhotoStudio.Plugin
{
    public class MessageWindowManagerSerializer : Serializer<MessageWindowManager>
    {
        private const short version = 1;

        public override void Serialize(MessageWindowManager manager, BinaryWriter writer)
        {
            writer.Write(MessageWindowManager.header);
            writer.WriteVersion(version);

            writer.Write(manager.ShowingMessage);
            writer.Write(manager.FontSize);
            writer.Write(manager.MessageName);
            writer.Write(manager.MessageText);
        }

        public override void Deserialize(MessageWindowManager manager, BinaryReader reader, SceneMetadata metadata)
        {
            manager.CloseMessagePanel();

            _ = reader.ReadVersion();

            var showingMessage = reader.ReadBoolean();
            manager.FontSize = reader.ReadInt32();
            var messageName = reader.ReadString();
            var messageText = reader.ReadString();
            if (showingMessage) manager.ShowMessage(messageName, messageText);
        }
    }
}
