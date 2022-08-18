using System;
using System.Collections.Generic;

namespace MeidoPhotoStudio.Plugin;

public class EffectManager : IManager
{
    public const string Header = "EFFECT";
    public const string Footer = "END_EFFECT";

    private readonly Dictionary<Type, IEffectManager> effectManagers = new();

    public T Get<T>()
        where T : IEffectManager =>
        effectManagers.ContainsKey(typeof(T)) ? (T)effectManagers[typeof(T)] : default;

    public T AddManager<T>()
        where T : IEffectManager, new()
    {
        var manager = new T();

        effectManagers[typeof(T)] = manager;
        manager.Activate();

        return manager;
    }

    public void Activate()
    {
        foreach (var effectManager in effectManagers.Values)
            effectManager.Activate();
    }

    public void Deactivate()
    {
        foreach (var effectManager in effectManagers.Values)
            effectManager.Deactivate();
    }

    public void Update()
    {
    }
}
