using MeidoPhotoStudio.Plugin.Framework.Menu;

namespace MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;

internal class MenuFileParser
{
    public MenuFilePropModel ParseMenuFile(string menuFilename, bool gameMenu)
    {
        if (string.IsNullOrEmpty(menuFilename))
            throw new ArgumentException($"'{nameof(menuFilename)}' cannot be null or empty.", nameof(menuFilename));

        using var aFileBase = GameUty.FileOpen(menuFilename);

        if (aFileBase is null)
            return null;

        if (!aFileBase.IsValid() || aFileBase.GetSize() is 0)
            return null;

        using var aFileBaseStream = new AFileBaseStream(aFileBase);
        using var binaryReader = new BinaryReader(aFileBaseStream, Encoding.UTF8);

        if (binaryReader.ReadString() is not "CM3D2_MENU")
            return null;

        // file version
        binaryReader.ReadInt32();

        // txt path
        binaryReader.ReadString();

        // name
        binaryReader.ReadString();

        // category
        var categoryMpnText = binaryReader.ReadString();

        // description
        binaryReader.ReadString();

        // idk (as long)
        binaryReader.ReadInt32();

        var categoryMpn = MPN.null_mpn;

        try
        {
            categoryMpn = (MPN)Enum.Parse(typeof(MPN), categoryMpnText, true);
        }
        catch
        {
            return null;
        }

        var menuBuilder = new MenuFilePropModel.Builder(menuFilename, gameMenu)
            .WithMpn(categoryMpn);

        while (true)
        {
            var numberOfProps = binaryReader.ReadByte();
            var menuPropString = string.Empty;

            if (numberOfProps is 0)
                break;

            for (var i = 0; i < numberOfProps; i++)
                menuPropString = $"{menuPropString}\"{binaryReader.ReadString()}\"";

            if (string.IsNullOrEmpty(menuPropString))
                continue;

            var header = UTY.GetStringCom(menuPropString);
            var menuProps = UTY.GetStringList(menuPropString);

            if (header is "end")
            {
                break;
            }
            else if (header is "name")
            {
                if (menuProps.Length > 1)
                    menuBuilder.WithName(menuProps[1]);
            }
            else if (header is "icons" or "icon")
            {
                menuBuilder.WithIconFilename(menuProps[1]);
            }
            else if (header is "priority")
            {
                menuBuilder.WithPriority(float.Parse(menuProps[1]));
            }
            else if (header is "マテリアル変更" or "tex")
            {
                var materialIndex = int.Parse(menuProps[2]);
                var materialFilename = menuProps[3];

                menuBuilder.AddMaterialChange(new()
                {
                    MaterialIndex = materialIndex,
                    MaterialFilename = materialFilename,
                });
            }
            else if (header is "additem")
            {
                menuBuilder.WithModelFilename(menuProps[1]);
            }
            else if (header is "anime")
            {
                if (menuProps.Length < 3)
                    continue;

                var slot = (SlotID)Enum.Parse(typeof(SlotID), menuProps[1], true);
                var animationName = menuProps[2];
                var loop = false;

                if (menuProps.Length > 3)
                    loop = menuProps[3] is "loop";

                menuBuilder.AddModelAnime(new()
                {
                    Slot = slot,
                    AnimationName = animationName,
                    Loop = loop,
                });
            }
            else if (header is "animematerial")
            {
                var slot = (SlotID)Enum.Parse(typeof(SlotID), menuProps[1], true);
                var materialIndex = int.Parse(menuProps[2]);

                menuBuilder.AddModelMaterialAnimation(new()
                {
                    Slot = slot,
                    MaterialIndex = materialIndex,
                });
            }
        }

        return menuBuilder.Build();
    }
}
