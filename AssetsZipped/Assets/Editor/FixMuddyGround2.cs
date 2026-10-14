using UnityEditor;
using UnityEngine;

public class FixMuddyGround2
{
    public static void Execute()
    {
        Material muddyMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/MuddyGround.mat");
        if (muddyMat == null)
        {
            Debug.LogError("Could not find MuddyGround.mat");
            return;
        }

        // Use white base color so the texture shows its true colors
        // The GR_Dirt1_AS texture already has the mud color baked in
        muddyMat.SetColor("_BaseColor", new Color(1f, 1f, 1f, 1f));

        // Each ground tile is 5x5 units. Tiling of 1 = one full texture per tile
        // Use 1x1 so the full texture maps to each tile naturally
        muddyMat.SetTextureScale("_BaseMap", new Vector2(1f, 1f));
        muddyMat.SetTextureScale("_BumpMap", new Vector2(1f, 1f));

        // Mud: low smoothness (not shiny), no metallic
        muddyMat.SetFloat("_Smoothness", 0.15f);
        muddyMat.SetFloat("_Metallic", 0.0f);
        muddyMat.SetFloat("_BumpScale", 1.0f);

        EditorUtility.SetDirty(muddyMat);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("MuddyGround material fixed - white tint, 1x1 tiling.");
    }
}
