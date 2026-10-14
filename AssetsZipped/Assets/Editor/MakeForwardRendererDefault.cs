using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor.SceneManagement;

public class MakeForwardRendererDefault
{
    [MenuItem("Tools/Make Forward Renderer Default")]
    public static void Execute()
    {
        var urpAsset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset == null) { Debug.LogError("No URP asset found"); return; }

        var so = new SerializedObject(urpAsset);
        var rendererList = so.FindProperty("m_RendererDataList");

        // Swap renderers: put ForwardRenderer at index 0, Renderer2D at index 1
        if (rendererList.arraySize >= 2)
        {
            var elem0 = rendererList.GetArrayElementAtIndex(0);
            var elem1 = rendererList.GetArrayElementAtIndex(1);

            var r0 = elem0.objectReferenceValue;
            var r1 = elem1.objectReferenceValue;

            // Put 3D renderer first
            if (r0 is Renderer2DData && r1 is UniversalRendererData)
            {
                elem0.objectReferenceValue = r1; // ForwardRenderer at 0
                elem1.objectReferenceValue = r0; // Renderer2D at 1
                Debug.Log("[MakeForwardRendererDefault] Swapped: ForwardRenderer is now at index 0.");
            }
            else if (r0 is UniversalRendererData)
            {
                Debug.Log("[MakeForwardRendererDefault] ForwardRenderer already at index 0.");
            }
        }

        // Set default renderer index to 0
        var defaultIndex = so.FindProperty("m_DefaultRendererIndex");
        if (defaultIndex != null)
        {
            defaultIndex.intValue = 0;
            Debug.Log("[MakeForwardRendererDefault] Default renderer index set to 0.");
        }

        so.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();

        // Set camera to use default renderer (-1) or explicitly 0
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
                    rendererIndexProp.intValue = -1; // Use default (which is now the 3D renderer)
                    camSO.ApplyModifiedProperties();
                    Debug.Log("[MakeForwardRendererDefault] Camera set to use default renderer.");
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[MakeForwardRendererDefault] Done! 3D Forward Renderer is now the default.");
    }
}
