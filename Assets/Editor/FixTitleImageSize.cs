using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FixTitleImageSize : MonoBehaviour
{
    [MenuItem("Tools/Fix Title Image Size")]
    public static void Execute()
    {
        GameObject canvasGO = GameObject.Find("Canvas");
        if (canvasGO == null) { Debug.LogError("Canvas not found!"); return; }

        Transform titleT = canvasGO.transform.Find("TitleALLAWIG");
        if (titleT == null) { Debug.LogError("TitleALLAWIG not found!"); return; }

        GameObject titleGO = titleT.gameObject;
        Image img = titleGO.GetComponent<Image>();
        RectTransform rt = titleGO.GetComponent<RectTransform>();

        if (img == null) { Debug.LogError("No Image component!"); return; }

        // Turn off preserveAspect — we'll set the rect to the exact aspect ratio
        img.preserveAspect = false;
        img.type = Image.Type.Simple;

        // Image is 819x447 pixels — aspect ratio 1.831
        // Display at a good size for the top-left corner of the menu
        // Use native size scaled down to fit nicely
        float displayHeight = 200f;
        float displayWidth = displayHeight * (819f / 447f); // = 366f

        // Actually let's make it bigger — use SetNativeSize then scale
        // Native size at 100 PPU would be 819x447 — too big
        // Let's use a fixed width of 550 and proportional height
        float targetWidth = 550f;
        float targetHeight = targetWidth * (447f / 819f); // = 300f

        rt.sizeDelta = new Vector2(targetWidth, targetHeight);
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(10f, -5f);

        EditorUtility.SetDirty(titleGO);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log($"✅ Title image sized to {targetWidth}x{targetHeight:F0} (preserveAspect OFF, exact 819:447 ratio)");
    }
}
