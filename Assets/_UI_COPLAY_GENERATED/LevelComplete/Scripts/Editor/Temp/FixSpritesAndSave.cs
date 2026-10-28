using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class FixSpritesAndSave
{
    public static void Execute()
    {
        string spritePath = "Assets/_UI_COPLAY_GENERATED/LevelComplete/Sprites/";

        // Fix texture import settings to Sprite type
        string[] paths = new string[]
        {
            spritePath + "StarFilled.png",
            spritePath + "StarEmpty.png",
            spritePath + "HazardStripe.png"
        };

        foreach (var p in paths)
        {
            var importer = AssetImporter.GetAtPath(p) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate);
                Debug.Log("[FixSprites] Fixed: " + p);
            }
        }

        AssetDatabase.Refresh();

        // Reload sprites
        Sprite starFilled = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "StarFilled.png");
        Sprite starEmpty  = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "StarEmpty.png");
        Sprite hazard     = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "HazardStripe.png");

        Debug.Log($"[FixSprites] StarFilled: {(starFilled != null ? "OK" : "NULL")}");
        Debug.Log($"[FixSprites] StarEmpty: {(starEmpty != null ? "OK" : "NULL")}");
        Debug.Log($"[FixSprites] Hazard: {(hazard != null ? "OK" : "NULL")}");

        // Find the canvas
        var canvasGO = GameObject.Find("LevelCompleteCanvas");
        if (canvasGO == null)
        {
            Debug.LogError("[FixSprites] LevelCompleteCanvas not found!");
            return;
        }

        // Assign star sprites
        AssignSprite(canvasGO, "LevelCompleteCanvas/Overlay/Panel/StarCenter", starFilled);
        AssignSprite(canvasGO, "LevelCompleteCanvas/Overlay/Panel/StarLeft",   starFilled);
        AssignSprite(canvasGO, "LevelCompleteCanvas/Overlay/Panel/StarRight",  starEmpty);

        // Assign hazard stripe sprites
        AssignHazardSprite(canvasGO, "LevelCompleteCanvas/Overlay/Panel/TopStripe", hazard);
        AssignHazardSprite(canvasGO, "LevelCompleteCanvas/Overlay/Panel/BotStripe", hazard);

        // Update LevelCompleteManager sprite refs
        var lcm = canvasGO.GetComponent<LevelCompleteManager>();
        if (lcm != null)
        {
            lcm.starFilledSprite = starFilled;
            lcm.starEmptySprite  = starEmpty;
            EditorUtility.SetDirty(lcm);
        }

        EditorUtility.SetDirty(canvasGO);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FixSprites] Done! Scene saved.");
    }

    static void AssignSprite(GameObject root, string path, Sprite sprite)
    {
        if (sprite == null) return;
        var t = root.transform.Find(path.Substring(path.IndexOf('/') + 1));
        if (t == null) { Debug.LogWarning("[FixSprites] Not found: " + path); return; }
        var img = t.GetComponent<Image>();
        if (img == null) { Debug.LogWarning("[FixSprites] No Image on: " + path); return; }
        img.sprite = sprite;
        img.color  = Color.white;
        img.type   = Image.Type.Simple;
        img.preserveAspect = true;
        EditorUtility.SetDirty(img);
        Debug.Log("[FixSprites] Assigned sprite to: " + path);
    }

    static void AssignHazardSprite(GameObject root, string path, Sprite sprite)
    {
        if (sprite == null) return;
        var t = root.transform.Find(path.Substring(path.IndexOf('/') + 1));
        if (t == null) { Debug.LogWarning("[FixSprites] Not found: " + path); return; }
        var img = t.GetComponent<Image>();
        if (img == null) { Debug.LogWarning("[FixSprites] No Image on: " + path); return; }
        img.sprite = sprite;
        img.color  = Color.white;
        img.type   = Image.Type.Tiled;
        EditorUtility.SetDirty(img);
        Debug.Log("[FixSprites] Assigned hazard sprite to: " + path);
    }
}
