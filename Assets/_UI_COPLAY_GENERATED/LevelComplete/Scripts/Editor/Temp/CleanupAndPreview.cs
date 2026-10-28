using UnityEngine;
using UnityEditor;

public class CleanupAndPreview
{
    public static void Execute()
    {
        // Remove all LevelCompleteCanvas duplicates
        var all = GameObject.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in all)
        {
            if (c.gameObject.name == "LevelCompleteCanvas")
                Undo.DestroyObjectImmediate(c.gameObject);
        }

        // Rebuild once
        BuildLevelCompleteUI.Build();

        // Enable the canvas for preview
        var canvas = GameObject.Find("LevelCompleteCanvas");
        if (canvas != null)
            canvas.SetActive(true);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[CleanupAndPreview] Done - canvas enabled for preview.");
    }
}
