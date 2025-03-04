using System.Reflection;

using BepInEx;
using BepInEx.Logging;
using MeidoPhotoStudio.Plugin.Core.Patchers;

namespace MeidoPhotoStudio.Plugin;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("org.bepinex.plugins.unityinjectorloader", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "com.habeebweeb.com3d2.meidophotostudio";
    public const string PluginName = "MeidoPhotoStudio";
    public const string PluginVersion = "1.0.0";
    public const string PluginSubVersion = "beta.5.1";

    public static readonly string PluginString = $"{PluginName} {PluginVersion}";

    private HarmonyLib.Harmony harmony;

    static Plugin()
    {
        if (!string.IsNullOrEmpty(PluginSubVersion))
            PluginString += $"-{PluginSubVersion}";
    }

    public static Core.PluginCore Instance { get; private set; }

    internal static string BuildVersion { get; private set; }

    internal static new ManualLogSource Logger { get; private set; }

    private void Awake()
    {
        try
        {
            var attribute = Attribute.GetCustomAttribute(typeof(Plugin).Assembly, typeof(AssemblyInformationalVersionAttribute));
            var version = (attribute as AssemblyInformationalVersionAttribute).InformationalVersion;

            BuildVersion = $"build {version}";
        }
        catch
        {
            BuildVersion = PluginString;
        }

        Logger = base.Logger;

        harmony = HarmonyLib.Harmony.CreateAndPatchAll(typeof(AllProcPropSeqPatcher));
        harmony.PatchAll(typeof(BgMgrPatcher));
        harmony.PatchAll(typeof(Core.Background.BackgroundService));

        var coreGameObject = new GameObject
        {
            name = "[MeidoPhotoStudio Plugin Core]",
            hideFlags = HideFlags.HideAndDontSave,
        };

        Instance = coreGameObject.AddComponent<Core.PluginCore>();
    }

    private void OnDestroy()
    {
        harmony.UnpatchSelf();

        if (Instance)
            Destroy(Instance.gameObject);
    }
}
