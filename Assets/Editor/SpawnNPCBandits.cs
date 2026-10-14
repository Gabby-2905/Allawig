using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using System.IO;

/// <summary>
/// Editor script that:
/// 1. Creates an NPC Animator Controller for the Basic Bandit
/// 2. Spawns 3 NPC Bandits in the scene with FSM, NavMeshAgent, and patrol waypoints
/// 3. Adds a NoiseSystem to the scene
/// </summary>
public class SpawnNPCBandits : EditorWindow
{
    [MenuItem("Tools/Spawn NPC Bandits")]
    public static void SpawnBandits()
    {
        // ── 1. Create Animator Controller ─────────────────────────────────
        string controllerPath = "Assets/BasicBandit/NPCBanditController.controller";
        UnityEditor.Animations.AnimatorController controller = CreateNPCAnimatorController(controllerPath);

        // ── 2. Ensure NoiseSystem exists ──────────────────────────────────
        NoiseSystem noiseSystem = Object.FindFirstObjectByType<NoiseSystem>();
        if (noiseSystem == null)
        {
            GameObject noiseSysGO = new GameObject("NoiseSystem");
            noiseSysGO.AddComponent<NoiseSystem>();
            Undo.RegisterCreatedObjectUndo(noiseSysGO, "Create NoiseSystem");
            Debug.Log("[SpawnNPCBandits] Created NoiseSystem.");
        }

        // ── 3. Load Basic Bandit prefab ───────────────────────────────────
        string prefabPath = "Assets/BasicBandit/Basic_BanditPrefab.prefab";
        GameObject banditPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (banditPrefab == null)
        {
            Debug.LogError($"[SpawnNPCBandits] Could not find prefab at {prefabPath}");
            return;
        }

        // ── 4. Create parent container ────────────────────────────────────
        GameObject npcParent = new GameObject("NPCBandits");
        Undo.RegisterCreatedObjectUndo(npcParent, "Create NPC Bandits");

        // ── 5. Spawn 3 bandits ────────────────────────────────────────────
        Vector3[] spawnPositions = new Vector3[]
        {
            new Vector3(  5f, 4f,  8f),
            new Vector3( -6f, 4f,  5f),
            new Vector3(  3f, 4f, -7f),
        };

        string[] npcNames = { "Bandit_Guard_01", "Bandit_Guard_02", "Bandit_Guard_03" };

        for (int i = 0; i < 3; i++)
        {
            GameObject npc = (GameObject)PrefabUtility.InstantiatePrefab(banditPrefab);
            npc.name = npcNames[i];
            npc.transform.SetParent(npcParent.transform);
            npc.transform.position = spawnPositions[i];
            npc.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            npc.tag = "Enemy";

            Undo.RegisterCreatedObjectUndo(npc, "Spawn NPC Bandit");

            // ── NavMeshAgent ──────────────────────────────────────────────
            NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
            if (agent == null) agent = Undo.AddComponent<NavMeshAgent>(npc);
            agent.speed        = 2f;
            agent.angularSpeed = 240f;
            agent.acceleration = 8f;
            agent.stoppingDistance = 0.5f;
            agent.radius       = 0.35f;
            agent.height       = 1.8f;
            agent.baseOffset   = 0f;

            // ── Animator ──────────────────────────────────────────────────
            Animator anim = npc.GetComponent<Animator>();
            if (anim == null) anim = Undo.AddComponent<Animator>(npc);
            anim.runtimeAnimatorController = controller;
            anim.applyRootMotion = false;

            // ── FSM ───────────────────────────────────────────────────────
            NPCStateMachine fsm = npc.GetComponent<NPCStateMachine>();
            if (fsm == null) fsm = Undo.AddComponent<NPCStateMachine>(npc);
            fsm.patrolSpeed  = 2f;
            fsm.chaseSpeed   = 5f;
            fsm.sightRange   = 14f;
            fsm.sightAngle   = 110f;
            fsm.attackRange  = 2.2f;
            fsm.attackDamage = 10f;

            // ── Patrol Waypoints ──────────────────────────────────────────
            Transform[] waypoints = CreatePatrolWaypoints(npc, spawnPositions[i], i);
            fsm.patrolWaypoints = waypoints;

            // ── Capsule Collider (if missing) ─────────────────────────────
            if (npc.GetComponent<Collider>() == null)
            {
                CapsuleCollider cap = Undo.AddComponent<CapsuleCollider>(npc);
                cap.height = 1.8f;
                cap.radius = 0.35f;
                cap.center = new Vector3(0f, 0.9f, 0f);
            }

            Debug.Log($"[SpawnNPCBandits] Spawned {npc.name} at {spawnPositions[i]}");
        }

        // ── 6. Mark scene dirty ───────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[SpawnNPCBandits] Done! 3 NPC Bandits spawned with FSM.");
        EditorUtility.DisplayDialog("NPC Bandits Spawned",
            "3 NPC Bandits have been placed in the scene with:\n" +
            "• Finite State Machine (Patrol → Investigate → Chase → Attack)\n" +
            "• NavMeshAgent pathfinding\n" +
            "• Patrol waypoints\n" +
            "• Noise-based hearing detection\n\n" +
            "Remember to Bake the NavMesh (Window > AI > Navigation)!", "OK");
    }

    // ─────────────────────────────────────────────────────────────────────
    //  CREATE ANIMATOR CONTROLLER
    // ─────────────────────────────────────────────────────────────────────
    static UnityEditor.Animations.AnimatorController CreateNPCAnimatorController(string path)
    {
        // Delete existing
        if (File.Exists(path))
            AssetDatabase.DeleteAsset(path);

        var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(path);

        // ── Parameters ────────────────────────────────────────────────────
        controller.AddParameter("Speed",   AnimatorControllerParameterType.Float);
        controller.AddParameter("Walk",    AnimatorControllerParameterType.Bool);
        controller.AddParameter("Attack",  AnimatorControllerParameterType.Trigger);

        var rootSM = controller.layers[0].stateMachine;

        // ── Load animation clips ──────────────────────────────────────────
        AnimationClip idleClip  = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/BasicBandit/DemoSceneBasicBandit/AnimationsDemoscene/IdleGrab_Neutral.fbx");
        AnimationClip walkClip  = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/BasicBandit/DemoSceneBasicBandit/AnimationsDemoscene/WalkFWD.fbx");
        AnimationClip sprintClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/BasicBandit/DemoSceneBasicBandit/AnimationsDemoscene/SprintJump_ToLeft_R.fbx");

        // ── States ────────────────────────────────────────────────────────
        var idleState   = rootSM.AddState("Idle",   new Vector3(200, 100, 0));
        var walkState   = rootSM.AddState("Walk",   new Vector3(200, 200, 0));
        var chaseState  = rootSM.AddState("Chase",  new Vector3(200, 300, 0));
        var attackState = rootSM.AddState("Attack", new Vector3(450, 200, 0));

        if (idleClip  != null) idleState.motion  = idleClip;
        if (walkClip  != null) walkState.motion  = walkClip;
        if (sprintClip != null) chaseState.motion = sprintClip;
        if (idleClip  != null) attackState.motion = idleClip;

        rootSM.defaultState = idleState;

        // ── Transitions ───────────────────────────────────────────────────
        // Idle → Walk
        var t = idleState.AddTransition(walkState);
        t.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "Speed");
        t.duration = 0.2f; t.hasExitTime = false;

        // Walk → Idle
        t = walkState.AddTransition(idleState);
        t.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "Speed");
        t.duration = 0.2f; t.hasExitTime = false;

        // Walk → Chase (fast)
        t = walkState.AddTransition(chaseState);
        t.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 3.5f, "Speed");
        t.duration = 0.15f; t.hasExitTime = false;

        // Idle → Chase
        t = idleState.AddTransition(chaseState);
        t.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 3.5f, "Speed");
        t.duration = 0.15f; t.hasExitTime = false;

        // Chase → Walk
        t = chaseState.AddTransition(walkState);
        t.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 3.5f, "Speed");
        t.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "Speed");
        t.duration = 0.2f; t.hasExitTime = false;

        // Chase → Idle
        t = chaseState.AddTransition(idleState);
        t.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "Speed");
        t.duration = 0.2f; t.hasExitTime = false;

        // Any → Attack
        var anyAttack = rootSM.AddAnyStateTransition(attackState);
        anyAttack.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "Attack");
        anyAttack.duration = 0.1f; anyAttack.hasExitTime = false;

        // Attack → Idle (exit time)
        t = attackState.AddTransition(idleState);
        t.hasExitTime = true; t.exitTime = 0.85f; t.duration = 0.15f;

        AssetDatabase.SaveAssets();
        Debug.Log($"[SpawnNPCBandits] Created NPC Animator Controller at {path}");
        return controller;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  CREATE PATROL WAYPOINTS
    // ─────────────────────────────────────────────────────────────────────
    static Transform[] CreatePatrolWaypoints(GameObject npc, Vector3 center, int index)
    {
        GameObject waypointParent = new GameObject($"{npc.name}_Waypoints");
        waypointParent.transform.SetParent(npc.transform.parent);
        Undo.RegisterCreatedObjectUndo(waypointParent, "Create Waypoints");

        // 4 waypoints in a square pattern around the spawn
        Vector3[] offsets = new Vector3[]
        {
            new Vector3( 4f, 0f,  0f),
            new Vector3( 0f, 0f,  4f),
            new Vector3(-4f, 0f,  0f),
            new Vector3( 0f, 0f, -4f),
        };

        Transform[] waypoints = new Transform[offsets.Length];
        for (int j = 0; j < offsets.Length; j++)
        {
            GameObject wp = new GameObject($"WP_{index}_{j}");
            wp.transform.SetParent(waypointParent.transform);
            wp.transform.position = center + offsets[j];
            Undo.RegisterCreatedObjectUndo(wp, "Create Waypoint");
            waypoints[j] = wp.transform;
        }

        return waypoints;
    }
}
