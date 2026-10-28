using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class PreviewCanvas
{
    public static void Execute()
    {
        // Remove all duplicates, keep only one
        var all = GameObject.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        GameObject toKeep = null;
        foreach (var c in all)
        {
            if (c.gameObject.name == "LevelCompleteCanvas")
            {
                if (toKeep == null)
                {
                    toKeep = c.gameObject;
                }
                else
                {
                    GameObject.DestroyImmediate(c.gameObject);
                }
            }
        }

        if (toKeep != null)
        {
            toKeep.SetActive(true);
            Debug.Log("[PreviewCanvas] Activated: " + toKeep.name + " children: " + toKeep.transform.childCount);
        }
        else
        {
            Debug.LogWarning("[PreviewCanvas] LevelCompleteCanvas not found!");
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    public static void Hide()
    {
        var all = GameObject.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in all)
        {
            if (c.gameObject.name == "LevelCompleteCanvas")
                c.gameObject.SetActive(false);
        }
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
