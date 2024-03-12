namespace MeidoPhotoStudio.Plugin.Framework.Extensions;

public static class AFileSystemBaseExtensions
{
    public static CsvParser OpenCsvParser(this AFileSystemBase aFileSystemBase, string neiFile)
    {
        if (!aFileSystemBase.IsExistentFile(neiFile))
            return null;

        CsvParser csvParser = null;
        AFileBase aFileBase = null;

        try
        {
            aFileBase = aFileSystemBase.FileOpen(neiFile);
            csvParser = new CsvParser();

            if (csvParser.Open(aFileBase))
                return csvParser;
        }
        catch
        {
        }

        csvParser?.Dispose();
        aFileBase?.Dispose();

        return null;
    }
}
