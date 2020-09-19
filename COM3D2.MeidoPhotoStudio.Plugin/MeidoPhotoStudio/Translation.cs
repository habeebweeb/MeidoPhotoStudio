using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using BepInEx.Configuration;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal static class Translation
    {
        private static readonly string[] props = { "ui", "props", "bg", "face" };
        private static Dictionary<string, Dictionary<string, string>> Translations;
        private static readonly ConfigEntry<string> currentLanguage;
        public static string CurrentLanguage
        {
            get => currentLanguage.Value;
            private set => currentLanguage.Value = value;
        }
        public static event EventHandler ReloadTranslationEvent;

        static Translation()
        {
            currentLanguage = Configuration.Config.Bind(
                "Translation", "Language",
                "en",
                "Directory to pull translations from"
                + "\nTranslations are found in the 'Translations' folder"
            );

            Configuration.Config.ConfigReloaded += OnSettingChange;
        }

        private static void OnSettingChange(object sender, EventArgs args) => ReloadTranslation();

        public static void Initialize(string language)
        {
            string rootTranslationPath = Path.Combine(Constants.configPath, Constants.translationDirectory);
            string currentTranslationPath = Path.Combine(rootTranslationPath, language);

            Translations = new Dictionary<string, Dictionary<string, string>>(
                StringComparer.InvariantCultureIgnoreCase
            );

            if (!Directory.Exists(currentTranslationPath))
            {
                Utility.LogWarning(
                    $"No translations found for '{language}' in '{currentTranslationPath}'"
                );
                return;
            }

            foreach (string prop in props)
            {
                string translationFile = $"translation.{prop}.json";
                try
                {
                    string translationPath = Path.Combine(currentTranslationPath, translationFile);

                    string translationJson = File.ReadAllText(translationPath);

                    JObject translation = JObject.Parse(translationJson);

                    foreach (JProperty translationProp in translation.AsJEnumerable())
                    {
                        JToken token = translationProp.Value;
                        Translations[translationProp.Path] = new Dictionary<string, string>(
                            token.ToObject<Dictionary<string, string>>(), StringComparer.InvariantCultureIgnoreCase
                        );
                    }
                }
                catch
                {
                    Utility.LogError($"Could not find translation file '{translationFile}'");
                }
            }
        }

        public static void ReinitializeTranslation() => Configuration.Config.Reload();

        public static void ReloadTranslation()
        {
            Initialize(CurrentLanguage);
            OnReloadTranslation();
        }

        public static void OnReloadTranslation()
        {
            ReloadTranslationEvent?.Invoke(null, EventArgs.Empty);
        }

        public static bool Has(string category, string text, bool warn = false)
        {
            if (!Translations.ContainsKey(category))
            {
                if (warn) Utility.LogWarning($"Could not translate '{text}': category '{category}' was not found");
                return false;
            }

            if (!Translations[category].ContainsKey(text))
            {
                if (warn)
                {
                    Utility.LogWarning(
                        $"Could not translate '{text}': '{text}' was not found in category '{category}'"
                    );
                }
                return false;
            }

            return true;
        }

        public static string Get(string category, string text, bool warn = true)
        {
            return Has(category, text, warn) ? Translations[category][text] : text;
        }

        public static string[] GetArray(string category, IEnumerable<string> list)
        {
            return GetList(category, list).ToArray();
        }

        public static IEnumerable<string> GetList(string category, IEnumerable<string> list)
        {
            return list.Select(uiName => Get(category, uiName));
        }
    }
}
