using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FinalizeGasStation
{
    [MenuItem("Tools/Finalize Gas Station")]
    public static void Execute()
    {
        var gasStation = GameObject.Find("GasStation");
        if (gasStation == null) { Debug.LogError("GasStation not found"); return; }

        // ── Update entrance trigger to be in front of the store door ─────────
        // Store A world bounds: X 24.39→29.39, Z -4.95→0.95 (front face at Z=-4.95)
        // Store entrance is at world Z ≈ -4.95, X center ≈ 27
        var entranceTrigger = gasStation.transform.Find("EntranceTrigger");
        if (entranceTrigger != null)
        {
            entranceTrigger.position = new Vector3(27f, 0.5f, -6f);
            var box = entranceTrigger.GetComponent<BoxCollider>();
            if (box != null) box.size = new Vector3(4f, 2f, 2f);
        }

        // ── Update exit trigger (inside store) ───────────────────────────────
        var exitTrigger = gasStation.transform.Find("ExitTrigger");
        if (exitTrigger != null)
        {
            exitTrigger.position = new Vector3(27f, 0.5f, -3f);
            var box = exitTrigger.GetComponent<BoxCollider>();
            if (box != null) box.size = new Vector3(4f, 2f, 2f);
        }

        // ── Update spawn points ──────────────────────────────────────────────
        var enterSpawn = gasStation.transform.Find("EnterSpawnPoint");
        if (enterSpawn != null)
        {
            enterSpawn.position = new Vector3(27f, 0.1f, -2f);
            enterSpawn.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        var exitSpawn = gasStation.transform.Find("ExitSpawnPoint");
        if (exitSpawn != null)
        {
            exitSpawn.position = new Vector3(27f, 0.1f, -7f);
            exitSpawn.rotation = Quaternion.Euler(0f, 180f, 0f);
        }

        // ── Update interior root position to match store location ────────────
        var interior = gasStation.transform.Find("Interior");
        if (interior != null)
        {
            interior.position = new Vector3(27f, 0f, -2f);
        }

        // ── Tag player ───────────────────────────────────────────────────────
        var player = GameObject.Find("Player");
        if (player != null)
        {
            try { player.tag = "Player"; } catch { }
        }

        // ── Force scene view skybox ──────────────────────────────────────────
        foreach (SceneView sv in SceneView.sceneViews)
        {
            sv.sceneViewState.showSkybox = true;
            sv.sceneLighting = true;
            sv.Repaint();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[FinalizeGasStation] Done! Triggers and spawn points updated.");
    }
}
