using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor.SceneManagement;

public class UseDefaultRenderer
{
    [MenuItem("Tools/Use Default URP Renderer")]
    public static void Execute()
    {
        // Load the copied default UniversalRendererData
        AssetDatabase.Refresh();
        var defaultRenderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(
            "Assets/Settings/UniversalRendererData.asset");

        if (defaultRenderer == null)
        {
            Debug.LogError("[UseDefaultRenderer] Could not load UniversalRendererData.asset");
            return;
        }

        Debug.Log($"[UseDefaultRenderer] Loaded renderer: {defaultRenderer.name}");

        // Get the active URP asset
        var urpAsset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset == null) { Debug.LogError("No URP asset"); return; }

        // Replace index 0 with the proper default renderer
        var so = new SerializedObject(urpAsset);
        var rendererList = so.FindProperty("m_RendererDataList");
        rendererList.arraySize = 1; // Only keep one renderer
        var elem0 = rendererList.GetArrayElementAtIndex(0);
        elem0.objectReferenceValue = defaultRenderer;

        var defaultIndex = so.FindProperty("m_DefaultRendererIndex");
        if (defaultIndex != null) defaultIndex.intValue = 0;

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(urpAsset);
        AssetDatabase.SaveAssets();

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
            var urpCamData = camGO.GetComponent<UniversalAdditionalCameraData>();
            if (urpCamData != null)
            {
                var camSO = new SerializedObject(urpCamData);
                var rendererIndexProp = camSO.FindProperty("m_RendererIndex");
                if (rendererIndexProp != null)
                {
                    rendererIndexProp.intValue = -1;
                    camSO.ApplyModifiedProperties();
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[UseDefaultRenderer] Done! Using proper URP default renderer.");
    }
}
