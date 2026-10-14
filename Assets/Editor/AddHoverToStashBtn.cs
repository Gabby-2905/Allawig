using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class AddHoverToStashBtn
{
    [MenuItem("Tools/Add Hover Effect to Stash Button")]
    public static void Execute()
    {
        // Find the ButtonPanel first, then iterate children
        GameObject buttonPanel = null;
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (Canvas c in canvases)
        {
            Transform t = c.transform.Find("ButtonPanel");
            if (t != null) { buttonPanel = t.gameObject; break; }
        }

        if (buttonPanel == null)
        {
            Debug.LogError("ButtonPanel not found.");
            return;
        }

        // Iterate children to find the stash button by name
        foreach (Transform child in buttonPanel.transform)
        {
            if (child.name.Contains("STASH") || child.name.Contains("INVENTORY"))
            {
                GameObject btn = child.gameObject;

                Outline outline = btn.GetComponent<Outline>();
                if (outline == null) outline = btn.AddComponent<Outline>();

                ButtonHoverEffect effect = btn.GetComponent<ButtonHoverEffect>();
                if (effect == null) effect = btn.AddComponent<ButtonHoverEffect>();

                Button button = btn.GetComponent<Button>();
                if (button != null)
                    button.transition = Selectable.Transition.None;

                EditorUtility.SetDirty(btn);
                Debug.Log($"ButtonHoverEffect added to: {btn.name}");
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
    }
}
