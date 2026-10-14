using UnityEditor;
using UnityEngine;

public class GetCombatWalkGuids
{
    public static void Execute()
    {
        string[] paths = new string[]
        {
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Walk/HumanM@Walk01_Forward.fbx",
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Walk/HumanM@Walk01_Backward.fbx",
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Walk/HumanM@Walk01_Left.fbx",
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Walk/HumanM@Walk01_Right.fbx",
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Run/HumanM@Run01_Forward.fbx",
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Idles/HumanM@Idle01.fbx",
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Combat/HumanM@CombatIdle01.fbx",
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
