using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class FixVersatileCityShaders
{
    [MenuItem("Tools/Fix Versatile City Shaders")]
    public static void Execute()
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null) { Debug.LogError("URP Lit shader not found!"); return; }

        // Fix all materials in the Versatile Studio materials folder
        string matFolder = "Assets/Versatile Studio Assets/Demo City By Versatile Studio/Materials";
        string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { matFolder });
        int fixedCount = 0;

        foreach (string guid in matGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;

            string shaderName = mat.shader != null ? mat.shader.name : "";
            Debug.Log($"Material: {mat.name} | Shader: {shaderName}");

            if (!shaderName.StartsWith("Universal Render Pipeline") &&
                !shaderName.StartsWith("Shader Graphs"))
            {
                // Cache textures and properties before switching
                Texture mainTex    = mat.HasProperty("_MainTex")    ? mat.GetTexture("_MainTex")    : null;
                Texture bumpMap    = mat.HasProperty("_BumpMap")    ? mat.GetTexture("_BumpMap")    : null;
                Color   color      = mat.HasProperty("_Color")      ? mat.GetColor("_Color")        : Color.white;
                float   metallic   = mat.HasProperty("_Metallic")   ? mat.GetFloat("_Metallic")     : 0f;
                float   smoothness = mat.HasProperty("_Glossiness") ? mat.GetFloat("_Glossiness")   : 0.3f;

                mat.shader = urpLit;

                if (mainTex != null) mat.SetTexture("_BaseMap", mainTex);
                if (bumpMap != null) mat.SetTexture("_BumpMap", bumpMap);
                mat.SetColor("_BaseColor", color);
                mat.SetFloat("_Metallic",   Mathf.Clamp01(metallic));
                mat.SetFloat("_Smoothness", Mathf.Clamp01(smoothness));

                EditorUtility.SetDirty(mat);
                fixedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Fixed {fixedCount} Versatile Studio materials to URP Lit.");

        // Fix GameScene lighting
        var gameScene = EditorSceneManager.OpenScene(
            "Assets/Scenes/GameScene.unity", OpenSceneMode.Single);

        RenderSettings.ambientMode      = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1.5f;

        GameObject dirLight = GameObject.Find("Directional Light");
        if (dirLight != null)
        {
            Light l = dirLight.GetComponent<Light>();
            if (l != null) { l.intensity = 2.5f; l.color = new Color(1f, 0.95f, 0.85f); }
        }

        EditorSceneManager.MarkSceneDirty(gameScene);
        EditorSceneManager.SaveScene(gameScene, "Assets/Scenes/GameScene.unity");
        Debug.Log("Lighting fixed and scene saved.");
    }
}
