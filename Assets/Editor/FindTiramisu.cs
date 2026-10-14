using UnityEditor;
using UnityEngine;

public class FindTiramisu
{
    public static void Execute()
    {
        string[] guids = AssetDatabase.FindAssets("tiramisu", new[] { "Assets" });
        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            Debug.Log($"Found: {path}");

            // If it's an fbx, list its clips
            if (path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
            {
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var a in assets)
                {
                    if (a is AnimationClip clip && !clip.name.StartsWith("__"))
                    {
                        string guid; long localId;
                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(clip, out guid, out localId);
                        Debug.Log($"  Clip: {clip.name} | GUID: {guid} | LocalID: {localId}");
                    }
                }
            }
        }
    }
}
