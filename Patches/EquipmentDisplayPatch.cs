using HarmonyLib;

namespace InventoryTweaks.Patches;

[HarmonyPatch(typeof(EquipmentDisplay))]
public class EquipmentDisplayPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(EquipmentDisplay.UpdateHighlights))]
    public static void UpdateHighlights_Postfix(EquipmentDisplay __instance, PlayerInventory targetInventory, int highlightIndex)
    {
        if (!InventoryTweaks.tierBasedColors)
            return;

        for (int i = 0; i < __instance.equipmentSlot.Length; i++)
        {
            var item = targetInventory.equippedItem[i];
            if (item.IsNone)
                continue;

            var dbItem = item.GetDataBaseItem();
            if (dbItem == null)
                continue;

            var tColor = ItemsBase.TierColor(dbItem.tier);
            if (highlightIndex == i)
            {
                tColor.a = 0.2f;
            }
            else
            {
                tColor.a = 0.1f;
            }

            __instance.equipmentSlot[i].SetHighlight(tColor);
        }
    }
}
