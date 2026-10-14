using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class DiagnoseJunkyard : EditorWindow
{
    [MenuItem("Tools/Diagnose Junkyard Magenta")]
    public static void Diagnose()
    {
        var allObjects = GameObject.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (var r in allObjects)
        {
            if (r.sharedMaterials == null) continue;
            foreach (var mat in r.sharedMaterials)
            {
                if (mat == null)
                {
                    Debug.Log($"[MAGENTA] NULL mat on: {GetPath(r.gameObject)}");
                    break;
                }
                if (mat.shader != null && mat.shader.name.Contains("Error"))
                {
                    Debug.Log($"[MAGENTA] Error shader on: {GetPath(r.gameObject)} | mat: {mat.name}");
                    break;
                }
            }
        }
        Debug.Log("[DiagnoseJunkyard] Done.");
    }

    static string GetPath(GameObject go)
    {
        string path = go.name;
        var t = go.transform.parent;
        while (t != null) { path = t.name + "/" + path; t = t.parent; }
        return path;
    }
}
