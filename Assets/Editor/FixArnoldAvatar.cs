using UnityEngine;
using UnityEditor;

public class FixArnoldAvatar
{
    public static void Execute()
    {
        string path = "Assets/Arnold/Arnold.fbx";
        var importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null)
        {
            Debug.LogError("[FixArnoldAvatar] Could not get ModelImporter for Arnold.fbx");
            return;
        }

        // Set to Humanoid rig
        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

        // Reimport
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();

        Debug.Log("[FixArnoldAvatar] Arnold.fbx reimported as Humanoid.");

        // Now assign the avatar to the Visual animator
        var avatar = GetAvatarFromFBX(path);
        if (avatar != null && avatar.isHuman)
        {
            var playerGO = GameObject.Find("Player");
            if (playerGO != null)
            {
                var visual = playerGO.transform.Find("Visual");
                if (visual != null)
                {
                    var animator = visual.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.avatar = avatar;
                        UnityEditor.EditorUtility.SetDirty(animator);
                        Debug.Log("[FixArnoldAvatar] Assigned humanoid avatar to Animator.");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("[FixArnoldAvatar] Avatar still not humanoid after reimport.");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
    }

    static Avatar GetAvatarFromFBX(string path)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (var a in assets)
            if (a is Avatar av) return av;
        return null;
    }
}
