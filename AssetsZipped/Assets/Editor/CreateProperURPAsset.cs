using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor.SceneManagement;

public class CreateProperURPAsset
{
    [MenuItem("Tools/Create Proper URP Asset")]
    public static void Execute()
    {
        // Use Unity's built-in method to create a proper URP asset with a 3D renderer
        // This ensures all shader references are properly set up
        var urpAsset = UniversalRenderPipelineAsset.Create();
        urpAsset.name = "UniversalRP_3D";

        // Save it
        string urpPath = "Assets/Settings/UniversalRP_3D.asset";
        AssetDatabase.CreateAsset(urpAsset, urpPath);

        // The Create() method also creates a renderer data - find it
        // It gets saved as a sub-asset or sibling
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Now set this as the active render pipeline in GraphicsSettings
        var so = new SerializedObject(GraphicsSettings.GetGraphicsSettings());
        var pipelineProp = so.FindProperty("m_CustomRenderPipeline");
        if (pipelineProp != null)
        {
            pipelineProp.objectReferenceValue = urpAsset;
            so.ApplyModifiedProperties();
            Debug.Log("[CreateProperURPAsset] Set new URP asset as active pipeline.");
        }

        // Also set in QualitySettings for all levels
        for (int i = 0; i < QualitySettings.count; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            // Can't easily set per-quality pipeline via script, GraphicsSettings is enough
        }

        // Fix camera
        var camGO = GameObject.Find("Main Camera");
        if (camGO != null)
        {
            var cam = camGO.GetComponent<Camera>();
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.Skybox;
                cam.backgroundColor = new Color(0.49f, 0.71f, 0.91f, 1f);
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[CreateProperURPAsset] Created new URP asset at {urpPath}.");
    }
}
