using UnityEditor;
using UnityEngine;

public class GetAnimGuids
{
    public static void Execute()
    {
        string[] paths = new string[]
        {
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Idles/HumanM@Idle01.fbx",
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Walk/HumanM@Walk01_Forward.fbx",
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Run/HumanM@Run01_Forward.fbx",
        };

        foreach (var path in paths)
        {
            string guid = AssetDatabase.AssetPathToGUID(path);
            // Get all sub-assets (animation clips inside fbx)
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in assets)
            {
                if (a is AnimationClip clip && !clip.name.StartsWith("__"))
                {
                    string clipGuid = AssetDatabase.AssetPathToGUID(path);
                    long localId;
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(clip, out clipGuid, out localId);
                    Debug.Log($"Clip: {clip.name} | GUID: {clipGuid} | LocalID: {localId} | Path: {path}");
                }
            }
        }
    }
}
