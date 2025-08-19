namespace MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;

public class ShiftClickExplorerModHandler : IRevealModInFileManagerHandler
{
    public void Reveal(MenuFilePropModel menuFilePropModel)
    {
        _ = menuFilePropModel ?? throw new ArgumentNullException(nameof(menuFilePropModel));

        if (!menuFilePropModel.GameMenu)
            ShiftClickExplorer.Main.RevealMenuFileInFileExplorer(menuFilePropModel.Filename);

        ShiftClickExplorer.Main.CopyToClipboard(menuFilePropModel.Filename);
    }

    public void Open(MenuFilePropModel menuFilePropModel)
    {
        _ = menuFilePropModel ?? throw new ArgumentNullException(nameof(menuFilePropModel));

        if (!menuFilePropModel.GameMenu)
            ShiftClickExplorer.Main.OpenMenuFile(menuFilePropModel.Filename);

        ShiftClickExplorer.Main.CopyToClipboard(menuFilePropModel.Filename);
    }
}
