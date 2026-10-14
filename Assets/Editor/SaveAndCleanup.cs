using UnityEditor;
using UnityEngine;

public class SaveAndCleanup
{
    public static void Execute()
    {
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("Scene saved.");
    }
}
