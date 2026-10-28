using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FindCityRoadSurface
{
    /// <summary>
    /// Opens the Versatile Studio map1 city scene additively,
    /// finds a road/ground mesh, logs its world Y position,
    /// then closes the additive scene.
    /// Also fixes the ambient lighting in GameScene.
    /// </summary>
    public static void Execute()
    {
        // ── Fix ambient lighting in GameScene ─────────────────────────────
        var gameScene = EditorSceneManager.OpenScene(
            "Assets/Scenes/GameScene.unity", OpenSceneMode.Single);

        // Set ambient light to a reasonable daytime value
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor     = new Color(0.5f, 0.6f, 0.8f);
        RenderSettings.ambientEquatorColor = new Color(0.4f, 0.4f, 0.4f);
        RenderSettings.ambientGroundColor  = new Color(0.2f, 0.2f, 0.2f);
        RenderSettings.ambientIntensity    = 1.2f;

        // ── Open map1 city scene additively to inspect road positions ──────
        var mapScene = EditorSceneManager.OpenScene(
            "Assets/Versatile Studio Assets/Demo City By Versatile Studio/Scenes/map1 city.unity",
            OpenSceneMode.Additive);

        // Find all MeshRenderers in the map scene and log road-like ones
        foreach (var go in mapScene.GetRootGameObjects())
        {
            MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var r in renderers)
            {
                string nameLower = r.gameObject.name.ToLower();
                if (nameLower.Contains("road") || nameLower.Contains("street") ||
                    nameLower.Contains("ground") || nameLower.Contains("floor") ||
                    nameLower.Contains("pavement") || nameLower.Contains("sidewalk"))
                {
                    Bounds b = r.bounds;
                    Debug.Log($"[Road] {r.gameObject.name} | center: {b.center} | min Y: {b.min.y} | max Y: {b.max.y}");
                }
            }
        }

        EditorSceneManager.CloseScene(mapScene, false);

        // Save GameScene with updated lighting
        EditorSceneManager.MarkSceneDirty(gameScene);
        EditorSceneManager.SaveScene(gameScene, "Assets/Scenes/GameScene.unity");
        Debug.Log("Ambient lighting fixed in GameScene.");
    }
}
