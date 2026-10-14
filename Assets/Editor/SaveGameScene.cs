using UnityEditor;
using UnityEditor.SceneManagement;

public class SaveGameScene
{
    public static void Execute()
    {
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
    }
}
