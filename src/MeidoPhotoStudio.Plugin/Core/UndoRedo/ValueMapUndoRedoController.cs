namespace MeidoPhotoStudio.Plugin.Core.UndoRedo;

public class ValueMapUndoRedoController<TKey, TValue>(
    UndoRedoService undoRedoService,
    Action<TKey, TValue> setter,
    Func<TKey, TValue> getter,
    IEqualityComparer<TKey> comparer)
{
    private readonly UndoRedoService undoRedoService = undoRedoService
        ?? throw new ArgumentNullException(nameof(undoRedoService));

    private readonly Action<TKey, TValue> setter = setter ?? throw new ArgumentNullException(nameof(setter));
    private readonly Func<TKey, TValue> getter = getter ?? throw new ArgumentNullException(nameof(getter));
    private readonly Dictionary<TKey, TValue> stateMap = new(comparer ?? throw new ArgumentNullException(nameof(comparer)));

    private TKey changingKey;

    public ValueMapUndoRedoController(UndoRedoService undoRedoService, Action<TKey, TValue> setter, Func<TKey, TValue> getter)
        : this(undoRedoService, setter, getter, EqualityComparer<TKey>.Default)
    {
    }

    public void Set(TKey key, TValue value)
    {
        var oldValue = getter(key);

        setter(key, value);

        var newValue = getter(key);

        if (newValue.Equals(oldValue))
            return;

        undoRedoService.Push(new UndoRedoAction(() => setter(key, oldValue), () => setter(key, newValue)));
    }

    public void StartChange(TKey key)
    {
        changingKey = key;

        var newState = getter(key);

        if (stateMap.TryGetValue(key, out var initialState) && newState.Equals(initialState))
            return;

        stateMap[key] = getter(key);
    }

    public void EndChange(TKey key)
    {
        var undoState = stateMap[changingKey];
        var redoState = getter(key);

        if (undoState.Equals(redoState))
            return;

        undoRedoService.Push(new UndoRedoAction(() => setter(key, undoState), () => setter(key, redoState)));
    }
}
