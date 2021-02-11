using System;
using System.Collections.Generic;

namespace MeidoPhotoStudio.Plugin
{
    public class EffectManager : IManager
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
