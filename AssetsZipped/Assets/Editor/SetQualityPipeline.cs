using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor.SceneManagement;

public class SetQualityPipeline
{
    [MenuItem("Tools/Set Quality Pipeline")]
    public static void Execute()
    {
        var newUrp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
            "Assets/Settings/UniversalRP_3D.asset");
        if (newUrp == null) { Debug.LogError("UniversalRP_3D.asset not found"); return; }

        // Update all quality levels in QualitySettings.asset directly
        var qsSO = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset")[0]);
        var qualitySettingsArray = qsSO.FindProperty("m_QualitySettings");
        if (qualitySettingsArray != null)
        {
            for (int i = 0; i < qualitySettingsArray.arraySize; i++)
            {
                var level = qualitySettingsArray.GetArrayElementAtIndex(i);
                var pipeline = level.FindPropertyRelative("customRenderPipeline");
                if (pipeline != null)
                {
                    pipeline.objectReferenceValue = newUrp;
                }
            }
            qsSO.ApplyModifiedProperties();
            Debug.Log($"[SetQualityPipeline] Updated {qualitySettingsArray.arraySize} quality levels.");
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[SetQualityPipeline] Done!");
    }
}
