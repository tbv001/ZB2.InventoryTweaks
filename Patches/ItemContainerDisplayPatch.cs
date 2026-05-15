using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace InventoryTweaks.Patches;

[HarmonyPatch(typeof(ItemContainerDisplay))]
public class ItemContainerDisplayPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ItemContainerDisplay.UpdateHighlights))]
    public static void UpdateHighlights_Postfix(ItemContainerDisplay __instance, ItemContainer ic,
        ref IntVec2 hoverSlot)
    {
        if (!InventoryTweaks.TierBasedColors)
            return;

        var traverse = Traverse.Create(__instance);

        var gridInit = traverse.Field<bool>("gridInit").Value;
        if (!gridInit)
            return;

        var size = traverse.Field<IntVec2>("size").Value;
        var gridColor = traverse.Field<byte[,]>("gridColor").Value;
        var gridSquare = traverse.Field<List<Image>>("gridSquare").Value;

        var tierColors = new Color?[size.x * size.y];
        foreach (var item2 in ic.items)
        {
            var dbItem = item2.GetDataBaseItem();
            if (dbItem == null)
                continue;

            var tColor = ItemsBase.TierColor(dbItem.tier);
            var itemSize = dbItem.Size(item2.rotated);

            for (int k = item2.pos.x; k < item2.pos.x + itemSize.x; k++)
            {
                for (int l = item2.pos.y; l < item2.pos.y + itemSize.y; l++)
                {
                    if (k >= 0 && k < size.x && l >= 0 && l < size.y)
                    {
                        tierColors[k + (l * size.x)] = tColor;
                    }
                }
            }
        }

        for (int m = 0; m < size.x; m++)
        {
            for (int n = 0; n < size.y; n++)
            {
                var index = m + (n * size.x);
                var colorIndex = gridColor[m, n];

                if ((colorIndex == 1 || colorIndex == 2) && tierColors[index].HasValue)
                {
                    var finalColor = tierColors[index].Value;
                    if (hoverSlot.x == m && hoverSlot.y == n)
                    {
                        finalColor.a = 0.2f;
                    }
                    else
                    {
                        finalColor.a = 0.1f;
                    }

                    gridSquare[index].color = finalColor;
                }
            }
        }
    }
}
