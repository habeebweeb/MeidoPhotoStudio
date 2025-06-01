using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class BodyController : IShapeKeyController
{
    private static readonly Dictionary<string, KeyedPropertyChangeEventArgs<string>> BlendValueChangeArgsCache =
        new(StringComparer.Ordinal);

    private readonly CharacterController characterController;

    private Dictionary<string, float> backupShapeKeys;

    public BodyController(CharacterController characterController)
    {
        this.characterController = characterController ?? throw new ArgumentNullException(nameof(characterController));

        this.characterController.ProcessingCharacterProps += OnCharacterPropsProcessing;
        this.characterController.ProcessedCharacterProps += OnCharacterPropsProcessed;

        BackupShapeKeys();
    }

    public event EventHandler<KeyedPropertyChangeEventArgs<string>> ChangedShapeKey;

    public event EventHandler ChangedMultipleShapeKeys;

    public event EventHandler ChangedShapeKeySet;

    public IEnumerable<string> ShapeKeys =>
        Body.BlendDatas.Where(static blendData => blendData is not null).Select(static blendData => blendData.name);

    private TMorph Body =>
        characterController.Maid.body0.goSlot[(int)SlotID.body].morph;

    public float this[string key]
    {
        get => GetBlendValue(key);
        set => SetBodyValue(key, value);
    }

    public bool ContainsShapeKey(string key) =>
        string.IsNullOrEmpty(key)
            ? throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key))
            : Body.hash.ContainsKey(key);

    public Dictionary<string, float> GetShapeKeyData(IEnumerable<string> shapeKeys) =>
        shapeKeys.ToDictionary(
            shapeKey => shapeKey,
            shapeKey => ContainsShapeKey(shapeKey) ? this[shapeKey] : 0f);

    public void ResetAllShapeKeys()
    {
        foreach (var (hash, value) in backupShapeKeys)
        {
            if (!Body.hash.Contains(hash))
                continue;

            var index = (int)Body.hash[hash];
            var valueWillChange = !Mathf.Approximately(value, Body.GetBlendValues(index));

            Body.SetBlendValues(index, value);

            if (valueWillChange)
                OnBlendValueChanged(hash);
        }

        Body.FixBlendValues();

        ChangedMultipleShapeKeys?.Invoke(this, EventArgs.Empty);
    }

    private void OnCharacterPropsProcessing(object sender, CharacterProcessingEventArgs e)
    {
        if (!e.ChangingSlots.Contains(SafeMpn.GetValue(nameof(MPN.body))))
            return;

        ResetAllShapeKeys();
    }

    private void OnCharacterPropsProcessed(object sender, CharacterProcessingEventArgs e)
    {
        if (!e.ChangingSlots.Contains(SafeMpn.GetValue(nameof(MPN.body))))
            return;

        BackupShapeKeys();

        ChangedShapeKeySet?.Invoke(this, EventArgs.Empty);
    }

    private float GetBlendValue(string key) =>
        string.IsNullOrEmpty(key)
            ? throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key))
            : !ContainsShapeKey(key)
                ? 0f
                : Body.GetBlendValues((int)Body.hash[key]);

    private void SetBodyValue(string key, float value, bool notify = true)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));

        if (!ContainsShapeKey(key))
            return;

        Body.SetBlendValues((int)Body.hash[key], value);
        Body.FixBlendValues();

        if (!notify)
            return;

        OnBlendValueChanged(key);
    }

    private void BackupShapeKeys()
    {
        var morph = Body;

        backupShapeKeys = morph.BlendDatas
            .Where(static data => data is not null)
            .Select(static data => data.name)
            .Except(new[] { "arml", "hara", "munel", "munes", "munetare", "regfat", "regmeet" })
            .ToDictionary(static key => key, key => morph.GetBlendValues((int)morph.hash[key]));
    }

    private void OnBlendValueChanged(string key)
    {
        if (!BlendValueChangeArgsCache.TryGetValue(key, out var e))
            e = BlendValueChangeArgsCache[key] = new(key);

        ChangedShapeKey?.Invoke(this, e);
    }
}
