using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SpawnPlayerOnStreet
{
    [MenuItem("Tools/Spawn Player On Street")]
    public static void Execute()
    {
        var scene = EditorSceneManager.OpenScene(
            "Assets/Scenes/GameScene.unity", OpenSceneMode.Single);

        GameObject player = GameObject.Find("Player");
        if (player == null) { Debug.LogError("Player not found!"); return; }

        // The city_part_collider bounds: min Y = 0.6928, center X=770, Z=699
        // The main road (city_part_main) is at the city center area.
        // city_part_main bounds center: (562, 31, 279) in the original scene.
        // Road surface Y = collider min Y = ~0.69
        // Place player at the center of the main road, just above the surface.

        // Disable the player's CharacterController temporarily so we can set position freely
        CharacterController cc = player.GetComponent<CharacterController>();
        bool wasEnabled = false;
        if (cc != null) { wasEnabled = cc.enabled; cc.enabled = false; }

        // Street center position — main road center X=562, Z=279, Y just above road surface
        float roadSurfaceY = 0.69f;
        Vector3 spawnPos = new Vector3(562f, roadSurfaceY + 0.1f, 279f);

        player.transform.position = spawnPos;

        // Re-enable CharacterController
        if (cc != null) cc.enabled = wasEnabled;

        // Update camera to match
        GameObject cam = GameObject.Find("Main Camera");
        if (cam != null)
        {
            GameCameraFollow follow = cam.GetComponent<GameCameraFollow>();
            if (follow != null)
            {
                follow.target = player.transform;
                cam.transform.position = spawnPos + new Vector3(0f, 13f, -9f);
                cam.transform.LookAt(spawnPos + new Vector3(0f, 0.9f, 4f));
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");
        Debug.Log($"Player placed on street at {player.transform.position}");
    }
}
