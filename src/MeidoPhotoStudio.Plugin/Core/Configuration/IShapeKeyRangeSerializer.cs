namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public interface IShapeKeyRangeSerializer
{
    void Serialize(Dictionary<string, ShapeKeyRange> ranges);

    Dictionary<string, ShapeKeyRange> Deserialize();
}
