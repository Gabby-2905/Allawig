using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class InspectVersatileCity
{
    public static void Execute()
    {
        var mapScene = EditorSceneManager.OpenScene(
            "Assets/Versatile Studio Assets/Demo City By Versatile Studio/Scenes/map1 city.unity",
            OpenSceneMode.Additive);

        // Log all root objects and their positions/bounds
        foreach (var go in mapScene.GetRootGameObjects())
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length > 0)
            {
                Bounds combined = renderers[0].bounds;
                foreach (var r in renderers) combined.Encapsulate(r.bounds);
                Debug.Log($"[Root] '{go.name}' | pos: {go.transform.position} | bounds center: {combined.center} | min Y: {combined.min.y:F2} | max Y: {combined.max.y:F2}");
            }
            else
            {
                Debug.Log($"[Root-NoRenderer] '{go.name}' | pos: {go.transform.position}");
            }
        }

        EditorSceneManager.CloseScene(mapScene, false);
    }
}
