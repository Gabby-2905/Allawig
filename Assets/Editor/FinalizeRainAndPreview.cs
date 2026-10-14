using UnityEngine;
using UnityEditor;

/// <summary>
/// Finalizes rain camera position and temporarily shows settings panel for preview.
/// </summary>
public class FinalizeRainAndPreview : MonoBehaviour
{
    [MenuItem("Tools/Finalize Rain + Show Settings Preview")]
    public static void Execute()
    {
        // ── Fix RainCamera position to face particles ──────────────────────
        GameObject rainCamGO = GameObject.Find("RainCamera");
        GameObject rainGO = GameObject.Find("RainParticles");

        if (rainCamGO != null)
        {
            // Position camera to look at the rain area
            rainCamGO.transform.position = new Vector3(0f, 0f, -5f);
            rainCamGO.transform.rotation = Quaternion.identity;

            Camera rainCam = rainCamGO.GetComponent<Camera>();
            if (rainCam != null)
            {
                rainCam.fieldOfView = 75f;
                rainCam.nearClipPlane = 0.1f;
                rainCam.farClipPlane = 100f;
                rainCam.orthographic = true;
                rainCam.orthographicSize = 6f;
            }
            EditorUtility.SetDirty(rainCamGO);
            Debug.Log("RainCamera positioned.");
        }

        if (rainGO != null)
        {
            // Center rain particles in front of camera
            rainGO.transform.position = new Vector3(0f, 4f, 5f);

            ParticleSystem ps = rainGO.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var shape = ps.shape;
                shape.scale = new Vector3(20f, 1f, 1f);
                shape.position = new Vector3(0f, 4f, 0f);

                var vel = ps.velocityOverLifetime;
                vel.y = new ParticleSystem.MinMaxCurve(-12f, -9f);
                vel.x = new ParticleSystem.MinMaxCurve(-0.5f, -0.2f);

                var main = ps.main;
                main.startLifetime = new ParticleSystem.MinMaxCurve(1.0f, 1.5f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.035f);
            }
            EditorUtility.SetDirty(rainGO);
            Debug.Log("RainParticles repositioned.");
        }

        // ── Show Settings Panel for preview ───────────────────────────────
        GameObject canvasGO = GameObject.Find("Canvas");
        if (canvasGO != null)
        {
            Transform settingsPanel = canvasGO.transform.Find("SettingsPanel");
            if (settingsPanel != null)
            {
                settingsPanel.gameObject.SetActive(true);
                EditorUtility.SetDirty(settingsPanel.gameObject);
                Debug.Log("SettingsPanel shown for preview.");
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("✅ Rain finalized.");
    }

    [MenuItem("Tools/Hide Settings Panel")]
    public static void HideSettings()
    {
        GameObject canvasGO = GameObject.Find("Canvas");
        if (canvasGO != null)
        {
            Transform settingsPanel = canvasGO.transform.Find("SettingsPanel");
            if (settingsPanel != null)
            {
                settingsPanel.gameObject.SetActive(false);
                EditorUtility.SetDirty(settingsPanel.gameObject);
            }
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("SettingsPanel hidden.");
    }
}
