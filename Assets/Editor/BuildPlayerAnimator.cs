using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class BuildPlayerAnimator
{
    public static void Execute()
    {
        // Load animation clips
        AnimationClip idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Idles/HumanM@Idle01.fbx");
        AnimationClip walk = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Walk/HumanM@Walk01_Forward.fbx");
        AnimationClip run = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Run/HumanM@Run01_Forward.fbx");

        if (idle == null) { Debug.LogError("Idle clip not found"); return; }
        if (walk == null) { Debug.LogError("Walk clip not found"); return; }
        if (run == null)  { Debug.LogError("Run clip not found");  return; }

        Debug.Log($"Loaded: {idle.name}, {walk.name}, {run.name}");

        // Create new AnimatorController
        string savePath = "Assets/animations/PlayerController.controller";
        AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(savePath);

        // Add parameters
        ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
        ctrl.AddParameter("IsRunning", AnimatorControllerParameterType.Bool);

        // Get root state machine
        AnimatorStateMachine rootSM = ctrl.layers[0].stateMachine;

        // Create a blend tree state for locomotion
        AnimatorState locoState = rootSM.AddState("Locomotion", new Vector3(300, 100, 0));

        BlendTree blendTree;
        AnimatorState blendState = ctrl.CreateBlendTreeInController("Locomotion", out blendTree, 0);
        // Remove the auto-created state, we'll use our own
        rootSM.RemoveState(blendState);

        // Set up blend tree manually on locoState
        BlendTree tree = new BlendTree();
        tree.name = "Locomotion";
        tree.blendType = BlendTreeType.Simple1D;
        tree.blendParameter = "Speed";
        tree.useAutomaticThresholds = false;

        tree.AddChild(idle, 0f);
        tree.AddChild(walk, 1f);
        tree.AddChild(run,  2f);

        // Assign blend tree as motion
        locoState.motion = tree;

        // Save blend tree as sub-asset
        AssetDatabase.AddObjectToAsset(tree, savePath);

        // Set as default state
        rootSM.defaultState = locoState;

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("PlayerController animator built with Idle/Walk/Run blend tree.");

        // Assign to the Player's animator
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            Animator anim = player.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.runtimeAnimatorController = ctrl;
                EditorUtility.SetDirty(anim.gameObject);
                Debug.Log($"Assigned controller to: {anim.gameObject.name}");
            }
            else Debug.LogWarning("No Animator found on Player children.");
        }

        // Remove PlayerMovement from Arnold (it causes the Rigidbody errors)
        GameObject arnold = GameObject.Find("Arnold");
        if (arnold != null)
        {
            PlayerMovement pm = arnold.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                Object.DestroyImmediate(pm);
                EditorUtility.SetDirty(arnold);
                Debug.Log("Removed PlayerMovement from Arnold.");
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("Scene saved.");
    }
}
