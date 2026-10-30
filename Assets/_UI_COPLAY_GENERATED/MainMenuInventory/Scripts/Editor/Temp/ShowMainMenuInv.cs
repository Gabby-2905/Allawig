using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ShowMainMenuInv
{
    public static void Show()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) { Debug.LogError("Canvas not found"); return; }
        var panel = canvas.transform.Find("InventoryPanel");
        if (panel != null)
        {
            panel.gameObject.SetActive(true);
            Debug.Log("[ShowMainMenuInv] InventoryPanel activated. Children: " + panel.childCount);
        }
        else Debug.LogError("[ShowMainMenuInv] InventoryPanel not found under Canvas!");
    }

    public static void Hide()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        var panel = canvas.transform.Find("InventoryPanel");
        if (panel != null) panel.gameObject.SetActive(false);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
