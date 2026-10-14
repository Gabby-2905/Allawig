using UnityEditor;
using UnityEngine;

public class FixMuddyGround4
{
    public static void Execute()
    {
        Material muddyMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/MuddyGround.mat");
        if (muddyMat == null)
        {
            Debug.LogError("Could not find MuddyGround.mat");
            return;
        }

        // Subtle warm brown tint — just enough to push the sandy texture toward mud
        // without killing the texture detail
        muddyMat.SetColor("_BaseColor", new Color(0.72f, 0.58f, 0.40f, 1f));

        // 2x2 tiling per tile (each tile is 5 units, so texture repeats every 2.5 units)
        // This breaks up the per-tile seam pattern
        muddyMat.SetTextureScale("_BaseMap", new Vector2(2f, 2f));
        muddyMat.SetTextureScale("_BumpMap", new Vector2(2f, 2f));

        // Matte mud — very low smoothness, no metallic
        muddyMat.SetFloat("_Smoothness", 0.08f);
        muddyMat.SetFloat("_Metallic", 0.0f);
        muddyMat.SetFloat("_BumpScale", 1.0f);

        EditorUtility.SetDirty(muddyMat);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("MuddyGround material updated - warm brown tint, 2x2 tiling, matte.");
    }
}
