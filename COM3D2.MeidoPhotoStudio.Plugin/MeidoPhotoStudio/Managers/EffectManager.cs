using System;
using System.Collections.Generic;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class EffectManager : IManager, ISerializable
    {
        public const string header = "EFFECT";
        public const string footer = "END_EFFECT";
        private readonly Dictionary<Type, IEffectManager> EffectManagers = new Dictionary<Type, IEffectManager>();

        public T Get<T>() where T : IEffectManager
            => EffectManagers.ContainsKey(typeof(T)) ? (T)EffectManagers[typeof(T)] : default;

        public T AddManager<T>() where T : IEffectManager, new()
        {
            T manager = new T();
            EffectManagers[typeof(T)] = manager;
            manager.Activate();
            return manager;
        }

        public void Serialize(System.IO.BinaryWriter binaryWriter)
        {
            binaryWriter.Write(header);
            foreach (IEffectManager effectManager in EffectManagers.Values) effectManager.Serialize(binaryWriter);
            binaryWriter.Write(footer);
        }

        public void Deserialize(System.IO.BinaryReader binaryReader)
        {
            string header;
            while ((header = binaryReader.ReadString()) != footer)
            {
                switch (header)
                {
                    case BloomEffectManager.header:
                        Get<BloomEffectManager>().Deserialize(binaryReader);
                        break;
                    case DepthOfFieldEffectManager.header:
                        Get<DepthOfFieldEffectManager>().Deserialize(binaryReader);
                        break;
                    case VignetteEffectManager.header:
                        Get<VignetteEffectManager>().Deserialize(binaryReader);
                        break;
                    case FogEffectManager.header:
                        Get<FogEffectManager>().Deserialize(binaryReader);
                        break;
                }
            }
        }

        public void Activate()
        {
            foreach (IEffectManager effectManager in EffectManagers.Values) effectManager.Activate();
        }

        public void Deactivate()
        {
            foreach (IEffectManager effectManager in EffectManagers.Values) effectManager.Deactivate();
        }

        public void Update() { }
    }
}
