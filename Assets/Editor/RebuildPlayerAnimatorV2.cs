using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class RebuildPlayerAnimatorV2
{
    public static void Execute()
    {
        // ── Load clips ──────────────────────────────────────────
        AnimationClip idle  = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Idles/HumanM@Idle01.fbx");
        AnimationClip walk  = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Walk/HumanM@Walk01_Forward.fbx");
        AnimationClip run   = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Run/HumanM@Run01_Forward.fbx");
        // Use Talk as punch (best available gesture in free pack)
        AnimationClip punch = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Social/Conversation/HumanM@Talk01.fbx");

        if (idle == null || walk == null || run == null || punch == null)
        {
            Debug.LogError("One or more animation clips not found.");
            return;
        }

        string savePath = "Assets/animations/PlayerController.controller";

        // Delete old controller
        AssetDatabase.DeleteAsset(savePath);
        AssetDatabase.Refresh();

        // ── Create controller ───────────────────────────────────
        AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(savePath);

        // Parameters
        ctrl.AddParameter("Speed",    AnimatorControllerParameterType.Float);
        ctrl.AddParameter("IsAiming", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Punch",    AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Shoot",    AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine sm = ctrl.layers[0].stateMachine;

        // ── Locomotion blend tree ───────────────────────────────
        AnimatorState locoState = sm.AddState("Locomotion", new Vector3(300, 100, 0));

        BlendTree tree = new BlendTree();
        tree.name            = "Locomotion";
        tree.blendType       = BlendTreeType.Simple1D;
        tree.blendParameter  = "Speed";
        tree.useAutomaticThresholds = false;
        tree.AddChild(idle,  0f);
        tree.AddChild(walk,  1f);
        tree.AddChild(run,   2f);

        locoState.motion = tree;
        AssetDatabase.AddObjectToAsset(tree, savePath);

        // ── Punch state ─────────────────────────────────────────
        AnimatorState punchState = sm.AddState("Punch", new Vector3(550, 0, 0));
        punchState.motion = punch;
        punchState.speed  = 2.0f; // play faster so it feels snappy

        // ── Shoot state (reuse punch clip, faster) ──────────────
        AnimatorState shootState = sm.AddState("Shoot", new Vector3(550, 200, 0));
        shootState.motion = punch;
        shootState.speed  = 3.0f;

        // ── Default state ───────────────────────────────────────
        sm.defaultState = locoState;

        // ── Transitions: Loco → Punch ───────────────────────────
        AnimatorStateTransition locToPunch = locoState.AddTransition(punchState);
        locToPunch.AddCondition(AnimatorConditionMode.If, 0, "Punch");
        locToPunch.duration = 0.05f;
        locToPunch.hasExitTime = false;

        // ── Transitions: Loco → Shoot ───────────────────────────
        AnimatorStateTransition locToShoot = locoState.AddTransition(shootState);
        locToShoot.AddCondition(AnimatorConditionMode.If, 0, "Shoot");
        locToShoot.duration = 0.05f;
        locToShoot.hasExitTime = false;

        // ── Transitions: Punch → Loco ───────────────────────────
        AnimatorStateTransition punchToLoc = punchState.AddTransition(locoState);
        punchToLoc.hasExitTime = true;
        punchToLoc.exitTime    = 0.85f;
        punchToLoc.duration    = 0.15f;

        // ── Transitions: Shoot → Loco ───────────────────────────
        AnimatorStateTransition shootToLoc = shootState.AddTransition(locoState);
        shootToLoc.hasExitTime = true;
        shootToLoc.exitTime    = 0.85f;
        shootToLoc.duration    = 0.15f;

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("PlayerController v2 built: Locomotion blend tree + Punch + Shoot states.");

        // ── Assign to player ────────────────────────────────────
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            Animator anim = player.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.runtimeAnimatorController = ctrl;
                EditorUtility.SetDirty(anim.gameObject);
                Debug.Log($"Assigned to: {anim.gameObject.name}");
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("Scene saved.");
    }
}
