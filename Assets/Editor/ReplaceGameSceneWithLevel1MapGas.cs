using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Replaces the contents of GameScene with everything from Level1MapGas,
/// preserving GameScene's render/lighting settings from Level1MapGas as well.
/// </summary>
public class ReplaceGameSceneWithLevel1MapGas
{
    [MenuItem("Tools/Replace GameScene With Level1MapGas")]
    public static void Execute()
    {
        // 1. Open Level1MapGas as the source
        var sourceScene = EditorSceneManager.OpenScene(
            "Assets/Toon Gas Station/Scenes/Level1MapGas.unity",
            OpenSceneMode.Single);

        if (!sourceScene.IsValid())
        {
            Debug.LogError("[ReplaceGameScene] Could not open Level1MapGas.unity");
            return;
        }

        // 2. Collect all root objects from the source scene
        var sourceRoots = sourceScene.GetRootGameObjects();
        Debug.Log($"[ReplaceGameScene] Found {sourceRoots.Length} root objects in Level1MapGas.");

        // 3. Open GameScene additively so we can clear it and repopulate
        var gameScene = EditorSceneManager.OpenScene(
            "Assets/Scenes/GameScene.unity",
            OpenSceneMode.Additive);

        if (!gameScene.IsValid())
        {
            Debug.LogError("[ReplaceGameScene] Could not open GameScene.unity");
            return;
        }

        // 4. Delete all existing root objects in GameScene
        SceneManager.SetActiveScene(gameScene);
        foreach (var go in gameScene.GetRootGameObjects())
        {
            Object.DestroyImmediate(go);
        }

        // 5. Move every root object from Level1MapGas into GameScene
        SceneManager.SetActiveScene(sourceScene);
        foreach (var go in sourceScene.GetRootGameObjects())
        {
            EditorSceneManager.MoveGameObjectToScene(go, gameScene);
        }

        // 6. Save GameScene
        SceneManager.SetActiveScene(gameScene);
        EditorSceneManager.MarkSceneDirty(gameScene);
        bool saved = EditorSceneManager.SaveScene(gameScene, "Assets/Scenes/GameScene.unity");
        Debug.Log(saved
            ? "[ReplaceGameScene] GameScene saved successfully with Level1MapGas content."
            : "[ReplaceGameScene] WARNING: GameScene save failed.");

        // 8. Close the now-empty source scene and open GameScene alone
        EditorSceneManager.CloseScene(sourceScene, true);
        EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity", OpenSceneMode.Single);

        Debug.Log("[ReplaceGameScene] Done! GameScene now contains Level1MapGas content.");
    }
}
