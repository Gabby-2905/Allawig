using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// 1. Removes city_part_highway and city_part_fences from Map1_City
/// 2. Adds buildings from the Versatile Studio prefabs around the main road
/// 3. Repositions player on the main road
/// </summary>
public class SetupCityMapFinal
{
    [MenuItem("Tools/Setup City Map Final")]
    public static void Execute()
    {
        var scene = EditorSceneManager.OpenScene(
            "Assets/Scenes/GameScene.unity", OpenSceneMode.Single);

        GameObject cityMap = GameObject.Find("Map1_City");
        if (cityMap == null) { Debug.LogError("Map1_City not found!"); return; }

        // ── 1. Remove highway and fences ──────────────────────────────────
        Transform highway = cityMap.transform.Find("city_part_highway");
        if (highway != null) { Object.DestroyImmediate(highway.gameObject); Debug.Log("Removed city_part_highway."); }

        Transform fences = cityMap.transform.Find("city_part_fences");
        if (fences != null) { Object.DestroyImmediate(fences.gameObject); Debug.Log("Removed city_part_fences."); }

        // ── 2. Find main road bounds to place buildings around it ──────────
        // city_part_main bounds: center ~(-785, 5, -805), size ~(896, 9, 470)
        // In world space (prefab offset -770, 0, -683):
        // center ~(-785-770, 5, -805-683) = wait, prefab is at (-770,0,-683)
        // so world = local + prefab_pos = local + (-770, 0, -683)
        // main road local center: (-785, 5, -805) -> world: (-785+(-770), 5, -805+(-683))
        // Actually the prefab children use world coords directly since prefab offset shifts them.
        // Let's just get the actual world bounds from the renderer.
        Transform mainRoad = cityMap.transform.Find("city_part_main");
        Bounds roadBounds = new Bounds(new Vector3(-785f, 0f, -805f), new Vector3(896f, 0f, 470f));
        if (mainRoad != null)
        {
            Renderer[] roadRenderers = mainRoad.GetComponentsInChildren<Renderer>();
            if (roadRenderers.Length > 0)
            {
                roadBounds = roadRenderers[0].bounds;
                foreach (var r in roadRenderers) roadBounds.Encapsulate(r.bounds);
                roadBounds.Expand(new Vector3(0f, 0f, 0f)); // keep as-is
                Debug.Log($"Main road world bounds: center={roadBounds.center}, size={roadBounds.size}");
            }
        }

        // ── 3. Create a Buildings parent ───────────────────────────────────
        GameObject buildingsParent = new GameObject("Buildings");
        buildingsParent.transform.SetParent(cityMap.transform, false);

        // ── 4. Load building prefabs ───────────────────────────────────────
        string prefabBase = "Assets/Versatile Studio Assets/Demo City By Versatile Studio/Prefabs/";

        GameObject[] officePrefabs = new GameObject[]
        {
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabBase + "office_building_1_with_base.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabBase + "office_building_2_with_base.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabBase + "office_building_3_with_base.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabBase + "office_building_4_with_base.prefab"),
        };

        GameObject[] midHousePrefabs = new GameObject[]
        {
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabBase + "mid_house_1.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabBase + "mid_house_2.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabBase + "mid_house_3.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabBase + "mid_house_4.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabBase + "mid_house_5.prefab"),
        };

        GameObject[] smallHousePrefabs = new GameObject[]
        {
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabBase + "small_house_1.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabBase + "small_house_2.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabBase + "small_house_3.prefab"),
        };

        GameObject factoryBig   = AssetDatabase.LoadAssetAtPath<GameObject>(prefabBase + "factory_building_big.prefab");
        GameObject factorySmall = AssetDatabase.LoadAssetAtPath<GameObject>(prefabBase + "factory_building_small.prefab");

        // Road center and extents
        Vector3 roadCenter = roadBounds.center;
        roadCenter.y = 0f;
        float roadHalfX = roadBounds.extents.x;
        float roadHalfZ = roadBounds.extents.z;

        // ── 5. Place tall office buildings along the back (north side) ─────
        float backZ = roadCenter.z + roadHalfZ + 40f;
        float[] officeXPositions = new float[] { -120f, -40f, 40f, 120f };
        for (int i = 0; i < officeXPositions.Length; i++)
        {
            GameObject prefab = officePrefabs[i % officePrefabs.Length];
            if (prefab == null) continue;
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            inst.transform.SetParent(buildingsParent.transform, false);
            inst.transform.position = new Vector3(roadCenter.x + officeXPositions[i], 0f, backZ);
            inst.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            inst.name = $"OfficeBuilding_{i + 1}";
        }

        // ── 6. Place mid houses along the left side ────────────────────────
        float leftX = roadCenter.x - roadHalfX - 25f;
        for (int i = 0; i < 5; i++)
        {
            GameObject prefab = midHousePrefabs[i % midHousePrefabs.Length];
            if (prefab == null) continue;
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            inst.transform.SetParent(buildingsParent.transform, false);
            inst.transform.position = new Vector3(leftX, 0f, roadCenter.z - 80f + i * 40f);
            inst.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            inst.name = $"MidHouse_Left_{i + 1}";
        }

        // ── 7. Place mid houses along the right side ───────────────────────
        float rightX = roadCenter.x + roadHalfX + 25f;
        for (int i = 0; i < 5; i++)
        {
            GameObject prefab = midHousePrefabs[i % midHousePrefabs.Length];
            if (prefab == null) continue;
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            inst.transform.SetParent(buildingsParent.transform, false);
            inst.transform.position = new Vector3(rightX, 0f, roadCenter.z - 80f + i * 40f);
            inst.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            inst.name = $"MidHouse_Right_{i + 1}";
        }

        // ── 8. Place small houses along the front (south side) ────────────
        float frontZ = roadCenter.z - roadHalfZ - 20f;
        for (int i = 0; i < 6; i++)
        {
            GameObject prefab = smallHousePrefabs[i % smallHousePrefabs.Length];
            if (prefab == null) continue;
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            inst.transform.SetParent(buildingsParent.transform, false);
            inst.transform.position = new Vector3(roadCenter.x - 75f + i * 30f, 0f, frontZ);
            inst.name = $"SmallHouse_{i + 1}";
        }

        // ── 9. Place factory buildings at the far ends ─────────────────────
        if (factoryBig != null)
        {
            GameObject fb1 = (GameObject)PrefabUtility.InstantiatePrefab(factoryBig);
            fb1.transform.SetParent(buildingsParent.transform, false);
            fb1.transform.position = new Vector3(roadCenter.x - roadHalfX - 50f, 0f, roadCenter.z);
            fb1.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            fb1.name = "Factory_Left";

            GameObject fb2 = (GameObject)PrefabUtility.InstantiatePrefab(factoryBig);
            fb2.transform.SetParent(buildingsParent.transform, false);
            fb2.transform.position = new Vector3(roadCenter.x + roadHalfX + 50f, 0f, roadCenter.z);
            fb2.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            fb2.name = "Factory_Right";
        }

        Debug.Log("Buildings placed around main road.");

        // ── 10. Reposition player on the main road ─────────────────────────
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            // Raycast from above the road center to find exact ground Y
            Vector3 rayOrigin = new Vector3(roadCenter.x, 50f, roadCenter.z);
            RaycastHit hit;
            float spawnY = 1f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 200f))
                spawnY = hit.point.y + 1f;

            player.transform.position = new Vector3(roadCenter.x, spawnY, roadCenter.z);
            Debug.Log($"Player placed at {player.transform.position}");
        }

        // ── 11. Save ───────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");
        Debug.Log("City map setup complete. Scene saved.");
    }
}
