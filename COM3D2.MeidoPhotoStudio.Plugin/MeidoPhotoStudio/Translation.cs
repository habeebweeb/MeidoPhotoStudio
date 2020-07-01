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
        private static readonly string[] props = { "ui", "props", "bg", "face" };
        private static Dictionary<string, Dictionary<string, string>> Translations;
        public static string CurrentLanguage { get; private set; }
        public static event EventHandler ReloadTranslationEvent;

        public static void Initialize(string language)
        {
            CurrentLanguage = language;

            Translations = new Dictionary<string, Dictionary<string, string>>(
                StringComparer.InvariantCultureIgnoreCase
            );

            string rootTranslationPath = Path.Combine(Constants.configPath, "Translations");

            string currentTranslationPath = Path.Combine(rootTranslationPath, CurrentLanguage);

            if (!Directory.Exists(currentTranslationPath))
            {
                // Directory.CreateDirectory(currentTranslationPath);
                Debug.LogWarning(
                    $"No translations found for '{CurrentLanguage}' in '{currentTranslationPath}'"
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
                    Debug.LogError($"Could not find translation file '{translationFile}'");
                }
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
                Debug.LogWarning($"Could not find translation category '{category}'");
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
