using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FixTitleSprite : MonoBehaviour
{
    [MenuItem("Tools/Fix Title Sprite Import")]
    public static void Execute()
    {
        string texPath = "Assets/Allawig title.png";

        // ── Fix importer settings ──────────────────────────────────────────
        TextureImporter importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
        if (importer == null) { Debug.LogError("Importer not found!"); return; }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.spritePivot = new Vector2(0.5f, 0.5f);
        importer.spritePixelsPerUnit = 100f;

        // Force full rect — no cropping
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteAlignment = (int)SpriteAlignment.Center;
        importer.SetTextureSettings(settings);

        // Set max texture size large enough
        TextureImporterPlatformSettings platformSettings = importer.GetDefaultPlatformTextureSettings();
        platformSettings.maxTextureSize = 2048;
        platformSettings.format = TextureImporterFormat.RGBA32;
        importer.SetPlatformTextureSettings(platformSettings);

        importer.SaveAndReimport();
        AssetDatabase.Refresh();

        Debug.Log("Reimported Allawig title.png with full rect, 2048 max size.");

        // ── Now use RawImage instead of Image for pixel-perfect display ────
        GameObject canvasGO = GameObject.Find("Canvas");
        if (canvasGO == null) { Debug.LogError("Canvas not found!"); return; }

        Transform titleT = canvasGO.transform.Find("TitleALLAWIG");
        if (titleT == null) { Debug.LogError("TitleALLAWIG not found!"); return; }

        GameObject titleGO = titleT.gameObject;

        // Remove Image component
        Image img = titleGO.GetComponent<Image>();
        if (img != null) DestroyImmediate(img);

        // Add RawImage — no sprite rect issues, displays full texture
        RawImage rawImg = titleGO.GetComponent<RawImage>();
        if (rawImg == null) rawImg = titleGO.AddComponent<RawImage>();

        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        rawImg.texture = tex;
        rawImg.color = Color.white;
        rawImg.raycastTarget = false;
        // UV rect = full texture
        rawImg.uvRect = new Rect(0f, 0f, 1f, 1f);

        // Size: 819x447 at a good display scale
        // Target width ~600, height proportional
        float targetWidth = 600f;
        float targetHeight = targetWidth * (447f / 819f); // ~327

        RectTransform rt = titleGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(targetWidth, targetHeight);
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(10f, -5f);

        EditorUtility.SetDirty(titleGO);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log($"✅ TitleALLAWIG now uses RawImage with full texture. Size: {targetWidth}x{targetHeight:F0}");
    }
}
