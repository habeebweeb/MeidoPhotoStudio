using System.IO;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class SepiaToneEffectManger : IEffectManager
    {
        public const string header = "EFFECT_SEPIA";
        private SepiaToneEffect SepiaTone { get; set; }
        public bool Ready { get; private set; }
        public bool Active { get; private set; }

        public void Activate()
        {
            if (SepiaTone == null)
            {
                Ready = true;
                SepiaTone = GameMain.Instance.MainCamera.GetOrAddComponent<SepiaToneEffect>();

                if (SepiaTone.shader == null) SepiaTone.shader = Shader.Find("Hidden/Sepiatone Effect");
            }
            SetEffectActive(false);
        }

        public void Deactivate() => SetEffectActive(false);

        public void Deserialize(BinaryReader binaryReader) => SetEffectActive(binaryReader.ReadBoolean());

        public void Serialize(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(header);
            binaryWriter.Write(Active);
        }

        public void SetEffectActive(bool active) => SepiaTone.enabled = Active = active;

        public void Reset() { }

        public void Update() { }
    }
}
