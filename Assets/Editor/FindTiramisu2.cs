using UnityEditor;
using UnityEngine;

public class FindTiramisu2
{
    public static void Execute()
    {
        // Search all fbx files and check clip names for tiramisu
        string[] guids = AssetDatabase.FindAssets("t:Object", new[] { "Assets" });
        int found = 0;
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
                    if (clip.name.ToLower().Contains("tiramisu") || path.ToLower().Contains("tiramisu"))
                    {
                        string guid; long localId;
                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(clip, out guid, out localId);
                        Debug.Log($"MATCH Clip: {clip.name} | GUID: {guid} | LocalID: {localId} | Path: {path}");
                        found++;
                    }
                }
            }
        }
        if (found == 0)
            Debug.LogWarning("No 'tiramisu' animation clip found anywhere in Assets.");
        else
            Debug.Log($"Total found: {found}");
    }
}
