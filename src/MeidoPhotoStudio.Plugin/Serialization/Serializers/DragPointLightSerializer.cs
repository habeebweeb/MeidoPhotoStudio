using System.IO;

namespace MeidoPhotoStudio.Plugin
{
    public class DragPointLightSerializer : Serializer<DragPointLight>
    {
        private const short version = 1;
        private static Serializer<LightProperty> LightPropertySerializer => Serialization.Get<LightProperty>();

        public override void Serialize(DragPointLight light, BinaryWriter writer)
        {
            writer.WriteVersion(version);

            LightProperty[] lightList = GetLightProperties(light);

            for (var i = 0; i < 3; i++) LightPropertySerializer.Serialize(lightList[i], writer);

            writer.Write(light.MyObject.position);
            writer.Write((int) light.SelectedLightType);
            writer.Write(light.IsColourMode);
            writer.Write(light.IsDisabled);
        }

        public override void Deserialize(DragPointLight light, BinaryReader reader, SceneMetadata metadata)
        {
            _ = reader.ReadVersion();

            LightProperty[] lightList = GetLightProperties(light);
            
            for (var i = 0; i < 3; i++) LightPropertySerializer.Deserialize(lightList[i], reader, metadata);

            light.MyObject.position = reader.ReadVector3();
            light.SetLightType((DragPointLight.MPSLightType) reader.ReadInt32());
            light.IsColourMode = reader.ReadBoolean();
            light.IsDisabled = reader.ReadBoolean();
        }

        private static LightProperty[] GetLightProperties(DragPointLight light)
            => Utility.GetFieldValue<DragPointLight, LightProperty[]>(light, "LightProperties");
    }
}
