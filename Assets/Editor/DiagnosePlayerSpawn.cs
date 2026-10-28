using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class DiagnosePlayerSpawn
{
    public static void Execute()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity", OpenSceneMode.Single);

        // Get the collider bounds to understand the actual road area
        GameObject colliderObj = GameObject.Find("city_part_collider");
        if (colliderObj != null)
        {
            MeshCollider mc = colliderObj.GetComponent<MeshCollider>();
            if (mc != null)
            {
                Debug.Log($"Collider bounds: center={mc.bounds.center}, size={mc.bounds.size}, min={mc.bounds.min}, max={mc.bounds.max}");
            }
        }

        // Raycast from many positions across the collider's XZ range to find valid road spots
        // Collider center is at X=770, Z=699 in world space
        // Try a grid of positions
        float[] xPositions = { 400f, 500f, 562f, 600f, 700f, 770f, 800f, 900f };
        float[] zPositions = { 200f, 279f, 400f, 500f, 600f, 699f, 800f };

        int hitCount = 0;
        foreach (float x in xPositions)
        {
            foreach (float z in zPositions)
            {
                RaycastHit hit;
                Vector3 origin = new Vector3(x, 200f, z);
                if (Physics.Raycast(origin, Vector3.down, out hit, 400f))
                {
                    Debug.Log($"HIT at world ({x}, {hit.point.y:F2}, {z}) on '{hit.collider.gameObject.name}'");
                    hitCount++;
                }
            }
        }
        Debug.Log($"Total hits: {hitCount} out of {xPositions.Length * zPositions.Length} tested");
    }
}
