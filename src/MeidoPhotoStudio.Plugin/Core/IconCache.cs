using MeidoPhotoStudio.Database.Props;
using MeidoPhotoStudio.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Framework;

namespace MeidoPhotoStudio.Plugin.Core;

public class IconCache
{
    private readonly Dictionary<MPN, Dictionary<int, Texture2D>> menuTextureCache = new(EnumEqualityComparer<MPN>.Instance);
    private readonly Dictionary<int, Dictionary<int, Texture>> myRoomTextureCache = [];

    public Texture2D GetMenuIcon(MenuFilePropModel model)
    {
        if (!menuTextureCache.ContainsKey(model.CategoryMpn))
            menuTextureCache[model.CategoryMpn] = [];

        if (menuTextureCache[model.CategoryMpn].TryGetValue(model.ID.GetHashCode(), out var icon))
            return icon;

        var generatedTexture = GenerateIcon(model.IconFilename);

        menuTextureCache[model.CategoryMpn][model.ID.GetHashCode()] = generatedTexture;

        return generatedTexture;

        static Texture2D GenerateIcon(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return null;

            if (TryCreateTexture(filename, out var icon))
                return icon;
            else if (TryCreateTexture($@"tex\{filename}", out icon))
                return icon;

            Utility.LogMessage($"Failed to generate icon {filename}");

            return null;

            static bool TryCreateTexture(string filename, out Texture2D texture)
            {
                try
                {
                    texture = ImportCM.CreateTexture(filename);

                    return true;
                }
                catch
                {
                }

                texture = null;

                return false;
            }
        }
    }

    public Texture GetMyRoomIcon(MyRoomPropModel model)
    {
        if (!myRoomTextureCache.ContainsKey(model.CategoryID))
            myRoomTextureCache[model.CategoryID] = [];

        if (myRoomTextureCache[model.CategoryID].TryGetValue(model.ID, out var icon))
            return icon;

        var generatedTexture = GenerateIcon(model.ID);

        myRoomTextureCache[model.CategoryID][model.ID] = generatedTexture;

        return generatedTexture;

        static Texture GenerateIcon(int id)
        {
            var placementData = MyRoomCustom.PlacementData.GetData(id);

            if (placementData is null)
                return null;

            return placementData.GetThumbnail();
        }
    }

    public void Destroy()
    {
        foreach (var texture in menuTextureCache.Values.SelectMany(cache => cache.Values))
            Object.DestroyImmediate(texture);

        foreach (var texture in myRoomTextureCache.Values.SelectMany(cache => cache.Values))
            Object.DestroyImmediate(texture);

        menuTextureCache.Clear();
        myRoomTextureCache.Clear();
    }
}
