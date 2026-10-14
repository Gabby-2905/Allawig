using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class FixApocLighting : MonoBehaviour
{
    [MenuItem("Tools/Fix Apocalypse Lighting")]
    public static void Execute()
    {
        FixLighting();
        FixSEWallCorner();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[FixApocLighting] Done.");
    }

    static void FixLighting()
    {
        var lightGO = GameObject.Find("Directional Light");
        if (lightGO != null)
        {
            var l = lightGO.GetComponent<Light>();
            if (l != null)
            {
                // Bright stormy overcast — clearly visible, cold blue-grey
                l.color     = new Color(0.80f, 0.84f, 0.92f, 1f);
                l.intensity = 1.8f;
                lightGO.transform.rotation = Quaternion.Euler(50f, 195f, 0f);
                l.shadows = LightShadows.Soft;
            }
        }

        RenderSettings.ambientMode      = AmbientMode.Flat;
        RenderSettings.ambientLight     = new Color(0.45f, 0.48f, 0.55f, 1f);
        RenderSettings.ambientIntensity = 1.4f;
        RenderSettings.skybox           = null;

        // Very light fog — just enough for depth, not obscuring
        RenderSettings.fog              = true;
        RenderSettings.fogMode          = FogMode.Linear;
        RenderSettings.fogColor         = new Color(0.28f, 0.30f, 0.35f, 1f);
        RenderSettings.fogStartDistance = 80f;
        RenderSettings.fogEndDistance   = 250f;
    }

    static void FixSEWallCorner()
    {
        var fenceRoot = GameObject.Find("Fence");
        if (fenceRoot == null) return;

        // The SE corner gap: the east wall ends at Z=-37.5 but the south wall
        // corner piece at X=37.5 is missing. Add the missing SE corner wall piece.
        var cornerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Apartment_Corner_01.prefab");
        var cornerRoof = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Apartment_Roof_Corner_01.prefab");

        // Remove any stray out-of-bounds pieces first
        var toRemove = new List<GameObject>();
        foreach (Transform child in fenceRoot.transform)
        {
            var pos = child.position;
            if (pos.x > 41f || pos.x < -41f || pos.z > 41f || pos.z < -41f)
                toRemove.Add(child.gameObject);
        }
        foreach (var go in toRemove)
            Undo.DestroyObjectImmediate(go);

        // Check if SE corner (X=37.5, Z=-37.5) wall piece exists
        bool hasSECorner = false;
        foreach (Transform child in fenceRoot.transform)
        {
            if (Mathf.Abs(child.position.x - 37.5f) < 1f && Mathf.Abs(child.position.z + 37.5f) < 1f)
            { hasSECorner = true; break; }
        }

        if (!hasSECorner && cornerPrefab != null)
        {
            var w = (GameObject)PrefabUtility.InstantiatePrefab(cornerPrefab, fenceRoot.transform);
            w.transform.SetPositionAndRotation(new Vector3(37.5f, 0f, -37.5f), Quaternion.Euler(0, 270f, 0));
            w.name = "Wall_SE_Corner";

            if (cornerRoof != null)
            {
                var r = (GameObject)PrefabUtility.InstantiatePrefab(cornerRoof, fenceRoot.transform);
                r.transform.SetPositionAndRotation(new Vector3(37.5f, 3f, -37.5f), Quaternion.Euler(0, 270f, 0));
                r.name = "WallRoof_SE_Corner";
            }
            Debug.Log("[FixApocLighting] SE corner wall added.");
        }

        // Also check SW corner (X=-37.5, Z=-37.5)
        bool hasSWCorner = false;
        foreach (Transform child in fenceRoot.transform)
        {
            if (Mathf.Abs(child.position.x + 37.5f) < 1f && Mathf.Abs(child.position.z + 37.5f) < 1f)
            { hasSWCorner = true; break; }
        }

        if (!hasSWCorner && cornerPrefab != null)
        {
            var w = (GameObject)PrefabUtility.InstantiatePrefab(cornerPrefab, fenceRoot.transform);
            w.transform.SetPositionAndRotation(new Vector3(-37.5f, 0f, -37.5f), Quaternion.Euler(0, 0f, 0));
            w.name = "Wall_SW_Corner";

            if (cornerRoof != null)
            {
                var r = (GameObject)PrefabUtility.InstantiatePrefab(cornerRoof, fenceRoot.transform);
                r.transform.SetPositionAndRotation(new Vector3(-37.5f, 3f, -37.5f), Quaternion.Euler(0, 0f, 0));
                r.name = "WallRoof_SW_Corner";
            }
            Debug.Log("[FixApocLighting] SW corner wall added.");
        }
    }
}
