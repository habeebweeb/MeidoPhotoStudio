using BepInEx;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("org.bepinex.plugins.unityinjectorloader", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    public const string PluginName = "MeidoPhotoStudio";
    public const string PluginVersion = "1.0.0";
    public const string PluginSubVersion = "beta.5.1";

    public static readonly string PluginString = $"{PluginName} {PluginVersion}";

    private const string PluginGuid = "com.habeebweeb.com3d2.meidophotostudio";

    private HarmonyLib.Harmony harmony;

    static Plugin()
    {
        if (!string.IsNullOrEmpty(PluginSubVersion))
            PluginString += $"-{PluginSubVersion}";
    }

    public static Core.PluginCore Instance { get; private set; }

    private void Awake()
    {
        harmony = HarmonyLib.Harmony.CreateAndPatchAll(typeof(AllProcPropSeqPatcher));
        harmony.PatchAll(typeof(BgMgrPatcher));
        harmony.PatchAll(typeof(MeidoManager));

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
