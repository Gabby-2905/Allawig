using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using UnityEditor.SceneManagement;

public class CreateForwardRenderer
{
    [MenuItem("Tools/Create 3D Forward Renderer")]
    public static void Execute()
    {
        // Create a new Universal Renderer Data (3D forward renderer)
        var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
        rendererData.name = "ForwardRenderer";

        // Save it as an asset
        string rendererPath = "Assets/Settings/ForwardRenderer.asset";
        AssetDatabase.CreateAsset(rendererData, rendererPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Load the URP pipeline asset and add the new renderer
        var urpAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
            "Assets/Settings/UniversalRP.asset");

        if (urpAsset == null)
        {
            Debug.LogError("Could not find UniversalRP.asset");
            return;
        }

        // Use SerializedObject to modify the renderer list
        var so = new SerializedObject(urpAsset);
        var rendererList = so.FindProperty("m_RendererDataList");

        // Add the new renderer at index 1
        rendererList.arraySize = 2;
        var newElement = rendererList.GetArrayElementAtIndex(1);
        newElement.objectReferenceValue = rendererData;

        so.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();

        // Set the camera to use the new 3D renderer (index 1)
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
                // Set renderer index to 1 (the new 3D forward renderer)
                var camSO = new SerializedObject(urpCamData);
                var rendererIndexProp = camSO.FindProperty("m_RendererIndex");
                if (rendererIndexProp != null)
                {
                    rendererIndexProp.intValue = 1;
                    camSO.ApplyModifiedProperties();
                    Debug.Log("[CreateForwardRenderer] Camera renderer index set to 1 (3D Forward Renderer).");
                }
            }
        }

        // Also set the default renderer index to 1 on the URP asset
        var urpSO = new SerializedObject(urpAsset);
        var defaultRendererIndex = urpSO.FindProperty("m_DefaultRendererIndex");
        if (defaultRendererIndex != null)
        {
            defaultRendererIndex.intValue = 1;
            urpSO.ApplyModifiedProperties();
        }
        AssetDatabase.SaveAssets();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[CreateForwardRenderer] 3D Forward Renderer created and assigned. Black screen should be fixed!");
    }
}
