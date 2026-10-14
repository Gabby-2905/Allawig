using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Places the 1stMap.fbx as the main game map:
/// - Imports with proper scale and mesh colliders
/// - Removes old Ground tile objects
/// - Scales the map to a large playable size
/// - Repositions the player, camera, rain, and containers
/// </summary>
public class PlaceFirstMap : MonoBehaviour
{
    [MenuItem("Tools/Place 1stMap as Game Map")]
    public static void Execute()
    {
        string fbxPath = "Assets/TripoModels/post-apocalyptic_scrapyard_3d_model/1stMap.fbx";

        // ── 1. Fix FBX import settings ─────────────────────────────────────
        ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        if (importer == null) { Debug.LogError($"Could not find importer for {fbxPath}"); return; }

        bool needsReimport = false;

        if (importer.globalScale != 1f)          { importer.globalScale = 1f; needsReimport = true; }
        if (!importer.addCollider)               { importer.addCollider = true; needsReimport = true; }
        if (importer.importNormals != ModelImporterNormals.Import)
        { importer.importNormals = ModelImporterNormals.Import; needsReimport = true; }

        if (needsReimport)
        {
            importer.SaveAndReimport();
            AssetDatabase.Refresh();
            Debug.Log("1stMap.fbx reimported with colliders and scale=1.");
        }

        // ── 2. Remove old Ground object ────────────────────────────────────
        GameObject oldGround = GameObject.Find("Ground");
        if (oldGround != null)
        {
            GameObject.DestroyImmediate(oldGround);
            Debug.Log("Removed old Ground tiles.");
        }

        // Also remove old post-apocalyptic_scrapyard model if present
        GameObject oldScrapyard = GameObject.Find("post-apocalyptic_scrapyard_3d_model");
        if (oldScrapyard != null)
        {
            GameObject.DestroyImmediate(oldScrapyard);
            Debug.Log("Removed old scrapyard model.");
        }

        // Remove existing 1stMap if already placed
        GameObject existing1stMap = GameObject.Find("1stMap");
        if (existing1stMap != null)
        {
            GameObject.DestroyImmediate(existing1stMap);
            Debug.Log("Removed existing 1stMap instance.");
        }

        // ── 3. Instantiate 1stMap ──────────────────────────────────────────
        GameObject mapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (mapPrefab == null) { Debug.LogError($"Could not load {fbxPath}"); return; }

        GameObject mapGO = (GameObject)PrefabUtility.InstantiatePrefab(mapPrefab);
        mapGO.name = "1stMap";

        // ── 4. Get natural bounds to determine scale needed ────────────────
        // First set scale to 1 to measure
        mapGO.transform.position = Vector3.zero;
        mapGO.transform.rotation = Quaternion.identity;
        mapGO.transform.localScale = Vector3.one;

        // Force bounds calculation
        Bounds naturalBounds = GetBounds(mapGO);
        Debug.Log($"1stMap natural bounds: {naturalBounds.size} center: {naturalBounds.center}");

        // ── 5. Scale to a large playable map ──────────────────────────────
        // Target: ~200x200 units playable area (large open world feel)
        // Use the larger of X/Z dimensions to determine scale factor
        float naturalSize = Mathf.Max(naturalBounds.size.x, naturalBounds.size.z);
        float targetSize = 200f;

        float scaleFactor = (naturalSize > 0.1f) ? (targetSize / naturalSize) : 10f;

        // Clamp scale to reasonable range
        scaleFactor = Mathf.Clamp(scaleFactor, 5f, 50f);

        mapGO.transform.localScale = Vector3.one * scaleFactor;
        Debug.Log($"Applied scale factor: {scaleFactor} (natural size: {naturalSize:F2}, target: {targetSize})");

        // ── 6. Position map so its surface is at Y=0 ──────────────────────
        Bounds scaledBounds = GetBounds(mapGO);
        float yOffset = -scaledBounds.min.y;
        mapGO.transform.position = new Vector3(
            -scaledBounds.center.x,
            yOffset,
            -scaledBounds.center.z
        );

        // Recalculate after repositioning
        scaledBounds = GetBounds(mapGO);
        Debug.Log($"1stMap scaled bounds: {scaledBounds.size}, min Y: {scaledBounds.min.y:F2}");

        // ── 7. Ensure mesh colliders are on all children ───────────────────
        EnsureColliders(mapGO);

        // ── 8. Reposition Player on the map ───────────────────────────────
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            // Place player at center of map, slightly above surface
            player.transform.position = new Vector3(0f, scaledBounds.min.y + 0.2f, 0f);
            EditorUtility.SetDirty(player);
            Debug.Log($"Player repositioned to {player.transform.position}");
        }

        // ── 9. Reposition Rain ─────────────────────────────────────────────
        GameObject rain = GameObject.Find("Rain");
        if (rain != null)
        {
            rain.transform.position = new Vector3(0f, scaledBounds.max.y + 20f, 0f);
            EditorUtility.SetDirty(rain);
        }

        // ── 10. Reposition Containers onto the map surface ─────────────────
        float mapSurfaceY = scaledBounds.min.y + 0.5f;
        float spread = Mathf.Min(scaledBounds.size.x, scaledBounds.size.z) * 0.35f;

        Vector3[] containerPositions = new Vector3[]
        {
            new Vector3( spread * 0.3f,  mapSurfaceY,  spread * 0.3f),
            new Vector3(-spread * 0.3f,  mapSurfaceY,  spread * 0.3f),
            new Vector3( spread * 0.3f,  mapSurfaceY, -spread * 0.3f),
            new Vector3(-spread * 0.3f,  mapSurfaceY, -spread * 0.3f),
            new Vector3( spread * 0.6f,  mapSurfaceY,  0f),
            new Vector3(-spread * 0.6f,  mapSurfaceY,  0f),
            new Vector3( 0f,             mapSurfaceY,  spread * 0.6f),
            new Vector3( 0f,             mapSurfaceY, -spread * 0.6f),
            new Vector3( spread * 0.5f,  mapSurfaceY,  spread * 0.5f),
            new Vector3(-spread * 0.5f,  mapSurfaceY, -spread * 0.5f),
            new Vector3( spread * 0.7f,  mapSurfaceY,  spread * 0.2f),
            new Vector3(-spread * 0.2f,  mapSurfaceY,  spread * 0.7f),
            new Vector3( spread * 0.4f,  mapSurfaceY, -spread * 0.6f),
            new Vector3(-spread * 0.6f,  mapSurfaceY,  spread * 0.4f),
        };

        // Find all Container objects
        List<GameObject> containers = new List<GameObject>();
        foreach (GameObject go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (go.name.StartsWith("Container"))
                containers.Add(go);
        }

        for (int i = 0; i < containers.Count && i < containerPositions.Length; i++)
        {
            containers[i].transform.position = containerPositions[i];
            EditorUtility.SetDirty(containers[i]);
        }
        Debug.Log($"Repositioned {Mathf.Min(containers.Count, containerPositions.Length)} containers.");

        // ── 11. Mark dirty and save ────────────────────────────────────────
        EditorUtility.SetDirty(mapGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log($"✅ 1stMap placed! Scale: {scaleFactor:F1}x, Bounds: {scaledBounds.size.x:F0}x{scaledBounds.size.z:F0} units.");
    }

    static Bounds GetBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.one);

        Bounds b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        return b;
    }

    static void EnsureColliders(GameObject go)
    {
        int added = 0;
        MeshFilter[] filters = go.GetComponentsInChildren<MeshFilter>();
        foreach (var mf in filters)
        {
            if (mf.sharedMesh == null) continue;
            MeshCollider mc = mf.GetComponent<MeshCollider>();
            if (mc == null)
            {
                mc = mf.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
                added++;
            }
        }
        Debug.Log($"Ensured colliders on {filters.Length} meshes ({added} added).");
    }
}
