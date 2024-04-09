using BepInEx.Configuration;
using Newtonsoft.Json.Linq;

namespace MeidoPhotoStudio.Plugin;

// TODO: Rework translation.
// The depth of translation is 2 deep so translation group -> translation key -> "translation". This is fairly limiting.
// Consider maybe a translation system like COM where translation keys are paths like
// ui/backgrounds/cm3d2/categoryName -> "Custom Maid 3D 2" and ui/backgrounds/cm3d2/backgrounds/Yashiki_Day -> "Inn".
public static class Translation
{
    private const string SettingsHeader = "Translation";

    private static readonly string[] Props = { "ui", "props", "bg", "face" };
    private static readonly ConfigEntry<string> CurrentLanguageConfig = Configuration.Config.Bind(
        SettingsHeader,
        "Language",
        "en",
        "Directory to pull translations from\nTranslations are found in the 'Translations' folder");

    private static readonly ConfigEntry<bool> SuppressWarningsConfig = Configuration.Config.Bind(
        SettingsHeader,
        "SuppressWarnings",
        false,
        "Suppress translation warnings from showing up in the console");

    private static Dictionary<string, Dictionary<string, string>> translations;
    private static bool forceSuppressWarnings;
    private static bool suppressWarningsCached;

    static Translation() =>
        suppressWarningsCached = !SuppressWarningsConfig.Value;

    public static event EventHandler ReloadTranslationEvent;

    public static bool SuppressWarnings
    {
        get => suppressWarningsCached;
        set
        {
            suppressWarningsCached = value;
            SuppressWarningsConfig.Value = value;
        }
    }

    public static string CurrentLanguage
    {
        get => CurrentLanguageConfig.Value;
        set => CurrentLanguageConfig.Value = value;
    }

    public static void Initialize(string language)
    {
        forceSuppressWarnings = false;

        var rootTranslationPath = Path.Combine(Constants.ConfigPath, Constants.TranslationDirectory);
        var currentTranslationPath = Path.Combine(rootTranslationPath, language);

        translations = new(StringComparer.InvariantCultureIgnoreCase);

        if (!Directory.Exists(currentTranslationPath))
        {
            Utility.LogError($"No translations found for '{language}' in '{currentTranslationPath}'");
            forceSuppressWarnings = true;

            return;
        }

        foreach (var prop in Props)
        {
            var translationFile = $"translation.{prop}.json";

            try
            {
                var translationPath = Path.Combine(currentTranslationPath, translationFile);
                var translationJson = File.ReadAllText(translationPath);
                var translation = JObject.Parse(translationJson);

                foreach (var translationProp in translation.AsJEnumerable().Cast<JProperty>())
                {
                    var token = translationProp.Value;

                    translations[translationProp.Path] =
                        new(token.ToObject<Dictionary<string, string>>(), StringComparer.InvariantCultureIgnoreCase);
                }
            }
            catch
            {
                forceSuppressWarnings = true;
                Utility.LogError($"Could not find translation file '{translationFile}'");
            }
        }
    }

    public static void ReinitializeTranslation()
    {
        Initialize(CurrentLanguage);
        ReloadTranslationEvent?.Invoke(null, EventArgs.Empty);
    }

    public static bool Has(string category, string text, bool warn = false)
    {
        warn = !forceSuppressWarnings && !SuppressWarnings && warn;

        if (!translations.ContainsKey(category))
        {
            if (warn)
                Utility.LogWarning($"Could not translate '{text}': category '{category}' was not found");

            return false;
        }

        if (!translations[category].ContainsKey(text))
        {
            if (warn)
                Utility.LogWarning(
                    $"Could not translate '{text}': '{text}' was not found in category '{category}'");

            return false;
        }

        return true;
    }

    public static string Get(string category, string text, bool warn = true) =>
        Has(category, text, warn) ? translations[category][text] : text;

    public static string[] GetArray(string category, IEnumerable<string> list) =>
        GetList(category, list).ToArray();

    public static IEnumerable<string> GetList(string category, IEnumerable<string> list) =>
        list.Select(uiName => Get(category, uiName));
}
