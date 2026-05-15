using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using InventoryTweaks.Components;

namespace InventoryTweaks;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public class InventoryTweaks : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;
    public const string PluginGuid = "com.theblackvoid.inventorytweaks";
    public const string PluginName = "Inventory Tweaks";
    public const string PluginVersion = "1.0.0";
    private readonly Harmony _harmony = new(PluginGuid);

    public static bool AutoSortOnOpen;
    public static bool TierBasedColors = true;
    public static FirstSortBy FirstSortBy = FirstSortBy.SubType;

    private void Awake()
    {
        Logger = base.Logger;
        try
        {
            InitConfigs();
            gameObject.AddComponent<InventorySorter>();
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo("Successfully loaded!");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to load: {ex}");
        }
    }

    private void InitConfigs()
    {
        ConfigEntry<bool> sortOnOpen = Config.Bind("Settings", "Auto-Sort", false,
            new ConfigDescription("If enabled, your inventory will be automatically sorted whenever it is opened."));
        AutoSortOnOpen = sortOnOpen.Value;

        ConfigEntry<bool> tierColors = Config.Bind("Settings", "Tier-Based Colors", true,
            new ConfigDescription("If enabled, inventory slots will be colored based on the item's tier."));
        TierBasedColors = tierColors.Value;

        ConfigEntry<FirstSortBy> firstSort = Config.Bind("Settings", "First Sort By", FirstSortBy.SubType,
            new ConfigDescription("The first sort method to use."));
        FirstSortBy = firstSort.Value;
    }
}
