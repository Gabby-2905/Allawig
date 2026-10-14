using UnityEditor;
using UnityEngine;

public class GetFightingMotionsGuids
{
    public static void Execute()
    {
        string[] guids = AssetDatabase.FindAssets("t:Object", new[] { "Assets/FightingMotionsVolume1" });
        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            if (!path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) continue;

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in assets)
            {
                if (a is AnimationClip clip && !clip.name.StartsWith("__"))
                {
                    string clipGuid; long localId;
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(clip, out clipGuid, out localId);
                    Debug.Log($"Clip: {clip.name} | GUID: {clipGuid} | LocalID: {localId} | Path: {path}");
                }
            }
        }
    }
}
