using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class RebuildPlayerAnimatorV4
{
    public static void Execute()
    {
        // ── Load clips ──────────────────────────────────────────────────────
        AnimationClip idle        = Load("Assets/Kevin Iglesias/Human Animations/Animations/Male/Idles/HumanM@Idle01.fbx");
        AnimationClip walk        = Load("Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Walk/HumanM@Walk01_Forward.fbx");
        AnimationClip walkBack    = Load("Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Walk/HumanM@Walk01_Backward.fbx");
        AnimationClip walkLeft    = Load("Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Walk/HumanM@Walk01_Left.fbx");
        AnimationClip walkRight   = Load("Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Walk/HumanM@Walk01_Right.fbx");
        AnimationClip run         = Load("Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Run/HumanM@Run01_Forward.fbx");
        AnimationClip combatIdle  = Load("Assets/Kevin Iglesias/Human Animations/Animations/Male/Combat/HumanM@CombatIdle01.fbx");
        AnimationClip punchL      = Load("Assets/Kevin Iglesias/Human Animations/Animations/Male/Combat/1H/HumanM@Attack1H01_L.fbx");
        AnimationClip punchR      = Load("Assets/Kevin Iglesias/Human Animations/Animations/Male/Combat/1H/HumanM@Attack1H01_R.fbx");

        if (idle==null||walk==null||walkBack==null||walkLeft==null||walkRight==null||
            run==null||combatIdle==null||punchL==null||punchR==null)
        { Debug.LogError("One or more clips not found."); return; }

        string savePath = "Assets/animations/PlayerController.controller";
        AssetDatabase.DeleteAsset(savePath);
        AssetDatabase.Refresh();

        // ── Create controller ───────────────────────────────────────────────
        AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(savePath);

        // Parameters
        ctrl.AddParameter("Speed",      AnimatorControllerParameterType.Float);
        ctrl.AddParameter("IsAiming",   AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("VelocityX",  AnimatorControllerParameterType.Float); // local right
        ctrl.AddParameter("VelocityZ",  AnimatorControllerParameterType.Float); // local forward
        ctrl.AddParameter("PunchLeft",  AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("PunchRight", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Shoot",      AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine sm = ctrl.layers[0].stateMachine;

        // ── 1. Locomotion blend tree (idle / walk / run) ────────────────────
        AnimatorState locoState = sm.AddState("Locomotion", new Vector3(300, 100, 0));
        BlendTree locoTree = new BlendTree();
        locoTree.name           = "Locomotion";
        locoTree.blendType      = BlendTreeType.Simple1D;
        locoTree.blendParameter = "Speed";
        locoTree.useAutomaticThresholds = false;
        locoTree.AddChild(idle, 0f);
        locoTree.AddChild(walk, 1f);
        locoTree.AddChild(run,  2f);
        locoState.motion = locoTree;
        AssetDatabase.AddObjectToAsset(locoTree, savePath);

        // ── 2. Combat Stance: 2D Freeform Directional blend tree ────────────
        // Uses VelocityX (strafe) and VelocityZ (forward/back) in local space
        // Values: -1 = backward/left, 0 = idle, +1 = forward/right
        AnimatorState combatState = sm.AddState("CombatStance", new Vector3(600, 100, 0));
        BlendTree combatTree = new BlendTree();
        combatTree.name            = "CombatStance";
        combatTree.blendType       = BlendTreeType.FreeformDirectional2D;
        combatTree.blendParameter  = "VelocityX";
        combatTree.blendParameterY = "VelocityZ";
        combatTree.useAutomaticThresholds = false;

        // Centre: combat idle (0, 0)
        combatTree.AddChild(combatIdle, new Vector2(0f,  0f));
        // Forward (0, 1)
        combatTree.AddChild(walk,       new Vector2(0f,  1f));
        // Backward (0, -1)
        combatTree.AddChild(walkBack,   new Vector2(0f, -1f));
        // Strafe left (-1, 0)
        combatTree.AddChild(walkLeft,   new Vector2(-1f, 0f));
        // Strafe right (1, 0)
        combatTree.AddChild(walkRight,  new Vector2(1f,  0f));

        combatState.motion = combatTree;
        AssetDatabase.AddObjectToAsset(combatTree, savePath);

        // ── 3. Punch Left ───────────────────────────────────────────────────
        AnimatorState punchLState = sm.AddState("PunchLeft",  new Vector3(600, -80, 0));
        punchLState.motion = punchL;
        punchLState.speed  = 1.4f;

        // ── 4. Punch Right ──────────────────────────────────────────────────
        AnimatorState punchRState = sm.AddState("PunchRight", new Vector3(600, 280, 0));
        punchRState.motion = punchR;
        punchRState.speed  = 1.4f;

        // ── 5. Shoot ────────────────────────────────────────────────────────
        AnimatorState shootState = sm.AddState("Shoot", new Vector3(900, 100, 0));
        shootState.motion = punchR;
        shootState.speed  = 2.5f;

        // ── Default ─────────────────────────────────────────────────────────
        sm.defaultState = locoState;

        // ── Loco ↔ CombatStance ─────────────────────────────────────────────
        AddTransition(locoState,    combatState, "IsAiming",   true,  false, 0.15f);
        AddTransition(combatState,  locoState,   "IsAiming",   false, false, 0.20f);

        // ── CombatStance / Loco → Punches ───────────────────────────────────
        AddTriggerTransition(combatState, punchLState, "PunchLeft",  0.05f);
        AddTriggerTransition(combatState, punchRState, "PunchRight", 0.05f);
        AddTriggerTransition(locoState,   punchLState, "PunchLeft",  0.05f);
        AddTriggerTransition(locoState,   punchRState, "PunchRight", 0.05f);

        // ── Punches → CombatStance (if aiming) / Loco ───────────────────────
        AddExitTransition(punchLState, combatState, "IsAiming", true,  0.8f, 0.10f);
        AddExitTransition(punchLState, locoState,   "IsAiming", false, 0.8f, 0.15f);
        AddExitTransition(punchRState, combatState, "IsAiming", true,  0.8f, 0.10f);
        AddExitTransition(punchRState, locoState,   "IsAiming", false, 0.8f, 0.15f);

        // ── Shoot ────────────────────────────────────────────────────────────
        AddTriggerTransition(combatState, shootState, "Shoot", 0.05f);
        AddTriggerTransition(locoState,   shootState, "Shoot", 0.05f);
        AddExitTransition(shootState, combatState, "IsAiming", true,  0.8f, 0.10f);
        AddExitTransition(shootState, locoState,   "IsAiming", false, 0.8f, 0.15f);

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("PlayerController v4 built: CombatStance 2D blend tree with directional walk.");

        // ── Assign to player ─────────────────────────────────────────────────
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

    // ── Helpers ──────────────────────────────────────────────────────────────

    static AnimationClip Load(string path)
    {
        var c = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (c == null) Debug.LogError($"Not found: {path}");
        return c;
    }

    static void AddTransition(AnimatorState from, AnimatorState to,
        string boolParam, bool value, bool hasExit, float duration)
    {
        var t = from.AddTransition(to);
        t.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, boolParam);
        t.hasExitTime = hasExit;
        t.duration    = duration;
    }

    static void AddTriggerTransition(AnimatorState from, AnimatorState to,
        string trigger, float duration)
    {
        var t = from.AddTransition(to);
        t.AddCondition(AnimatorConditionMode.If, 0, trigger);
        t.hasExitTime = false;
        t.duration    = duration;
    }

    static void AddExitTransition(AnimatorState from, AnimatorState to,
        string boolParam, bool value, float exitTime, float duration)
    {
        var t = from.AddTransition(to);
        t.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, boolParam);
        t.hasExitTime = true;
        t.exitTime    = exitTime;
        t.duration    = duration;
    }
}
