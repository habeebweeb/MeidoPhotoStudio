using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.Props;

public class ShapeKeyController(Mesh mesh, TBodySkin.OriVert oriVert, BlendData[] blendDatas)
    : IEnumerable<(string HashKey, float BlendValue)>
{
    private readonly Dictionary<string, KeyedPropertyChangeEventArgs<string>> shapeKeyChangeArgsCache = new(StringComparer.Ordinal);
    private readonly Mesh mesh = mesh ? mesh : throw new ArgumentNullException(nameof(mesh));
    private readonly TBodySkin.OriVert oriVert = oriVert ?? throw new ArgumentNullException(nameof(oriVert));
    private readonly BlendData[] blendDatas = blendDatas ?? throw new ArgumentNullException(nameof(blendDatas));
    private readonly float[] blendValues = new float[blendDatas.Length];
    private readonly Vector3[] temporaryVerts = new Vector3[oriVert.VCount];
    private readonly Vector3[] temporaryNorms = new Vector3[oriVert.VCount];
    private readonly Dictionary<string, int> hashKeyToBlendValueIndex = blendDatas
        .Select((blendData, index) => (index, blendData))
        .ToDictionary(kvp => kvp.blendData.name, kvp => kvp.index, StringComparer.OrdinalIgnoreCase);

    public event EventHandler<KeyedPropertyChangeEventArgs<string>> ShapeKeyChanged;

    public IEnumerable<string> Keys =>
        blendDatas.Select(blendData => blendData.name);

    public IEnumerable<float> BlendValues =>
        blendValues;

    public float this[string hashKey]
    {
        get => blendValues[hashKeyToBlendValueIndex[hashKey]];
        set
        {
            if (blendValues[hashKeyToBlendValueIndex[hashKey]] == value)
                return;

            blendValues[hashKeyToBlendValueIndex[hashKey]] = value;

            FixBlendValues();
            OnShapeKeyChanged(hashKey);
        }
    }

    public IEnumerator<(string HashKey, float BlendValue)> GetEnumerator() =>
        blendDatas.Select(blendData => blendData.name).Zip(blendValues).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    private void FixBlendValues()
    {
        oriVert.vOriVert.CopyTo(temporaryVerts, 0);
        oriVert.vOriNorm.CopyTo(temporaryNorms, 0);

        foreach (var (blendValue, blendData) in blendValues.Zip(blendDatas))
        {
            for (var i = 0; i < blendData.v_index.Length; i++)
            {
                var index = blendData.v_index[i];

                temporaryVerts[index] += blendData.vert[i] * blendValue;
                temporaryNorms[index] += blendData.norm[i] * blendValue;
            }
        }

        mesh.vertices = temporaryVerts;
        mesh.normals = temporaryNorms;
    }

    private void OnShapeKeyChanged(string key)
    {
        if (!shapeKeyChangeArgsCache.TryGetValue(key, out var e))
            e = shapeKeyChangeArgsCache[key] = new(key);

        ShapeKeyChanged?.Invoke(this, e);
    }
}
