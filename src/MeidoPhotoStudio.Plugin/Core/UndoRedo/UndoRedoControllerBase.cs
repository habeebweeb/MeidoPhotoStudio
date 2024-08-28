using System.Linq.Expressions;

using HarmonyLib;

namespace MeidoPhotoStudio.Plugin.Core.UndoRedo;

public abstract class UndoRedoControllerBase(UndoRedoService undoRedoService)
{
    protected readonly UndoRedoService undoRedoService = undoRedoService ?? throw new ArgumentNullException(nameof(undoRedoService));

    private static readonly Dictionary<string, (Delegate Getter, Delegate Setter)> PropertyDelegateCache = [];
    private static readonly Dictionary<string, Delegate> ValueUndoRedoSetterCache = [];
    private static readonly Dictionary<string, Delegate> BackupUndoRedoSetterCache = [];
    private static readonly Dictionary<string, Delegate> BackupInstantiatorCache = [];

    protected Action<TObject, TValue> MakeUndoRedoSetter<TObject, TValue>(string propertyName)
        where TObject : class
        where TValue : struct
    {
        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentException($"'{nameof(propertyName)}' cannot be null or empty.", nameof(propertyName));

        var setterKey = $"{typeof(TObject).FullName}.{propertyName}";

        if (ValueUndoRedoSetterCache.TryGetValue(setterKey, out var setter))
            return (Action<TObject, TValue>)setter;

        var (getterDelegate, setterDelegate) = CreatePropertyDelegates<TObject, TValue>(propertyName);

        setter = Setter;

        ValueUndoRedoSetterCache[setterKey] = setter;

        return (Action<TObject, TValue>)setter;

        void Setter(TObject obj, TValue value)
        {
            var oldValue = getterDelegate(obj);

            setterDelegate(obj, value);

            var newValue = getterDelegate(obj);

            if (newValue.Equals(oldValue))
                return;

            undoRedoService.Push(new UndoRedoAction(() => setterDelegate(obj, oldValue), () => setterDelegate(obj, newValue)));
        }
    }

    protected Action<TObject, TValue> MakeUndoRedoSetter<TObject, TValue, TBackup>(string propertyName)
        where TObject : class
        where TBackup : struct, IValueBackup<TObject>
        where TValue : struct
    {
        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentException($"'{nameof(propertyName)}' cannot be null or empty.", nameof(propertyName));

        var backupSetterKey = $"{typeof(TObject).FullName}.{propertyName}";

        if (BackupUndoRedoSetterCache.TryGetValue(backupSetterKey, out var setter))
            return (Action<TObject, TValue>)setter;

        var (_, valueSetter) = CreatePropertyDelegates<TObject, TValue>(propertyName);
        var backupGetter = CreateBackupInstantiator<TObject, TBackup>();

        setter = Setter;

        BackupUndoRedoSetterCache[backupSetterKey] = setter;

        return (Action<TObject, TValue>)setter;

        void Setter(TObject obj, TValue newValue)
        {
            var oldBackup = backupGetter(obj);

            valueSetter(obj, newValue);

            var newBackup = backupGetter(obj);

            if (newBackup.Equals(oldBackup))
                return;

            undoRedoService.Push(new UndoRedoAction(() => oldBackup.Apply(obj), () => newBackup.Apply(obj)));
        }
    }

    protected ISettableTransactionalUndoRedo<TBackup> MakeCustomTransactionalUndoRedo<TBackup>(Func<TBackup> getter, Action<TBackup> setter)
        where TBackup : struct =>
        new CustomTransactionalUndoRedo<TBackup>(undoRedoService, getter, setter);

    protected ITransactionalUndoRedo<TBackup> MakeSimpleTransactionalUndoRedo<TObject, TBackup>(TObject controller)
        where TObject : class
        where TBackup : struct, IValueBackup<TObject>
    {
        var backupInstantiator = CreateBackupInstantiator<TObject, TBackup>();

        return new SimpleTransactionalUndoRedo<TObject, TBackup>(controller, undoRedoService, backupInstantiator);
    }

    protected ISettableTransactionalUndoRedo<TValue> MakeTransactionalUndoRedo<TObject, TValue>(
        TObject controller, string propertyName)
        where TObject : class
        where TValue : struct
    {
        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentException($"'{nameof(propertyName)}' cannot be null or empty.", nameof(propertyName));

        var (getter, setter) = CreatePropertyDelegates<TObject, TValue>(propertyName);

        return new TransactionalUndoRedo<TObject, TValue>(controller, undoRedoService, getter, setter);
    }

    protected ISettableTransactionalUndoRedo<TValue> MakeTransactionalUndoRedo<TObject, TValue, TBackup>(TObject controller, string propertyName)
        where TObject : class
        where TBackup : struct, IValueBackup<TObject>
        where TValue : struct
    {
        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentException($"'{nameof(propertyName)}' cannot be null or empty.", nameof(propertyName));

        var (_, setter) = CreatePropertyDelegates<TObject, TValue>(propertyName);
        var backupInstantiator = CreateBackupInstantiator<TObject, TBackup>();

        return new TransactionalUndoRedo<TObject, TValue, TBackup>(controller, undoRedoService, backupInstantiator, setter);
    }

    private static (Func<TObject, TValue> Getter, Action<TObject, TValue> Setter) CreatePropertyDelegates<TObject, TValue>(string propertyName)
        where TObject : class
        where TValue : struct
    {
        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentException($"'{nameof(propertyName)}' cannot be null or empty.", nameof(propertyName));

        var propertyKey = $"{typeof(TObject).FullName}.{propertyName}";

        if (PropertyDelegateCache.TryGetValue(propertyKey, out var delegates))
            return ((Func<TObject, TValue>)delegates.Getter,
                (Action<TObject, TValue>)delegates.Setter);

        var propertyInfo = AccessTools.Property(typeof(TObject), propertyName)
            ?? throw new ArgumentException($"'{nameof(propertyName)}' is not a property");

        var setterMethod = propertyInfo.GetSetMethod(true);
        var getterMethod = propertyInfo.GetGetMethod(true);

        var setterDelegate = (Action<TObject, TValue>)Delegate.CreateDelegate(typeof(Action<TObject, TValue>), null, setterMethod);
        var getterDelegate = (Func<TObject, TValue>)Delegate.CreateDelegate(typeof(Func<TObject, TValue>), null, getterMethod);

        delegates = (getterDelegate, setterDelegate);

        PropertyDelegateCache[propertyKey] = delegates;

        return ((Func<TObject, TValue>)delegates.Getter,
            (Action<TObject, TValue>)delegates.Setter);
    }

    private static Func<TObject, TBackup> CreateBackupInstantiator<TObject, TBackup>()
        where TObject : class
        where TBackup : struct, IValueBackup<TObject>
    {
        var instantiatorKey = $"{typeof(TObject).FullName}+{typeof(TBackup).FullName}";

        if (BackupInstantiatorCache.TryGetValue(instantiatorKey, out var backupCreator))
            return (Func<TObject, TBackup>)backupCreator;

        var constructorInfo = typeof(TBackup).GetConstructor([typeof(TObject)])
            ?? throw new InvalidOperationException($"Type {typeof(TBackup).Name} does not have a constructor with a parameter of type {typeof(TObject).Name}");

        var parameterExpression = Expression.Parameter(typeof(TObject), "obj");
        var newExpression = Expression.New(constructorInfo, parameterExpression);
        var lambdaExpression = Expression.Lambda<Func<TObject, TBackup>>(newExpression, parameterExpression);

        backupCreator = lambdaExpression.Compile();

        BackupInstantiatorCache[instantiatorKey] = backupCreator;

        return (Func<TObject, TBackup>)backupCreator;
    }

    private class TransactionalUndoRedo<TObj, TValue>(
        TObj controller,
        UndoRedoService undoRedoService,
        Func<TObj, TValue> getter,
        Action<TObj, TValue> setter)
        : ISettableTransactionalUndoRedo<TValue>
        where TObj : class
        where TValue : struct
    {
        private bool changePending;
        private TValue startingValue;

        public void Set(TValue value)
        {
            var oldValue = getter(controller);

            setter(controller, value);

            var newValue = getter(controller);

            if (newValue.Equals(oldValue))
                return;

            undoRedoService.Push(new UndoRedoAction(() => setter(controller, oldValue), () => setter(controller, newValue)));
        }

        public void StartChange()
        {
            var newValue = getter(controller);

            changePending = true;

            if (startingValue.Equals(newValue))
                return;

            startingValue = newValue;
        }

        public void EndChange()
        {
            if (!changePending)
                return;

            var undoState = startingValue;
            var redoState = getter(controller);

            if (undoState.Equals(redoState))
                return;

            undoRedoService.Push(new UndoRedoAction(() => setter(controller, undoState), () => setter(controller, redoState)));
        }

        public void Cancel()
        {
            changePending = false;
            startingValue = default;
        }
    }

    private class TransactionalUndoRedo<TObj, TValue, TBackup>(
        TObj controller,
        UndoRedoService undoRedoService,
        Func<TObj, TBackup> backupGetter,
        Action<TObj, TValue> setter)
        : ISettableTransactionalUndoRedo<TValue>
        where TBackup : struct, IValueBackup<TObj>
        where TValue : struct
    {
        private TBackup startingState;
        private bool changePending;

        public void Set(TValue value)
        {
            var oldState = backupGetter(controller);

            setter(controller, value);

            var newState = backupGetter(controller);

            if (oldState.Equals(newState))
                return;

            undoRedoService.Push(new UndoRedoAction(() => oldState.Apply(controller), () => newState.Apply(controller)));
        }

        public void StartChange()
        {
            var newState = backupGetter(controller);

            changePending = true;

            if (newState.Equals(startingState))
                return;

            startingState = newState;
        }

        public void EndChange()
        {
            if (!changePending)
                return;

            var undoState = startingState;
            var redoState = backupGetter(controller);

            changePending = false;

            if (undoState.Equals(redoState))
                return;

            undoRedoService.Push(new UndoRedoAction(() => undoState.Apply(controller), () => redoState.Apply(controller)));
        }

        public void Cancel()
        {
            changePending = false;
            startingState = default;
        }
    }

    private class SimpleTransactionalUndoRedo<TObj, TBackup>(
        TObj controller,
        UndoRedoService undoRedoService,
        Func<TObj, TBackup> getter)
        : ITransactionalUndoRedo<TBackup>
        where TBackup : struct, IValueBackup<TObj>
    {
        private bool changePending;
        private IValueBackup<TObj> startingState;

        public void StartChange()
        {
            var newState = getter(controller);

            changePending = true;

            if (newState.Equals(startingState))
                return;

            startingState = newState;
        }

        public void EndChange()
        {
            if (!changePending)
                return;

            var undoState = startingState;
            var redoState = getter(controller);

            changePending = false;

            if (undoState.Equals(redoState))
                return;

            undoRedoService.Push(new UndoRedoAction(() => undoState.Apply(controller), () => redoState.Apply(controller)));
        }

        public void Cancel()
        {
            changePending = false;
            startingState = default;
        }
    }

    private class CustomTransactionalUndoRedo<TBackup>(
        UndoRedoService undoRedoService,
        Func<TBackup> getter,
        Action<TBackup> setter)
        : ISettableTransactionalUndoRedo<TBackup>
        where TBackup : struct
    {
        private TBackup startingState;
        private bool changePending;

        public void Set(TBackup value)
        {
            var oldState = getter();

            setter(value);

            var newState = getter();

            if (newState.Equals(oldState))
                return;

            undoRedoService.Push(new UndoRedoAction(() => setter(oldState), () => setter(newState)));
        }

        public void StartChange()
        {
            var newState = getter();

            changePending = true;

            if (newState.Equals(startingState))
                return;

            startingState = newState;
        }

        public void EndChange()
        {
            if (!changePending)
                return;

            var undoState = startingState;
            var redoState = getter();

            changePending = false;

            if (undoState.Equals(redoState))
                return;

            undoRedoService.Push(new UndoRedoAction(() => setter(undoState), () => setter(redoState)));
        }

        public void Cancel()
        {
            changePending = false;
            startingState = default;
        }
    }
}
