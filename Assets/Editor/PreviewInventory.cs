using UnityEditor;
using UnityEngine;

public class PreviewInventory
{
    [MenuItem("Tools/Preview Inventory")]
    public static void Show()
    {
        var root = GameObject.Find("InventoryRoot");
        if (root != null) root.SetActive(true);
    }

    [MenuItem("Tools/Hide Inventory")]
    public static void Hide()
    {
        var root = GameObject.Find("InventoryRoot");
        if (root != null) root.SetActive(false);
    }
}
