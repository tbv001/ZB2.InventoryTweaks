using System.Collections.Generic;
using UnityEngine;

namespace InventoryTweaks.Components;

public enum FirstSortBy
{
    Tier,
    SubType,
}

public class InventorySorter : MonoBehaviour
{
    public static PlayerInventory curInventory;
    public static bool showGui = false;
    public static bool dontSendNotifications = false;

    private void OnGUI()
    {
        if (!showGui || curInventory == null)
            return;

        var referenceWidth = 1920f;
        var referenceHeight = 1200f;
        var scaleX = Screen.width / referenceWidth;
        var scaleY = Screen.height / referenceHeight;

        GUI.matrix = Matrix4x4.TRS(
            Vector3.zero,
            Quaternion.identity,
            new Vector3(scaleX, scaleY, 1f)
        );

        if (GUI.Button(new Rect(875f, 175f, 150f, 50f), "Sort Inventory"))
        {
            SortItems();
        }
    }

    public static void SortItems()
    {
        if (curInventory == null || curInventory.storage == null)
            return;

        dontSendNotifications = true;
        var items = new List<InventoryItem>(curInventory.storage.items);

        items.Sort(
            (a, b) =>
            {
                var dbA = a.GetDataBaseItem();
                var dbB = b.GetDataBaseItem();

                var tierComp = dbB.tier.CompareTo(dbA.tier); // Highest tier first
                var typeComp = dbA.GetSubType().CompareTo(dbB.GetSubType()); // Subtype

                var sortByTier = InventoryTweaks.firstSortBy == FirstSortBy.Tier;
                var primary = sortByTier ? tierComp : typeComp;
                var secondary = sortByTier ? typeComp : tierComp;

                if (primary != 0)
                    return primary;
                if (secondary != 0)
                    return secondary;

                // Size largest first
                var areaComp = (dbB.size.x * dbB.size.y).CompareTo(dbA.size.x * dbA.size.y);
                if (areaComp != 0)
                    return areaComp;

                // Id smallest first
                return a.id.CompareTo(b.id);
            }
        );

        curInventory.storage.items.Clear();

        foreach (var item in items)
        {
            item.rotated = false;
            curInventory.AddItem(item, true, false, true);
        }

        InventoryDisplay.instance?.Show(curInventory);

        dontSendNotifications = false;
    }
}
