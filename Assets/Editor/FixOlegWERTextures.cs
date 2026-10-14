using UnityEngine;
using UnityEditor;

/// <summary>
/// Diagnoses and fixes OlegWER materials that have missing _BaseMap textures.
/// For materials with no texture, assigns a fallback rust-colored base color.
/// </summary>
public class FixOlegWERTextures : EditorWindow
{
    [MenuItem("Tools/Fix OlegWER Textures")]
    public static void Fix()
    {
        string matFolder = "Assets/OlegWER/RustyCarsFree/Materials";
        var matGuids = AssetDatabase.FindAssets("t:Material", new[] { matFolder });

        // Rust/metal color palette for fallback
        Color[] rustColors = {
            new Color(0.35f, 0.18f, 0.08f), // dark rust
            new Color(0.45f, 0.22f, 0.10f), // rust orange
            new Color(0.28f, 0.25f, 0.22f), // dark metal
            new Color(0.50f, 0.28f, 0.12f), // burnt orange
            new Color(0.20f, 0.18f, 0.16f), // charcoal
            new Color(0.40f, 0.35f, 0.28f), // weathered tan
        };

        int idx = 0;
        int fixed_count = 0;

        foreach (var guid in matGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;

            bool needsFix = false;

            // Check _BaseMap
            if (mat.HasProperty("_BaseMap"))
            {
                var tex = mat.GetTexture("_BaseMap");
                if (tex == null) needsFix = true;
            }

            // Check _BaseColor – if it's pure white with no texture, it renders magenta on error
            if (mat.HasProperty("_BaseColor"))
            {
                var col = mat.GetColor("_BaseColor");
                if (col == Color.white && !mat.HasProperty("_BaseMap")) needsFix = true;
            }

            if (needsFix)
            {
                Color fallback = rustColors[idx % rustColors.Length];
                mat.SetColor("_BaseColor", fallback);

                // Also set metallic/smoothness for a worn metal look
                if (mat.HasProperty("_Metallic"))    mat.SetFloat("_Metallic", 0.4f);
                if (mat.HasProperty("_Smoothness"))  mat.SetFloat("_Smoothness", 0.15f);

                EditorUtility.SetDirty(mat);
                Debug.Log($"[FixOlegWERTextures] Fixed color on: {mat.name} => {fallback}");
                fixed_count++;
                idx++;
            }
            else
            {
                // Even if texture exists, ensure BaseColor isn't pure white (which can look wrong)
                if (mat.HasProperty("_BaseColor"))
                {
                    var col = mat.GetColor("_BaseColor");
                    if (col.r > 0.95f && col.g > 0.95f && col.b > 0.95f)
                    {
                        mat.SetColor("_BaseColor", new Color(0.85f, 0.82f, 0.78f));
                        EditorUtility.SetDirty(mat);
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[FixOlegWERTextures] Done. Fixed {fixed_count} materials.");
    }

    [MenuItem("Tools/Diagnose OlegWER Textures")]
    public static void DiagnoseTextures()
    {
        string matFolder = "Assets/OlegWER/RustyCarsFree/Materials";
        var matGuids = AssetDatabase.FindAssets("t:Material", new[] { matFolder });

        foreach (var guid in matGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;

            string baseMap = "N/A";
            string baseColor = "N/A";

            if (mat.HasProperty("_BaseMap"))
            {
                var tex = mat.GetTexture("_BaseMap");
                baseMap = tex != null ? tex.name : "NULL";
            }
            if (mat.HasProperty("_BaseColor"))
                baseColor = mat.GetColor("_BaseColor").ToString();

            Debug.Log($"[OlegWER TEX] {mat.name} | BaseMap={baseMap} | BaseColor={baseColor}");
        }
        Debug.Log("[DiagnoseOlegWERTex] Done.");
    }
}
