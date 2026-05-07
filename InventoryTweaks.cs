using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using InventoryTweaks.Components;

namespace InventoryTweaks;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class InventoryTweaks : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    public const string PLUGIN_GUID = "com.theblackvoid.inventorytweaks";
    public const string PLUGIN_NAME = "Inventory Tweaks";
    public const string PLUGIN_VERSION = "1.0.0";
    private readonly Harmony HarmonyInstance = new(PLUGIN_GUID);

    public static bool autoSortOnOpen = false;
    public static bool tierBasedColors = true;
    public static FirstSortBy firstSortBy = FirstSortBy.SubType;

    private void Awake()
    {
        Logger = base.Logger;
        try
        {
            InitConfigs();
            gameObject.AddComponent<InventorySorter>();
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo("Successfully loaded!");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to load: {ex}");
        }
    }

    private void InitConfigs()
    {
        ConfigEntry<bool> sortOnOpen = Config.Bind(
            "Settings",
            "Auto-Sort",
            false,
            new ConfigDescription(
                "If enabled, your inventory will be automatically sorted whenever it is opened."
            )
        );
        autoSortOnOpen = sortOnOpen.Value;

        ConfigEntry<bool> tierColors = Config.Bind(
            "Settings",
            "Tier-Based Colors",
            true,
            new ConfigDescription(
                "If enabled, inventory slots will be colored based on the item's tier."
            )
        );
        tierBasedColors = tierColors.Value;

        ConfigEntry<FirstSortBy> firstSort = Config.Bind(
            "Settings",
            "First Sort By",
            FirstSortBy.SubType,
            new ConfigDescription("The first sort method to use.")
        );
        firstSortBy = firstSort.Value;
    }
}
