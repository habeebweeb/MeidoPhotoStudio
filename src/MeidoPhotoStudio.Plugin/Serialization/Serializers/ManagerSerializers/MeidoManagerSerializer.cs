using System.IO;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class MeidoManagerSerializer : Serializer<MeidoManager>
{
    private const short Version = 1;

    private static Serializer<Meido> MeidoSerializer =>
        Serialization.Get<Meido>();

    public override void Serialize(MeidoManager manager, BinaryWriter writer)
    {
        writer.Write(MeidoManager.Header);
        writer.WriteVersion(Version);

        var meidoList = manager.ActiveMeidoList;

        var meidoCount = meidoList.Count;

        var hairPosition = Vector3.zero;
        var skirtPosition = Vector3.zero;

        var hairMeidoFound = false;
        var skirtMeidoFound = false;

        var globalGravity = manager.GlobalGravity;

        writer.Write(meidoCount);

        foreach (var meido in meidoList)
        {
            MeidoSerializer.Serialize(meido, writer);

            if (!globalGravity || meidoCount <= 0)
                continue;

            // Get gravity and skirt control positions to apply to meidos past the meido count
            if (!hairMeidoFound && meido.HairGravityControl.Valid)
            {
                hairPosition = meido.HairGravityControl.Control.transform.localPosition;
                hairMeidoFound = true;
            }
            else if (!skirtMeidoFound && meido.SkirtGravityControl.Valid)
            {
                skirtPosition = meido.SkirtGravityControl.Control.transform.localPosition;
                skirtMeidoFound = true;
            }
        }

        writer.Write(globalGravity);
        writer.Write(hairPosition);
        writer.Write(skirtPosition);
    }

    public override void Deserialize(MeidoManager manager, BinaryReader reader, SceneMetadata metadata)
    {
        _ = reader.ReadVersion();

        var meidoCount = reader.ReadInt32();

        for (var i = 0; i < meidoCount; i++)
        {
            if (i >= manager.ActiveMeidoList.Count)
            {
                reader.BaseStream.Seek(reader.ReadInt64(), SeekOrigin.Current);

                continue;
            }

            MeidoSerializer.Deserialize(manager.ActiveMeidoList[i], reader, metadata);
        }

        var globalGravity = reader.ReadBoolean();
        var hairPosition = reader.ReadVector3();
        var skirtPosition = reader.ReadVector3();

        Utility.SetFieldValue(manager, "globalGravity", globalGravity);

        if (!globalGravity)
            return;

        foreach (var meido in manager.ActiveMeidoList)
        {
            meido.HairGravityActive = true;
            meido.SkirtGravityActive = true;
            meido.ApplyGravity(hairPosition);
            meido.ApplyGravity(skirtPosition, true);
        }
    }
}
