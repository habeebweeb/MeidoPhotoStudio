using System.IO;
using BepInEx;
using BepInEx.Logging;
using MeidoPhotoStudio.Converter.Converters;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MeidoPhotoStudio.Converter
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.habeebweeb.com3d2.meidophotostudio")]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "com.habeebweeb.com3d2.meidophotostudio.converter";
        public const string PluginName = "MeidoPhotoStudio Converter";
        public const string PluginVersion = "0.0.1";

        private PluginCore pluginCore;
        private UI ui;

        public static Plugin? Instance { get; private set; }
        public new ManualLogSource Logger { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(this);

            Instance = this;
            Logger = base.Logger;

            var workingDirectory = Path.Combine(Paths.ConfigPath, PluginName);

            if (!Directory.Exists(workingDirectory))
                Directory.CreateDirectory(workingDirectory);

            pluginCore = new(workingDirectory, new MMConverter());
            ui = new(pluginCore);

            SceneManager.sceneLoaded += (scene, _) =>
                ui.Visible = scene.buildIndex is 3 or 9;
        }

        private void OnGUI() =>
            ui.Draw();
    }
}
