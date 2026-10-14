using UnityEngine;
using UnityEditor;

public class TriggerPauseInPlayMode
{
    public static void Execute()
    {
        var pmm = Object.FindFirstObjectByType<PauseMenuManager>();
        if (pmm != null)
        {
            pmm.Pause();
            Debug.Log("[TriggerPause] Pause triggered.");
        }
        else
        {
            Debug.LogWarning("[TriggerPause] PauseMenuManager not found.");
        }
    }
}
