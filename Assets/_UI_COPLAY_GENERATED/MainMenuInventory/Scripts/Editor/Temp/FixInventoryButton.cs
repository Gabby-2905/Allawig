using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixInventoryButton
{
    public static void Execute()
    {
        // 1. Find MainMenuManager on Canvas
        var canvasGO = GameObject.Find("Canvas");
        if (canvasGO == null) { Debug.LogError("[FixInvBtn] Canvas not found!"); return; }

        var mmm = canvasGO.GetComponent<MainMenuManager>();
        if (mmm == null) { Debug.LogError("[FixInvBtn] MainMenuManager not found!"); return; }

        // 2. Find the InventoryPanel and assign it
        var invPanel = canvasGO.transform.Find("InventoryPanel");
        if (invPanel == null) { Debug.LogError("[FixInvBtn] InventoryPanel not found under Canvas!"); return; }

        mmm.inventoryPanel = invPanel.gameObject;
        EditorUtility.SetDirty(mmm);
        Debug.Log("[FixInvBtn] inventoryPanel assigned: " + invPanel.name);

        // 3. Find the STASH/INVENTORY button and wire onClick -> OpenInventory
        var btnPanel = canvasGO.transform.Find("ButtonPanel");
        if (btnPanel == null) { Debug.LogError("[FixInvBtn] ButtonPanel not found!"); return; }

        Button invBtn = null;
        foreach (Transform child in btnPanel)
        {
            if (child.name.Contains("INVENTORY") || child.name.Contains("STASH"))
            {
                invBtn = child.GetComponent<Button>();
                break;
            }
        }

        if (invBtn == null) { Debug.LogError("[FixInvBtn] Inventory button not found!"); return; }

        // Remove all existing listeners and add fresh one
        invBtn.onClick.RemoveAllListeners();
        UnityEditor.Events.UnityEventTools.RemovePersistentListener(invBtn.onClick, 0);

        // Clear all persistent listeners
        int count = invBtn.onClick.GetPersistentEventCount();
        for (int i = count - 1; i >= 0; i--)
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(invBtn.onClick, i);

        // Add OpenInventory as persistent listener
        var method = typeof(MainMenuManager).GetMethod("OpenInventory",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (method != null)
        {
            var action = (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(
                typeof(UnityEngine.Events.UnityAction), mmm, method);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(invBtn.onClick, action);
            EditorUtility.SetDirty(invBtn);
            Debug.Log("[FixInvBtn] onClick wired to OpenInventory on button: " + invBtn.name);
        }

        // 4. Save
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FixInvBtn] Done! Inventory button is now wired.");
    }
}
