using System.ComponentModel;

using BepInEx.Configuration;

using SortingMode = MeidoPhotoStudio.Plugin.SceneBrowserWindow.SortingMode;

namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class SceneBrowserConfiguration
{
    private readonly ConfigFile configFile;
    private readonly ConfigEntry<bool> sortDescendingConfigEntry;
    private readonly ConfigEntry<SortingMode> sortingModeConfigEntry;

    public SceneBrowserConfiguration(ConfigFile configFile)
    {
        this.configFile = configFile ?? throw new ArgumentNullException(nameof(configFile));

        sortDescendingConfigEntry = this.configFile.Bind("Scene Browser", "Sort Scenes Descending", false);
        sortingModeConfigEntry = this.configFile.Bind("Scene Browser", "Sorting Mode", SortingMode.Name);
    }

    public bool SortDescending
    {
        get => sortDescendingConfigEntry.Value;
        set => sortDescendingConfigEntry.Value = value;
    }

    public SortingMode SortingMode
    {
        get => sortingModeConfigEntry.Value;
        set
        {
            if (!Enum.IsDefined(typeof(SortingMode), value))
                throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(SortingMode));

            sortingModeConfigEntry.Value = value;
        }
    }
}
