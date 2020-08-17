using System;
using System.Collections.Generic;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class EffectManager : IManager
    {
        public const string header = "EFFECT";
        public const string footer = "END_EFFECT";
        private Dictionary<Type, IEffectManager> EffectManagers = new Dictionary<Type, IEffectManager>();
        private BloomEffectManager bloomEffectManager;

        public EffectManager()
        {
            // Not going to add more effects because SceneCapture does it better anyway
            bloomEffectManager = AddManager<BloomEffectManager>();
            AddManager<DepthOfFieldEffectManager>();
            AddManager<VignetteEffectManager>();
            AddManager<FogEffectManager>();
        }

        public T Get<T>() where T : IEffectManager
        {
            if (EffectManagers.ContainsKey(typeof(T))) return (T)EffectManagers[typeof(T)];
            else return default(T);
        }

        private T AddManager<T>() where T : IEffectManager, new()
        {
            T manager = new T();
            EffectManagers[typeof(T)] = manager;
            return manager;
        }

        public void Serialize(System.IO.BinaryWriter binaryWriter)
        {
            binaryWriter.Write(header);
            foreach (IEffectManager effectManager in EffectManagers.Values)
            {
                effectManager.Serialize(binaryWriter);
            }
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
            foreach (IEffectManager effectManager in EffectManagers.Values)
            {
                effectManager.Activate();
            }
        }

        public void Deactivate()
        {
            foreach (IEffectManager effectManager in EffectManagers.Values)
            {
                effectManager.Deactivate();
            }
        }

        public void Update()
        {
            // Bloom is the only effect that needs to update because I'm dumb/lazy
            bloomEffectManager.Update();
        }
    }
}
