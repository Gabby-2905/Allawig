using UnityEditor;
using UnityEngine;

public class FixMuddyGround3
{
    public static void Execute()
    {
        Material muddyMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/MuddyGround.mat");
        if (muddyMat == null)
        {
            Debug.LogError("Could not find MuddyGround.mat");
            return;
        }

        // Dark muddy brown tint — makes the sandy texture look wet and muddy
        // RGB roughly: dark brown with slight warmth
        muddyMat.SetColor("_BaseColor", new Color(0.30f, 0.22f, 0.13f, 1f));

        // Tile across multiple tiles so seams are less obvious
        // Each tile is 5 units, tiling of 3 = texture repeats every ~1.67 units, 
        // breaking up the per-tile pattern
        muddyMat.SetTextureScale("_BaseMap", new Vector2(3f, 3f));
        muddyMat.SetTextureScale("_BumpMap", new Vector2(3f, 3f));

        // Wet mud: moderate smoothness gives a damp sheen
        muddyMat.SetFloat("_Smoothness", 0.45f);
        muddyMat.SetFloat("_Metallic", 0.0f);
        muddyMat.SetFloat("_BumpScale", 0.8f);

        EditorUtility.SetDirty(muddyMat);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("MuddyGround material updated - dark muddy brown, 3x3 tiling, wet sheen.");
    }
}
