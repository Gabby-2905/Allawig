using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One-shot editor utility: adds ButtonHoverEffect (and its required Outline)
/// to every button in the Main Menu ButtonPanel.
/// </summary>
public static class AddButtonHoverEffect
{
    [MenuItem("Tools/Add Button Hover Effect to Main Menu Buttons")]
    public static void Execute()
    {
        string[] buttonPaths = new[]
        {
            "Canvas/ButtonPanel/START_NEW_GAME_Btn",
            "Canvas/ButtonPanel/CONTINUE_Btn",
            "Canvas/ButtonPanel/STASH_/_INVENTORY_Btn",
            "Canvas/ButtonPanel/SETTINGS_Btn",
            "Canvas/ButtonPanel/QUIT_Btn"
        };

        // Find the root canvas in the active scene
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        if (canvases.Length == 0)
        {
            Debug.LogError("No Canvas found in the active scene.");
            return;
        }

        int count = 0;
        foreach (string path in buttonPaths)
        {
            // Search all canvases for the button
            GameObject btn = null;
            foreach (Canvas canvas in canvases)
            {
                Transform t = canvas.transform.Find(path.Substring(path.IndexOf('/') + 1));
                if (t != null) { btn = t.gameObject; break; }
            }

            if (btn == null)
            {
                Debug.LogWarning($"Button not found at path: {path}");
                continue;
            }

            // Ensure Outline exists (required by ButtonHoverEffect)
            Outline outline = btn.GetComponent<Outline>();
            if (outline == null)
                outline = btn.AddComponent<Outline>();

            // Add ButtonHoverEffect if not already present
            ButtonHoverEffect effect = btn.GetComponent<ButtonHoverEffect>();
            if (effect == null)
                effect = btn.AddComponent<ButtonHoverEffect>();

            // Set the Button transition to None so it doesn't fight our effect
            UnityEngine.UI.Button button = btn.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
                button.transition = Selectable.Transition.None;

            EditorUtility.SetDirty(btn);
            count++;
        }

        // Save the scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log($"ButtonHoverEffect added to {count} buttons and scene saved.");
    }
}
