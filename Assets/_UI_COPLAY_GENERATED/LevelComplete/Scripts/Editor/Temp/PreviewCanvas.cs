using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class PreviewCanvas
{
    public static void Execute()
    {
        SetActive("LevelCompleteCanvas", true);
    }

    public static void Hide()
    {
        SetActive("LevelCompleteCanvas", false);
    }

    public static void ShowPause()
    {
        SetActive("PauseCanvas", true);
    }

    public static void ShowInventory()
    {
        SetActive("InventoryCanvas", true);
    }

    public static void HideAll()
    {
        SetActive("PauseCanvas", false);
        SetActive("InventoryCanvas", false);
    }

    static void SetActive(string name, bool active)
    {
        var all = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        foreach (var c in all)
            if (c.gameObject.name == name)
                c.gameObject.SetActive(active);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
