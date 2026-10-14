using UnityEditor;
using UnityEngine;

public class GetCombatAnimGuids
{
    public static void Execute()
    {
        // Check what clips are inside the Talk fbx (closest to a punch gesture)
        string[] paths = new string[]
        {
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Social/Conversation/HumanM@Talk01.fbx",
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Sprint/HumanM@Sprint01_Forward.fbx",
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Idles/HumanM@Idle01.fbx",
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
                    Debug.Log($"Clip: {clip.name} | GUID: {guid} | LocalID: {localId}");
                }
            }
        }
    }
}
