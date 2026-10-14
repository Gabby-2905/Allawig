using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class FixWASDIcon
{
    [MenuItem("Tools/Fix WASD Icon Transparency")]
    public static void Execute()
    {
        // Re-import ALL control icons with alpha from grayscale so white bg becomes transparent
        string[] paths = new[]
        {
            "Assets/UI/ControlIcons/icon_wasd.png",
            "Assets/UI/ControlIcons/icon_mouse.png",
            "Assets/UI/ControlIcons/icon_esc.png",
            "Assets/UI/ControlIcons/icon_e.png",
            "Assets/UI/ControlIcons/icon_leftclick.png",
        };

        foreach (string path in paths)
        {
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) continue;

            ti.textureType         = TextureImporterType.Sprite;
            ti.spriteImportMode    = SpriteImportMode.Single;
            ti.alphaIsTransparency = true;
            ti.alphaSource         = TextureImporterAlphaSource.FromGrayScale; // white → transparent
            ti.mipmapEnabled       = false;
            ti.maxTextureSize      = 512;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        AssetDatabase.Refresh();

        // Find ControlsPanel and update icon colors to white so they show on dark bg
        GameObject controlsPanel = null;
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (go.name == "ControlsPanel") { controlsPanel = go; break; }
        }

        if (controlsPanel == null) { Debug.LogError("ControlsPanel not found."); return; }

        for (int i = 0; i < 5; i++)
        {
            Transform lineT = controlsPanel.transform.Find($"Line_{i}");
            if (lineT == null) continue;
            Transform iconT = lineT.Find("Icon");
            if (iconT == null) continue;

            Image img = iconT.GetComponent<Image>();
            if (img == null) continue;

            // Reload sprite after re-import
            string spritePath = paths[i];
            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sp != null) img.sprite = sp;

            img.color          = Color.white;
            img.preserveAspect = true;
            EditorUtility.SetDirty(iconT.gameObject);
        }

        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("WASD and all control icons fixed with grayscale alpha.");
    }
}
