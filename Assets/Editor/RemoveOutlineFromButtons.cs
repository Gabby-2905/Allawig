using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class RemoveOutlineFromButtons
{
    [MenuItem("Tools/Remove Outline Components from Menu Buttons")]
    public static void Execute()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        int removed = 0;
        foreach (Canvas canvas in canvases)
        {
            Transform bp = canvas.transform.Find("ButtonPanel");
            if (bp == null) continue;

            foreach (Transform child in bp)
            {
                Outline outline = child.GetComponent<Outline>();
                if (outline != null)
                {
                    Object.DestroyImmediate(outline);
                    EditorUtility.SetDirty(child.gameObject);
                    removed++;
                    Debug.Log($"Removed Outline from: {child.name}");
                }
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log($"Done. Removed {removed} Outline components.");
    }
}
