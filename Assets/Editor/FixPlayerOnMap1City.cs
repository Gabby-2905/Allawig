using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixPlayerOnMap1City
{
    [MenuItem("Tools/Fix Player On Map1 City")]
    public static void Execute()
    {
        var scene = EditorSceneManager.OpenScene(
            "Assets/Scenes/GameScene.unity", OpenSceneMode.Single);

        GameObject player = GameObject.Find("Player");
        if (player == null) { Debug.LogError("Player not found!"); return; }

        // The map1 city scene content sits at world origin.
        // STATIC_MODELS bounds: center=(562, 31, 279), min Y=-3.86
        // Try raycasting from above the road area to find exact ground surface.
        Vector3[] candidates = new Vector3[]
        {
            new Vector3(562f, 200f, 279f),
            new Vector3(500f, 200f, 250f),
            new Vector3(600f, 200f, 300f),
            new Vector3(450f, 200f, 200f),
            new Vector3(650f, 200f, 350f),
            new Vector3(562f, 200f, 200f),
            new Vector3(562f, 200f, 350f),
        };

        Vector3 spawnPos = new Vector3(562f, 2f, 279f);
        bool found = false;

        foreach (var c in candidates)
        {
            RaycastHit hit;
            if (Physics.Raycast(c, Vector3.down, out hit, 400f))
            {
                spawnPos = hit.point + Vector3.up * 1.2f;
                Debug.Log($"Hit '{hit.collider.gameObject.name}' at {hit.point}. Spawn: {spawnPos}");
                found = true;
                break;
            }
        }

        if (!found)
        {
            // Fallback: use known ground Y from STATIC_MODELS min Y = -3.86 + some offset
            // The road surface in the original scene is at Y≈0
            spawnPos = new Vector3(562f, 1.5f, 279f);
            Debug.Log($"No raycast hit — fallback spawn: {spawnPos}");
        }

        player.transform.position = spawnPos;

        // Fix camera to follow player at new position
        GameObject cam = GameObject.Find("Main Camera");
        if (cam != null)
        {
            GameCameraFollow follow = cam.GetComponent<GameCameraFollow>();
            if (follow != null)
            {
                follow.target = player.transform;
                Vector3 camPos = spawnPos + new Vector3(0f, 13f, -9f);
                cam.transform.position = camPos;
                cam.transform.LookAt(spawnPos + new Vector3(0f, 0.9f, 4f));
                Debug.Log($"Camera repositioned to {camPos}");
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");
        Debug.Log($"Player and camera fixed. Player at {player.transform.position}");
    }
}
