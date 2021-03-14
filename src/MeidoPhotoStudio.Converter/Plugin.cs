using System.IO;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MeidoPhotoStudio.Converter
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "com.habeebweeb.com3d2.meidophotostudio.converter";
        public const string PluginName = "MeidoPhotoStudio Converter";
        public const string PluginVersion = "0.0.0";

        private readonly PluginCore pluginCore;
        private readonly UI ui;

        public static Plugin? Instance { get; private set; }
        public new ManualLogSource Logger { get; private set; }

        public Plugin()
        {
            pluginCore = new(); // Path.Combine(Paths.ConfigPath, PluginName)
            ui = new();
        }

        private void Awake()
        {
            DontDestroyOnLoad(this);

            Instance = this;
            Logger = base.Logger;

            SceneManager.sceneLoaded += (scene, _) => ui.Visible = scene.buildIndex is 3 or 9;
        }

        private void OnGUI() => ui.Draw();
    }
}
