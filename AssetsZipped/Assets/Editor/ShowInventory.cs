using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ShowInventory
{
    public static void Execute()
    {
        // Find InventoryRoot including inactive
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var r in roots)
        {
            if (r.name == "InventoryCanvas")
            {
                var invRoot = r.transform.Find("InventoryRoot");
                if (invRoot != null)
                {
                    invRoot.gameObject.SetActive(true);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    Debug.Log("[ShowInventory] InventoryRoot shown.");
                }
                return;
            }
        }
        Debug.LogWarning("[ShowInventory] InventoryCanvas not found.");
    }
}
