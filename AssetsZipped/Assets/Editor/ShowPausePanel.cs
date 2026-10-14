using UnityEngine;
using UnityEditor;

public class ShowPausePanel
{
    public static void Execute()
    {
        GameObject panel = GameObject.Find("PauseMenuCanvas/PausePanel");
        if (panel != null)
        {
            panel.SetActive(true);
            Debug.Log("PausePanel shown");
        }
        else
        {
            Debug.LogWarning("PausePanel not found");
        }
    }
}
