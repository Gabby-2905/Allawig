using UnityEditor;
using UnityEngine;

public class FindAllFbxClips
{
    public static void Execute()
    {
        // Search all FBX files in the project
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { "Assets" });
        int count = 0;
        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            // Only show fbx files
            if (!path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) continue;
            
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in assets)
            {
                if (a is AnimationClip clip && !clip.name.StartsWith("__"))
                {
                    string lower = clip.name.ToLower();
                    if (lower.Contains("punch") || lower.Contains("fight") || lower.Contains("attack") ||
                        lower.Contains("hit") || lower.Contains("combat") || lower.Contains("kick") ||
                        lower.Contains("strike") || lower.Contains("action") || lower.Contains("idle"))
                    {
                        Debug.Log($"FOUND: {clip.name} | Path: {path}");
                        count++;
                    }
                }
            }
        }
        Debug.Log($"Total matching clips: {count}");
    }
}
