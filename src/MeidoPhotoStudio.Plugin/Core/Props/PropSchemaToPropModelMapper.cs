using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Core.Schema.Props;
using MeidoPhotoStudio.Plugin.Framework;

namespace MeidoPhotoStudio.Plugin.Core.Props;

public class PropSchemaToPropModelMapper(
    Translation translation,
    BackgroundPropRepository backgroundPropRepository,
    DeskPropRepository deskPropRepository,
    MyRoomPropRepository myRoomPropRepository,
    PhotoBgPropRepository photoBgPropRepository,
    MenuPropRepository menuPropRepository,
    OtherPropRepository otherPropRepository)
{
    private readonly Translation translation = translation
        ?? throw new ArgumentNullException(nameof(translation));

    private readonly BackgroundPropRepository backgroundPropRepository = backgroundPropRepository
        ?? throw new ArgumentNullException(nameof(backgroundPropRepository));

    private readonly DeskPropRepository deskPropRepository = deskPropRepository
        ?? throw new ArgumentNullException(nameof(deskPropRepository));

    private readonly MyRoomPropRepository myRoomPropRepository = myRoomPropRepository
        ?? throw new ArgumentNullException(nameof(myRoomPropRepository));

    private readonly PhotoBgPropRepository photoBgPropRepository = photoBgPropRepository
        ?? throw new ArgumentNullException(nameof(photoBgPropRepository));

    private readonly MenuPropRepository menuPropRepository = menuPropRepository
        ?? throw new ArgumentNullException(nameof(menuPropRepository));

    private readonly OtherPropRepository otherPropRepository = otherPropRepository
        ?? throw new ArgumentNullException(nameof(otherPropRepository));

    public IPropModel Resolve(IPropModelSchema propModelSchema)
    {
        if (propModelSchema is BackgroundPropModelSchema backgroundPropModelSchema)
        {
            return backgroundPropRepository.GetByID(backgroundPropModelSchema.ID);
        }
        else if (propModelSchema is DeskPropModelSchema deskPropModelSchema)
        {
            return deskPropRepository.GetByID(deskPropModelSchema.ID);
        }
        else if (propModelSchema is MyRoomPropModelSchema myRoomPropModelSchema)
        {
            return myRoomPropRepository.GetByID(myRoomPropModelSchema.ID);
        }
        else if (propModelSchema is OtherPropModelSchema otherPropModel)
        {
            return otherPropRepository.GetByID(otherPropModel.AssetName);
        }
        else if (propModelSchema is PhotoBgPropModelSchema photoBgPropModelSchema)
        {
            return photoBgPropRepository.GetByID(photoBgPropModelSchema.ID);
        }
        else if (propModelSchema is MenuFilePropModelSchema menuFilePropModelSchema)
        {
            if (menuPropRepository.Busy)
            {
                if (string.IsNullOrEmpty(menuFilePropModelSchema.Filename))
                    return null;

                var sanitizedFilename = SanitizeFilename(menuFilePropModelSchema.Filename);

                var menuFile = new MenuFileParser()
                    .ParseMenuFile(sanitizedFilename, false);

                if (menuFile is null)
                    return null;

                if (menuFile.CategoryMpn == SafeMpn.handitem)
                    menuFile.Name = translation["propNames", menuFile.Filename];

                return menuFile;
            }
            else
            {
                if (string.IsNullOrEmpty(menuFilePropModelSchema.ID))
                    return null;

                var sanitizedID = SanitizeFilename(menuFilePropModelSchema.ID);

                return menuPropRepository.GetByID(sanitizedID);
            }

            static string SanitizeFilename(string filename)
            {
                var sanitized = Path.GetFileName(filename);

                if (!string.Equals(filename, sanitized, StringComparison.OrdinalIgnoreCase))
                    Plugin.Logger.LogInfo($"menu filename is invalid: original: '{filename}', sanitized: '{sanitized}'");

                return sanitized;
            }
        }

        return null;
    }
}
