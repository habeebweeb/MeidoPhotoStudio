using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using MeidoPhotoStudio.Database.Character;
using MeidoPhotoStudio.Plugin.Core.Serialization;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class FaceController
{
    private readonly CharacterController characterController;

    private string backupBlendSetName;
    private float[] backupBlendSetValues;

    public FaceController(CharacterController characterController)
    {
        this.characterController = characterController ?? throw new ArgumentNullException(nameof(characterController));

        BackupBlendSet();
    }

    public event EventHandler ChangedBlendSet;

    public IBlendSetModel BlendSet { get; private set; }

    public IEnumerable<string> ExpressionKeys =>
        Face.BlendDatas.Where(blendData => blendData is not null).Select(blendData => blendData.name);

    public bool Blink
    {
        get => !Maid.MabatakiUpdateStop;
        set
        {
            Maid.MabatakiUpdateStop = !value;
            Maid.body0.Face.morph.EyeMabataki = 0f;
            Maid.MabatakiVal = 0f;
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

        set
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));

            if (!ContainsExpressionKey(key))
                return;

            var index = (int)Face.hash[Face.GP01FbFaceHashKey(key)];

            Face.dicBlendSet[Maid.ActiveFace][index] = value;

            if (key is "nosefook")
                Maid.boNoseFook = Convert.ToBoolean(value);
        }
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

        ChangedBlendSet?.Invoke(this, EventArgs.Empty);

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

            foreach (var kvp in facialExpressionSet.Where(kvp => ContainsExpressionKey(kvp.Key)))
                this[kvp.Key] = Mathf.Clamp(kvp.Value, 0f, 1f);
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
}
