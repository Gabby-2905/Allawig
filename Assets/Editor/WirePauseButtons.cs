using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public class WirePauseButtons
{
    public static void Execute()
    {
        // Find PauseCanvas and PauseMenuManager
        var pauseCanvas = GameObject.Find("PauseCanvas");
        if (pauseCanvas == null) { Debug.LogError("[WirePause] PauseCanvas not found!"); return; }

        var pmm = pauseCanvas.GetComponent<PauseMenuManager>();
        if (pmm == null) { Debug.LogError("[WirePause] PauseMenuManager not found on PauseCanvas!"); return; }

        // Button name -> method name mapping
        var map = new System.Collections.Generic.Dictionary<string, string>
        {
            { "Btn_RESUME",    "Resume"         },
            { "Btn_SAVEGAME",  "SaveGame"        },
            { "Btn_SETTINGS",  "OpenSettings"    },
            { "Btn_CONTROLS",  "OpenControls"    },
            { "Btn_MAINMENU",  "GoToMainMenu"    },
            { "Btn_QUIT",      "QuitToDesktop"   },
        };

        int wired = 0;
        // Search all buttons under PauseCanvas
        var allButtons = pauseCanvas.GetComponentsInChildren<Button>(true);
        foreach (var btn in allButtons)
        {
            string btnName = btn.gameObject.name;
            foreach (var kvp in map)
            {
                if (btnName.Equals(kvp.Key, System.StringComparison.OrdinalIgnoreCase) ||
                    btnName.ToUpper().Contains(kvp.Key.Replace("Btn_", "")))
                {
                    // Clear existing persistent listeners
                    int count = btn.onClick.GetPersistentEventCount();
                    for (int i = count - 1; i >= 0; i--)
                        UnityEditor.Events.UnityEventTools.RemovePersistentListener(btn.onClick, i);

                    // Add fresh persistent listener
                    var method = typeof(PauseMenuManager).GetMethod(kvp.Value,
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (method != null)
                    {
                        var action = (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(
                            typeof(UnityEngine.Events.UnityAction), pmm, method);
                        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, action);
                        EditorUtility.SetDirty(btn);
                        Debug.Log($"[WirePause] Wired '{btnName}' -> {kvp.Value}");
                        wired++;
                    }
                    break;
                }
            }
        }

        Debug.Log($"[WirePause] Total buttons wired: {wired}");
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
