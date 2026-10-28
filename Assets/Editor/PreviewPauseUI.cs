using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class PreviewPauseUI
{
    [MenuItem("Tools/Preview Pause UI")]
    public static void ShowPause()
    {
        GameObject pauseCanvas = GameObject.Find("PauseCanvas");
        if (pauseCanvas == null) return;
        Transform pp = pauseCanvas.transform.Find("PausePanel");
        if (pp != null) pp.gameObject.SetActive(true);
    }

    [MenuItem("Tools/Preview Settings UI")]
    public static void ShowSettings()
    {
        GameObject pauseCanvas = GameObject.Find("PauseCanvas");
        if (pauseCanvas == null) return;
        Transform sp = pauseCanvas.transform.Find("SettingsPanel");
        if (sp != null) sp.gameObject.SetActive(true);
    }

    [MenuItem("Tools/Preview Inventory UI")]
    public static void ShowInventory()
    {
        GameObject invCanvas = GameObject.Find("InventoryCanvas");
        if (invCanvas == null) return;
        Transform ir = invCanvas.transform.Find("InventoryRoot");
        if (ir != null) ir.gameObject.SetActive(true);
    }

    [MenuItem("Tools/Hide Preview UIs")]
    public static void HideAll()
    {
        GameObject pauseCanvas = GameObject.Find("PauseCanvas");
        if (pauseCanvas != null)
        {
            Transform pp = pauseCanvas.transform.Find("PausePanel");
            if (pp != null) pp.gameObject.SetActive(false);
            Transform sp = pauseCanvas.transform.Find("SettingsPanel");
            if (sp != null) sp.gameObject.SetActive(false);
            Transform cp = pauseCanvas.transform.Find("ControlsPanel");
            if (cp != null) cp.gameObject.SetActive(false);
        }
        GameObject invCanvas = GameObject.Find("InventoryCanvas");
        if (invCanvas != null)
        {
            Transform ir = invCanvas.transform.Find("InventoryRoot");
            if (ir != null) ir.gameObject.SetActive(false);
        }
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("[PreviewPauseUI] UIs hidden and scene saved.");
    }
}
