using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixSkybox
{
    [MenuItem("Tools/Fix Skybox")]
    public static void Execute()
    {
        // Try to find an existing skybox material in the project
        string[] skyboxGuids = AssetDatabase.FindAssets("t:Material skybox", new[] { "Assets" });
        Material skyMat = null;

        foreach (var guid in skyboxGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null && mat.shader != null && mat.shader.name.Contains("Skybox"))
            {
                skyMat = mat;
                break;
            }
        }

        // If none found, create a procedural one and save it as an asset
        if (skyMat == null)
        {
            skyMat = new Material(Shader.Find("Skybox/Procedural"));
            skyMat.name = "DaySkybox";
            skyMat.SetFloat("_SunSize", 0.04f);
            skyMat.SetFloat("_SunSizeConvergence", 5f);
            skyMat.SetFloat("_AtmosphereThickness", 1.0f);
            skyMat.SetColor("_SkyTint", new Color(0.53f, 0.67f, 0.85f));
            skyMat.SetColor("_GroundColor", new Color(0.37f, 0.35f, 0.35f));
            skyMat.SetFloat("_Exposure", 1.3f);
            AssetDatabase.CreateAsset(skyMat, "Assets/DaySkybox.mat");
            AssetDatabase.SaveAssets();
        }

        RenderSettings.skybox = skyMat;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1.0f;
        RenderSettings.fog = false;

        // Fix directional light
        var dirLight = GameObject.Find("Directional Light");
        if (dirLight != null)
        {
            var light = dirLight.GetComponent<Light>();
            if (light != null)
            {
                light.intensity = 1.2f;
                light.color = new Color(1f, 0.96f, 0.84f);
                light.shadows = LightShadows.Soft;
            }
            dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        DynamicGI.UpdateEnvironment();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[FixSkybox] Skybox and lighting fixed.");
    }
}
