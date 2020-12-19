﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class EnvironmentManagerSerializer : Serializer<EnvironmentManager>
    {
        private const short version = 1;

        private static SimpleSerializer<TransformDTO> TransformDtoSerializer => Serialization.GetSimple<TransformDTO>();

        public override void Serialize(EnvironmentManager manager, BinaryWriter writer)
        {
            writer.Write(EnvironmentManager.header);
            writer.WriteVersion(version);

            writer.Write(manager.CurrentBgAsset);

            TransformDtoSerializer.Serialize(new TransformDTO(GetBgTransform(manager)), writer);
        }

        public override void Deserialize(EnvironmentManager manager, BinaryReader reader, SceneMetadata metadata)
        {
            _ = reader.ReadVersion();

            var bg = GetBgTransform(manager);

            var bgAsset = reader.ReadString();

            var transformDto = TransformDtoSerializer.Deserialize(reader, metadata);

            var creativeBg = Utility.IsGuidString(bgAsset);

            List<string> bgList = creativeBg
                ? Constants.MyRoomCustomBGList.ConvertAll(kvp => kvp.Key)
                : Constants.BGList;

            var assetIndex = bgList.FindIndex(
                asset => asset.Equals(bgAsset, StringComparison.InvariantCultureIgnoreCase)
            );

            var validBg = assetIndex >= 0;

            if (validBg) bgAsset = bgList[assetIndex];
            else
            {
                Utility.LogWarning($"Could not load BG '{bgAsset}'");
                creativeBg = false;
                bgAsset = EnvironmentManager.defaultBg;
            }

            manager.ChangeBackground(bgAsset, creativeBg);

            if (!validBg) return;

            bg.position = transformDto.Position;
            bg.rotation = transformDto.Rotation;
            bg.localScale = transformDto.LocalScale;
        }

        private static Transform GetBgTransform(EnvironmentManager manager)
            => Utility.GetFieldValue<EnvironmentManager, Transform>(manager, "bg");
    }
}
