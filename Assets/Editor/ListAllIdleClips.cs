using UnityEditor;
using UnityEngine;

public class ListAllIdleClips
{
    public static void Execute()
    {
        string[] guids = AssetDatabase.FindAssets("t:Object", new[] { "Assets" });
        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            if (!path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase) &&
                !path.EndsWith(".anim", System.StringComparison.OrdinalIgnoreCase)) continue;

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in assets)
            {
                if (a is AnimationClip clip && !clip.name.StartsWith("__"))
                {
                    string lower = clip.name.ToLower();
                    if (lower.Contains("idle") || lower.Contains("stance") || lower.Contains("combat"))
                    {
                        Debug.Log($"Clip: {clip.name} | Path: {path}");
                    }
                }
            }
        }
    }
}
