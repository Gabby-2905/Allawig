using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class FixControlIcons
{
    private static readonly string[] IconPaths = new[]
    {
        "Assets/UI/ControlIcons/icon_wasd.png",
        "Assets/UI/ControlIcons/icon_mouse.png",
        "Assets/UI/ControlIcons/icon_esc.png",
        "Assets/UI/ControlIcons/icon_e.png",
        "Assets/UI/ControlIcons/icon_leftclick.png",
    };

    [MenuItem("Tools/Fix Control Icons Import")]
    public static void Execute()
    {
        foreach (string path in IconPaths)
        {
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) { Debug.LogWarning($"No importer for {path}"); continue; }

            ti.textureType        = TextureImporterType.Sprite;
            ti.spriteImportMode   = SpriteImportMode.Single;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled      = false;
            ti.filterMode         = FilterMode.Bilinear;
            ti.maxTextureSize     = 512;

            // Ensure readable
            ti.isReadable = false;

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Debug.Log($"Re-imported: {path}");
        }

        // Now fix the icon Image components — make them larger and set native size
        GameObject controlsPanel = null;
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (go.name == "ControlsPanel") { controlsPanel = go; break; }
        }

        if (controlsPanel == null) { Debug.LogError("ControlsPanel not found."); return; }

        string[] iconPaths = IconPaths;
        for (int i = 0; i < 5; i++)
        {
            Transform lineT = controlsPanel.transform.Find($"Line_{i}");
            if (lineT == null) continue;

            Transform iconT = lineT.Find("Icon");
            if (iconT == null) continue;

            Image img = iconT.GetComponent<Image>();
            if (img == null) continue;

            // Reload sprite
            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(iconPaths[i]);
            if (sp != null)
            {
                img.sprite          = sp;
                img.preserveAspect  = true;
                img.color           = Color.white;
            }

            // Make icon bigger
            RectTransform rt = iconT.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(52f, 52f);

            EditorUtility.SetDirty(iconT.gameObject);
        }

        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("Control icons fixed.");
    }
}
