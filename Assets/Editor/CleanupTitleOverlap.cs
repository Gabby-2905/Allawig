using UnityEngine;
using UnityEditor;

public class CleanupTitleOverlap : MonoBehaviour
{
    [MenuItem("Tools/Cleanup Title Overlap")]
    public static void Execute()
    {
        GameObject canvasGO = GameObject.Find("Canvas");
        if (canvasGO == null) return;

        // Hide SubtitleLooter — the title image already has "LOOTER-SURVIVAL" in it
        Transform subtitle = canvasGO.transform.Find("SubtitleLooter");
        if (subtitle != null)
        {
            subtitle.gameObject.SetActive(false);
            EditorUtility.SetDirty(subtitle.gameObject);
            Debug.Log("SubtitleLooter hidden.");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("✅ Title cleanup done.");
    }
}
