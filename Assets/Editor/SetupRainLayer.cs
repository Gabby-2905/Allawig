using UnityEngine;
using UnityEditor;

/// <summary>
/// Creates a "Rain" layer, assigns RainParticles to it,
/// sets RainCamera to only render that layer,
/// and excludes it from the Main Camera.
/// </summary>
public class SetupRainLayer : MonoBehaviour
{
    [MenuItem("Tools/Setup Rain Layer")]
    public static void Execute()
    {
        // ── 1. Add "Rain" layer to TagManager ─────────────────────────────
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

        SerializedProperty layersProp = tagManager.FindProperty("layers");
        int rainLayerIndex = -1;

        // Check if Rain layer already exists
        for (int i = 8; i < layersProp.arraySize; i++)
        {
            SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(i);
            if (layerProp.stringValue == "Rain")
            {
                rainLayerIndex = i;
                Debug.Log($"Rain layer already exists at index {i}");
                break;
            }
        }

        // Find an empty slot and add it
        if (rainLayerIndex == -1)
        {
            for (int i = 8; i < layersProp.arraySize; i++)
            {
                SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(layerProp.stringValue))
                {
                    layerProp.stringValue = "Rain";
                    rainLayerIndex = i;
                    tagManager.ApplyModifiedProperties();
                    Debug.Log($"Created Rain layer at index {i}");
                    break;
                }
            }
        }

        if (rainLayerIndex == -1)
        {
            Debug.LogError("Could not create Rain layer — no empty slots!");
            return;
        }

        // ── 2. Assign RainParticles to Rain layer ──────────────────────────
        GameObject rainGO = GameObject.Find("RainParticles");
        if (rainGO != null)
        {
            rainGO.layer = rainLayerIndex;
            EditorUtility.SetDirty(rainGO);
            Debug.Log($"RainParticles assigned to layer {rainLayerIndex} (Rain)");
        }

        // ── 3. Set RainCamera to only render Rain layer ────────────────────
        GameObject rainCamGO = GameObject.Find("RainCamera");
        if (rainCamGO != null)
        {
            Camera rainCam = rainCamGO.GetComponent<Camera>();
            if (rainCam != null)
            {
                rainCam.cullingMask = 1 << rainLayerIndex;
                EditorUtility.SetDirty(rainCamGO);
                Debug.Log($"RainCamera cullingMask set to Rain layer only.");
            }
        }

        // ── 4. Exclude Rain layer from Main Camera ─────────────────────────
        GameObject mainCamGO = GameObject.Find("Main Camera");
        if (mainCamGO != null)
        {
            Camera mainCam = mainCamGO.GetComponent<Camera>();
            if (mainCam != null)
            {
                // Remove Rain layer from main camera's culling mask
                mainCam.cullingMask &= ~(1 << rainLayerIndex);
                EditorUtility.SetDirty(mainCamGO);
                Debug.Log("Main Camera now excludes Rain layer.");
            }
        }

        // ── 5. Save ────────────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log($"✅ Rain layer setup complete. Layer index: {rainLayerIndex}");
    }
}
