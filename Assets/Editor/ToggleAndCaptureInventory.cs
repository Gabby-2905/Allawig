using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToggleAndCaptureInventory
{
    [MenuItem("Tools/Enable Inventory Root For Preview")]
    public static void EnableForPreview()
    {
        // Find in currently open scene
        var allGOs = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var go in allGOs)
        {
            if (go.name == "InventoryRoot")
            {
                go.SetActive(true);
                EditorUtility.SetDirty(go);
                Debug.Log("[ToggleInventory] InventoryRoot set active: " + go.activeInHierarchy);
                break;
            }
        }
        // Also check inactive objects
        var invCanvas = GameObject.Find("InventoryCanvas");
        if (invCanvas != null)
        {
            var rt = invCanvas.transform.Find("InventoryRoot");
            if (rt != null)
            {
                rt.gameObject.SetActive(true);
                EditorUtility.SetDirty(rt.gameObject);
                Debug.Log("[ToggleInventory] Found via transform: " + rt.gameObject.activeInHierarchy);
            }
        }

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
    }

    [MenuItem("Tools/Disable Inventory Root After Preview")]
    public static void DisableAfterPreview()
    {
        var invCanvas = GameObject.Find("InventoryCanvas");
        if (invCanvas != null)
        {
            var rt = invCanvas.transform.Find("InventoryRoot");
            if (rt != null)
            {
                rt.gameObject.SetActive(false);
                EditorUtility.SetDirty(rt.gameObject);
            }
        }
        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[ToggleInventory] InventoryRoot hidden and scene saved.");
    }
}
