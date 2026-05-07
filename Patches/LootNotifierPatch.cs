using HarmonyLib;
using InventoryTweaks.Components;

namespace InventoryTweaks.Patches;

[HarmonyPatch(typeof(LootNotifier))]
public class LootNotifierPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(LootNotifier.AddLootNotification))]
    public static bool AddLootNotification_Prefix()
    {
        return !InventorySorter.dontSendNotifications;
    }
}
