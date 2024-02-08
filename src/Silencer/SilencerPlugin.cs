using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using Silencer.Data;
using SpaceWarp;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Mods;
using Silencer.UI;
using UitkForKsp2.API;
using UnityEngine.UIElements;

namespace Silencer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class SilencerPlugin : BaseSpaceWarpPlugin
{
    // Useful in case some other mod wants to use this mod a dependency
    [PublicAPI] public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    [PublicAPI] public const string ModName = MyPluginInfo.PLUGIN_NAME;
    [PublicAPI] public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    /// Singleton instance of the plugin class
    [PublicAPI] public static SilencerPlugin Instance { get; set; }

    internal StorageManager Storage { get; private set; }

    private void Start()
    {
        Instance = this;

        string assemblyFolder = Path.GetDirectoryName(GetType().Assembly.Location);
        Storage = new StorageManager(assemblyFolder);
        Harmony.CreateAndPatchAll(typeof(Patches));
    }

    /// <summary>
    /// Runs when the mod is first initialized.
    /// </summary>
    public override void OnInitialized()
    {
        base.OnInitialized();

        // Instantiate the UI
        var windowUxml = AssetManager.GetAsset<VisualTreeAsset>($"{ModGuid}/silencer_ui/ui/silencer.uxml");
        var windowOptions = WindowOptions.Default with { WindowId = "Silencer_Window" };
        var window = Window.Create(windowOptions, windowUxml);
        window.gameObject.AddComponent<UIController>();
    }
}