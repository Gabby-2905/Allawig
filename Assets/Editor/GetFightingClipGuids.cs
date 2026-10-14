using UnityEditor;
using UnityEngine;

public class GetFightingClipGuids
{
    public static void Execute()
    {
        string[] paths = new string[]
        {
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Combat/HumanM@CombatIdle01.fbx",
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Combat/1H/HumanM@Attack1H01_L.fbx",
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Combat/1H/HumanM@Attack1H01_R.fbx",
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Combat/HumanM@CombatDamage01.fbx",
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Combat/1H/HumanM@CombatIdle1H01.fbx",
        };

        foreach (var path in paths)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in assets)
            {
                if (a is AnimationClip clip && !clip.name.StartsWith("__"))
                {
                    string guid; long localId;
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(clip, out guid, out localId);
                    Debug.Log($"Clip: {clip.name} | GUID: {guid} | LocalID: {localId} | Path: {path}");
                }
            }
        }
    }
}
