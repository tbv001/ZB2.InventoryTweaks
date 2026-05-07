using HarmonyLib;
using InventoryTweaks.Components;
using UnityEngine;

namespace InventoryTweaks.Patches;

[HarmonyPatch(typeof(InventoryDisplay))]
public class InventoryDisplayPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(InventoryDisplay.Open))]
    public static void Open_Postfix()
    {
        var playerMain = PlayersController.instance.MyPlayer();
        if (playerMain == null)
            return;

        InventorySorter.curInventory = playerMain.inventory;
        InventorySorter.showGui = true;

        if (InventoryTweaks.autoSortOnOpen)
        {
            InventorySorter.SortItems();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(InventoryDisplay.OnClosed))]
    public static void OnClosed_Postfix()
    {
        InventorySorter.showGui = false;
        InventorySorter.curInventory = null;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(InventoryDisplay.LeftClickedInventoryItem))]
    public static bool LeftClickedInventoryItem_Prefix(InventoryDisplay __instance)
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            var traverse = Traverse.Create(__instance);
            var hoveredItem = traverse.Field<InventoryItem>("hoveredItem").Value;
            var curSubMenu = traverse.Field<InventoryDisplay.SubMenu>("curSubMenu").Value;

            if (curSubMenu < InventoryDisplay.SubMenu.Context && hoveredItem != null)
            {
                var inventory = __instance.targetInventory;
                if (__instance.hoverContainer == InventoryDisplay.ContainerType.Equipment)
                {
                    var pos = inventory.FindPlaceFor(
                        hoveredItem.id,
                        hoveredItem.stackCount,
                        true,
                        false
                    );

                    if (pos != null)
                    {
                        inventory.RemoveItem(hoveredItem);
                        inventory.PutLootIntoPosition(hoveredItem, pos, false);
                        __instance.Show(inventory);
                        AudioController.instance.PlayUI(AudioController.UIFXID.InventoryRelease);

                        return false;
                    }

                    return false;
                }
                else if (__instance.hoverContainer == InventoryDisplay.ContainerType.Inventory)
                {
                    var pos = inventory.FindPlaceFor(
                        hoveredItem.id,
                        hoveredItem.stackCount,
                        true,
                        true
                    );

                    if (pos != null && pos.AutoEquip)
                    {
                        inventory.RemoveItem(hoveredItem);
                        inventory.PutLootIntoPosition(hoveredItem, pos, false);
                        __instance.Show(inventory);
                        AudioController.instance.PlayUI(AudioController.UIFXID.InventoryEquip);

                        return false;
                    }

                    var subType = (int)hoveredItem.GetDataBaseItem().GetSubType();
                    if (subType >= 0 && subType < inventory.equippedItem.Length)
                    {
                        var currentEquip = inventory.equippedItem[subType];
                        if (currentEquip != null && !currentEquip.IsNone)
                        {
                            var backupPos = hoveredItem.pos;
                            var backupRot = hoveredItem.rotated;

                            inventory.RemoveItem(hoveredItem);

                            var storagePos = inventory.FindPlaceFor(
                                currentEquip.id,
                                currentEquip.stackCount,
                                true,
                                false
                            );

                            if (storagePos != null)
                            {
                                inventory.RemoveEquippedItem(subType);
                                inventory.PutLootIntoPosition(currentEquip, storagePos, false);

                                var equipPos = new LootTargetPosition
                                {
                                    targetEquipmentIndex = subType,
                                };
                                inventory.PutLootIntoPosition(hoveredItem, equipPos, false);

                                __instance.Show(inventory);
                                AudioController.instance.PlayUI(
                                    AudioController.UIFXID.InventoryEquip
                                );

                                return false;
                            }
                            else
                            {
                                var revertPos = new LootTargetPosition
                                {
                                    addNewItemToStorage = true,
                                    containerPos = backupPos,
                                    rotated = backupRot,
                                };
                                inventory.PutLootIntoPosition(hoveredItem, revertPos, false);
                            }
                        }
                    }

                    return false;
                }
            }
        }
        return true;
    }
}
