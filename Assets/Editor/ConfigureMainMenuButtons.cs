using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Configures all main-menu buttons:
///  - Sets per-button hover fill color (blue for most, red for Quit)
///  - Wires onClick listeners to MainMenuManager methods
/// </summary>
public static class ConfigureMainMenuButtons
{
    [MenuItem("Tools/Configure Main Menu Buttons")]
    public static void Execute()
    {
        // ── Find MainMenuManager ───────────────────────────────────────────
        MainMenuManager manager = Object.FindAnyObjectByType<MainMenuManager>();
        if (manager == null)
        {
            // Try to find the Canvas and add it
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                manager = canvas.gameObject.AddComponent<MainMenuManager>();
                Debug.Log("Added MainMenuManager to Canvas.");
            }
            else
            {
                Debug.LogError("No Canvas found in scene.");
                return;
            }
        }

        // ── Find ButtonPanel ───────────────────────────────────────────────
        Transform buttonPanel = null;
        foreach (Canvas c in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            Transform t = c.transform.Find("ButtonPanel");
            if (t != null) { buttonPanel = t; break; }
        }

        if (buttonPanel == null)
        {
            Debug.LogError("ButtonPanel not found.");
            return;
        }

        // Blue tint for normal buttons
        Color blueHover = new Color(0.2f, 0.5f, 1f, 0.25f);
        // Red tint for Quit
        Color redHover  = new Color(0.9f, 0.1f, 0.1f, 0.30f);

        // ── Configure each button ──────────────────────────────────────────
        foreach (Transform child in buttonPanel)
        {
            Button btn = child.GetComponent<Button>();
            ButtonHoverEffect hover = child.GetComponent<ButtonHoverEffect>();
            if (btn == null || hover == null) continue;

            string n = child.name;

            // Set fill color
            if (n.Contains("QUIT"))
                hover.fillColor = redHover;
            else
                hover.fillColor = blueHover;

            // Clear all existing persistent listeners
            btn.onClick.RemoveAllListeners();
            int listenerCount = btn.onClick.GetPersistentEventCount();
            for (int i = listenerCount - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(btn.onClick, i);

            if (n.Contains("START"))
            {
                UnityAction action = manager.StartNewGame;
                UnityEventTools.AddPersistentListener(btn.onClick, action);
                Debug.Log($"Wired {n} → StartNewGame");
            }
            else if (n.Contains("CONTINUE"))
            {
                UnityAction action = manager.ContinueGame;
                UnityEventTools.AddPersistentListener(btn.onClick, action);
                Debug.Log($"Wired {n} → ContinueGame");
            }
            else if (n.Contains("STASH") || n.Contains("INVENTORY"))
            {
                UnityAction action = manager.OpenInventory;
                UnityEventTools.AddPersistentListener(btn.onClick, action);
                Debug.Log($"Wired {n} → OpenInventory");
            }
            else if (n.Contains("SETTINGS"))
            {
                UnityAction action = manager.OpenSettings;
                UnityEventTools.AddPersistentListener(btn.onClick, action);
                Debug.Log($"Wired {n} → OpenSettings");
            }
            else if (n.Contains("QUIT"))
            {
                UnityAction action = manager.QuitGame;
                UnityEventTools.AddPersistentListener(btn.onClick, action);
                Debug.Log($"Wired {n} → QuitGame");
            }

            EditorUtility.SetDirty(child.gameObject);
        }

        EditorUtility.SetDirty(manager.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("Main menu buttons configured and scene saved.");
    }
}
