using UnityEditor;
using UnityEngine;

public class FixMuddyGround
{
    public static void Execute()
    {
        Material muddyMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/MuddyGround.mat");
        if (muddyMat == null)
        {
            Debug.LogError("Could not find MuddyGround.mat");
            return;
        }

        // Brighter, more natural mud/wet dirt color
        muddyMat.SetColor("_BaseColor", new Color(0.38f, 0.27f, 0.16f, 1f));

        // Larger tiling so the texture doesn't look too repetitive across 5-unit tiles
        muddyMat.SetTextureScale("_BaseMap", new Vector2(0.5f, 0.5f));
        muddyMat.SetTextureScale("_BumpMap", new Vector2(0.5f, 0.5f));

        // Slightly wet mud: low-medium smoothness
        muddyMat.SetFloat("_Smoothness", 0.35f);
        muddyMat.SetFloat("_Metallic", 0.0f);
        muddyMat.SetFloat("_BumpScale", 1.0f);

        EditorUtility.SetDirty(muddyMat);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("MuddyGround material updated.");
    }
}
