using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class RebuildPlayerAnimatorV3
{
    public static void Execute()
    {
        // ── Load clips ──────────────────────────────────────────────────────
        AnimationClip idle        = Load("Assets/Kevin Iglesias/Human Animations/Animations/Male/Idles/HumanM@Idle01.fbx");
        AnimationClip walk        = Load("Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Walk/HumanM@Walk01_Forward.fbx");
        AnimationClip run         = Load("Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Run/HumanM@Run01_Forward.fbx");
        AnimationClip combatIdle  = Load("Assets/Kevin Iglesias/Human Animations/Animations/Male/Combat/HumanM@CombatIdle01.fbx");
        AnimationClip punchL      = Load("Assets/Kevin Iglesias/Human Animations/Animations/Male/Combat/1H/HumanM@Attack1H01_L.fbx");
        AnimationClip punchR      = Load("Assets/Kevin Iglesias/Human Animations/Animations/Male/Combat/1H/HumanM@Attack1H01_R.fbx");

        if (idle == null || walk == null || run == null || combatIdle == null || punchL == null || punchR == null)
        {
            Debug.LogError("One or more clips not found. Aborting.");
            return;
        }

        string savePath = "Assets/animations/PlayerController.controller";
        AssetDatabase.DeleteAsset(savePath);
        AssetDatabase.Refresh();

        // ── Create controller ───────────────────────────────────────────────
        AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(savePath);

        // Parameters
        ctrl.AddParameter("Speed",      AnimatorControllerParameterType.Float);
        ctrl.AddParameter("IsAiming",   AnimatorControllerParameterType.Bool);
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

        // ── 2. Combat Idle (right-click stance) ─────────────────────────────
        AnimatorState combatIdleState = sm.AddState("CombatIdle", new Vector3(600, 100, 0));
        combatIdleState.motion = combatIdle;

        // ── 3. Punch Left ───────────────────────────────────────────────────
        AnimatorState punchLState = sm.AddState("PunchLeft", new Vector3(600, -60, 0));
        punchLState.motion = punchL;
        punchLState.speed  = 1.4f;

        // ── 4. Punch Right ──────────────────────────────────────────────────
        AnimatorState punchRState = sm.AddState("PunchRight", new Vector3(600, 260, 0));
        punchRState.motion = punchR;
        punchRState.speed  = 1.4f;

        // ── 5. Shoot (reuse punchR, faster) ────────────────────────────────
        AnimatorState shootState = sm.AddState("Shoot", new Vector3(900, 100, 0));
        shootState.motion = punchR;
        shootState.speed  = 2.5f;

        // ── Default ─────────────────────────────────────────────────────────
        sm.defaultState = locoState;

        // ── Transitions: Loco ↔ CombatIdle ─────────────────────────────────
        var locoToCombat = locoState.AddTransition(combatIdleState);
        locoToCombat.AddCondition(AnimatorConditionMode.If, 0, "IsAiming");
        locoToCombat.duration    = 0.15f;
        locoToCombat.hasExitTime = false;

        var combatToLoco = combatIdleState.AddTransition(locoState);
        combatToLoco.AddCondition(AnimatorConditionMode.IfNot, 0, "IsAiming");
        combatToLoco.duration    = 0.2f;
        combatToLoco.hasExitTime = false;

        // ── Transitions: CombatIdle → PunchLeft ─────────────────────────────
        var combatToPunchL = combatIdleState.AddTransition(punchLState);
        combatToPunchL.AddCondition(AnimatorConditionMode.If, 0, "PunchLeft");
        combatToPunchL.duration    = 0.05f;
        combatToPunchL.hasExitTime = false;

        // ── Transitions: CombatIdle → PunchRight ────────────────────────────
        var combatToPunchR = combatIdleState.AddTransition(punchRState);
        combatToPunchR.AddCondition(AnimatorConditionMode.If, 0, "PunchRight");
        combatToPunchR.duration    = 0.05f;
        combatToPunchR.hasExitTime = false;

        // ── Transitions: Loco → PunchLeft (allow punching without aiming) ───
        var locoToPunchL = locoState.AddTransition(punchLState);
        locoToPunchL.AddCondition(AnimatorConditionMode.If, 0, "PunchLeft");
        locoToPunchL.duration    = 0.05f;
        locoToPunchL.hasExitTime = false;

        var locoToPunchR = locoState.AddTransition(punchRState);
        locoToPunchR.AddCondition(AnimatorConditionMode.If, 0, "PunchRight");
        locoToPunchR.duration    = 0.05f;
        locoToPunchR.hasExitTime = false;

        // ── Transitions: PunchLeft → CombatIdle (if aiming) / Loco ─────────
        var punchLToCombat = punchLState.AddTransition(combatIdleState);
        punchLToCombat.AddCondition(AnimatorConditionMode.If, 0, "IsAiming");
        punchLToCombat.hasExitTime = true;
        punchLToCombat.exitTime    = 0.8f;
        punchLToCombat.duration    = 0.1f;

        var punchLToLoco = punchLState.AddTransition(locoState);
        punchLToLoco.AddCondition(AnimatorConditionMode.IfNot, 0, "IsAiming");
        punchLToLoco.hasExitTime = true;
        punchLToLoco.exitTime    = 0.8f;
        punchLToLoco.duration    = 0.15f;

        // ── Transitions: PunchRight → CombatIdle / Loco ─────────────────────
        var punchRToCombat = punchRState.AddTransition(combatIdleState);
        punchRToCombat.AddCondition(AnimatorConditionMode.If, 0, "IsAiming");
        punchRToCombat.hasExitTime = true;
        punchRToCombat.exitTime    = 0.8f;
        punchRToCombat.duration    = 0.1f;

        var punchRToLoco = punchRState.AddTransition(locoState);
        punchRToLoco.AddCondition(AnimatorConditionMode.IfNot, 0, "IsAiming");
        punchRToLoco.hasExitTime = true;
        punchRToLoco.exitTime    = 0.8f;
        punchRToLoco.duration    = 0.15f;

        // ── Transitions: CombatIdle → Shoot ─────────────────────────────────
        var combatToShoot = combatIdleState.AddTransition(shootState);
        combatToShoot.AddCondition(AnimatorConditionMode.If, 0, "Shoot");
        combatToShoot.duration    = 0.05f;
        combatToShoot.hasExitTime = false;

        var locoToShoot = locoState.AddTransition(shootState);
        locoToShoot.AddCondition(AnimatorConditionMode.If, 0, "Shoot");
        locoToShoot.duration    = 0.05f;
        locoToShoot.hasExitTime = false;

        // ── Transitions: Shoot → CombatIdle / Loco ──────────────────────────
        var shootToCombat = shootState.AddTransition(combatIdleState);
        shootToCombat.AddCondition(AnimatorConditionMode.If, 0, "IsAiming");
        shootToCombat.hasExitTime = true;
        shootToCombat.exitTime    = 0.8f;
        shootToCombat.duration    = 0.1f;

        var shootToLoco = shootState.AddTransition(locoState);
        shootToLoco.AddCondition(AnimatorConditionMode.IfNot, 0, "IsAiming");
        shootToLoco.hasExitTime = true;
        shootToLoco.exitTime    = 0.8f;
        shootToLoco.duration    = 0.15f;

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("PlayerController v3 built with proper combat animations.");

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

    static AnimationClip Load(string path)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null) Debug.LogError($"Clip not found: {path}");
        return clip;
    }
}
