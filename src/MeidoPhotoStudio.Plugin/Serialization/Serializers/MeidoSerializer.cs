using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    public class MeidoSerializer : Serializer<Meido>
    {
        private const short version = 1;
        private const short headVersion = 1;
        private const short bodyVersion = 1;
        private const short clothingVersion = 1;

        private static SimpleSerializer<PoseInfo> PoseInfoSerializer => Serialization.GetSimple<PoseInfo>();

        private static SimpleSerializer<TransformDTO> TransformDtoSerializer => Serialization.GetSimple<TransformDTO>();

        public override void Serialize(Meido meido, BinaryWriter writer)
        {
            var maid = meido.Maid;

            using var memoryStream = new MemoryStream();
            using var tempWriter = new BinaryWriter(memoryStream, Encoding.UTF8);

            tempWriter.WriteVersion(version);

            TransformDtoSerializer.Serialize(new TransformDTO(maid.transform), tempWriter);

            SerializeHead(meido, tempWriter);

            SerializeBody(meido, tempWriter);

            SerializeClothing(meido, tempWriter);

            writer.Write(memoryStream.Length);
            writer.Write(memoryStream.ToArray());
        }

        public override void Deserialize(Meido meido, BinaryReader reader, SceneMetadata metadata)
        {
            var maid = meido.Maid;

            maid.GetAnimation().Stop();
            meido.DetachAllMpnAttach();
            meido.StopBlink();

            reader.ReadInt64(); // data length

            _ = reader.ReadVersion();

            var transformDto = TransformDtoSerializer.Deserialize(reader, metadata);
            var maidTransform = maid.transform;
            maidTransform.position = transformDto.Position;
            maidTransform.rotation = transformDto.Rotation;
            maidTransform.localScale = transformDto.LocalScale;

            meido.IKManager.SetDragPointScale(maidTransform.localScale.x);

            DeserializeHead(meido, reader, metadata);

            DeserializeBody(meido, reader, metadata);

            DeserializeClothing(meido, reader, metadata);
        }

        private static void SerializeHead(Meido meido, BinaryWriter writer)
        {
            var body = meido.Body;

            writer.WriteVersion(headVersion);

            // eye direction
            writer.WriteQuaternion(body.quaDefEyeL * Quaternion.Inverse(meido.DefaultEyeRotL));
            writer.WriteQuaternion(body.quaDefEyeR * Quaternion.Inverse(meido.DefaultEyeRotR));

            // free look
            writer.Write(meido.FreeLook);
            writer.WriteVector3(body.offsetLookTarget);
            writer.WriteVector3(Utility.GetFieldValue<TBody, Vector3>(body, "HeadEulerAngle"));

            // Head/eye to camera
            writer.Write(meido.HeadToCam);
            writer.Write(meido.EyeToCam);

            // face
            Dictionary<string, float> faceDict = meido.SerializeFace();
            writer.Write(faceDict.Count);
            foreach (var (hash, value) in faceDict)
            {
                writer.Write(hash);
                writer.Write(value);
            }
        }

        private static void SerializeBody(Meido meido, BinaryWriter writer)
        {
            writer.WriteVersion(bodyVersion);

            // pose
            var poseBuffer = meido.SerializePose(true);
            writer.Write(poseBuffer.Length);
            writer.Write(poseBuffer);

            PoseInfoSerializer.Serialize(meido.CachedPose, writer);
        }

        private static void SerializeClothing(Meido meido, BinaryWriter writer)
        {
            var maid = meido.Maid;
            var body = meido.Body;

            writer.WriteVersion(clothingVersion);

            // body visible
            writer.Write(body.GetMask(TBody.SlotID.body));

            // clothing
            foreach (var clothingSlot in MaidDressingPane.ClothingSlots)
            {
                var value = true;
                if (clothingSlot == TBody.SlotID.wear)
                {
                    if (MaidDressingPane.WearSlots.Any(slot => body.GetSlotLoaded(slot)))
                    {
                        value = MaidDressingPane.WearSlots.Any(slot => body.GetMask(slot));
                    }
                }
                else if (clothingSlot == TBody.SlotID.megane)
                {
                    var slots = new[] { TBody.SlotID.megane, TBody.SlotID.accHead };
                    if (slots.Any(slot => body.GetSlotLoaded(slot))) { value = slots.Any(slot => body.GetMask(slot)); }
                }
                else if (body.GetSlotLoaded(clothingSlot)) value = body.GetMask(clothingSlot);

                writer.Write(value);
            }

            // zurashi and mekure
            writer.Write(meido.CurlingFront);
            writer.Write(meido.CurlingBack);
            writer.Write(meido.PantsuShift);

            // mpn attach props
            var hasKousokuUpper = body.GetSlotLoaded(TBody.SlotID.kousoku_upper);
            writer.Write(hasKousokuUpper);
            writer.Write(maid.GetProp(MPN.kousoku_upper).strTempFileName);

            var hasKousokuLower = body.GetSlotLoaded(TBody.SlotID.kousoku_lower);
            writer.Write(hasKousokuLower);
            writer.Write(maid.GetProp(MPN.kousoku_lower).strTempFileName);

            // hair/skirt gravity
            writer.Write(meido.HairGravityActive);
            writer.Write(meido.HairGravityControl.Control.transform.localPosition);

            writer.Write(meido.SkirtGravityActive);
            writer.Write(meido.SkirtGravityControl.Control.transform.localPosition);
        }

        private static void DeserializeHead(Meido meido, BinaryReader reader, SceneMetadata metadata)
        {
            var body = meido.Body;

            _ = reader.ReadVersion();

            var mmConverted = metadata.MMConverted;

            var eyeRotationL = reader.ReadQuaternion();
            var eyeRotationR = reader.ReadQuaternion();

            if (!mmConverted)
            {
                eyeRotationL *= meido.DefaultEyeRotL;
                eyeRotationR *= meido.DefaultEyeRotR;
            }

            body.quaDefEyeL = eyeRotationL;
            body.quaDefEyeR = eyeRotationR;

            var freeLook = meido.FreeLook = reader.ReadBoolean();
            var offsetLookTarget = reader.ReadVector3();
            var headEulerAngle = reader.ReadVector3();

            if (freeLook) body.offsetLookTarget = offsetLookTarget;

            if (!metadata.MMConverted)
            {
                Utility.SetFieldValue(body, "HeadEulerAngleG", Vector3.zero);
                Utility.SetFieldValue(body, "HeadEulerAngle", headEulerAngle);
            }

            meido.HeadToCam = reader.ReadBoolean();
            meido.EyeToCam = reader.ReadBoolean();

            var faceBlendCount = reader.ReadInt32();
            for (var i = 0; i < faceBlendCount; i++)
            {
                var hash = reader.ReadString();
                var value = reader.ReadSingle();
                meido.SetFaceBlendValue(hash, value);
            }
        }

        private static void DeserializeBody(Meido meido, BinaryReader reader, SceneMetadata metadata)
        {
            _ = reader.ReadVersion();

            var muneSetting = new KeyValuePair<bool, bool>(true, true);
            if (metadata.MMConverted) meido.IKManager.Deserialize(reader);
            else
            {
                var poseBufferLength = reader.ReadInt32();
                byte[] poseBuffer = reader.ReadBytes(poseBufferLength);
                muneSetting = meido.SetFrameBinary(poseBuffer);
            }

            var poseInfo = PoseInfoSerializer.Deserialize(reader, metadata);
            Utility.SetPropertyValue(meido, nameof(Meido.CachedPose), poseInfo);
            
            meido.SetMune(!muneSetting.Key, true);
            meido.SetMune(!muneSetting.Value);
        }

        private static void DeserializeClothing(Meido meido, BinaryReader reader, SceneMetadata metadata)
        {
            var body = meido.Body;

            _ = reader.ReadVersion();

            meido.SetBodyMask(reader.ReadBoolean());

            foreach (var clothingSlot in MaidDressingPane.ClothingSlots)
            {
                var value = reader.ReadBoolean();
                if (metadata.MMConverted) continue;

                if (clothingSlot == TBody.SlotID.wear)
                {
                    body.SetMask(TBody.SlotID.wear, value);
                    body.SetMask(TBody.SlotID.mizugi, value);
                    body.SetMask(TBody.SlotID.onepiece, value);
                }
                else if (clothingSlot == TBody.SlotID.megane)
                {
                    body.SetMask(TBody.SlotID.megane, value);
                    body.SetMask(TBody.SlotID.accHead, value);
                }
                else if (body.GetSlotLoaded(clothingSlot)) body.SetMask(clothingSlot, value);
            }

            // zurashi and mekure
            var curlingFront = reader.ReadBoolean();
            var curlingBack = reader.ReadBoolean();
            var curlingPantsu = reader.ReadBoolean();

            if (!metadata.MMConverted)
            {
                if (meido.CurlingFront != curlingFront) meido.SetCurling(Meido.Curl.Front, curlingFront);
                if (meido.CurlingBack != curlingBack) meido.SetCurling(Meido.Curl.Back, curlingBack);
                meido.SetCurling(Meido.Curl.Shift, curlingPantsu);
            }

            // MPN attach upper prop
            var hasKousokuUpper = reader.ReadBoolean();
            var upperMenuFile = reader.ReadString();
            if (hasKousokuUpper) meido.SetMpnProp(new MpnAttachProp(MPN.kousoku_upper, upperMenuFile), false);

            // MPN attach lower prop
            var hasKousokuLower = reader.ReadBoolean();
            var lowerMenuFile = reader.ReadString();
            if (hasKousokuLower) meido.SetMpnProp(new MpnAttachProp(MPN.kousoku_lower, lowerMenuFile), false);

            // hair gravity
            var hairGravityActive = reader.ReadBoolean();
            var hairPosition = reader.ReadVector3();
            meido.HairGravityActive = hairGravityActive;
            if (meido.HairGravityActive) meido.ApplyGravity(hairPosition);

            // skirt gravity
            var skirtGravityActive = reader.ReadBoolean();
            var skirtPosition = reader.ReadVector3();
            meido.SkirtGravityActive = skirtGravityActive;
            if (meido.SkirtGravityActive) meido.ApplyGravity(skirtPosition, true);
        }
    }
}
