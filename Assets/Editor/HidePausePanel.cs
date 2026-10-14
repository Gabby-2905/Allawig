using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class HidePausePanel
{
    public static void Execute()
    {
        var panel = GameObject.Find("PauseMenuCanvas/PausePanel");
        if (panel != null)
        {
            panel.SetActive(false);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            Debug.Log("[HidePausePanel] PausePanel hidden and scene saved.");
        }
        else
        {
            Debug.LogWarning("[HidePausePanel] PausePanel not found.");
        }
    }
}
