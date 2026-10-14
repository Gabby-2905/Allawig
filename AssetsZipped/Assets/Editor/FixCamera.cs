using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixCamera
{
    [MenuItem("Tools/Fix Camera")]
    public static void Execute()
    {
        var camGO = GameObject.Find("Main Camera");
        if (camGO == null) { Debug.LogError("Main Camera not found"); return; }

        var cam = camGO.GetComponent<Camera>();
        if (cam == null) { Debug.LogError("No Camera component"); return; }

        // Fix clear flags to Skybox so the sky renders
        cam.clearFlags = CameraClearFlags.Skybox;

        // Make sure background color is a nice sky blue as fallback
        cam.backgroundColor = new Color(0.49f, 0.71f, 0.91f, 1f);

        // Fix near clip plane - 0.3 is fine but let's make sure far is large enough
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 1000f;

        // Fix the GameCameraFollow - make sure target is assigned
        var follow = camGO.GetComponent<GameCameraFollow>();
        if (follow != null)
        {
            var player = GameObject.Find("Player");
            if (player != null)
            {
                follow.target = player.transform;
                Debug.Log("[FixCamera] Camera target set to Player.");
            }
        }

        // Fix URP camera data - make sure it uses skybox
        var urpData = camGO.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        if (urpData != null)
        {
            urpData.renderPostProcessing = false;
        }

        // Also fix the Rain particle system - it might be blocking the camera
        var rain = GameObject.Find("Rain");
        if (rain != null)
        {
            // Make sure rain follows the player/camera
            var rainFollower = rain.GetComponent<RainFollower>();
            if (rainFollower == null)
            {
                // Check if it has a script
                Debug.Log("[FixCamera] Rain object found at: " + rain.transform.position);
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[FixCamera] Camera clearFlags set to Skybox. Black screen should be fixed.");
    }
}
