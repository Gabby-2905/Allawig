using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FindMagentaObjects : EditorWindow
{
    [MenuItem("Tools/Find Magenta Objects")]
    public static void Find()
    {
        // Check every renderer in the scene for any material using an error/hidden shader
        // OR any material whose shader keyword suggests it won't render in URP
        var renderers = GameObject.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        var reported = new HashSet<string>();

        foreach (var r in renderers)
        {
            if (r.sharedMaterials == null) continue;
            foreach (var mat in r.sharedMaterials)
            {
                if (mat == null)
                {
                    string key = GetPath(r.gameObject);
                    if (!reported.Contains(key))
                    {
                        Debug.Log($"[MAGENTA-NULL] {key}  pos={r.transform.position}");
                        reported.Add(key);
                    }
                    break;
                }

                string sn = mat.shader != null ? mat.shader.name : "NULL_SHADER";
                bool isBad = sn == "NULL_SHADER"
                    || sn.Contains("Hidden/")
                    || sn.Contains("Error")
                    || sn.Contains("Standard") // Built-in Standard won't render in URP
                    || sn.Contains("Legacy")
                    || (!sn.Contains("Universal") && !sn.Contains("Unlit") && !sn.Contains("Sprites") && !sn.Contains("UI") && !sn.Contains("Skybox") && !sn.Contains("Particles"));

                if (isBad)
                {
                    string key = GetPath(r.gameObject) + "|" + mat.name;
                    if (!reported.Contains(key))
                    {
                        Debug.Log($"[MAGENTA-SHADER] {GetPath(r.gameObject)}  shader={sn}  mat={mat.name}  pos={r.transform.position}");
                        reported.Add(key);
                    }
                    break;
                }
            }
        }
        Debug.Log($"[FindMagenta] Done. Found {reported.Count} issues.");
    }

    static string GetPath(GameObject go)
    {
        string p = go.name;
        var t = go.transform.parent;
        while (t != null) { p = t.name + "/" + p; t = t.parent; }
        return p;
    }
}
