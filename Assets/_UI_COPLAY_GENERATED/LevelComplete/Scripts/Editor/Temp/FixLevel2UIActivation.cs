using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixLevel2UIActivation
{
    public static void Execute()
    {
        // ── PauseCanvas ────────────────────────────────────────────────────
        // Canvas must be ACTIVE so PauseMenuManager.Update() runs (listens for Escape)
        // Only the PausePanel inside should start hidden
        var pauseCanvas = GameObject.Find("PauseCanvas");
        if (pauseCanvas == null)
        {
            // Try inactive
            var all = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
            foreach (var c in all)
                if (c.gameObject.name == "PauseCanvas") { pauseCanvas = c.gameObject; break; }
        }

        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(true);

            // Hide the PausePanel child (the actual menu panel)
            var pausePanel = pauseCanvas.transform.Find("Overlay/PausePanel");
            if (pausePanel != null)
            {
                pausePanel.gameObject.SetActive(false);
                Debug.Log("[FixLevel2UI] PausePanel hidden, PauseCanvas active.");
            }

            // Update PauseMenuManager reference
            var pmm = pauseCanvas.GetComponent<PauseMenuManager>();
            if (pmm != null)
            {
                pmm.pauseMenuPanel = pausePanel != null ? pausePanel.gameObject : null;
                EditorUtility.SetDirty(pmm);
            }

            EditorUtility.SetDirty(pauseCanvas);
        }
        else
        {
            Debug.LogWarning("[FixLevel2UI] PauseCanvas not found!");
        }

        // ── InventoryCanvas ────────────────────────────────────────────────
        // Canvas must be ACTIVE so InventoryUI.Update() runs (listens for Tab)
        // Only the InventoryRoot panel inside should start hidden
        var invCanvas = GameObject.Find("InventoryCanvas");
        if (invCanvas == null)
        {
            var all = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
            foreach (var c in all)
                if (c.gameObject.name == "InventoryCanvas") { invCanvas = c.gameObject; break; }
        }

        if (invCanvas != null)
        {
            invCanvas.SetActive(true);

            // Hide the InventoryRoot child
            var invRoot = invCanvas.transform.Find("Overlay/InventoryRoot");
            if (invRoot != null)
            {
                invRoot.gameObject.SetActive(false);
                Debug.Log("[FixLevel2UI] InventoryRoot hidden, InventoryCanvas active.");
            }

            // Update InventoryUI reference
            var invUI = invCanvas.GetComponent<InventoryUI>();
            if (invUI != null)
            {
                invUI.inventoryRoot = invRoot != null ? invRoot.gameObject : null;
                EditorUtility.SetDirty(invUI);
            }

            EditorUtility.SetDirty(invCanvas);
        }
        else
        {
            Debug.LogWarning("[FixLevel2UI] InventoryCanvas not found!");
        }

        // ── Save ───────────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FixLevel2UI] Done! Both canvases active, panels hidden.");
    }
}
