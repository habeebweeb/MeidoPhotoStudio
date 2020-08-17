using System;
using System.Collections.Generic;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class EffectManager
    {
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
