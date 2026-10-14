using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ReplaceWithArnold
{
    public static void Execute()
    {
        // ---- Load Arnold prefab/model ----
        var arnoldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Arnold/Arnold.fbx");
        if (arnoldPrefab == null)
        {
            Debug.LogError("[ReplaceWithArnold] Arnold.fbx not found!");
            return;
        }

        // ---- Find Player/Visual ----
        var playerGO = GameObject.Find("Player");
        if (playerGO == null) { Debug.LogError("Player not found!"); return; }

        Transform visualTransform = playerGO.transform.Find("Visual");
        if (visualTransform == null) { Debug.LogError("Player/Visual not found!"); return; }
        GameObject visualGO = visualTransform.gameObject;

        // ---- Remove old character mesh (Character_Male_Hoodie) ----
        Transform oldChar = visualTransform.Find("Character_Male_Hoodie");
        if (oldChar != null)
        {
            Object.DestroyImmediate(oldChar.gameObject);
            Debug.Log("[ReplaceWithArnold] Removed Character_Male_Hoodie.");
        }

        // Also remove any existing Arnold instance
        Transform existingArnold = visualTransform.Find("Arnold");
        if (existingArnold != null)
            Object.DestroyImmediate(existingArnold.gameObject);

        // ---- Instantiate Arnold under Visual ----
        GameObject arnoldGO = (GameObject)PrefabUtility.InstantiatePrefab(arnoldPrefab, visualTransform);
        arnoldGO.name = "Arnold";

        // ---- Position & scale ----
        arnoldGO.transform.localPosition = Vector3.zero;
        arnoldGO.transform.localRotation = Quaternion.identity;

        // Check bounds to auto-scale if needed
        // Arnold.fbx is typically in cm scale from some exporters - check and fix
        var renderers = arnoldGO.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            float height = b.size.y;
            Debug.Log($"[ReplaceWithArnold] Arnold raw height: {height}");

            // Target ~1.8 units tall (matching CharacterController height)
            if (height > 10f)
            {
                float scale = 1.8f / height;
                arnoldGO.transform.localScale = Vector3.one * scale;
                Debug.Log($"[ReplaceWithArnold] Scaled Arnold by {scale}");
            }
            else if (height < 0.5f)
            {
                float scale = 1.8f / height;
                arnoldGO.transform.localScale = Vector3.one * scale;
                Debug.Log($"[ReplaceWithArnold] Scaled Arnold up by {scale}");
            }
        }

        // ---- Get Arnold's avatar ----
        var arnoldAvatar = GetAvatarFromFBX("Assets/Arnold/Arnold.fbx");

        // ---- Set up Animator on Visual ----
        var animator = visualGO.GetComponent<Animator>();
        if (animator == null)
            animator = visualGO.AddComponent<Animator>();

        // Assign controller
        var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/animations/PlayerController.controller");
        animator.runtimeAnimatorController = controller;

        // Assign Arnold's avatar if humanoid
        if (arnoldAvatar != null && arnoldAvatar.isHuman)
        {
            animator.avatar = arnoldAvatar;
            Debug.Log("[ReplaceWithArnold] Assigned Arnold humanoid avatar.");
        }
        else
        {
            Debug.LogWarning("[ReplaceWithArnold] Arnold avatar not humanoid or not found - animations may not retarget.");
        }

        // ---- Wire animator on GamePlayerMovement ----
        var movement = playerGO.GetComponent<GamePlayerMovement>();
        if (movement != null)
        {
            movement.animator = animator;
            EditorUtility.SetDirty(movement);
            Debug.Log("[ReplaceWithArnold] Wired animator to GamePlayerMovement.");
        }

        EditorUtility.SetDirty(visualGO);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        Debug.Log("[ReplaceWithArnold] Done!");
    }

    static Avatar GetAvatarFromFBX(string path)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (var a in assets)
            if (a is Avatar av) return av;
        return null;
    }
}
