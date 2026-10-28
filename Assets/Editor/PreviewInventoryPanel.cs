using UnityEngine;
using UnityEditor;

public class PreviewInventoryPanel
{
    public static void Show()
    {
        GameObject panel = GameObject.Find("Canvas/InventoryPanel") ??
                           GameObject.Find("InventoryPanel");
        if (panel != null) panel.SetActive(true);
    }

    public static void Hide()
    {
        GameObject panel = GameObject.Find("Canvas/InventoryPanel") ??
                           GameObject.Find("InventoryPanel");
        if (panel != null) panel.SetActive(false);
    }
}
