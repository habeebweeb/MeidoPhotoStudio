using BepInEx.Configuration;

namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class TranslationConfiguration
{
    private readonly ConfigEntry<string> currentLanguageConfig;
    private readonly ConfigEntry<bool> suppressWarningsConfigEntry;

    public TranslationConfiguration(ConfigFile configFile)
    {
        _ = configFile ?? throw new ArgumentNullException(nameof(configFile));

        suppressWarningsConfigEntry = configFile.Bind(
            "Translation",
            "SuppressWarnings",
            true,
            "Suppress translation warnings from showing up in the console");

        currentLanguageConfig = configFile.Bind(
            "Translation",
            "Language",
            "en",
            "Directory to pull translations from\nTranslations are found in the 'Translations' folder");

        Translation.SuppressWarnings = suppressWarningsConfigEntry.Value;
        Translation.CurrentLanguage = currentLanguageConfig.Value;
    }

    public bool SuppressWarnings
    {
        get => suppressWarningsConfigEntry.Value;
        set
        {
            suppressWarningsConfigEntry.Value = value;
            Translation.SuppressWarnings = value;
        }
    }

    public string CurrentLanguage
    {
        get => currentLanguageConfig.Value;
        set
        {
            currentLanguageConfig.Value = value;
            Translation.CurrentLanguage = value;
            Translation.ReinitializeTranslation();
        }
    }
}
