using UnityEditor;
using UnityEngine;

public static class PreviewControlsPanel
{
    [MenuItem("Tools/Show Controls Panel for Preview")]
    public static void Show()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (go.name == "ControlsPanel") { go.SetActive(true); EditorUtility.SetDirty(go); break; }
            if (go.name == "PausePanel")    { go.SetActive(false); EditorUtility.SetDirty(go); }
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
    }

    [MenuItem("Tools/Hide Controls Panel After Preview")]
    public static void Hide()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (go.name == "ControlsPanel") { go.SetActive(false); EditorUtility.SetDirty(go); break; }
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
    }
}
