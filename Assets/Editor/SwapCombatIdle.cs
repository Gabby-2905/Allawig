using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class SwapCombatIdle
{
    public static void Execute()
    {
        string ctrlPath = "Assets/animations/PlayerController.controller";
        AnimatorController ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
        if (ctrl == null) { Debug.LogError("PlayerController.controller not found."); return; }

        AnimationClip idleTiramis = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/FightingMotionsVolume1/FBX/idle_Tiramis.fbx");
        if (idleTiramis == null) { Debug.LogError("idle_Tiramis.fbx not found."); return; }

        // Walk the state machine and find the CombatStance blend tree
        AnimatorStateMachine sm = ctrl.layers[0].stateMachine;
        int swapped = 0;

        foreach (var childState in sm.states)
        {
            if (childState.state.name == "CombatStance")
            {
                BlendTree tree = childState.state.motion as BlendTree;
                if (tree == null) { Debug.LogError("CombatStance motion is not a BlendTree."); return; }

                // Get children, replace the centre (0,0) idle clip
                var children = tree.children;
                for (int i = 0; i < children.Length; i++)
                {
                    // The centre child has position (0,0) — that's the combat idle
                    if (children[i].position == Vector2.zero)
                    {
                        children[i].motion = idleTiramis;
                        swapped++;
                        Debug.Log($"Swapped child [{i}] at (0,0) to idle_Tiramis.");
                    }
                }
                tree.children = children;
                EditorUtility.SetDirty(tree);
                break;
            }
        }

        if (swapped == 0)
            Debug.LogWarning("No centre (0,0) child found in CombatStance blend tree.");

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("CombatStance idle swapped to idle_Tiramis. Scene saved.");
    }
}
