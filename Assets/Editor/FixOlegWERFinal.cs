using UnityEngine;
using UnityEditor;

/// <summary>
/// Final targeted fix for OlegWER materials:
/// - Mi_016: uses Particles/Lit (wrong for mesh) → switch to URP/Lit with T_010
/// - Mi_017: NULL BaseMap, near-black color → assign dark metal color
/// - Mi_018: NULL BaseMap, black color → assign dark rubber/tire color
/// Also fixes any remaining magenta by scanning all scene renderers.
/// </summary>
public class FixOlegWERFinal : EditorWindow
{
    [MenuItem("Tools/Fix OlegWER Final")]
    public static void Fix()
    {
        var urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null) { Debug.LogError("URP Lit not found!"); return; }

        // ── Fix Mi_016: Particles/Lit → URP/Lit ──────────────────────────
        var mi016 = AssetDatabase.LoadAssetAtPath<Material>("Assets/OlegWER/RustyCarsFree/Materials/Mi_016.mat");
        if (mi016 != null)
        {
            var t010 = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/OlegWER/RustyCarsFree/Textures/T_010.png");
            mi016.shader = urpLit;
            if (t010 != null) mi016.SetTexture("_BaseMap", t010);
            mi016.SetColor("_BaseColor", new Color(0.25f, 0.38f, 0.18f)); // dark green ivy
            mi016.SetFloat("_Metallic", 0f);
            mi016.SetFloat("_Smoothness", 0.1f);
            // Enable alpha clipping for leaf transparency
            mi016.SetFloat("_AlphaClip", 1f);
            mi016.SetFloat("_Cutoff", 0.4f);
            mi016.renderQueue = 2450;
            EditorUtility.SetDirty(mi016);
            Debug.Log("[FixOlegWERFinal] Fixed Mi_016 (ivy leaves)");
        }

        // ── Fix Mi_017: NULL BaseMap, near-black → dark glass/window ─────
        var mi017 = AssetDatabase.LoadAssetAtPath<Material>("Assets/OlegWER/RustyCarsFree/Materials/Mi_017.mat");
        if (mi017 != null)
        {
            mi017.shader = urpLit;
            mi017.SetColor("_BaseColor", new Color(0.08f, 0.10f, 0.12f, 0.6f)); // dark tinted glass
            mi017.SetFloat("_Metallic", 0.1f);
            mi017.SetFloat("_Smoothness", 0.7f);
            // Set surface type to transparent
            mi017.SetFloat("_Surface", 1f); // 1 = Transparent
            mi017.SetFloat("_Blend", 0f);   // Alpha blend
            mi017.renderQueue = 3000;
            EditorUtility.SetDirty(mi017);
            Debug.Log("[FixOlegWERFinal] Fixed Mi_017 (glass)");
        }

        // ── Fix Mi_018: NULL BaseMap, black → dark rubber/tire ────────────
        var mi018 = AssetDatabase.LoadAssetAtPath<Material>("Assets/OlegWER/RustyCarsFree/Materials/Mi_018.mat");
        if (mi018 != null)
        {
            mi018.shader = urpLit;
            mi018.SetColor("_BaseColor", new Color(0.06f, 0.06f, 0.06f)); // near-black rubber
            mi018.SetFloat("_Metallic", 0f);
            mi018.SetFloat("_Smoothness", 0.05f);
            EditorUtility.SetDirty(mi018);
            Debug.Log("[FixOlegWERFinal] Fixed Mi_018 (rubber/tires)");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ── Now scan ALL scene renderers for any remaining error shaders ──
        FixSceneRenderers(urpLit);

        Debug.Log("[FixOlegWERFinal] All done!");
    }

    static void FixSceneRenderers(Shader urpLit)
    {
        var renderers = GameObject.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        int count = 0;
        foreach (var r in renderers)
        {
            var mats = r.sharedMaterials;
            if (mats == null) continue;
            bool dirty = false;
            foreach (var mat in mats)
            {
                if (mat == null) continue;
                if (mat.shader == null || mat.shader.name.Contains("Error") ||
                    mat.shader.name.Contains("Hidden/Internal"))
                {
                    mat.shader = urpLit;
                    mat.SetColor("_BaseColor", new Color(0.4f, 0.3f, 0.2f));
                    EditorUtility.SetDirty(mat);
                    dirty = true;
                    count++;
                    Debug.Log($"[FixOlegWERFinal] Fixed error shader on: {r.gameObject.name} mat: {mat.name}");
                }
            }
        }
        if (count > 0) AssetDatabase.SaveAssets();
        Debug.Log($"[FixOlegWERFinal] Fixed {count} error-shader materials in scene.");
    }
}
