using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor.SceneManagement;

public class FindAndFixPipeline
{
    [MenuItem("Tools/Find And Fix Pipeline")]
    public static void Execute()
    {
        // Find the actual active render pipeline asset
        var activePipeline = GraphicsSettings.defaultRenderPipeline;
        Debug.Log($"[FindAndFixPipeline] Active pipeline: {activePipeline?.name} at {AssetDatabase.GetAssetPath(activePipeline)}");

        var urpAsset = activePipeline as UniversalRenderPipelineAsset;
        if (urpAsset == null)
        {
            Debug.LogError("[FindAndFixPipeline] Active pipeline is not a URP asset!");
            return;
        }

        // Check what renderers are in the pipeline
        var so = new SerializedObject(urpAsset);
        var rendererList = so.FindProperty("m_RendererDataList");
        Debug.Log($"[FindAndFixPipeline] Renderer count: {rendererList.arraySize}");

        for (int i = 0; i < rendererList.arraySize; i++)
        {
            var elem = rendererList.GetArrayElementAtIndex(i);
            var renderer = elem.objectReferenceValue;
            Debug.Log($"[FindAndFixPipeline] Renderer[{i}]: {renderer?.name} ({renderer?.GetType().Name}) at {AssetDatabase.GetAssetPath(renderer)}");
        }

        // Check if any renderer is a 2D renderer
        bool has3DRenderer = false;
        int firstRendererIndex = -1;

        for (int i = 0; i < rendererList.arraySize; i++)
        {
            var elem = rendererList.GetArrayElementAtIndex(i);
            var renderer = elem.objectReferenceValue;
            if (renderer is UniversalRendererData)
            {
                has3DRenderer = true;
                firstRendererIndex = i;
                Debug.Log($"[FindAndFixPipeline] Found 3D renderer at index {i}");
                break;
            }
        }

        if (!has3DRenderer)
        {
            Debug.Log("[FindAndFixPipeline] No 3D renderer found. Creating one...");

            // Create a new 3D Universal Renderer
            var newRenderer = ScriptableObject.CreateInstance<UniversalRendererData>();
            newRenderer.name = "UniversalRenderer";
            AssetDatabase.CreateAsset(newRenderer, "Assets/Settings/UniversalRenderer.asset");
            AssetDatabase.SaveAssets();

            // Replace the first renderer in the list with the new 3D one
            rendererList.arraySize = Mathf.Max(rendererList.arraySize, 1);
            var firstElem = rendererList.GetArrayElementAtIndex(0);
            firstElem.objectReferenceValue = newRenderer;
            so.ApplyModifiedProperties();

            var defaultIndex = so.FindProperty("m_DefaultRendererIndex");
            if (defaultIndex != null) defaultIndex.intValue = 0;
            so.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();
            firstRendererIndex = 0;
            Debug.Log("[FindAndFixPipeline] Created and assigned 3D Universal Renderer.");
        }

        // Fix the camera to use the 3D renderer
        var camGO = GameObject.Find("Main Camera");
        if (camGO != null)
        {
            var cam = camGO.GetComponent<Camera>();
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.Skybox;
                cam.backgroundColor = new Color(0.49f, 0.71f, 0.91f, 1f);
            }

            var urpCamData = camGO.GetComponent<UniversalAdditionalCameraData>();
            if (urpCamData != null)
            {
                var camSO = new SerializedObject(urpCamData);
                var rendererIndexProp = camSO.FindProperty("m_RendererIndex");
                if (rendererIndexProp != null)
                {
                    rendererIndexProp.intValue = firstRendererIndex;
                    camSO.ApplyModifiedProperties();
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[FindAndFixPipeline] Done!");
    }
}
