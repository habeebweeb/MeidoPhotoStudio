using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal static class Translation
    {
        public static Dictionary<string, Dictionary<string, string>> Translations;
        public static string CurrentLanguage { get; private set; }
        public static event EventHandler ReloadTranslationEvent;

        public static void Initialize(string language)
        {
            CurrentLanguage = language;

            string translationFile = $"translations.{language}.json";
            string translationPath = Path.Combine(Constants.configPath, translationFile);
            string translationJson = File.ReadAllText(translationPath);

            JObject translation = JObject.Parse(translationJson);

            Translations = new Dictionary<string, Dictionary<string, string>>(
                StringComparer.InvariantCultureIgnoreCase
            );

            foreach (JProperty translationProp in translation.AsJEnumerable())
            {
                JToken token = translationProp.Value;
                Translations[translationProp.Path] = new Dictionary<string, string>(
                    token.ToObject<Dictionary<string, string>>(), StringComparer.InvariantCultureIgnoreCase
                );
            }
        }

        public static void SetLanguage(string language)
        {
            Initialize(language);
            OnReloadTranslation();
        }

        public static void ReloadTranslation()
        {
            Initialize(CurrentLanguage);
            OnReloadTranslation();
        }

        public static void OnReloadTranslation()
        {
            ReloadTranslationEvent?.Invoke(null, EventArgs.Empty);
        }

        public static string Get(string category, string text)
        {
            if (!Translations.ContainsKey(category))
            {
                Debug.LogWarning($"Could not find category '{category}'");
                return text;
            }

            if (!Translations[category].ContainsKey(text))
            {
                Debug.LogWarning($"Could not find translation for '{text}' in '{category}'");
                return text;
            }

            return Translations[category][text];
        }

        public static string[] GetArray(string category, IEnumerable<string> list)
        {
            return GetList(category, list).ToArray();
        }

        public static IEnumerable<string> GetList(string category, IEnumerable<string> list)
        {
            return list.Select(uiName => Get(category, uiName));
        }

        public static string[] GetList(string category, IEnumerable<KeyValuePair<string, string>> list)
        {
            return list.Select(kvp => Get(category, kvp.Key)).ToArray();
        }
    }
}
