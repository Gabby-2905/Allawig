using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class PreviewPause
{
    public static void ShowPanel()
    {
        var canvas = GameObject.Find("PauseCanvas");
        if (canvas == null) { Debug.LogWarning("PauseCanvas not found"); return; }
        var panel = canvas.transform.Find("Overlay/PausePanel");
        if (panel != null) panel.gameObject.SetActive(true);
    }

    public static void HidePanel()
    {
        var canvas = GameObject.Find("PauseCanvas");
        if (canvas == null) return;
        var panel = canvas.transform.Find("Overlay/PausePanel");
        if (panel != null) panel.gameObject.SetActive(false);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
