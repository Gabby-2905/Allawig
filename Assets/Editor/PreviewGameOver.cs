using UnityEngine;
using UnityEditor;

public class PreviewGameOver : MonoBehaviour
{
    [MenuItem("Tools/Show Game Over Panel (Preview)")]
    public static void Show()
    {
        var go = GameObject.Find("PauseMenuCanvas/GameOverPanel");
        if (go != null) { go.SetActive(true); EditorUtility.SetDirty(go); }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    [MenuItem("Tools/Hide Game Over Panel")]
    public static void Hide()
    {
        var go = GameObject.Find("PauseMenuCanvas/GameOverPanel");
        if (go != null) { go.SetActive(false); EditorUtility.SetDirty(go); }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
    }
}
