using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class PreviewPauseMenu
{
    public static void Execute()
    {
        // Show pause panel, hide others
        var pausePanel = FindInactive("PauseMenuCanvas/PausePanel");
        var settingsPanel = FindInactive("PauseMenuCanvas/SettingsPanel");
        var controlsPanel = FindInactive("PauseMenuCanvas/ControlsPanel");

        if (pausePanel != null) pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);

        // Frame the canvas in scene view
        var canvas = GameObject.Find("PauseMenuCanvas");
        if (canvas != null)
        {
            Selection.activeGameObject = canvas;
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    static GameObject FindInactive(string path)
    {
        var parts = path.Split('/');
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        GameObject current = null;
        foreach (var r in roots)
            if (r.name == parts[0]) { current = r; break; }
        if (current == null) return null;
        for (int i = 1; i < parts.Length; i++)
        {
            var t = current.transform.Find(parts[i]);
            if (t == null) return null;
            current = t.gameObject;
        }
        return current;
    }
}
