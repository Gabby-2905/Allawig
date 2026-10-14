using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class AddGameSaveManager
{
    [MenuItem("Tools/Add GameSaveManager to GameScene")]
    public static void Execute()
    {
        // Open GameScene additively to modify it
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity", OpenSceneMode.Single);

        // Check if it already exists
        GameSaveManager existing = Object.FindAnyObjectByType<GameSaveManager>();
        if (existing != null)
        {
            Debug.Log("GameSaveManager already exists in GameScene.");
            EditorSceneManager.SaveScene(scene);
            return;
        }

        GameObject go = new GameObject("GameSaveManager");
        go.AddComponent<GameSaveManager>();
        EditorUtility.SetDirty(go);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("GameSaveManager added to GameScene and saved.");
    }
}
