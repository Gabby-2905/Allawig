using UnityEngine;
using UnityEditor;

/// <summary>
/// Disables PausePanel and ControlsPanel in the GameScene so they
/// don't show on scene load. They should only appear when the player
/// presses Escape at runtime.
/// </summary>
public class FixGameSceneStartState : MonoBehaviour
{
    [MenuItem("Tools/Fix GameScene Start State")]
    public static void Execute()
    {
        // PausePanel
        GameObject pausePanel = GameObject.Find("PauseMenuCanvas/PausePanel");
        if (pausePanel == null)
        {
            // Try finding by name directly
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in allObjects)
            {
                if (go.name == "PausePanel" && go.scene.name == "GameScene")
                {
                    pausePanel = go;
                    break;
                }
            }
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            EditorUtility.SetDirty(pausePanel);
            Debug.Log("PausePanel set inactive.");
        }
        else Debug.LogWarning("PausePanel not found.");

        // ControlsPanel
        GameObject controlsPanel = GameObject.Find("PauseMenuCanvas/ControlsPanel");
        if (controlsPanel == null)
        {
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in allObjects)
            {
                if (go.name == "ControlsPanel" && go.scene.name == "GameScene")
                {
                    controlsPanel = go;
                    break;
                }
            }
        }

        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
            EditorUtility.SetDirty(controlsPanel);
            Debug.Log("ControlsPanel set inactive.");
        }
        else Debug.LogWarning("ControlsPanel not found.");

        // Save
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("✅ GameScene start state fixed — PausePanel and ControlsPanel are now hidden on load.");
    }
}
