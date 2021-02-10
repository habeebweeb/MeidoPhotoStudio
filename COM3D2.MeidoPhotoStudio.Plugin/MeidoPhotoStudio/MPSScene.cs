using System.IO;
using System.Text;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class MPSScene
    {
        public Texture2D Thumbnail { get; }
        public FileInfo FileInfo { get; }
        public bool Environment { get; private set; }
        public int NumberOfMaids { get; private set; }

        private byte[] data;

        public byte[] Data
        {
            get
            {
                if (data == null) Preload();
                return data;
            }
            private set => data = value;
        }

        public MPSScene(string path, Texture2D thumbnail = null)
        {
            FileInfo = new FileInfo(path);

            if (!thumbnail)
            {
                thumbnail = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                thumbnail.LoadImage(File.ReadAllBytes(FileInfo.FullName));
            }

            Thumbnail = thumbnail;
        }

        public void Preload()
        {
            if (data != null) return;

            using var fileStream = FileInfo.OpenRead();
            Utility.SeekPngEnd(fileStream);

            using var memoryStream = new MemoryStream();

            fileStream.CopyTo(memoryStream);
            memoryStream.Position = 0L;

            using var binaryReader = new BinaryReader(memoryStream, Encoding.UTF8);

            var sceneHeader = MeidoPhotoStudio.SceneHeader;
            if (!Utility.BytesEqual(binaryReader.ReadBytes(sceneHeader.Length), sceneHeader))
            {
                Utility.LogWarning($"'{FileInfo.FullName}' is not a MPS Scene");
                return;
            }

            (_, Environment, NumberOfMaids, _) = SceneMetadata.ReadMetadata(binaryReader);

            Data = memoryStream.ToArray();
        }

        public void Destroy()
        {
            if (Thumbnail) Object.DestroyImmediate(Thumbnail);
        }
    }
}
