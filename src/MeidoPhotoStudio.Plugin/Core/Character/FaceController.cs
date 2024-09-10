using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Database.Character;
using MeidoPhotoStudio.Plugin.Core.Serialization;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class FaceController : INotifyPropertyChanged
{
    private readonly Dictionary<string, KeyedPropertyChangeEventArgs<string>> blendValueChangeArgsCache =
        new(StringComparer.Ordinal);

    private readonly CharacterController characterController;

    private string backupBlendSetName;
    private float[] backupBlendSetValues;
    private IBlendSetModel blendSet;

    public FaceController(CharacterController characterController)
    {
        this.characterController = characterController ?? throw new ArgumentNullException(nameof(characterController));

        BackupBlendSet();
    }

    public event EventHandler<KeyedPropertyChangeEventArgs<string>> BlendValueChanged;

    public event PropertyChangedEventHandler PropertyChanged;

    public IBlendSetModel BlendSet
    {
        get => blendSet;
        private set
        {
            blendSet = value;

            RaisePropertyChanged(nameof(BlendSet));
        }
    }

    public IEnumerable<string> ExpressionKeys =>
        Face.BlendDatas.Where(blendData => blendData is not null).Select(blendData => blendData.name);

    public bool Blink
    {
        get => !Maid.MabatakiUpdateStop;
        set
        {
            if (value == Blink)
                return;

            Maid.MabatakiUpdateStop = !value;
            Maid.body0.Face.morph.EyeMabataki = 0f;
            Maid.MabatakiVal = 0f;

            RaisePropertyChanged(nameof(Blink));
        }
    }

    private Maid Maid =>
        characterController.Maid;

    private TMorph Face =>
        Maid.body0.Face.morph;

    public float this[string key]
    {
        get
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));

            if (!ContainsExpressionKey(key))
                return 0f;

            var index = (int)Face.hash[Face.GP01FbFaceHashKey(key)];

            return Face.dicBlendSet[Maid.ActiveFace][index];
        }

        set =>
            SetBlendValue(key, value);
    }

    public void ApplyBlendSet(IBlendSetModel blendSet)
    {
        _ = blendSet ?? throw new ArgumentNullException(nameof(blendSet));

        try
        {
            if (blendSet.Custom)
                ApplyCustomBlendSet(blendSet);
            else
                ApplyGameBlendSet(blendSet);
        }
        catch
        {
            Utility.LogError($"Could not load blendset: {blendSet.BlendSetName}");

            return;
        }

        BlendSet = blendSet;

        void ApplyGameBlendSet(IBlendSetModel blendSet)
        {
            ApplyBackupBlendSet();

            Maid.FaceAnime(blendSet.BlendSetName, 0f);

            BackupBlendSet();
        }

        void ApplyCustomBlendSet(IBlendSetModel blendSet)
        {
            using var fileStream = File.OpenRead(blendSet.BlendSetName);

            var facialExpressionSet = new BlendSetSerializer().Deserialize(fileStream);

            ApplyBackupBlendSet();

            foreach (var (key, value) in facialExpressionSet.Where(kvp => ContainsExpressionKey(kvp.Key)))
                SetBlendValue(key, Mathf.Clamp(value, 0f, 1f), false);
        }
    }

    public bool ContainsExpressionKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));

        var gp01FbFaceHashKey = Face.GP01FbFaceHashKey(key);

        return Face.hash.ContainsKey(gp01FbFaceHashKey);
    }

    public FacialExpressionSet GetFaceData(IEnumerable<string> facialExpressionKeys) =>
        new(facialExpressionKeys
            .ToDictionary(
                expressionKey => expressionKey,
                expressionKey => ContainsExpressionKey(expressionKey) ? this[expressionKey] : 0f));

    private void SetBlendValue(string key, float value, bool notify = true)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));

        if (!ContainsExpressionKey(key))
            return;

        var index = (int)Face.hash[Face.GP01FbFaceHashKey(key)];

        if (value == Face.dicBlendSet[Maid.ActiveFace][index])
            return;

        Face.dicBlendSet[Maid.ActiveFace][index] = value;

        if (key is "nosefook")
            Maid.boNoseFook = Convert.ToBoolean(value);

        Face.SetBlendValues(index, value);
        Face.FixBlendValues();
        Face.FixBlendValues_Face();

        if (notify)
            OnBlendValueChanged(key);
    }

    private void BackupBlendSet()
    {
        backupBlendSetName = Maid.ActiveFace;

        var blendSet = Face.dicBlendSet[Maid.ActiveFace];

        backupBlendSetValues ??= new float[blendSet.Length];

        blendSet.CopyTo(backupBlendSetValues, 0);
    }

    private void ApplyBackupBlendSet()
    {
        if (!string.Equals(backupBlendSetName, Maid.ActiveFace))
            return;

        var blendSet = Face.dicBlendSet[Maid.ActiveFace];

        backupBlendSetValues.CopyTo(blendSet, 0);
    }

    private void OnBlendValueChanged(string key)
    {
        if (!blendValueChangeArgsCache.TryGetValue(key, out var e))
            e = blendValueChangeArgsCache[key] = new(key);

        BlendValueChanged?.Invoke(this, e);
    }

    private void RaisePropertyChanged(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        PropertyChanged?.Invoke(this, new(name));
    }
}
