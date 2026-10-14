using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Replaces the TitleALLAWIG TextMeshPro text with the Allawig title image asset.
/// </summary>
public class ReplaceAllaWigTitle : MonoBehaviour
{
    [MenuItem("Tools/Replace Allawig Title With Image")]
    public static void Execute()
    {
        GameObject canvasGO = GameObject.Find("Canvas");
        if (canvasGO == null) { Debug.LogError("Canvas not found!"); return; }

        Transform titleT = canvasGO.transform.Find("TitleALLAWIG");
        if (titleT == null) { Debug.LogError("TitleALLAWIG not found!"); return; }

        GameObject titleGO = titleT.gameObject;

        // ── Load the Allawig title sprite ──────────────────────────────────
        // Import as sprite first
        string texPath = "Assets/Allawig title.png";
        TextureImporter importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
            Debug.Log("Reimported Allawig title.png as Sprite.");
        }

        Sprite titleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);
        if (titleSprite == null)
        {
            Debug.LogError($"Could not load sprite at {texPath}");
            return;
        }

        // ── Remove TMP component ───────────────────────────────────────────
        TMPro.TextMeshProUGUI tmp = titleGO.GetComponent<TMPro.TextMeshProUGUI>();
        if (tmp != null)
        {
            DestroyImmediate(tmp);
            Debug.Log("Removed TextMeshProUGUI from TitleALLAWIG.");
        }

        // Remove CanvasRenderer if present (will be re-added by Image)
        CanvasRenderer cr = titleGO.GetComponent<CanvasRenderer>();
        // Keep CanvasRenderer — Image needs it

        // ── Add Image component with the title sprite ──────────────────────
        Image img = titleGO.GetComponent<Image>();
        if (img == null) img = titleGO.AddComponent<Image>();

        img.sprite = titleSprite;
        img.color = Color.white;
        img.preserveAspect = true;
        img.raycastTarget = false;

        // ── Resize to fit the image nicely ─────────────────────────────────
        RectTransform rt = titleGO.GetComponent<RectTransform>();

        // Get natural size of the image
        float naturalW = titleSprite.rect.width;
        float naturalH = titleSprite.rect.height;
        float aspect = naturalW / naturalH;

        // Keep height at ~130, scale width proportionally
        float targetHeight = 160f;
        float targetWidth = targetHeight * aspect;

        rt.sizeDelta = new Vector2(targetWidth, targetHeight);
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(18f, -10f);

        EditorUtility.SetDirty(titleGO);

        // ── Save ───────────────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log($"✅ TitleALLAWIG replaced with image. Size: {targetWidth:F0}x{targetHeight:F0}");
    }
}
