using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Database.Character;
using MeidoPhotoStudio.Plugin.Core.Serialization;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class FaceController : INotifyPropertyChanged, IShapeKeyController
{
    private static readonly Dictionary<string, KeyedPropertyChangeEventArgs<string>> BlendValueChangeArgsCache =
        new(StringComparer.Ordinal);

    private readonly CharacterController characterController;

    private string backupBlendSetName;
    private float[] backupBlendSetValues;
    private IBlendSetModel blendSet;

    public FaceController(CharacterController characterController)
    {
        this.characterController = characterController ?? throw new ArgumentNullException(nameof(characterController));

        this.characterController.ProcessedCharacterProps += OnCharacterPropsProcessed;

        BackupBlendSet();
    }

    public event EventHandler<KeyedPropertyChangeEventArgs<string>> ChangedShapeKey;

    public event EventHandler ChangedShapeKeySet;

    public event EventHandler ChangedMultipleShapeKeys;

    public event PropertyChangedEventHandler PropertyChanged;

    public IBlendSetModel BlendSet
    {
        get => blendSet;
        private set
        {
            blendSet = value;

            ChangedMultipleShapeKeys?.Invoke(this, EventArgs.Empty);

            RaisePropertyChanged(nameof(BlendSet));
        }
    }

    public IEnumerable<string> ShapeKeys =>
        Face.BlendDatas.Where(static blendData => blendData is not null).Select(static blendData => blendData.name);

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

            if (!ContainsShapeKey(key))
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
            Plugin.Logger.LogError($"Could not load blendset: {blendSet.BlendSetName}");

            return;
        }

        BlendSet = blendSet;

        void ApplyGameBlendSet(IBlendSetModel blendSet)
        {
            ApplyBackupBlendSet();

            Maid.FaceAnime(blendSet.BlendSetName, 0f);

            BackupBlendSet();

            ExplicitApply();
        }

        void ApplyCustomBlendSet(IBlendSetModel blendSet)
        {
            using var fileStream = File.OpenRead(blendSet.BlendSetName);

            var facialExpressionSet = new BlendSetSerializer().Deserialize(fileStream);

            ApplyBackupBlendSet();

            foreach (var (key, value) in facialExpressionSet.Where(kvp => ContainsShapeKey(kvp.Key)))
                SetBlendValue(key, value, false, false);

            ExplicitApply();
        }
    }

    public bool ContainsShapeKey(string key)
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
                expressionKey => ContainsShapeKey(expressionKey) ? this[expressionKey] : 0f));

    private void OnCharacterPropsProcessed(object sender, CharacterProcessingEventArgs e)
    {
        if (!e.ChangingSlots.Contains(SafeMpn.head))
            return;

        BackupBlendSet();

        if (BlendSet is not null)
            ApplyBlendSet(BlendSet);

        ChangedShapeKeySet?.Invoke(this, EventArgs.Empty);
    }

    private void SetBlendValue(string key, float value, bool fix = true, bool notify = true)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));

        if (!ContainsShapeKey(key))
            return;

        var index = (int)Face.hash[Face.GP01FbFaceHashKey(key)];

        Face.dicBlendSet[Maid.ActiveFace][index] = value;

        if (key is "nosefook")
            Maid.boNoseFook = Convert.ToBoolean(value);

        Face.SetBlendValues(index, value);

        if (fix)
            FixBlendValues();

        if (notify)
            OnBlendValueChanged(key);
    }

    private void BackupBlendSet()
    {
        backupBlendSetName = Maid.ActiveFace;

        var blendSet = Face.dicBlendSet[Maid.ActiveFace];

        if (backupBlendSetValues is null || backupBlendSetValues.Length != blendSet.Length)
            backupBlendSetValues = new float[blendSet.Length];

        blendSet.CopyTo(backupBlendSetValues, 0);
    }

    private void ApplyBackupBlendSet()
    {
        if (!string.Equals(backupBlendSetName, Maid.ActiveFace))
            return;

        var blendSet = Face.dicBlendSet[Maid.ActiveFace];

        if (backupBlendSetValues is null || backupBlendSetValues.Length != blendSet.Length)
            return;

        backupBlendSetValues.CopyTo(blendSet, 0);
    }

    // NOTE: Maid.FaceAnime indirectly calls TMorph.MulBlendValues which essentially does what this loop does
    // but directly calls TMorph.SetBlendValues. This is required for maid ijiri support.
    private void ExplicitApply()
    {
        foreach (var key in Face.hash.Keys)
        {
            var index = (int)Face.hash[key];
            var value = Face.dicBlendSet[Maid.ActiveFace][index];

            if (key is "nosefook")
                Maid.boNoseFook = Convert.ToBoolean(value);

            Face.SetBlendValues(index, value);
        }

        FixBlendValues();
    }

    private void FixBlendValues()
    {
        Face.FixBlendValues();
        Face.FixBlendValues_Face();
    }

    private void OnBlendValueChanged(string key)
    {
        if (!BlendValueChangeArgsCache.TryGetValue(key, out var e))
            e = BlendValueChangeArgsCache[key] = new(key);

        ChangedShapeKey?.Invoke(this, e);
    }

    private void RaisePropertyChanged(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        PropertyChanged?.Invoke(this, new(name));
    }
}
