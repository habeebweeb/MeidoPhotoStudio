namespace MeidoPhotoStudio.Plugin;

public class EffectManager : IManager, IEnumerable<IEffectManager>
{
    public const string Header = "EFFECT";
    public const string Footer = "END_EFFECT";

    private readonly Dictionary<Type, IEffectManager> effectManagers = new();

    public IEffectManager this[Type type]
    {
        get => effectManagers[type];
        set => effectManagers[type] = value;
    }

    public T Get<T>()
        where T : IEffectManager =>
        effectManagers.ContainsKey(typeof(T)) ? (T)effectManagers[typeof(T)] : default;

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

    public void Add<T>(T effectManager)
        where T : IEffectManager
    {
        if (effectManager is null)
            throw new ArgumentNullException(nameof(effectManager));

        effectManagers[effectManager.GetType()] = effectManager;
    }

    public IEnumerator<IEffectManager> GetEnumerator() =>
        effectManagers.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
