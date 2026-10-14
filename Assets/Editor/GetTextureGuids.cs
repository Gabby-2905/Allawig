using UnityEditor;
using UnityEngine;

public class GetTextureGuids
{
    public static void Execute()
    {
        string[] guids = AssetDatabase.FindAssets("GR_Dirt1", new[] { "Assets/Flooded_Grounds" });
        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            Debug.Log($"GUID: {g} | Path: {path}");
        }

        string[] guids2 = AssetDatabase.FindAssets("GR_Moss1", new[] { "Assets/Flooded_Grounds" });
        foreach (var g in guids2)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            Debug.Log($"GUID: {g} | Path: {path}");
        }
    }
}
