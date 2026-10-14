using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Fixes all OlegWER RustyCars materials by upgrading them to URP Lit shader
/// and reassigning their textures properly.
/// </summary>
public class FixOlegWERMaterials : EditorWindow
{
    [MenuItem("Tools/Fix OlegWER Materials")]
    public static void Fix()
    {
        string matFolder = "Assets/OlegWER/RustyCarsFree/Materials";
        string texFolder = "Assets/OlegWER/RustyCarsFree/Textures";

        var matGuids = AssetDatabase.FindAssets("t:Material", new[] { matFolder });
        var urpLit = Shader.Find("Universal Render Pipeline/Lit");

        if (urpLit == null)
        {
            Debug.LogError("[FixOlegWERMaterials] URP Lit shader not found!");
            return;
        }

        int fixed_count = 0;
        foreach (var guid in matGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;

            // Check if shader is not URP
            if (mat.shader == null || !mat.shader.name.Contains("Universal Render Pipeline"))
            {
                // Cache existing textures before changing shader
                Texture mainTex = null;
                Texture bumpMap = null;
                Texture metallicMap = null;
                Color color = Color.white;

                if (mat.HasProperty("_MainTex"))   mainTex    = mat.GetTexture("_MainTex");
                if (mat.HasProperty("_BumpMap"))   bumpMap    = mat.GetTexture("_BumpMap");
                if (mat.HasProperty("_MetallicGlossMap")) metallicMap = mat.GetTexture("_MetallicGlossMap");
                if (mat.HasProperty("_Color"))     color      = mat.GetColor("_Color");

                mat.shader = urpLit;

                // Reassign textures to URP property names
                if (mainTex != null)    mat.SetTexture("_BaseMap", mainTex);
                if (bumpMap != null)    mat.SetTexture("_BumpMap", bumpMap);
                if (metallicMap != null) mat.SetTexture("_MetallicGlossMap", metallicMap);
                mat.SetColor("_BaseColor", color);

                EditorUtility.SetDirty(mat);
                fixed_count++;
                Debug.Log($"[FixOlegWERMaterials] Fixed: {mat.name}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[FixOlegWERMaterials] Done. Fixed {fixed_count} materials.");
    }

    [MenuItem("Tools/Diagnose OlegWER Shaders")]
    public static void Diagnose()
    {
        string matFolder = "Assets/OlegWER/RustyCarsFree/Materials";
        var matGuids = AssetDatabase.FindAssets("t:Material", new[] { matFolder });

        foreach (var guid in matGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;
            string shaderName = mat.shader != null ? mat.shader.name : "NULL";
            Debug.Log($"[OlegWER] {mat.name} => shader: {shaderName}");
        }
        Debug.Log("[DiagnoseOlegWER] Done.");
    }
}
