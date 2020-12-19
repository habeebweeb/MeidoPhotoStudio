﻿using System.IO;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class LightPropertySerializer : Serializer<LightProperty>
    {
        private const short version = 1;

        public override void Serialize(LightProperty prop, BinaryWriter writer)
        {
            writer.WriteVersion(version);

            writer.Write(prop.Rotation);
            writer.Write(prop.Intensity);
            writer.Write(prop.Range);
            writer.Write(prop.SpotAngle);
            writer.Write(prop.ShadowStrength);
            writer.Write(prop.LightColour);
        }

        public override void Deserialize(LightProperty prop, BinaryReader reader, SceneMetadata metadata)
        {
            _ = reader.ReadVersion();

            prop.Rotation = reader.ReadQuaternion();
            prop.Intensity = reader.ReadSingle();
            prop.Range = reader.ReadSingle();
            prop.SpotAngle = reader.ReadSingle();
            prop.ShadowStrength = reader.ReadSingle();
            prop.LightColour = reader.ReadColour();
        }
    }
}
